using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class AgreedRateMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<AgreedRateMessageSendingObject>
	{
		public AgreedRateMessageSendingObjectCollection(JobDeclaration declaration)
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
				Add(new AgreedRateMessageSendingObject(entry));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new AgreedRateMessageSendingObject(Factory);
		protected override bool AllowNewCore => false;
	}
}
