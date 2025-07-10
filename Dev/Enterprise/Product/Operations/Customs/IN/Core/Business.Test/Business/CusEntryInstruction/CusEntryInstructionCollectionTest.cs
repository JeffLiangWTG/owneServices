using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(CusEntryInstructionCollection))]
sealed class CusEntryInstructionCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestChildType()
	{
		AssertType<CusEntryInstruction>(Collection.AddNew());
	}

	public void TestDefaultCEI_Style()
	{
		var collection = (CusEntryInstructionCollection)Collection;
		collection.Master.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals(DeclarationTypeList.Codes.ForeignExchangeInvolved, collection.AddNew().CEI_Style);

		collection.Master.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals(ZString.Empty, collection.AddNew().CEI_Style);
	}

	public void TestDefaultCEI_WeightUQ()
	{
		var collection = (CusEntryInstructionCollection)Collection;
		collection.Master.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals(Core.Constants.Weight.Kilograms, collection.AddNew().CEI_WeightUQ);

		collection.Master.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals(ZString.Empty, collection.AddNew().CEI_WeightUQ);
	}

	protected override BusinessObjectCollection GetCollectionToTest() => new CusEntryInstructionCollection(Factory.New<JobDeclaration>());
}
