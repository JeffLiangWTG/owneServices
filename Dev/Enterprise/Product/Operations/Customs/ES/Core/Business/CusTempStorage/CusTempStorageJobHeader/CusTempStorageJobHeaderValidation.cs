using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.ES.Business.CusTempStorage
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
			if (Parent.IsIST)
			{
				var guarantee = Parent.GuaranteeHeader;
				var customDec = Parent.CusTempStorageDec;
				if (guarantee != null && customDec != null)
				{
					var totallineAmount = customDec.TotalTSIGuaranteedValueAmount;
					if (guarantee.CPH_Calc_TotalBalanceIncludingPending.Amount < customDec.TotalTSIGuaranteedValueAmount)
					{
						var errorMessage = Res.GetString("6B7B5403-D11E-424E-B8E2-5C1D4CAD5231", "{0} has only {1} remaining but this job requires {2}.", guarantee.HumanReadableName, guarantee.CPH_Calc_TotalBalanceIncludingPending.Amount, totallineAmount);
						Parent.SJH_CPH_GuaranteeInfo.AddMessageError(errorMessage);
					}
				}
			}
		}

		protected new CusTempStorageJobHeader Parent => base.Parent as CusTempStorageJobHeader;
	}
}
