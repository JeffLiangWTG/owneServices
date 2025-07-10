using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Business.Testing;

[TestedType(typeof(CusTempStorageRegHeaderStatusListProvider))]
sealed class CusTempStorageRegHeaderStatusListProviderTest : TestCaseWithFactory
{
	public void TestGetCodeDescriptionPairList()
	{
		var provider = new CusTempStorageRegHeaderStatusListProvider();
		var actualCodes = provider.GetCodeDescriptionPairList().GetAllCodes();
		AssertContainsExactElementsInAnyOrder(new ZString[] { "OPN", "CLS", "DEL", "FIN", "LCK", "PAC", "PRE", "TST" }, actualCodes.AsEnumerable());
	}

	public void TestGetDependenceCodeDescriptionPairList_IST()
	{
		var provider = new CusTempStorageRegHeaderStatusListProvider();
		var actualCodes = provider.GetDependenceCodeDescriptionPairList(TemporaryStorageApplicationCodesList.Codes.IST).GetAllCodes();
		AssertContainsExactElementsInAnyOrder(new ZString[] { "OPN", "CLS" }, actualCodes.AsEnumerable());
	}

	public void TestGetDependenceCodeDescriptionPairList_SUM()
	{
		var provider = new CusTempStorageRegHeaderStatusListProvider();
		var actualCodes = provider.GetDependenceCodeDescriptionPairList(TemporaryStorageApplicationCodesList.Codes.SUM).GetAllCodes();
		AssertContainsExactElementsInAnyOrder(new ZString[] { "DEL", "FIN", "LCK", "PAC", "PRE", "TST", "NCM" }, actualCodes.AsEnumerable());
	}

	public void TestGetDependenceCodeDescriptionPairList_Empty()
	{
		var provider = new CusTempStorageRegHeaderStatusListProvider();
		var actualCodes = provider.GetDependenceCodeDescriptionPairList("").GetAllCodes();
		AssertContainsExactElementsInAnyOrder(new ZString[] { "OPN", "CLS", "DEL", "FIN", "LCK", "PAC", "PRE", "TST" }, actualCodes.AsEnumerable());
	}

	public void TestGetDependenceCodeDescriptionPairList_EU()
	{
		var provider = new CusTempStorageRegHeaderStatusListProvider();
		var actualCodes = provider.GetDependenceCodeDescriptionPairList("INV").GetAllCodes();
		AssertContainsExactElementsInAnyOrder(System.Array.Empty<ZString>(), actualCodes.AsEnumerable());
	}
}
