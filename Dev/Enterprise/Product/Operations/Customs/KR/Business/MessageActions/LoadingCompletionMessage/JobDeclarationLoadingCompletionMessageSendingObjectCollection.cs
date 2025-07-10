using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class JobDeclarationLoadingCompletionMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<JobDeclarationLoadingCompletionMessageSendingObject>
	{
		public JobDeclarationLoadingCompletionMessageSendingObjectCollection(JobDeclaration declaration, string messageType)
			: base(declaration.Factory)
		{
			this.declaration = declaration;
			PopulateElements(messageType);
		}
		readonly JobDeclaration declaration;

		void PopulateElements(string messageType)
		{
			RemoveAll();
			foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
			{
				Add(new JobDeclarationLoadingCompletionMessageSendingObject(entry, messageType));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotImplementedException();
		protected override bool AllowNewCore => false;
	}
}
