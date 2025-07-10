using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class DuimpMessageSendingObjectParent : JobDeclarationMessageSendingObjectParent<DeclarationMessageSendingObject>, IMessageSendingObjectParent
	{
		public DuimpMessageSendingObjectParent(JobDeclaration declaration) : base(declaration)
		{
		}

		public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		protected override JobDeclarationMessageSendingObject CreateNewJobDeclarationMessageSendingObject(Customs.Business.CusEntryHeader header)
		{
			return new DuimpMessageSendingObject((CusEntryHeader)header);
		}

		protected override NonPersistentBusinessObjectCollection<DeclarationMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			var jobDeclarationMessageSendingObjectCollection = new JobDeclarationMessageSendingObjectCollection<DeclarationMessageSendingObject>(Factory);
			foreach (var item in ParentDeclaration.ActiveEntryHeaders.FormalEntries)
			{
				jobDeclarationMessageSendingObjectCollection.Add(CreateNewJobDeclarationMessageSendingObject(item));
			}

			return jobDeclarationMessageSendingObjectCollection;
		}

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => columnDefinitions;

		readonly IEnumerable<MessageSendingObjectProperty> columnDefinitions = new MessageSendingObjectProperty[]
		{
			new MessageSendingObjectProperty(nameof(DuimpMessageSendingObject.MessageType), true, 100),
			new MessageSendingObjectProperty(nameof(DuimpMessageSendingObject.MovementReferenceNumber), true, 140, Res.GetData("403397D4-8083-43E3-A0E9-8231740A0127", "Entry Number")),
			new MessageSendingObjectProperty(nameof(DuimpMessageSendingObject.SubmittedDate), true, 140),
			new MessageSendingObjectProperty(nameof(DuimpMessageSendingObject.CustomsStatus), true, 100),
			new MessageSendingObjectProperty(nameof(DuimpMessageSendingObject.MessageStatusDescription), false, 180),
		};
	}
}
