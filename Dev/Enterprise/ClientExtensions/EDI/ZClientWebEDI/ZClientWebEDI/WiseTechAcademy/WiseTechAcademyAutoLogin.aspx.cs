using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.WiseTechAcademy;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class WiseTechAcademyAutoLogin : BasePage
	{
		protected void Page_Init(object sender, EventArgs e)
		{
			if (EDIDataRegistry.Instance.EnableAcademyJWTAuthentication.Value)
			{
				LoginForm.Controls.Remove(TenantId);
				LoginForm.Controls.Remove(Product);
				LoginForm.Controls.Remove(OrgPk);
				LoginForm.Controls.Remove(OrgName);
				LoginForm.Controls.Remove(ContactWorkingAddressOrgName);
				LoginForm.Controls.Remove(ContactWorkingAddressCountry);
				LoginForm.Controls.Remove(ContactLocation);
				LoginForm.Controls.Remove(LicenceDatabaseMasterOrgCode);
				LoginForm.Controls.Remove(LicenceDatabaseMasterOrgName);
				LoginForm.Controls.Remove(LicenceDatabaseBillingOrgCode);
				LoginForm.Controls.Remove(LicenceDatabaseBillingOrgName);
				LoginForm.Controls.Remove(UserId);
				LoginForm.Controls.Remove(ContactPk);
				LoginForm.Controls.Remove(ContactName);
				LoginForm.Controls.Remove(ContactEmail);
				LoginForm.Controls.Remove(PersonalEmail);
				LoginForm.Controls.Remove(PersonIDs);
			}
			base.OnPreRender(e);
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			var autoLoginUrl = EDIDataRegistry.Instance.WiseTechAcademyAutoLoginUrl.Value;
			if (string.IsNullOrEmpty(autoLoginUrl))
			{
				return;
			}

			var loginContact = SiteUser?.LoggedInUser;
			if (loginContact == null || loginContact.Person == null || SiteUser.IsSuperUser || EnvProxy.Instance?.CurrentUser == null)
			{
				return;
			}

			var (userAccount, billingOrg) = WiseTechAcademyAutoLoginHelper.GetOrCreateCustomerUserAccount(SiteUser.LoggedInOrganisation, SiteUser.LoggedInOrgContact);
			if (userAccount == null)
			{
				return;
			}

			var ld = userAccount.Database;
			LicenceDatabase originalSystemLicenceDatabase = null;

			var databaseNumber = Session["DatabaseNumber"];
			if (databaseNumber != null && int.TryParse(databaseNumber.ToString(), out int sessionNumber) && sessionNumber != 0)
			{
				originalSystemLicenceDatabase = Factory.LoadTop1<LicenceDatabase>(
					new ZQuery(LicenceDatabaseSchema.LD_DatabaseNumber, sessionNumber));
			}

			if (EDIDataRegistry.Instance.EnableAcademyJWTAuthentication.Value)
			{
				Token.Value = WiseTechAcademyAutoLoginAuthTokenHelper.GenerateAuthToken(userAccount, ld, loginContact, billingOrg, originalSystemLicenceDatabase);
			}
			else
			{
				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				var accessToken = accessControl.CreateLimitedToken(WiseTechAcademyAccessTokenData.TokenType, new AccessTokenInfo("", loginContact.PK.ToGuid(), OrgContactSchema.Constants.Prefix), TimeSpan.FromMinutes(5), maxUses: 1);
				Token.Value = accessToken;
				TenantId.Value = ld.LD_TenantID;
				Product.Value = ld.LD_Product;
				OrgPk.Value = loginContact.OC_OH.ToString();
				OrgName.Value = loginContact.Header.OH_FullName;
				ContactWorkingAddressOrgName.Value = loginContact.WorkingAddressCompanyName;
				ContactWorkingAddressCountry.Value = WiseTechAcademyAutoLoginHelper.GetWorkingAddressCountryCode(loginContact);
				ContactLocation.Value = loginContact.Location;
				LicenceDatabaseMasterOrgCode.Value = ld.WebAccessOrg?.OH_Code ?? ZString.Empty;
				LicenceDatabaseMasterOrgName.Value = ld.WebAccessOrg?.OH_FullName ?? ZString.Empty;
				LicenceDatabaseBillingOrgCode.Value = billingOrg.OH_Code;
				var billingOrgName = billingOrg.Addresses.DefaultAddressOfType(OrgAddressType.Receivables)?.CompanyName ?? ZString.Empty;
				LicenceDatabaseBillingOrgName.Value = !billingOrgName.IsEmpty ? billingOrgName : billingOrg.OH_FullName;
				UserId.Value = userAccount.EUA_UserID;
				ContactPk.Value = loginContact.PK.ToString();
				ContactName.Value = loginContact.ContactNameWithoutNumberSuffix;
				ContactEmail.Value = loginContact.OC_Email;
				PersonalEmail.Value = loginContact.Person.PER_EmailAddress;
				PersonIDs.Value = string.Join(",", loginContact.Person.PersonIDs.Select(x => x.ToString()));
			}

			Path.Value = Request.QueryString["path"];
			Target.Value = Request.QueryString["target"];
			QuickstartId.Value = Request.QueryString["quickstartid"];
			SearchText.Value = Request.QueryString["searchText"];
			CourseId.Value = Request.QueryString["courseid"];
			Type.Value = Request.QueryString["type"];
			ProgramId.Value = Request.QueryString["programid"];
			AutoLoginOriginProduct.Value = originalSystemLicenceDatabase?.CurrentVersion?.HL_Product ?? string.Empty;
			AutoLoginOriginProductVersion.Value = originalSystemLicenceDatabase?.CurrentVersion?.VersionNumber.ToString() ?? string.Empty;

			LoginForm.Action = autoLoginUrl;
			DefaultBody.Attributes.Add("onload", "SubmitLoginForm()");
		}
	}
}
