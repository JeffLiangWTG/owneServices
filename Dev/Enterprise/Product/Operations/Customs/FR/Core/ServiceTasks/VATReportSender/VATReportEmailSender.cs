using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.FR.ServiceTasks
{
	public class VATReportEmailSender
	{
		readonly BusinessObjectFactory factory;
		readonly LoggingInformation logger;

		public VATReportEmailSender(BusinessObjectFactory factory, LoggingInformation logger)
		{
			this.factory = factory;
			this.logger = logger;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Email")]
		public void DoEverything(OrgHeader importer, GlbCompany company, ZString filePath, ZString attachDisplayName)
		{
			var date = ZDateTime.Today.AddMonths(-1);

			if (File.Exists(filePath))
			{
				try
				{
					var email = new EmailDef();
					email.Subject = string.Format(@"VAT report for {0} in {1}/{2} for {3}", importer.OH_FullName, date.Month, date.Year, company.GC_Name);
					email.Body = @"Bonjour,

Veuillez trouver ci-joint le rapport d’auto-liquidation de TVA du mois précédent.";
					var attachment = new AttachmentDef(attachDisplayName, filePath);
					email.Attachments.Add(attachment);

					var recipients = importer.Contacts.Cast<OrgContact>().Where(x => x.OC_IsActive && !x.OC_Email.IsEmpty && x.Allocations.Cast<OrgContactAllocation>().Any(y => y.PC_Type == OrgConstants.ContactAllocationType.VAT))?.Select(x => x.OC_Email).Distinct() ?? Enumerable.Empty<ZString>();
					if (!recipients.Any())
					{
						if (!importer.MainAddress.OA_Email.IsEmpty)
						{
							recipients = new ZString[] { importer.MainAddress.OA_Email };
						}
					}

					if (!recipients.Any())
					{
						if (!company.GC_Email.IsEmpty)
						{
							recipients = new ZString[] { company.GC_Email };
							email.Body += string.Format("\r\nIl n'y a pas d'adresse e-mail définie pour l'importateur {0}, en conséquence le rapport de TVA ne leur a pas été envoyé, veuillez leur transmettre l'e-mail et configurer l'adresse e-mail dans CargoWiseOne.", importer.OH_FullName);
						}
					}

					email.Body += @"

Sincères salutations,
Le service Douane.";

					if (!recipients.Any())
					{
						logger.LogError(string.Format(@"Please set the email of the contacts with VAT allocated contact of {0}.", importer.OH_FullName));
					}
					else
					{
						email.AddRecipientForSystemCommunication(recipients.Select(x => x.ToString()).ToArray());
						Env.OutgoingCustomsMailManager.Create(factory, email);
						logger.Log(string.Format(@"Send email for {0} successfully.", importer.OH_FullName));
					}
				}
				finally
				{
					File.Delete(filePath);
				}
			}
			else
			{
				logger.LogError(string.Format(@"There is no VAT report for {0} in {1}/{2} for {3}.", importer.OH_FullName, date.Month, date.Year, company.GC_Name));
			}
		}
	}
}
