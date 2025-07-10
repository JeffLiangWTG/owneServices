using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.BE.Business.Testing;

class AlternativeEvidenceLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestDocTypeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ExportTransportDocument, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ExportTransportDocument + " DESC");
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Belgium, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ExportTransportDocument, "01", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Belgium, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ExportTransportDocument, "02", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		Factory.Save();

		var list = (ZZRefCusCodeListCombinedCollection)lookups.TransportDocumentTypeList;
		list.Load();
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("Codes", new[] { "01", "02" }, list.Select(x => x.ZZD_Code));
			AssertSame("Cached", list, lookups.TransportDocumentTypeList);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		var alternativeEvidence = new AlternativeEvidence(Factory);
		lookups = new AlternativeEvidenceLookups(alternativeEvidence);
	}
	AlternativeEvidenceLookups lookups;
}
