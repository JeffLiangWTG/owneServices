using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using RefCusCodeListTypesCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IE.Business.Testing
{
	class PreviousDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPackTypeList()
		{
			dataHelper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCodes.UnitedNationsRecommendations);
			dataHelper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypesCodes.UnitedNationsPackageTypes, "United Nations Package Types");
			dataHelper.CreateNewOrGetExistingCusCodeList(RefDataGroupingCodes.UnitedNationsRecommendations, RefCusCodeListTypesCodes.UnitedNationsPackageTypes, "PB", "United Nations Package Type B", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			dataHelper.CreateNewOrGetExistingCusCodeList(RefDataGroupingCodes.UnitedNationsRecommendations, RefCusCodeListTypesCodes.UnitedNationsPackageTypes, "PA", "United Nations Package Type A", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var previousDocument = Factory.New<PreviousDocument>();
			AssertEquals("Count", "PA, PB", previousDocument.Lookups.PackTypeList.CodesAsString);

			var newPreviousDocument = Factory.New<PreviousDocument>();
			AssertSame("Should hav been cached.", previousDocument.Lookups.PackTypeList, newPreviousDocument.Lookups.PackTypeList);
		}

		public void TestUnitOfQuantityList()
		{
			dataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland);
			dataHelper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypesCodes.SupportingDocumentsUnitOfMeasure, "Supporting Documents Unit of Measure");
			dataHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, RefCusCodeListTypesCodes.SupportingDocumentsUnitOfMeasure, "UQB", "Supporting Documents Unit of Measure B", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			dataHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, RefCusCodeListTypesCodes.SupportingDocumentsUnitOfMeasure, "UQA", "Supporting Documents Unit of Measure A", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var previousDocument = Factory.New<PreviousDocument>();
			AssertEquals("Count", "UQA, UQB", previousDocument.Lookups.UnitOfQuantityList.CodesAsString);

			var newPreviousDocument = Factory.New<PreviousDocument>();
			AssertSame("Should hav been cached.", previousDocument.Lookups.UnitOfQuantityList, newPreviousDocument.Lookups.UnitOfQuantityList);
		}

		UniversalReferenceTestDataHelper dataHelper;

		protected override void SetUp()
		{
			base.SetUp();
			dataHelper = new UniversalReferenceTestDataHelper(Factory);
		}
	}
}
