using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class CusOutTurnValidation : Customs.Business.CusOutturnValidation
	{
		public CusOutTurnValidation(CusOutTurn parent) : base(parent)
		{ }

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateWarehouseLocationID();
		}

		public void ValidateWarehouseLocationID()
		{
			ValidateCalculatedProperty(Parent.WarehouseLocationIDInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "ZPropertyInfo validation method call via reflection. Origin call is ValidateWarehouseLocationID")]
		void CheckWarehouseLocationID()
		{
			ListValidation.ErrorIfInvalidPK(Parent.WarehouseLocationIDInfo);
			CheckLocationWeightCapacity(Parent.WarehouseLocationIDInfo);
		}

		void CheckLocationWeightCapacity(ZPropertyInfo info)
		{
			if (Parent.ShedStorageLocation != null && Parent.ShedStorageLocation.WLV_MaxWeight != 0 && Parent.C5_PackagesOutturned > 0 && Parent.AwbOrSplit.NumberOfPiecesExpected > 0)
			{
				var maxWeightInKilos = Enterprise.Core.Constants.Weight.Convert(Parent.ShedStorageLocation.WLV_MaxWeight, Parent.ShedStorageLocation.WLV_MaxWeightUnit, Enterprise.Core.Constants.Weight.Kilograms);
				var proRatedWeight = Parent.AwbOrSplit.Weight * Parent.C5_PackagesOutturned / Parent.AwbOrSplit.NumberOfPiecesExpected;
				if (maxWeightInKilos < proRatedWeight)
				{
					info.AddWarning(string.Format("This location's maximum weight ({0}kg) is less than the pro-rated weight of this line ({1}kg x {2}/{3}pcs = {4}kg). It might not fit.",
						maxWeightInKilos, Parent.AwbOrSplit.Weight, Parent.C5_PackagesOutturned, Parent.AwbOrSplit.NumberOfPiecesExpected, proRatedWeight));
				}
			}
		}

		// IsDelivered
		protected override void CheckC5_ReceiptOnlyIndicator()
		{
			base.CheckC5_ReceiptOnlyIndicator();
			if (Parent.C5_ReceiptOnlyIndicator)
			{
				if (!Parent.IsInDatabase)
				{
					Parent.IsDeliveredInfo.AddError("Please save first before ticking this box");
				}
				else
				{
					Parent.IsDeliveredInfo.AddWarning("After saving, this row will become read only and cannot be edited. Proceed with caution.");
				}

				if (!Parent.IsReleasedAlready)
				{
					if (Parent.AwbOrSplit.CustomsActionCode == CustomsStatusCodes.Codes.SeizedDestroyedOrRetainedByCustoms)
					{
						if (Environment.Env.CurrentUser.IsController)
						{
							Parent.IsDeliveredInfo.AddWarning("As a Controller, you are allowed to deliver this seized consignment to Customs without a release document.  Do not forget to scan the seizure notice into eDocs.");
						}
						else
						{
							Parent.IsDeliveredInfo.AddError("This record is customs seized (CS status) and you cannot deliver it without a release document because you are not a Controller. Have your administrator log in and can deliver it instead.");
						}
					}
					else
					{
						Parent.IsDeliveredInfo.AddError("This record is not marked as released, it cannot be delivered.");
					}
				}
				EnsureTotalDeliveredIsNotMoreThanTotalReceivedOrReleased(Parent.IsDeliveredInfo);
			}
		}

		public new CusOutTurn Parent
		{
			get { return (CusOutTurn)base.Parent; }
		}

		// SplitReferenceToWhichThisPertains
		protected override void CheckC5_MessageStatus()
		{
			base.CheckC5_MessageStatus();
			if (Parent.Awb != null)
			{
				if (Parent.Awb.HasSplits)
				{
					if (Parent.SplitReferenceToWhichThisPertains.IsEmpty)
					{
						Parent.C5_MessageStatusInfo.AddError("The AWB is split, select a split reference");
					}
					if (Parent.Awb.Splits[Parent.SplitReferenceToWhichThisPertains] == null)
					{
						Parent.C5_MessageStatusInfo.AddError("Select a valid split reference from the dropdown");
					}
				}
			}
		}

		protected override void CheckC5_PackagesOutturned()
		{
			base.CheckC5_PackagesOutturned();
			EnsureTotalDeliveredIsNotMoreThanTotalReceivedOrReleased(Parent.C5_PackagesOutturnedInfo);
			ValidateWarehouseLocationID();
		}

		//IsReleasedAlready
		protected override void CheckC5_PillageIndicator()
		{
			base.CheckC5_PillageIndicator();
			EnsureTotalDeliveredIsNotMoreThanTotalReceivedOrReleased(Parent.IsReleasedAlreadyInfo);
		}

		void EnsureTotalDeliveredIsNotMoreThanTotalReceivedOrReleased(ZPropertyInfo zPropertyInfo)
		{
			var awb = Parent.Awb;

			if (awb != null)
			{
				if (awb.HasSplits && !Parent.SplitReferenceToWhichThisPertains.IsEmpty)
				{
					awb = awb.Splits[Parent.SplitReferenceToWhichThisPertains];
				}

				if (awb != null)
				{
					if (awb.OutTurns.TotalDelivered > awb.NumberOfPiecesReceived)
					{
						zPropertyInfo.AddError(string.Format("Total NPR of {0} is less than the number of pieces delivered in this grid ({1}). You are claiming to have delivered more than you have received.", awb.HumanReadableName, awb.OutTurns.TotalDelivered));
					}

					var sumOfReceivedPiecesAccordingToOutTurnsCollection = (from CusOutTurn cot in awb.OutTurns select (int)cot.C5_PackagesOutturned).Sum();
					var totalShedReleasedSoFar = awb.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.ShedEvent);
					var sumOfReleasedPiecesAccordingToOutTurnsCollection = (from CusOutTurn cot in awb.OutTurns where cot.IsReleasedAlready select (int)cot.C5_PackagesOutturned).Sum();
					if (sumOfReleasedPiecesAccordingToOutTurnsCollection > totalShedReleasedSoFar)
					{
						zPropertyInfo.AddMessageError(string.Format("Total release count of {0}, {1}, is less than the number of pieces released in this grid ({2})", awb.HumanReadableName, totalShedReleasedSoFar, sumOfReleasedPiecesAccordingToOutTurnsCollection));
					}
					else if (sumOfReleasedPiecesAccordingToOutTurnsCollection != totalShedReleasedSoFar)
					{
						zPropertyInfo.AddWarning(string.Format("Total release count of {0}, {1}, is not equal to the number of pieces released in this grid ({2}). Ensure that 'Release/Cleared' and the package count are set correctly", awb.HumanReadableName, totalShedReleasedSoFar, sumOfReleasedPiecesAccordingToOutTurnsCollection));
					}
				}
			}

			ValidateC5_ReceiptOnlyIndicator();
			ValidateC5_PillageIndicator();
			ValidateC5_PackagesOutturned();
		}

		protected override void CheckC5_MarksAndNumbers()
		{
			base.CheckC5_MarksAndNumbers();
			if (Parent.C5_MarksAndNumbers.IsEmpty)
			{
				if (!Parent.SplitReferenceToWhichThisPertains.IsEmpty || (Parent.Awb != null && Parent.Awb.HasSplits))
				{
					Parent.C5_MarksAndNumbersInfo.AddError("Splits exist, marks & numbers are required");
				}
			}
		}
	}
}
