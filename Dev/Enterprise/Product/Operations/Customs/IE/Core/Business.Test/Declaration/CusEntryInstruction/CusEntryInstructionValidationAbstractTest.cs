using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	abstract class CusEntryInstructionValidationAbstractTest<T> : BusinessObjectValidationTestCase where T : CusEntryInstructionValidation
	{
		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = MessageType;
			instruction = jobDeclaration.CustomsEntryInstructions[0];
			validation = GetValidation();
		}
		protected JobDeclaration jobDeclaration;
		protected CusEntryInstruction instruction;
		protected T validation;

		protected abstract string MessageType { get; }

		protected abstract T GetValidation();
	}
}
