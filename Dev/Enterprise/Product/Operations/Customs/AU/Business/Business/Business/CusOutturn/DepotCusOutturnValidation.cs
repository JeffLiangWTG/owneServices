using CargoWise.EntityFramework;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DepotCusOutturnValidation : BaseCusOutturnValidation
	{
		public DepotCusOutturnValidation(DepotCusOutturn outturn)
			: base(outturn)
		{
			this.outturn = outturn;
		}

		readonly DepotCusOutturn outturn;

		#region C5_ContainerNumber

		protected override void CheckC5_ContainerNumber()
		{
			base.CheckC5_ContainerNumber();
			if (!outturn.IsBulk && !outturn.IsBreakBulk)
			{
				MessageValidation.CheckEntered(outturn.C5_ContainerNumberInfo, "Container Number is required if not Bulk or Break Bulk.");
			}
		}

		#endregion

		#region C5_ContainerSeal
		protected override void CheckC5_ContainerSeal()
		{
			base.CheckC5_ContainerSeal();
			if (outturn.C5_ContainerSeal.Length > 10)
			{
				outturn.C5_ContainerSealInfo.AddMessageError("Seal number should not be mre than 10 characters.");
			}
		}

		#endregion

		#region C5_OutturnResultType

		protected override void CheckC5_OutturnResultType()
		{
			base.CheckC5_OutturnResultType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(outturn.C5_OutturnResultTypeInfo, outturn.Lookups.OutturnResultTypeList);

			ValidateC5_GoodsDescription();
			ValidateC5_MarksAndNumbers();
			ValidateC5_PackagesOutturned();
		}

		#endregion

		#region C5_HouseBill

		protected override void CheckC5_HouseBill()
		{
			base.CheckC5_HouseBill();
			if (outturn.IsLCL || outturn.IsBulk || outturn.IsBreakBulk)
			{
				MessageValidation.CheckEntered(outturn.C5_HouseBillInfo, "House Bill is required if Bulk, Break Bulk or LCL.");
			}
			else if (!outturn.C5_HouseBill.IsEmpty && (outturn.IsFCL || outturn.IsFCX))
			{
				outturn.C5_HouseBillInfo.AddMessageError("House Bill is not required if FCL or FCX.");
			}
		}

		#endregion

		#region C5_MasterBill

		protected override void CheckC5_MasterBill()
		{
			base.CheckC5_MasterBill();
			if (!outturn.IsFCL && !outturn.IsFCX)
			{
				MessageValidation.CheckEntered(outturn.C5_MasterBillInfo, "Ocean Bill is required if not FCL or FCX.");
			}
			else if (!outturn.C5_MasterBill.IsEmpty)
			{
				outturn.C5_MasterBillInfo.AddMessageError("Ocean Bill is not required if FCL or FCX.");
			}
		}

		#endregion

		#region C5_PackagesOutterned

		protected override void CheckC5_PackagesOutturned()
		{
			base.CheckC5_PackagesOutturned();

			if (outturn.C5_PackagesOutturned < 0)
			{
				outturn.C5_PackagesOutturnedInfo.AddMessageError("Packages Outturned must be positive.");
			}
			else if (outturn.C5_OutturnResultType != CMROutturnResultType.Codes.ShortLanded && !outturn.C5_CargoUnpackDate.IsEmpty)
			{
				MessageValidation.CheckEntered(outturn.C5_PackagesOutturnedInfo);
			}
		}

		#endregion

		#region C5_CargoType

		protected override void CheckC5_CargoType()
		{
			base.CheckC5_CargoType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(outturn.C5_CargoTypeInfo, outturn.Lookups.CargoTypes);

			ValidateC5_ContainerNumber();
			ValidateC5_HouseBill();
			ValidateC5_MarksAndNumbers();
			ValidateC5_MasterBill();
			ValidateC5_OuterPacks();
			ValidateC5_OuterPackUnits();
			ValidateC5_PackagesOutturned();
			ValidateC5_PackagesUnits();
		}

		#endregion

		#region C5_PackagesUnits

		protected override void CheckC5_PackagesUnits()
		{
			base.CheckC5_PackagesUnits();
			if (!outturn.C5_CargoUnpackDate.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(outturn.C5_PackagesUnitsInfo, outturn.Lookups.PackageTypes);
			}
		}

		#endregion

		#region C5_GoodsDescription

		protected override void CheckC5_GoodsDescription()
		{
			base.CheckC5_GoodsDescription();
			if (outturn.IsSurplus)
			{
				MessageValidation.CheckEntered(outturn.C5_GoodsDescriptionInfo, "Goods Description is required if Surplus.");
			}
		}

		#endregion

		#region C5_MarksAndNumbers

		protected override void CheckC5_MarksAndNumbers()
		{
			base.CheckC5_MarksAndNumbers();
			if ((outturn.IsSurplus && outturn.IsLCL) || outturn.IsBreakBulk)
			{
				MessageValidation.CheckEntered(outturn.C5_MarksAndNumbersInfo, "Marks and Numbers is required if Surplus and either LCL or Break Bulk.");
			}
		}

		#endregion

		#region C5_CargoUnpackDate

		protected override void CheckC5_CargoUnpackDate()
		{
			base.CheckC5_CargoUnpackDate();
			if (!outturn.C5_CargoUnpackDate.IsEmpty)
			{
				if (outturn.C5_ReceiptOnlyIndicator)
				{
					outturn.C5_CargoUnpackDateInfo.AddMessageError("This is a Receipt Only Line, it cannot be unpacked.");
				}
				if (!outturn.C5_CargoReceiptDate.IsEmpty && outturn.C5_CargoUnpackDate < outturn.C5_CargoReceiptDate)
				{
					outturn.C5_CargoUnpackDateInfo.AddMessageError("Unpack date must be after receipt date.");
				}
				if (outturn.FreightForwarderIndicator == "Y")
				{
					outturn.C5_CargoUnpackDateInfo.AddMessageError("Bills with Freight Forwarder Indicator set are not normally outturned.");
				}
			}
		}

		#endregion

		#region C5_CargoReceiptDate

		protected override void CheckC5_CargoReceiptDate()
		{
			base.CheckC5_CargoReceiptDate();
			ValidateC5_CargoUnpackDate();
		}

		#endregion

		#region C5_MessageStatus

		protected override void CheckC5_MessageStatus()
		{
			base.CheckC5_MessageStatus();

			if (outturn.MessageStatus.Code == CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived)
			{
				outturn.C5_MessageStatusInfo.AddWarning("Outturn has been rescinded");
				outturn.Validation.ValidateC5_CustomsStatus();
			}
		}

		#endregion

		#region C5_CustomsStatus

		protected override void CheckC5_CustomsStatus()
		{
			base.CheckC5_CustomsStatus();

			if (outturn.MessageStatus.Code == CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived)
			{
				outturn.C5_CustomsStatusInfo.AddWarning("Outturn has been rescinded");
			}
		}

		#endregion
	}
}
