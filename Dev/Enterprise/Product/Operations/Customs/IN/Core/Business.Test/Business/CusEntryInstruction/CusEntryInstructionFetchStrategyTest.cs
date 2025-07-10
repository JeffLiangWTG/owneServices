using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(CusEntryInstructionFetchStrategy))]
sealed class CusEntryInstructionFetchStrategyTest : BusinessObjectFetchStrategyTestCase
{
	protected override IBusinessObjectCollection CreateCollectionToTest(BusinessObjectFactory factory)
	{
		return new CusEntryInstructionCollection<CusEntryInstruction>(factory.New<JobDeclaration>());
	}

	public void TestFetchForLoadChildEditableObjects()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var entryInstruction = Factory.New<CusEntryInstruction>();
		entryInstruction.CEI_JE = declaration.PK;
		var fetchStrategy = new CusEntryInstructionFetchStrategy(entryInstruction);
		fetchStrategy.FetchForLoadChildEditableObjects();
		var tableHits = new ZStringBuilder(Factory.GetAllFetchHintedTableNames().OrderBy(x => x)).ToStringWithNewLineBetweenAppends();
		AssertContains("All Table Hits", "CusEntryNum", tableHits);
	}

	public void TestFetchForValidate()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var entryInstruction = Factory.New<CusEntryInstruction>();
		entryInstruction.CEI_JE = declaration.PK;
		var fetchStrategy = new CusEntryInstructionFetchStrategy(entryInstruction);
		fetchStrategy.FetchForValidate();
		var tableHits = new ZStringBuilder(Factory.GetAllFetchHintedTableNames().OrderBy(x => x)).ToStringWithNewLineBetweenAppends();
		AssertContains("All Table Hits", "CusEntryNum", tableHits);
	}
}
