using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.Business
{
	public class UPEDocumentPack : DocumentPack
	{
		public UPEDocumentPack() : base() { }

		public UPEDocumentPack(DocumentCommand command, IDocumentSupportable bizObject, UserControlProviderList userFieldList, DocumentCommand parentCommand)
			: base(command, bizObject, userFieldList, parentCommand) { }

		protected override void Run(DeliveryInstructions instructions, INotifications notifications = null)
		{
			foreach (string receiptant in new EmailGroupUtility().GetGroupEmailCollection(UPEDataRegistry.Instance.CreditNotificationGroup, false))
			{
				DeliveryContact.Email = receiptant;
				base.RunForContact(DeliveryContact, instructions, notifications);
			}
		}

		DocDeliveryContact DeliveryContact
		{
			get
			{
				if (deliveryContact == null)
				{
					deliveryContact = TempContact.DocDeliveryDetails();
					deliveryContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
					deliveryContact.AttachmentType = OrgConstants.AttachmentType.PDF;
				}
				return deliveryContact;
			}
		}
		DocDeliveryContact deliveryContact;

		OrgContact TempContact
		{
			get { return tempContact ?? (tempContact = NonPersistentFactory.New<OrgContact>()); }
		}
		OrgContact tempContact;

		protected BusinessObjectFactory NonPersistentFactory
		{
			get
			{
				if (nonPersistentFactory == null)
				{
					nonPersistentFactory = new BusinessObjectFactory();
					nonPersistentFactory.NameForDebugging = "TempContactCreator";
					nonPersistentFactory.RefreshEnabled = false;
				}
				return nonPersistentFactory;
			}
		}
		protected BusinessObjectFactory nonPersistentFactory;
	}
}