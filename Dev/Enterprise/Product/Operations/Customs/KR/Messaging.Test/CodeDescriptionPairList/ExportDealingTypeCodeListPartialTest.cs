namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class ExportDealingTypeCodeListPartialTest : NUnit.Framework.TestCase
	{
		public void TestIsImportedToReExported()
		{
			AssertEquals(false, ExportDealingTypeCodeList.IsImportedToReExported(ExportDealingTypeCodeList.Codes._100));
			AssertEquals(true, ExportDealingTypeCodeList.IsImportedToReExported(ExportDealingTypeCodeList.Codes._72));
			AssertEquals(true, ExportDealingTypeCodeList.IsImportedToReExported(ExportDealingTypeCodeList.Codes._84));
			AssertEquals(true, ExportDealingTypeCodeList.IsImportedToReExported(ExportDealingTypeCodeList.Codes._86));
			AssertEquals(true, ExportDealingTypeCodeList.IsImportedToReExported(ExportDealingTypeCodeList.Codes._89));
			AssertEquals(true, ExportDealingTypeCodeList.IsImportedToReExported(ExportDealingTypeCodeList.Codes._93));
			AssertEquals(true, ExportDealingTypeCodeList.IsImportedToReExported(ExportDealingTypeCodeList.Codes._94));
		}

		public void TestIsImportDeclarationNumberOptional()
		{
			AssertEquals(false, ExportDealingTypeCodeList.IsImportedToReExported(ExportDealingTypeCodeList.Codes._100));
			AssertEquals(true, ExportDealingTypeCodeList.IsImportedToReExported(ExportDealingTypeCodeList.Codes._94));
		}
	}
}
