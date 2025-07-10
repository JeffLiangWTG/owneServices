using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestsSubclassesOf(typeof(TemporaryStorageSupportingDocumentConfiguration))]
	public abstract class TemporaryStorageSupportingDocumentConfigurationAbstractTest<T> : TestCaseWithFactory where T : TemporaryStorageSupportingDocumentConfiguration, new()
	{
		public abstract void TestGetCodeList();

		protected override void SetUp()
		{
			base.SetUp();
			configuration = new T();
		}
		protected T configuration;
	}

	[TestedType(typeof(TemporaryStorageSupportingDocumentConfiguration))]
	sealed class TemporaryStorageSupportingDocumentConfigurationTest : TemporaryStorageSupportingDocumentConfigurationAbstractTest<TemporaryStorageSupportingDocumentConfiguration>
	{
		[ExpectNoExceptions]
		public override void TestGetCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);

			var codeLists1 = helper.CreateNewOrGetExistingCusCodeList(
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentsTemporaryStorageUCC6,
				"AAAA",
				"ENS",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			var codeLists2 = helper.CreateNewOrGetExistingCusCodeList(
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentsTemporaryStorageUCC6,
				"BBBB",
				"Transit",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			var codeLists3 = helper.CreateNewOrGetExistingCusCodeList(
				Core.Constants.CountryCodes.Latvia,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentsTemporaryStorageUCC6,
				"CCCC",
				"Transit",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			var codeLists4 = helper.CreateNewOrGetExistingCusCodeList(
				Core.Constants.CountryCodes.Germany,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentsTemporaryStorageUCC6,
				"DDDD",
				"Transit",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			NUnit.Framework.Assert.That(storageHeader.AMA_RN_NKCountry, NUnit.Framework.Is.EqualTo(Core.Constants.CountryCodes.Latvia).Using(CustomComparers.TypeComparison), "AMA_RN_NKCountry must be LV");
			var bill = storageHeader.Bills.AddNew();
			var supportingDocument = bill.SupportingDocuments.AddNew();
			var collection = configuration.GetCodeList(supportingDocument) as ZZRefCusCodeListCombinedCollection;

			var completeFilter = collection.CompleteFilter;
			NUnit.Framework.Assert.That(Factory.Load<ZZRefCusCodeListCombined>(codeLists1.PK).MatchesFilter(completeFilter), NUnit.Framework.Is.EqualTo(true), "Matched Purpose: Presentation");
			NUnit.Framework.Assert.That(Factory.Load<ZZRefCusCodeListCombined>(codeLists2.PK).MatchesFilter(completeFilter), NUnit.Framework.Is.EqualTo(true), "Matched Purpose: Presentation");
			NUnit.Framework.Assert.That(Factory.Load<ZZRefCusCodeListCombined>(codeLists3.PK).MatchesFilter(completeFilter), NUnit.Framework.Is.EqualTo(true), "Matched Purpose: Presentation");
			NUnit.Framework.Assert.That(Factory.Load<ZZRefCusCodeListCombined>(codeLists4.PK).MatchesFilter(completeFilter), NUnit.Framework.Is.EqualTo(false), "Unmatched Purpose: Presentation");
			NUnit.Framework.Assert.That((ZZRefCusCodeListCombinedCollection)supportingDocument.Lookups.CodeList, NUnit.Framework.Is.SameAs(collection), "Cached");
		}
	}
}
