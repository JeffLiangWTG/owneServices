using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.XmlCredential;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business
{
	public class GBCustomsCDSXmlCredentialConfigurationHandler : GlbExternalPasswordConfigurationHandler
	{
		public GBCustomsCDSXmlCredentialConfigurationHandler(LoggingInformation logger)
		: base(logger)
		{
		}

		protected override void ProcessCredential(GlbExternalPassword externalPassword, Group group, Credential credential, ZString statusReason)
		{
			if (group.StatusSpecified)
			{
				PopulateFields(group, externalPassword as GlbExternalPassword_GB);
			}
		}

		protected override ZString GetPasswordStatus(Group group)
		{
			return group.Status;
		}

		protected override GlbExternalPassword GetExternalPassword(Group groupData, Credential credential, GlbCompany company, GlbGroup group, GlbStaff staff)
		{
			GlbExternalPassword_GB result = null;
			var passwordType = groupData.Type;
			if (string.Equals(passwordType, PasswordTypesList.Codes.CDS, System.StringComparison.OrdinalIgnoreCase))
			{
				var mailBoxItem = GetMailBoxItem(groupData);
				if (mailBoxItem != null)
				{
					result = FindAndLoadGlbExternalPassword((credential?.UserName ?? ZString.Empty), mailBoxItem.Value, Factory);
					if (result != null)
					{
						result.DisableConfigurationToSender = true;
					}
					else
					{
						result = CreatePassword(groupData, credential, mailBoxItem, company);
					}
				}
			}

			return result;
		}

		internal static GlbExternalPassword_GB FindAndLoadGlbExternalPassword(ZString userIdAkaBadge, ZString mailboxIdAkaEori, BusinessObjectFactory factory)
		{
			var query = new ZQuery(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.CDS); // application code essentially
			query.AddToFilter(GlbExternalPasswordSchema.GP_UserID, mailboxIdAkaEori + "." + userIdAkaBadge);
			return factory.LoadTop1<GlbExternalPassword_GB>(query);
		}
		public static GlbExternalPassword_GB FindAndLoadGlbExternalPassword(ZString eori, BusinessObjectFactory factory)
		{
			var query = new ZQuery(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.CDS);
			query.AddToFilter(GlbExternalPasswordSchema.GP_UserID, SQLComparisonOperator.StartsWith, eori + ".");
			var passwords = factory.Load<GlbExternalPassword_GB>(query);

			return passwords.OfType<GlbExternalPassword_GB>().Where(x => !x.IsInvalid).OrderBy(x => x.Badge).ThenByDescending(x => x.GP_ExpiryDate).FirstOrDefault();
		}

		GlbExternalPassword_GB CreatePassword(Group groupData, Credential credential, Item mailBoxItem, GlbCompany company)
		{
			if (company == null)
			{
				company = GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.UnitedKingdom).FirstOrDefault();
			}
			GlbExternalPassword_GB result = Factory.New<GlbExternalPassword_GB>();
			PopulateFields(groupData, result);
			result.GP_GC = company.PK;
			result.GP_PasswordType = PasswordTypesList.Codes.CDS;
			result.GP_CurrentPassword = string.Empty;
			result.GP_NextPassword = string.Empty;
			result.Badge = credential?.UserName ?? ZString.Empty;
			result.EORI = mailBoxItem.Value;
			return result;
		}

		void PopulateFields(Group groupData, GlbExternalPassword_GB password)
		{
			if (password != null)
			{
				password.Status = GetPasswordStatus(groupData);
				var message = GetAnnotationItem(groupData, Constants.ItemTypes.Message)?.Value;
				var tokenType = GetAnnotationItem(groupData, Constants.ItemTypes.TokenType)?.Value;
				password.StatusMessage = ZString.Format("{0} {1} - {2}", tokenType, Constants.ItemTypes.Token, message).SubstringSafe(0, GlbExternalPassword.Schema.GP_StatusReasonMaxLength);

				var issueDate = GetDateTimeField(groupData, Constants.ItemTypes.Issued);
				if (!issueDate.IsEmpty)
				{
					if (password.GP_IssueDate.IsEmpty)
					{
						password.GP_IssueDate = issueDate;
					}
					else if (message.HasValue && ZArchitecture.Core.StringExtension.Contains(message.ToString(), "received", System.StringComparison.CurrentCultureIgnoreCase))
					{
						password.GP_IssueDate = issueDate;
					}
				}

				var expiryDate = GetDateTimeField(groupData, Constants.ItemTypes.Expires);
				password.GP_ExpiryDate = expiryDate.IsEmpty ? password.GP_ExpiryDate : expiryDate;
			}
		}

		ZDateTime GetDateTimeField(Group groupData, ZString item)
		{
			ZDateTime result = ZDateTime.Empty;
			if (ZDateTimeOffset.TryParse(GetAnnotationItem(groupData, item)?.Value, out ZDateTimeOffset date, CultureInfo.InvariantCulture))
			{
				if (date.IsValid)
				{
					result = date.ToDateTime();
				}
			}

			return result;
		}

		Item GetMailBoxItem(Group groupData)
		{
			return groupData?.Items.OfType<Item>().FirstOrDefault(x => string.Equals(x.Name, Constants.ItemTypes.MailBoxID, System.StringComparison.OrdinalIgnoreCase));
		}

		Item GetAnnotationItem(Group groupData, ZString name)
		{
			return groupData?.Annotations.OfType<Item>().FirstOrDefault(x => string.Equals(x.Name, name, System.StringComparison.OrdinalIgnoreCase));
		}
	}
}
