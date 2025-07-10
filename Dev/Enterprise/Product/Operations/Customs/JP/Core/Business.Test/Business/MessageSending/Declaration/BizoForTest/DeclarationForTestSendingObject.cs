using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.JP.Business.Testing
{
	public class DeclarationForTestSendingObject : JobDeclaration
	{
		public DeclarationForTestSendingObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool CreateMessageErrorForTest { get; set; }

		public bool CreateErrorForTest { get; set; }

		public bool CreateWarningForTest { get; set; }

		protected override void RunPreSaveValidationCore() => Validation.ValidateAll();

		public new DeclarationValidationForTestSendingObject Validation => GetNewValidation();

		protected new DeclarationValidationForTestSendingObject GetNewValidation()
		{
			return new DeclarationValidationForTestSendingObject(this);
		}

		public new CusEntryInstructionCollectionForTestSendingObject CustomsEntryInstructions => (CusEntryInstructionCollectionForTestSendingObject)base.CustomsEntryInstructions;

		protected override Customs.Business.EntryInstructionProvider GetCustomsEntryInstructionProviderCore()
		{
			return new EntryInstructionProviderForTestSendingObject(this);
		}
	}
}
