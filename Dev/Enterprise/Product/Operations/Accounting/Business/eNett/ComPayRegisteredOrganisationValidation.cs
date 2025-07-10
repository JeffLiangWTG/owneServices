namespace Enterprise.Accounting.Business.eNett
{
	public class ComPayRegisteredOrganisationValidation : AutoComPayRegisteredOrganisationValidation
	{
		public ComPayRegisteredOrganisationValidation(AutoComPayRegisteredOrganisation parent)
			: base(parent) { }

		#region Implementation

		public new ComPayRegisteredOrganisation Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ComPayRegisteredOrganisation)base.Parent; }
		}

		protected override void CheckRegistrationDateIsValidZDateTimeRange()
		{
		}

		#endregion
	}
}