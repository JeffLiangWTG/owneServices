using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class EarlyReleaseMiscMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<EarlyReleaseMiscMessageSendingObject>
	{
		public EarlyReleaseMiscMessageSendingObjectCollection(JobDeclaration declaration)
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
				Add(new EarlyReleaseMiscMessageSendingObject(entry));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new EarlyReleaseMiscMessageSendingObject(Factory);
		protected override bool AllowNewCore => false;
	}
}
