using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

abstract class CusEntryInstructionLookupsAbstractTest<T> : BusinessObjectLookupsTestCase where T : CusEntryInstructionLookups
{
	protected override void SetUp()
	{
		base.SetUp();
		jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = MessageType;
		instruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		lookups = GetLookups();
	}
	protected JobDeclaration jobDeclaration;
	protected CusEntryInstruction instruction;
	protected T lookups;

	protected abstract string MessageType { get; }
	protected abstract T GetLookups();
}
