using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(EXDOCCutCodeCollection))]
	sealed class EXDOCCutCodeCollectionTest : EXDOCRefCodeCollectionTest<EXDOCCutCodeCollection>
	{
		public void TestCreateRelationshipFilter()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType("CUTCW", "Wool");
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "CUTCW", "W00401", "GREASY WOOL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "CUTCW", "W00408", "GOAT HAIR", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateNewOrGetExistingCusCodeType("CUTCE", "Egg");
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "CUTCE", "BUT", "Butter", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateNewOrGetExistingCusCodeType("CUTCM", "Meat");
			var veal = refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "CUTCM", "3398", "BONLESS VEAL ROSTBIFF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateNewOrGetExistingCusCodeListAttribute(veal.PK, "BoneInIndicator", "O");
			refHelper.CreateNewOrGetExistingCusCodeListAttribute(veal.PK, "IsBeefVeal", "Y");
			refHelper.CreateNewOrGetExistingCusCodeListAttribute(veal.PK, "IsChemicalLean", "N");

			Factory.Save();

			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			var header = helper.Header1.QuarantineExDocHeader;

			header.QH_ProduceType = EXDOCCommodityCodes.Codes.Wool;
			var cutCodeCollection = new EXDOCCutCodeCollection(helper.Declaration.Invoices[0].JobComInvoiceLines[0].QuarantineExDocLine.QuarantineExDocHeader);
			cutCodeCollection.Load();
			AssertEquals("Collection is only the Wool codes", 2, cutCodeCollection.Count);

			header.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
			cutCodeCollection.Load();
			AssertEquals("Collection is only the Egg codes", 1, cutCodeCollection.Count);

			header.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			cutCodeCollection.Load();
			AssertEquals("Collection is only the Dairy codes", 0, cutCodeCollection.Count);

			header.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			cutCodeCollection.Load();
			AssertEquals("Collection is only the Meat codes", 1, cutCodeCollection.Count);

			var meatCode = cutCodeCollection[0];
			var vealAtrib = meatCode.GetAttribute("IsBeefVeal");
			AssertEquals("Is Veal", "Y", vealAtrib);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var quarantineHeader = Factory.New<QuarantineExDocHeader>();
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			return new EXDOCCutCodeCollection(quarantineHeader);
		}
	}
}
