namespace Enterprise.Accounting.Business.GlobalChargeCode
{
	using CargoWise.EntityFramework;

	public class GlobalChargeCodeMapOrganizationValidation : AccGlobalChargeCodeMapValidation
	{
		public GlobalChargeCodeMapOrganizationValidation(AutoAccGlobalChargeCodeMap parent)
			: base(parent)
		{
		}

		#region Properties

		protected override void CheckYG_OH()
		{
			base.CheckYG_OH();
			MandatoryValidation.CheckEntered(Parent.YG_OHInfo);
		}

		#endregion
	}
}

