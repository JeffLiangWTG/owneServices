using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSExpiringTokenNotificationProcessor
	{
		public const string EmailSubject = "CDS Access Tokens Expiring";
		public const string DateTimeFormat = "yyyy-MM-dd HH:mm";

		public CDSExpiringTokenNotificationProcessor(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public void CheckForExpiringCredentials()
		{
			var cdsTokens = factory.Load<GlbExternalPassword_GB>(new ZQuery(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.CDS)
																		.AddToFilter(GlbExternalPasswordSchema.GP_GC, GlbCompany.CurrentCompany.PK)
																		.AddToFilter(GlbExternalPasswordSchema.GP_PasswordStatus, PasswordStatusList.Codes.Valid));

			var expiring = cdsTokens.Where(x => CdsGlbExternalPasswordChecker.ExternalPasswordAboutToExpire(x)).ToList();

			if (expiring.Any())
			{
				CreateAndSendExpiryNotification(expiring);
			}
		}

		void CreateAndSendExpiryNotification(List<GlbExternalPassword_GB> expiring)
		{
			var emailContent = CreateEmailContent(expiring);

			var email = new HtmlNotificationEmailSender().CreateEmail(EmailSubject, emailContent);
			var emailSender = new EmailSender(new LoggingInformation());

			emailSender.SendEmail_SaveNow(email,
						GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationCDS, "", GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty),
						GBCustomsDataRegistry.Instance.GetNotificationItem("", "", GBCustomsDataRegistry.Instance.NotificationCDS));
		}

		string CreateEmailContent(List<GlbExternalPassword_GB> expiring)
		{
			var hyperlinkedText = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.GlbCompany, GlbCompany.CurrentCompany.PK.ToGuid());

			return string.Format(CultureInfo.InvariantCulture,
				string.Concat(
					@"<p>Company: <a href=""{1}"">{0}</a></p>",
					@"<p>The following set of credentials to access CDS will expire soon.</p>",
					@"<p>{2}</p>"
				),
				GlbCompany.CurrentCompany.CompanyName,
				hyperlinkedText,
				CreateEmailDataTable(expiring)
			);
		}

		string CreateEmailDataTable(List<GlbExternalPassword_GB> expiring)
		{
			var tbl = new HtmlTableCreator();

			tbl.WriteRowWithFormatting(
				new CellWithFormatting("EORI & Badge", "width", "180px", true),
				new CellWithFormatting("Issue Date", "width", "120px", true),
				new CellWithFormatting("Expiry Date", "width", "120px", true)
			);

			foreach (var ep in expiring)
			{
				tbl.WriteRow(ep.GP_UserID, ep.GP_IssueDate.ToString(DateTimeFormat, CultureInfo.InvariantCulture), ep.GP_ExpiryDate.ToString(DateTimeFormat, CultureInfo.InvariantCulture));
			}

			return tbl.ToHtml();
		}

		readonly BusinessObjectFactory factory;
	}
}
