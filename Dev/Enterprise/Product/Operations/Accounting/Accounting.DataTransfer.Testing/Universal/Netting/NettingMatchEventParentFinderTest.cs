using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer.Universal;
using Enterprise.Accounting.DataTransfer.Universal.Netting;
using Enterprise.Accounting.Netting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.DataTransfer.Testing.Universal.Netting
{
	class NettingMatchEventParentFinderTest : TestCaseWithFactory
	{
		public void TestSettledOutOfNettingWhenNettingTransactionIsNotMatched()
		{
			InvoicingBase arInvoice = null;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), initiatingBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				arInvoice = creator.CreateInvoice(typeof(ARInvoice), "00001000", creator.AUD, 1M, receiver.Organisation);
				creator.CreateInvoiceLine(arInvoice, creator.AUD, 1M, 120M, 0M, 0M, 120M, 0M, 0M, creator.FRT.PK);
				Factory.Save();
			}

			var nettingARTransaction = ImportNettingReceivableTransaction(arInvoice);
			AssertNotNull("Transaction should be created", nettingARTransaction);
			AssertEquals(NettingTransactionApprovalStatus.Approved, nettingARTransaction.ApprovalStatus);
			var nettingAPTransaction = Factory.Load<NettingPayableTransaction>(new ZQuery()).First();
			AssertNotNull("Transaction should be created", nettingAPTransaction);
			AssertEquals(NettingTransactionApprovalStatus.Added, nettingAPTransaction.ApprovalStatus);

			var finder = new NettingMatchEventParentFinder(Factory, new NettingTransactionDataContextManager(), new DummyLogger());

			var manager = new NettingTransactionDataContextManager();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), initiatingBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var universalEvent = UniversalTransactionTransmitter.GetUniversalEvent_ForTestOnly(Factory, arInvoice, InvoiceAdditionalReference.FullyMatched);

				var transaction = finder.GetLogParentsForEvent(universalEvent).First();
				manager.OnLogParentFoundFromEDIMessage(logger, universalEvent, null, transaction);
				Factory.Save();
			}

			nettingARTransaction.Reload();
			nettingAPTransaction.Reload();
			AssertEquals(NettingTransactionApprovalStatus.SettledOutOfNetting, nettingARTransaction.ApprovalStatus);
			AssertEquals(NettingTransactionApprovalStatus.Added, nettingAPTransaction.ApprovalStatus);
		}

		public void TestSettledOutOfNettingWhenNettingTransactionIsMatched()
		{
			InvoicingBase arInvoice = null;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), initiatingBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				arInvoice = creator.CreateInvoice(typeof(ARInvoice), "00001000", creator.AUD, 1M, receiver.Organisation);
				creator.CreateInvoiceLine(arInvoice, creator.AUD, 1M, 120M, 0M, 0M, 120M, 0M, 0M, creator.GLHeader1.PK);
				Factory.Save();
			}

			var nettingARTransaction = ImportNettingReceivableTransaction(arInvoice);
			AssertNotNull("Transaction should be created", nettingARTransaction);
			AssertEquals("Default status is APP for AR", NettingTransactionApprovalStatus.Approved, nettingARTransaction.ApprovalStatus);
			var nettingAPTransaction = Factory.Load<NettingPayableTransaction>(new ZQuery()).First();
			AssertNotNull("Transaction should be created", nettingAPTransaction);
			AssertEquals("Default status is ADD for AP", NettingTransactionApprovalStatus.Added, nettingAPTransaction.ApprovalStatus);

			InvoicingBase apInvoice = null;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), receivingBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				apInvoice = creator.CreateInvoice(typeof(APInvoice), "00001000", creator.AUD, 1M, sender.Organisation);
				creator.CreateInvoiceLine(apInvoice, creator.AUD, 1M, 120M, 0M, 0M, 120M, 0M, 0M, creator.GLHeader1.PK);
				Factory.Save();
			}

			ImportNettingPayableTransaction(apInvoice);
			nettingAPTransaction.Reload();
			AssertEquals("Status for AP changed to APP", NettingTransactionApprovalStatus.Approved, nettingAPTransaction.ApprovalStatus);

			//simulating matching
			var pivot = Factory.New<NettingMatchPivot>();
			pivot.NMP_NSP_Period = period.PK;
			pivot.NMP_NRT_ReceivableTransaction = nettingARTransaction.PK;
			pivot.NMP_NPT_PayableTransaction = nettingAPTransaction.PK;
			nettingARTransaction.ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			nettingAPTransaction.ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			Factory.Save();

			var finder = new NettingMatchEventParentFinder(Factory, new NettingTransactionDataContextManager(), new DummyLogger());
			var manager = new NettingTransactionDataContextManager();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());

			//simulates event after AR is fully matched
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), initiatingBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var universalEventForAR = UniversalTransactionTransmitter.GetUniversalEvent_ForTestOnly(Factory, arInvoice, InvoiceAdditionalReference.FullyMatched);

				var arTransaction = finder.GetLogParentsForEvent(universalEventForAR).First();
				manager.OnLogParentFoundFromEDIMessage(logger, universalEventForAR, null, arTransaction);
				Factory.Save();
			}

			nettingARTransaction.Reload();
			nettingAPTransaction.Reload();
			AssertEquals("AR invoice is matched outside of Netting, AR transaction in Netting System gets SON status", NettingTransactionApprovalStatus.SettledOutOfNetting, nettingARTransaction.ApprovalStatus);
			AssertEquals("AP transaction in Netting System gets APP status and is open for matching", NettingTransactionApprovalStatus.Approved, nettingAPTransaction.ApprovalStatus);

			//simulates event after AP is fully matched
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), receivingBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var universalEventForAP = UniversalTransactionTransmitter.GetUniversalEvent_ForTestOnly(Factory, apInvoice, InvoiceAdditionalReference.FullyMatched);

				var apTransaction = finder.GetLogParentsForEvent(universalEventForAP).First();
				manager.OnLogParentFoundFromEDIMessage(logger, universalEventForAP, null, apTransaction);
				Factory.Save();
			}

			nettingARTransaction.Reload();
			nettingAPTransaction.Reload();
			AssertEquals("AR transaction remains in SON status", NettingTransactionApprovalStatus.SettledOutOfNetting, nettingARTransaction.ApprovalStatus);
			AssertEquals("Now AP transaction too becomes SON since it too is matched off outside of Netting", NettingTransactionApprovalStatus.SettledOutOfNetting, nettingAPTransaction.ApprovalStatus);
		}

		public void TestMultipleAPTransactionWithSameInternalReference()
		{
			InvoicingBase arInvoice = null;
			InvoicingBase apInvoice1 = null;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), initiatingBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				arInvoice = creator.CreateInvoice(typeof(ARInvoice), "00001000", creator.AUD, 1M, receiver.Organisation);
				creator.CreateInvoiceLine(arInvoice, creator.AUD, 1M, 120M, 0M, 0M, 120M, 0M, 0M, creator.GLHeader1.PK);
				Factory.Save();

				apInvoice1 = creator.CreateInvoice(typeof(APInvoice), "00001001", creator.AUD, 1M, receiver.Organisation);
				creator.CreateInvoiceLine(apInvoice1, creator.AUD, 1M, 120M, 0M, 0M, 120M, 0M, 0M, creator.GLHeader1.PK);
				Factory.Save();
			}

			InvoicingBase apInvoice2 = null;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), receivingBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				apInvoice2 = creator.CreateInvoice(typeof(APInvoice), "00001000", creator.AUD, 1M, receiver.Organisation);
				creator.CreateInvoiceLine(apInvoice2, creator.AUD, 1M, 120M, 0M, 0M, 120M, 0M, 0M, creator.GLHeader1.PK);
				Factory.Save();
			}

			var query = new ZQuery(AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, "00001000");
			query.OrderBy = AccTransactionHeaderSchema.AH_TransactionNum.Name;

			var items = Factory.Load<AccTransactionHeader>(query);
			AssertEquals("Precondition: 2 payable transactions have the same AH_ConsolidatedInvoiceRef", 2, items.Length);

			ImportNettingPayableTransaction(apInvoice1);
			ImportNettingPayableTransaction(apInvoice2);

			Factory.Save();

			var finder = new NettingMatchEventParentFinder(Factory, new NettingTransactionDataContextManager(), new DummyLogger());
			var manager = new NettingTransactionDataContextManager();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), receivingBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var universalEventForAP = UniversalTransactionTransmitter.GetUniversalEvent_ForTestOnly(Factory, apInvoice2, InvoiceAdditionalReference.FullyMatched);

				var apTransaction = (NettingPayableTransaction)finder.GetLogParentsForEvent(universalEventForAP).First();

				AssertEquals("The system should find the transaction with transaction number 00001000", "00001000", apTransaction.NPT_Reference);
			}
		}

		public void TestUndoSettledOutOfNetting()
		{
			InvoicingBase arInvoice = null;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), initiatingBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				arInvoice = creator.CreateInvoice(typeof(ARInvoice), "00001000", creator.AUD, 1M, receiver.Organisation);
				creator.CreateInvoiceLine(arInvoice, creator.AUD, 1M, 120M, 0M, 0M, 120M, 0M, 0M, creator.GLHeader1.PK);
				Factory.Save();
			}

			var nettingARTransaction = ImportNettingReceivableTransaction(arInvoice);
			AssertNotNull("Transaction should be created", nettingARTransaction);
			AssertEquals("Default status of AR", NettingTransactionApprovalStatus.Approved, nettingARTransaction.ApprovalStatus);
			var nettingAPTransaction = Factory.Load<NettingPayableTransaction>(new ZQuery()).First();
			AssertNotNull("Transaction should be created", nettingAPTransaction);
			AssertEquals("Default status of AP", NettingTransactionApprovalStatus.Added, nettingAPTransaction.ApprovalStatus);

			InvoicingBase apInvoice = null;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), receivingBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				apInvoice = creator.CreateInvoice(typeof(APInvoice), "00001000", creator.AUD, 1M, sender.Organisation);
				creator.CreateInvoiceLine(apInvoice, creator.AUD, 1M, 120M, 0M, 0M, 120M, 0M, 0M, creator.GLHeader1.PK);
				Factory.Save();
			}

			ImportNettingPayableTransaction(apInvoice);
			nettingAPTransaction.Reload();
			AssertEquals("AP transaction changed status to APP", NettingTransactionApprovalStatus.Approved, nettingAPTransaction.ApprovalStatus);

			//simulate SettledOutOfNetting
			nettingARTransaction.ApprovalStatus = NettingTransactionApprovalStatus.SettledOutOfNetting;
			nettingAPTransaction.ApprovalStatus = NettingTransactionApprovalStatus.SettledOutOfNetting;
			Factory.Save();

			var finder = new NettingMatchEventParentFinder(Factory, new NettingTransactionDataContextManager(), new DummyLogger());
			var manager = new NettingTransactionDataContextManager();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());

			//simulates undo fully match for AR invoice
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), initiatingBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var universalEventForAR = UniversalTransactionTransmitter.GetUniversalEvent_ForTestOnly(Factory, arInvoice, InvoiceAdditionalReference.UndoFullyMatched);

				var arTransaction = finder.GetLogParentsForEvent(universalEventForAR).First();
				manager.OnLogParentFoundFromEDIMessage(logger, universalEventForAR, null, arTransaction);
				Factory.Save();
			}

			nettingARTransaction.Reload();
			nettingARTransaction.Reload();
			AssertEquals(NettingTransactionApprovalStatus.Approved, nettingARTransaction.ApprovalStatus);
			AssertEquals(NettingTransactionApprovalStatus.SettledOutOfNetting, nettingAPTransaction.ApprovalStatus);

			//simulates undo fully match for AP invoice
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), receivingBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var universalEventForAP = UniversalTransactionTransmitter.GetUniversalEvent_ForTestOnly(Factory, apInvoice, InvoiceAdditionalReference.UndoFullyMatched);

				var apTransaction = finder.GetLogParentsForEvent(universalEventForAP).First();
				manager.OnLogParentFoundFromEDIMessage(logger, universalEventForAP, null, apTransaction);
				Factory.Save();
			}

			nettingARTransaction.Reload();
			nettingARTransaction.Reload();
			AssertEquals(NettingTransactionApprovalStatus.Approved, nettingARTransaction.ApprovalStatus);
			AssertEquals(NettingTransactionApprovalStatus.Approved, nettingAPTransaction.ApprovalStatus);
		}

		public void TestNoExceptionButLogWhenNoRelevantTransactionFound()
		{
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var finder = new NettingMatchEventParentFinder(Factory, new NettingTransactionDataContextManager(), logger);
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), initiatingBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				InvoicingBase arInvoice = null;
				arInvoice = creator.CreateInvoice(typeof(ARInvoice), "10001111", creator.AUD, 1M, receiver.Organisation);
				creator.CreateInvoiceLine(arInvoice, creator.AUD, 1M, 120M, 0M, 0M, 120M, 0M, 0M, creator.FRT.PK);
				var universalEvent_ar = UniversalTransactionTransmitter.GetUniversalEvent_ForTestOnly(Factory, arInvoice, InvoiceAdditionalReference.FullyMatched);
				var internalReferenceNumToBeRemoved = universalEvent_ar.ContextCollection.Find(contextItem => contextItem.Type.Type.Value == "InternalReferenceNum" && contextItem.Value.Value == "");
				universalEvent_ar.ContextCollection.Remove(internalReferenceNumToBeRemoved); // remove this in order to add new internalReferenceNumber for testing without duplicate
				universalEvent_ar.ContextCollection.Add(new Context() { Type = new ContextType() { Type = InvoiceForNettingInfoList.InternalReferenceNumber }, Value = "00000001" });

				AssertNoExceptionThrown(delegate
				{
					var transactions = finder.GetLogParentsForEvent(universalEvent_ar);
					AssertEquals("Expecting no relevant transaction found.", true, transactions == null);
					var logMessage = logger.ToString();
					var expectedLogMessage = "Transaction not found in Netting to update.\r\nTransaction Number: 10001111\r\nInternal Reference Number: 00000001\r\nTransaction Status: " + InvoiceAdditionalReference.FullyMatched
					+ "\r\nLedger: AR\r\nTransaction Type: INV\r\nSender: EDISND001\r\nReceiver: EDIRCV001";
					AssertEquals("Expecting correct format and content of log message", "Warning - " + expectedLogMessage, logMessage);
				});
			}
		}

		NettingReceivableTransaction ImportNettingReceivableTransaction(InvoicingBase invoice)
		{
			NettingReceivableTransaction nettingTransaction = null;

			var message = GetMessage(initiatingEHubId, nettingSystemEHubID);
			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.IDB, invoice)));
			var universalTransaction = writer.GetDataObject(invoice);
			var importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref nettingTransaction));
			Factory.Save();

			return nettingTransaction;
		}

		void ImportNettingPayableTransaction(InvoicingBase invoice)
		{
			var message = GetMessage(receivingEHubId, nettingSystemEHubID);
			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.IDB, invoice)));
			var universalTransaction = writer.GetDataObject(invoice);
			var importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			importer.ImportNettingTransaction(message, universalTransaction);

			Factory.Save();
		}

		IEDIMessage GetMessage(string sender, string receiver)
		{
			var newFactory = new BusinessObjectFactory();
			var message = newFactory.New<IEDIMessage>();

			var interchange = newFactory.New<IEDIInterchange>();
			interchange.EI_From = sender;
			interchange.EI_To = receiver;

			message.EM_EI = interchange.PK;

			return message;
		}

		NettingSystemPeriod SetupNettingPeriod(NettingSystem ns)
		{
			var nsp = Factory.New<NettingSystemPeriod>();
			nsp.NSP_Period = "234234";
			nsp.NSP_NS_NettingSystem = ns.PK;
			nsp.NSP_EarliestInvoiceDateUtc = ZDateTime.Now.AddDays(-15);
			nsp.NSP_LatestInvoiceDateUtc = ZDateTime.Now.AddDays(15);
			nsp.NSP_NettingExecutionDateUtc = ZDateTime.Now.AddDays(8);
			nsp.NSP_LatestApprovalDateUtc = ZDateTime.Now.AddDays(13);

			nsp.NSP_LatestFXOfferDateUtc = ZDateTime.Now.AddDays(8);
			nsp.NSP_LatestUploadDateUtc = ZDateTime.Now.AddDays(8);

			nsp.NSP_ValueDate = ZDate.Today.AddDays(8);
			nsp.NSP_OfferPrepaymentDate = ZDate.Today.AddDays(8);

			return nsp;
		}

		NettingOrganisation SetupSender(NettingSystem ns, string senderEHubID)
		{
			var sender = Creator.CreateOrgHeader("TSTSND1", true, true);
			sender.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, senderEHubID);

			return Creator.CreateNettingOrganisation(ns, sender, "CUR");
		}

		NettingOrganisation SetupReceiver(NettingSystem ns, string eHubID)
		{
			var receiver = Creator.CreateOrgHeader("TSTRCV1", true, true);
			creator.AddEdiCommunication(receiver, EDICommunicationsMode.Modules.Netting, EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, ns.NS_Code);
			receiver.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, eHubID);

			return Creator.CreateNettingOrganisation(ns, receiver, "CUR");
		}

		NettingObjectCreator Creator
		{
			get
			{
				return creator ?? (creator = new NettingObjectCreator(Factory));
			}
		}
		NettingObjectCreator creator;

		NettingSystem SetupNettingSystem(string eHubID)
		{
			var ns = Factory.New<NettingSystem>();
			ns.NS_Code = eHubID;
			ns.NS_GC = GlbCompany.CurrentCompany.PK;
			ns.NS_Description = "bla bla";

			var nettingSystem = Creator.CreateOrgHeader("TSTNET", true, true);
			nettingSystem.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, eHubID);

			var noNettingSystem = Factory.New<NettingOrganisation>();
			noNettingSystem.NSO_NS_NettingSystem = ns.PK;
			noNettingSystem.NSO_NettingType = "CUR";
			noNettingSystem.NSO_OH_Organisation = nettingSystem.PK;

			return ns;
		}

		NettingSystem ns;
		NettingSystemPeriod period;
		ZString nettingSystemEHubID;
		NettingOrganisation sender;
		ZString initiatingEHubId;
		NettingOrganisation receiver;
		ZString receivingEHubId;
		GlbBranch initiatingBranch;
		GlbBranch receivingBranch;

		protected override void SetUp()
		{
			base.SetUp();

			SetupControlAccounts();

			nettingSystemEHubID = "EDIWNS001";
			ns = SetupNettingSystem(nettingSystemEHubID);

			initiatingEHubId = "EDISND001";
			sender = SetupSender(ns, initiatingEHubId);
			var initiatingCompany = Creator.CreateNewCompany("CX1", orgProxy: sender.Organisation);
			initiatingBranch = Creator.CreateBranch("BX1", initiatingCompany);

			receivingEHubId = "EDIRCV001";
			receiver = SetupReceiver(ns, receivingEHubId);
			var receivingCompany = Creator.CreateNewCompany("CX2", orgProxy: receiver.Organisation);
			receivingBranch = Creator.CreateBranch("BX2", receivingCompany);

			period = SetupNettingPeriod(ns);

			Factory.Save();
		}

		void SetupControlAccounts()
		{
			AccGLHeader aRSuspenseControlAccount = Creator.CreateARSuspenseControlAccount();
			AccGLHeader aPSuspenseControlAccount = Creator.CreateAPSuspenseControlAccount();
			AccGLHeader jobRevenueJournalControlAccount = Creator.CreateJobRevenueJournalControlAccount();
			AccGLHeader cFXAccount = Creator.CreateCFXAccount();
			Factory.Save();

			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aRSuspenseControlAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aPSuspenseControlAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, jobRevenueJournalControlAccount.PK.ToGuid());
			GlbDepartment department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CES"));
			AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(Guid.Empty, Guid.Empty, department.PK.ToGuid(), cFXAccount.PK.ToGuid());
		}
	}
}
