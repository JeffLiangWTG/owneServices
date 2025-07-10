using System;
using System.Web;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class RegisterPersonalEmail : BasePage
	{
		protected override void OnLoad(EventArgs e)
		{
			LogoImage.ImageUrl = AppInstance.LogoImage;
			LogoImage.NavigateUrl = AppInstance.HomePage;
			LogoImage.ToolTip = AppInstance.CompanyName;

			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			if (string.IsNullOrEmpty(QueryToken) || !accessControl.TryConsume(QueryToken, AccessTokenTypes.RegisterPersonalEmail, out accessToken))
			{
				ErrorMessage.Text = InvalidTokenMessage;
				RegisterResult.Visible = false;
				return;
			}

			if (!IsPostBack && string.IsNullOrEmpty(Email))
			{
				ErrorMessage.Text = InvalidTokenMessage;
				RegisterResult.Visible = false;
				return;
			}

			base.OnLoad(e);
			MergeGlbPersons();

			ResultMessageLabel.Text = registeredEmailAddress ?
				Res.GetString("ad799af0-7337-4949-abad-6536e8dba525", "You have successfully changed your personal email.") :
				Res.GetString("c3ad0087-ff7f-4219-8807-57b6b519142c", "You have successfully registered your personal email.");
		}

		bool registeredEmailAddress;

		protected override bool ShouldSetupSessionOnLoad => !string.IsNullOrEmpty(QueryToken) && !string.IsNullOrEmpty(Email);

		protected override bool PageRequiresLogin(Uri url)
		{
			return false;
		}

		protected override bool ShowFooter => true;

		AccessTokenInfo accessToken;
		string Email => accessToken.Scope;

		string QueryToken => HttpContext.Current.Request.QueryString[AppInstance.RegisterKey];

		string InvalidTokenMessage => Res.GetString("794d9294-cd3e-4f2b-87c8-e5803ad39763", "The register link is invalid.");

		void MergeGlbPersons()
		{
			var orgContact = Factory.Load<OrgContact>(accessToken.ParentId);
			if (orgContact != null && orgContact.Person != null)
			{
				registeredEmailAddress = !orgContact.Person.PER_EmailAddress.IsEmpty;

				var query = new ZQuery(GlbPersonSchema.PER_EmailAddress, Email);
				query.AddToFilter(GlbPersonSchema.PER_FullName, orgContact.ContactNameWithoutNumberSuffix);
				var persons = Factory.Load<GlbPerson>(query);
				foreach (var person in persons)
				{
					if (person.PK != orgContact.Person.PK)
					{
						using (var personMerge = new PersonMerger(person, orgContact.Person))
						{
							personMerge.Merge();
						}
					}
				}

				orgContact.Person.Reload();
				orgContact.Person.PER_EmailAddress = Email;
				Factory.Save();
			}
		}
	}
}
