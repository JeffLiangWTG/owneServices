using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(NEXDOCCategoryCodesCollection))]
	sealed class NEXDOCCategoryCodesCollectionTest : EXDOCRefCodeCollectionTest<NEXDOCCategoryCodesCollection>
	{
		public void TestCreateCategoryCollection()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType("NPRCD", "Dairy");
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "NPRCD", "BUT", "Butter", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "NPRCD", "ICE", "Ice Cream", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			var header = helper.Header1.QuarantineExDocHeader;

			var categoryCodesCollection = new NEXDOCCategoryCodesCollection(header, "");
			header.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			categoryCodesCollection.Load();
			AssertEquals("Collection has only the Dairy codes", 2, categoryCodesCollection.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			helper.Header1.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			return new NEXDOCCategoryCodesCollection(helper.Header1.QuarantineExDocHeader, ZString.Empty);
		}

		protected override BusinessObjectCollection GetCollectionWithNullTypeProviderToTest() => new NEXDOCCategoryCodesCollection(null, ZString.Empty);
	}
}
