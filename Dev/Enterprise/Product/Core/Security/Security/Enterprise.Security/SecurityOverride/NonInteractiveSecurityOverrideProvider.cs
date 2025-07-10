namespace Enterprise.Security
{
	public sealed class NonInteractiveSecurityOverrideProvider : SecurityOverrideProvider, Enterprise.Integration.Security.INonInteractiveSecurityOverrideProvider
	{
		public string OverrideLogin
		{
			get { return fOverrideLogin; }
			set { fOverrideLogin = value; }
		}

		public string OverridePassword
		{
			get { return fOverridePassword; }
			set { fOverridePassword = value; }
		}

		protected override SecurityCore RequestLoginCredentials(SecurityCheckpoint checkPoint)
		{
			return new AlternativeCredentials(fOverrideLogin, fOverridePassword).UserSecurity;
		}

		protected override SecurityCertificate RequestGrantedConfirmation(SecurityCheckpoint checkPoint)
		{
			return SecurityCertificate.Granted;
		}

		#region Implementation

		string fOverridePassword;
		string fOverrideLogin;

		#endregion
	}
}
