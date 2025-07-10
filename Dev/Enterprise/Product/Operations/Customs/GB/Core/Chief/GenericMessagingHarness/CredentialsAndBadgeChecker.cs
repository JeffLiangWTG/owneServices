using System.Globalization;
using CargoWise.Application;
using CargoWise.BrandManager;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Chief.GenericMessagingHarness
{
	public class CredentialsAndBadgeChecker
	{
		readonly ISendsMessagesToCustoms warner;
		public CredentialsAndBadgeChecker(ISendsMessagesToCustoms warner)
		{
			this.warner = warner;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public bool CredentialsExistForBadgeAndAreNotLockedOut(string badge, int lockoutThreshold, string csp)
		{
			if (!string.IsNullOrEmpty(csp))
			{
				var credential = GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, System.Guid.Empty, System.Guid.Empty).FindByBadgeCode(badge);
				if (credential == null)
				{
					warner.NotifyUserOfAnInvalidOperation(string.Format(CultureInfo.InvariantCulture, "No credentials could be found for badge {0}. It is not possible to communicate with {1} without such. Please have your administrator set up the registry correctly.", badge, csp));
					return false;
				}
				else if (lockoutThreshold > -1 && credential.WebServiceFailureCount >= lockoutThreshold)
				{
					warner.NotifyUserOfAnInvalidOperation(string.Format(CultureInfo.InvariantCulture,
	@"Credentials for badge {0} are suspended after repeated failed logins.

{1} has rejected {2}'s use of the username and password for this badge multiple times.

{2} has suspended further use of these credentials in order to prevent a permanent block by {1} of your badge’s credentials or source IP address.

To lift the suspension follow these steps:
1) Navigate to Maintain > System > Registry > Customs > United Kingdom > Badge Code Credentials.
2) Select the relevant row in the grid by left clicking it, then right-click the row's gutter to bring up the context menu.
3) Select 'Check Credentials' from the popup menu.
(This will make a connection to {1} and attempt to login. Note that the connection will be made from your desktop or terminal server, not from the Process Controller server, so your network must allow such traffic.  If the login is permitted by {1}, the suspension will be removed.)
4) Save the changes to the registry.

If the connection check is unsuccessful, and only after undertaking the above four steps for each affected badge, please contact the {1} helpdesk.

Please only contact {3} about this issue once you have performed the above steps.", badge, csp, BrandingFactory.Instance.ProductName, BrandingFactory.Instance.CompanyName));
					return false;
				}
				else if (GBCustomsDataRegistry.Instance.ForceOldWebCredentialsToBeTestedBeforeUse.Value && credential.IsHttpWebCredential && credential.DataTestStatus != DataTestStatusList.Codes.Valid)
				{
					warner.NotifyUserOfAnInvalidOperation(string.Format(CultureInfo.InvariantCulture, "Credentials found for badge {0} have not been proved as valid. {1} \r\n{2}", badge, ExplanationOfWhyCredentialsCannotBeUsedNotTested, new DataTestStatusList().GetDescriptionFromCode(credential.DataTestStatus)));
					return false;
				}
				return true;
			}
			else
			{
				warner.NotifyUserOfAnInvalidOperation(string.Format(CultureInfo.InvariantCulture, Business.Declaration.JobDeclarationValidation.CSPZG_GatewayValidationError));
				return false;
			}
		}

		public const string ExplanationOfWhyCredentialsCannotBeUsedNotTested = "To mitigate the problems caused by use of potentially bad credentials, you may not use this badge until the credentials have been tested positively. This step need only be performed once before this badge can be used. Open Registry > Customs > United Kingdom > Badge Code Credentials, select this badge's credentials row in the grid, right click the row's gutter, choose 'Check Credentials' from the popup, and save the changes.";

		public static string GetUserAgentForSoapRequests(string badgeCode)
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var localInv = CultureInfo.InvariantCulture;
			var ret = string.Format(localInv,
									"Vendor={0}, Application={1}, Version={2}, Badge={3}, ClientID={4}",
									BrandingFactory.Instance.CompanyName,
									BrandingFactory.Instance.ProductName,
									new EnterpriseInformationRetriever().VersionNumber,
									badgeCode,
									registrationKey.EnterpriseCode + registrationKey.ServerCode);
			return ret;
		}
	}
}
