using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class PageForTest : BasePage
	{
		protected override BusinessObject GetNewDataSource()
		{
			return new UserAccountDeactivationManager(SiteUser?.LoggedInOrgContact?.Person);
		}

		public void DoPageLoad()
		{
			try
			{
				base.OnLoad(EventArgs.Empty);
				UserAccountRelationship.OnLoad();
				PasswordChangeRequirements.OnLoad();
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
			UserAccountRelationship = new UserAccountRelationshipControlForTest();
			UserAccountRelationship.Page = this;

			PasswordChangeRequirements = new PasswordChangeRequirementsControlForTest();
			PasswordChangeRequirements.Page = this;
		}

		public UserAccountRelationshipControlForTest UserAccountRelationship;

		public PasswordChangeRequirementsControlForTest PasswordChangeRequirements;

		class GlobalForTest : Global
		{
			public void OnCustomSessionStart()
			{
				base.OnCustomSessionStart(this, EventArgs.Empty);
			}
		}
	}
}
