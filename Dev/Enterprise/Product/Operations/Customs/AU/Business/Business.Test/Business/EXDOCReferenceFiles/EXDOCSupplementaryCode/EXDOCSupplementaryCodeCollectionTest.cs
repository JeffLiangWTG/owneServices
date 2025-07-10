using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(EXDOCSupplementaryCodeCollection))]
	sealed class EXDOCSupplementaryCodeCollectionTest : EXDOCRefCodeCollectionTest<EXDOCSupplementaryCodeCollection>
	{
		public void TestCreateRelationshipFilter()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType("SUPP", "SUPP");
			var beef = refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "SUPP", "G", "GRAIN FED", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateCusCodeListAttribute(beef.PK, "IsMeat", "");
			var trout = refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "SUPP", "WO", "WILD ORIGIN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateCusCodeListAttribute(trout.PK, "IsFish", "");
			var wheat = refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "SUPP", "GS", "GRAINS & SEEDS FOR SOWING", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateCusCodeListAttribute(wheat.PK, "IsGrain", "");
			Factory.Save();

			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			var header = helper.Header1.QuarantineExDocHeader;
			var suppCodeCollection = new EXDOCSupplementaryCodeCollection(helper.Declaration.Invoices[0].JobComInvoiceLines[0].QuarantineExDocLine.QuarantineExDocHeader);
			header.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			suppCodeCollection.Load();
			AssertEquals("Collection Will not only include the Meat codes", true, suppCodeCollection.Count > 1);
			header.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			suppCodeCollection.Load();
			AssertEquals("Collection Will not only include the Fish codes", true, suppCodeCollection.Count > 1);
		}
	}
}
