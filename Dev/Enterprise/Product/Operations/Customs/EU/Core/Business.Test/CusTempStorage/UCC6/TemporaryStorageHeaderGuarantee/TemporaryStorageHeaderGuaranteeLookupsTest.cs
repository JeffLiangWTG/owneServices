using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	internal class TemporaryStorageHeaderGuaranteeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestGuaranteeTypeFilter()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";

			var guaranteeHeader1 = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader1.CPH_Type = "TRA";
			guaranteeHeader1.CPH_Number = "EU1";
			guaranteeHeader1.CPH_RN_NKCountryCode = "ES";
			guaranteeHeader1.CPH_StartDate = ZDate.Today;
			guaranteeHeader1.CPH_OH_PermitHolder = orgHeader.PK;

			var guaranteeHeader2 = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader2.CPH_Type = "COD";
			guaranteeHeader2.CPH_Number = "EU2";
			guaranteeHeader2.CPH_RN_NKCountryCode = "ES";
			guaranteeHeader2.CPH_StartDate = ZDate.Today;
			guaranteeHeader2.CPH_OH_PermitHolder = orgHeader.PK;

			var guaranteeHeader5 = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader5.CPH_Type = "IMP";
			guaranteeHeader5.CPH_Number = "EU3";
			guaranteeHeader5.CPH_RN_NKCountryCode = "ES";
			guaranteeHeader5.CPH_StartDate = ZDate.Today;
			guaranteeHeader5.CPH_OH_PermitHolder = orgHeader.PK;
			Factory.Save();

			var header = Factory.New<TemporaryStorageHeader>();
			var guarantee = header.Guarantee;

			var referenceNumbers = guarantee.Lookups.ReferenceNumbers;
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Collection elements", new[] { "EU2" }, referenceNumbers.Select(x => x.CPH_Number));
				AssertEquals("TRA Should NOT match filter", false, Factory.Load<CusGuaranteeHeader>(guaranteeHeader1.PK).MatchesFilter(referenceNumbers.CompleteFilter));
				AssertEquals("COD should match filter", true, Factory.Load<CusGuaranteeHeader>(guaranteeHeader2.PK).MatchesFilter(referenceNumbers.CompleteFilter));
				AssertEquals("IMP should NOT match filter", false, Factory.Load<CusGuaranteeHeader>(guaranteeHeader5.PK).MatchesFilter(referenceNumbers.CompleteFilter));
			});
		}
	}
}
