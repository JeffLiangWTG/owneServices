using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Business.Testing;

[TestedType(typeof(CusTempStorageRegHeaderAppCodesListProvider))]
sealed class CusTempStorageRegHeaderAppCodesListProviderTest : TestCaseWithFactory
{
	public void TestGetCodeDescriptionPairList()
	{
		var provider = new CusTempStorageRegHeaderAppCodesListProvider();
		var actualCodes = provider.GetCodeDescriptionPairList().GetAllCodes();
		AssertContainsExactElementsInAnyOrder(new ZString[] { "IST", "SUM" }, actualCodes.AsEnumerable());
	}
}
