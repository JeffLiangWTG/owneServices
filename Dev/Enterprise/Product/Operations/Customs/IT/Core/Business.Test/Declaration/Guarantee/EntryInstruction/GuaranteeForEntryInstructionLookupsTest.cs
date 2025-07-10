using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class GuaranteeForEntryInstructionLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestBondTypesList()
	{
		var guarantee = Factory.New<GuaranteeForEntryInstruction>();
		AssertType<ImportGuaranteeSubTypeList>("BondTypeList Type", guarantee.Lookups.BondTypeList);
	}
}
