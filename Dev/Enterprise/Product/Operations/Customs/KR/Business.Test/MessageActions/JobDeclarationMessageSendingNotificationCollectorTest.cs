using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	class JobDeclarationMessageSendingNotificationCollectorTest : TestCaseWithFactory
	{
		public void TestExcludeNotificationsFromJobHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = declaration.PK;
			jobHeader.JH_ParentTableCode = "JE";
			jobHeader.JH_GB = declaration.JE_GB;
			jobHeader.JH_GC = declaration.JE_GC;
			declaration.RegisterEditableChildObject(jobHeader);
			Factory.Save();
			var warningMessage = jobHeader.JH_GS_NKRepSalesInfo.GetWarnings().ToUniqueMessageListString();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = KRJobMessageTypeList.Codes.Export;
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var sendingObject1 = new JobDeclarationMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._830);
			sendingObject1.ShouldSend = true;
			AssertNotContains(warningMessage, sendingObject1.BizObjValidationMessageErrors);
		}
	}
}
