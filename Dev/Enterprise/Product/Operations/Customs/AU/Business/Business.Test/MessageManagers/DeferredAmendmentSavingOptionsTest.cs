using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(DeferredAmendmentSavingOptions))]
	sealed class DeferredAmendmentSavingOptionsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProcessWhenChangesAreSavedWithoutSending()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 20000m;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 20000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_CL = entryLine.PK;
			line.JI_LinePrice = 20000m;

			declaration.JE_MessageStatus = CustomsEntryStatus.ClearFormalLodge.Code;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();

			invoice.JZ_InvoiceAmount = 50000m;
			line.JI_LinePrice = 50000m;

			var savingOptions = new DeferredAmendmentSavingOptions(declaration);
			Assert("Not Significantly changed", !savingOptions.SignificantAmendmentsHaveBeenMade);

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			savingOptions = new DeferredAmendmentSavingOptions(declaration);
			Assert("Significantly changed", savingOptions.SignificantAmendmentsHaveBeenMade);

			savingOptions.SaveWithoutEntryChanges = true;

			AssertEquals("CustomsValue still remains 20000", 20000m, line.CusEntryLine.CL_CustomsValue);
			AssertEquals("No outstanding amendment log", false, new OutstandingAmendmentLogManager(declaration).HasOutstandingAmendmentsNotQueued);

			var bizObj = (IBackDoorSavingSupportableBizObj)declaration;
			AssertEquals("Remerged", ContinueWithDetection.Yes, bizObj.ProcessBeforeDetectingAmendmentAndContinue());
			AssertEquals("No need to merge", false, declaration.MergeManager.RequiresMergeBeforeSave);

			AssertEquals("CustomsValue recalculated", 50000m, line.CusEntryLine.CL_CustomsValue);

			savingOptions.ProcessWhenChangesAreSavedWithoutSending("");
			AssertEquals("Customs value reverted as saved without entry changes", 20000m, line.CusEntryLine.CL_CustomsValue);
			Assert(new OutstandingAmendmentLogManager(declaration).HasOutstandingAmendmentsNotQueued);

			savingOptions.SaveWithEntryChanges = true;
			Assert(!new OutstandingAmendmentLogManager(declaration).HasOutstandingAmendments);

			savingOptions.ProcessWhenChangesAreSavedWithoutSending("TEST");
			Assert(new OutstandingAmendmentLogManager(declaration).HasOutstandingAmendments);
			Assert(new OutstandingAmendmentLogManager(declaration).AllOustandingAmendmentReferences.Contains("TEST"));

			var sO = new DeferredAmendmentSavingOptions(null);
			sO.SaveWithoutEntryChanges = true;
			AssertNoExceptionThrown(() => sO.ProcessWhenChangesAreSavedWithoutSending("Test"));
		}

		protected override BusinessObject GetNewBusinessObject() => new DeferredAmendmentSavingOptions(Factory.New<JobDeclaration>());
	}
}
