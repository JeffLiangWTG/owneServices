using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class DummyPageForTest : BasePage
	{
		public DummyPageForTest(EdiCustomerUserAccount user, OrgContact contact)
		{
			UserAccount = user;
			Contact = contact;
		}

		readonly EdiCustomerUserAccount UserAccount;
		readonly OrgContact Contact;
		protected override BusinessObject GetNewDataSource()
		{
			var helper = new LoginOptionsHelper(Factory, Contact, UserAccount);
			return helper;
		}

		public void DoPageLoad()
		{
			try
			{
				base.OnLoad(EventArgs.Empty);
				AccountVerification.OnLoad();
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

		public AccountVerificationControlForTest AccountVerification;
		class GlobalForTest : Global
		{
			public void OnCustomSessionStart()
			{
				base.OnCustomSessionStart(this, EventArgs.Empty);
			}
		}
	}
}