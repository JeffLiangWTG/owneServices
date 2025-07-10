using Enterprise.Customs.Business.AccumulativeAmendment;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class AmendTypeCodeListPartialTest : NUnit.Framework.TestCase
	{
		public void TestGetAmendTypeDescription()
		{
			AssertEquals(nameof(EntityAmendType.Add), AmendTypeCodeList.GetAmendTypeDescription(AmendTypeCodeList.Codes._01));
			AssertEquals(nameof(EntityAmendType.Delete), AmendTypeCodeList.GetAmendTypeDescription(AmendTypeCodeList.Codes._02));
			AssertEquals(nameof(EntityAmendType.Update), AmendTypeCodeList.GetAmendTypeDescription(AmendTypeCodeList.Codes._03));
		}
		public void TestAmendTypeForMessage()
		{
			AssertEquals(AmendTypeCodeList.Codes._01, AmendTypeCodeList.AmendTypeForMessage(EntityAmendType.Add));
			AssertEquals(AmendTypeCodeList.Codes._02, AmendTypeCodeList.AmendTypeForMessage(EntityAmendType.Delete));
			AssertEquals(AmendTypeCodeList.Codes._03, AmendTypeCodeList.AmendTypeForMessage(EntityAmendType.Update));
		}
		public void TestLocalExportAmendTypeForMessage()
		{
			AssertEquals("2", AmendTypeCodeList.LocalExportAmendTypeForMessage(EntityAmendType.Delete));
			AssertEquals("3", AmendTypeCodeList.LocalExportAmendTypeForMessage(EntityAmendType.Update));
		}
		public void TestImportFTAAmendTypeForItem()
		{
			AssertEquals(ImportFTAItemAmendTypeCodeList.Codes._99I, AmendTypeCodeList.ImportFTAAmendTypeForItem(EntityAmendType.Add));
			AssertEquals(ImportFTAItemAmendTypeCodeList.Codes._99D, AmendTypeCodeList.ImportFTAAmendTypeForItem(EntityAmendType.Delete));
		}
	}
}
