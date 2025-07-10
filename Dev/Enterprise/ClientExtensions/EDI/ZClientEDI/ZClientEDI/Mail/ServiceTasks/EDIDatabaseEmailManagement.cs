using System.Globalization;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MailManager;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.Mail.ServiceTasks
{
	public class EDIDatabaseEmailManagement : DatabaseEmailManagement
	{
		public override int StopUnsuccessfulUpgradeSending()
		{
			const string packageColumnName = "Package";

			var factory = new BusinessObjectFactory();
			var failedUpgrades = new DynamicBusinessObjectCollection(factory);
			string sqlText = "EXECUTE ClientStopUnsuccessfulUpgradesSendingSP @AckIntervalInMinutes, @MaxAttempts";
			var parameters = new ZSqlParameterCollection();
			parameters.Add(ZSqlParameter.New("@AckIntervalInMinutes", MailAcknowledgement.AcknowledgmentTimeout, CargoWise.Schema.Schema.GenericIntSchemaColumn));
			parameters.Add(ZSqlParameter.New("@MaxAttempts", (int)MailAcknowledgement.MaxAttempts, CargoWise.Schema.Schema.GenericIntSchemaColumn));

			failedUpgrades.Load(sqlText, parameters);

			foreach (DynamicBusinessObject upgrade in failedUpgrades)
			{
				if (!string.IsNullOrEmpty(upgrade[GlbStaffSchema.GS_EmailAddress.Name].ToString()))
				{
					SendUpgradeSendingFailureEmail(
						upgrade[packageColumnName].ToString(),
						upgrade[MailDBRecipientsSchema.MR_RecipientMailAddress.Name].ToString(),
						upgrade[OrgHeaderSchema.OH_Code.Name].ToString(),
						upgrade[OrgHeaderSchema.OH_FullName.Name].ToString(),
						upgrade[GlbStaffSchema.GS_EmailAddress.Name].ToString());
				}
			}

			return failedUpgrades.Count;
		}

		void SendUpgradeSendingFailureEmail(string package, string upgradeEmail, string clientCode, string clientName, string userEmail)
		{
			upgradeEmail = upgradeEmail.Replace("Deliverance Update ", "");
			var email = new EmailDef
			{
				FromAddress = Env.Registry.MailboxEmailAddress,
				Subject = Res.GetString("27a7d031-261b-4c25-bb68-38c4a36fcd5b", "Upgrade Delivery Failure"),
				Body = Res.GetString("eb7b15d9-128e-417b-b67a-fd7b5d0ab490", "Dear User.\r\n\r\nThe upgrade package {0} which you sent to the Client {1} {2} to the following email address {3} could not be delivered.\r\nPlease try to identify and fix the reason of the failure before sending any upgrades to this address",
					package, clientCode, clientName, upgradeEmail),
			};
			email.AddRecipientForUserCommunication(userEmail);

			Env.OutgoingMailManager.CreateAndSave(email);
		}

		public override int TruncateBodyOutgoingUpgradeEmailOlderThan(int daysOld)
		{
			var setClause = string.Format(CultureInfo.InvariantCulture, "SET {0} = SUBSTRING({0}, 1, 72)", MI_Body);
			var additionalWhereClause = string.Format(CultureInfo.InvariantCulture, "{0} = '{1}' AND {2} LIKE 'Package%' AND len({3}) > 72",
				MI_Direction, Transmit, MI_Subject, MI_Body);

			return CleanOldMail(() => GetUpdateCommand(daysOld, setClause, additionalWhereClause));
		}

		const string MI_Direction = MailDBItemsSchema.Constants.MI_Direction;
		const string MI_Body = MailDBItemsSchema.Constants.MI_Body;
		const string MI_Subject = MailDBItemsSchema.Constants.MI_Subject;
		const string Transmit = MailDirection.Transmit;
	}
}
