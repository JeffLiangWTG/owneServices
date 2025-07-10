using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class ZAUCustomsDeclarationFormTest : TestCaseWithFactory
	{
		public void TestAmendmentCheckerType()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (var form = new ZAUCustomsDeclarationFormForTest(declaration))
			{
				AssertEquals(typeof(MessagingActionsController), form.GetNewMessagingActionsController().GetType());
			}
		}

		[TestDate(2006, 1, 1)]
		public void TestShowPreSaveDialogsInitialisation()
		{
			// Setup
			var creator = new DeclarationCreator(Factory);
			creator.Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			// Act
			using (var form = new ZAUCustomsDeclarationFormForTest(creator.Declaration))
			{
				form.Show();
				creator.InvoiceLine1.JI_InvoiceQuantity = 1m;
			}

			// Assert
			AssertEquals("CanContinueWithSaveSendingAnAmendmentIfNeeded should not have been called at all.", 0, creator.Declaration.CanContinueWithSaveSendingAnAmendmentIfNeededInvocations);
		}

		[TestDate(2006, 1, 1)]
		public void TestShowPreSaveDialogsWithInvoiceQuantity1m()
		{
			// Setup
			var creator = new DeclarationCreator(Factory);
			creator.Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			using (var form = new ZAUCustomsDeclarationFormForTest(creator.Declaration))
			{
				form.Show();
				creator.Declaration.CanContinueWithSaveSendingAnAmendmentIfNeededReturns = false;

				// Act
				creator.InvoiceLine1.JI_InvoiceQuantity = 2m;

				// Assert
				AssertEquals("needs merge", true, creator.Declaration.MergeManager.RequiresMerge);
				AssertEquals("ContinueWithSave", ContinueWithSave.No, form.ShowPreSaveDialogs());
				AssertEquals("needs merge", false, creator.Declaration.MergeManager.RequiresMerge);
				AssertEquals("CanContinueWithSaveSendingAnAmendmentIfNeeded should have been called at least once.", true, creator.Declaration.CanContinueWithSaveSendingAnAmendmentIfNeededInvocations > 0);
			}
		}

		[TestDate(2006, 1, 1)]
		public void TestShowPreSaveDialogsWithInvoiceQuantity2m()
		{
			// Setup
			var creator = new DeclarationCreator(Factory);
			creator.Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			using (var form = new ZAUCustomsDeclarationFormForTest(creator.Declaration))
			{
				form.Show();
				creator.Declaration.CanContinueWithSaveSendingAnAmendmentIfNeededReturns = true;

				// Act
				creator.InvoiceLine1.JI_InvoiceQuantity = 1m;

				// Assert
				AssertEquals("needs merge", true, creator.Declaration.MergeManager.RequiresMerge);
				AssertEquals("ContinueWithSave", ContinueWithSave.Yes, form.ShowPreSaveDialogs());
				AssertEquals("needs merge", false, creator.Declaration.MergeManager.RequiresMerge);
				AssertEquals("CanContinueWithSaveSendingAnAmendmentIfNeeded should have been called at least once.", true, creator.Declaration.CanContinueWithSaveSendingAnAmendmentIfNeededInvocations > 0);
			}
		}

		[TestDate(2006, 1, 1)]
		public void TestNotShowPreSaveDialogsOnConsolidatedEntryMemberDeclaration()
		{
			var creator = new DeclarationCreator(Factory);
			var declaration = creator.Declaration;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.EntryHeader.CH_Status = Common.AU.CustomsEntryStatus.ClearFormalLodge.Code;
			declaration.EntryHeader.CH_EntryStatus = Common.AU.CustomsEntryStatus.ClearFormalLodge.Code;
			Factory.Save();

			using (var form = new ZAUCustomsDeclarationFormForTest(declaration))
			{
				form.Show();
				declaration.CanContinueWithSaveSendingAnAmendmentIfNeededReturns = true;
				declaration.ForceRefreshCurrentEntryMessageStatus();

				creator.InvoiceLine1.JI_InvoiceQuantity = 3m;
				declaration.JE_DateOfFirstArrival = new ZDateTime(2006, 1, 3);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.LastFormShownDialogForTest = null;
				AssertEquals("needs merge", true, declaration.MergeManager.RequiresMerge);
				var result = form.ShowPreSaveDialogs();
				AssertType<Enterprise.Customs.GUI.BackdoorForSavingOnAmendmentForm>("Standalone Dec shows Amendment Dialog", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("ContinueWithSave", ContinueWithSave.No, result);
				AssertEquals("has merged", false, declaration.MergeManager.RequiresMerge);

				var cecEvents = declaration.Logs.GetAllLogs().Find(l => l.SL_SE_NKEvent == AutoEvents.ConsolidatedEntryChanged.Code);
				AssertEquals("No CEC Event created on Consolidated Entry", 0, cecEvents.Count());
			}

			var consolidatedEntry = Factory.NewWithValidTestData<ConsolidatedDeclaration>();
			consolidatedEntry.CRD_OA_DeclarantAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses[0].PK;
			declaration.JE_EntryStatus = Common.Shared.ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
			consolidatedEntry.JobDeclarations.Add(declaration);
			Factory.Save();
			declaration.EntryHeader.CH_Status = Common.AU.CustomsEntryStatus.ClearFormalLodge.Code;
			declaration.EntryHeader.CH_EntryStatus = Common.AU.CustomsEntryStatus.ClearFormalLodge.Code;
			Factory.Save();

			using (var form = new ZAUCustomsDeclarationFormForTest(declaration))
			{
				form.Show();
				declaration.CanContinueWithSaveSendingAnAmendmentIfNeededReturns = true;
				declaration.ForceRefreshCurrentEntryMessageStatus();

				creator.InvoiceLine1.JI_InvoiceQuantity = 4m;
				declaration.JE_DateOfFirstArrival = new ZDateTime(2006, 1, 4);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.LastFormShownDialogForTest = null;
				AssertEquals("needs merge", true, declaration.MergeManager.RequiresMerge);
				var result = form.ShowPreSaveDialogs();
				AssertNull("Consolidated Entry member Dec skips Amendment Dialog", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("ContinueWithSave", ContinueWithSave.Yes, result);
				AssertEquals("has merged", false, declaration.MergeManager.RequiresMerge);

				var cecEvents = declaration.Logs.GetAllLogs().Find(l => l.SL_SE_NKEvent == AutoEvents.ConsolidatedEntryChanged.Code);
				AssertEquals("CEC Event created on Consolidated Entry", 1, cecEvents.Count());
			}
		}

		sealed class ZAUCustomsDeclarationFormForTest : ZAUCustomsDeclarationForm
		{
			public ZAUCustomsDeclarationFormForTest(JobDeclaration declaration) : base(declaration)
			{
			}

			public new Customs.GUI.SendsMessagesToCustomsGUI GetNewMessagingActionsController() => base.GetNewMessagingActionsController();
			public new ContinueWithSave ShowPreSaveDialogs() => base.ShowPreSaveDialogs();
		}

		sealed class DummyJobDeclaration_JobComInvoiceLine : JobDeclaration
		{
			public DummyJobDeclaration_JobComInvoiceLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool GetIsWHSUniversalXMLActiveReturns { get; set; }
			public int CanContinueWithSaveSendingAnAmendmentIfNeededInvocations { get; set; }
			public bool CanContinueWithSaveSendingAnAmendmentIfNeededReturns { get; set; }
			public bool ForceRefreshCurrentEntryMessageStatus() => base.fRefreshCurrentEntryMessageStatus = true;

			protected override bool GetIsWHSUniversalXMLActive() => GetIsWHSUniversalXMLActiveReturns;

			public override bool CanContinueWithSaveSendingAnAmendmentIfNeeded()
			{
				CanContinueWithSaveSendingAnAmendmentIfNeededInvocations++;
				return CanContinueWithSaveSendingAnAmendmentIfNeededReturns;
			}
		}

		sealed class DeclarationCreator
		{
			public DeclarationCreator(BusinessObjectFactory factory)
			{
				Factory = factory;
				var mock = Factory.New<DummyJobDeclaration_JobComInvoiceLine>();
				mock.GetIsWHSUniversalXMLActiveReturns = false;
				var declaration = mock;
				declaration.FillWithValidTestData();
				var supplier = OrgHeader.New(Factory);
				supplier.FillWithValidTestData();
				supplier.MiscServ.OM_RX_NKEXDefCurrency = declaration.LocalCurrencyCode;
				declaration.JE_OH_Supplier = supplier.PK;
				var buyer = OrgHeader.New(Factory);
				buyer.FillWithValidTestData();
				declaration.JE_OH_Importer = buyer.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(true);
				Customs.Business.BaseJobComInvoiceLine bLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
				declaration.FilteredInvoiceLines.Add(bLine);

				if (!declaration.DoMerge())
				{
					var merger = new Customs.Business.LineMerger(declaration);
					merger.DoMerge();
				}

				Declaration = declaration;
			}

			public DummyJobDeclaration_JobComInvoiceLine Declaration { get; set; }

			public BusinessObjectFactory Factory { get; set; }

			public Customs.Business.BaseJobComInvoiceLine InvoiceLine1
			{
				get { return Declaration.FilteredInvoiceLines[0]; }
			}
		}
	}
}
