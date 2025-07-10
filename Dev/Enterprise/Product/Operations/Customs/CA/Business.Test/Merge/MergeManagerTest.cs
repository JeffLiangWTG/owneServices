using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class MergeManagerTest : Customs.Business.Testing.MergeManagerTest
	{
		public void TestRequiresMergeWhenNoInvoiceButEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.Save();

			var mergeManager = (MergeManager)declaration.MergeManager;
			declaration.JE_MergeBy = "TRF";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;

			Assert("Require merge when there are entry generated", mergeManager.RequiresMergeBeforeSave);
		}

		public void TestCannotMergeReason()
		{
			var b2Dec = Factory.New<JobDeclaration>();
			var invoice = b2Dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			b2Dec.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2Dec.CA_B2Type = B2TypeList.Codes.Specific;
			Factory.Save();
			b2Dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			AssertNoExceptionThrown(delegate
			{ b2Dec.DoMerge(); });
			b2Dec.CA_B2AcceptedDate = new ZDateTime(2016, 08, 18);
			var header = b2Dec.Invoices.AddNew();
			var line = header.AsAccountForFilteredInvoiceLines.AddNew();
			AssertNoExceptionThrown(delegate
			{ b2Dec.DoMerge(); });
		}

		public void TestRequiresMergeCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var mergeManager = (MergeManager)declaration.MergeManager;
			Factory.Save();
			Assert("Not requires merge originally", !mergeManager.RequiresMergeBeforeSave);

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV123";
			Assert("Not requires merge when only have invoice header", !mergeManager.RequiresMergeBeforeSave);

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10m;
			Assert("Requires merge when there are invoice and invoice lines", mergeManager.RequiresMergeBeforeSave);
		}

		public void TestRequiresMergeWhenIsThrowingAwayMerge()
		{
			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var mergeManager = (MergeManager)declaration.MergeManager;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV123";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10m;
			Assert("Requires merge originally", mergeManager.RequiresMergeBeforeSave);

			invoiceLine.JI_LinePrice = 11m;
			declaration.IsThrowingAwayMergeReturns = true;
			Assert("Not requires merge when IsThrowingAwayMerge = true", !mergeManager.RequiresMergeBeforeSave);

			invoiceLine.JI_LinePrice = 12m;
			declaration.IsThrowingAwayMergeReturns = false;
			Assert("Requires merge when IsThrowingAwayMerge set back to false", mergeManager.RequiresMergeBeforeSave);
		}

		public void TestRequiresMergeCore_SupportAttachedInvoices()
		{
			var lvsJob = Factory.New<JobDeclaration>();
			lvsJob.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var mergeManager = (MergeManager)lvsJob.MergeManager;
			Factory.Save();
			Assert("Not requires merge originally", !mergeManager.RequiresMergeBeforeSave);

			var lvxJob = Factory.New<JobDeclaration>();
			lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var invoice = lvxJob.LVXInvoiceHeader;
			invoice.JZ_InvoiceNumber = "INV123";
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(invoice, lvsJob);
			Assert("Not requires merge when LVX has no invoice line", !mergeManager.RequiresMergeBeforeSave);

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10m;
			Assert("Requires merge when LVX has invoice line", mergeManager.RequiresMergeBeforeSave);

			using (new BaseJobDeclaration.InvoicesOverrideDeclarationSupporter(lvsJob))
			{
				invoiceLine.JI_LinePrice = 20m;
				Assert("Requires merge when LVX invoice overriden to LVS", mergeManager.RequiresMergeBeforeSave);
			}
		}

		public void TestExportMerge()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;

			declaration.Invoices.AddNew();
			var line1 = declaration.InvoiceLines.AddNew();
			line1.JI_Tariff = "2203.00.60";
			var line2 = declaration.InvoiceLines.AddNew();
			line2.JI_Tariff = "2203.00.60";
			var manager = new MergeManager(declaration);
			manager.Execute(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEquals(1, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestGetAbnormalDataWarningMessage()
		{
			var testDec = Factory.New<JobDeclaration>();
			var invoice = testDec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			testDec.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			testDec.CA_K84AccountingDate = new ZDateTime(2024, 09, 20);
			Factory.Save();

			var notifier = new SendsMessagesToCustomsShutterUpperer();
			notifier.AnswerToContinueWithAction = false;
			var manager = new MergeManager(testDec);
			Assert("B2 merge execute success", manager.Execute(notifier));
			AssertNullOrEmpty("B2 no abnormal message", notifier.ContinueWithActionMessage);

			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("IMP merge execute fail", !manager.Execute(notifier));
			AssertEquals("Changes have been made which result in a merge being required, however this declaration has already been 'accounted for' (B3 accepted) and so these changes should not be made. Do you want to continue with this save?", notifier.ContinueWithActionMessage);

			notifier.ContinueWithActionMessage = string.Empty;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				Assert("CAD merge execute success", manager.Execute(notifier));
				AssertNullOrEmpty("CAD no abnormal message", notifier.ContinueWithActionMessage);
			}
		}

		protected override Type GetLineMergerType() => typeof(LineMerger);

		protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();
	}
}
