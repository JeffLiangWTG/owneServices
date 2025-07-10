using Enterprise.Customs.EU.Business.Declaration.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(GuaranteeForEntryInstructionCollection))]
sealed class GuaranteeForEntryInstructionCollectionTest : GuaranteeForEntryInstructionCollectionAbstractTest<GuaranteeForEntryInstructionCollection>
{
	public void TestAddNewReturnedType()
	{
		var guaranteeCollection = GetNewGuaranteeCollection();
		AssertType<GuaranteeForEntryInstruction>("AddNew() returned type", guaranteeCollection.AddNew());
	}

	public void TestIndexerReturnedType()
	{
		var guaranteeCollection = GetNewGuaranteeCollection();
		guaranteeCollection.AddNew();
		AssertType<GuaranteeForEntryInstruction>("Indexer returned type", guaranteeCollection[0]);
	}

	protected override GuaranteeForEntryInstructionCollection GetNewGuaranteeCollection() => Factory.New<CusEntryInstruction>().Guarantees;
}
