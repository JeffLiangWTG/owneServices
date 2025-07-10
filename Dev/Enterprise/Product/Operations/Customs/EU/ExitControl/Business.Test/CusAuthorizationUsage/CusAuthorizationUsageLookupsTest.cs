using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing;

sealed class CusAuthorizationUsageLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCodeListForExitReport()
	{
		const string testCountryCode = "XX";
		var expectedCodeList = new CodeDescriptionPairList();

		var cusExitHeader = Factory.New<CusExitHeader>();
		cusExitHeader.Company.GC_RN_NKCountryCode = testCountryCode;
		var authorizationUsage = cusExitHeader
			.CusExitReports.AddNew()
			.CusAuthorizationUsages.AddNew();

		var providerMock = new Mock<CusAuthorisationHeaderProvider>(ZString.Empty);
		providerMock.Protected().Setup<CodeDescriptionPairList>("GetAuthorisationTypeListCore", ItExpr.IsAny<BusinessObjectFactory>()).Returns(expectedCodeList);
		var objectHandelMock = Mock.Of<ObjectHandle>(m => m.GetObject(It.IsAny<object[]>()) == providerMock.Object);
		var providerMapping = new Hashtable { { testCountryCode, objectHandelMock } };
		using var substituteProvidersMapping = ObjectFactory.Substitute("CusAuthorisationHeaderProviders", providerMapping);
		AssertEquals(expectedCodeList, authorizationUsage.Lookups.CodeList);
	}

	public void TestCodeListForExitReportItem()
	{
		var authorizationUsage = Factory.New<CusExitHeader>()
			.CusExitReports.AddNew()
			.CusExitReportItems.AddNew()
			.CusAuthorizationUsages.AddNew();
		var codeList = (CodeDescriptionPairList)authorizationUsage.Lookups.CodeList;
		var expectedCodes = new ZString[] { "BTI", "BOI" };
		AssertContainsExactElementsInAnyOrder(expectedCodes, codeList.GetAllCodes());
	}
}
