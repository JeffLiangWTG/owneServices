using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(EXDOCDominantProductCollection))]
	sealed class EXDOCDominantProductCollectionTest : EXDOCRefCodeCollectionTest<EXDOCDominantProductCollection>
	{
		public void TestFindByRefCode()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType("DOMP", "DOMP");
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "DOMP", "EMU", "EMU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "DOMP", "GOAT", "GOAT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			_ = helper.Header1.QuarantineExDocHeader;

			var collection = new EXDOCDominantProductCollection(helper.Declaration.Invoices[0].JobComInvoiceLines[0].QuarantineExDocLine.QuarantineExDocHeader);
			collection.Load();
			AssertNotNull("Contains", collection.FindByCode("EMU"));
			AssertEquals("Selects", "EMU", collection.FindByCode("EMU").ZZD_Code);
			AssertNull("Not Contains", collection.FindByCode("XYZ"));
		}
	}
}
