using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class CusTempStorageJobHeaderValidation : EU.Business.CusTempStorage.CusTempStorageJobHeaderValidation
	{
		public CusTempStorageJobHeaderValidation(AutoCusTempStorageJobHeader parent) : base(parent)
		{
		}

		protected override void CheckSJH_CustomsProfile()
		{
			base.CheckSJH_CustomsProfile();
			if (Parent.Lookups.CustomsProfileList.Count > 0)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.SJH_CustomsProfileInfo);
			}
		}

		protected override void CheckSJH_CPH_Guarantee()
		{
			base.CheckSJH_CPH_Guarantee();
			if (Parent.IsIST || Parent.IsLADT)
			{
				var guarantee = Parent.GuaranteeHeader;
				var customDec = Parent.CusTempStorageDec;
				if (guarantee != null && customDec != null)
				{
					var totallineAmount = customDec.TotalTSIGuaranteedValueAmount;
					if (guarantee.CPH_Calc_TotalBalanceIncludingPending.Amount < customDec.TotalTSIGuaranteedValueAmount)
					{
						var errorMessage = Res.GetString("45088C99-4376-47EA-ABA7-E0DBB68E12EE", "{0} has only {1} remaining but this job requires {2}.", guarantee.HumanReadableName, guarantee.CPH_Calc_TotalBalanceIncludingPending.Amount, totallineAmount);
						Parent.SJH_CPH_GuaranteeInfo.AddMessageError(errorMessage);
					}
				}
			}
		}

		protected override void CheckSJH_OA_Presenter()
		{
			base.CheckSJH_OA_Presenter();
			if (Parent.IsIST)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SJH_OA_PresenterInfo);
			}
		}

		protected override void CheckSJH_PreviousReferenceType()
		{
			base.CheckSJH_PreviousReferenceType();
			if (Parent.IsIST)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SJH_PreviousReferenceTypeInfo);
				ValidateSJH_PreviousReferenceNumber();
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.SJH_PreviousReferenceTypeInfo);
		}

		protected override void CheckSJH_PreviousReferenceNumber()
		{
			base.CheckSJH_PreviousReferenceNumber();
			if (Parent.IsIST)
			{
				if (!Parent.SJH_PreviousReferenceType.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.SJH_PreviousReferenceNumberInfo);
				}
			}
		}

		protected override void CheckSJH_CustomsOffice()
		{
			base.CheckSJH_CustomsOffice();
			if (Parent.IsIST)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SJH_CustomsOfficeInfo);
			}
		}

		protected new CusTempStorageJobHeader Parent => base.Parent as CusTempStorageJobHeader;
	}
}
