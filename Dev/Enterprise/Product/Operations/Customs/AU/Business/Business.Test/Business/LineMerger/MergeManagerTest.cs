using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class MergeManagerTest : Customs.Business.Testing.MergeManagerTest
	{
		public void TestGetReasonCannotMerge()
		{
			var testDec = GetJobDeclaration();
			var invoice = testDec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var mergeManager = testDec.MergeManager;
			var getReasonCannotMergeMethodInfo = typeof(MergeManager).GetMethod("GetReasonCannotMerge",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

			testDec.JE_MessageType = JobMessageTypeList.Codes.Export;
			var reason = (string)getReasonCannotMergeMethodInfo.Invoke(mergeManager, Array.Empty<object>());
			AssertEquals("Not Import", "You can only merge lines when doing an import declaration.", reason);

			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_EntryStatus = CustomsEntryStatus.AwaitingLodge.Code;
			reason = (string)getReasonCannotMergeMethodInfo.Invoke(mergeManager, Array.Empty<object>());
			AssertEquals("HasLodgeBeenSent", "You can't merge lines because you have already lodged the declaration.", reason);

			testDec.JE_EntryStatus = CustomsEntryStatus.AwaitingCPDec.Code;
			reason = (string)getReasonCannotMergeMethodInfo.Invoke(mergeManager, Array.Empty<object>());
			AssertEquals("No error when test", "", reason);

			testDec = GetJobDeclaration();
			testDec.Invoices.AddNew().JobComInvoiceLines.AddNew();
			testDec.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entryNum = CusEntryNumber.New(testDec, CANType.CustomsAuthorityNumber.Code, Core.Constants.CountryCodes.Australia);
			entryNum.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			mergeManager = testDec.MergeManager;

			getReasonCannotMergeMethodInfo = typeof(CMRMergeManager).GetMethod("GetReasonCannotMerge",
			System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			reason = (string)getReasonCannotMergeMethodInfo.Invoke(mergeManager, Array.Empty<object>());
			AssertEquals("Has entry number against EXP declaration", "Export merge functionality is only available for new declarations.", reason);
		}

		public override void TestSupportsAutoMerge()
		{
			var manager = GetJobDeclaration().MergeManager;
			AssertEquals("SupportsAutoMerge", false, manager.SupportsAutoMerge);
		}

		public override void TestSupportsAmendments()
		{
			var manager = GetJobDeclaration().MergeManager;
			AssertEquals("SupportsAmendments", false, MergeManagerTestHelper.GetSupportsAmendments(manager));
		}

		protected override Type GetLineMergerType() => typeof(LineMerger);

		protected override Customs.Business.BaseJobDeclaration GetJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = AUJobMessageTypeList.Codes.MiscellaneousCustoms;
			return declaration;
		}
	}
}
