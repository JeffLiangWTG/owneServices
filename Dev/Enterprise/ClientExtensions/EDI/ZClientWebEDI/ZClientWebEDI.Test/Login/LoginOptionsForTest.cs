using System;
using CargoWise.Common;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class LoginOptionsForTest : LoginOptions
	{
		public void DoPageLoad()
		{
			try
			{
				base.OnLoad(EventArgs.Empty);
				AccountVerificationExposed.OnLoad();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (ex is QueryStringException)
				{
					throw;
				}
			}
		}

		protected override ZGlobal GetNewTestGlobal()
		{
			var result = new GlobalForTest();
			result.OnCustomSessionStart();
			return result;
		}

		public void InitialiseControls()
		{
			AccountVerification = new AccountVerificationControlForTest();
			AccountVerification.Page = this;
		}

		public AccountVerificationControlForTest AccountVerificationExposed => (AccountVerificationControlForTest)AccountVerification;
		class GlobalForTest : Global
		{
			public void OnCustomSessionStart()
			{
				base.OnCustomSessionStart(this, EventArgs.Empty);
			}
		}
	}
}