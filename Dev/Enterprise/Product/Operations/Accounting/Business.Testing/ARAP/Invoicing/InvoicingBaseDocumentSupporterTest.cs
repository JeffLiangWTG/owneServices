using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class InvoicingBaseDocumentSupporterTest : TestCaseWithFactory
	{
		[GuiTest]
		public void TestGetPDFPasswordCore_ForKRElectronicInvoice()
		{
			TestObjectCreator.AALSHI.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "TestPassword1", CountryCodes.KoreaSouth);
			TestObjectCreator.ABIGAS.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "TestPassword2", CountryCodes.KoreaSouth);
			Factory.Save();

			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (AccountingConfigurationRegistry.Instance.ElectronicInvoiceDocumentFallbackPassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestPassword3"))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormShown(showForm =>
				{
					if (showForm is ZForm form && form.BusinessEntity is DeliveryInstructions instructions)
					{
						AssertEquals(1, instructions.Recipients.Count);
						AssertEquals(TestObjectCreator.AALSHI.PK, instructions.Recipients[0].OrgHeaderPK);
						instructions.Recipients[0].DeliveryMethod = ContactNotifyModes.Email;
						instructions.Recipients[0].Email = "AALSHI@test.com";

						instructions.Recipients.Add(new DocDeliveryContact(Factory) { OrgHeaderPK = TestObjectCreator.ABIGAS.PK, DeliveryMethod = ContactNotifyModes.Email, Email = "ABIGAS@test.com" });
						instructions.Recipients.Add(new DocDeliveryContact(Factory) { OrgHeaderPK = TestObjectCreator.ZECTRA.PK, DeliveryMethod = ContactNotifyModes.Email, Email = "ZECTRA@test.com" });

						Application.DoEvents();
					}
				});

				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1.0m, TestObjectCreator.AALSHI);
				TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
				var batch = TestObjectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, invoice, Core.Constants.EInvoicingPivotState.Sent);
				Assert("PreCondition", invoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));
				Factory.Save();

				var command = Factory.LoadTop1<DocumentCommand>(new ZQuery(StmMenuItemSchema.SU_MenuName, EInvoicingKoreaSouthConstants.KRElectronicInvoice));
				command.Parent = invoice;
				new ElectronicInvoicingTransactionProxy(invoice).MostRecentPivot.AIP_Status = EInvoicingPivotState.Succeed;
				Factory.Save();

				var printJobs = Factory.Load<StmPrintJob>(new ZQuery());
				AssertEquals("Should not create a print job before run DocumentRunner", 0, printJobs.Length);

				new DocumentRunner().Run(command);
				printJobs = Factory.Load<StmPrintJob>(new ZQuery());
				AssertEquals("Should create 3 print job", 3, printJobs.Length);
				AssertEquals(true, printJobs.All(x => x.EmailToRecipients.Count == 1));
				AssertEquals("Should equal to the encrypted 'TestPassword1", "2jaXPgqEJiPUK4cXnqa/a8d8fwcwRJD1jL5RYHnZjhg=", printJobs.Single(x => x.EmailToRecipients[0].SPR_EmailAddress == "AALSHI@test.com").SP_PDFEncryptedPassword);
				AssertEquals("Should equal to the encrypted 'TestPassword2", "2jaXPgqEJiPUK4cXnqa/a8QGgeHIC2+naUYV1FB3TcU=", printJobs.Single(x => x.EmailToRecipients[0].SPR_EmailAddress == "ABIGAS@test.com").SP_PDFEncryptedPassword);
				AssertEquals("Should equal to the encrypted 'TestPassword1", "2jaXPgqEJiPUK4cXnqa/a8d8fwcwRJD1jL5RYHnZjhg=", printJobs.Single(x => x.EmailToRecipients[0].SPR_EmailAddress == "ZECTRA@test.com").SP_PDFEncryptedPassword);

				TestObjectCreator.AALSHI.CustomsCodes.RemoveAndDeleteAll();
				TestObjectCreator.ABIGAS.CustomsCodes.RemoveAndDeleteAll();
				printJobs.ForEach(x => x.Delete());
				Factory.Save();

				new DocumentRunner().Run(command);
				printJobs = Factory.Load<StmPrintJob>(new ZQuery());
				AssertEquals("Should create 3 print job", 3, printJobs.Length);
				AssertEquals("Should create 3 print job", 3, printJobs.Length);
				AssertEquals(true, printJobs.All(x => x.EmailToRecipients.Count == 1));
				AssertEquals("Should equal to the encrypted 'TestPassword3", "2jaXPgqEJiPUK4cXnqa/azEYfJVryfEh5vqrao1P6mY=", printJobs.Single(x => x.EmailToRecipients[0].SPR_EmailAddress == "AALSHI@test.com").SP_PDFEncryptedPassword);
				AssertEquals("Should equal to the encrypted 'TestPassword3", "2jaXPgqEJiPUK4cXnqa/azEYfJVryfEh5vqrao1P6mY=", printJobs.Single(x => x.EmailToRecipients[0].SPR_EmailAddress == "ABIGAS@test.com").SP_PDFEncryptedPassword);
				AssertEquals("Should equal to the encrypted 'TestPassword3", "2jaXPgqEJiPUK4cXnqa/azEYfJVryfEh5vqrao1P6mY=", printJobs.Single(x => x.EmailToRecipients[0].SPR_EmailAddress == "ZECTRA@test.com").SP_PDFEncryptedPassword);
			}
		}

		public void TestGetPDFPasswordCore_ForKRElectronicInvoice_WorkFlowTrigger()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (AccountingConfigurationRegistry.Instance.ElectronicInvoiceDocumentFallbackPassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestPassword1"))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1.0M, TestObjectCreator.ABIGAS);
				TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
				var batch = TestObjectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Sent);
				Assert("PreCondition", arInvoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));
				Factory.Save();

				new ElectronicInvoicingTransactionProxy(arInvoice).MostRecentPivot.AIP_Status = EInvoicingPivotState.Succeed;
				Factory.Save();

				var menuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, EInvoicingKoreaSouthConstants.KRElectronicInvoice));
				var trigger = ((IWorkflowProvider)arInvoice).WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Test Trigger";
				((Enterprise.Integration.ITriggerConditions)trigger).TriggerEventCode = Events.CustomisableEvent00Code;
				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
				notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
				notification.PQ_EmailAddr = "test@test.com";
				notification.PQ_SU_Document = menuItem.PK;
				Factory.Save();

				TestObjectCreator.ABIGAS.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "TestPassword2", CountryCodes.KoreaSouth);
				arInvoice.Logs.AddNew(Events.CustomisableEvent00, "Log1");
				Factory.Save();

				var printJobs = Factory.Load<StmPrintJob>(new ZQuery());
				AssertEquals("Should not create a print job before run Log Walker", 0, printJobs.Length);

				MasterFilesTestHelper.RunLogWalker();
				printJobs = Factory.Load<StmPrintJob>(new ZQuery());
				AssertEquals("Should create a print job after run Log Walker", 1, printJobs.Length);
				AssertEquals("Should equal to the encrypted 'TestPassword2", "2jaXPgqEJiPUK4cXnqa/a8QGgeHIC2+naUYV1FB3TcU=", printJobs[0].SP_PDFEncryptedPassword);

				TestObjectCreator.ABIGAS.CustomsCodes.RemoveAndDeleteAll();
				printJobs.ForEach(x => x.Delete());
				arInvoice.Logs.AddNew(Events.CustomisableEvent00, "Log2");
				Factory.Save();

				MasterFilesTestHelper.RunLogWalker();
				printJobs = Factory.Load<StmPrintJob>(new ZQuery());
				AssertEquals("Should create a print job after run Log Walker", 1, printJobs.Length);
				AssertEquals("Should equal to the encrypted 'TestPassword1", "2jaXPgqEJiPUK4cXnqa/a8d8fwcwRJD1jL5RYHnZjhg=", printJobs[0].SP_PDFEncryptedPassword);
			}
		}

		public void TestKRElectronicInvoiceMenuItemHasMenuPath()
		{
			var command = Factory.LoadTop1<DocumentCommand>(new ZQuery(StmMenuItemSchema.SU_MenuName, EInvoicingKoreaSouthConstants.KRElectronicInvoice));
			AssertEquals(EInvoicingKoreaSouthConstants.ElectronicInvoiceMenuPath, command.SU_MenuPath);
		}

		public void TestGetAdditionalDeliveryContact()
		{
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			ARInvoice.AH_OC_InvoiceContactOverride = contact.PK;
			AssertEquals(DocumentSupporterAR.GetAdditionalDeliveryContact().PK, contact.PK);

			ARInvoice.AH_OC_InvoiceContactOverride = ZGuid.Empty;
			AssertEquals(DocumentSupporterAR.GetAdditionalDeliveryContact(), null);
		}

		public void TestGetOverriddenDeliveryDetails()
		{
			OrgAddress address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "123 Main Street";
			ARInvoice.AH_OA_InvoiceAddressOverride = address.PK;
			AssertNotNull("Overridden delivery address should not be null as invoice address is overridden", DocumentSupporterAR.GetOverriddenDeliveryDetails(string.Empty, null, DocumentDirection.ANY));
			AssertEquals(DocumentSupporterAR.GetOverriddenDeliveryDetails(string.Empty, null, DocumentDirection.ANY).E2_Address1, address.OA_Address1);

			ARInvoice.AH_OA_InvoiceAddressOverride = ZGuid.Empty;
			AssertNull("Overridden delivery address should be null as invoice address is not overridden", DocumentSupporterAR.GetOverriddenDeliveryDetails(string.Empty, null, DocumentDirection.ANY));
		}

		public void TestPortsDirectionAndMode_Shipment()
		{
			CommonShipment shipment1 = CommonShipment.New(Factory);
			shipment1.JS_TransportMode = "SEA";
			shipment1.JS_PackingMode = "FCL";
			shipment1.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment1.JS_RL_NKOrigin = "USLAX";
			Factory.Save();

			Job job1 = Job.CreateWithMutex(Factory, shipment1);
			ARInvoice.AH_JH = job1.PK;
			Factory.Save();

			InvoicingBaseDocumentSupporter documentSupporter = InvoicingBaseDocumentSupporter.New(ARInvoice);
			AssertEquals(GlbBranch.CurrentBranch.GB_RL_NKHomePort, documentSupporter.LocalPort(ContactType.Receivables, DocumentDirection.ARV));
			AssertEquals("USLAX", documentSupporter.ForeignPort(ContactType.Receivables, DocumentDirection.DEP));
			AssertEquals(true, documentSupporter.IsImport);
			AssertEquals("SEA", documentSupporter.TransportMode);
			AssertEquals("FCL", documentSupporter.ContainerMode);

			shipment1.JS_PackingMode = "LCL";
			AssertEquals("SEA", documentSupporter.TransportMode);
			AssertEquals("LCL", documentSupporter.ContainerMode);

			shipment1.JS_PackingMode = "BLK";
			AssertEquals("SEA", documentSupporter.TransportMode);
			AssertEquals("BLK", documentSupporter.ContainerMode);

			CommonShipment shipment2 = CommonShipment.New(Factory);
			shipment2.JS_TransportMode = "AIR";
			shipment1.JS_PackingMode = "LSE";
			shipment2.JS_RL_NKDestination = "INBOM";
			shipment2.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Factory.Save();

			Job job2 = Job.CreateWithMutex(Factory, shipment2);
			ARInvoice.AH_JH = job2.PK;
			Factory.Save();

			documentSupporter = InvoicingBaseDocumentSupporter.New(ARInvoice);
			AssertEquals(GlbBranch.CurrentBranch.GB_RL_NKHomePort, documentSupporter.LocalPort(ContactType.Receivables, DocumentDirection.DEP));
			AssertEquals("INBOM", documentSupporter.ForeignPort(ContactType.Receivables, DocumentDirection.ARV));
			AssertEquals(false, documentSupporter.IsImport);

			AssertEquals("AIR", documentSupporter.TransportMode);
			AssertEquals("LSE", documentSupporter.ContainerMode);
		}

		public void TestPortsDirectionAndMode_Consol()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "USLAX";
			transport.JW_RL_NKDiscPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			consol.JK_UniqueConsignRef = "ZZZZZ11111";
			Factory.Save();
			ARInvoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef + "/A";

			InvoicingBaseDocumentSupporter documentSupporter = InvoicingBaseDocumentSupporter.New(ARInvoice);
			AssertEquals(GlbBranch.CurrentBranch.GB_RL_NKHomePort, documentSupporter.LocalPort(ContactType.Receivables, DocumentDirection.ARV));
			AssertEquals("USLAX", documentSupporter.ForeignPort(ContactType.Receivables, DocumentDirection.ARV));
			AssertEquals(true, documentSupporter.IsImport);

			AssertEquals("AIR", documentSupporter.TransportMode);
			AssertEquals("LSE", documentSupporter.ContainerMode);

			consol.JK_TransportMode = "SEA";
			consol.JK_ConsolMode = "FCL";
			AssertEquals("SEA", documentSupporter.TransportMode);
			AssertEquals("FCL", documentSupporter.ContainerMode);

			consol.JK_ConsolMode = "LCL";
			AssertEquals("SEA", documentSupporter.TransportMode);
			AssertEquals("LCL", documentSupporter.ContainerMode);

			consol.JK_ConsolMode = "BLK";
			AssertEquals("SEA", documentSupporter.TransportMode);
		}

		public void TestPortsDirectionAndMode_ConsolForFirstConsolInvoice()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "USLAX";
			transport.JW_RL_NKDiscPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol.JK_UniqueConsignRef = "ZZZZZ11111";
			Factory.Save();
			ARInvoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;

			InvoicingBaseDocumentSupporter documentSupporter = InvoicingBaseDocumentSupporter.New(ARInvoice);
			AssertEquals(GlbBranch.CurrentBranch.GB_RL_NKHomePort, documentSupporter.LocalPort(ContactType.Receivables, DocumentDirection.DEP));
			AssertEquals("USLAX", documentSupporter.ForeignPort(ContactType.Receivables, DocumentDirection.ARV));
			AssertEquals(true, documentSupporter.IsImport);
			AssertEquals("AIR", documentSupporter.TransportMode);
		}

		public void TestSupportedDataContextsForARInvoice()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CN"))
			{
				AssertEquals("Constants.DataContext.ARInvoice is Supported", true, DocumentSupporterAR.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.ARInvoice)));
				AssertEquals("Constants.DataContext.GenericFreightJob is Supported", true, DocumentSupporterAR.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.GenericFreightJob)));
				AssertEquals("Constants.DataContext.AccountingVoucher is Supported", true, DocumentSupporterAR.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.AccountingVoucher)));
				AssertEquals("Constants.DataContext.JobInvoicingJob is Supported", true, DocumentSupporterAR.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.JobInvoicingJob)));
				AssertEquals("Constants.DataContext.Statement is Supported", true, DocumentSupporterAR.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.Statement)));
				AssertEquals("Constants.DataContext.TransactionHeader is Supported", true, DocumentSupporterAR.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.TransactionHeader)));
				AssertEquals("Constants.DataContext.StatementSummary is Supported", true, DocumentSupporterAR.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.StatementSummary)));
				AssertEquals("Constants.DataContext.APInvoice is Supported", false, DocumentSupporterAR.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.APInvoice)));
				AssertEquals("Constants.DataContext.WhsInvoiceDetail is Supported", false, DocumentSupporterAR.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.WhsInvoiceDetail)));
				AssertEquals("Constants.DataContext.WhsOrdersInvoiceJobHistory is Supported", false, DocumentSupporterAR.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.WhsOrdersInvoiceJobHistory)));
				AssertEquals("Constants.DataContext.WhsReceiveInvoiceJobHistory is Supported", false, DocumentSupporterAR.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.WhsReceiveInvoiceJobHistory)));

				Job job = Factory.NewJobForTesting<Job>();
				job.JH_ParentTableCode = JobStorageSchema.Constants.Prefix;
				ARInvoice.AH_JH = job.PK;

				AssertEquals("Constants.DataContext.ARInvoice is Supported", true, DocumentSupporterAR.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.ARInvoice)));
				AssertEquals("Constants.DataContext.GenericFreightJob is Supported", true, DocumentSupporterAR.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.GenericFreightJob)));
				AssertEquals("Constants.DataContext.AccountingVoucher is Supported", true, DocumentSupporterAR.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.AccountingVoucher)));
				AssertEquals("Constants.DataContext.JobInvoicingJob is Supported", true, DocumentSupporterAR.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.JobInvoicingJob)));
				AssertEquals("Constants.DataContext.Statement is Supported", true, DocumentSupporterAR.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.Statement)));
				AssertEquals("Constants.DataContext.TransactionHeader is Supported", true, DocumentSupporterAR.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.TransactionHeader)));
				AssertEquals("Constants.DataContext.StatementSummary is Supported", true, DocumentSupporterAR.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.StatementSummary)));
				AssertEquals("Constants.DataContext.WhsInvoiceDetail is Supported", true, DocumentSupporterAR.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.WhsInvoiceDetail)));
				AssertEquals("Constants.DataContext.WhsOrdersInvoiceJobHistory is Supported", true, DocumentSupporterAR.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.WhsOrdersInvoiceJobHistory)));
				AssertEquals("Constants.DataContext.WhsReceiveInvoiceJobHistory is Supported", true, DocumentSupporterAR.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.WhsReceiveInvoiceJobHistory)));
				AssertEquals("Constants.DataContext.APInvoice is Supported", false, DocumentSupporterAR.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.APInvoice)));

				DocumentWrapper[] wrapperArray;

				wrapperArray = DocumentSupporterAR.GetDocumentWrappers(Constants.DataContext.ARInvoice, null);
				AssertEquals("Document Supporter BusinessObjects", 1, wrapperArray.Length);

				wrapperArray = DocumentSupporterAR.GetDocumentWrappers(Constants.DataContext.WhsInvoiceDetail, null);
				AssertEquals("Document Supporter BusinessObjects", 1, wrapperArray.Length);

				wrapperArray = DocumentSupporterAR.GetDocumentWrappers(Constants.DataContext.WhsReceiveInvoiceJobHistory, null);
				AssertEquals("Document Supporter BusinessObjects", 1, wrapperArray.Length);

				wrapperArray = DocumentSupporterAR.GetDocumentWrappers(Constants.DataContext.WhsOrdersInvoiceJobHistory, null);
				AssertEquals("Document Supporter BusinessObjects", 1, wrapperArray.Length);
			}
		}

		public void TestSuspendInvoiceCopiesForARInvoice()
		{
			var collection = AccountingConfigurationRegistry.Instance.InvoiceCopies.Value;
			var copy = collection.AddNew();
			copy.Name = (NoResString)"Copy1";
			copy.DeliveryMethod = "ALL";
			AssertEquals(2, collection.Count);

			AccountingConfigurationRegistry.Instance.InvoiceCopies.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, createWithMutex: false);
			ARInvoice.AH_JH = job.PK;

			var wrappers = DocumentSupporterAR.GetDocumentWrappers(Constants.DataContext.ARInvoice, null);
			AssertEquals("Document Supporter BusinessObjects", 2, wrappers.Length);
			AssertContainsExactElementsInAnyOrder(new string[] { null, "Copy1" }, new string[] { GetWrapperCopyInfo(0), GetWrapperCopyInfo(1) });

			Factory.SetContext(BusinessContext.SuspendInvoiceCopies);

			wrappers = DocumentSupporterAR.GetDocumentWrappers(Constants.DataContext.ARInvoice, null);
			AssertEquals("Document Supporter BusinessObjects", 1, wrappers.Length);
			AssertEquals(null, GetWrapperCopyInfo(0));

			string GetWrapperCopyInfo(int index) => ((IBODocDataProvider)wrappers[index]).AdditionalCopyInfo?.Name;
		}

		public void TestSupportedDataContextsForAPInvoice()
		{
			AssertEquals("Constants.DataContext.APInvoice is Supported", true, DocumentSupporterAP.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.APInvoice)));
			AssertEquals("Constants.DataContext.ARInvoice is Supported", false, DocumentSupporterAP.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.ARInvoice)));
			AssertEquals("Constants.DataContext.AccountingVoucher is Supported", false, DocumentSupporterAP.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.AccountingVoucher)));
			AssertEquals("Constants.DataContext.JobInvoicingJob is Supported", false, DocumentSupporterAP.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.JobInvoicingJob)));
			AssertEquals("Constants.DataContext.Statement is Supported", false, DocumentSupporterAP.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.Statement)));
			AssertEquals("Constants.DataContext.TransactionHeader is Supported", false, DocumentSupporterAP.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.TransactionHeader)));
			AssertEquals("Constants.DataContext.StatementSummary is Supported", false, DocumentSupporterAP.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.StatementSummary)));
			AssertEquals("Constants.DataContext.WhsInvoiceDetail is Supported", false, DocumentSupporterAP.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.WhsInvoiceDetail)));
			AssertEquals("Constants.DataContext.WhsOrdersInvoiceJobHistory is Supported", false, DocumentSupporterAP.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.WhsOrdersInvoiceJobHistory)));
			AssertEquals("Constants.DataContext.WhsReceiveInvoiceJobHistory is Supported", false, DocumentSupporterAP.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.WhsReceiveInvoiceJobHistory)));

			DocumentWrapper[] wrapperArray;

			InvoiceCopyCollection collection = new InvoiceCopyCollection();
			InvoiceCopy copy = collection.AddNew();
			copy.Name = (NoResString)"copy1";
			copy.DeliveryMethod = "ALL";
			copy = collection.AddNew();
			copy.Name = (NoResString)"copy2";
			copy.DeliveryMethod = "ALL";
			AccountingConfigurationRegistry.Instance.InvoiceCopies.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			wrapperArray = DocumentSupporterAP.GetDocumentWrappers(Constants.DataContext.APInvoice, null);
			AssertNotNull("Wrapper should exist", wrapperArray[0]);
			AssertEquals("Wrapper must ignore invoice copy registry item", 1, wrapperArray.Length);
		}

		public void TestMenuTemplateFilterValuesForPrintStandardInvoice()
		{
			ZString result;
			result = DocumentSupporterAR.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintStandardInvoice, DummyDocumentBusinessObjectWrapper.New(ARInvoice));
			AssertEquals("Filter result for printing Standard Invoice", new ZString("Y"), result);
		}

		public void TestMenuTemplateFilterValuesForPrintClientSpecificInvoice()
		{
			ZString result;
			result = DocumentSupporterAR.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintClientSpecificInvoice, DummyDocumentBusinessObjectWrapper.New(ARInvoice));
			AssertEquals("Filter result for printing Client Specific Invoice", new ZString("N"), result);
		}

		public void TestMenuTemplateFilterValuesForPrintClientSpecificTrailingPage()
		{
			ZString result;
			result = DocumentSupporterAR.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintClientSpecificTrailingPage, DummyDocumentBusinessObjectWrapper.New(ARInvoice));
			AssertEquals("Filter result for printing Client Specific Trailing (2nd) Page", new ZString("N"), result);
		}

		public void TestGetDocumentTitlesForPivot()
		{
			string[] invoiceDocuments = { "Invoice", "DocBuilder Invoice" };
			ZQuery query = new ZQuery(StmMenuTemplatePivotSchema.SI_DocumentTitle, "Invoice");
			query.AddToFilter(StmMenuTemplatePivotSchema.SI_IsClientSpecific, ZBool.False);
			StmMenuTemplatePivot pivot = Factory.LoadTop1<StmMenuTemplatePivot>(query);
			AssertNotNull("Precondition: pivot should not be null", pivot);

			foreach (var document in invoiceDocuments)
			{
				AssertGetDocumentTitleForPivot(document, pivot, false, null);
				AssertGetDocumentTitleForPivot(document, pivot, true, "Pro Forma Invoice");
				ARInvoice.IsPrintingProformaInvoice = false;
			}

			InvoiceCopyCollection invoiceCopies = (InvoiceCopyCollection)AccountingConfigurationRegistry.Instance.InvoiceCopies.Value.Clone(null, null);
			foreach (InvoiceCopy entry in invoiceCopies)
			{
				if (entry.IsOriginal)
				{
					entry.Name = (NoResString)"Test";
				}
			}
			AccountingConfigurationRegistry.Instance.InvoiceCopies.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, invoiceCopies);
			foreach (var document in invoiceDocuments)
			{
				AssertGetDocumentTitleForPivot(document, pivot, false, "Test");
				AssertGetDocumentTitleForPivot(document, pivot, true, "Test");
				ARInvoice.IsPrintingProformaInvoice = false;
			}
		}

		void AssertGetDocumentTitleForPivot(string document, StmMenuTemplatePivot pivot, bool isPrintingProformaInvoice, object expectedValue)
		{
			ARInvoice.IsPrintingProformaInvoice = isPrintingProformaInvoice;
			TitleCopyCountPair titleCopyCountPair = DocumentSupporterAR.GetDocumentTitlesForPivot(document, ARInvoice, pivot);
			if (expectedValue == null)
			{
				AssertNull("TitleCopyCountPair should be null", titleCopyCountPair);
			}
			else
			{
				AssertEquals(string.Format("GetDocumentTitlesForPivot should return {0}", expectedValue), expectedValue, titleCopyCountPair.Title);
			}
		}

		public void TestGetDocBusinessObjectsWithCopies()
		{
			IBODocDataProvider[] docDataProviders = DocumentSupporterAR.GetDocumentWrappers(Constants.DataContext.ARInvoice, null);
			AssertEquals("By default, should just have the original, no copies", 1, docDataProviders.Length);

			InvoiceCopyCollection invoiceCopies = (InvoiceCopyCollection)AccountingConfigurationRegistry.Instance.InvoiceCopies.Value.Clone(null, null);

			InvoiceCopy copy1 = invoiceCopies.AddNew();
			copy1.IncludeTradingTerms = true;
			copy1.Name = (NoResString)"Test Copy";
			copy1.DeliveryMethod = "EML";
			copy1.Message = "message 01";

			InvoiceCopy copy2 = invoiceCopies.AddNew();
			copy2.IncludeTradingTerms = false;
			copy2.Name = (NoResString)"Another Test Copy";
			copy2.DeliveryMethod = "FAX";
			copy2.Message = "message 02";

			AccountingConfigurationRegistry.Instance.InvoiceCopies.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, invoiceCopies);

			docDataProviders = DocumentSupporterAR.GetDocumentWrappers(Constants.DataContext.ARInvoice, null);
			AssertEquals("Should be original + same number of copies as in registry", 3, docDataProviders.Length);

			AssertNull("Original has no Copy Info", docDataProviders[0].AdditionalCopyInfo);

			AssertNotNull("First copy has copy info", docDataProviders[1].AdditionalCopyInfo);
			AssertEquals("CopyCount", (short)1, docDataProviders[1].AdditionalCopyInfo.CopyCount);
			AssertEquals("Trading Terms", true, ((ARInvoiceDocWrapperCopyInfo)docDataProviders[1].AdditionalCopyInfo).IncludeTradingTerms);
			AssertEquals("Print Copy Type", PrintCopyType.EML, docDataProviders[1].AdditionalCopyInfo.DeliveryMethod);
			AssertEquals("Name", "Test Copy", docDataProviders[1].AdditionalCopyInfo.Name);
			AssertEquals("message 01", ((ARInvoiceDocWrapperCopyInfo)docDataProviders[1].AdditionalCopyInfo).Message);

			AssertNotNull("Second copy has copy info", docDataProviders[2].AdditionalCopyInfo);
			AssertEquals("CopyCount", (short)1, docDataProviders[2].AdditionalCopyInfo.CopyCount);
			AssertEquals("Trading Terms", false, ((ARInvoiceDocWrapperCopyInfo)docDataProviders[2].AdditionalCopyInfo).IncludeTradingTerms);
			AssertEquals("Print Copy Type", PrintCopyType.FAX, docDataProviders[2].AdditionalCopyInfo.DeliveryMethod);
			AssertEquals("Name", "Another Test Copy", docDataProviders[2].AdditionalCopyInfo.Name);
			AssertEquals("message 02", ((ARInvoiceDocWrapperCopyInfo)docDataProviders[2].AdditionalCopyInfo).Message);
		}

		public void TestAPDocumentPrintOnceAndARMultipleCopies()
		{
			InvoiceCopyCollection invoiceCopies = (InvoiceCopyCollection)AccountingConfigurationRegistry.Instance.InvoiceCopies.Value.Clone(null, null);

			InvoiceCopy copy1 = invoiceCopies.AddNew();
			copy1.IncludeTradingTerms = true;
			copy1.Name = (NoResString)"Test Copy";
			copy1.DeliveryMethod = "EML";

			InvoiceCopy copy2 = invoiceCopies.AddNew();
			copy2.IncludeTradingTerms = false;
			copy2.Name = (NoResString)"Another Test Copy";
			copy2.DeliveryMethod = "FAX";

			AccountingConfigurationRegistry.Instance.InvoiceCopies.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, invoiceCopies);

			DocumentWrapper[] wrapperAR = DocumentSupporterAR.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, null);
			DocumentWrapper[] wrapperAP = DocumentSupporterAP.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, null);
			AssertEquals("Should be original + same number of copies as in registry", 3, wrapperAR.Length);
			AssertEquals("Should be original", 1, wrapperAP.Length);
		}

		public void TestGenericFreightJobInvoiceIsSupported()
		{
			DocumentWrapper[] wrapperAR = DocumentSupporterAR.GetDocumentWrappers(Constants.DataContext.GenericFreightJobInvoice, null);
			AssertNotNull("Should support GenericFreightJobInvoice context", wrapperAR);
			AssertEquals("Should support GenericFreightJobInvoice context", 1, wrapperAR.Length);
		}

		public void TestITAutofatturaDocumentRequirements()
		{
			APInvoice.AH_TransactionNum = "00001";
			APInvoice.AH_TransactionType = TransactionTypes.Invoice;
			APInvoice.AH_ComplianceSubType = ItalyComplianceInfo.ComplianceSubTypeCodes.APS;
			APInvoice.AH_TransactionReference = "TR00001";

			var menuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, JobInvoicingEDocsProviderSupporter.ITAutofattura));
			Factory.Save();

			var errorMessage = "Autofattura (IT) can only be printed for Invoice, Credit Note and Adjustment transactions that have a Compliance Sub Type = APS and allocated Compliance Number";

			using (AccountingConfigurationRegistry.Instance.ARInvoiceMenuItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, menuItem.PK.ToGuid()))
			{
				var documentSupporterDataState = DocumentSupporterAP.GetDataStateBeforeRun(menuItem);
				AssertEquals("Document can be printed (No error message dialog)", true, documentSupporterDataState.IsValid);

				APInvoice.AH_TransactionType = TransactionTypes.AdjustmentNote;
				documentSupporterDataState = DocumentSupporterAP.GetDataStateBeforeRun(menuItem);
				AssertEquals("Document can be printed (No error message dialog)", true, documentSupporterDataState.IsValid);

				APInvoice.AH_TransactionType = TransactionTypes.CreditNote;
				documentSupporterDataState = DocumentSupporterAP.GetDataStateBeforeRun(menuItem);
				AssertEquals("Document can be printed (No error message dialog)", true, documentSupporterDataState.IsValid);

				APInvoice.AH_TransactionType = TransactionTypes.DirectPayment;
				documentSupporterDataState = DocumentSupporterAP.GetDataStateBeforeRun(menuItem);
				AssertEquals("Error message is shown (AH_TransactionType != 'INV'/'ADJ'/'CRD')", documentSupporterDataState.ErrorMessage, errorMessage);

				APInvoice.AH_TransactionType = TransactionTypes.Invoice;
				APInvoice.AH_ComplianceSubType = ItalyComplianceInfo.ComplianceSubTypeCodes.API;
				AssertEquals("Error message is shown (AH_ComplianceSubType != 'APS')", documentSupporterDataState.ErrorMessage, errorMessage);

				APInvoice.AH_ComplianceSubType = ItalyComplianceInfo.ComplianceSubTypeCodes.APS;
				APInvoice.AH_TransactionReference = "";
				AssertEquals("Error message is shown (Compliance Number is empty/not allocated)", documentSupporterDataState.ErrorMessage, errorMessage);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			ARInvoice = Factory.NewWithValidTestData<ARInvoice>();
			AssertNotNull("ARInvoice should not be null", ARInvoice);
			DocumentSupporterAR = InvoicingBaseDocumentSupporter.New(ARInvoice);
			AssertNotNull("Document Supporter AP should not be null", DocumentSupporterAR);

			APInvoice = Factory.NewWithValidTestData<APInvoice>();
			AssertNotNull("APInvoice should not be null", APInvoice);
			DocumentSupporterAP = InvoicingBaseDocumentSupporter.New(APInvoice);
			AssertNotNull("Document Supporter AP should not be null", DocumentSupporterAP);
		}

		InvoicingBaseDocumentSupporter DocumentSupporterAP;
		APInvoice APInvoice;
		InvoicingBaseDocumentSupporter DocumentSupporterAR;
		ARInvoice ARInvoice;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
