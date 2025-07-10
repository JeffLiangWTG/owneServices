using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class JobDeclarationLoadingCompletionMessageSendingObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckLoadingDate()
		{
			var entry = Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			var parent = new JobDeclarationLoadingCompletionMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._DF3);

			parent.LoadingDate = ZDate.Empty;
			parent.ShouldSend = true;
			AssertNoMessageErrors(parent.LoadingDateInfo);
		}

		public void TestDefaultLoadingDate()
		{
			var entry = Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			entry.Declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			entry.Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
			entry.Declaration.JE_EntryDate = ZDate.Empty;
			var parent = new JobDeclarationLoadingCompletionMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._DF3);
			parent.ShouldSend = true;
			AssertEquals(ZDateTime.Today, parent.LoadingDate);

			entry.Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._08;
			entry.Declaration.JE_EntryDate = ZDate.Empty;
			parent = new JobDeclarationLoadingCompletionMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._DF3);
			parent.ShouldSend = true;
			AssertEquals(ZDateTime.Empty, parent.LoadingDate);
		}
	}
}
