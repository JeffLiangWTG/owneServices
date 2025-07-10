using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CalculateInsuranceNonPersistentBizObjValidation : AutoCalculateInsuranceNonPersistentBizObjValidation
	{
		public CalculateInsuranceNonPersistentBizObjValidation(AutoCalculateInsuranceNonPersistentBizObj parent) : base(parent)
		{
		}

		protected override void CheckInsurancePercentage()
		{
			base.CheckInsurancePercentage();

			MandatoryValidation.CheckNotNegative(Parent.InsurancePercentageInfo);
		}

		protected override void CheckDutiablePercent()
		{
			base.CheckDutiablePercent();
			if (!Parent.DutiablePercent.IsInRange(CargoWise.Types.ZDecimal.Zero, 100m))
			{
				Parent.DutiablePercentInfo.AddError(PercentageShouldBeBetween);
			}
		}

		static string PercentageShouldBeBetween => Res.GetString("AC38359B-63FD-4C47-BC08-0B3D8B859494", "Percentage value should be between 0 and 100.");
	}
}
