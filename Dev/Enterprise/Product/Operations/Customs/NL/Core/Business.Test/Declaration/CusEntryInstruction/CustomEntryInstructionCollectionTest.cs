using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(CusEntryInstructionCollection))]
class CustomEntryInstructionCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestDefaultValuesForNewChild()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var collection = new CusEntryInstructionCollection(declaration);
		foreach (var existingInstruction in declaration.CustomsEntryInstructions.ToList())
		{
			declaration.CustomsEntryInstructions.Delete(existingInstruction);
		}

		var instruction = collection.AddNew();

		AssertEquals("Default style", "B1", instruction.CEI_Style);
		AssertEquals("Default substyle", "A", instruction.CEI_SubStyle);
		AssertEquals("Default procedure", "10", instruction.CEI_Procedure);
		AssertEquals("Default transaction nature", "11", instruction.ZG_TransNature);
	}

	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var declaration = Factory.New<JobDeclaration>();
		return new CusEntryInstructionCollection(declaration);
	}
}
