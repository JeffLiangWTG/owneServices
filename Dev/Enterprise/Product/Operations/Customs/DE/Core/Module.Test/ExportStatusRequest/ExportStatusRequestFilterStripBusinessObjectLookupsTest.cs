using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Module.Testing
{
	public class ExportStatusRequestFilterStripBusinessObjectLookupsTest : TestCaseWithFactory
	{
		public void TestModuleCodeList()
		{
			CombineAssertions(() =>
			{
				var moduleCodeList = lookups.ModuleCodeList;
				AssertEquals("ModuleCodeList of correct type", "AES, NCTS", moduleCodeList.CodesAsString);
				AssertSame("Cached", moduleCodeList, lookups.ModuleCodeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			lookups = new ExportStatusRequestFilterStripBusinessObjectLookups(new ExportStatusRequestFilterBusinessObject());
		}
		ExportStatusRequestFilterStripBusinessObjectLookups lookups;
	}
}
