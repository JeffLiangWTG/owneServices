using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;
[TestedType(typeof(INOrgImpAddInfoLookups))]
sealed class INOrgImpAddInfoLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestExporterTypeList()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var info = INOrgImpAddInfo.Get(orgHeader);
		var cachedList = Factory.GetCachedValue<ExporterTypeList>();
		var lookedUpList = (ExporterTypeList)info.Lookups.ExporterTypeList;
		CombineAssertions(() =>
		{
			AssertSame("ExporterTypeList is cached", cachedList, lookedUpList);
			AssertEquals("ExporterTypeList values", "R, F", lookedUpList.CodesAsString);
		});
	}

	public void TestImporterTypeList()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var info = INOrgImpAddInfo.Get(orgHeader);
		var cachedList = Factory.GetCachedValue<ImporterTypeList>();
		var lookedUpList = (ImporterTypeList)info.Lookups.ImporterTypeList;
		CombineAssertions(() =>
		{
			AssertSame("ImporterTypeList is cached", cachedList, lookedUpList);
			AssertEquals("ImporterTypeList values", "G, U, O, P", lookedUpList.CodesAsString);
		});
	}
}
