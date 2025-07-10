using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAPivotValidation : Customs.Business.CusSCAPivotValidation
	{
		public CusSCAPivotValidation(Customs.Business.BaseCusSCAPivot parent)
			: base(parent)
		{
		}

		public CusSCAPivot SCAPivot
		{
			get
			{
				return (CusSCAPivot)Parent;
			}
		}

		public void RunContainerValidation()
		{
			ValidateCN_SealNumber();
			ValidateCN_ContainerMode();
			ValidateCN_ContainerNumber();
			ValidateCN_ContainerType();
			ValidateCN_ShipperOwnedContainer();
			ValidateCV_AssociatedContainer();
		}

		public void ValidateCN_ContainerMode()
		{
			ValidateCalculatedProperty(SCAPivot.CN_ContainerModeInfo);
		}

		public void ValidateCN_ContainerNumber()
		{
			ValidateCalculatedProperty(SCAPivot.CN_ContainerNumberInfo);
		}

		public void ValidateCN_ContainerType()
		{
			ValidateCalculatedProperty(SCAPivot.CN_ContainerTypeInfo);
		}

		public void ValidateCN_SealNumber()
		{
			ValidateCalculatedProperty(SCAPivot.CN_SealNumberInfo);
		}

		public void ValidateCN_ShipperOwnedContainer()
		{
			ValidateCalculatedProperty(SCAPivot.CN_ShipperOwnedContainerInfo);
		}

		public void ValidateCV_AssociatedContainer()
		{
			ValidateCalculatedProperty(SCAPivot.CV_AssociatedContainerInfo);
		}

		#region Implementation

		#region House Bill and Container Number for Error Messages

		protected ZString HouseBillNumberForErrormessage
		{
			get
			{
				if (SCAPivot.HouseBill != null && !SCAPivot.HouseBill.CA_HouseBill.IsEmpty)
				{
					return SCAPivot.HouseBill.CA_HouseBill;
				}
				return "House Bill not set";
			}
		}

		protected ZString ContainerNumberForErrorMessage
		{
			get
			{
				if (SCAPivot.Container != null && !SCAPivot.Container.CN_ContainerNumber.IsEmpty)
				{
					return SCAPivot.Container.CN_ContainerNumber;
				}
				return "Container Number not set";
			}
		}

		#endregion

		protected MessageValidation MessageValidation
		{
			get { return new MessageValidation(SCAPivot); }
		}

		protected bool IsPackageTypeOrCountRequired
		{
			get { return !CusSCAPivot.ContainerIsBulk(SCAPivot.CV_AssociatedContainer); }
		}

		protected bool IsMarksAndNumbersRequired
		{
			get
			{
				return CusSCAPivot.ContainerIsBreakBulk(SCAPivot.CV_AssociatedContainer)
						|| (SCAPivot.Container != null && SCAPivot.Container.CN_ContainerMode == Core.Constants.ContainerModes.LCL);
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			RunContainerValidation();
		}

		#region Checks

		internal const string DuplicateFCLContainer = "This FCL container already has a reference on house bill: ";

		protected virtual void CheckCN_ContainerMode()
		{
			if (SCAPivot.Container != null && !CusSCAPivot.ContainerIsBulkOrBreakBulk(SCAPivot.CV_AssociatedContainer))
			{
				SCAPivot.Container.Validation.ValidateCN_ContainerMode();
				SCAPivot.CN_ContainerModeInfo.AddAllNotificationsFrom(SCAPivot.Container.CN_ContainerModeInfo);
			}
		}

		protected virtual void CheckCN_ContainerNumber()
		{
			if (SCAPivot.Container != null && !CusSCAPivot.ContainerIsBulkOrBreakBulk(SCAPivot.CV_AssociatedContainer))
			{
				SCAPivot.Container.Validation.ValidateCN_ContainerNumber();
				SCAPivot.CN_ContainerNumberInfo.AddAllNotificationsFrom(SCAPivot.Container.CN_ContainerNumberInfo);
			}
		}

		protected virtual void CheckCN_ContainerType()
		{
			if (SCAPivot.Container != null && !CusSCAPivot.ContainerIsBulkOrBreakBulk(SCAPivot.CV_AssociatedContainer))
			{
				SCAPivot.Container.Validation.ValidateCN_RC_NKContainerType();
				SCAPivot.CN_ContainerTypeInfo.AddAllNotificationsFrom(SCAPivot.Container.CN_RC_NKContainerTypeInfo);
			}
		}

		protected virtual void CheckCN_SealNumber()
		{
			if (SCAPivot.Container != null && !CusSCAPivot.ContainerIsBulkOrBreakBulk(SCAPivot.CV_AssociatedContainer))
			{
				SCAPivot.Container.Validation.ValidateCN_SealNumber();
				SCAPivot.CN_SealNumberInfo.AddAllNotificationsFrom(SCAPivot.Container.CN_SealNumberInfo);
			}
		}

		protected virtual void CheckCN_ShipperOwnedContainer()
		{
			if (SCAPivot.Container != null && !CusSCAPivot.ContainerIsBulkOrBreakBulk(SCAPivot.CV_AssociatedContainer))
			{
				SCAPivot.Container.Validation.ValidateCN_ShipperOwnedContainer();
				SCAPivot.CN_ShipperOwnedContainerInfo.AddAllNotificationsFrom(SCAPivot.Container.CN_ShipperOwnedContainerInfo);
			}
		}

		protected void CheckCV_AssociatedContainer()
		{
			var pivot = SCAPivot;
			var container = pivot.Container;
			if (container != null && !CusSCAPivot.ContainerIsBulkOrBreakBulk(container.CN_ContainerMode))
			{
				container.Validation.ValidateCN_ContainerNumber();
				pivot.CV_AssociatedContainerInfo.AddAllNotificationsFrom(container.CN_ContainerNumberInfo);
				if ((container.CN_ContainerMode == Core.Constants.ContainerModes.FCL || container.CN_ContainerMode == Core.Constants.ContainerModes.FCLMixedShipper) && pivot.HouseBill != null)
				{
					var associatedWithMultiHouseBills = container.IsAssociatedWithMultipleHouseBills;
					var associatedWithMultiConsignees = container.IsAssociatedWithMultipleConsignees;

					if (associatedWithMultiHouseBills)
					{
						if (associatedWithMultiConsignees)
						{
							SCAPivot.CV_AssociatedContainerInfo.AddMessageError(InvalidContainerLCL);
						}
						else if (SCAPivot.Container.CN_ContainerMode == Core.Constants.ContainerModes.FCL)
						{
							SCAPivot.CV_AssociatedContainerInfo.AddMessageError(InvalidContainerFCXOrLCL);
						}
					}
				}
			}

			AddErrorIfLinkedToSameHouseAndContainerAsAnotherPivot(pivot.CV_AssociatedContainerInfo);
		}

		internal const string InvalidContainerFCXOrLCL = "Container Mode is invalid. Container Mode should be either FCX or LCL.";
		internal const string InvalidContainerLCL = "Container Mode is invalid. Container Mode should be LCL.";
		internal const string ValueCannotLessThan001 = "{0} cannot be less than 0.01. Otherwise it will be sent as 0.00.";

		protected override void CheckCV_GoodsDescription()
		{
			base.CheckCV_GoodsDescription();
			MessageValidation.CheckEntered(SCAPivot.CV_GoodsDescriptionInfo, "Goods description required on house bill: " + HouseBillNumberForErrormessage + " in container: " + ContainerNumberForErrorMessage);
			if (SCAPivot.CV_GoodsDescription.ToUpper().KeepChars("ABCDEFGHIJKLMNOPQRSTUVWXYZ").Length < 2)
			{
				SCAPivot.CV_GoodsDescriptionInfo.AddMessageError("Goods description should be at least two characters long.");
			}
		}

		protected override void CheckCV_IsSAC()
		{
			base.CheckCV_IsSAC();
			if (SCAPivot.CV_IsSAC && SCAPivot.HouseBill != null && SCAPivot.HouseBill.CA_IsMasterHouse)
			{
				SCAPivot.CV_IsSACInfo.AddMessageError("This cannot be self-assessed as the house bill is a co-load master.");
			}

			if (SCAPivot.HouseBill != null)
			{
				SCAPivot.HouseBill.Validation.ValidateCA_IsMasterHouse();
			}
		}

		protected override void CheckCV_MarksAndNumbers()
		{
			base.CheckCV_MarksAndNumbers();
			if (IsMarksAndNumbersRequired)
			{
				MessageValidation.CheckEntered(SCAPivot.CV_MarksAndNumbersInfo, "Marks and numbers required on house bill: " + HouseBillNumberForErrormessage + " in container: " + ContainerNumberForErrorMessage);
			}
		}

		protected override void CheckCV_PackageType()
		{
			base.CheckCV_PackageType();
			if (IsPackageTypeOrCountRequired)
			{
				MessageValidation.CheckEntered(SCAPivot.CV_PackageTypeInfo, "Package type is required on house bill: " + HouseBillNumberForErrormessage + " in container: " + ContainerNumberForErrorMessage);
			}
			ListValidation.MessageErrorIfInvalidCode(SCAPivot.CV_PackageTypeInfo, SCAPivot.Lookups.PackageTypes);
		}

		protected override void CheckCV_PackageCount()
		{
			base.CheckCV_PackageCount();
			if (IsPackageTypeOrCountRequired)
			{
				MessageValidation.CheckEntered(SCAPivot.CV_PackageCountInfo);
			}

			if (SCAPivot.CV_AssociatedContainer == CusSCAPivot.Bulk && SCAPivot.CV_PackageCount > 0)
			{
				SCAPivot.CV_PackageCountInfo.AddMessageError("Package count should not be included on Bulk Entries");
			}
		}

		protected override void CheckCV_Volume()
		{
			base.CheckCV_Volume();
			MessageValidation.CheckEntered(SCAPivot.CV_VolumeInfo, "Volume is required on house bill: " + HouseBillNumberForErrormessage + " in container: " + ContainerNumberForErrorMessage);
			CheckSmallValue(Parent.CV_VolumeInfo, "Volume");
		}

		protected override void CheckCV_Weight()
		{
			base.CheckCV_Weight();
			MessageValidation.CheckEntered(SCAPivot.CV_WeightInfo, "Gross weight is required on house bill: " + HouseBillNumberForErrormessage + " in container: " + ContainerNumberForErrorMessage);
			CheckSmallValue(Parent.CV_WeightInfo, "Gross Weight");
		}

		protected override void CheckCV_NetWeight()
		{
			base.CheckCV_NetWeight();
			CheckSmallValue(Parent.CV_NetWeightInfo, "Net Weight");
		}

		protected override void CheckCV_WeightUQ()
		{
			base.CheckCV_WeightUQ();
			MessageValidation.CheckEntered(SCAPivot.CV_WeightUQInfo, "Weight unit of quantity is required on house bill: " + HouseBillNumberForErrormessage + " in container: " + ContainerNumberForErrorMessage);
			ListValidation.MessageErrorIfInvalidCode(SCAPivot.CV_WeightUQInfo, SCAPivot.CV_WeightUQ_List);
		}

		protected void AddErrorIfLinkedToSameHouseAndContainerAsAnotherPivot(ZPropertyInfo infoToPutErrorOn)
		{
			if (SCAPivot.HouseBill != null)
			{
				foreach (CusSCAPivot pivot in SCAPivot.HouseBill.Pivot)
				{
					if (pivot != SCAPivot && pivot.CV_CN == SCAPivot.CV_CN && pivot.CV_CA == SCAPivot.CV_CA)
					{
						infoToPutErrorOn.AddError("Only one Packing Line is allowed to be linked per each House Bill and Container. Please remove any extra Packing Lines.");
						break;
					}
				}
			}
		}

		void CheckSmallValue(ZPropertyInfo info, string propertyName)
		{
			if (!info.Value.IsEmpty && ((ZDecimal)info.Value) < 0.01m)
			{
				info.AddMessageError(string.Format(ValueCannotLessThan001, propertyName));
			}
		}

		#endregion

		#endregion
	}
}
