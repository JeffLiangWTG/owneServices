using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using ContactNotifyModes = Enterprise.Core.Constants.ContactNotifyModes;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class OnlyElectronicBulkDeliveryMethod : BulkDeliveryMethod
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is used as a code.")]
		public const string CodeText = "Only Electronic";

		public OnlyElectronicBulkDeliveryMethod()
			: base(CodeText, Res.GetString("6ee32a6f-e2fa-4266-a4b8-d11a01163e8c", "Only deliver documents destined for electronic delivery (email, fax)")) { }

		public override bool AllowCoverNote
		{
			get { return true; }
		}

		public override bool AllowDeliverDocumentsInOneEmail => true;

		protected override void SetRecipientsCore(DeliveryInstructions instructions, DocDeliveryContactCollection contacts)
		{
			foreach (DocDeliveryContact contact in contacts)
			{
				if (instructions.DeliverDocumentsInOneEmail)
				{
					if (contact.DeliveryMethod == ContactNotifyModes.Email)
					{
						instructions.Recipients.Add(contact);
					}
				}
				else if (contact.DeliveryMethod != ContactNotifyModes.Print)
				{
					instructions.Recipients.Add(contact);
				}
			}
		}
	}
}
