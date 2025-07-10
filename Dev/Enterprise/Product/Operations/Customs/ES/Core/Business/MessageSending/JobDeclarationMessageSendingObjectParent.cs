using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageSending
{
	public class JobDeclarationMessageSendingObjectParent : Customs.Business.JobDeclarationMessageSendingObjectParent<JobDeclarationMessageSendingObject>
	{
		public JobDeclarationMessageSendingObjectParent(MessageSendingObject sendingObject, bool isAutoSend = false) : base(sendingObject.Declaration)
		{
			this.sendingObject = sendingObject;
			this.isAutoSend = isAutoSend;
		}
		readonly MessageSendingObject sendingObject;
		readonly bool isAutoSend;
		public ICertificateProvider CertificateData => sendingObject;

		public ZBool ShouldEditMessage => sendingObject.ShouldEditMessage;

		public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		protected override Customs.Business.JobDeclarationMessageSendingObject CreateNewJobDeclarationMessageSendingObject(Customs.Business.CusEntryHeader header)
			=> new JobDeclarationMessageSendingObject((CusEntryHeader)header, isAutoSend);

		protected override NonPersistentBusinessObjectCollection<JobDeclarationMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			var result = new Customs.Business.JobDeclarationMessageSendingObjectCollection<JobDeclarationMessageSendingObject>(Factory);
			foreach (CusEntryHeader header in ParentDeclaration.ActiveEntryHeaders)
			{
				var entryInstruction = header?.EntryInstruction?.CEI_SubStyle ?? ZString.Empty;

				if (!entryInstruction.IsEmpty)
				{
					result.Add(CreateNewJobDeclarationMessageSendingObject(header));
				}
			}
			return result;
		}

		public IEnumerable<JobDeclarationMessageSendingObject> ObjectsToSend
			=> SendingObjectsCollection.OfType<JobDeclarationMessageSendingObject>().Where(x => x.ShouldSend);
	}
}
