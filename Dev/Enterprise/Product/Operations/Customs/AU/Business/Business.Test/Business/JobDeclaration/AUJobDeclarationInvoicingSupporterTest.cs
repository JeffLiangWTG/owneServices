using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(JobDeclaration.AUJobDeclarationInvoicingSupporter))]
	sealed class AUJobDeclarationInvoicingSupporterTest : MasterFiles.Business.Testing.JobInvoicingSupporterTest
	{
		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			return Factory.New<JobDeclaration>();
		}

		public void TestGetWarningForContinueAutoRate()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TESTIMP";
			importer.AUIsDutyDeferred = true;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = CustomsEntryStatus.ClearOriginal.Code;

			var invoicingSupporter = declaration.InvoicingSupporter;
			AssertContains("the importer organization of this declaration has the Duty Deferred box ticked", invoicingSupporter.GetWarningForContinueAutoRate());

			importer.AUIsDutyDeferred = false;
			AssertNullOrEmpty(invoicingSupporter.GetWarningForContinueAutoRate());

			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 100m);
			AssertContains("the importer organization of this declaration has the Duty Deferred box NOT ticked", invoicingSupporter.GetWarningForContinueAutoRate());

			importer.AUIsDutyDeferred = true;
			AssertNullOrEmpty(invoicingSupporter.GetWarningForContinueAutoRate());
		}
	}
}
