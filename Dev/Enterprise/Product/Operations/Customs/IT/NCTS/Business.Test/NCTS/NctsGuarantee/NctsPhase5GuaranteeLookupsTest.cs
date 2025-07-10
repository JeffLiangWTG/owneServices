using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsPhase5GuaranteeLookupsTest : TestCaseWithFactory
{
	public void TestOfficeCodeList()
	{
		CustomsOfficeTestDataHelper.SetupTraderGroups(Factory);
		CustomsOfficeTestDataHelper.SetupCustomsOfficeData(Factory);
		Factory.Save();

		var officeCodeList = nctsGuarantee.Lookups.OfficeCodeList;
		officeCodeList.Load();
		var expectedOfficeCodes = new ZString[] { "IT303199", "TR44556", "GB42556" };
		AssertContainsExactElementsInAnyOrder(expectedOfficeCodes, officeCodeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
	}

	protected override void SetUp()
	{
		base.SetUp();

		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		nctsGuarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
	}

	NctsGuarantee nctsGuarantee;
}
