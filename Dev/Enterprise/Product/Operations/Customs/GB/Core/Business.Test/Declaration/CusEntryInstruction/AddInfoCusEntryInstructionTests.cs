using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(AddInfoCusEntryInstruction))]
	public class AddInfoCusEntryInstructionBOTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			CusEntryInstruction entryInstruction = Factory.New<CusEntryInstruction>();
			return new AddInfoCusEntryInstruction(entryInstruction.CEI_AddInfoInfo);
		}
	}
}
