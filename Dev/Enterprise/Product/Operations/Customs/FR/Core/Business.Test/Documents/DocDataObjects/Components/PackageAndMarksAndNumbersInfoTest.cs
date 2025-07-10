using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FR.Business.Documents.Testing
{
	class PackageAndMarksAndNumbersInfoTest : TestCaseWithFactory
	{
		public void TestGetPackageType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage(CountryCodes.France, "French");
			helper.CreateCusCodeType(Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "Packages");
			var code = helper.CreateCusCodeList(Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "CT", "CARTON", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateCusCodeListLanguage(code, CountryCodes.France, "Carton");
			Factory.Save();

			var package = Factory.New<BasePackage>();
			package.CW_PackType = "CT";

			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals("FR package type should be the package description if available.", "Carton", new PackageAndMarksAndNumbersInfoForTest(invoiceLine).GetPackageType(package));

			package.CW_PackType = "XX";
			AssertEquals("FR package type should be the package type if description is not available.", "XX", new PackageAndMarksAndNumbersInfoForTest(invoiceLine).GetPackageType(package));
		}
	}

	public class PackageAndMarksAndNumbersInfoForTest : PackageAndMarksAndNumbersInfo
	{
		public PackageAndMarksAndNumbersInfoForTest(JobComInvoiceLine invoiceLine) : base(invoiceLine)
		{
		}

		public new string GetPackageType(BasePackage package) => base.GetPackageType(package);
	}
}
