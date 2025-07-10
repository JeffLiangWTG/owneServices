using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(SealNumberCollection))]
	class SealNumberCollectionTest : CusCodeDataCollectionTest<SealNumber>
	{
		protected override CusCodeDataCollection<SealNumber> GetCusCodeDataCollection() => new SealNumberCollection(Instruction);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<SealNumber>();
			result.CY_ParentID = Instruction.PK;
			result.CY_ParentTableCode = Instruction.TablePrefix;
			return result;
		}

		CusEntryInstruction Instruction => instruction ?? (instruction = Factory.New<CusEntryInstruction>());
		CusEntryInstruction instruction;
	}
}
