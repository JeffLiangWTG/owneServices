using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.DeliveryMethods
{
	class Fax : QueuedForBatchProcessor
	{
		public Fax(DocDeliveryContact docContact)
		{
			Contact = docContact;
			Number = docContact.Fax;
			if (string.IsNullOrEmpty(Number.Trim()))
			{
				string subject = Res.GetString("be92ccfc-f6f6-4e22-bde7-44ee22d6f161", "Fax Sending Failure");
				string recepientsWithoutFax = "\r\n\t\t" + Res.GetString("3dd12039-0282-4404-a2f7-36d01296c61f", @"Organization: {0}
		Contact: {1}", docContact.CompanyName, docContact.Name);
				string body = "\r\n" + Res.GetString("a1627c4d-38ef-4bc4-af49-d355c77aadfd", "Failed to send Fax because fax number was empty. \r\n\t\r\n\tPlease set up fax number for recipient(s): \r\n\t{0}", recepientsWithoutFax);
				string bodyForMessageBox = Res.GetString("2a277848-bdfe-432f-a301-cb4af1dd2d4b", "Failed to send Fax because fax number was empty. Please set up fax number for recipient(s): \r\n{0}", recepientsWithoutFax);

				EmailDef emailDef = new EmailDef();
				emailDef.AddGroupOfRecipientsOrAllUsersIfGroupIsEmpty(Env.Registry.PostMasterGroup, RawDataRegistry.Instance.NotificationGroup);
				emailDef.Subject = subject;
				emailDef.Body = body;

				Globals.Message.ShowError(bodyForMessageBox, subject);

				Env.OutgoingMailManager.CreateAndSave(emailDef);
			}
		}

		readonly DocDeliveryContact Contact;

		protected override bool ConsolidateReports => !Contact.SendIndividually;

		public readonly string Number;

		protected override void SetAdditionalProperties(StmPrintJob printJob, DeliveryInfo deliveryInfo)
		{
			printJob.SP_FaxDestination = Number;
		}

		protected override DocumentEngine.PrintType PrintType
		{
			get { return DocumentEngine.PrintType.FAX; }
		}

		protected override bool MergeTiffsOnDelivery
		{
			get
			{
				return true;
			}
		}
	}
}
