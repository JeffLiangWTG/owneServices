using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class ValuationDeclarationMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<ValuationDeclarationMessageSendingObject>
	{
		public ValuationDeclarationMessageSendingObjectCollection(JobDeclaration declaration)
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
				Add(new ValuationDeclarationMessageSendingObject(entry));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new System.NotImplementedException();
		protected override bool AllowNewCore => false;
	}
}
