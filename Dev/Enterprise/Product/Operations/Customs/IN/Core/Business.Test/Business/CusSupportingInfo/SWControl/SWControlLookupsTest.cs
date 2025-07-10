using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(SWControlLookups))]
sealed class SWControlLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestControlResultCodeList()
	{
		RefDataSetupTestHelper.SetupControlResultCode(Factory);
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var swControl = instruction.SWControls.AddNew();
		var controlResultCodeList = swControl.Lookups.ControlResultCodeList as BusinessObjectCollection;
		controlResultCodeList.Load();
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder(new[] { "IN00121", "IN00122" }, controlResultCodeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
			AssertSame("Cached", controlResultCodeList, swControl.Lookups.ControlResultCodeList);
		});
	}
}
