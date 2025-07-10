using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class JobDeclarationAmendmentMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<JobDeclarationAmendmentMessageSendingObject>
	{
		public JobDeclarationAmendmentMessageSendingObjectCollection(JobDeclaration declaration, string messageType)
			: base(declaration.Factory)
		{
			this.declaration = declaration;
			this.messageType = messageType;
			PopulateElements(messageType);
		}
		readonly JobDeclaration declaration;
		readonly string messageType;

		void PopulateElements(string messageType)
		{
			RemoveAll();
			foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
			{
				Add(new JobDeclarationAmendmentMessageSendingObject(entry, messageType));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new JobDeclarationAmendmentMessageSendingObject(Factory, messageType);
		protected override bool AllowNewCore => false;
	}
}
