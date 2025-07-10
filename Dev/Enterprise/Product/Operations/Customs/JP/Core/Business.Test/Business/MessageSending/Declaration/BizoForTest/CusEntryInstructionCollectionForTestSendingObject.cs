namespace Enterprise.Customs.JP.Business.Testing
{
	public class CusEntryInstructionCollectionForTestSendingObject : CusEntryInstructionCollection
	{
		public CusEntryInstructionCollectionForTestSendingObject(DeclarationForTestSendingObject master) : base(master)
		{
		}

		public new CusEntryInstructionForTestSendingObject this[int index]
		{
			get { return (CusEntryInstructionForTestSendingObject)Elements[index]; }
		}

		public new CusEntryInstructionForTestSendingObject AddNew()
		{
			return (CusEntryInstructionForTestSendingObject)base.AddNew();
		}
	}
}
