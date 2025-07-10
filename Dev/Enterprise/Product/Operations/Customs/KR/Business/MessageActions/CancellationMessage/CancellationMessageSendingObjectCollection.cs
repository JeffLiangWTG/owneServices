using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class CancellationMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<CancellationMessageSendingObject>
	{
		public CancellationMessageSendingObjectCollection(JobDeclaration declaration)
			: base(declaration.Factory)
		{
			this.declaration = declaration;
			PopulateElements();
		}
		readonly JobDeclaration declaration;

		void PopulateElements()
		{
			RemoveAll();
			foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
			{
				Add(new CancellationMessageSendingObject(entry));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotImplementedException();
		protected override bool AllowNewCore => false;
	}
}
