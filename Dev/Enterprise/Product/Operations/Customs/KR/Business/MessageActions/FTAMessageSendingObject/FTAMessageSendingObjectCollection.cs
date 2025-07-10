using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	public class FTAMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<FTAMessageSendingObject>
	{
		public FTAMessageSendingObjectCollection(JobDeclaration declaration, ZString messageType)
			: base(declaration.Factory)
		{
			this.declaration = declaration;
			PopulateElements(messageType);
		}
		readonly JobDeclaration declaration;

		void PopulateElements(ZString messageType)
		{
			RemoveAll();
			foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
			{
				Add(new FTAMessageSendingObject(entry, messageType));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotImplementedException();
		protected override bool AllowNewCore => false;
	}
}
