using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.JP.Business.Testing
{
	public class CusEntryInstructionForTestSendingObject : CusEntryInstruction, IObsoleteValidation
	{
		public CusEntryInstructionForTestSendingObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool CreateMessageErrorForTest { get; set; }

		public bool CreateErrorForTest { get; set; }

		public bool CreateWarningForTest { get; set; }

		protected override void RunPreSaveValidationCore() => Validation.ValidateAll();

		public new CusEntryInstructionValidationForTestSendingObject Validation => (CusEntryInstructionValidationForTestSendingObject)base.Validation;

		protected override Customs.Business.CusEntryInstructionValidation GetNewValidation()
		{
			return new CusEntryInstructionValidationForTestSendingObject(this);
		}
	}

	public class EntryInstructionProviderForTestSendingObject : EntryInstructionProvider
	{
		public EntryInstructionProviderForTestSendingObject(DeclarationForTestSendingObject declaration) : base(declaration) { }

		protected new DeclarationForTestSendingObject ParentDeclaration => base.ParentDeclaration as DeclarationForTestSendingObject;

		protected override ICusEntryInstructionCollection<Customs.Business.CusEntryInstruction> GetNewCusEntryInstructionCollectionCore()
		{
			return new CusEntryInstructionCollectionForTestSendingObject(ParentDeclaration);
		}
	}
}
