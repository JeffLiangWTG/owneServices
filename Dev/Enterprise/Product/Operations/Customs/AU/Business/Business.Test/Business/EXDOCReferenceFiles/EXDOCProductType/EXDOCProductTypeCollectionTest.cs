using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(EXDOCProductTypeCollection))]
	sealed class EXDOCProductTypeCollectionTest : EXDOCRefCodeCollectionTest<EXDOCProductTypeCollection>
	{
		public void TestCreateRelationshipFilter()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType("PRODD", "Dairy");
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "PRODD", "BUT", "Butter", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateNewOrGetExistingCusCodeType("PRODE", "Egg");
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "PRODE", "BUT", "Butter", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateNewOrGetExistingCusCodeType("PRODW", "Wool");
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "PRODW", "BUT", "Butter", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "PRODW", "BOX", "Butter", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			var quarantineHeader = helper.Header1.QuarantineExDocHeader;

			var productTypeCodeCollection = new EXDOCProductTypeCollection(quarantineHeader);
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Wool;
			productTypeCodeCollection.Load();
			AssertEquals("Collection is only the Wool codes", 2, productTypeCodeCollection.Count);

			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
			productTypeCodeCollection.Load();
			AssertEquals("Collection is only the Egg codes", 1, productTypeCodeCollection.Count);

			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			productTypeCodeCollection.Load();
			AssertEquals("Collection is only the Meat codes", 0, productTypeCodeCollection.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			helper.Header1.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			return new EXDOCProductTypeCollection(helper.Header1.QuarantineExDocHeader);
		}
	}
}
