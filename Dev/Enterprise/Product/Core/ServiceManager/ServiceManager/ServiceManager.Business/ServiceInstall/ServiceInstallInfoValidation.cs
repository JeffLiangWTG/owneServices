namespace Enterprise.ServiceManager.Business
{
	public class ServiceInstallInfoValidation : AutoServiceInstallInfoValidation
	{
		public ServiceInstallInfoValidation(AutoServiceInstallInfo parent)
			: base(parent) { }

		protected override void CheckConfirmPassword()
		{
			base.CheckConfirmPassword();

			if (Parent.Password != Parent.ConfirmPassword)
			{
				Parent.ConfirmPasswordInfo.AddError("Passwords mismatch.");
			}
		}

		#region Implementation

		public new ServiceInstallInfo Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ServiceInstallInfo)base.Parent; }
		}

		#endregion
	}
}

