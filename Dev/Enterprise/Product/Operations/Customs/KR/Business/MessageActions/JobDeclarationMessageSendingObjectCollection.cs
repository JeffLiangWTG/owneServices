using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class JobDeclarationMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<JobDeclarationMessageSendingObject>
	{
		public JobDeclarationMessageSendingObjectCollection(JobDeclaration declaration, string messageType)
			: base(declaration.Factory)
		{
			this.declaration = declaration;
			this.messageType = messageType;
			PopulateElements();
		}

		readonly JobDeclaration declaration;
		readonly string messageType;
		void PopulateElements()
		{
			RemoveAll();
			foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
			{
				Add(new JobDeclarationMessageSendingObject(entry, messageType));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotImplementedException();
		protected override bool AllowNewCore => false;
	}
}
