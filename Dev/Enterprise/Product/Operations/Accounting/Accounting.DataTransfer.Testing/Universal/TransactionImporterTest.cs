using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.Testing;
using Enterprise.Accounting.DataTransfer.Universal;
using Enterprise.Accounting.Export;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Accounting.Business.TestObjectCreator;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;
using Constants = Enterprise.Core.Constants;
using Country = Enterprise.UniversalDataBuss.DataObjects.Universal.Country;
using Currency = Enterprise.UniversalDataBuss.DataObjects.Universal.Currency;
using UniversalCodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;

namespace Enterprise.Accounting.DataTransfer.Testing.Universal
{
	partial class TransactionImporterTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestImportTransactionLinesWithInvalidConsolCost_NoExceptions()
		{
			var shipment = TestObjectCreator.CreateShipment("C001");

			shipment.JS_HouseBill = "HouseBill1";
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			Factory.Save();

			var importer = new TransactionImporter();
			var invoice = new BusinessObjectFactory().New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.IsFinalCharge = true;
			universalLine.OSAmount = -10;
			universalLine.Job = new EntityReference { Key = "Job1", Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			universalLine.CostSource = new EntityReference { Key = "Job1", Type = nameof(DataContextType.ForwardingShipment) };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "Job1");
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			universalShipment.WayBillNumber = shipment.JS_HouseBill;
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var universalTransactionXml = universalTransaction.Serialize();
			importer.ImportTransactionLines(universalTransactionXml, invoice, false);

			AssertEquals(1, invoice.Lines.Count);
			var line = invoice.Lines[0];
			AssertHasWarning(line.ConsolIDFromApportionedChargeInfo, "Source XML errors: Found ForwardingShipment with number 'Job1' is not a consolidation and can't be used for consol costing.");

			invoice.ReleaseAllMutexOnInvoice();
		}

		public void TestImportARTransactionWithJobShipmentDataTargetAndSource_NoExceptions()
		{
			var shipment = TestObjectCreator.CreateShipment("C001");
			var job1 = TestObjectCreator.CreateJob(shipment);
			job1.JH_GE = TestObjectCreator.FEADepartment.PK;
			Factory.Save();

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.None),
				Address1 = "street",
				OrganizationCode = TestObjectCreator.Debtor.OH_Code,
			};
			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				Ledger = LedgerTypes.AccountsReceivable,
				TransactionType = TransactionType.INV,
				OrganizationAddress = orgAddress,
				Number = "1234",
				OSExGSTVATAmount = 100,
				Branch = new Branch { Code = TestObjectCreator.NonCurrentBranch.GB_Code },
				Department = new Department { Code = TestObjectCreator.NonCurrentDepartment.GE_Code },
				BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), OrganizationCode = TestObjectCreator.NonCurrentCompany.OrgProxy.OH_Code }
			};
			universalTransaction.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ORP } } });
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.RevenueChargeCode.AC_Code };
			universalLine.OSAmount = 100;
			universalLine.Job = new EntityReference { Key = "C001", Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataTarget(DataContextType.ForwardingShipment, "C001");
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "C001");
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var importer = new TransactionImporter();
			var message = new BusinessObjectFactory().New<EDIMessage>();
			message.EM_GB = TestObjectCreator.NonCurrentBranch.PK;
			message.EM_GE = TestObjectCreator.NonCurrentDepartment.PK;
			message.EM_MessageText = universalTransaction.Serialize();

			var result = false;
			AssertNoExceptionThrown(() => result = importer.ImportTransaction(message, universalTransaction, new XmlSessionTracker(new ServiceTaskLogForTesting()), new UniversalObjectFactory()));
			Assert(result);
		}

		#region SetBranchAndDepartmentOnUXMLImport

		public void TestSetBranchAndDepartmentOnUXMLImport_MiscInvoice()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-1));
			TestObjectCreator.NonCurrentCompany.OrgProxy.OH_IsCreditor = true;

			Factory.Save();

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.None),
				Address1 = "street",
				OrganizationCode = TestObjectCreator.Debtor.OH_Code,
			};

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				Ledger = LedgerTypes.AccountsReceivable,
				OrganizationAddress = orgAddress,
				Number = "1234",
				OSExGSTVATAmount = 100,
				Branch = new Branch { Code = TestObjectCreator.NonCurrentCompanyBranch.GB_Code },
				Department = new Department { Code = TestObjectCreator.MiscDepartment.GE_Code },
				BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), OrganizationCode = TestObjectCreator.NonCurrentCompany.OrgProxy.OH_Code }
			};
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal> { new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) });
			AssertNull("Precondition: DataProvider", ((DataContext)universalTransaction.DataContext).DataProvider);
			universalTransaction.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.IDB } } });
			Assert("Precondition: HasRecipientRole", universalTransaction.HasRecipientRole(RecipientRoleType.IDB));

			var importer = new TransactionImporter();
			var message = new BusinessObjectFactory().New<EDIMessage>();
			message.EM_GB = TestObjectCreator.NonCurrentBranch.PK;
			message.EM_GE = TestObjectCreator.NonCurrentDepartment.PK;
			message.EM_MessageText = universalTransaction.Serialize();

			var universalFactory = new UniversalObjectFactory();
			var serviceLogger = new ServiceTaskLogForTesting();
			var logger = new XmlSessionTracker(serviceLogger);
			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			var createdInvoices = universalFactory.BOFactory.Load<TransactionPendingAllocation>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			var newFactory = new BusinessObjectFactory();
			var savedInvoice = newFactory.Load<TransactionPendingAllocation>(createdInvoices[0].PK);
			AssertEquals("Branch should be transaction default value as consol is not found.", GlbBranch.CurrentBranch.PK, savedInvoice.AH_GB);
			AssertEquals("Department should be original one.", TestObjectCreator.MiscDepartment.PK, savedInvoice.AH_GE);
			Assert("logger.HasErrors", !logger.HasErrors);
			Assert("logger.HasWarnings", logger.HasWarnings);
			var expectedBranchWarning = "Transaction Branch is set to the message branch because Transaction Branch cannot be set with reference to the invoice debtor organization proxy.";
			AssertContains(expectedBranchWarning, logger.ToString());
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
			AssertNotNull("Postcondition: request is created.", savedInvoice.TransactionApprovalRequest);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, savedInvoice.TransactionApprovalRequest.XP_ApprovalStatus);

			var invoice = new BusinessObjectFactory().Load<APInvoice>(savedInvoice.PK);
			invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.SubmittedFromInvoicingForm = true;

			importer.ImportTransactionLines(savedInvoice.TransactionApprovalRequest.PostingDetails.SourceXML, invoice, true);
			AssertEquals(1, invoice.Lines.Count);
			AssertEquals("Branch should be from header.", savedInvoice.AH_GB, invoice.Lines[0].AL_GB);
			AssertEquals("Department should be from header.", savedInvoice.AH_GE, invoice.Lines[0].AL_GE);
		}

		public void TestSetBranchAndDepartmentOnUXMLImport()
		{
			//test all cases from IntercompanyTransactionImportHelperTest.TestSetBranchAndDepartmentOnUXMLImport test, but here it will be real transaction and it will check how we pass paameters to the helper.

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-1));
			TestObjectCreator.NonCurrentCompany.OrgProxy.OH_IsCreditor = true;

			var shipment1 = TestObjectCreator.CreateShipment("S001");
			var job1 = TestObjectCreator.CreateJob(shipment1);
			job1.JH_GE = TestObjectCreator.FEADepartment.PK;

			Factory.Save();

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				Ledger = LedgerTypes.AccountsReceivable,
				OSExGSTVATAmount = 100,
				BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), OrganizationCode = TestObjectCreator.NonCurrentCompany.OrgProxy.OH_Code }
			};
			AssertNull("Precondition: DataProvider", ((DataContext)universalTransaction.DataContext).DataProvider);
			universalTransaction.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.IDB } } });
			Assert("Precondition: HasRecipientRole", universalTransaction.HasRecipientRole(RecipientRoleType.IDB));

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.None),
				Address1 = "street"
			};
			universalTransaction.OrganizationAddress = orgAddress;
			universalTransaction.Number = "1234";
			universalTransaction.Branch = new Branch { Code = TestObjectCreator.NonCurrentCompanyBranch.GB_Code };
			universalTransaction.Department = new Department { Code = TestObjectCreator.MiscDepartment.GE_Code };

			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { Job = new EntityReference { Key = "Job1", Type = AccountingDataTransferConstants.DataContextTypeString.Job } };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalShipment = new Shipment { DataContext = DataContextFactory.New() };
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "Job1");
			universalShipment.DataContext.AddDataTarget(DataContextType.ForwardingShipment, shipment1.JS_UniqueConsignRef);
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var importer = new TransactionImporter();
			var message = new BusinessObjectFactory().New<EDIMessage>();
			message.EM_GB = TestObjectCreator.NonCurrentBranch.PK;
			message.EM_GE = TestObjectCreator.NonCurrentDepartment.PK;

			var universalFactory = new UniversalObjectFactory();
			var serviceLogger = new ServiceTaskLogForTesting();
			var logger = new XmlSessionTracker(serviceLogger);
			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			var createdInvoices = universalFactory.BOFactory.Load<TransactionPendingAllocation>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			var newFactory = new BusinessObjectFactory();
			var savedInvoice = newFactory.Load<TransactionPendingAllocation>(createdInvoices[0].PK);
			AssertEquals("Branch should be transaction default value as debtor is not set in sourceWrapper.", GlbBranch.CurrentBranch.PK, savedInvoice.AH_GB);
			AssertEquals("Department should be from JobDepartment.", TestObjectCreator.FEADepartment.PK, savedInvoice.AH_GE);
			Assert("logger.HasErrors", !logger.HasErrors);
			Assert("logger.HasWarnings", logger.HasWarnings);
			var expectedBranchWarning = "Transaction Branch is set to the message branch because Transaction Branch cannot be set with reference to the invoice debtor organization proxy.";
			AssertContains(expectedBranchWarning, logger.ToString());
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
			AssertNotNull("Postcondition: request is created.", savedInvoice.TransactionApprovalRequest);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, savedInvoice.TransactionApprovalRequest.XP_ApprovalStatus);

			TestObjectCreator.CreateBranch("NB1", TestObjectCreator.NonCurrentCompany, TestObjectCreator.Agent);
			TestObjectCreator.CreateBranch("NB2", TestObjectCreator.NonCurrentCompany, TestObjectCreator.Agent);
			var branch1 = TestObjectCreator.CreateBranch("CB1", GlbCompany.CurrentCompany);
			job1.JH_GB = branch1.PK;
			Factory.Save();
			orgAddress.OrganizationCode = TestObjectCreator.Agent.OH_Code;
			universalTransaction.Number = string.Format(CultureInfo.InvariantCulture, "{0}_2", universalTransaction.Number);
			universalFactory = new UniversalObjectFactory();
			serviceLogger = new ServiceTaskLogForTesting();
			logger = new XmlSessionTracker(serviceLogger);
			result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			createdInvoices = universalFactory.BOFactory.Load<TransactionPendingAllocation>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			newFactory = new BusinessObjectFactory();
			savedInvoice = newFactory.Load<TransactionPendingAllocation>(createdInvoices[0].PK);
			AssertEquals("Branch should be from job branch.", branch1.PK, savedInvoice.AH_GB);
			AssertEquals("Department should be from JobDepartment.", TestObjectCreator.FEADepartment.PK, savedInvoice.AH_GE);
			Assert("logger.HasErrors", !logger.HasErrors);
			Assert("logger.HasWarnings", !logger.HasWarnings);
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
			AssertNotNull("Postcondition: request is created.", savedInvoice.TransactionApprovalRequest);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, savedInvoice.TransactionApprovalRequest.XP_ApprovalStatus);

			var branch2 = TestObjectCreator.CreateBranch("CB2", GlbCompany.CurrentCompany, TestObjectCreator.LocalClient);
			var branch3 = TestObjectCreator.CreateBranch("CB3", GlbCompany.CurrentCompany, TestObjectCreator.Debtor);
			var branch4 = TestObjectCreator.CreateBranch("CB4", GlbCompany.CurrentCompany, TestObjectCreator.Debtor);

			orgAddress.OrganizationCode = TestObjectCreator.Debtor.OH_Code;

			var consol = TestObjectCreator.CreateConsol();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_OA_SendingForwarderAddress = TestObjectCreator.LocalClient.MainAddress.PK;
			var shipment2 = TestObjectCreator.CreateShipment("S0002", consol);
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			Factory.Save();

			universalTransaction.JobInvoiceNumber = "Consol1/AA";
			var universalConsol = new Shipment { DataContext = DataContextFactory.New() };
			universalConsol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "Consol1");
			universalConsol.DataContext.AddDataTarget(DataContextType.ForwardingConsol, "C001");
			universalTransaction.ShipmentCollection.Add(universalConsol);
			universalTransaction.Number = string.Format(CultureInfo.InvariantCulture, "{0}_3", universalTransaction.Number);
			universalFactory = new UniversalObjectFactory();
			serviceLogger = new ServiceTaskLogForTesting();
			logger = new XmlSessionTracker(serviceLogger);
			result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			createdInvoices = universalFactory.BOFactory.Load<TransactionPendingAllocation>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			newFactory = new BusinessObjectFactory();
			savedInvoice = newFactory.Load<TransactionPendingAllocation>(createdInvoices[0].PK);
			AssertEquals("Branch should be calculated from consol.", branch2.PK, savedInvoice.AH_GB);
			AssertEquals("Department should be calculated from consol.", TestObjectCreator.FESDepartment.PK, savedInvoice.AH_GE);
			Assert("logger.HasErrors", !logger.HasErrors);
			Assert("logger.HasWarnings", !logger.HasWarnings);
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
			AssertNotNull("Postcondition: request is created.", savedInvoice.TransactionApprovalRequest);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, savedInvoice.TransactionApprovalRequest.XP_ApprovalStatus);

			var branch5 = TestObjectCreator.CreateBranch("CB5", GlbCompany.CurrentCompany, TestObjectCreator.LocalClient2);
			orgAddress.OrganizationCode = TestObjectCreator.LocalClient2.OH_Code;
			universalTransaction.Number = string.Format(CultureInfo.InvariantCulture, "{0}_4", universalTransaction.Number);
			universalFactory = new UniversalObjectFactory();
			serviceLogger = new ServiceTaskLogForTesting();
			logger = new XmlSessionTracker(serviceLogger);
			result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			createdInvoices = universalFactory.BOFactory.Load<TransactionPendingAllocation>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			newFactory = new BusinessObjectFactory();
			savedInvoice = newFactory.Load<TransactionPendingAllocation>(createdInvoices[0].PK);
			AssertEquals("Branch should be calculated org proxy from debtor.", branch5.PK, savedInvoice.AH_GB);
			AssertEquals("Department should be calculated from consol as consol is not found.", TestObjectCreator.FESDepartment.PK, savedInvoice.AH_GE);
			Assert("logger.HasErrors", !logger.HasErrors);
			Assert("logger.HasWarnings", !logger.HasWarnings);
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
			AssertNotNull("Postcondition: request is created.", savedInvoice.TransactionApprovalRequest);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, savedInvoice.TransactionApprovalRequest.XP_ApprovalStatus);

			consol.JK_TransportMode = "";
			Factory.Save();
			universalTransaction.Number = string.Format(CultureInfo.InvariantCulture, "{0}_5", universalTransaction.Number);
			universalFactory = new UniversalObjectFactory();
			serviceLogger = new ServiceTaskLogForTesting();
			logger = new XmlSessionTracker(serviceLogger);
			result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			createdInvoices = universalFactory.BOFactory.Load<TransactionPendingAllocation>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			newFactory = new BusinessObjectFactory();
			savedInvoice = newFactory.Load<TransactionPendingAllocation>(createdInvoices[0].PK);
			AssertEquals("Branch should be calculated org proxy from debtor.", branch5.PK, savedInvoice.AH_GB);
			AssertEquals("Consol department can't be calculated and so it is original department.", TestObjectCreator.MiscDepartment.PK, savedInvoice.AH_GE);
			Assert("logger.HasErrors", !logger.HasErrors);
			Assert("logger.HasWarnings", logger.HasWarnings);
			AssertNotContains(expectedBranchWarning, logger.ToString());
			var expectedDepartmentWarning = "Intercompany Transaction Department is used because Department cannot be set with reference to the Consolidation 'C001'.";
			AssertContains(expectedDepartmentWarning, logger.ToString());
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
			AssertNotNull("Postcondition: request is created.", savedInvoice.TransactionApprovalRequest);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, savedInvoice.TransactionApprovalRequest.XP_ApprovalStatus);
		}

		public void TestSetBranchAndDepartmentOnUXMLImport_ConsolNotFound()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-1));
			TestObjectCreator.NonCurrentCompany.OrgProxy.OH_IsCreditor = true;

			var branch2 = TestObjectCreator.CreateBranch("CB2", GlbCompany.CurrentCompany, TestObjectCreator.LocalClient);
			var branch3 = TestObjectCreator.CreateBranch("CB3", GlbCompany.CurrentCompany, TestObjectCreator.Debtor);
			var branch4 = TestObjectCreator.CreateBranch("CB4", GlbCompany.CurrentCompany, TestObjectCreator.Debtor);

			var consol = TestObjectCreator.CreateConsol();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_OA_SendingForwarderAddress = TestObjectCreator.LocalClient.MainAddress.PK;
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);

			Factory.Save();

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.None),
				Address1 = "street",
				OrganizationCode = TestObjectCreator.Debtor.OH_Code,
			};

			var universalConsol = new Shipment { DataContext = DataContextFactory.New() };
			universalConsol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "Consol1");

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				Ledger = LedgerTypes.AccountsReceivable,
				OrganizationAddress = orgAddress,
				Number = "1234",
				JobInvoiceNumber = "Consol1/AA",
				OSExGSTVATAmount = 100,
				Branch = new Branch { Code = TestObjectCreator.NonCurrentCompanyBranch.GB_Code },
				Department = new Department { Code = TestObjectCreator.MiscDepartment.GE_Code },
				BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), OrganizationCode = TestObjectCreator.NonCurrentCompany.OrgProxy.OH_Code }
			};
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal> { new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) });
			universalTransaction.SetShipmentCollection(() => new List<Shipment> { universalConsol });

			AssertNull("Precondition: DataProvider", ((DataContext)universalTransaction.DataContext).DataProvider);
			universalTransaction.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.IDB } } });
			Assert("Precondition: HasRecipientRole", universalTransaction.HasRecipientRole(RecipientRoleType.IDB));

			var importer = new TransactionImporter();
			var message = new BusinessObjectFactory().New<EDIMessage>();
			message.EM_GB = TestObjectCreator.NonCurrentBranch.PK;
			message.EM_GE = TestObjectCreator.NonCurrentDepartment.PK;

			var universalFactory = new UniversalObjectFactory();
			var serviceLogger = new ServiceTaskLogForTesting();
			var logger = new XmlSessionTracker(serviceLogger);
			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			var createdInvoices = universalFactory.BOFactory.Load<TransactionPendingAllocation>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			var newFactory = new BusinessObjectFactory();
			var savedInvoice = newFactory.Load<TransactionPendingAllocation>(createdInvoices[0].PK);
			AssertEquals("Branch should be transaction default value as consol is not found.", GlbBranch.CurrentBranch.PK, savedInvoice.AH_GB);
			AssertEquals("Department should be original one.", TestObjectCreator.MiscDepartment.PK, savedInvoice.AH_GE);
			Assert("logger.HasErrors", !logger.HasErrors);
			Assert("logger.HasWarnings", logger.HasWarnings);
			var expectedBranchWarning = "Transaction Branch is set to the message branch because Transaction Branch cannot be set with reference to the invoice debtor organization proxy.";
			AssertContains(expectedBranchWarning, logger.ToString());
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
			AssertNotNull("Postcondition: request is created.", savedInvoice.TransactionApprovalRequest);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, savedInvoice.TransactionApprovalRequest.XP_ApprovalStatus);

			universalConsol.DataContext.AddDataTarget(DataContextType.ForwardingConsol, "C001");
			universalTransaction.Number = string.Format(CultureInfo.InvariantCulture, "{0}_2", universalTransaction.Number);
			universalFactory = new UniversalObjectFactory();
			serviceLogger = new ServiceTaskLogForTesting();
			logger = new XmlSessionTracker(serviceLogger);
			result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			createdInvoices = universalFactory.BOFactory.Load<TransactionPendingAllocation>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			newFactory = new BusinessObjectFactory();
			savedInvoice = newFactory.Load<TransactionPendingAllocation>(createdInvoices[0].PK);
			AssertEquals("Branch should be calculated from consol.", branch2.PK, savedInvoice.AH_GB);
			AssertEquals("Department should be calculated from consol.", TestObjectCreator.FESDepartment.PK, savedInvoice.AH_GE);
			Assert("logger.HasErrors", !logger.HasErrors);
			Assert("logger.HasWarnings", !logger.HasWarnings);
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
			AssertNotNull("Postcondition: request is created.", savedInvoice.TransactionApprovalRequest);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, savedInvoice.TransactionApprovalRequest.XP_ApprovalStatus);
		}

		public void TestSetBranchAndDepartmentOnUXMLImport_DebtorNotFound()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-1));
			TestObjectCreator.NonCurrentCompany.OrgProxy.OH_IsCreditor = true;
			Factory.Save();

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				Ledger = LedgerTypes.AccountsReceivable,
				OSExGSTVATAmount = 100,
				BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), OrganizationCode = TestObjectCreator.NonCurrentCompany.OrgProxy.OH_Code }
			};
			AssertNull("Precondition: DataProvider", ((DataContext)universalTransaction.DataContext).DataProvider);
			universalTransaction.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.IDB } } });
			Assert("Precondition: HasRecipientRole", universalTransaction.HasRecipientRole(RecipientRoleType.IDB));

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.None),
				Address1 = "street"
			};
			universalTransaction.OrganizationAddress = orgAddress;
			universalTransaction.Number = "1234";
			universalTransaction.Branch = new Branch { Code = TestObjectCreator.NonCurrentCompanyBranch.GB_Code };
			universalTransaction.Department = new Department { Code = TestObjectCreator.MiscDepartment.GE_Code };
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal> { new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) });

			var importer = new TransactionImporter();
			var message = new BusinessObjectFactory().New<EDIMessage>();
			message.EM_GB = TestObjectCreator.NonCurrentBranch.PK;
			message.EM_GE = TestObjectCreator.NonCurrentDepartment.PK;

			var universalFactory = new UniversalObjectFactory();
			var serviceLogger = new ServiceTaskLogForTesting();
			var logger = new XmlSessionTracker(serviceLogger);
			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			var createdInvoices = universalFactory.BOFactory.Load<TransactionPendingAllocation>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			var newFactory = new BusinessObjectFactory();
			var savedInvoice = newFactory.Load<TransactionPendingAllocation>(createdInvoices[0].PK);
			AssertEquals("Branch should be transaction default value as debtor is not set in sourceWrapper.", GlbBranch.CurrentBranch.PK, savedInvoice.AH_GB);
			AssertEquals("Department should be original one.", TestObjectCreator.MiscDepartment.PK, savedInvoice.AH_GE);
			Assert("logger.HasErrors", !logger.HasErrors);
			Assert("logger.HasWarnings", logger.HasWarnings);
			var expectedBranchWarning = "Transaction Branch is set to the message branch because Transaction Branch cannot be set with reference to the invoice debtor organization proxy.";
			AssertContains(expectedBranchWarning, logger.ToString());
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
			AssertNotNull("Postcondition: request is created.", savedInvoice.TransactionApprovalRequest);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, savedInvoice.TransactionApprovalRequest.XP_ApprovalStatus);

			var branch1 = TestObjectCreator.CreateBranch("CB1", GlbCompany.CurrentCompany, TestObjectCreator.Agent);
			orgAddress.OrganizationCode = TestObjectCreator.Agent.OH_Code;
			universalTransaction.Number = string.Format(CultureInfo.InvariantCulture, "{0}_2", universalTransaction.Number);
			universalFactory = new UniversalObjectFactory();
			serviceLogger = new ServiceTaskLogForTesting();
			logger = new XmlSessionTracker(serviceLogger);
			result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			createdInvoices = universalFactory.BOFactory.Load<TransactionPendingAllocation>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			newFactory = new BusinessObjectFactory();
			savedInvoice = newFactory.Load<TransactionPendingAllocation>(createdInvoices[0].PK);
			AssertEquals("Branch should be from job branch.", branch1.PK, savedInvoice.AH_GB);
			AssertEquals("Department should be original one.", TestObjectCreator.MiscDepartment.PK, savedInvoice.AH_GE);
			Assert("logger.HasErrors", !logger.HasErrors);
			Assert("logger.HasWarnings", !logger.HasWarnings);
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
			AssertNotNull("Postcondition: request is created.", savedInvoice.TransactionApprovalRequest);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, savedInvoice.TransactionApprovalRequest.XP_ApprovalStatus);
		}

		public void TestSetBranchAndDepartmentOnUXMLImport_ShipmentIsNotFound()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-1));
			TestObjectCreator.NonCurrentCompany.OrgProxy.OH_IsCreditor = true;

			var branch1 = TestObjectCreator.CreateBranch("CB1", GlbCompany.CurrentCompany);
			var branch3 = TestObjectCreator.CreateBranch("CB3", GlbCompany.CurrentCompany, TestObjectCreator.Debtor);
			var branch4 = TestObjectCreator.CreateBranch("CB4", GlbCompany.CurrentCompany, TestObjectCreator.Debtor);

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment);
			job.JH_GB = branch1.PK;
			job.JH_GE = TestObjectCreator.FEADepartment.PK;

			Factory.Save();

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.None),
				Address1 = "street",
				OrganizationCode = TestObjectCreator.Debtor.OH_Code,
			};

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { Job = new EntityReference { Key = "Job1", Type = AccountingDataTransferConstants.DataContextTypeString.Job } };

			var universalShipment = new Shipment { DataContext = DataContextFactory.New() };
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "Job1");

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				Ledger = LedgerTypes.AccountsReceivable,
				OrganizationAddress = orgAddress,
				Number = "1234",
				OSExGSTVATAmount = 100,
				Branch = new Branch { Code = TestObjectCreator.NonCurrentCompanyBranch.GB_Code },
				Department = new Department { Code = TestObjectCreator.MiscDepartment.GE_Code },
				BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), OrganizationCode = TestObjectCreator.NonCurrentCompany.OrgProxy.OH_Code }
			};
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal> { universalLine });
			universalTransaction.SetShipmentCollection(() => new List<Shipment> { universalShipment });
			AssertNull("Precondition: DataProvider", ((DataContext)universalTransaction.DataContext).DataProvider);
			universalTransaction.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.IDB } } });
			Assert("Precondition: HasRecipientRole", universalTransaction.HasRecipientRole(RecipientRoleType.IDB));

			var importer = new TransactionImporter();
			var message = new BusinessObjectFactory().New<EDIMessage>();
			message.EM_GB = TestObjectCreator.NonCurrentBranch.PK;
			message.EM_GE = TestObjectCreator.NonCurrentDepartment.PK;

			var universalFactory = new UniversalObjectFactory();
			var serviceLogger = new ServiceTaskLogForTesting();
			var logger = new XmlSessionTracker(serviceLogger);
			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			var createdInvoices = universalFactory.BOFactory.Load<TransactionPendingAllocation>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			var newFactory = new BusinessObjectFactory();
			var savedInvoice = newFactory.Load<TransactionPendingAllocation>(createdInvoices[0].PK);
			AssertEquals("Branch should be transaction default value as shipment is not found.", GlbBranch.CurrentBranch.PK, savedInvoice.AH_GB);
			AssertEquals("Department should be original one as shipment is not found.", TestObjectCreator.MiscDepartment.PK, savedInvoice.AH_GE);
			Assert("logger.HasErrors", !logger.HasErrors);
			Assert("logger.HasWarnings", logger.HasWarnings);
			var expectedBranchWarning = "Transaction Branch is set to the message branch because Transaction Branch cannot be set with reference to the invoice debtor organization proxy.";
			AssertContains(expectedBranchWarning, logger.ToString());
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
			AssertNotNull("Postcondition: request is created.", savedInvoice.TransactionApprovalRequest);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, savedInvoice.TransactionApprovalRequest.XP_ApprovalStatus);

			universalShipment.DataContext.AddDataTarget(DataContextType.ForwardingShipment, shipment.JS_UniqueConsignRef);
			universalTransaction.Number = string.Format(CultureInfo.InvariantCulture, "{0}_2", universalTransaction.Number);
			universalFactory = new UniversalObjectFactory();
			serviceLogger = new ServiceTaskLogForTesting();
			logger = new XmlSessionTracker(serviceLogger);
			result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			createdInvoices = universalFactory.BOFactory.Load<TransactionPendingAllocation>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			newFactory = new BusinessObjectFactory();
			savedInvoice = newFactory.Load<TransactionPendingAllocation>(createdInvoices[0].PK);
			AssertEquals("Branch should be calculated from job.", branch1.PK, savedInvoice.AH_GB);
			AssertEquals("Department should be calculated from job.", TestObjectCreator.FEADepartment.PK, savedInvoice.AH_GE);
			Assert("logger.HasErrors", !logger.HasErrors);
			Assert("logger.HasWarnings", !logger.HasWarnings);
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
			AssertNotNull("Postcondition: request is created.", savedInvoice.TransactionApprovalRequest);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, savedInvoice.TransactionApprovalRequest.XP_ApprovalStatus);
		}

		public void TestSetBranchAndDepartmentOnUXMLImport_JobIsNotCreated()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-1));
			TestObjectCreator.NonCurrentCompany.OrgProxy.OH_IsCreditor = true;

			var branch1 = TestObjectCreator.CreateBranch("CB1", GlbCompany.CurrentCompany);
			var branch3 = TestObjectCreator.CreateBranch("CB3", GlbCompany.CurrentCompany, TestObjectCreator.Debtor);
			var branch4 = TestObjectCreator.CreateBranch("CB4", GlbCompany.CurrentCompany, TestObjectCreator.Debtor);

			var shipment = TestObjectCreator.CreateShipment("S001");
			Factory.Save();

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.None),
				Address1 = "street",
				OrganizationCode = TestObjectCreator.Debtor.OH_Code,
			};

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { Job = new EntityReference { Key = "Job1", Type = AccountingDataTransferConstants.DataContextTypeString.Job } };

			var universalShipment = new Shipment { DataContext = DataContextFactory.New() };
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "Job1");
			universalShipment.DataContext.AddDataTarget(DataContextType.ForwardingShipment, shipment.JS_UniqueConsignRef);

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				Ledger = LedgerTypes.AccountsReceivable,
				OrganizationAddress = orgAddress,
				Number = "1234",
				OSExGSTVATAmount = 100,
				Branch = new Branch { Code = TestObjectCreator.NonCurrentCompanyBranch.GB_Code },
				Department = new Department { Code = TestObjectCreator.MiscDepartment.GE_Code },
				BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), OrganizationCode = TestObjectCreator.NonCurrentCompany.OrgProxy.OH_Code }
			};
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal> { universalLine });
			universalTransaction.SetShipmentCollection(() => new List<Shipment> { universalShipment });
			AssertNull("Precondition: DataProvider", ((DataContext)universalTransaction.DataContext).DataProvider);
			universalTransaction.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.IDB } } });
			Assert("Precondition: HasRecipientRole", universalTransaction.HasRecipientRole(RecipientRoleType.IDB));

			var importer = new TransactionImporter();
			var message = new BusinessObjectFactory().New<EDIMessage>();
			message.EM_GB = TestObjectCreator.NonCurrentBranch.PK;
			message.EM_GE = TestObjectCreator.NonCurrentDepartment.PK;

			var universalFactory = new UniversalObjectFactory();
			var serviceLogger = new ServiceTaskLogForTesting();
			var logger = new XmlSessionTracker(serviceLogger);
			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			var createdInvoices = universalFactory.BOFactory.Load<TransactionPendingAllocation>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			var newFactory = new BusinessObjectFactory();
			var savedInvoice = newFactory.Load<TransactionPendingAllocation>(createdInvoices[0].PK);
			AssertEquals("Branch should be transaction default value as job is not created.", GlbBranch.CurrentBranch.PK, savedInvoice.AH_GB);
			AssertEquals("Department should be original one as job is not created.", TestObjectCreator.MiscDepartment.PK, savedInvoice.AH_GE);
			Assert("logger.HasErrors", !logger.HasErrors);
			Assert("logger.HasWarnings", logger.HasWarnings);
			var expectedBranchWarning = "Transaction Branch is set to the message branch because Transaction Branch cannot be set with reference to the invoice debtor organization proxy.";
			AssertContains(expectedBranchWarning, logger.ToString());
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
			AssertNotNull("Postcondition: request is created.", savedInvoice.TransactionApprovalRequest);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, savedInvoice.TransactionApprovalRequest.XP_ApprovalStatus);

			var job = TestObjectCreator.CreateJob(shipment);
			job.JH_GB = branch1.PK;
			job.JH_GE = TestObjectCreator.FEADepartment.PK;
			Factory.Save();
			universalTransaction.Number = string.Format(CultureInfo.InvariantCulture, "{0}_2", universalTransaction.Number);
			universalFactory = new UniversalObjectFactory();
			serviceLogger = new ServiceTaskLogForTesting();
			logger = new XmlSessionTracker(serviceLogger);
			result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			createdInvoices = universalFactory.BOFactory.Load<TransactionPendingAllocation>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			newFactory = new BusinessObjectFactory();
			savedInvoice = newFactory.Load<TransactionPendingAllocation>(createdInvoices[0].PK);
			AssertEquals("Branch should be calculated from job.", branch1.PK, savedInvoice.AH_GB);
			AssertEquals("Department should be calculated from job.", TestObjectCreator.FEADepartment.PK, savedInvoice.AH_GE);
			Assert("logger.HasErrors", !logger.HasErrors);
			Assert("logger.HasWarnings", !logger.HasWarnings);
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
			AssertNotNull("Postcondition: request is created.", savedInvoice.TransactionApprovalRequest);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, savedInvoice.TransactionApprovalRequest.XP_ApprovalStatus);
		}

		public void TestSetBranchAndDepartmentOnUXMLImport_JobsWithDifferentBranchesAndDepartments()
		{
			//test we get the first in lines list.
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-1));
			TestObjectCreator.NonCurrentCompany.OrgProxy.OH_IsCreditor = true;

			var branch1 = TestObjectCreator.CreateBranch("CB1", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("CB2", GlbCompany.CurrentCompany);
			var branch3 = TestObjectCreator.CreateBranch("CB3", GlbCompany.CurrentCompany, TestObjectCreator.Debtor);
			var branch4 = TestObjectCreator.CreateBranch("CB4", GlbCompany.CurrentCompany, TestObjectCreator.Debtor);

			var shipment1 = TestObjectCreator.CreateShipment("S001");
			var job1 = TestObjectCreator.CreateJob(shipment1);
			job1.JH_GB = branch1.PK;
			job1.JH_GE = TestObjectCreator.FEADepartment.PK;
			var shipment2 = TestObjectCreator.CreateShipment("S002");
			var job2 = TestObjectCreator.CreateJob(shipment2);
			job2.JH_GB = branch2.PK;
			job2.JH_GE = TestObjectCreator.FESDepartment.PK;
			var shipment3 = TestObjectCreator.CreateShipment("S003");
			var job3 = TestObjectCreator.CreateJob(shipment3);
			job3.JH_GB = branch2.PK;
			job3.JH_GE = TestObjectCreator.FIADepartment.PK;
			Factory.Save();

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.None),
				Address1 = "street",
				OrganizationCode = TestObjectCreator.Debtor.OH_Code,
			};

			var universalLine1 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { Job = new EntityReference { Key = "Job1", Type = AccountingDataTransferConstants.DataContextTypeString.Job }, Sequence = 3 };
			var universalLine2 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { Job = new EntityReference { Key = "Job2", Type = AccountingDataTransferConstants.DataContextTypeString.Job }, Sequence = 2 };
			var universalLine3 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { Job = new EntityReference { Key = "Job3", Type = AccountingDataTransferConstants.DataContextTypeString.Job }, Sequence = 1 };

			var universalShipment1 = new Shipment { DataContext = DataContextFactory.New() };
			universalShipment1.DataContext.AddDataSource(DataContextType.ForwardingShipment, "Job1");
			universalShipment1.DataContext.AddDataTarget(DataContextType.ForwardingShipment, shipment1.JS_UniqueConsignRef);
			var universalShipment2 = new Shipment { DataContext = DataContextFactory.New() };
			universalShipment2.DataContext.AddDataSource(DataContextType.ForwardingShipment, "Job2");
			universalShipment2.DataContext.AddDataTarget(DataContextType.ForwardingShipment, shipment2.JS_UniqueConsignRef);
			var universalShipment3 = new Shipment { DataContext = DataContextFactory.New() };
			universalShipment3.DataContext.AddDataSource(DataContextType.ForwardingShipment, "Job3");
			universalShipment3.DataContext.AddDataTarget(DataContextType.ForwardingShipment, shipment3.JS_UniqueConsignRef);

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				Ledger = LedgerTypes.AccountsReceivable,
				OrganizationAddress = orgAddress,
				Number = "1234",
				OSExGSTVATAmount = 100,
				Branch = new Branch { Code = TestObjectCreator.NonCurrentCompanyBranch.GB_Code },
				Department = new Department { Code = TestObjectCreator.MiscDepartment.GE_Code },
				BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), OrganizationCode = TestObjectCreator.NonCurrentCompany.OrgProxy.OH_Code }
			};
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal> { universalLine1, universalLine2, universalLine3 });
			universalTransaction.SetShipmentCollection(() => new List<Shipment> { universalShipment3, universalShipment2, universalShipment1 });
			AssertNull("Precondition: DataProvider", ((DataContext)universalTransaction.DataContext).DataProvider);
			universalTransaction.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.IDB } } });
			Assert("Precondition: HasRecipientRole", universalTransaction.HasRecipientRole(RecipientRoleType.IDB));

			var importer = new TransactionImporter();
			var message = new BusinessObjectFactory().New<EDIMessage>();
			message.EM_GB = TestObjectCreator.NonCurrentBranch.PK;
			message.EM_GE = TestObjectCreator.NonCurrentDepartment.PK;

			var universalFactory = new UniversalObjectFactory();
			var serviceLogger = new ServiceTaskLogForTesting();
			var logger = new XmlSessionTracker(serviceLogger);
			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			var createdInvoices = universalFactory.BOFactory.Load<TransactionPendingAllocation>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			var newFactory = new BusinessObjectFactory();
			var savedInvoice = newFactory.Load<TransactionPendingAllocation>(createdInvoices[0].PK);
			AssertEquals("Branch should be calculated from the first line job.", branch1.PK, savedInvoice.AH_GB);
			AssertEquals("Department should be calculated from the first line job.", TestObjectCreator.FEADepartment.PK, savedInvoice.AH_GE);
			Assert("logger.HasErrors", !logger.HasErrors);
			Assert("logger.HasWarnings", !logger.HasWarnings);
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
			AssertNotNull("Postcondition: request is created.", savedInvoice.TransactionApprovalRequest);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, savedInvoice.TransactionApprovalRequest.XP_ApprovalStatus);
		}

		#endregion

		#region Code Mapping tests

		public void TestSetMappedValuesForForeignCodesWithoutMappingRules()
		{
			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			var importer = new TransactionImporter();
			importer.TrySetMappedValueDirectlyFromSourceCode(universalTransaction);
			AssertNull("OrganizationAddress", universalTransaction.OrganizationAddress);

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddress.AddressType = nameof(DocAddressType.None);
			orgAddress.Address1 = "some address";
			universalTransaction.OrganizationAddress = orgAddress;
			importer.TrySetMappedValueDirectlyFromSourceCode(universalTransaction);
			AssertNull("OrganizationAddress.OrganizationCode", universalTransaction.OrganizationAddress.OrganizationCode);

			var expectedMappedValue = "SomeOrg";
			orgAddress.OrganizationCode = new ZCodeMappedZString(TestObjectCreator.Creditor1.OH_Code) { MappedValue = expectedMappedValue };
			importer.TrySetMappedValueDirectlyFromSourceCode(universalTransaction);
			AssertEquals("OrganizationAddress.OrganizationCode.SourceValue", TestObjectCreator.Creditor1.OH_Code, universalTransaction.OrganizationAddress.OrganizationCode.Value.SourceValue);
			AssertEquals("OrganizationAddress.OrganizationCode.MappedValue", expectedMappedValue, universalTransaction.OrganizationAddress.OrganizationCode.Value.MappedValue);

			orgAddress.OrganizationCode = new ZCodeMappedZString(TestObjectCreator.Creditor1.OH_Code);
			importer.TrySetMappedValueDirectlyFromSourceCode(universalTransaction);
			AssertEquals("OrganizationAddress.OrganizationCode.SourceValue", TestObjectCreator.Creditor1.OH_Code, universalTransaction.OrganizationAddress.OrganizationCode.Value.SourceValue);
			AssertEquals("OrganizationAddress.OrganizationCode.MappedValue", TestObjectCreator.Creditor1.OH_Code, universalTransaction.OrganizationAddress.OrganizationCode.Value.MappedValue);

			var expectedSourceValue = "SomeSourceOrg";
			orgAddress.OrganizationCode = new ZCodeMappedZString(expectedSourceValue);
			importer.TrySetMappedValueDirectlyFromSourceCode(universalTransaction);
			AssertEquals("OrganizationAddress.OrganizationCode.SourceValue", expectedSourceValue, universalTransaction.OrganizationAddress.OrganizationCode.Value.SourceValue);
			AssertNull("OrganizationAddress.OrganizationCode.MappedValue", universalTransaction.OrganizationAddress.OrganizationCode.Value.MappedValue);
		}

		public void TestSetMappedValuesForForeignCodesWithoutMappingRules_ForLines()
		{
			var line = Factory.New<APInvoiceLine>();
			line.AL_ACInfo.ValueChanged += (sender, e) => Fail("line AL_AC field must not be changed.");
			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);

			var importer = new TransactionImporter();
			importer.TrySetMappedValueDirectlyFromSourceCode(universalLine, line);
			AssertNull("ChargeCode", universalLine.ChargeCode);

			var chargeCode = new ChargeCode();
			universalLine.ChargeCode = chargeCode;
			importer.TrySetMappedValueDirectlyFromSourceCode(universalLine, line);
			AssertNull("ChargeCode.Code", universalLine.ChargeCode.Code);

			var expectedMappedValue = "SomeCode";
			chargeCode.Code = new ZCodeMappedZString(TestObjectCreator.FRT.AC_Code) { MappedValue = expectedMappedValue };
			importer.TrySetMappedValueDirectlyFromSourceCode(universalLine, line);
			AssertEquals("ChargeCode.Code.SourceValue", TestObjectCreator.FRT.AC_Code, universalLine.ChargeCode.Code.Value.SourceValue);
			AssertEquals("ChargeCode.Code.MappedValue", expectedMappedValue, universalLine.ChargeCode.Code.Value.MappedValue);

			chargeCode.Code = new ZCodeMappedZString(TestObjectCreator.FRT.AC_Code);
			importer.TrySetMappedValueDirectlyFromSourceCode(universalLine, line);
			AssertEquals("ChargeCode.Code.SourceValue", TestObjectCreator.FRT.AC_Code, universalLine.ChargeCode.Code.Value.SourceValue);
			AssertEquals("ChargeCode.Code.MappedValue", TestObjectCreator.FRT.AC_Code, universalLine.ChargeCode.Code.Value.MappedValue);

			var expectedSourceValue = "SomeSourceCode";
			chargeCode.Code = new ZCodeMappedZString(expectedSourceValue);
			importer.TrySetMappedValueDirectlyFromSourceCode(universalLine, line);
			AssertEquals("ChargeCode.Code.SourceValue", expectedSourceValue, universalLine.ChargeCode.Code.Value.SourceValue);
			AssertNull("ChargeCode.Code.MappedValue", universalLine.ChargeCode.Code.Value.MappedValue);
		}

		public void TestSetMatchedProperies_JobConsolXMLData()
		{
			var consol1 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			consol1.JK_MasterBillNum = "MASTERBILL1";
			var shipment1 = TestObjectCreator.CreateShipment("S001", consol1);
			shipment1.JS_HouseBill = "HOUSEBILL1";
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			shipment1.JS_ActualWeight = 80;
			var shipment2 = TestObjectCreator.CreateShipment("S002");
			shipment2.JS_HouseBill = "HOUSEBILL2";
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_ActualWeight = 20;
			Factory.Save();

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.IsFinalCharge = true;
			universalLine.OSAmount = -10;
			universalLine.CostSource = new EntityReference { Key = "Consol1", Type = nameof(DataContextType.ForwardingConsol) };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.OSAmount = -20;
			universalLine.Job = new EntityReference { Key = "Job1", Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			universalLine.CostSource = new EntityReference { Key = "Consol1", Type = nameof(DataContextType.ForwardingConsol) };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.OSAmount = -30;
			universalLine.Job = new EntityReference { Key = "Job2", Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.OSAmount = -40;
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingConsol, "Consol1");
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
			universalShipment.WayBillNumber = consol1.JK_MasterBillNum;
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			universalTransaction.ShipmentCollection.Add(universalShipment);

			universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "Job1");
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			universalShipment.WayBillNumber = shipment1.JS_HouseBill;
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			universalTransaction.ShipmentCollection.Add(universalShipment);

			universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "Job2");
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			universalShipment.WayBillNumber = shipment2.JS_HouseBill;
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var universalTransactionXml = universalTransaction.Serialize();
			var unallocatedTransaction = TestObjectCreator.CreateTransactionPendingAllocation("INV1", TestObjectCreator.Creditor1, 100);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(unallocatedTransaction, universalTransactionXml, false);
			Factory.Save();

			var importer = new TransactionImporter();
			var invoice = new BusinessObjectFactory().Load<APInvoice>(unallocatedTransaction.PK);
			invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.SubmittedFromInvoicingForm = true;

			importer.ImportTransactionLines(universalTransactionXml, invoice, false);
			AssertEquals(4, invoice.Lines.Count);
			invoice.Lines.Sort("AL_OSExTaxAmount"); // This is unit test in separate debug only assembly

			Action<int, string> assert = (lineIndex, expectedValue) =>
				{
					var line = invoice.Lines[lineIndex];
					AssertEquals(expectedValue, line.JobConsolXMLData);
					var requestLine = request.PostingDetails.UniversalTransaction.Lines[lineIndex];
					AssertEquals(expectedValue, requestLine.JobConsolXMLData);
				};

			assert(0,
@"Consol Consol1
Master Bill: MASTERBILL1");

			assert(1,
@"Job Job1
House Bill #: HOUSEBILL1

Consol Consol1
Master Bill: MASTERBILL1");

			assert(2,
@"Job Job2
House Bill #: HOUSEBILL2");

			assert(3, "");

			invoice.ReleaseAllMutexOnInvoice();
		}

		public void TestSetMatchedProperies()
		{
			var address = TestObjectCreator.CreateAddress(TestObjectCreator.Creditor1, "123 Road Ave");
			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddress.AddressType = nameof(DocAddressType.None);
			orgAddress.AddressShortCode = address.OA_Code;
			orgAddress.Address1 = address.OA_Address1;
			orgAddress.OrganizationCode = "CreditorABC";

			var currentCompnay = GlbCompany.GetCurrentCompany(Factory);
			OrgPatternMatchOverride patternMatchOverride = AddMatchingRuleForOrganization(currentCompnay.OrgProxy, orgAddress.OrganizationCode.GetValueOrDefault(), TestObjectCreator.Creditor1);

			patternMatchOverride = AddMatchingRuleForChargeCode(currentCompnay.OrgProxy, "FrnCode", TestObjectCreator.CC1);

			Factory.Save();

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.OrganizationAddress = orgAddress;
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());

			var universalLine1 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine1.ChargeCode = new ChargeCode { Code = "FrnCode" };
			universalTransaction.PostingJournalCollection.Add(universalLine1);

			var universalLine2 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine2.GLAccount = new GLAccount { AccountCode = TestObjectCreator.GLHeader1.AG_AccountNum };
			universalTransaction.PostingJournalCollection.Add(universalLine2);

			var universalLine3 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine3.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC2.AC_Code };
			universalTransaction.PostingJournalCollection.Add(universalLine3);

			var universalTransactionXml = universalTransaction.Serialize();
			var unallocatedTransaction = TestObjectCreator.CreateTransactionPendingAllocation("INV1", TestObjectCreator.Creditor2, 100);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(unallocatedTransaction, universalTransactionXml, false);
			Factory.Save();

			var importer = new TransactionImporter();
			var invoice = new BusinessObjectFactory().Load<APInvoice>(unallocatedTransaction.PK);
			invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.SubmittedFromInvoicingForm = true;

			importer.ImportTransactionLines(universalTransactionXml, invoice, false);
			//simulation of changing line order and editing
			invoice.Lines.RemoveAndDeleteAll();
			var secondLineThatBecomeFirst = (InvoicingLineBase)invoice.Lines.AddNew();
			secondLineThatBecomeFirst.GenericCharge = TestObjectCreator.CC2.PK;
			secondLineThatBecomeFirst.IndexOfImportedUniversalTransactionLine = 1;
			var newLine = (InvoicingLineBase)invoice.Lines.AddNew();
			newLine.GenericCharge = TestObjectCreator.CC2.PK;
			var firstLineThatBecomeThird = (InvoicingLineBase)invoice.Lines.AddNew();
			firstLineThatBecomeThird.GenericCharge = TestObjectCreator.CC2.PK;
			firstLineThatBecomeThird.IndexOfImportedUniversalTransactionLine = 0;

			AssertEquals("ImportedCreditor", "ZCreditor1", invoice.ImportedCreditor);
			AssertEquals("ImportedCreditorXmlCode", "CreditorABC", invoice.ImportedCreditorXmlCode);

			var line = invoice.Lines[0];
			AssertEquals("ImportedChargeCode", "", line.ImportedChargeCode);
			AssertEquals("ImportedChargeCode", "", line.ImportedChargeCodeXmlCode);

			line = invoice.Lines[1];
			AssertEquals("ImportedXMLValues", -1, line.IndexOfImportedUniversalTransactionLine);

			line = invoice.Lines[2];
			AssertEquals("ImportedChargeCode", "ZZCC1", line.ImportedChargeCode);
			AssertEquals("ImportedChargeCode", "FrnCode", line.ImportedChargeCodeXmlCode);
		}

		#endregion

		public void TestNextLineJobIsNotAutoPopulated()
		{
			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var importer = new TransactionImporter();
			var invoice = new BusinessObjectFactory().New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;
			invoice.SubmittedFromInvoicingForm = true;

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.OSAmount = -60;
			universalLine.Job = new EntityReference { Key = "Job", Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.OSAmount = -80;
			universalLine.Job = new EntityReference { Key = "S001", Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "Job");
			universalShipment.DataContext.AddDataTarget(DataContextType.ForwardingShipment, "S001");
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var universalTransactionXml = universalTransaction.Serialize();
			importer.ImportTransactionLines(universalTransactionXml, invoice, false);
			AssertEquals(2, invoice.Lines.Count);
			var line = invoice.Lines[0];
			AssertEquals("Line job", job.PK, line.AL_JH);
			AssertNoNotifications(line.AL_JHInfo);
			AssertEquals("GenericCharge", TestObjectCreator.CC1.PK, line.GenericCharge);
			AssertEquals("OSExTaxAmount", 60m, line.AL_OSExTaxAmount);
			AssertEquals("LocalExTaxAmount", 60m, line.AL_LocalExTaxAmount);

			line = invoice.Lines[1];
			AssertEquals("Line job", ZGuid.Empty, line.AL_JH);
			AssertHasWarning(line.AL_JHInfo, "Source XML errors: Can't find operational job with number 'S001'. XML should have operational job with DataSource Key element the same as line job number and correct Type.");
			AssertEquals("GenericCharge", TestObjectCreator.CC1.PK, line.GenericCharge);
			AssertEquals("OSExTaxAmount", 80m, line.AL_OSExTaxAmount);
			AssertEquals("LocalExTaxAmount", 80m, line.AL_LocalExTaxAmount);
		}

		public void TestImportTransactionLinesWithConsolIdOnly()
		{
			var consol1 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			consol1.JK_MasterBillNum = "MasterBill1";
			var shipment1 = TestObjectCreator.CreateShipment("S001", consol1);
			shipment1.JS_HouseBill = "HouseBill1";
			shipment1.JS_ActualChargeable = 80;
			var shipment2 = TestObjectCreator.CreateShipment("S002", consol1);
			shipment2.JS_HouseBill = "HouseBill2";
			shipment2.JS_ActualChargeable = 20;
			var consol2 = TestObjectCreator.CreateConsol("NZAKL", "USLAX", "C002");
			consol2.JK_MasterBillNum = "MasterBill2";
			consol2.Shipments.Add(shipment1);
			consol2.Shipments.Add(shipment2);
			Factory.Save();

			var importer = new TransactionImporter();
			var invoice = new BusinessObjectFactory().New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;
			invoice.SubmittedFromInvoicingForm = true;

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.IsFinalCharge = true;
			universalLine.OSAmount = -10;
			universalLine.CostSource = new EntityReference { Key = "Consol1", Type = nameof(DataContextType.ForwardingConsol) };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.OSAmount = -20;
			universalLine.Job = new EntityReference { Key = "Job2", Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			universalLine.CostSource = new EntityReference { Key = "Consol2", Type = nameof(DataContextType.ForwardingConsol) };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.OSAmount = -30;
			universalLine.CostSource = new EntityReference { Key = "Consol1", Type = nameof(DataContextType.ForwardingConsol) };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.OSAmount = -40;
			universalLine.Job = new EntityReference { Key = "Job1", Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			universalLine.CostSource = new EntityReference { Key = "Consol1", Type = nameof(DataContextType.ForwardingConsol) };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingConsol, "Consol1");
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
			universalShipment.WayBillNumber = consol1.JK_MasterBillNum;
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			universalTransaction.ShipmentCollection.Add(universalShipment);

			universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingConsol, "Consol2");
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
			universalShipment.WayBillNumber = consol2.JK_MasterBillNum;
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			universalTransaction.ShipmentCollection.Add(universalShipment);

			universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "Job2");
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			universalShipment.WayBillNumber = shipment2.JS_HouseBill;
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var universalTransactionXml = universalTransaction.Serialize();
			int invoiceLinesListChangedHitCount = 0;
			var listChangedHandler = new ListChangedEventHandler(
				(sender, e) =>
				{ invoiceLinesListChangedHitCount++; }
			);
			((IBindingList)invoice.Lines).ListChanged += listChangedHandler;
			importer.ImportTransactionLines(universalTransactionXml, invoice, false);
			AssertEquals("ListChanged on invoice.Lines should be called once", 1, invoiceLinesListChangedHitCount);
			AssertEquals(6, invoice.Lines.Count);
			invoice.Lines.Sort("AL_OSExTaxAmount"); // This is unit test in separate debug only assembly
			var line = invoice.Lines[0];
			AssertNoNotifications(line.AL_JHInfo);
			AssertNoNotifications(line.ConsolIDFromApportionedChargeInfo);
			AssertEquals("Line job", shipment2.PK, line.Job.JH_ParentID);
			AssertEquals("Line consol", consol1.PK, line.Consol.PK);
			AssertEquals("GenericCharge", TestObjectCreator.CC1.PK, line.GenericCharge);
			AssertEquals("IsFinalCharge", true, line.AL_IsFinalCharge);
			AssertEquals("OSExTaxAmount", 2m, line.AL_OSExTaxAmount);
			AssertEquals("LocalExTaxAmount", 2m, line.AL_LocalExTaxAmount);

			line = invoice.Lines[1];
			AssertNoNotifications(line.AL_JHInfo);
			AssertNoNotifications(line.ConsolIDFromApportionedChargeInfo);
			AssertEquals("Line job", shipment2.PK, line.Job.JH_ParentID);
			AssertEquals("Line consol", consol1.PK, line.Consol.PK);
			AssertEquals("GenericCharge", TestObjectCreator.CC1.PK, line.GenericCharge);
			AssertEquals("IsFinalCharge", false, line.AL_IsFinalCharge);
			AssertEquals("OSExTaxAmount", 6m, line.AL_OSExTaxAmount);
			AssertEquals("LocalExTaxAmount", 6m, line.AL_LocalExTaxAmount);

			line = invoice.Lines[2];
			AssertNoNotifications(line.AL_JHInfo);
			AssertNoNotifications(line.ConsolIDFromApportionedChargeInfo);
			AssertEquals("Line job", shipment1.PK, line.Job.JH_ParentID);
			AssertEquals("Line consol", consol1.PK, line.Consol.PK);
			AssertEquals("GenericCharge", TestObjectCreator.CC1.PK, line.GenericCharge);
			AssertEquals("IsFinalCharge", true, line.AL_IsFinalCharge);
			AssertEquals("OSExTaxAmount", 8m, line.AL_OSExTaxAmount);
			AssertEquals("LocalExTaxAmount", 8m, line.AL_LocalExTaxAmount);

			line = invoice.Lines[3];
			AssertNoNotifications(line.AL_JHInfo);
			AssertNoNotifications(line.ConsolIDFromApportionedChargeInfo);
			AssertEquals("Line job", shipment2.PK, line.Job.JH_ParentID);
			AssertEquals("Line consol", consol2.PK, line.Consol.PK);
			AssertEquals("GenericCharge", TestObjectCreator.CC1.PK, line.GenericCharge);
			AssertEquals("IsFinalCharge", false, line.AL_IsFinalCharge);
			AssertEquals("OSExTaxAmount", 20m, line.AL_OSExTaxAmount);
			AssertEquals("LocalExTaxAmount", 20m, line.AL_LocalExTaxAmount);

			line = invoice.Lines[4];
			AssertNoNotifications(line.AL_JHInfo);
			AssertNoNotifications(line.ConsolIDFromApportionedChargeInfo);
			AssertEquals("Line job", shipment1.PK, line.Job.JH_ParentID);
			AssertEquals("Line consol", consol1.PK, line.Consol.PK);
			AssertEquals("GenericCharge", TestObjectCreator.CC1.PK, line.GenericCharge);
			AssertEquals("IsFinalCharge", false, line.AL_IsFinalCharge);
			AssertEquals("OSExTaxAmount", 24m, line.AL_OSExTaxAmount);
			AssertEquals("LocalExTaxAmount", 24m, line.AL_LocalExTaxAmount);

			line = invoice.Lines[5];
			AssertHasWarning(line.AL_JHInfo, "Source XML errors: Can't find operational job with number 'Job1'. XML should have operational job with DataSource Key element the same as line job number and correct Type.");
			AssertNoNotifications(line.ConsolIDFromApportionedChargeInfo);
			AssertNull("Line job", line.Job);
			AssertNull("Line consol", line.Consol);
			AssertEquals("GenericCharge", TestObjectCreator.CC1.PK, line.GenericCharge);
			AssertEquals("IsFinalCharge", false, line.AL_IsFinalCharge);
			AssertEquals("OSExTaxAmount", 40m, line.AL_OSExTaxAmount);
			AssertEquals("LocalExTaxAmount", 40m, line.AL_LocalExTaxAmount);

			AssertEquals("ConsolCosts.Count", 3, invoice.ConsolCosting.ConsolCosts.Count);
			var consolCost = invoice.ConsolCosting.ConsolCosts[0];
			AssertEquals("consolCost ParentID", consol2.PK, consolCost.E6_ParentID);
			AssertEquals("consolCost ApportionmentMethod", AllocationMethod.Manual, consolCost.E6_ApportionmentMethod);
			AssertEquals("consolCost OSCostAmount", 20m, consolCost.E6_OSCostAmount);
			AssertEquals("consolCost UnApportionedAmount", 0m, consolCost.UnApportionedAmount);
			AssertEquals("consolCost ApportionmentCharges Count", 2, consolCost.ApportionmentCharges.Count);
			AssertEquals("consolCost used ApportionmentCharges Count", 1, consolCost.ApportionmentCharges.Count(x => ((ApportionSplitCharge)x).JR_OSCostAmt != 0));
			consolCost = invoice.ConsolCosting.ConsolCosts[1];
			AssertEquals("consolCost ParentID", consol1.PK, consolCost.E6_ParentID);
			AssertEquals("consolCost ApportionmentMethod", AllocationMethod.ChargeableUnits, consolCost.E6_ApportionmentMethod);
			AssertEquals("consolCost OSCostAmount", 10m, consolCost.E6_OSCostAmount);
			AssertEquals("consolCost UnApportionedAmount", 0m, consolCost.UnApportionedAmount);
			AssertEquals("consolCost ApportionmentCharges Count", 2, consolCost.ApportionmentCharges.Count);
			AssertEquals("consolCost used ApportionmentCharges Count", 2, consolCost.ApportionmentCharges.Count(x => ((ApportionSplitCharge)x).JR_OSCostAmt != 0));
			consolCost = invoice.ConsolCosting.ConsolCosts[2];
			AssertEquals("consolCost ParentID", consol1.PK, consolCost.E6_ParentID);
			AssertEquals("consolCost ApportionmentMethod", AllocationMethod.ChargeableUnits, consolCost.E6_ApportionmentMethod);
			AssertEquals("consolCost OSCostAmount", 30m, consolCost.E6_OSCostAmount);
			AssertEquals("consolCost UnApportionedAmount", 0m, consolCost.UnApportionedAmount);
			AssertEquals("consolCost ApportionmentCharges Count", 2, consolCost.ApportionmentCharges.Count);
			AssertEquals("consolCost used ApportionmentCharges Count", 2, consolCost.ApportionmentCharges.Count(x => ((ApportionSplitCharge)x).JR_OSCostAmt != 0));

			Assert("ConsolCosting.HasErrors", !invoice.ConsolCosting.HasErrors);

			invoice.ReleaseAllMutexOnInvoice();
		}

		public void TestReadAttachedDocuments_WithoutDataContext()
		{
			AssertReadAttachedDocuments(false);
		}

		public void TestReadAttachedDocuments_WithDataContext()
		{
			AssertReadAttachedDocuments(true);
		}

		void AssertReadAttachedDocuments(bool withDataContext)
		{
			AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var address = TestObjectCreator.CreateAddress(TestObjectCreator.Creditor1, "123 Road Ave");
			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddress.AddressType = nameof(DocAddressType.None);
			orgAddress.AddressShortCode = address.OA_Code;
			orgAddress.Address1 = address.OA_Address1;
			orgAddress.OrganizationCode = "CreditorABC";

			var importer = new TransactionImporter();
			var message = new BusinessObjectFactory().New<EDIMessage>();
			var universalFactory = new UniversalObjectFactory();
			var serviceLogger = new ServiceTaskLogForTesting();
			var logger = new XmlSessionTracker(serviceLogger);
			using (var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance))
			{
				universalTransaction.Ledger = LedgerTypes.AccountsPayable;
				universalTransaction.OrganizationAddress = orgAddress;

				var document = new AttachedDocument();
				document.FileName = "Hahaha";
				document.ImageData = (SubStreamableStream)new MemoryStream(new byte[] { 1, 2, 3 });
				document.Type = new DocumentType { Code = "HHH", Description = "Hahaha" };
				universalTransaction.SetAttachedDocumentCollection(() => new List<AttachedDocument> { document });
				var imageDataToCompare = document.ImageData.Copy();

				var nameSpace = UniversalXmlInfo.Namespace_2012_11;
				var systemNamespace = SchemaVersionManager.Current.Namespace;
				AssertNotEquals(
					"Massage XML should have different namespace to system default one to test namespace calculation when removing attachments in XML for approval request.",
					systemNamespace, nameSpace);

				if (withDataContext)
				{
					universalTransaction.DataContext = DataContextFactory.New(nameSpace);
				}

				using (var stream = new CargoWise.IO.Shim.SubStreamableStream())
				using (var reader = new StreamReader(stream))
				{
					new XmlWriter().WriteXML(universalTransaction, stream, nameSpace);
					stream.Flush();
					stream.Position = 0;
					message.EM_MessageText = reader.ReadToEnd();
				}

				var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
				Assert("Precondition: Import result", result);
				var savedInvoice = new BusinessObjectFactory().LoadTop1<TransactionPendingAllocation>(new ZQuery());
				AssertNotNull(savedInvoice);
				Assert("Precondition: logger.HasErrors", !logger.HasErrors);
				AssertEquals(1, ((IDocManagerSupport)savedInvoice).DocManagerInfo.AllEDocs.Count);
				var eDoc = ((IDocManagerSupport)savedInvoice).DocManagerInfo.AllEDocs[0];

				AssertEquals(document.FileName, eDoc.FileName);
				AssertArrayEqualsByElements(imageDataToCompare.ConvertToByteArrayAndCloseStream(), eDoc.GetImageDataReader().ConvertToByteArrayAndCloseStream());
				AssertEquals(document.Type.Code, eDoc.DocType);

				var request = savedInvoice.TransactionApprovalRequest;
				AssertNotNull("Precondition: TransactionApprovalRequest", request);
				var requestXML = request.PostingDetails.SourceXML;
				Assert("PreconditionSourceXML", !requestXML.IsEmpty);
				var universalTransactionInRequest = (TransactionInfo)importer.ImportUniversalTransactionFromXml(requestXML, Factory, false).Item1;
				AssertNull("Attached documents should not be saved in request source xml as the already added in transaction eDocs and are not required anymore.", universalTransactionInRequest.AttachedDocumentCollection);
				AssertContains("requestXML has namespace as original XML", nameSpace, requestXML);
				AssertNotContains("Postcondition: systemNamespace is not included.", systemNamespace, requestXML);
			}
		}

		public void TestImportInvalidTransactionCreatesRequestWithERRStatus()
		{
			AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var importer = new TransactionImporter();
			var message = new BusinessObjectFactory().New<EDIMessage>();
			var universalFactory = new UniversalObjectFactory();
			var serviceLogger = new ServiceTaskLogForTesting();
			var logger = new XmlSessionTracker(serviceLogger);
			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.Ledger = LedgerTypes.AccountsPayable;

			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			var savedInvoice = new BusinessObjectFactory().LoadTop1<TransactionPendingAllocation>(new ZQuery());
			AssertNotNull(savedInvoice);
			Assert("logger.HasErrors", !logger.HasErrors);
			AssertNotNull("TransactionApprovalRequest", savedInvoice.TransactionApprovalRequest);
			savedInvoice.RunPreSaveValidation();
			Assert("Precondition: invoice has errors", savedInvoice.HasErrors);
			AssertEquals("ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Error, savedInvoice.TransactionApprovalRequest.XP_ApprovalStatus);
		}

		public void TestImportValidTransactionCreatesRequestWithREQStatus()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-10));
			Factory.Save();

			var address = TestObjectCreator.CreateAddress(TestObjectCreator.Creditor1, "123 Road Ave");
			var contact = TestObjectCreator.CreateContact(TestObjectCreator.Creditor1, "Mark", "ww@www.com");
			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddress.AddressType = nameof(DocAddressType.None);
			orgAddress.AddressShortCode = address.OA_Code;
			orgAddress.Address1 = address.OA_Address1;
			orgAddress.OrganizationCode = TestObjectCreator.Creditor1.OH_Code;
			orgAddress.Contact = contact.OC_ContactName;
			orgAddress.Email = contact.OC_Email;

			AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var importer = new TransactionImporter();
			var message = new BusinessObjectFactory().New<EDIMessage>();
			var universalFactory = new UniversalObjectFactory();
			var serviceLogger = new ServiceTaskLogForTesting();
			var logger = new XmlSessionTracker(serviceLogger);
			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.Ledger = LedgerTypes.AccountsPayable;
			universalTransaction.OrganizationAddress = orgAddress;
			universalTransaction.TransactionDate = ZDateTime.Today.AddDays(-3);
			universalTransaction.DueDate = ZDateTime.Today.AddDays(2);
			universalTransaction.PostDate = ZDateTime.Today;
			universalTransaction.Number = "1234";
			universalTransaction.NumberOfSupportingDocuments = 5;
			universalTransaction.Description = "Some text";
			universalTransaction.Branch = new Branch { Code = TestObjectCreator.NonCurrentBranch.GB_Code };
			universalTransaction.Department = new Department { Code = TestObjectCreator.NonCurrentDepartment.GE_Code };
			universalTransaction.OSCurrency = new Currency { Code = "GBP" };
			universalTransaction.LocalCurrency = new Currency { Code = "AUD" };
			universalTransaction.OSExGSTVATAmount = 120;
			universalTransaction.LocalExVATAmount = 60;
			universalTransaction.OSGSTVATAmount = 10;
			universalTransaction.LocalVATAmount = 5;

			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			var savedInvoice = new BusinessObjectFactory().LoadTop1<TransactionPendingAllocation>(new ZQuery());
			AssertNotNull(savedInvoice);
			Assert("logger.HasErrors", !logger.HasErrors);
			AssertNotNull("TransactionApprovalRequest", savedInvoice.TransactionApprovalRequest);
			savedInvoice.RunPreSaveValidation();
			Assert("Precondition: invoice doesn't have errors", !savedInvoice.HasErrors);
			AssertEquals("ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Requested, savedInvoice.TransactionApprovalRequest.XP_ApprovalStatus);

			var requestPostingDetails = savedInvoice.TransactionApprovalRequest.PostingDetails;
			AssertNotNull("Precondition: posting details have to be filled", requestPostingDetails);
		}

		public void TestCrossLedgerImportTransactionLinesWithConsolCosts_ErrorMessages()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			consol.JK_MasterBillNum = "MasterBill1";
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			shipment.JS_HouseBill = "HouseBill1";
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			Factory.Save();

			var importer = new TransactionImporter();
			var invoice = new BusinessObjectFactory().New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			universalTransaction.JobInvoiceNumber = "Consol1/AA";

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.IsFinalCharge = true;
			universalLine.OSAmount = -10;
			universalLine.Job = new EntityReference { Key = "Job1", Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			universalLine.CostSource = new EntityReference { Key = "Consol1", Type = nameof(DataContextType.ForwardingConsol) };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "Job1");
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			universalShipment.WayBillNumber = shipment.JS_HouseBill;
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var universalConsol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalConsol.DataContext = DataContextFactory.New();
			universalConsol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C001");
			universalConsol.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			universalTransaction.ShipmentCollection.Add(universalConsol);

			var universalTransactionXml = universalTransaction.Serialize();
			importer.ImportTransactionLines(universalTransactionXml, invoice, true);
			AssertEquals(1, invoice.Lines.Count);
			var line = invoice.Lines[0];
			AssertHasWarning(line.ConsolIDFromApportionedChargeInfo, "Source XML errors: Can't find consolidation with number 'Consol1'. XML should have consolidation with DataSource Key element the same as in invoice JobInvoiceNumber and correct Type.");
			AssertEquals("Line consol", "", line.ConsolIDFromApportionedCharge);

			universalConsol.DataContext.DataSourceCollection.First().Key = "Consol1";
			invoice.Lines.RemoveAndDeleteAll();
			universalTransactionXml = universalTransaction.Serialize();
			importer.ImportTransactionLines(universalTransactionXml, invoice, true);
			AssertEquals(1, invoice.Lines.Count);
			line = invoice.Lines[0];
			AssertHasWarning(line.ConsolIDFromApportionedChargeInfo, "Source XML errors: Consolidation ForwardingConsol with DataSource Key 'Consol1' is not found.");
			AssertEquals("Line consol", "", line.ConsolIDFromApportionedCharge);

			universalConsol.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
			universalConsol.WayBillNumber = consol.JK_MasterBillNum;
			invoice.Lines.RemoveAndDeleteAll();
			universalTransactionXml = universalTransaction.Serialize();
			importer.ImportTransactionLines(universalTransactionXml, invoice, true);
			AssertEquals(1, invoice.Lines.Count);
			line = invoice.Lines[0];
			AssertNoNotifications(line.ConsolIDFromApportionedChargeInfo);
			AssertEquals("Line consol", "C001", line.ConsolIDFromApportionedCharge);

			invoice.ReleaseAllMutexOnInvoice();
		}

		public void TestCrossLedgerImportTransactionLinesWithConsolCosts()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			consol.JK_MasterBillNum = "MasterBill1";
			var shipment1 = TestObjectCreator.CreateShipment("S001", consol);
			shipment1.JS_HouseBill = "HouseBill1";
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			var shipment2 = TestObjectCreator.CreateShipment("S002", consol);
			shipment2.JS_HouseBill = "HouseBill2";
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			Factory.Save();

			var importer = new TransactionImporter();
			var invoice = new BusinessObjectFactory().New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			universalTransaction.JobInvoiceNumber = "Consol1/AA";

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.IsFinalCharge = true;
			universalLine.OSAmount = -10;
			universalLine.Job = new EntityReference { Key = "Job1", Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.OSAmount = -30;
			universalLine.Job = new EntityReference { Key = "Job2", Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "Job1");
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			universalShipment.WayBillNumber = shipment1.JS_HouseBill;
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			universalTransaction.ShipmentCollection.Add(universalShipment);

			universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "Job2");
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			universalShipment.WayBillNumber = shipment2.JS_HouseBill;
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			universalTransaction.ShipmentCollection.Add(universalShipment);

			universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingConsol, "Consol1");
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
			universalShipment.WayBillNumber = consol.JK_MasterBillNum;
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var universalTransactionXml = universalTransaction.Serialize();

			int invoiceLinesListChangedHitCount = 0;
			var listChangedHandler = new ListChangedEventHandler(
				(sender, e) =>
				{ invoiceLinesListChangedHitCount++; }
			);
			((IBindingList)invoice.Lines).ListChanged += listChangedHandler;

			importer.ImportTransactionLines(universalTransactionXml, invoice, true);
			AssertEquals("ListChanged on invoice.Lines should be called once", 1, invoiceLinesListChangedHitCount);
			AssertEquals(2, invoice.Lines.Count);
			invoice.Lines.Sort("AL_OSExTaxAmount"); // This is unit test in separate debug only assembly
			var line = invoice.Lines[0];
			AssertNoNotifications(line.AL_JHInfo);
			AssertNoNotifications(line.ConsolIDFromApportionedChargeInfo);
			AssertEquals("Line job", shipment2.PK, line.Job.JH_ParentID);
			AssertEquals("Line consol", consol.PK, line.Consol.PK);
			AssertEquals("GenericCharge", TestObjectCreator.CC1.PK, line.GenericCharge);
			AssertEquals("IsFinalCharge", true, line.AL_IsFinalCharge);
			AssertEquals("OSExTaxAmount", -30m, line.AL_OSExTaxAmount);
			AssertEquals("LocalExTaxAmount", -30m, line.AL_LocalExTaxAmount);

			line = invoice.Lines[1];
			AssertNoNotifications(line.AL_JHInfo);
			AssertNoNotifications(line.ConsolIDFromApportionedChargeInfo);
			AssertEquals("Line job", shipment1.PK, line.Job.JH_ParentID);
			AssertEquals("Line consol", consol.PK, line.Consol.PK);
			AssertEquals("GenericCharge", TestObjectCreator.CC1.PK, line.GenericCharge);
			AssertEquals("IsFinalCharge", true, line.AL_IsFinalCharge);
			AssertEquals("OSExTaxAmount", -10m, line.AL_OSExTaxAmount);
			AssertEquals("LocalExTaxAmount", -10m, line.AL_LocalExTaxAmount);

			AssertEquals("ConsolCosts.Count", 1, invoice.ConsolCosting.ConsolCosts.Count);
			var consolCost = invoice.ConsolCosting.ConsolCosts[0];
			AssertEquals("consolCost ParentID", consol.PK, consolCost.E6_ParentID);
			AssertEquals("consolCost ApportionmentMethod", AllocationMethod.Manual, consolCost.E6_ApportionmentMethod);
			AssertEquals("consolCost OSCostAmount", -40m, consolCost.E6_OSCostAmount);
			AssertEquals("consolCost UnApportionedAmount", 0m, consolCost.UnApportionedAmount);
			AssertEquals("consolCost ApportionmentCharges Count", 2, consolCost.ApportionmentCharges.Count);
			AssertEquals("consolCost used ApportionmentCharges Count", 2, consolCost.ApportionmentCharges.Count(x => ((ApportionSplitCharge)x).JR_OSCostAmt != 0));

			Assert("ConsolCosting.HasErrors", !invoice.ConsolCosting.HasErrors);

			invoice.ReleaseAllMutexOnInvoice();
		}

		public void TestCrossLedgerImportTransaction_FromTheSameSystem()
		{
			AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var importer = new TransactionImporter();
			var message = new BusinessObjectFactory().New<EDIMessage>();
			var universalFactory = new UniversalObjectFactory();
			var serviceLogger = new ServiceTaskLogForTesting();
			var logger = new XmlSessionTracker(serviceLogger);
			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.DataContext = DataContextFactory.New();
			universalTransaction.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			var dataContext = (DataContext)universalTransaction.DataContext;
			dataContext.Company = null;
			universalTransaction.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.IDB } } });
			Assert("Precondition: HasRecipientRole", universalTransaction.HasRecipientRole(RecipientRoleType.IDB));

			universalTransaction.Ledger = LedgerTypes.AccountsReceivable;
			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);

			var interchange = new BusinessObjectFactory().NewWithValidTestData<EDIInterchange>();
			interchange.EI_From = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			interchange.Factory.Save();
			message.EM_EI = interchange.PK;

			universalFactory = new UniversalObjectFactory();
			serviceLogger = new ServiceTaskLogForTesting();
			logger = new XmlSessionTracker(serviceLogger);
			universalTransaction.Number = string.Format(CultureInfo.InvariantCulture, "{0}_1", universalTransaction.Number);
			result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", !result);
			Assert("logger.HasErrors", logger.HasErrors);
			AssertEquals("Error - Transaction Pending Allocation is not created since both the Issuer and Recipient is in the same company.", logger.ToString());

			interchange.EI_From = "";
			interchange.Factory.Save();
			universalFactory = new UniversalObjectFactory();
			serviceLogger = new ServiceTaskLogForTesting();
			logger = new XmlSessionTracker(serviceLogger);
			result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
		}

		public void TestCrossLedgerImportTransaction()
		{
			var address = TestObjectCreator.CreateAddress(TestObjectCreator.Creditor1, "123 Road Ave");
			var contact = TestObjectCreator.CreateContact(TestObjectCreator.Creditor1, "Mark", "ww@www.com");

			Factory.Save();

			var importer = new TransactionImporter();
			var message = new BusinessObjectFactory().New<EDIMessage>();
			var universalFactory = new UniversalObjectFactory();
			var serviceLogger = new ServiceTaskLogForTesting();
			var logger = new XmlSessionTracker(serviceLogger);
			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.None),
				AddressShortCode = address.OA_Code,
				Address1 = address.OA_Address1,
				OrganizationCode = TestObjectCreator.Creditor1.OH_Code,
				Contact = contact.OC_ContactName,
				Email = contact.OC_Email
			};
			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), Country = Country.New(RefCountry.LoadFromCountryCode(Factory, "NZ")) },
				OrganizationAddress = orgAddress,

				TransactionDate = ZDateTime.Today.AddDays(-3),
				DueDate = ZDateTime.Today.AddDays(2),
				PostDate = ZDateTime.Today.AddDays(-1),
				Number = "1234",
				NumberOfSupportingDocuments = 5,
				Description = "Some text",
				Branch = new Branch { Code = TestObjectCreator.NonCurrentBranch.GB_Code },
				Department = new Department { Code = TestObjectCreator.NonCurrentDepartment.GE_Code },

				OSCurrency = new Currency { Code = "GBP" },
				LocalCurrency = new Currency { Code = "AUD" },
				OSExGSTVATAmount = 120,
				LocalExVATAmount = 60,
				OSGSTVATAmount = 10,
				LocalVATAmount = 5
			};

			universalTransaction.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ORP } } });
			Assert("Precondition: HasRecipientRole", universalTransaction.HasRecipientRole(RecipientRoleType.ORP));

			AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", !result);
			var createdInvoices = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(0, createdInvoices.Length);
			Assert("logger.HasErrors", logger.HasErrors);
			AssertEquals("Error - Transaction Ledger is not supported", logger.ToString());

			AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			universalTransaction.Ledger = LedgerTypes.AccountsReceivable;
			universalFactory = new UniversalObjectFactory();
			serviceLogger = new ServiceTaskLogForTesting();
			logger = new XmlSessionTracker(serviceLogger);
			universalTransaction.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.IDB } } });
			Assert("Precondition: HasRecipientRole", universalTransaction.HasRecipientRole(RecipientRoleType.IDB));
			result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			createdInvoices = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			var newFactory = new BusinessObjectFactory();
			var savedInvoice = newFactory.Load<InvoicingBase>(createdInvoices[0].PK);
			AssertType(typeof(TransactionPendingAllocation), savedInvoice);
			AssertEquals("Organization", ZGuid.Empty, savedInvoice.AH_OH);
			AssertEquals("Address", ZGuid.Empty, savedInvoice.AH_OA_InvoiceAddressOverride);
			AssertEquals("Contact", ZGuid.Empty, savedInvoice.AH_OC_InvoiceContactOverride);
			AssertEquals("TransactionDate", universalTransaction.TransactionDate, savedInvoice.AH_InvoiceDate);
			AssertEquals("PostDate", universalTransaction.PostDate, savedInvoice.AH_PostDate);
			AssertEquals("TransactionNumber", universalTransaction.Number, savedInvoice.AH_TransactionNum);
			AssertEquals("NumberOfSupportingDocuments", universalTransaction.NumberOfSupportingDocuments, (int)savedInvoice.AH_NumberOfSupportingDocuments);
			AssertEquals("Currency", universalTransaction.OSCurrency.Code, savedInvoice.AH_RX_NKTransactionCurrency);
			AssertEquals("ExchangeRate", 2m, savedInvoice.AH_ExchangeRate);
			AssertEquals("OSExTaxAmount", 130m, savedInvoice.AH_OSExTaxAmount);
			AssertEquals("OSTaxAmount", 0m, savedInvoice.AH_OSTaxAmount);
			AssertEquals("LocalExTaxAmount", 65m, savedInvoice.AH_LocalExTaxAmount);
			AssertEquals("LocalTaxAmount", 0m, savedInvoice.AH_LocalTaxAmount);
			AssertEquals("Transaction type", TransactionTypes.InvoicePendingAllocation, savedInvoice.AH_TransactionType);
			AssertEquals("Branch", GlbBranch.CurrentBranch.PK, savedInvoice.AH_GB);
			AssertEquals("Department", TestObjectCreator.NonCurrentDepartment.PK, savedInvoice.AH_GE);
			Assert("logger.HasErrors", !logger.HasErrors);
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);

			var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			orgProxy.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			Factory.Save();
			universalTransaction.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			universalTransaction.BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), Country = Country.New(GlbCompany.CurrentCompany.Country) };
			universalFactory = new UniversalObjectFactory();
			serviceLogger = new ServiceTaskLogForTesting();
			logger = new XmlSessionTracker(serviceLogger);
			universalTransaction.Number = string.Format(CultureInfo.InvariantCulture, "{0}_2", universalTransaction.Number);
			result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			createdInvoices = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			newFactory = new BusinessObjectFactory();
			savedInvoice = newFactory.Load<InvoicingBase>(createdInvoices[0].PK);
			AssertType(typeof(TransactionPendingAllocation), savedInvoice);
			AssertEquals("Organization", GlbCompany.CurrentCompany.GC_OH_OrgProxy, savedInvoice.AH_OH);
			AssertEquals("Address", ZGuid.Empty, savedInvoice.AH_OA_InvoiceAddressOverride);
			AssertEquals("Contact", ZGuid.Empty, savedInvoice.AH_OC_InvoiceContactOverride);
			AssertEquals("TransactionDate", universalTransaction.TransactionDate, savedInvoice.AH_InvoiceDate);
			AssertEquals("PostDate", universalTransaction.PostDate, savedInvoice.AH_PostDate);
			AssertEquals("TransactionNumber", universalTransaction.Number, savedInvoice.AH_TransactionNum);
			AssertEquals("NumberOfSupportingDocuments", universalTransaction.NumberOfSupportingDocuments, (int)savedInvoice.AH_NumberOfSupportingDocuments);
			AssertEquals("Currency", universalTransaction.OSCurrency.Code, savedInvoice.AH_RX_NKTransactionCurrency);
			AssertEquals("ExchangeRate", 2m, savedInvoice.AH_ExchangeRate);
			AssertEquals("OSExTaxAmount", 120m, savedInvoice.AH_OSExTaxAmount);
			AssertEquals("OSTaxAmount", 10m, savedInvoice.AH_OSTaxAmount);
			AssertEquals("LocalExTaxAmount", 60m, savedInvoice.AH_LocalExTaxAmount);
			AssertEquals("LocalTaxAmount", 5m, savedInvoice.AH_LocalTaxAmount);
			AssertEquals("Transaction type", TransactionTypes.InvoicePendingAllocation, savedInvoice.AH_TransactionType);
			AssertEquals("Branch", GlbBranch.CurrentBranch.PK, savedInvoice.AH_GB);
			AssertEquals("Department", TestObjectCreator.NonCurrentDepartment.PK, savedInvoice.AH_GE);
			Assert("logger.HasErrors", !logger.HasErrors);
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);

			AssertEquals("TotalErrorCount", 0, ErrorReporter.TotalErrorCount);
			Assert(logger.HasWarnings);
			var logs = ((ISimpleLogResult)logger).Logs.ToList();
			var expectedWarning = "Transaction Header Branch Address is missing or incomplete. You must at-least populate BranchAddress/AddressType and BranchAddress/Country/Code when importing XML Universal Transactions.";
			AssertEquals("Branch Address Warning added only once", 1, logs.Where(x => x.Type == LogType.Warning && x.Message.Contains(expectedWarning)).Count());
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);

			OrgPatternMatchOverride patternMatchOverride = AddMatchingRuleForOrganization(orgProxy, universalTransaction.DataContext.DataProviderForCodeMapping, TestObjectCreator.Creditor1);

			TestObjectCreator.Creditor1.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;
			Factory.Save();
			universalFactory = new UniversalObjectFactory();
			serviceLogger = new ServiceTaskLogForTesting();
			logger = new XmlSessionTracker(serviceLogger);
			universalTransaction.Number = string.Format(CultureInfo.InvariantCulture, "{0}_3", universalTransaction.Number);
			result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			createdInvoices = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			newFactory = new BusinessObjectFactory();
			savedInvoice = newFactory.Load<InvoicingBase>(createdInvoices[0].PK);
			AssertType(typeof(TransactionPendingAllocation), savedInvoice);
			AssertEquals("Organization", TestObjectCreator.Creditor1.PK, savedInvoice.AH_OH);
			AssertEquals("Address", ZGuid.Empty, savedInvoice.AH_OA_InvoiceAddressOverride);
			AssertEquals("Contact", ZGuid.Empty, savedInvoice.AH_OC_InvoiceContactOverride);
			AssertEquals("TransactionDate", universalTransaction.TransactionDate, savedInvoice.AH_InvoiceDate);
			AssertEquals("PostDate", universalTransaction.PostDate, savedInvoice.AH_PostDate);
			AssertEquals("TransactionNumber", universalTransaction.Number, savedInvoice.AH_TransactionNum);
			AssertEquals("NumberOfSupportingDocuments", universalTransaction.NumberOfSupportingDocuments, (int)savedInvoice.AH_NumberOfSupportingDocuments);
			AssertEquals("Currency", universalTransaction.OSCurrency.Code, savedInvoice.AH_RX_NKTransactionCurrency);
			AssertEquals("ExchangeRate", 2m, savedInvoice.AH_ExchangeRate);
			AssertEquals("OSExTaxAmount", 130m, savedInvoice.AH_OSExTaxAmount);
			AssertEquals("OSTaxAmount", 0m, savedInvoice.AH_OSTaxAmount);
			AssertEquals("LocalExTaxAmount", 65m, savedInvoice.AH_LocalExTaxAmount);
			AssertEquals("LocalTaxAmount", 0m, savedInvoice.AH_LocalTaxAmount);
			AssertEquals("Transaction type", TransactionTypes.InvoicePendingAllocation, savedInvoice.AH_TransactionType);
			AssertEquals("Branch", GlbBranch.CurrentBranch.PK, savedInvoice.AH_GB);
			AssertEquals("Department", TestObjectCreator.NonCurrentDepartment.PK, savedInvoice.AH_GE);
			Assert("logger.HasErrors", !logger.HasErrors);
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
		}

		public void TestCrossLedgerImportTransaction_CreditorSettingLogic()
		{
			AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var importer = new TransactionImporter();
			var message = new BusinessObjectFactory().New<EDIMessage>();
			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.Ledger = LedgerTypes.AccountsReceivable;
			universalTransaction.DataContext = DataContextFactory.New();
			universalTransaction.DataContext.SetCompanyAndDataProviderDetails(TestObjectCreator.NonCurrentCompany);
			universalTransaction.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.IDB } } });
			Assert("Precondition: HasRecipientRole", universalTransaction.HasRecipientRole(RecipientRoleType.IDB));

			var dataContext = (DataContext)universalTransaction.DataContext;
			var expectedDataProviderValue = dataContext.DataProvider;
			dataContext.DataProvider = null;
			AssertEquals("Precondition: DataProviderForCodeMapping", ZString.Empty, universalTransaction.DataContext.DataProviderForCodeMapping);

			var universalFactory = new UniversalObjectFactory();
			var serviceLogger = new ServiceTaskLogForTesting();
			var logger = new XmlSessionTracker(serviceLogger);
			universalTransaction.Number = string.Format(CultureInfo.InvariantCulture, "{0}_1", universalTransaction.Number);
			message.EM_MessageText = universalTransaction.Serialize();
			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			var createdInvoices = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			var newFactory = new BusinessObjectFactory();
			var savedInvoice = newFactory.Load<InvoicingBase>(createdInvoices[0].PK);
			AssertType(typeof(TransactionPendingAllocation), savedInvoice);
			AssertEquals("Transaction type", TransactionTypes.InvoicePendingAllocation, savedInvoice.AH_TransactionType);
			Assert("logger.HasErrors", !logger.HasErrors);
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
			AssertEquals("Organization", TestObjectCreator.NonCurrentCompany.GC_OH_OrgProxy, savedInvoice.AH_OH);
			AssertEquals("UniversalTransaction.Creditor", ZString.Empty, savedInvoice.AllocationApprovalRequest.PostingDetails.UniversalTransaction.Creditor);
			AssertEquals("UniversalTransaction.CreditorSource", ZString.Empty, savedInvoice.AllocationApprovalRequest.PostingDetails.UniversalTransaction.CreditorSource);

			AssertEquals("TotalErrorCount", 0, ErrorReporter.TotalErrorCount);
			Assert(logger.HasWarnings);
			var logs = ((ISimpleLogResult)logger).Logs.ToList();
			var expectedWarning = "Transaction Header Branch Address is missing or incomplete. You must at-least populate BranchAddress/AddressType and BranchAddress/Country/Code when importing XML Universal Transactions.";
			AssertEquals("Branch Address Warning added only once", 1, logs.Where(x => x.Type == LogType.Warning && x.Message.Contains(expectedWarning)).Count());
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);

			dataContext.DataProvider = expectedDataProviderValue;
			AssertEquals("Precondition: DataProviderForCodeMapping", expectedDataProviderValue, universalTransaction.DataContext.DataProviderForCodeMapping);

			universalFactory = new UniversalObjectFactory();
			serviceLogger = new ServiceTaskLogForTesting();
			logger = new XmlSessionTracker(serviceLogger);
			universalTransaction.Number = string.Format(CultureInfo.InvariantCulture, "{0}_1", universalTransaction.Number);
			message.EM_MessageText = universalTransaction.Serialize();
			result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			createdInvoices = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			newFactory = new BusinessObjectFactory();
			savedInvoice = newFactory.Load<InvoicingBase>(createdInvoices[0].PK);
			AssertType(typeof(TransactionPendingAllocation), savedInvoice);
			AssertEquals("Transaction type", TransactionTypes.InvoicePendingAllocation, savedInvoice.AH_TransactionType);
			Assert("logger.HasErrors", !logger.HasErrors);
			AssertEquals("Organization", TestObjectCreator.NonCurrentCompany.GC_OH_OrgProxy, savedInvoice.AH_OH);
			AssertEquals("UniversalTransaction.Creditor", ZString.Empty, savedInvoice.AllocationApprovalRequest.PostingDetails.UniversalTransaction.Creditor);
			AssertEquals("UniversalTransaction.CreditorSource", expectedDataProviderValue, savedInvoice.AllocationApprovalRequest.PostingDetails.UniversalTransaction.CreditorSource);

			AssertEquals("TotalErrorCount", 0, ErrorReporter.TotalErrorCount);
			Assert(logger.HasWarnings);
			logs = ((ISimpleLogResult)logger).Logs.ToList();
			AssertEquals("Branch Address Warning added only once", 1, logs.Where(x => x.Type == LogType.Warning && x.Message.Contains(expectedWarning)).Count());
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);

			universalFactory = new UniversalObjectFactory();
			serviceLogger = new ServiceTaskLogForTesting();
			logger = new XmlSessionTracker(serviceLogger);
			universalTransaction.BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), OrganizationCode = GlbCompany.CurrentCompany.OrgProxy.OH_Code, Country = Country.New(GlbCompany.CurrentCompany.Country) };
			universalTransaction.Number = string.Format(CultureInfo.InvariantCulture, "{0}_2", universalTransaction.Number);
			message.EM_MessageText = universalTransaction.Serialize();
			result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			createdInvoices = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			newFactory = new BusinessObjectFactory();
			savedInvoice = newFactory.Load<InvoicingBase>(createdInvoices[0].PK);
			AssertType(typeof(TransactionPendingAllocation), savedInvoice);
			AssertEquals("Transaction type", TransactionTypes.InvoicePendingAllocation, savedInvoice.AH_TransactionType);
			Assert("logger.HasErrors", !logger.HasErrors);
			AssertEquals("Organization", GlbCompany.CurrentCompany.GC_OH_OrgProxy, savedInvoice.AH_OH);
			AssertEquals("UniversalTransaction.Creditor", ZString.Empty, savedInvoice.AllocationApprovalRequest.PostingDetails.UniversalTransaction.Creditor);
			AssertEquals("UniversalTransaction.CreditorSource", expectedDataProviderValue, savedInvoice.AllocationApprovalRequest.PostingDetails.UniversalTransaction.CreditorSource);
			AssertEquals("TotalErrorCount", 0, ErrorReporter.TotalErrorCount);
			logs = ((ISimpleLogResult)logger).Logs.ToList();
			Assert(!logs.Any(x => x.Type == LogType.Warning && x.Message == expectedWarning));
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);

			var orgProxy = new BusinessObjectFactory().Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			AddMatchingRuleForOrganization(orgProxy, universalTransaction.DataContext.DataProviderForCodeMapping, TestObjectCreator.Creditor1);
			orgProxy.Factory.Save();

			universalFactory = new UniversalObjectFactory();
			serviceLogger = new ServiceTaskLogForTesting();
			logger = new XmlSessionTracker(serviceLogger);
			universalTransaction.Number = string.Format(CultureInfo.InvariantCulture, "{0}_1", universalTransaction.Number);
			message.EM_MessageText = universalTransaction.Serialize();
			result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			createdInvoices = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			newFactory = new BusinessObjectFactory();
			savedInvoice = newFactory.Load<InvoicingBase>(createdInvoices[0].PK);
			AssertType(typeof(TransactionPendingAllocation), savedInvoice);
			AssertEquals("Transaction type", TransactionTypes.InvoicePendingAllocation, savedInvoice.AH_TransactionType);
			Assert("logger.HasErrors", !logger.HasErrors);
			Assert(!logs.Any(x => x.Type == LogType.Warning && x.Message == expectedWarning));
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
			AssertEquals("Organization", TestObjectCreator.Creditor1.PK, savedInvoice.AH_OH);
			AssertEquals("UniversalTransaction.Creditor", TestObjectCreator.Creditor1.OH_Code, savedInvoice.AllocationApprovalRequest.PostingDetails.UniversalTransaction.Creditor);
			AssertEquals("UniversalTransaction.CreditorSource", expectedDataProviderValue, savedInvoice.AllocationApprovalRequest.PostingDetails.UniversalTransaction.CreditorSource);
		}

		[TestDate(2017, 07, 10)]
		public void TestCrossLedgerImportSkippedIfNettingEnabledAndOriginatedFromSameSystem()
		{
			AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var message = new BusinessObjectFactory().New<EDIMessage>();
			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Ledger = LedgerTypes.AccountsReceivable,
				DataContext = DataContextFactory.New(),
				BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), OrganizationCode = TestObjectCreator.NonCurrentCompany.OrgProxy.OH_Code, Country = Country.New(TestObjectCreator.NonCurrentCompany.Country) }
			};
			universalTransaction.DataContext.SetCompanyAndDataProviderDetails(TestObjectCreator.NonCurrentCompany);
			universalTransaction.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.IDB } } });
			Assert("Precondition: HasRecipientRole", universalTransaction.HasRecipientRole(RecipientRoleType.IDB));

			var dataContext = (DataContext)universalTransaction.DataContext;
			var originalServerID = dataContext.ServerID;

			var nettingStartDateRegistryValue = AccountingConfigurationRegistry.Instance.NettingStartDate.Value;
			Assert("Precondition: Netting Start Date is not set", nettingStartDateRegistryValue == AccountingConfigurationRegistry.Instance.NettingStartDate.DefaultValue);

			dataContext.ServerID = "123";
			Assert("Precondition: IsFromSameSystem is false", !universalTransaction.DataContext.IsFromSameSystem());

			AssertTPACreation(message, universalTransaction, true);

			dataContext.ServerID = originalServerID;
			Assert("Precondition: IsFromSameSystem is true", universalTransaction.DataContext.IsFromSameSystem());

			AssertTPACreation(message, universalTransaction, true);

			AccountingConfigurationRegistry.Instance.NettingStartDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(10).ToDateTime());
			nettingStartDateRegistryValue = AccountingConfigurationRegistry.Instance.NettingStartDate.Value;
			Assert("Precondition: Netting Start Date is set and in the future", ZDateTime.UtcNow < nettingStartDateRegistryValue.ToUniversalTime());

			dataContext.ServerID = "123";
			Assert("Precondition: IsFromSameSystem is false", !universalTransaction.DataContext.IsFromSameSystem());

			AssertTPACreation(message, universalTransaction, true);

			dataContext.ServerID = originalServerID;
			Assert("Precondition: IsFromSameSystem is true", universalTransaction.DataContext.IsFromSameSystem());

			AssertTPACreation(message, universalTransaction, true);

			AccountingConfigurationRegistry.Instance.NettingStartDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-10).ToDateTime());
			nettingStartDateRegistryValue = AccountingConfigurationRegistry.Instance.NettingStartDate.Value;
			Assert("Precondition: Netting Start Date is set and in the past, that means netting is up and running", ZDateTime.UtcNow > nettingStartDateRegistryValue.ToUniversalTime());

			dataContext.ServerID = "123";
			Assert("Precondition: IsFromSameSystem is false", !universalTransaction.DataContext.IsFromSameSystem());

			AssertTPACreation(message, universalTransaction, true);

			dataContext.ServerID = originalServerID;
			Assert("Precondition: IsFromSameSystem is true", universalTransaction.DataContext.IsFromSameSystem());

			AssertTPACreation(message, universalTransaction, true);

			universalTransaction.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.IDB }, new RecipientRoleDetail { Type = RecipientRoleType.WNS } } });

			AssertTPACreation(message, universalTransaction, false);
		}

		public void TestCrossLedgerImportTransactionLines_NVOCC_ShipmentInvoice()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_CoLoadMasterBill = "MasterBill1";
			consol.JK_OA_CreditorAddress = TestObjectCreator.Creditor1.MainAddress.PK;
			var shipment1 = TestObjectCreator.CreateShipment("S001", consol);
			shipment1.JS_HouseBill = "HouseBill1";
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			var shipment2 = TestObjectCreator.CreateShipment("S002", consol);
			shipment2.JS_HouseBill = "HouseBill2";
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			Factory.Save();

			var importer = new TransactionImporter();
			var invoice = new BusinessObjectFactory().New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			universalTransaction.Job = new EntityReference { Type = AccountingDataTransferConstants.DataContextTypeString.Job, Key = "S123456" };
			universalTransaction.JobInvoiceNumber = "S123456";

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.IsFinalCharge = true;
			universalLine.OSAmount = 100;
			universalLine.Job = new EntityReference { Type = AccountingDataTransferConstants.DataContextTypeString.Job, Key = "S123456" };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalLine2 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine2.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC2.AC_Code };
			universalLine2.IsFinalCharge = true;
			universalLine2.OSAmount = 1000;
			universalLine2.Job = new EntityReference { Type = AccountingDataTransferConstants.DataContextTypeString.Job, Key = "S123456" };
			universalTransaction.PostingJournalCollection.Add(universalLine2);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123456");
			universalShipment.ShipmentType = new UniversalCodeDescriptionPair { Code = Constants.ShipmentTypes.CoLoadMaster };
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			universalShipment.WayBillNumber = "MasterBill1";
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var universalTransactionXml = universalTransaction.Serialize();

			int invoiceLinesListChangedHitCount = 0;
			var listChangedHandler = new ListChangedEventHandler(
				(sender, e) =>
				{ invoiceLinesListChangedHitCount++; }
			);
			((IBindingList)invoice.Lines).ListChanged += listChangedHandler;

			using (eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				importer.ImportTransactionLines(universalTransactionXml, invoice, true);
			}

			AssertEquals("ListChanged on invoice.Lines should be called once", 1, invoiceLinesListChangedHitCount);
			AssertEquals(4, invoice.Lines.Count);

			var lines = invoice.Lines.OfType<InvoicingLineBase>();
			AssertNVOCCLine(lines, shipment1, TestObjectCreator.CC1, consol, 50m, 50m);
			AssertNVOCCLine(lines, shipment1, TestObjectCreator.CC2, consol, 500m, 500m);
			AssertNVOCCLine(lines, shipment2, TestObjectCreator.CC1, consol, 50m, 50m);
			AssertNVOCCLine(lines, shipment2, TestObjectCreator.CC2, consol, 500m, 500m);

			AssertEquals("ConsolCosts.Count", 2, invoice.ConsolCosting.ConsolCosts.Count);
			var consolCosts = invoice.ConsolCosting.ConsolCosts.OfType<JobConsolCost>();
			AssertNVOCCConsolCost(consolCosts, TestObjectCreator.CC1, consol, AllocationMethod.ChargeableUnits, 100m, 0m, 2, 2);
			AssertNVOCCConsolCost(consolCosts, TestObjectCreator.CC2, consol, AllocationMethod.ChargeableUnits, 1000m, 0m, 2, 2);
			Assert("ConsolCosting.HasErrors", !invoice.ConsolCosting.HasErrors);
			Assert(invoice.GetLogs().HasLogWith(StmALogSchema.SL_Reference, "Matched to Consol [C001] Co-load MBL=[MasterBill1] and Co-Load With=[ZCreditor1]"));

			invoice.ReleaseAllMutexOnInvoice();
		}

		public void TestCrossLedgerImportTransactionLines_NVOCC_PeriodicInvoice()
		{
			var consol1 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			consol1.JK_AgentType = Constants.AgentType.CoLoad;
			consol1.JK_CoLoadMasterBill = "MasterBill1";
			consol1.JK_OA_CreditorAddress = TestObjectCreator.Creditor1.MainAddress.PK;
			var shipment1 = TestObjectCreator.CreateShipment("S001", consol1);
			shipment1.JS_HouseBill = "HouseBill1";
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			var shipment2 = TestObjectCreator.CreateShipment("S002", consol1);
			shipment2.JS_HouseBill = "HouseBill2";
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			Factory.Save();

			var consol2 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C002");
			consol2.JK_AgentType = Constants.AgentType.CoLoad;
			consol2.JK_CoLoadMasterBill = "MasterBill2";
			consol2.JK_OA_CreditorAddress = TestObjectCreator.Creditor1.MainAddress.PK;
			var shipment3 = TestObjectCreator.CreateShipment("S003", consol2);
			shipment3.JS_HouseBill = "HouseBill3";
			shipment3.JS_TransportMode = Constants.TransportModes.Air;
			var shipment4 = TestObjectCreator.CreateShipment("S004", consol2);
			shipment4.JS_HouseBill = "HouseBill4";
			shipment4.JS_TransportMode = Constants.TransportModes.Air;
			Factory.Save();

			var importer = new TransactionImporter();
			var invoice = new BusinessObjectFactory().New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			universalTransaction.Job = new EntityReference { Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			universalTransaction.JobInvoiceNumber = string.Empty;

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.IsFinalCharge = true;
			universalLine.OSAmount = 100;
			universalLine.Job = new EntityReference { Type = AccountingDataTransferConstants.DataContextTypeString.Job, Key = "S123456" };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalLine2 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine2.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC2.AC_Code };
			universalLine2.IsFinalCharge = true;
			universalLine2.OSAmount = 1000;
			universalLine2.Job = new EntityReference { Type = AccountingDataTransferConstants.DataContextTypeString.Job, Key = "S123456" };
			universalTransaction.PostingJournalCollection.Add(universalLine2);

			var universalLine3 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine3.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine3.IsFinalCharge = true;
			universalLine3.OSAmount = 300;
			universalLine3.Job = new EntityReference { Type = AccountingDataTransferConstants.DataContextTypeString.Job, Key = "S789000" };
			universalTransaction.PostingJournalCollection.Add(universalLine3);

			var universalLine4 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine4.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC2.AC_Code };
			universalLine4.IsFinalCharge = true;
			universalLine4.OSAmount = 3000;
			universalLine4.Job = new EntityReference { Type = AccountingDataTransferConstants.DataContextTypeString.Job, Key = "S789000" };
			universalTransaction.PostingJournalCollection.Add(universalLine4);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123456");
			universalShipment.ShipmentType = new UniversalCodeDescriptionPair { Code = Constants.ShipmentTypes.CoLoadMaster };
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			universalShipment.WayBillNumber = "MasterBill1";
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var universalShipment2 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment2.DataContext = DataContextFactory.New();
			universalShipment2.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S789000");
			universalShipment2.ShipmentType = new UniversalCodeDescriptionPair { Code = Constants.ShipmentTypes.CoLoadMaster };
			universalShipment2.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			universalShipment2.WayBillNumber = "MasterBill2";
			universalShipment2.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			universalTransaction.ShipmentCollection.Add(universalShipment2);

			var universalTransactionXml = universalTransaction.Serialize();

			int invoiceLinesListChangedHitCount = 0;
			var listChangedHandler = new ListChangedEventHandler(
				(sender, e) =>
				{ invoiceLinesListChangedHitCount++; }
			);
			((IBindingList)invoice.Lines).ListChanged += listChangedHandler;

			using (eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				importer.ImportTransactionLines(universalTransactionXml, invoice, true);
			}

			AssertEquals("ListChanged on invoice.Lines should be called once", 1, invoiceLinesListChangedHitCount);
			AssertEquals(8, invoice.Lines.Count);

			var lines = invoice.Lines.OfType<InvoicingLineBase>();
			AssertNVOCCLine(lines, shipment1, TestObjectCreator.CC1, consol1, 50m, 50m);
			AssertNVOCCLine(lines, shipment1, TestObjectCreator.CC2, consol1, 500m, 500m);
			AssertNVOCCLine(lines, shipment2, TestObjectCreator.CC1, consol1, 50m, 50m);
			AssertNVOCCLine(lines, shipment2, TestObjectCreator.CC2, consol1, 500m, 500m);
			AssertNVOCCLine(lines, shipment3, TestObjectCreator.CC1, consol2, 150m, 150m);
			AssertNVOCCLine(lines, shipment3, TestObjectCreator.CC2, consol2, 1500m, 1500m);
			AssertNVOCCLine(lines, shipment4, TestObjectCreator.CC1, consol2, 150m, 150m);
			AssertNVOCCLine(lines, shipment4, TestObjectCreator.CC2, consol2, 1500m, 1500m);

			AssertEquals("ConsolCosts.Count", 4, invoice.ConsolCosting.ConsolCosts.Count);
			var consolCosts = invoice.ConsolCosting.ConsolCosts.OfType<JobConsolCost>();
			AssertNVOCCConsolCost(consolCosts, TestObjectCreator.CC1, consol1, AllocationMethod.ChargeableUnits, 100m, 0m, 2, 2);
			AssertNVOCCConsolCost(consolCosts, TestObjectCreator.CC2, consol1, AllocationMethod.ChargeableUnits, 1000m, 0m, 2, 2);
			AssertNVOCCConsolCost(consolCosts, TestObjectCreator.CC1, consol2, AllocationMethod.ChargeableUnits, 300m, 0m, 2, 2);
			AssertNVOCCConsolCost(consolCosts, TestObjectCreator.CC2, consol2, AllocationMethod.ChargeableUnits, 3000m, 0m, 2, 2);
			Assert("ConsolCosting.HasErrors", !invoice.ConsolCosting.HasErrors);

			Assert(invoice.GetLogs().HasLogWith(StmALogSchema.SL_Reference, "Matched to Consol [C001] Co-load MBL=[MasterBill1] and Co-Load With=[ZCreditor1]"));
			Assert(invoice.GetLogs().HasLogWith(StmALogSchema.SL_Reference, "Matched to Consol [C002] Co-load MBL=[MasterBill2] and Co-Load With=[ZCreditor1]"));

			invoice.ReleaseAllMutexOnInvoice();
		}

		public void TestCrossLedgerImportTransactionLines_NVOCC_ConsolInvoice()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_MasterBillNum = "MasterBill1";
			consol.JK_CoLoadMasterBill = string.Empty;
			consol.JK_OA_CreditorAddress = TestObjectCreator.Creditor1.MainAddress.PK;
			var shipment1 = TestObjectCreator.CreateShipment("S001", consol);
			shipment1.JS_HouseBill = "HouseBill1";
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			var shipment2 = TestObjectCreator.CreateShipment("S002", consol);
			shipment2.JS_HouseBill = "HouseBill2";
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			Factory.Save();

			var consol2 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C002");
			consol2.JK_AgentType = Constants.AgentType.CoLoad;
			consol2.JK_MasterBillNum = "MasterBill2";
			consol2.JK_CoLoadMasterBill = string.Empty;
			consol2.JK_OA_CreditorAddress = TestObjectCreator.Creditor1.MainAddress.PK;
			var shipment3 = TestObjectCreator.CreateShipment("S003", consol2);
			shipment3.JS_HouseBill = "HouseBill3";
			shipment3.JS_TransportMode = Constants.TransportModes.Air;
			var shipment4 = TestObjectCreator.CreateShipment("S004", consol2);
			shipment4.JS_HouseBill = "HouseBill4";
			shipment4.JS_TransportMode = Constants.TransportModes.Air;
			Factory.Save();

			var importer = new TransactionImporter();
			var invoice = new BusinessObjectFactory().New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			universalTransaction.Job = new EntityReference { Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			universalTransaction.JobInvoiceNumber = "C001/A";

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.IsFinalCharge = true;
			universalLine.OSAmount = 100;
			universalLine.Job = new EntityReference { Type = AccountingDataTransferConstants.DataContextTypeString.Job, Key = "S123456" };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C001");
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
			universalShipment.WayBillNumber = "MasterBill1";
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			universalTransaction.ShipmentCollection.Add(universalShipment);

			universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123456");
			universalShipment.ShipmentType = new UniversalCodeDescriptionPair { Code = Constants.ShipmentTypes.CoLoadMaster };
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			universalShipment.WayBillNumber = "MasterBill2";
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var universalTransactionXml = universalTransaction.Serialize();

			int invoiceLinesListChangedHitCount = 0;
			var listChangedHandler = new ListChangedEventHandler(
				(sender, e) =>
				{ invoiceLinesListChangedHitCount++; }
			);
			((IBindingList)invoice.Lines).ListChanged += listChangedHandler;

			using (eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				importer.ImportTransactionLines(universalTransactionXml, invoice, true);
			}

			AssertEquals("ListChanged on invoice.Lines should be called once", 1, invoiceLinesListChangedHitCount);
			AssertEquals(2, invoice.Lines.Count);

			var lines = invoice.Lines.OfType<InvoicingLineBase>();
			AssertEquals("ConsolCosts.Count", 1, invoice.ConsolCosting.ConsolCosts.Count);
			var consolCosts = invoice.ConsolCosting.ConsolCosts.OfType<JobConsolCost>();
			AssertNVOCCConsolCost(consolCosts, TestObjectCreator.CC1, consol, AllocationMethod.ChargeableUnits, 100m, 0m, 2, 2);
			Assert("ConsolCosting.HasErrors", !invoice.ConsolCosting.HasErrors);

			invoice.ReleaseAllMutexOnInvoice();
		}

		public void TestCrossLedgerImportTransactionLines_NVOCC_ShipmentInvoice_NoConsolMatched()
		{
			var importer = new TransactionImporter();
			var invoice = new BusinessObjectFactory().New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			universalTransaction.Job = new EntityReference { Type = AccountingDataTransferConstants.DataContextTypeString.Job, Key = "S123456" };
			universalTransaction.JobInvoiceNumber = "S123456";

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.IsFinalCharge = true;
			universalLine.OSAmount = 100;
			universalLine.Job = new EntityReference { Type = AccountingDataTransferConstants.DataContextTypeString.Job, Key = "S123456" };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalLine2 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine2.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC2.AC_Code };
			universalLine2.IsFinalCharge = true;
			universalLine2.OSAmount = 1000;
			universalLine2.Job = new EntityReference { Type = AccountingDataTransferConstants.DataContextTypeString.Job, Key = "S123456" };
			universalTransaction.PostingJournalCollection.Add(universalLine2);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123456");
			universalShipment.ShipmentType = new UniversalCodeDescriptionPair { Code = Constants.ShipmentTypes.CoLoadMaster };
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			universalShipment.WayBillNumber = "MasterBill1";
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var universalTransactionXml = universalTransaction.Serialize();

			int invoiceLinesListChangedHitCount = 0;
			var listChangedHandler = new ListChangedEventHandler(
				(sender, e) =>
				{ invoiceLinesListChangedHitCount++; }
			);
			((IBindingList)invoice.Lines).ListChanged += listChangedHandler;

			using (eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				importer.ImportTransactionLines(universalTransactionXml, invoice, true);
			}

			AssertEquals("ListChanged on invoice.Lines should be called once", 1, invoiceLinesListChangedHitCount);
			AssertEquals(2, invoice.Lines.Count);
			var lines = invoice.Lines.OfType<InvoicingLineBase>();
			var expectedJobErrors = new string[] { "You must select a job for this charge code." };
			var expectedJobWarnings = new string[] { "Source XML errors: Operational job ForwardingShipment with DataSource Key 'S123456' is not found." };
			AssertNVOCCLine(lines, null, TestObjectCreator.CC1, null, 100m, 100m, expectedJobErrors, expectedJobWarnings);
			AssertNVOCCLine(lines, null, TestObjectCreator.CC2, null, 1000m, 1000m, expectedJobErrors, expectedJobWarnings);
			AssertEquals("ConsolCosts.Count", 0, invoice.ConsolCosting.ConsolCosts.Count);
			var log = invoice.GetLogs().MostRecentLogByEventTime(Events.EditedARecord);
			AssertNotNull(log);
			AssertEquals("No Consol found with Co-Load MBL=[MasterBill1] and Co-Load With=[ZCreditor1]", log.SL_Reference);

			invoice.ReleaseAllMutexOnInvoice();
		}

		public void TestCrossLedgerImportTransactionLines_NVOCC_ShipmentInvoice_MultipleConsolsMatched()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_CoLoadMasterBill = "MasterBill1";
			consol.JK_OA_CreditorAddress = TestObjectCreator.Creditor1.MainAddress.PK;
			var shipment1 = TestObjectCreator.CreateShipment("S001", consol);
			shipment1.JS_HouseBill = "HouseBill1";
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			var shipment2 = TestObjectCreator.CreateShipment("S002", consol);
			shipment2.JS_HouseBill = "HouseBill2";
			shipment2.JS_TransportMode = Constants.TransportModes.Air;

			var consol2 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C002");
			consol2.JK_AgentType = Constants.AgentType.CoLoad;
			consol2.JK_CoLoadMasterBill = "MasterBill1";
			consol2.JK_OA_CreditorAddress = TestObjectCreator.Creditor1.MainAddress.PK;
			var shipment3 = TestObjectCreator.CreateShipment("S003", consol2);
			shipment3.JS_HouseBill = "HouseBill3";
			shipment3.JS_TransportMode = Constants.TransportModes.Air;
			var shipment4 = TestObjectCreator.CreateShipment("S004", consol2);
			shipment4.JS_HouseBill = "HouseBill4";
			shipment4.JS_TransportMode = Constants.TransportModes.Air;
			Factory.Save();

			var importer = new TransactionImporter();
			var invoice = new BusinessObjectFactory().New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			universalTransaction.Job = new EntityReference { Type = AccountingDataTransferConstants.DataContextTypeString.Job, Key = "S123456" };
			universalTransaction.JobInvoiceNumber = "S123456";

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.IsFinalCharge = true;
			universalLine.OSAmount = 100;
			universalLine.Job = new EntityReference { Type = AccountingDataTransferConstants.DataContextTypeString.Job, Key = "S123456" };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalLine2 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine2.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC2.AC_Code };
			universalLine2.IsFinalCharge = true;
			universalLine2.OSAmount = 1000;
			universalLine2.Job = new EntityReference { Type = AccountingDataTransferConstants.DataContextTypeString.Job, Key = "S123456" };
			universalTransaction.PostingJournalCollection.Add(universalLine2);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123456");
			universalShipment.ShipmentType = new UniversalCodeDescriptionPair { Code = Constants.ShipmentTypes.CoLoadMaster };
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			universalShipment.WayBillNumber = "MasterBill1";
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var universalTransactionXml = universalTransaction.Serialize();

			int invoiceLinesListChangedHitCount = 0;
			var listChangedHandler = new ListChangedEventHandler(
				(sender, e) =>
				{ invoiceLinesListChangedHitCount++; }
			);
			((IBindingList)invoice.Lines).ListChanged += listChangedHandler;

			using (eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				importer.ImportTransactionLines(universalTransactionXml, invoice, true);
			}

			AssertEquals("ListChanged on invoice.Lines should be called once", 1, invoiceLinesListChangedHitCount);
			AssertEquals(2, invoice.Lines.Count);
			var lines = invoice.Lines.OfType<InvoicingLineBase>();
			var expectedJobErrors = new string[] { "You must select a job for this charge code." };
			var expectedJobWarnings = new string[] { "Source XML errors: Operational job ForwardingShipment with DataSource Key 'S123456' is not found." };
			AssertNVOCCLine(lines, null, TestObjectCreator.CC1, null, 100m, 100m, expectedJobErrors, expectedJobWarnings);
			AssertNVOCCLine(lines, null, TestObjectCreator.CC2, null, 1000m, 1000m, expectedJobErrors, expectedJobWarnings);
			AssertEquals("ConsolCosts.Count", 0, invoice.ConsolCosting.ConsolCosts.Count);
			var log = invoice.GetLogs().MostRecentLogByEventTime(Events.EditedARecord);
			AssertNotNull(log);
			AssertEquals("Charges not matched to Consol. More than one Consol found with Co-Load MBL[MasterBill1] and Co-Load With=[ZCreditor1]. (Duplicate Consols: C001,C002)", log.SL_Reference);

			invoice.ReleaseAllMutexOnInvoice();
		}

		[ExpectNoExceptions]
		public void TestImportUniversalTransactionFromXmlCore_WithShipmentsSetAdditionalBillCollection_NoException()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.SetShipmentCollection(() => new List<Shipment>() { new Shipment(DefaultDataObjectWriterStrategy.TestInstance) });
			
			var transactionInfoXml = transactionInfo.Serialize();

			var importer = new TransactionImporter();
			var importedTransactionInfo = importer.ImportUniversalTransactionFromXml(transactionInfoXml, new BusinessObjectFactory(), false).Item1 as TransactionInfo;

			var shipment = importedTransactionInfo.ShipmentCollection.First();

			shipment.SetAdditionalBillCollection(() => new List<AdditionalBill>());

			AssertNotNull(shipment.AdditionalBillCollection);
		}

		[ExpectNoExceptions]
		public void TestImportUniversalTransactionFromXmlCore_WithSubShipmentsSetAdditionalBillCollection_NoException()
		{
			var subShipmentWithShipments = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipmentWithShipments.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { new Shipment(DefaultDataObjectWriterStrategy.TestInstance) });

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { subShipmentWithShipments });

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.SetShipmentCollection(() => new List<Shipment>() { shipment });

			var transactionInfoXml = transactionInfo.Serialize();

			var importer = new TransactionImporter();
			var importedTransactionInfo = importer.ImportUniversalTransactionFromXml(transactionInfoXml, new BusinessObjectFactory(), false).Item1 as TransactionInfo;

			var subShipment = importedTransactionInfo
				.ShipmentCollection.First()
				.SubShipmentCollection.First()
				.SubShipmentCollection.First();

			subShipment.SetAdditionalBillCollection(() => new List<AdditionalBill>());

			AssertNotNull(subShipment.AdditionalBillCollection);
		}

		void AssertNVOCCLine(IEnumerable<InvoicingLineBase> lines, ForwardingShipment shipment, AccChargeCode chargeCode, ForwardingConsol consol, ZDecimal expectedOSExTaxAmount, ZDecimal expectedLocalExTaxAmount, string[] expectedJobErrors = null, string[] expectedJobWarnings = null)
		{
			InvoicingLineBase line = null;
			if (shipment != null && consol != null)
			{
				line = lines.First(x => x.Job.JH_ParentID == shipment.PK && x.AL_AC == chargeCode.PK && x.Consol.PK == consol.PK);
				AssertNotNull(line);
				AssertNoNotifications(line.AL_JHInfo);
				AssertNoNotifications(line.ConsolIDFromApportionedChargeInfo);
			}
			else
			{
				line = lines.First(x => x.AL_JH == ZGuid.Empty && x.AL_AC == chargeCode.PK);
				AssertNotNull(line);
				expectedJobErrors.ForEach(x => AssertHasError(line.AL_JHInfo, x));
				expectedJobWarnings.ForEach(x => AssertHasWarning(line.AL_JHInfo, x));
			}
			AssertEquals("OSExTaxAmount", expectedOSExTaxAmount, line.AL_OSExTaxAmount);
			AssertEquals("LocalExTaxAmount", expectedLocalExTaxAmount, line.AL_LocalExTaxAmount);
		}

		void AssertNVOCCConsolCost(IEnumerable<JobConsolCost> consolCosts, AccChargeCode chargeCode, ForwardingConsol consol, ZString expectedAllocationMethod, ZDecimal expectedOSCostAmount,
									ZDecimal expectedUnApportionedAmount, int expectedApportionedChargesCount, int expectedUsedApportionedChargesCount)
		{
			var consolCost = consolCosts.First(x => x.E6_AC_ChargeCode == chargeCode.PK && x.E6_ParentID == consol.PK);
			AssertNotNull(consolCost);
			AssertEquals("consolCost ApportionmentMethod", expectedAllocationMethod, consolCost.E6_ApportionmentMethod);
			AssertEquals("consolCost OSCostAmount", expectedOSCostAmount, consolCost.E6_OSCostAmount);
			AssertEquals("consolCost UnApportionedAmount", expectedUnApportionedAmount, consolCost.UnApportionedAmount);
			AssertEquals("consolCost ApportionmentCharges Count", expectedApportionedChargesCount, consolCost.ApportionmentCharges.Count);
			AssertEquals("consolCost used ApportionmentCharges Count", expectedUsedApportionedChargesCount, consolCost.ApportionmentCharges.Count(x => ((ApportionSplitCharge)x).JR_OSCostAmt != 0));
		}

		public void TestImportShipmentForWarehouseInvoice()
		{
			var schema = UniversalXmlSchema.Version_2011_11;
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-1));
			TestObjectCreator.NonCurrentCompany.OrgProxy.OH_IsCreditor = true;

			var branch1 = TestObjectCreator.CreateBranch("BR6", GlbCompany.CurrentCompany);
			var dept1 = TestObjectCreator.CreateDepartment("DE6");
			var orgHeader = TestObjectCreator.CreateOrgHeader("OH6", false, true);
			var address = TestObjectCreator.CreateAddress(orgHeader, "72 Oriordan Street");
			var contact = TestObjectCreator.CreateContact(orgHeader, "Chris", "ww@www.com");

			var whsReceive = (IWhsReceive)Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsReceive>());
			whsReceive.WD_DocketID = "W001";
			whsReceive.WD_ExternalReference = "W001";
			whsReceive.WD_OH_Client = orgHeader.PK;

			var job1 = TestObjectCreator.CreateJob((IJobInvoicingPlugIn)whsReceive);
			job1.JH_GB = branch1.PK;
			job1.JH_GE = dept1.PK;

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress),
				AddressShortCode = address.OA_Code,
				Address1 = address.OA_Address1,
				OrganizationCode = orgHeader.OH_Code,
				Contact = contact.OC_ContactName,
				Email = contact.OC_Email,
			};

			Factory.Save();

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				Ledger = LedgerTypes.AccountsReceivable,
				BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), OrganizationCode = TestObjectCreator.NonCurrentCompany.OrgProxy.OH_Code },
				OrganizationAddress = orgAddress,
				Number = "1234",
				OSExGSTVATAmount = 100,
				Branch = new Branch { Code = GlbBranch.CurrentBranch.GB_Code },
				Department = new Department { Code = GlbDepartment.CurrentDepartment.GE_Code },
			};
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal> { new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { Job = new EntityReference { Key = "W001", Type = AccountingDataTransferConstants.DataContextTypeString.Job } } });
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			universalTransaction.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.IDB } } });
			Assert("Precondition: HasRecipientRole", universalTransaction.HasRecipientRole(RecipientRoleType.IDB));

			var universalShipment = TestObjectCreator.GetUniversalShipmentDataObject((BusinessObject)whsReceive, schema, whsReceive.WD_ExternalReference);
			universalShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			universalShipment.OrganizationAddressCollection.Add(orgAddress);
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var universalFactory = new UniversalObjectFactory();
			var serviceLogger = new ServiceTaskLogForTesting();
			var logger = new TestErrorLogger { TopLevelDataObject = universalTransaction };

			var message = new BusinessObjectFactory().New<EDIMessage>();
			message.EM_MessageText = universalTransaction.Serialize();

			var importer = new TransactionImporter();
			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);

			Assert("Import result", result);
			var createdInvoices = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			Assert("logger.HasErrors", !logger.HasErrors);

			var newFactory = new BusinessObjectFactory();
			var savedInvoice = newFactory.Load<TransactionPendingAllocation>(createdInvoices[0].PK);
			AssertEquals("Department should be from the Job.", dept1.PK, savedInvoice.AH_GE);
			Assert("logger.HasWarnings", logger.HasWarnings);
			AssertNotNull("Postcondition: request is created.", savedInvoice.TransactionApprovalRequest);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, savedInvoice.TransactionApprovalRequest.XP_ApprovalStatus);

			var invoice = new BusinessObjectFactory().Load<APInvoice>(savedInvoice.PK);
			invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.SubmittedFromInvoicingForm = true;

			importer.ImportTransactionLines(savedInvoice.TransactionApprovalRequest.PostingDetails.SourceXML, invoice, true);
			AssertEquals(1, invoice.Lines.Count);
			AssertEquals("Job is set on the invoice line.", job1.PK, invoice.Lines[0].AL_JH);
		}

		void AssertTPACreation(EDIMessage message, TransactionInfo universalTransaction, bool wasSuccessful)
		{
			var importer = new TransactionImporter();
			var universalFactory = new UniversalObjectFactory();
			var serviceLogger = new ServiceTaskLogForTesting();
			var logger = new XmlSessionTracker(serviceLogger);
			universalTransaction.Number = string.Format(CultureInfo.InvariantCulture, "{0}_1", universalTransaction.Number);
			message.EM_MessageText = universalTransaction.Serialize();
			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			AssertEquals("Import result", wasSuccessful, result);
			var createdInvoices = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true });
			if (wasSuccessful)
			{
				AssertEquals(1, createdInvoices.Length);
				var newFactory = new BusinessObjectFactory();
				var savedInvoice = newFactory.Load<InvoicingBase>(createdInvoices[0].PK);
				AssertType(typeof(TransactionPendingAllocation), savedInvoice);
				AssertEquals("Transaction type", TransactionTypes.InvoicePendingAllocation, savedInvoice.AH_TransactionType);
				Assert("logger.HasErrors", !logger.HasErrors);
				AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
				AssertEquals("Organization", TestObjectCreator.NonCurrentCompany.GC_OH_OrgProxy, savedInvoice.AH_OH);
				AssertEquals("UniversalTransaction.Creditor", ZString.Empty, savedInvoice.AllocationApprovalRequest.PostingDetails.UniversalTransaction.Creditor);
			}
			else
			{
				AssertEquals(0, createdInvoices.Length);
				Assert("logger.HasWarnings", logger.HasWarnings);
				var logs = ((ISimpleLogResult)logger).Logs.ToList();
				var expectedWarning = "Transaction Pending Allocation is not created since both the Issuer and Recipient is in the same Database and Recipient participates in Netting. Please use Intercompany Transaction Approval to approve any intercompany invoice.";
				AssertEquals(expectedWarning, logs.First(x => x.Type == LogType.Warning).Message);
			}
		}

		public void TestImportTransactionLinesWithConsolCosts_ErrorMessages()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			consol.JK_MasterBillNum = "MasterBill1";
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			shipment.JS_HouseBill = "HouseBill1";
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			Factory.Save();

			var importer = new TransactionImporter();
			var invoice = new BusinessObjectFactory().New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.IsFinalCharge = true;
			universalLine.OSAmount = -10;
			universalLine.Job = new EntityReference { Key = "Job1", Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			universalLine.CostSource = new EntityReference { Key = "Consol1", Type = nameof(DataContextType.ForwardingConsol) };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "Job1");
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			universalShipment.WayBillNumber = shipment.JS_HouseBill;
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var universalConsol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalConsol.DataContext = DataContextFactory.New();
			universalConsol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C001");
			universalConsol.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			universalTransaction.ShipmentCollection.Add(universalConsol);

			var universalTransactionXml = universalTransaction.Serialize();
			importer.ImportTransactionLines(universalTransactionXml, invoice, false);
			AssertEquals(1, invoice.Lines.Count);
			var line = invoice.Lines[0];
			AssertHasWarning(line.ConsolIDFromApportionedChargeInfo, "Source XML errors: Can't find consolidation ForwardingConsol with number 'Consol1'. XML should have consolidation ForwardingConsol with DataSource Key and Type elements the same as line CostSource.");
			AssertEquals("Line consol", "", line.ConsolIDFromApportionedCharge);

			universalConsol.DataContext.DataSourceCollection.First().Key = "Consol1";
			invoice.Lines.RemoveAndDeleteAll();
			universalTransactionXml = universalTransaction.Serialize();
			importer.ImportTransactionLines(universalTransactionXml, invoice, false);
			AssertEquals(1, invoice.Lines.Count);
			line = invoice.Lines[0];
			AssertHasWarning(line.ConsolIDFromApportionedChargeInfo, "Source XML errors: Consolidation ForwardingConsol with DataSource Key 'Consol1' is not found.");
			AssertEquals("Line consol", "", line.ConsolIDFromApportionedCharge);

			universalConsol.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
			universalConsol.WayBillNumber = consol.JK_MasterBillNum;
			invoice.Lines.RemoveAndDeleteAll();
			universalTransactionXml = universalTransaction.Serialize();
			importer.ImportTransactionLines(universalTransactionXml, invoice, false);
			AssertEquals(1, invoice.Lines.Count);
			line = invoice.Lines[0];
			AssertNoNotifications(line.ConsolIDFromApportionedChargeInfo);
			AssertEquals("Line consol", "C001", line.ConsolIDFromApportionedCharge);

			invoice.ReleaseAllMutexOnInvoice();
		}

		public void TestImportTransactionLinesWithConsolCosts()
		{
			var consol1 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			consol1.JK_MasterBillNum = "MasterBill1";
			var shipment1 = TestObjectCreator.CreateShipment("S001", consol1);
			shipment1.JS_HouseBill = "HouseBill1";
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			var shipment2 = TestObjectCreator.CreateShipment("S002", consol1);
			shipment2.JS_HouseBill = "HouseBill2";
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			var consol2 = TestObjectCreator.CreateConsol("NZAKL", "USLAX", "C002");
			consol2.JK_MasterBillNum = "MasterBill2";
			consol2.Shipments.Add(shipment1);
			consol2.Shipments.Add(shipment2);
			Factory.Save();

			var importer = new TransactionImporter();
			var invoice = new BusinessObjectFactory().New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.IsFinalCharge = true;
			universalLine.OSAmount = -10;
			universalLine.Job = new EntityReference { Key = "Job1", Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			universalLine.CostSource = new EntityReference { Key = "Consol1", Type = nameof(DataContextType.ForwardingConsol) };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.OSAmount = -20;
			universalLine.Job = new EntityReference { Key = "Job2", Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			universalLine.CostSource = new EntityReference { Key = "Consol2", Type = nameof(DataContextType.ForwardingConsol) };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.OSAmount = -30;
			universalLine.Job = new EntityReference { Key = "Job2", Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			universalLine.CostSource = new EntityReference { Key = "Consol1", Type = nameof(DataContextType.ForwardingConsol) };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.OSAmount = -40;
			universalLine.Job = new EntityReference { Key = "Job1", Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "Job1");
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			universalShipment.WayBillNumber = shipment1.JS_HouseBill;
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			universalTransaction.ShipmentCollection.Add(universalShipment);

			universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "Job2");
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			universalShipment.WayBillNumber = shipment2.JS_HouseBill;
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			universalTransaction.ShipmentCollection.Add(universalShipment);

			universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingConsol, "Consol1");
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
			universalShipment.WayBillNumber = consol1.JK_MasterBillNum;
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			universalTransaction.ShipmentCollection.Add(universalShipment);

			universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingConsol, "Consol2");
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
			universalShipment.WayBillNumber = consol2.JK_MasterBillNum;
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var universalTransactionXml = universalTransaction.Serialize();

			int invoiceLinesListChangedHitCount = 0;
			var listChangedHandler = new ListChangedEventHandler(
				(sender, e) =>
				{ invoiceLinesListChangedHitCount++; }
			);
			((IBindingList)invoice.Lines).ListChanged += listChangedHandler;

			importer.ImportTransactionLines(universalTransactionXml, invoice, false);

			AssertEquals("ListChanged on Invoice.Lines should be called once", 1, invoiceLinesListChangedHitCount);

			AssertEquals(4, invoice.Lines.Count);
			invoice.Lines.Sort("AL_OSExTaxAmount"); // This is unit test in separate debug only assembly
			var line = invoice.Lines[0];
			AssertNoNotifications(line.AL_JHInfo);
			AssertNoNotifications(line.ConsolIDFromApportionedChargeInfo);
			AssertEquals("Line job", shipment1.PK, line.Job.JH_ParentID);
			AssertEquals("Line consol", consol1.PK, line.Consol.PK);
			AssertEquals("GenericCharge", TestObjectCreator.CC1.PK, line.GenericCharge);
			AssertEquals("IsFinalCharge", true, line.AL_IsFinalCharge);
			AssertEquals("OSExTaxAmount", 10m, line.AL_OSExTaxAmount);
			AssertEquals("LocalExTaxAmount", 10m, line.AL_LocalExTaxAmount);

			line = invoice.Lines[1];
			AssertNoNotifications(line.AL_JHInfo);
			AssertNoNotifications(line.ConsolIDFromApportionedChargeInfo);
			AssertEquals("Line job", shipment2.PK, line.Job.JH_ParentID);
			AssertEquals("Line consol", consol2.PK, line.Consol.PK);
			AssertEquals("GenericCharge", TestObjectCreator.CC1.PK, line.GenericCharge);
			AssertEquals("IsFinalCharge", false, line.AL_IsFinalCharge);
			AssertEquals("OSExTaxAmount", 20m, line.AL_OSExTaxAmount);
			AssertEquals("LocalExTaxAmount", 20m, line.AL_LocalExTaxAmount);

			line = invoice.Lines[2];
			AssertNoNotifications(line.AL_JHInfo);
			AssertNoNotifications(line.ConsolIDFromApportionedChargeInfo);
			AssertEquals("Line job", shipment2.PK, line.Job.JH_ParentID);
			AssertEquals("Line consol", consol1.PK, line.Consol.PK);
			AssertEquals("GenericCharge", TestObjectCreator.CC1.PK, line.GenericCharge);
			AssertEquals("IsFinalCharge", false, line.AL_IsFinalCharge);
			AssertEquals("OSExTaxAmount", 30m, line.AL_OSExTaxAmount);
			AssertEquals("LocalExTaxAmount", 30m, line.AL_LocalExTaxAmount);

			line = invoice.Lines[3];
			AssertNoNotifications(line.AL_JHInfo);
			AssertNoNotifications(line.ConsolIDFromApportionedChargeInfo);
			AssertEquals("Line job", shipment1.PK, line.Job.JH_ParentID);
			AssertNull("Line consol", line.Consol);
			AssertEquals("GenericCharge", TestObjectCreator.CC1.PK, line.GenericCharge);
			AssertEquals("IsFinalCharge", false, line.AL_IsFinalCharge);
			AssertEquals("OSExTaxAmount", 40m, line.AL_OSExTaxAmount);
			AssertEquals("LocalExTaxAmount", 40m, line.AL_LocalExTaxAmount);

			AssertEquals("ConsolCosts.Count", 2, invoice.ConsolCosting.ConsolCosts.Count);
			var consolCost = invoice.ConsolCosting.ConsolCosts[0];
			AssertEquals("consolCost ParentID", consol1.PK, consolCost.E6_ParentID);
			AssertEquals("consolCost ApportionmentMethod", AllocationMethod.Manual, consolCost.E6_ApportionmentMethod);
			AssertEquals("consolCost OSCostAmount", 40m, consolCost.E6_OSCostAmount);
			AssertEquals("consolCost UnApportionedAmount", 0m, consolCost.UnApportionedAmount);
			AssertEquals("consolCost ApportionmentCharges Count", 2, consolCost.ApportionmentCharges.Count);
			AssertEquals("consolCost used ApportionmentCharges Count", 2, consolCost.ApportionmentCharges.Count(x => ((ApportionSplitCharge)x).JR_OSCostAmt != 0));
			consolCost = invoice.ConsolCosting.ConsolCosts[1];
			AssertEquals("consolCost ParentID", consol2.PK, consolCost.E6_ParentID);
			AssertEquals("consolCost ApportionmentMethod", AllocationMethod.Manual, consolCost.E6_ApportionmentMethod);
			AssertEquals("consolCost OSCostAmount", 20m, consolCost.E6_OSCostAmount);
			AssertEquals("consolCost UnApportionedAmount", 0m, consolCost.UnApportionedAmount);
			AssertEquals("consolCost ApportionmentCharges Count", 2, consolCost.ApportionmentCharges.Count);
			AssertEquals("consolCost used ApportionmentCharges Count", 1, consolCost.ApportionmentCharges.Count(x => ((ApportionSplitCharge)x).JR_OSCostAmt != 0));

			Assert("ConsolCosting.HasErrors", !invoice.ConsolCosting.HasErrors);

			invoice.ReleaseAllMutexOnInvoice();
		}

		public void TestImportTransactionLinesWithJobs_MutexError()
		{
			var shipment = TestObjectCreator.CreateShipment("S001");
			shipment.JS_HouseBill = "Some HouseBill";
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			Factory.Save();

			var jobWithMutex = new Job.Loader(shipment).TryCreateWithMutex();

			var importer = new TransactionImporter();
			var invoice = new BusinessObjectFactory().New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.OSAmount = -80;
			universalLine.Job = new EntityReference { Key = "Some job", Type = nameof(DataContextType.ForwardingShipment) };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, universalLine.Job.Key.Value);
			universalTransaction.ShipmentCollection.Add(universalShipment);
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			universalShipment.WayBillNumber = shipment.JS_HouseBill;
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };

			var universalTransactionXml = universalTransaction.Serialize();
			importer.ImportTransactionLines(universalTransactionXml, invoice, false);
			AssertEquals(1, invoice.Lines.Count);
			var line = invoice.Lines[0];
			var expectedError = @"Source XML errors: You have created the job S001 on another form, but haven't saved it yet.
Please close or save other forms that use job S001 to continue.";
			Assert("Line job", line.AL_JH.IsEmpty);
			AssertHasWarning(line.AL_JHInfo, expectedError);

			jobWithMutex.Dispose();
		}

		#region TestImportTransactionLinesWithJobs_JobIsNotCreated

		public void TestImportTransactionLinesWithJobs_ConsolJobIsNotCreated()
		{
			AssertImportTransactionLinesWithJobsForGatewayConsol(false);
		}

		public void TestImportTransactionLinesWithJobs_ConsolJobIsCreated()
		{
			AssertImportTransactionLinesWithJobsForGatewayConsol(true);
		}

		void AssertImportTransactionLinesWithJobsForGatewayConsol(bool isGatewayConsol)
		{
			var consol = isGatewayConsol ? TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany) : TestObjectCreator.CreateConsol();
			consol.JK_MasterBillNum = "123-12345678";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			Factory.Save();

			var importer = new TransactionImporter();
			var invoice = new BusinessObjectFactory().New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.OSAmount = -80;
			universalLine.Job = new EntityReference { Key = "Some job", Type = nameof(DataContextType.ForwardingConsol) };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingConsol, universalLine.Job.Key.Value);
			universalTransaction.ShipmentCollection.Add(universalShipment);
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
			universalShipment.WayBillNumber = consol.JK_MasterBillNum;
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };

			var universalTransactionXml = universalTransaction.Serialize();
			importer.ImportTransactionLines(universalTransactionXml, invoice, false);
			AssertEquals(1, invoice.Lines.Count);
			var line = invoice.Lines[0];
			if (isGatewayConsol)
			{
				AssertNotNull("Line job", line.Job);
				AssertNoWarnings(line.AL_JHInfo);
				invoice.ReleaseAllMutexOnInvoice();
			}
			else
			{
				var expectedError = @"Source XML errors: Operational job 'C001' does not support creation of invoicing job.";
				Assert("Line job", line.AL_JH.IsEmpty);
				AssertHasWarning(line.AL_JHInfo, expectedError);
			}
		}

		#endregion

		public void TestImportTransactionLinesWithJobs_NoJobCreatedAndJobBranchDepartment()
		{
			var shipment = TestObjectCreator.CreateShipment("S001");
			shipment.JS_HouseBill = "Some HouseBill";
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			Factory.Save();

			var importer = new TransactionImporter();
			var invoice = new BusinessObjectFactory().New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			var universalLine = CreateTestingJournalLine();
			universalLine.Branch = new Branch { Code = GlbBranch.CurrentBranch.GB_Code };
			universalLine.Job = new EntityReference { Key = "Some job", Type = nameof(DataContextType.ForwardingShipment) };
			universalLine.SupplyType = new UniversalCodeDescriptionPair { Code = "DSB" };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, universalLine.Job.Key.Value);
			universalTransaction.ShipmentCollection.Add(universalShipment);
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			universalShipment.WayBillNumber = shipment.JS_HouseBill;
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };

			universalShipment.JobCosting = new JobCosting(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.JobCosting.Branch = new Branch { Code = TestObjectCreator.NonCurrentBranch.GB_Code };
			universalShipment.JobCosting.Department = new Department { Code = TestObjectCreator.FESDepartment.GE_Code };

			var universalTransactionXml = universalTransaction.Serialize();
			importer.ImportTransactionLines(universalTransactionXml, invoice, false);
			AssertEquals(1, invoice.Lines.Count);
			var line = invoice.Lines[0];
			AssertNoNotifications(line.AL_JHInfo);
			AssertNotNull("Line job", line.InvoicingJob);
			Assert("Line job is just created", !line.InvoicingJob.IsInDatabase);
			AssertEquals("Line job number", "S001", line.InvoicingJob.JH_JobNum);
			AssertEquals("Line parent ID", shipment.PK, line.InvoicingJob.JH_ParentID);
			AssertEquals("Line job branch", TestObjectCreator.NonCurrentBranch.PK, line.InvoicingJob.JH_GB);
			AssertEquals("Line job department", TestObjectCreator.FESDepartment.PK, line.InvoicingJob.JH_GE);
			AssertEquals("Line Branch comes from line xml", GlbBranch.CurrentBranch.PK, line.AL_GB);
			AssertEquals("Line Department comes from line xml", TestObjectCreator.NonCurrentDepartment.PK, line.AL_GE);
			AssertEquals("GenericCharge", TestObjectCreator.CC1.PK, line.GenericCharge);
			AssertEquals("Description", universalLine.Description, line.AL_Desc);
			AssertEquals("IsFinalCharge", universalLine.IsFinalCharge, line.AL_IsFinalCharge);
			AssertEquals("Sequence", universalLine.Sequence, (int)line.AL_Sequence);
			AssertEquals("Currency", universalLine.OSCurrency.Code, line.AL_RX_NKTransactionCurrency);
			AssertEquals("ExchangeRate", 2m, line.AL_ExchangeRate);
			AssertEquals("OSExTaxAmount", 80m, line.AL_OSExTaxAmount);
			AssertEquals("LocalExTaxAmount", 40m, line.AL_LocalExTaxAmount);

			AssertNull("Line job should be created with mutex so another job can't be created for the shipment.", new Job.Loader(shipment).TryCreateWithMutex());

			invoice.Lines.RemoveAndDeleteAll();
			importer.ImportTransactionLines(universalTransactionXml, invoice, true);
			AssertEquals(1, invoice.Lines.Count);
			line = invoice.Lines[0];
			AssertNoNotifications(line.AL_JHInfo);
			AssertNotNull("Line job", line.InvoicingJob);
			Assert("Line job is just created", !line.InvoicingJob.IsInDatabase);
			AssertEquals("Line job number", "S001", line.InvoicingJob.JH_JobNum);
			AssertEquals("Line parent ID", shipment.PK, line.InvoicingJob.JH_ParentID);
			AssertEquals("Line job branch", TestObjectCreator.NonCurrentBranch.PK, line.InvoicingJob.JH_GB);
			AssertEquals("Line job department", TestObjectCreator.FESDepartment.PK, line.InvoicingJob.JH_GE);
			AssertEquals("Line Branch the same as job", TestObjectCreator.NonCurrentBranch.PK, line.AL_GB);
			AssertEquals("Line Department the same as job", TestObjectCreator.FESDepartment.PK, line.AL_GE);
			AssertEquals("GenericCharge", TestObjectCreator.CC1.PK, line.GenericCharge);
			AssertEquals("Description", universalLine.Description, line.AL_Desc);
			AssertEquals("IsFinalCharge", universalLine.IsFinalCharge, line.AL_IsFinalCharge);
			AssertEquals("Sequence", universalLine.Sequence, (int)line.AL_Sequence);
			AssertEquals("Currency", universalLine.OSCurrency.Code, line.AL_RX_NKTransactionCurrency);
			AssertEquals("ExchangeRate", 2m, line.AL_ExchangeRate);
			AssertEquals("OSExTaxAmount", -80m, line.AL_OSExTaxAmount);
			AssertEquals("LocalExTaxAmount", -40m, line.AL_LocalExTaxAmount);

			line.InvoicingJob.Dispose();
		}

		public void TestImportTransactionLinesWithJobs()
		{
			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var importer = new TransactionImporter();
			var invoice = new BusinessObjectFactory().New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			var universalLine = CreateTestingJournalLine();
			universalLine.Job = new EntityReference { Key = "S001", Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.CFSShipment, "Some job");
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var universalTransactionXml = universalTransaction.Serialize();
			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			importer.ImportTransactionLines(universalTransactionXml, invoice, false);
			AssertEquals(1, invoice.Lines.Count);
			var line = invoice.Lines[0];
			AssertHasWarning(line.AL_JHInfo, "Source XML errors: Can't find operational job with number 'S001'. XML should have operational job with DataSource Key element the same as line job number and correct Type.");
			AssertEquals("Line job", ZGuid.Empty, line.AL_JH);
			Action assertOtherLineFields = () =>
			{
				AssertEquals("Branch", TestObjectCreator.NonCurrentBranch.PK, line.AL_GB);
				AssertEquals("Department", TestObjectCreator.NonCurrentDepartment.PK, line.AL_GE);
				AssertEquals("GenericCharge", TestObjectCreator.CC1.PK, line.GenericCharge);
				AssertEquals("Description", universalLine.Description, line.AL_Desc);
				AssertEquals("IsFinalCharge", universalLine.IsFinalCharge, line.AL_IsFinalCharge);
				AssertEquals("Sequence", universalLine.Sequence, (int)line.AL_Sequence);
				AssertEquals("Currency", universalLine.OSCurrency.Code, line.AL_RX_NKTransactionCurrency);
				AssertEquals("ExchangeRate", 2m, line.AL_ExchangeRate);
				AssertEquals("OSExTaxAmount", 80m, line.AL_OSExTaxAmount);
				AssertEquals("LocalExTaxAmount", 40m, line.AL_LocalExTaxAmount);
				if (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value)
				{
					AssertEquals("GovernmentReportingChargeCode", universalLine.GovernmentReportingChargeCode, line.AL_GovtChargeCode);
				}
				else
				{
					AssertEquals("GovernmentReportingChargeCode", string.Empty, line.AL_GovtChargeCode);
				}

				if (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value)
				{
					AssertEquals("SupplyType", universalLine.SupplyType, line.AL_SupplyType);
				}
				else
				{
					AssertEquals("SupplyType", string.Empty, line.AL_SupplyType);
				}
			};
			assertOtherLineFields();

			universalLine.Job.Key = universalShipment.DataContext.DataSourceCollection.First().Key;
			invoice.Lines.RemoveAndDeleteAll();
			universalTransactionXml = universalTransaction.Serialize();
			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			importer.ImportTransactionLines(universalTransactionXml, invoice, false);
			AssertEquals(1, invoice.Lines.Count);
			line = invoice.Lines[0];
			AssertHasWarning(line.AL_JHInfo, "Source XML errors: Operational job CFSShipment with DataSource Key 'Some job' is not found.");
			AssertEquals("Line job", ZGuid.Empty, line.AL_JH);
			assertOtherLineFields();

			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, universalLine.Job.Key.Value);
			invoice.Lines.RemoveAndDeleteAll();
			universalTransactionXml = universalTransaction.Serialize();
			importer.ImportTransactionLines(universalTransactionXml, invoice, false);
			AssertEquals(1, invoice.Lines.Count);
			line = invoice.Lines[0];
			AssertHasWarning(line.AL_JHInfo, "Source XML errors: Operational job ForwardingShipment with DataSource Key 'Some job' is not found.");
			AssertEquals("Line job", ZGuid.Empty, line.AL_JH);
			assertOtherLineFields();

			shipment.JS_HouseBill = "Some HouseBill";
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			var parentConsol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			parentConsol.DataContext = DataContextFactory.New();
			parentConsol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "Some consol");
			parentConsol.DataContext.AddDataSource(DataContextType.ForwardingShipment, universalLine.Job.Key.Value);
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());
			universalTransaction.ShipmentCollection.Add(parentConsol);
			parentConsol.SetSubShipmentCollection(() => new DataObjectList<Shipment> { universalShipment });
			Factory.Save();
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			universalShipment.WayBillNumber = shipment.JS_HouseBill;
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			invoice.Lines.RemoveAndDeleteAll();
			universalTransactionXml = universalTransaction.Serialize();
			importer.ImportTransactionLines(universalTransactionXml, invoice, false);
			AssertEquals(1, invoice.Lines.Count);
			line = invoice.Lines[0];
			AssertNoNotifications(line.AL_JHInfo);
			AssertEquals("Line job", job.PK, line.AL_JH);
			assertOtherLineFields();
		}

		public void TestImportTransactionLinesWithShipmentJob_NoMatchingCriteria()
		{
			AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var newFactory = new BusinessObjectFactory();
			var targetInvoice = newFactory.New<APInvoice>();
			targetInvoice.AH_OH = TestObjectCreator.Creditor1.PK;

			var importer = new TransactionImporter();
			var universalTransaction = CreateUniversalTransaction("S0000010", null);
			var universalTransactionXml = universalTransaction.Serialize();
			AssertNotNull(universalTransactionXml);
			AssertEquals(0, targetInvoice.Lines.Count);

			importer.ImportTransactionLines(universalTransactionXml, targetInvoice, false);
			AssertEquals(1, targetInvoice.Lines.Count);
			var importedLine = targetInvoice.Lines[0];
			AssertNull(importedLine.OriginalJobCharge);
		}

		public void TestImportTransactionLinesWithShipmentJob_HasMatchingCriteria()
		{
			AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var jobNumber = "S0000010";
			var sourceJobCharge = CreateJobCharge(jobNumber);

			var newFactory = new BusinessObjectFactory();
			var targetInvoice = newFactory.New<APInvoice>();
			targetInvoice.AH_OH = TestObjectCreator.Creditor1.PK;

			var importer = new TransactionImporter();
			var universalTransaction = CreateUniversalTransaction(jobNumber, sourceJobCharge.PK.ToString());
			var universalTransactionXml = universalTransaction.Serialize();
			AssertNotNull(universalTransactionXml);
			AssertEquals(0, targetInvoice.Lines.Count);

			importer.ImportTransactionLines(universalTransactionXml, targetInvoice, false);
			AssertEquals(1, targetInvoice.Lines.Count);
			var importedLine = targetInvoice.Lines[0];
			AssertEquals(sourceJobCharge.PK, importedLine.OriginalJobCharge.PK);
		}

		public void TestImportTransactionLinesWithShipmentJob_AssertMatchingDetails()
		{
			AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var jobNumber = "S0000010";
			var shipment = TestObjectCreator.CreateShipment(jobNumber);
			shipment.JS_HouseBill = jobNumber;
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var sourceJobCharge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 20m, 0m);
			sourceJobCharge1.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			var sourceJobCharge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 40m, 0m);
			sourceJobCharge2.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			Factory.Save();
			var linePKBelongToSourceJobCharge2 = sourceJobCharge2.JR_AL_APLine;
			var query = new ZQuery(JobChargeSchema.JR_JH, job.PK);
			var originalCharges = Factory.Load<Charge>(query);
			AssertEquals(2, originalCharges.Length);

			var newFactory = new BusinessObjectFactory();
			var targetInvoice = newFactory.New<APInvoice>();
			targetInvoice.AH_OH = TestObjectCreator.Creditor1.PK;

			var importer = new TransactionImporter();
			var universalTransaction = CreateUniversalTransaction(jobNumber, sourceJobCharge2.PK.ToString());
			var universalTransactionXml = universalTransaction.Serialize();
			AssertNotNull(universalTransactionXml);
			AssertEquals(0, targetInvoice.Lines.Count);

			importer.ImportTransactionLines(universalTransactionXml, targetInvoice, false);
			AssertEquals(1, targetInvoice.Lines.Count);
			var importedLine = targetInvoice.Lines[0];
			AssertEquals(TransactionLineTypes.Cost, importedLine.AL_LineType);

			targetInvoice.AH_TransactionNum = "00000020";
			targetInvoice.SubmittedFromInvoicingForm = true;

			AssertEquals(sourceJobCharge2.PK, importedLine.OriginalJobCharge.PK);

			newFactory.Save();

			var charges = newFactory.Load<Charge>(query);
			AssertEquals(3, charges.Length);
			var charge1 = charges.Single(x => x.PK == sourceJobCharge1.PK);
			var charge2 = charges.Single(x => x.PK == sourceJobCharge2.PK);
			var charge3 = charges.Single(x => x.PK != sourceJobCharge1.PK && x.PK != sourceJobCharge2.PK);

			AssertEquals(20m, charge1.JR_OSCostAmt);
			AssertEquals(20m, charge1.LocalCostAmount);
			AssertEquals(ZDateTime.Empty, charge1.APLine.AL_ReverseDate);
			AssertEquals(TransactionLineTypes.Accrual, charge1.APLine.AL_LineType);

			AssertEquals(10m, charge2.JR_OSCostAmt);
			AssertEquals(10m, charge2.LocalCostAmount);
			AssertNotEquals(ZDateTime.Empty, charge2.APLine.AL_ReverseDate);
			AssertEquals(TransactionLineTypes.Cost, charge2.APLine.AL_LineType);
			var lineBelongToSourceJobCharge2 = Factory.Load<AccTransactionLines>(linePKBelongToSourceJobCharge2);
			AssertNotEquals("Original Line belong to Charge2 should be reversed.", ZDateTime.Empty, lineBelongToSourceJobCharge2.AL_ReverseDate);
			AssertEquals(40m, lineBelongToSourceJobCharge2.AL_LineAmount);
			AssertEquals(TransactionLineTypes.Accrual, lineBelongToSourceJobCharge2.AL_LineType);

			AssertEquals(30m, charge3.JR_OSCostAmt);
			AssertEquals(30m, charge3.LocalCostAmount);
			AssertEquals(ZDateTime.Empty, charge3.APLine.AL_ReverseDate);
			AssertEquals(TransactionLineTypes.Accrual, charge3.APLine.AL_LineType);
		}

		Charge CreateJobCharge(string jobNumber)
		{
			var shipment = TestObjectCreator.CreateShipment(jobNumber);
			shipment.JS_HouseBill = jobNumber;
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 20m, 0m);
			charge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			Factory.Save();

			return charge;
		}

		TransactionInfo CreateUniversalTransaction(string jobNumber, string primaryKey)
		{
			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());
			universalTransaction.IsCancelled = false;
			universalTransaction.TransactionType = TransactionType.INV;
			universalTransaction.Ledger = LedgerTypes.AccountsPayable;

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, jobNumber);
			universalShipment.WayBillNumber = jobNumber;
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var universalLine = CreateTestingJournalLine(jobNumber, primaryKey);
			universalTransaction.PostingJournalCollection.Add(universalLine);

			return universalTransaction;
		}

		PostingJournal CreateTestingJournalLine(string jobNumber, string primaryKey)
		{
			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.Branch = new Branch { Code = Env.CurrentBranch.Code };
			universalLine.Department = new Department { Code = Env.CurrentDepartment.Code };
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.Description = "Some line text";
			universalLine.IsFinalCharge = false;
			universalLine.Sequence = 1;
			universalLine.OSCurrency = new Currency { Code = "AUD" };
			universalLine.LocalCurrency = new Currency { Code = "AUD" };
			universalLine.OSAmount = -10m;
			universalLine.LocalAmount = -10m;
			universalLine.Job = new EntityReference { Key = jobNumber, Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			if (!string.IsNullOrWhiteSpace(primaryKey))
			{
				var matchingCriterion = new MatchingCriteria()
				{
					FieldName = "PrimaryKey",
					Value = primaryKey
				};
				var matchingCriteriaCollection = new List<MatchingCriteria>()
				{
					matchingCriterion
				};
				universalLine.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Instruction = InstructionType.UpdateAndInsertIfNotFound
				};
				universalLine.ImportMetaData.SetMatchingCriteriaCollection(() => matchingCriteriaCollection);
			}

			return universalLine;
		}

		public void TestImportAPTransactionWithInvalidBranch()
		{
			AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var importer = new TransactionImporter();
			var message = new BusinessObjectFactory().New<EDIMessage>();
			var universalFactory = new UniversalObjectFactory();
			var serviceLogger = new ServiceTaskLogForTesting();
			var logger = new XmlSessionTracker(serviceLogger);
			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.Ledger = LedgerTypes.AccountsPayable;
			universalTransaction.Branch = new Branch { Code = "XXX" };

			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			var savedInvoice = new BusinessObjectFactory().LoadTop1<InvoicingBase>(new ZQuery());
			AssertNotNull(savedInvoice);
			Assert("logger.HasErrors", !logger.HasErrors);
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
			AssertEquals("invoice Branch", GlbBranch.CurrentBranch.PK, savedInvoice.AH_GB);
		}

		public void TestImportARTransactionWithInvalidBranch()
		{
			AssertImportARTransaction(TransactionType.INV, typeof(ARInvoice), withInvalidBranch: true);
		}

		public void TestImportAPTransactionWithInvalidDepartment()
		{
			AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var importer = new TransactionImporter();
			var message = new BusinessObjectFactory().New<EDIMessage>();
			var universalFactory = new UniversalObjectFactory();
			var serviceLogger = new ServiceTaskLogForTesting();
			var logger = new XmlSessionTracker(serviceLogger);
			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.Ledger = LedgerTypes.AccountsPayable;
			universalTransaction.Department = new Department { Code = "XXX" };

			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			var savedInvoice = new BusinessObjectFactory().LoadTop1<InvoicingBase>(new ZQuery());
			AssertNotNull(savedInvoice);
			Assert("logger.HasErrors", !logger.HasErrors);
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
			AssertEquals("invoice Department", GlbDepartment.CurrentDepartment.PK, savedInvoice.AH_GE);
		}

		public void TestImportARTransactionWithInvalidDepartment()
		{
			AssertImportARTransaction(TransactionType.INV, typeof(ARInvoice), withInvalidDepartment: true);
		}

		public void TestImportARInvoiceWithDuplicateCheckNumberOrPaymentRef()
		{
			AssertImportARTransactionWithDuplicateCheckNumberOrPaymentRef(TransactionType.INV);
		}

		public void TestImportARCreditNoteWithDuplicateCheckNumberOrPaymentRef()
		{
			AssertImportARTransactionWithDuplicateCheckNumberOrPaymentRef(TransactionType.CRD);
		}

		void AssertImportARTransactionWithDuplicateCheckNumberOrPaymentRef(TransactionType transactipnType)
		{
			var address = TestObjectCreator.CreateAddress(TestObjectCreator.Creditor1, "123 Road Ave");
			var contact = TestObjectCreator.CreateContact(TestObjectCreator.Creditor1, "Mark", "ww@www.com");

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR0001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			arInvoice.AH_ChequeOrReference = "1234567890";
			Factory.Save();

			var importer = new TransactionImporter();
			var message = new BusinessObjectFactory().New<EDIMessage>();
			var universalFactory = new UniversalObjectFactory();
			var serviceLogger = new ServiceTaskLogForTesting();
			var logger = new XmlSessionTracker(serviceLogger);
			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.None),
				AddressShortCode = address.OA_Code,
				Address1 = address.OA_Address1,
				OrganizationCode = TestObjectCreator.Creditor1.OH_Code,
				Contact = contact.OC_ContactName,
				Email = contact.OC_Email
			};
			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), Country = Country.New(RefCountry.LoadFromCountryCode(Factory, "NZ")) },

				OrganizationAddress = orgAddress,
				TransactionType = transactipnType,
				Ledger = LedgerTypes.AccountsReceivable,
				TransactionDate = ZDateTime.Today.AddDays(-3),
				DocumentReceivedDate = ZDateTime.Today.AddDays(1),
				DueDate = ZDateTime.Today.AddDays(2),
				PostDate = ZDateTime.Today.AddDays(-1),
				Number = "1234",
				CheckNumberOrPaymentRef = "1234567890",
				NumberOfSupportingDocuments = 5,
				Description = "Some text",
				Branch = new Branch { Code = TestObjectCreator.NonCurrentBranch.GB_Code },
				Department = new Department { Code = TestObjectCreator.NonCurrentDepartment.GE_Code },

				OSCurrency = new Currency { Code = "GBP" },
				LocalCurrency = new Currency { Code = "AUD" },
				OSExGSTVATAmount = 120,
				LocalExVATAmount = 60,
				OSGSTVATAmount = 10,
				LocalVATAmount = 5
			};

			universalTransaction.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ORP } } });
			Assert("Precondition: HasRecipientRole", universalTransaction.HasRecipientRole(RecipientRoleType.ORP));

			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", !result);
			Assert("logger.HasErrors", logger.HasErrors);
			AssertEquals("Error - There is an existing AR Invoice or Credit Note with the same cheque number or payment reference.", logger.ToString());
		}

		public void TestImportTransaction()
		{
			var address = TestObjectCreator.CreateAddress(TestObjectCreator.Creditor1, "123 Road Ave");
			var contact = TestObjectCreator.CreateContact(TestObjectCreator.Creditor1, "Mark", "ww@www.com");

			Factory.Save();

			var importer = new TransactionImporter();
			var message = new BusinessObjectFactory().New<EDIMessage>();
			var universalFactory = new UniversalObjectFactory();
			var serviceLogger = new ServiceTaskLogForTesting();
			var logger = new XmlSessionTracker(serviceLogger);
			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.None),
				AddressShortCode = address.OA_Code,
				Address1 = address.OA_Address1,
				OrganizationCode = TestObjectCreator.Creditor1.OH_Code,
				Contact = contact.OC_ContactName,
				Email = contact.OC_Email
			};
			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), Country = Country.New(RefCountry.LoadFromCountryCode(Factory, "NZ")) },

				OrganizationAddress = orgAddress,

				TransactionDate = ZDateTime.Today.AddDays(-3),
				DocumentReceivedDate = ZDateTime.Today.AddDays(1),
				DueDate = ZDateTime.Today.AddDays(2),
				PostDate = ZDateTime.Today.AddDays(-1),
				Number = "1234",
				CheckNumberOrPaymentRef = "Cheque#1",
				NumberOfSupportingDocuments = 5,
				Description = "Some text",
				Branch = new Branch { Code = TestObjectCreator.NonCurrentBranch.GB_Code },
				Department = new Department { Code = TestObjectCreator.NonCurrentDepartment.GE_Code },
				GovernmentAllocatedID = Guid.NewGuid().ToString(),
				ComplianceSubType = "PIN",

				OSCurrency = new Currency { Code = "GBP" },
				LocalCurrency = new Currency { Code = "AUD" },
				OSExGSTVATAmount = 120,
				LocalExVATAmount = 60,
				OSGSTVATAmount = 10,
				LocalVATAmount = 5
			};

			AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", !result);
			var createdInvoices = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(0, createdInvoices.Length);
			Assert("logger.HasErrors", logger.HasErrors);
			AssertEquals("Error - Transaction Ledger is not supported", logger.ToString());

			AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			universalTransaction.Ledger = LedgerTypes.AccountsReceivable;
			universalFactory = new UniversalObjectFactory();
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", !result);
			createdInvoices = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(0, createdInvoices.Length);
			Assert("logger.HasErrors", logger.HasErrors);
			AssertEquals("Error - Transaction Ledger is not supported", logger.ToString());

			universalFactory = new UniversalObjectFactory();
			serviceLogger = new ServiceTaskLogForTesting();
			logger = new XmlSessionTracker(serviceLogger);
			universalTransaction.Ledger = LedgerTypes.AccountsPayable;
			result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			createdInvoices = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			var newFactory = new BusinessObjectFactory();
			var savedInvoice = newFactory.Load<InvoicingBase>(createdInvoices[0].PK);
			AssertType(typeof(TransactionPendingAllocation), savedInvoice);
			AssertEquals("Organization", TestObjectCreator.Creditor1.PK, savedInvoice.AH_OH);
			AssertEquals("Address", address.PK, savedInvoice.AH_OA_InvoiceAddressOverride);
			AssertEquals("Contact", contact.PK, savedInvoice.AH_OC_InvoiceContactOverride);
			AssertEquals("TransactionDate", universalTransaction.TransactionDate, savedInvoice.AH_InvoiceDate);
			AssertEquals("DocumentReceivedDate", universalTransaction.DocumentReceivedDate, savedInvoice.AH_DocumentReceivedDate);
			AssertEquals("PostDate", universalTransaction.PostDate, savedInvoice.AH_PostDate);
			AssertEquals("TransactionNumber", universalTransaction.Number, savedInvoice.AH_TransactionNum);
			AssertEquals("ChequeOrReference", universalTransaction.CheckNumberOrPaymentRef, savedInvoice.AH_ChequeOrReference);
			AssertEquals("NumberOfSupportingDocuments", universalTransaction.NumberOfSupportingDocuments, (int)savedInvoice.AH_NumberOfSupportingDocuments);
			AssertEquals("Currency", universalTransaction.OSCurrency.Code, savedInvoice.AH_RX_NKTransactionCurrency);
			AssertEquals("ExchangeRate", 2m, savedInvoice.AH_ExchangeRate);
			AssertEquals("OSExTaxAmount", -130m, savedInvoice.AH_OSExTaxAmount);
			AssertEquals("OSTaxAmount", 0m, savedInvoice.AH_OSTaxAmount);
			AssertEquals("LocalExTaxAmount", -65m, savedInvoice.AH_LocalExTaxAmount);
			AssertEquals("LocalTaxAmount", 0m, savedInvoice.AH_LocalTaxAmount);
			AssertEquals("Transaction type", TransactionTypes.CreditNotePendingAllocation, savedInvoice.AH_TransactionType);
			AssertEquals("Branch", TestObjectCreator.NonCurrentBranch.PK, savedInvoice.AH_GB);
			AssertEquals("Department", TestObjectCreator.NonCurrentDepartment.PK, savedInvoice.AH_GE);
			AssertEquals("GovernmentAllocatedID", universalTransaction.GovernmentAllocatedID, savedInvoice.AH_GovernmentAllocatedID);
			AssertEquals("ComplianceSubType", universalTransaction.ComplianceSubType, savedInvoice.AH_ComplianceSubType);
			Assert("logger.HasErrors", !logger.HasErrors);
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);

			universalTransaction.BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), Country = Country.New(GlbCompany.CurrentCompany.Country) };
			universalFactory = new UniversalObjectFactory();
			serviceLogger = new ServiceTaskLogForTesting();
			logger = new XmlSessionTracker(serviceLogger);
			universalTransaction.Number = string.Format(CultureInfo.InvariantCulture, "{0}_2", universalTransaction.Number);
			result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			createdInvoices = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			newFactory = new BusinessObjectFactory();
			savedInvoice = newFactory.Load<InvoicingBase>(createdInvoices[0].PK);
			AssertType(typeof(TransactionPendingAllocation), savedInvoice);
			AssertEquals("Organization", TestObjectCreator.Creditor1.PK, savedInvoice.AH_OH);
			AssertEquals("Address", address.PK, savedInvoice.AH_OA_InvoiceAddressOverride);
			AssertEquals("Contact", contact.PK, savedInvoice.AH_OC_InvoiceContactOverride);
			AssertEquals("TransactionDate", universalTransaction.TransactionDate, savedInvoice.AH_InvoiceDate);
			AssertEquals("DocumentReceivedDate", universalTransaction.DocumentReceivedDate, savedInvoice.AH_DocumentReceivedDate);
			AssertEquals("PostDate", universalTransaction.PostDate, savedInvoice.AH_PostDate);
			AssertEquals("TransactionNumber", universalTransaction.Number, savedInvoice.AH_TransactionNum);
			AssertEquals("NumberOfSupportingDocuments", universalTransaction.NumberOfSupportingDocuments, (int)savedInvoice.AH_NumberOfSupportingDocuments);
			AssertEquals("Currency", universalTransaction.OSCurrency.Code, savedInvoice.AH_RX_NKTransactionCurrency);
			AssertEquals("ExchangeRate", 2m, savedInvoice.AH_ExchangeRate);
			AssertEquals("OSExTaxAmount", -120m, savedInvoice.AH_OSExTaxAmount);
			AssertEquals("OSTaxAmount", -10m, savedInvoice.AH_OSTaxAmount);
			AssertEquals("LocalExTaxAmount", -60m, savedInvoice.AH_LocalExTaxAmount);
			AssertEquals("LocalTaxAmount", -5m, savedInvoice.AH_LocalTaxAmount);
			AssertEquals("Transaction type", TransactionTypes.CreditNotePendingAllocation, savedInvoice.AH_TransactionType);
			AssertEquals("Branch", TestObjectCreator.NonCurrentBranch.PK, savedInvoice.AH_GB);
			AssertEquals("Department", TestObjectCreator.NonCurrentDepartment.PK, savedInvoice.AH_GE);
			Assert("logger.HasErrors", !logger.HasErrors);
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);

			TestObjectCreator.Creditor1.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;
			Factory.Save();
			universalFactory = new UniversalObjectFactory();
			serviceLogger = new ServiceTaskLogForTesting();
			logger = new XmlSessionTracker(serviceLogger);
			universalTransaction.Number = string.Format(CultureInfo.InvariantCulture, "{0}_3", universalTransaction.Number);
			result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			createdInvoices = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			newFactory = new BusinessObjectFactory();
			savedInvoice = newFactory.Load<InvoicingBase>(createdInvoices[0].PK);
			AssertType(typeof(TransactionPendingAllocation), savedInvoice);
			AssertEquals("Organization", TestObjectCreator.Creditor1.PK, savedInvoice.AH_OH);
			AssertEquals("Address", address.PK, savedInvoice.AH_OA_InvoiceAddressOverride);
			AssertEquals("Contact", contact.PK, savedInvoice.AH_OC_InvoiceContactOverride);
			AssertEquals("TransactionDate", universalTransaction.TransactionDate, savedInvoice.AH_InvoiceDate);
			AssertEquals("DocumentReceivedDate", universalTransaction.DocumentReceivedDate, savedInvoice.AH_DocumentReceivedDate);
			AssertEquals("PostDate", universalTransaction.PostDate, savedInvoice.AH_PostDate);
			AssertEquals("TransactionNumber", universalTransaction.Number, savedInvoice.AH_TransactionNum);
			AssertEquals("NumberOfSupportingDocuments", universalTransaction.NumberOfSupportingDocuments, (int)savedInvoice.AH_NumberOfSupportingDocuments);
			AssertEquals("Currency", universalTransaction.OSCurrency.Code, savedInvoice.AH_RX_NKTransactionCurrency);
			AssertEquals("ExchangeRate", 2m, savedInvoice.AH_ExchangeRate);
			AssertEquals("OSExTaxAmount", -130m, savedInvoice.AH_OSExTaxAmount);
			AssertEquals("OSTaxAmount", 0m, savedInvoice.AH_OSTaxAmount);
			AssertEquals("LocalExTaxAmount", -65m, savedInvoice.AH_LocalExTaxAmount);
			AssertEquals("LocalTaxAmount", 0m, savedInvoice.AH_LocalTaxAmount);
			AssertEquals("Transaction type", TransactionTypes.CreditNotePendingAllocation, savedInvoice.AH_TransactionType);
			AssertEquals("Branch", TestObjectCreator.NonCurrentBranch.PK, savedInvoice.AH_GB);
			AssertEquals("Department", TestObjectCreator.NonCurrentDepartment.PK, savedInvoice.AH_GE);
			Assert("logger.HasErrors", !logger.HasErrors);
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
		}

		public void TestImportARINVTransaction()
		{
			AssertImportARTransaction(TransactionType.INV, typeof(ARInvoice));
		}

		public void TestImportARINVTransactionForClient_EDI() => AssertImportARINVTransactionWithClientHookCore(Clients.EDI);

		public void TestImportARINVTransactionForClient_WLG() => AssertImportARINVTransactionWithClientHookCore(Clients.WLG);

		public void TestImportARINVTransactionForClient_JAS() => AssertImportARINVTransactionWithClientHookCore(Clients.JAS);

		public void TestIntroductionOfNewClientSpecificARInvoiceChildTypeIsDetected()
		{
			var allClientTypes = Enum.GetValues(typeof(Clients)).Cast<Clients>();
			foreach (var clientType in allClientTypes)
			{
				using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(clientType))
				{
					var clientHook = ClientHookLoader.Instance.ClientHook;
					var transactionType = typeof(ARInvoice);
					if (clientType == Clients.EDI || clientType == Clients.WLG || clientType == Clients.JAS)
					{
						AssertNotNull(clientHook);
						var overriddenType = clientHook.ClientTypeDeciders[transactionType].GetTypeForBinding();
						AssertNotNull(overriddenType);
					}
					else
					{
						var overriddenType = clientHook?.ClientTypeDeciders?[transactionType]?.GetTypeForBinding();
						AssertNull(FormattableString.Invariant($"Looks like {overriddenType} is a new client specific AR invoice type written for {clientType}. Please add a new unit test to verify that Import process works correctly for this type."), overriddenType);
					}
				}
			}
		}

		void AssertImportARINVTransactionWithClientHookCore(Clients clientType)
		{
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(clientType))
			{
				var clientHook = ClientHookLoader.Instance.ClientHook;
				AssertNotNull(clientHook);

				new ClientDbSchemaCreationForTesting().RunClientDbCreateScripts(false);
				var overriddenType = clientHook.ClientTypeDeciders[typeof(ARInvoice)].GetTypeForBinding();
				AssertNotNull(overriddenType);
				AssertImportARTransaction(TransactionType.INV, overriddenType);
			}
		}

		public void TestImportARCRDTransaction()
		{
			AssertImportARTransaction(TransactionType.CRD, typeof(ARCreditNote));
		}

		public void TestImportARCRDTransaction_ReferenceDates()
		{
			using (InvoiceBaseValidationTest.InitaliseComplianceFactory(areOriginalTransactionReferenceFieldsMandatory: true))
			{
				AssertImportARTransaction(TransactionType.CRD, typeof(ARCreditNote));
			}
		}

		public void TestImportARADJTransaction()
		{
			AssertImportARTransaction(TransactionType.ADJ, typeof(ARAdjustmentNote));
		}

		public void TestImportARTransactionWithEmptyJob()
		{
			AssertImportARTransaction(TransactionType.INV, typeof(ARInvoice), true);
		}

		public void TestImportARTransactionWithSurcharge()
		{
			var surchargeLineCreator = new Mock<ISurchargeLineCreator>(MockBehavior.Strict);

			TestObjectCreator.Debtor1.CompanyData.SetARTaxApplicable(true);
			Factory.Save();

			var orgAddress = TestObjectCreator.CreateAddress(TestObjectCreator.Debtor1, "123 Road Ave");
			var orgContact = TestObjectCreator.CreateContact(TestObjectCreator.Debtor1, "Mark", "ww@www.com");

			var organizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.None),
				AddressShortCode = orgAddress.OA_Code,
				Address1 = orgAddress.OA_Address1,
				OrganizationCode = TestObjectCreator.Debtor1.OH_Code,
				Contact = orgContact.OC_ContactName,
				Email = orgContact.OC_Email
			};

			var importer = new TransactionImporter();
			var message = new BusinessObjectFactory().New<EDIMessage>();
			var universalFactory = new UniversalObjectFactory();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());

			var universalTransaction = CreateTransactionWithLine(organizationAddress);
			universalTransaction.Ledger = LedgerTypes.AccountsReceivable;
			universalTransaction.TransactionType = TransactionType.INV;

			universalTransaction.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ORP } } });
			Assert("Precondition: HasRecipientRole", universalTransaction.HasRecipientRole(RecipientRoleType.ORP));

			ObjectFactory.Substitute(surchargeLineCreator.Object);
			surchargeLineCreator.Setup(x => x.AddSurchargeLine(It.IsAny<InvoicingBase>()));

			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			surchargeLineCreator.Verify(x => x.AddSurchargeLine(It.IsAny<InvoicingBase>()), Times.Once);
		}

		public void AssertImportARTransaction(TransactionType transactionType, Type type, bool withEmptyJob = false, bool withInvalidBranch = false, bool withInvalidDepartment = false)
		{
			TestObjectCreator.Debtor1.CompanyData.SetARTaxApplicable(true);
			Factory.Save();

			var orgAddress = TestObjectCreator.CreateAddress(TestObjectCreator.Debtor1, "123 Road Ave");
			var orgContact = TestObjectCreator.CreateContact(TestObjectCreator.Debtor1, "Mark", "ww@www.com");

			var organizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.None),
				AddressShortCode = orgAddress.OA_Code,
				Address1 = orgAddress.OA_Address1,
				OrganizationCode = TestObjectCreator.Debtor1.OH_Code,
				Contact = orgContact.OC_ContactName,
				Email = orgContact.OC_Email
			};

			var importer = new TransactionImporter();
			var message = new BusinessObjectFactory().New<EDIMessage>();
			var universalFactory = new UniversalObjectFactory();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());

			var universalTransaction = CreateTransactionWithLine(organizationAddress, withInvalidBranch, withInvalidDepartment);
			universalTransaction.Ledger = LedgerTypes.AccountsReceivable;
			universalTransaction.TransactionType = transactionType;
			universalTransaction.ComplianceSubType = "EIN";

			if (withEmptyJob)
			{
				var job = new EntityReference { Type = AccountingDataTransferConstants.DataContextTypeString.Job };
				universalTransaction.Job = job;
			}

			universalTransaction.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ORP } } });
			Assert("Precondition: HasRecipientRole", universalTransaction.HasRecipientRole(RecipientRoleType.ORP));

			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);

			Assert("Logger without error", !logger.HasErrors);
			Assert("transaction imported with success", result);

			var createdInvoices = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);

			var newFactory = new BusinessObjectFactory();
			var savedInvoice = newFactory.Load<InvoicingBase>(createdInvoices[0].PK);
			if (universalTransaction.Ledger.Value == LedgerTypes.AccountsPayable)
			{
				AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
			}
			else
			{
				AssertEquals(0, savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Length);
			}

			AssertImportedTransaction(createdInvoices[0], type, LedgerTypes.AccountsReceivable, transactionType.ToString(),
				TestObjectCreator.Debtor1.PK, orgAddress.PK, orgContact.PK, universalTransaction.TransactionDate.Value, universalTransaction.PostDate.Value, "00001000", complianceSubType: "",
				universalTransaction.CheckNumberOrPaymentRef, universalTransaction.NumberOfSupportingDocuments.Value, universalTransaction.OSCurrency.Code, 2m, 130m, 13m, 65m, 6.5m, withInvalidBranch, withInvalidDepartment);

			var lines = createdInvoices[0].Lines;
			AssertEquals("One transaction line", 1, lines.Count);
			AssertImportedTransactionLine(lines[0], "GBP", 2m, 130m, 13m, 65m, 6.5m, ZDate.Today.AddDays(-2), withInvalidBranch, withInvalidDepartment);
		}

		public void TestImportARTransactionWithJob()
		{
			TestObjectCreator.Debtor1.CompanyData.SetARTaxApplicable(true);
			Factory.Save();

			var orgAddress = TestObjectCreator.CreateAddress(TestObjectCreator.Debtor1, "123 Road Ave");
			var orgContact = TestObjectCreator.CreateContact(TestObjectCreator.Debtor1, "Mark", "ww@www.com");

			var organizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.None),
				AddressShortCode = orgAddress.OA_Code,
				Address1 = orgAddress.OA_Address1,
				OrganizationCode = TestObjectCreator.Debtor1.OH_Code,
				Contact = orgContact.OC_ContactName,
				Email = orgContact.OC_Email
			};

			var importer = new TransactionImporter();
			var message = new BusinessObjectFactory().New<EDIMessage>();
			var universalFactory = new UniversalObjectFactory();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());

			var universalTransaction = CreateTransactionWithLine(organizationAddress);
			universalTransaction.Ledger = LedgerTypes.AccountsReceivable;
			universalTransaction.TransactionType = TransactionType.INV;

			universalTransaction.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ORP } } });
			Assert("Precondition: HasRecipientRole", universalTransaction.HasRecipientRole(RecipientRoleType.ORP));

			var shipment = TestObjectCreator.CreateShipment("S001");
			TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			var job = new EntityReference { Key = "S001", Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			universalTransaction.Job = job;
			universalTransaction.PostingJournalCollection[0].Job = job;

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.CFSShipment, "Some job");
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);

			Assert("transaction not imported", !result);
			Assert("Logger with error", logger.HasErrors);
			AssertEquals("Error - We cannot import AR transactions which have a Job.", logger.ToString());
		}

		public void TestImportARTransactionWithWrongTransactionType()
		{
			TestObjectCreator.Debtor1.CompanyData.SetARTaxApplicable(true);
			Factory.Save();

			var orgAddress = TestObjectCreator.CreateAddress(TestObjectCreator.Debtor1, "123 Road Ave");
			var orgContact = TestObjectCreator.CreateContact(TestObjectCreator.Debtor1, "Mark", "ww@www.com");

			var organizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.None),
				AddressShortCode = orgAddress.OA_Code,
				Address1 = orgAddress.OA_Address1,
				OrganizationCode = TestObjectCreator.Debtor1.OH_Code,
				Contact = orgContact.OC_ContactName,
				Email = orgContact.OC_Email
			};

			var importer = new TransactionImporter();
			var message = new BusinessObjectFactory().New<EDIMessage>();
			var universalFactory = new UniversalObjectFactory();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());

			var universalTransaction = CreateTransactionWithLine(organizationAddress);
			universalTransaction.Ledger = LedgerTypes.AccountsReceivable;
			universalTransaction.TransactionType = TransactionType.REC;

			universalTransaction.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ORP } } });
			Assert("Precondition: HasRecipientRole", universalTransaction.HasRecipientRole(RecipientRoleType.ORP));

			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert(!result);
			Assert("logger.HasErrors", logger.HasErrors);
			AssertEquals("Error - You can import AR transaction only with type Invoice, Credit Note or Adjustment Note.", logger.ToString());
		}

		void AssertImportedTransaction(InvoicingBase transaction, Type type, string ledger, string transactionType,
			ZGuid orgPK, ZGuid addressPK, ZGuid contactPK, ZDateTime transactionDate, ZDateTime postDate,
			string transactionNum, string complianceSubType, string checkNumberOrPaymentRef, int numberOfSupportingDocuments, string currencyCode,
			ZDecimal exRate, ZDecimal oSExTaxAmount, ZDecimal oSTaxAmount, ZDecimal localExTaxAmount, ZDecimal localTaxAmount, bool withInvalidBranch = false, bool withInvalidDepartment = false)
		{
			AssertType(type, transaction);
			AssertEquals("Organization", orgPK, transaction.AH_OH);
			AssertEquals("Address", addressPK, transaction.AH_OA_InvoiceAddressOverride);
			AssertEquals("Contact", contactPK, transaction.AH_OC_InvoiceContactOverride);
			AssertEquals("TransactionDate", transactionDate, transaction.AH_InvoiceDate);
			AssertEquals("PostDate", postDate, transaction.AH_PostDate);
			AssertEquals("TransactionNumber", transactionNum, transaction.AH_TransactionNum);
			AssertEquals("ComplianceSubType", complianceSubType, transaction.AH_ComplianceSubType);
			AssertEquals("ChequeOrReference", checkNumberOrPaymentRef, transaction.AH_ChequeOrReference);
			AssertEquals("NumberOfSupportingDocuments", numberOfSupportingDocuments, (int)transaction.AH_NumberOfSupportingDocuments);
			AssertEquals("Currency", currencyCode, transaction.AH_RX_NKTransactionCurrency);
			AssertEquals("ExchangeRate", exRate, transaction.AH_ExchangeRate);
			AssertEquals("OSExTaxAmount", oSExTaxAmount, transaction.AH_OSExTaxAmount);
			AssertEquals("OSTaxAmount", oSTaxAmount, transaction.AH_OSTaxAmount);
			AssertEquals("LocalExTaxAmount", localExTaxAmount, transaction.AH_LocalExTaxAmount);
			AssertEquals("LocalTaxAmount", localTaxAmount, transaction.AH_LocalTaxAmount);
			AssertEquals("Transaction type", transactionType, transaction.AH_TransactionType);
			AssertEquals("Branch", withInvalidBranch ? GlbBranch.CurrentBranch.PK : TestObjectCreator.NonCurrentBranch.PK, transaction.AH_GB);
			AssertEquals("Department", withInvalidDepartment ? GlbDepartment.CurrentDepartment.PK : TestObjectCreator.NonCurrentDepartment.PK, transaction.AH_GE);
			var expectedDate = transaction.AreOriginalTransactionReferenceFieldsMandatory ? transaction.AH_InvoiceDate : ZDate.Empty;
			AssertEquals("OriginalReferenceStartDate", expectedDate, transaction.AH_OriginalReferenceStartDate);
			AssertEquals("OriginalReferenceEndDate", expectedDate, transaction.AH_OriginalReferenceEndDate);
		}

		void AssertImportedTransactionLine(InvoicingLineBase line, string currency, ZDecimal exRate, ZDecimal osExTaxAmount, ZDecimal osTaxAmount, ZDecimal localExTaxAmount, ZDecimal localTaxAmount, ZDate taxDate, bool withInvalidBranch = false, bool withInvalidDepartment = false)
		{
			AssertEquals(exRate, line.AL_ExchangeRate);
			AssertEquals(currency, line.AL_RX_NKTransactionCurrency);
			AssertEquals(osExTaxAmount, line.AL_OSExTaxAmount);
			AssertEquals(osTaxAmount, line.AL_OSTaxAmount);
			AssertEquals(localExTaxAmount, line.AL_LocalExTaxAmount);
			AssertEquals(localTaxAmount, line.AL_LocalTaxAmount);
			AssertEquals(withInvalidBranch ? GlbBranch.CurrentBranch.PK : TestObjectCreator.NonCurrentBranch.PK, line.AL_GB);
			AssertEquals(withInvalidDepartment ? GlbDepartment.CurrentDepartment.PK : TestObjectCreator.NonCurrentDepartment.PK, line.AL_GE);
			AssertEquals(taxDate, line.AL_TaxDate);
		}

		TransactionInfo CreateTransaction(OrganizationAddress orgAddress, bool withInvalidBranch, bool withInvalidDepartment)
		{
			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), Country = Country.New(RefCountry.LoadFromCountryCode(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)) },

				OrganizationAddress = orgAddress,

				TransactionDate = ZDateTime.Today.AddDays(-3),
				DueDate = ZDateTime.Today.AddDays(2),
				PostDate = ZDateTime.Today,
				Number = "1234",
				CheckNumberOrPaymentRef = "Cheque#1",
				NumberOfSupportingDocuments = 5,
				Description = "Some text",
				Branch = new Branch { Code = withInvalidBranch ? "XXX" : TestObjectCreator.NonCurrentBranch.GB_Code.ToString() },
				Department = new Department { Code = withInvalidDepartment ? "YYY" : TestObjectCreator.NonCurrentDepartment.GE_Code.ToString() },

				ExchangeRate = 2m,
				OSCurrency = new Currency { Code = "GBP" },
				LocalCurrency = new Currency { Code = "AUD" },
				OSExGSTVATAmount = 120,
				LocalExVATAmount = 60,
				OSGSTVATAmount = 10,
				LocalVATAmount = 5
			};

			return universalTransaction;
		}

		TransactionInfo CreateTransactionWithLine(OrganizationAddress orgAddress, bool withInvalidBranch = false, bool withInvalidDepartment = false)
		{
			var universalTransaction = CreateTransaction(orgAddress, withInvalidBranch, withInvalidDepartment);

			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.Branch = new Branch { Code = withInvalidBranch ? "XXX" : TestObjectCreator.NonCurrentBranch.GB_Code.ToString() };
			universalLine.Department = new Department { Code = withInvalidDepartment ? "YYY" : TestObjectCreator.NonCurrentDepartment.GE_Code.ToString() };
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.RevenueChargeCode.AC_Code };
			universalLine.Description = "Some line text";
			universalLine.GovernmentReportingChargeCode = "Govt Charge Code";
			universalLine.IsFinalCharge = true;
			universalLine.Sequence = 3;
			universalLine.OSCurrency = new Currency { Code = "GBP" };
			universalLine.LocalCurrency = new Currency { Code = "AUD" };
			universalLine.OSAmount = 130;
			universalLine.LocalAmount = 65;
			universalLine.TaxDate = ZDate.Today.AddDays(-2);
			universalLine.SupplyType = new UniversalCodeDescriptionPair { Code = "DSB" };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			return universalTransaction;
		}

		public void TestImportTransactionLines()
		{
			var address = TestObjectCreator.CreateAddress(TestObjectCreator.Creditor1, "123 Road Ave");
			var contact = TestObjectCreator.CreateContact(TestObjectCreator.Creditor1, "Mark", "ww@www.com");
			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddress.AddressType = nameof(DocAddressType.None);
			orgAddress.AddressShortCode = address.OA_Code;
			orgAddress.Address1 = address.OA_Address1;
			orgAddress.OrganizationCode = "CreditorABC";
			orgAddress.Contact = contact.OC_ContactName;
			orgAddress.Email = contact.OC_Email;

			var currentCompnay = GlbCompany.GetCurrentCompany(Factory);
			OrgPatternMatchOverride patternMatchOverride = AddMatchingRuleForOrganization(currentCompnay.OrgProxy, orgAddress.OrganizationCode.GetValueOrDefault(), TestObjectCreator.Creditor1);

			patternMatchOverride = AddMatchingRuleForChargeCode(currentCompnay.OrgProxy, "FrnCode", TestObjectCreator.CC1);

			Factory.Save();

			var prevGC_IsWHTRegistered = currentCompnay.GC_IsWHTRegistered;
			Action tearDown = () =>
			{
				currentCompnay.GC_IsWHTRegistered = prevGC_IsWHTRegistered;
				Factory.Save();
			};

			using (new DisposableAction(tearDown))
			{
				var taxID = TestObjectCreator.GST1;
				var whtTaxID = TestObjectCreator.WHT1;
				Factory.Save();

				var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = DataContextFactory.New(),
					OrganizationAddress = orgAddress,
					BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), Country = Country.New(RefCountry.LoadFromCountryCode(Factory, "NZ")) }
				};
				universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());

				var universalLine1 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Branch = new Branch { Code = TestObjectCreator.NonCurrentBranch.GB_Code },
					Department = new Department { Code = TestObjectCreator.NonCurrentDepartment.GE_Code },
					ChargeCode = new ChargeCode { Code = "FrnCode" },
					Description = "Some line text",
					IsFinalCharge = true,
					Sequence = 3,
					OSCurrency = new Currency { Code = "USD" },
					LocalCurrency = new Currency { Code = "AUD" },
					OSAmount = -80,
					OSGSTVATAmount = -10,
					LocalAmount = -40,
					LocalGSTVATAmount = -5,
					OSWHTAmount = -6,
					VATTaxID = new TaxID { TaxCode = taxID.AT_Code },
					WithholdingTaxID = new TaxID { TaxCode = whtTaxID.AW_Code }
				};
				universalTransaction.PostingJournalCollection.Add(universalLine1);

				var universalTransactionXml = universalTransaction.Serialize();
				var unallocatedTransaction = TestObjectCreator.CreateTransactionPendingAllocation("INV1", TestObjectCreator.Creditor2, 100);
				var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
				request.Initialize(unallocatedTransaction, universalTransactionXml, false);
				Factory.Save();

				var importer = new TransactionImporter();
				var invoice = new BusinessObjectFactory().Load<APInvoice>(unallocatedTransaction.PK);
				invoice.SubmittedFromInvoicingForm = true;
				invoice.AH_Ledger = LedgerTypes.AccountsPayable;
				invoice.AH_TransactionType = TransactionTypes.Invoice;
				importer.ImportTransactionLines(universalTransactionXml, invoice, false);
				AssertEquals("ImportedCreditor", "ZCreditor1", invoice.ImportedCreditor);
				AssertEquals("ImportedCreditorXmlCode", "CreditorABC", invoice.ImportedCreditorXmlCode);
				AssertEquals(1, invoice.Lines.Count);
				var line = invoice.Lines[0];
				AssertEquals("Branch", TestObjectCreator.NonCurrentBranch.PK, line.AL_GB);
				AssertEquals("Department", TestObjectCreator.NonCurrentDepartment.PK, line.AL_GE);
				AssertEquals("GenericCharge", TestObjectCreator.CC1.PK, line.GenericCharge);
				AssertEquals("Description", universalLine1.Description, line.AL_Desc);
				AssertEquals("IsFinalCharge", universalLine1.IsFinalCharge, line.AL_IsFinalCharge);
				AssertEquals("Sequence", universalLine1.Sequence, (int)line.AL_Sequence);
				AssertEquals("Currency", universalLine1.OSCurrency.Code, line.AL_RX_NKTransactionCurrency);
				AssertEquals("ExchangeRate", 2m, line.AL_ExchangeRate);
				AssertEquals("OSExTaxAmount", 90m, line.AL_OSExTaxAmount);
				AssertEquals("OSTaxAmount", 0m, line.AL_OSTaxAmount);
				AssertEquals("LocalExTaxAmount", 45m, line.AL_LocalExTaxAmount);
				AssertEquals("LocalTaxAmount", 0m, line.AL_LocalTaxAmount);
				AssertEquals("OSWHTAmount", 0m, line.AL_OSWHTAmount);

				universalTransaction.BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), Country = Country.New(GlbCompany.CurrentCompany.Country) };
				var universalLine2 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					GLAccount = new GLAccount { AccountCode = TestObjectCreator.GLHeader1.AG_AccountNum },
					OSAmount = 50
				};
				universalTransaction.PostingJournalCollection.Add(universalLine2);
				universalTransactionXml = universalTransaction.Serialize();
				request.Initialize(unallocatedTransaction, universalTransactionXml, false);
				Factory.Save();

				invoice.Lines.RemoveAndDeleteAll();
				universalTransactionXml = universalTransaction.Serialize();
				importer.ImportTransactionLines(universalTransactionXml, invoice, false);
				AssertEquals(2, invoice.Lines.Count);
				line = invoice.Lines[0];
				AssertEquals("Branch", TestObjectCreator.NonCurrentBranch.PK, line.AL_GB);
				AssertEquals("Department", TestObjectCreator.NonCurrentDepartment.PK, line.AL_GE);
				AssertEquals("GenericCharge", TestObjectCreator.CC1.PK, line.GenericCharge);
				AssertEquals("Description", universalLine1.Description, line.AL_Desc);
				AssertEquals("IsFinalCharge", universalLine1.IsFinalCharge, line.AL_IsFinalCharge);
				AssertEquals("Sequence", universalLine1.Sequence, (int)line.AL_Sequence);
				AssertEquals("Currency", universalLine1.OSCurrency.Code, line.AL_RX_NKTransactionCurrency);
				AssertEquals("ExchangeRate", 2m, line.AL_ExchangeRate);
				AssertEquals("OSExTaxAmount", 80m, line.AL_OSExTaxAmount);
				AssertEquals("OSTaxAmount", 10m, line.AL_OSTaxAmount);
				AssertEquals("LocalExTaxAmount", 40m, line.AL_LocalExTaxAmount);
				AssertEquals("LocalTaxAmount", 5m, line.AL_LocalTaxAmount);
				AssertEquals("OSWHTAmount", 0m, line.AL_OSWHTAmount);
				AssertEquals("IndexOfImportedUniversalTransactionLine", 0, line.IndexOfImportedUniversalTransactionLine);
				AssertEquals("ImportedChargeCode", "ZZCC1", line.ImportedChargeCode);
				AssertEquals("ImportedChargeCode", "FrnCode", line.ImportedChargeCodeXmlCode);

				line = invoice.Lines[1];
				AssertEquals("Branch", GlbBranch.CurrentBranch.PK, line.AL_GB);
				AssertEquals("Department", GlbDepartment.CurrentDepartment.PK, line.AL_GE);
				AssertEquals("GenericCharge", TestObjectCreator.GLHeader1.PK, line.GenericCharge);
				AssertEquals("Description", "", line.AL_Desc);
				AssertEquals("IsFinalCharge", false, line.AL_IsFinalCharge);
				AssertEquals("Sequence", 4, (int)line.AL_Sequence);
				AssertEquals("Currency", "AUD", line.AL_RX_NKTransactionCurrency);
				AssertEquals("ExchangeRate", 1m, line.AL_ExchangeRate);
				AssertEquals("OSExTaxAmount", -50m, line.AL_OSExTaxAmount);
				AssertEquals("OSTaxAmount", 0m, line.AL_OSTaxAmount);
				AssertEquals("LocalExTaxAmount", -50m, line.AL_LocalExTaxAmount);
				AssertEquals("LocalTaxAmount", 0m, line.AL_LocalTaxAmount);
				AssertEquals("OSWHTAmount", 0m, line.AL_OSWHTAmount);
				AssertEquals("IndexOfImportedUniversalTransactionLine", 1, line.IndexOfImportedUniversalTransactionLine);
				AssertEquals("ImportedChargeCode", "", line.ImportedChargeCode);
				AssertEquals("ImportedChargeCode", "", line.ImportedChargeCodeXmlCode);

				Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).GC_IsWHTRegistered = true;
				TestObjectCreator.Creditor1.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;
				Factory.Save();
				universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
				universalTransaction.PostingJournalCollection.Add(universalLine1);
				var creditNote = new BusinessObjectFactory().New<APCreditNote>();
				creditNote.AH_OH = TestObjectCreator.Creditor1.PK;
				universalTransactionXml = universalTransaction.Serialize();
				importer.ImportTransactionLines(universalTransactionXml, creditNote, false);
				AssertEquals(1, creditNote.Lines.Count);
				line = creditNote.Lines[0];
				AssertEquals("Branch", TestObjectCreator.NonCurrentBranch.PK, line.AL_GB);
				AssertEquals("Department", TestObjectCreator.NonCurrentDepartment.PK, line.AL_GE);
				AssertEquals("GenericCharge", TestObjectCreator.CC1.PK, line.GenericCharge);
				AssertEquals("Description", universalLine1.Description, line.AL_Desc);
				AssertEquals("IsFinalCharge", universalLine1.IsFinalCharge, line.AL_IsFinalCharge);
				AssertEquals("Sequence", universalLine1.Sequence, (int)line.AL_Sequence);
				AssertEquals("Currency", universalLine1.OSCurrency.Code, line.AL_RX_NKTransactionCurrency);
				AssertEquals("ExchangeRate", 2m, line.AL_ExchangeRate);
				AssertEquals("OSExTaxAmount", -90m, line.AL_OSExTaxAmount);
				AssertEquals("OSTaxAmount", 0m, line.AL_OSTaxAmount);
				AssertEquals("LocalExTaxAmount", -45m, line.AL_LocalExTaxAmount);
				AssertEquals("LocalTaxAmount", 0m, line.AL_LocalTaxAmount);
				AssertEquals("OSWHTAmount", -4.5m, line.AL_OSWHTAmount);
			}
		}

		public void TestImportTransaction_CreditNote()
		{
			var address = TestObjectCreator.CreateAddress(TestObjectCreator.Creditor1, "123 Road Ave");
			var contact = TestObjectCreator.CreateContact(TestObjectCreator.Creditor1, "Mark", "ww@www.com");

			Factory.Save();

			var importer = new TransactionImporter();
			var message = new BusinessObjectFactory().New<EDIMessage>();
			var universalFactory = new UniversalObjectFactory();
			var serviceLogger = new ServiceTaskLogForTesting();
			var logger = new XmlSessionTracker(serviceLogger);
			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.None),
				AddressShortCode = address.OA_Code,
				Address1 = address.OA_Address1,
				OrganizationCode = TestObjectCreator.Creditor1.OH_Code,
				Contact = contact.OC_ContactName,
				Email = contact.OC_Email
			};
			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), Country = Country.New(RefCountry.LoadFromCountryCode(Factory, "NZ")) },

				OrganizationAddress = orgAddress,

				Ledger = LedgerTypes.AccountsPayable,
				TransactionDate = ZDateTime.Today.AddDays(-3),
				DueDate = ZDateTime.Today.AddDays(2),
				PostDate = ZDateTime.Today.AddDays(-1),
				Number = "1234",
				CheckNumberOrPaymentRef = "Cheque#1",
				NumberOfSupportingDocuments = 5,
				Description = "Some text",
				Branch = new Branch { Code = TestObjectCreator.NonCurrentBranch.GB_Code },
				Department = new Department { Code = TestObjectCreator.NonCurrentDepartment.GE_Code },

				OSCurrency = new Currency { Code = "GBP" },
				LocalCurrency = new Currency { Code = "AUD" },
				OSExGSTVATAmount = 120,
				LocalExVATAmount = 60,
				OSGSTVATAmount = 10,
				LocalVATAmount = 5
			};

			AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", !result);
			var createdInvoices = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(0, createdInvoices.Length);
			Assert("logger.HasErrors", logger.HasErrors);
			AssertEquals("Error - Transaction Pending Allocation is not created since it would create Transaction Pending Allocation with negative amounts but Posting of Credit Notes is prevented. This is controlled by the registry setting Accounting -> Payable Defaults -> Default Settings -> Prevent Creation of Credit Notes.", logger.ToString());

			AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			universalFactory = new UniversalObjectFactory();
			serviceLogger = new ServiceTaskLogForTesting();
			logger = new XmlSessionTracker(serviceLogger);
			universalTransaction.Ledger = LedgerTypes.AccountsPayable;
			result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			createdInvoices = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			var newFactory = new BusinessObjectFactory();
			var savedInvoice = newFactory.Load<InvoicingBase>(createdInvoices[0].PK);
			AssertType(typeof(TransactionPendingAllocation), savedInvoice);
			AssertEquals("Organization", TestObjectCreator.Creditor1.PK, savedInvoice.AH_OH);
			AssertEquals("Address", address.PK, savedInvoice.AH_OA_InvoiceAddressOverride);
			AssertEquals("Contact", contact.PK, savedInvoice.AH_OC_InvoiceContactOverride);
			AssertEquals("TransactionDate", universalTransaction.TransactionDate, savedInvoice.AH_InvoiceDate);
			AssertEquals("PostDate", universalTransaction.PostDate, savedInvoice.AH_PostDate);
			AssertEquals("TransactionNumber", universalTransaction.Number, savedInvoice.AH_TransactionNum);
			AssertEquals("ChequeOrReference", universalTransaction.CheckNumberOrPaymentRef, savedInvoice.AH_ChequeOrReference);
			AssertEquals("NumberOfSupportingDocuments", universalTransaction.NumberOfSupportingDocuments, (int)savedInvoice.AH_NumberOfSupportingDocuments);
			AssertEquals("Currency", universalTransaction.OSCurrency.Code, savedInvoice.AH_RX_NKTransactionCurrency);
			AssertEquals("ExchangeRate", 2m, savedInvoice.AH_ExchangeRate);
			AssertEquals("OSExTaxAmount", -130m, savedInvoice.AH_OSExTaxAmount);
			AssertEquals("OSTaxAmount", 0m, savedInvoice.AH_OSTaxAmount);
			AssertEquals("LocalExTaxAmount", -65m, savedInvoice.AH_LocalExTaxAmount);
			AssertEquals("LocalTaxAmount", 0m, savedInvoice.AH_LocalTaxAmount);
			AssertEquals("Transaction type", TransactionTypes.CreditNotePendingAllocation, savedInvoice.AH_TransactionType);
			AssertEquals("Branch", TestObjectCreator.NonCurrentBranch.PK, savedInvoice.AH_GB);
			AssertEquals("Department", TestObjectCreator.NonCurrentDepartment.PK, savedInvoice.AH_GE);
			Assert("logger.HasErrors", !logger.HasErrors);
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
		}

		public void TestInvoiceLocalAmountHasDifferentLocalCurrency()
		{
			AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var importer = new TransactionImporter();
			var message = new BusinessObjectFactory().New<EDIMessage>();
			var universalFactory = new UniversalObjectFactory();
			var serviceLogger = new ServiceTaskLogForTesting();
			var logger = new XmlSessionTracker(serviceLogger);
			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), Country = Country.New(GlbCompany.CurrentCompany.Country) },
				Ledger = LedgerTypes.AccountsPayable,
				OSCurrency = new Currency { Code = "GBP" },
				LocalCurrency = new Currency { Code = "AUD" },
				OSExGSTVATAmount = 120,
				LocalExVATAmount = 60,
				OSGSTVATAmount = 10,
				LocalVATAmount = 5
			};

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.GBP, 10);

			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			var createdInvoices = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			var newFactory = new BusinessObjectFactory();
			var savedInvoice = newFactory.Load<InvoicingBase>(createdInvoices[0].PK);
			AssertType(typeof(TransactionPendingAllocation), savedInvoice);
			AssertEquals("Currency", universalTransaction.OSCurrency.Code, savedInvoice.AH_RX_NKTransactionCurrency);
			AssertEquals("ExchangeRate", 2m, savedInvoice.AH_ExchangeRate);
			AssertEquals("OSExTaxAmount", -120m, savedInvoice.AH_OSExTaxAmount);
			AssertEquals("OSTaxAmount", -10m, savedInvoice.AH_OSTaxAmount);
			AssertEquals("LocalExTaxAmount", -60m, savedInvoice.AH_LocalExTaxAmount);
			AssertEquals("LocalTaxAmount", -5m, savedInvoice.AH_LocalTaxAmount);
			Assert("logger.HasErrors", !logger.HasErrors);
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);

			universalTransaction.LocalCurrency = new Currency { Code = "USD" };
			universalTransaction.Number = string.Format(CultureInfo.InvariantCulture, "{0}_2", universalTransaction.Number);
			universalFactory = new UniversalObjectFactory();
			serviceLogger = new ServiceTaskLogForTesting();
			logger = new XmlSessionTracker(serviceLogger);
			result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			createdInvoices = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			newFactory = new BusinessObjectFactory();
			savedInvoice = newFactory.Load<InvoicingBase>(createdInvoices[0].PK);
			AssertType(typeof(TransactionPendingAllocation), savedInvoice);
			AssertEquals("Currency", universalTransaction.OSCurrency.Code, savedInvoice.AH_RX_NKTransactionCurrency);
			AssertEquals("ExchangeRate", 10m, savedInvoice.AH_ExchangeRate);
			AssertEquals("OSExTaxAmount", -120m, savedInvoice.AH_OSExTaxAmount);
			AssertEquals("OSTaxAmount", -10m, savedInvoice.AH_OSTaxAmount);
			AssertEquals("LocalExTaxAmount", -12m, savedInvoice.AH_LocalExTaxAmount);
			AssertEquals("LocalTaxAmount", -1m, savedInvoice.AH_LocalTaxAmount);
			Assert("logger.HasErrors", !logger.HasErrors);
			AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
		}

		public void TestLineLocalAmountHasDifferentLocalCurrency()
		{
			var currentCompnay = GlbCompany.GetCurrentCompany(Factory);
			var prevGC_IsWHTRegistered = currentCompnay.GC_IsWHTRegistered;
			Action tearDown = () =>
			{
				currentCompnay.GC_IsWHTRegistered = prevGC_IsWHTRegistered;
				Factory.Save();
			};

			using (new DisposableAction(tearDown))
			{
				currentCompnay.GC_IsWHTRegistered = true;

				var taxID = TestObjectCreator.GST1;
				var whtTaxID = TestObjectCreator.WHT1;
				Factory.Save();

				var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = DataContextFactory.New(),
					BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), Country = Country.New(GlbCompany.CurrentCompany.Country) },
				};
				universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());

				var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					OSCurrency = new Currency { Code = "GBP" },
					LocalCurrency = new Currency { Code = "AUD" },
					OSAmount = -80,
					OSGSTVATAmount = -10,
					LocalAmount = -40,
					LocalGSTVATAmount = -5,
					LocalExtraVATAmount = -2,
					OSWHTAmount = -6,
					VATTaxID = new TaxID { TaxCode = taxID.AT_Code },
					WithholdingTaxID = new TaxID { TaxCode = whtTaxID.AW_Code }
				};
				universalTransaction.PostingJournalCollection.Add(universalLine);

				TestObjectCreator.CreateExchangeRate(TestObjectCreator.GBP, 10);

				var importer = new TransactionImporter();
				var invoice = new BusinessObjectFactory().New<APInvoice>();
				var universalTransactionXml = universalTransaction.Serialize();
				importer.ImportTransactionLines(universalTransactionXml, invoice, false);
				AssertEquals(1, invoice.Lines.Count);
				var line = invoice.Lines[0];
				AssertEquals("Currency", universalLine.OSCurrency.Code, line.AL_RX_NKTransactionCurrency);
				AssertEquals("ExchangeRate", 2m, line.AL_ExchangeRate);
				AssertEquals("OSExTaxAmount", 80m, line.AL_OSExTaxAmount);
				AssertEquals("OSTaxAmount", 10m, line.AL_OSTaxAmount);
				AssertEquals("LocalExTaxAmount", 40m, line.AL_LocalExTaxAmount);
				AssertEquals("LocalTaxAmount", 5m, line.AL_LocalTaxAmount);
				AssertEquals("LocalExtraTaxAmount should not be set since the tax does not qualify for persistent extra tax type", 0m, line.AL_LocalExtraTaxAmount);
				AssertEquals("OSWHTAmount", 4m, line.AL_OSWHTAmount);
				AssertEquals("OSWHTAmount", 2m, line.AL_LocalWHTAmount);

				universalLine.LocalCurrency = new Currency { Code = "USD" };
				invoice = new BusinessObjectFactory().New<APInvoice>();
				universalTransactionXml = universalTransaction.Serialize();
				importer.ImportTransactionLines(universalTransactionXml, invoice, false);
				AssertEquals(1, invoice.Lines.Count);
				line = invoice.Lines[0];
				AssertEquals("Currency", universalLine.OSCurrency.Code, line.AL_RX_NKTransactionCurrency);
				AssertEquals("ExchangeRate", 10m, line.AL_ExchangeRate);
				AssertEquals("OSExTaxAmount", 80m, line.AL_OSExTaxAmount);
				AssertEquals("OSTaxAmount", 10m, line.AL_OSTaxAmount);
				AssertEquals("LocalExTaxAmount", 8m, line.AL_LocalExTaxAmount);
				AssertEquals("LocalTaxAmount", 1m, line.AL_LocalTaxAmount);
				AssertEquals("OSWHTAmount", 4m, line.AL_OSWHTAmount);
				AssertEquals("OSWHTAmount", 0.4m, line.AL_LocalWHTAmount);
			}
		}

		public void TestLineTaxID()
		{
			var companyInCurrentCountry = Factory.NewWithValidTestData<GlbCompany>();
			companyInCurrentCountry.GC_RN_NKCountryCode = "AU";
			AssertEquals("Precondition: correct country code", companyInCurrentCountry.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var companyInAnotherCountry = Factory.NewWithValidTestData<GlbCompany>();
			companyInAnotherCountry.GC_RN_NKCountryCode = "NZ";

			var taxID = TestObjectCreator.GST1;
			Factory.Save();

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
			};
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.DataContext.SetCompanyAndDataProviderDetails(companyInCurrentCountry);
			universalTransaction.BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), OrganizationCode = GlbCompany.CurrentCompany.OrgProxy.OH_Code };

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				VATTaxID = new TaxID { TaxCode = taxID.AT_Code }
			};
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var invoice = new BusinessObjectFactory().New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;

			var importer = new TransactionImporter();
			invoice.Lines.RemoveAndDeleteAll();
			var universalTransactionXml = universalTransaction.Serialize();
			importer.ImportTransactionLines(universalTransactionXml, invoice, false);
			AssertEquals(1, invoice.Lines.Count);
			var line = invoice.Lines[0];
			AssertEquals("TaxID", taxID.PK, line.AL_AT);

			AssertEquals("TotalErrorCount", 0, ErrorReporter.TotalErrorCount);

			universalTransaction.BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), Country = Country.New(GlbCompany.CurrentCompany.Country) };
			invoice.Lines.RemoveAndDeleteAll();
			universalTransactionXml = universalTransaction.Serialize();
			importer.ImportTransactionLines(universalTransactionXml, invoice, false);
			AssertEquals(1, invoice.Lines.Count);
			line = invoice.Lines[0];
			AssertEquals("TaxID", taxID.PK, line.AL_AT);
			AssertEquals("TotalErrorCount", 0, ErrorReporter.TotalErrorCount);

			universalTransaction.DataContext.SetCompanyAndDataProviderDetails(companyInAnotherCountry);
			universalTransaction.BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), Country = Country.New(RefCountry.LoadFromCountryCode(Factory, "NZ")) };
			invoice.Lines.RemoveAndDeleteAll();
			universalTransactionXml = universalTransaction.Serialize();
			importer.ImportTransactionLines(universalTransactionXml, invoice, false);
			AssertEquals(1, invoice.Lines.Count);
			line = invoice.Lines[0];
			Assert("TaxID", line.AL_AT.IsEmpty);

			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.RevenueChargeCode.AC_Code };
			invoice.Lines.RemoveAndDeleteAll();
			universalTransactionXml = universalTransaction.Serialize();
			importer.ImportTransactionLines(universalTransactionXml, invoice, false);
			AssertEquals(1, invoice.Lines.Count);
			line = invoice.Lines[0];
			AssertEquals("TaxID", AccTaxRate.Types.NotReportable, line.TaxRate.AT_Type);
		}

		public void TestLineExtraTaxForIndia()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var taxID = TestObjectCreator.STAGST;
				Factory.Save();

				var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = DataContextFactory.New(),
					BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), Country = Country.New(GlbCompany.CurrentCompany.Country) },
				};
				universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());

				var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					VATTaxID = new TaxID { TaxCode = taxID.AT_Code },
					LocalExtraVATAmount = -2
				};
				universalTransaction.PostingJournalCollection.Add(universalLine);

				var invoice = new BusinessObjectFactory().New<APInvoice>();
				invoice.AH_OH = TestObjectCreator.Creditor1.PK;

				var importer = new TransactionImporter();
				invoice.Lines.RemoveAndDeleteAll();
				var universalTransactionXml = universalTransaction.Serialize();
				importer.ImportTransactionLines(universalTransactionXml, invoice, false);
				AssertEquals(1, invoice.Lines.Count);
				var line = invoice.Lines[0];
				AssertEquals("TaxID", taxID.PK, line.AL_AT);
				AssertEquals("LocalExtraTaxAmount", 2M, line.AL_LocalExtraTaxAmount);
			}
		}

		public void TestLineTaxMessageID()
		{
			var taxID = TestObjectCreator.GST1;
			var taxMessage = TestObjectCreator.TaxMsg1;
			Factory.Save();

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), Country = Country.New(GlbCompany.CurrentCompany.Country) },
			};
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.TaxMessageID = new TaxMessageID { TaxMessageCode = taxMessage.A9_Code };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var invoice = new BusinessObjectFactory().New<APInvoice>();

			var importer = new TransactionImporter();
			invoice.Lines.RemoveAndDeleteAll();
			var universalTransactionXml = universalTransaction.Serialize();
			importer.ImportTransactionLines(universalTransactionXml, invoice, false);
			AssertEquals(1, invoice.Lines.Count);
			var line = invoice.Lines[0];
			AssertEquals("TaxMessageID", ZGuid.Empty, line.AL_A9_VATClass);

			universalLine.VATTaxID = new TaxID { TaxCode = taxID.AT_Code };
			invoice.Lines.RemoveAndDeleteAll();
			universalTransactionXml = universalTransaction.Serialize();
			importer.ImportTransactionLines(universalTransactionXml, invoice, false);
			AssertEquals(1, invoice.Lines.Count);
			line = invoice.Lines[0];
			AssertEquals("TaxMessageID", taxMessage.PK, line.AL_A9_VATClass);
		}

		public void TestImportTransactionLineWithPlaceOfSupply()
		{
			var importer = new TransactionImporter();

			var headerPlaceOfSupply = new PlaceOfSupply();
			headerPlaceOfSupply.Location = new CodeDescriptionPair5Char { Code = "JH" };
			headerPlaceOfSupply.LocationType = new UniversalCodeDescriptionPair { Code = PlaceOfSupplyTypes.State.Code };

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), Country = Country.New(GlbCompany.CurrentCompany.Country) },
				PlaceOfSupply = headerPlaceOfSupply,
			};
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());

			var linePlaceOfSupply1 = new PlaceOfSupply();
			linePlaceOfSupply1.Location = new CodeDescriptionPair5Char { Code = "JH" };
			linePlaceOfSupply1.LocationType = new UniversalCodeDescriptionPair { Code = PlaceOfSupplyTypes.State.Code };

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.PlaceOfSupply = linePlaceOfSupply1;
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var linePlaceOfSupply2 = new PlaceOfSupply();
			linePlaceOfSupply2.Location = new CodeDescriptionPair5Char { Code = "DL" };
			linePlaceOfSupply2.LocationType = new UniversalCodeDescriptionPair { Code = PlaceOfSupplyTypes.State.Code };

			var universalLine2 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine2.PlaceOfSupply = linePlaceOfSupply2;
			universalTransaction.PostingJournalCollection.Add(universalLine2);

			var apInvoice = new BusinessObjectFactory().New<APInvoice>();
			AssertImport(apInvoice, string.Empty, string.Empty, string.Empty, string.Empty);

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				AssertImport(apInvoice, "JH", PlaceOfSupplyTypes.State.Code, "DL", PlaceOfSupplyTypes.State.Code);
			}

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				AssertImport(apInvoice, "JH", PlaceOfSupplyTypes.State.Code, "JH", PlaceOfSupplyTypes.State.Code);
			}

			var arInvoice = new BusinessObjectFactory().New<ARInvoice>();
			AssertImport(arInvoice, string.Empty, string.Empty, string.Empty, string.Empty);

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				AssertImport(arInvoice, "JH", PlaceOfSupplyTypes.State.Code, "DL", PlaceOfSupplyTypes.State.Code);
			}

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				AssertImport(arInvoice, "JH", PlaceOfSupplyTypes.State.Code, "JH", PlaceOfSupplyTypes.State.Code);
			}

			void AssertImport(InvoicingBase invoice, string line1PlaceOfSupply, string line1PlaceOfSupplyType, string line2PlaceOfSupply, string line2PlaceOfSupplyType)
			{
				invoice.Lines.RemoveAndDeleteAll();
				var universalTransactionXml = universalTransaction.Serialize();
				importer.ImportTransactionLines(universalTransactionXml, invoice, false);
				AssertEquals(2, invoice.Lines.Count);
				AssertEquals(line1PlaceOfSupply, invoice.Lines[0].AL_PlaceOfSupply);
				AssertEquals(line1PlaceOfSupplyType, invoice.Lines[0].AL_PlaceOfSupplyType);
				AssertEquals(line2PlaceOfSupply, invoice.Lines[1].AL_PlaceOfSupply);
				AssertEquals(line2PlaceOfSupplyType, invoice.Lines[1].AL_PlaceOfSupplyType);
			}
		}

		public void TestImportTransactionLine_LinePlaceOfSupplyWithNullLocation()
		{
			AssertImportTransactionLine_PlaceOfSupplyWithNullLocationOrLocationType(null, PlaceOfSupplyTypes.State.Code);
		}

		public void TestImportTransactionLine_LinePlaceOfSupplyWithNullLocationType()
		{
			AssertImportTransactionLine_PlaceOfSupplyWithNullLocationOrLocationType(null, null);
		}

		void AssertImportTransactionLine_PlaceOfSupplyWithNullLocationOrLocationType(string lineLocation, string lineLocationType)
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				var importer = new TransactionImporter();

				var headerPlaceOfSupply = new PlaceOfSupply();
				headerPlaceOfSupply.Location = new CodeDescriptionPair5Char { Code = "JH" };
				headerPlaceOfSupply.LocationType = new UniversalCodeDescriptionPair { Code = PlaceOfSupplyTypes.State.Code };

				var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = DataContextFactory.New(),
					BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), Country = Country.New(GlbCompany.CurrentCompany.Country) },
					PlaceOfSupply = headerPlaceOfSupply,
				};
				universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());

				var linePlaceOfSupply = new PlaceOfSupply();
				linePlaceOfSupply.Location = lineLocation != null ? new CodeDescriptionPair5Char { Code = lineLocation } : null;
				linePlaceOfSupply.LocationType = lineLocationType != null ? new UniversalCodeDescriptionPair { Code = lineLocationType } : null;

				var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
				universalLine.PlaceOfSupply = linePlaceOfSupply;
				universalTransaction.PostingJournalCollection.Add(universalLine);

				var apInvoice = new BusinessObjectFactory().New<APInvoice>();
				apInvoice.Lines.RemoveAndDeleteAll();
				var universalTransactionXml = universalTransaction.Serialize();

				AssertNoExceptionThrown(() => importer.ImportTransactionLines(universalTransactionXml, apInvoice, false));
				AssertEquals(1, apInvoice.Lines.Count);
				AssertEquals(lineLocation ?? string.Empty, apInvoice.Lines[0].AL_PlaceOfSupply);
				AssertEquals(lineLocationType ?? string.Empty, apInvoice.Lines[0].AL_PlaceOfSupplyType);
			}
		}

		public void TestLineRecoverableGSTVATPercentage()
		{
			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var importer = new TransactionImporter();
			var invoice = new BusinessObjectFactory().New<APInvoice>();
			var universalTransactionXml = universalTransaction.Serialize();
			importer.ImportTransactionLines(universalTransactionXml, invoice, false);
			AssertEquals(1, invoice.Lines.Count);
			var line = invoice.Lines[0];
			AssertEquals("RecoverableGSTVATPercentage", 100M, line.AL_Calc_InputGSTVATRecoverablePercentage);

			universalLine.RecoverableGSTVATPercentage = 7;
			invoice = new BusinessObjectFactory().New<APInvoice>();
			universalTransactionXml = universalTransaction.Serialize();
			importer.ImportTransactionLines(universalTransactionXml, invoice, false);
			AssertEquals(1, invoice.Lines.Count);
			line = invoice.Lines[0];
			AssertEquals("RecoverableGSTVATPercentage", 7M, line.AL_Calc_InputGSTVATRecoverablePercentage);
		}

		public void TestLineSubAccounts()
		{
			var universalTransaction = CreateUniversalTransactionWithSubAccounts(true);
			SetupPostingJournal(universalTransaction);

			var message = new BusinessObjectFactory().New<EDIMessage>();
			var importer = new TransactionImporter();

			//Test when XML not set GL Account(with Sub Account), and not set SubAccounts
			var universalFactory = new UniversalObjectFactory();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var result = false;
			AssertExceptionThrown<MessageProcessingBusinessFailureException>(() => result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory));
			var invoice = universalFactory.BOFactory.Load<ARInvoice>(new ZQuery { FetchOnlyFromLocalCache = true }).FirstOrDefault();
			AssertEquals(1, invoice.Lines.Count);
			Assert("Failed Import Transaction", !result);

			//Test when XML not set GL Account(with Sub Account), and set SubAccounts
			SetupSubAccounts(universalTransaction);
			universalFactory = new UniversalObjectFactory();
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			AssertExceptionThrown<MessageProcessingBusinessFailureException>(() => importer.ImportTransaction(message, universalTransaction, logger, universalFactory));
			invoice = universalFactory.BOFactory.Load<ARInvoice>(new ZQuery { FetchOnlyFromLocalCache = true }).FirstOrDefault();
			AssertEquals(1, invoice.Lines.Count);
			Assert("Failed Import Transaction", !result);

			//Test when XML set GL Account(with no Sub Account), and set SubAccounts
			universalTransaction.PostingJournalCollection[0].VATTaxID = new TaxID { TaxCode = TestObjectCreator.GSTFREE1.AT_Code };
			universalTransaction.BranchAddress = new OrganizationAddress { Country = new Country { Code = GlbCompany.CurrentCompany.Country.Code } };
			universalTransaction.PostingJournalCollection[0].GLAccount = new GLAccount { AccountCode = TestObjectCreator.GLHeader2.AG_AccountNum };
			universalFactory = new UniversalObjectFactory();
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			invoice = universalFactory.BOFactory.Load<ARInvoice>(new ZQuery { FetchOnlyFromLocalCache = true }).FirstOrDefault();
			Assert("Successfully Import Transaction", result);
			AssertEquals("SubAccounts", 0, invoice.Lines[0].SubAccounts.Count);

			//Test when XML set GL Account(with Sub Account), and set correct SubAccounts
			universalTransaction.PostingJournalCollection[0].GLAccount = new GLAccount { AccountCode = TestObjectCreator.GLHeader1.AG_AccountNum };
			universalFactory = new UniversalObjectFactory();
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			invoice = universalFactory.BOFactory.Load<ARInvoice>(new ZQuery { FetchOnlyFromLocalCache = true }).FirstOrDefault();
			AssertEquals(1, invoice.Lines.Count);

			Assert("Successfully Import Transaction", result);
			AssertEquals("SubAccounts", 2, invoice.Lines[0].SubAccounts.Count);
			var subAccount1 = invoice.Lines[0].SubAccounts.OfType<AccTransactionLineSubAccount>().FirstOrDefault(x => x.AL1_SubClassParentTableCode.Equals(OrgHeaderSchema.Constants.Prefix));
			var subAccount2 = invoice.Lines[0].SubAccounts.OfType<AccTransactionLineSubAccount>().FirstOrDefault(x => x.AL1_SubClassParentTableCode.Equals(AccGroupsSchema.Constants.Prefix));
			AssertEquals("SubAccounts 1", TestObjectCreator.ABIGAS.PK, subAccount1.AL1_SubClassParentId);
			AssertEquals("SubAccounts 2", TestObjectCreator.AR1.PK, subAccount2.AL1_SubClassParentId);
		}

		public void TestLineSubAccountsWithMandatorySubClass()
		{
			TestLineSubAccountsWithMandatorySubClass(true);
			TestLineSubAccountsWithMandatorySubClass(false);
		}

		void TestLineSubAccountsWithMandatorySubClass(bool isAR)
		{
			var universalTransaction = CreateUniversalTransactionWithSubAccounts(isAR);

			var glHeaderSubAccount3 = Factory.New<AccGLHeaderSubAccount>();
			glHeaderSubAccount3.ASA_AG = TestObjectCreator.GLHeader1.PK;
			glHeaderSubAccount3.ASA_SubClass = GlbStaffSchema.Constants.Prefix;
			glHeaderSubAccount3.ASA_IsSubClassValidationRuleMandatory = true;
			Factory.Save();

			SetupPostingJournal(universalTransaction);
			SetupSubAccounts(universalTransaction);

			//no Sub Account
			var message = new BusinessObjectFactory().New<EDIMessage>();
			var importer = new TransactionImporter();
			var universalFactory = new UniversalObjectFactory();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());

			universalTransaction.PostingJournalCollection[0].VATTaxID = new TaxID { TaxCode = TestObjectCreator.GSTFREE1.AT_Code };
			universalTransaction.BranchAddress = new OrganizationAddress { Country = new Country { Code = GlbCompany.CurrentCompany.Country.Code } };
			universalTransaction.PostingJournalCollection[0].GLAccount = new GLAccount { AccountCode = TestObjectCreator.GLHeader1.AG_AccountNum };
			var result = false;
			if (isAR)
			{
				var expectedExceptionMessage = @"Import failed because transaction has validation errors:
Error - Sub Account: Please enter a Sub Account.
";
				AssertExceptionThrown<MessageProcessingBusinessFailureException>("expect exception with validation error in the message",
					expectedExceptionMessage, () => result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory));
			}
			else
			{
				result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
				AssertContains("Error - Error - AL1_SubClassParentId: Please enter a Sub Account.", logger.ToString());
			}
			Assert("Filed Import Transaction", !result);

			//not valid sub account Type or value
			universalTransaction.PostingJournalCollection[0].SubAccountCollection.Add(new SubAccount { Type = new UniversalCodeDescriptionPair { Code = "Err" }, Code = TestObjectCreator.GS1.GS_Code });
			universalTransaction.PostingJournalCollection[0].SubAccountCollection.Add(new SubAccount { Type = new UniversalCodeDescriptionPair { Code = Constants.SubAccountType.StaffAndResources }, Code = "Err" });
			universalFactory = new UniversalObjectFactory();
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			if (isAR)
			{
				var expectedExceptionMessage = @"Import failed because transaction has validation errors:
Error - Sub Account: Please enter a Sub Account.
";
				AssertExceptionThrown<MessageProcessingBusinessFailureException>("expect exception with validation error in the message",
					expectedExceptionMessage, () => result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory));
			}
			else
			{
				result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
				AssertContains("Error - Error - AL1_SubClassParentId: Please enter a Sub Account.", logger.ToString());
				AssertContains("Warning - The system found invalid sub account info.", logger.ToString());
			}
			Assert("Filed Import Transaction", !result);

			//valid Sub Account
			universalTransaction.PostingJournalCollection[0].SetSubAccountCollection(() =>
			{
				var subAccountCollection = new List<SubAccount>();
				subAccountCollection.Add(new SubAccount { Type = new UniversalCodeDescriptionPair { Code = Constants.SubAccountType.StaffAndResources }, Code = TestObjectCreator.GS1.GS_Code });

				return subAccountCollection;
			});

			universalFactory = new UniversalObjectFactory();
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			if (isAR)
			{
				var invoice = universalFactory.BOFactory.Load<ARInvoice>(new ZQuery { FetchOnlyFromLocalCache = true }).FirstOrDefault();
				Assert("Successfully Import Transaction", result);
				AssertEquals(1, invoice.Lines.Count);
				AssertEquals("SubAccounts", 1, invoice.Lines[0].SubAccounts.Count);
				AssertEquals(TestObjectCreator.GS1.PK, invoice.Lines[0].SubAccounts[0].AL1_SubClassParentId);
				AssertEquals(0, invoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Length);
			}
			else
			{
				AssertContains("Error - Error - AL1_SubClassParentId: Please enter a Sub Account.", logger.ToString());
			}
		}

		public void TestLineSubAccountsWithNotValidSubAccountValue()
		{
			TestLineSubAccountsWithNotValidSubAccountValue(true);
			TestLineSubAccountsWithNotValidSubAccountValue(false);
		}

		void TestLineSubAccountsWithNotValidSubAccountValue(bool isAR)
		{
			var universalTransaction = CreateUniversalTransactionWithSubAccounts(isAR);
			SetupPostingJournal(universalTransaction);
			SetupSubAccounts(universalTransaction);
			var message = new BusinessObjectFactory().New<EDIMessage>();
			var importer = new TransactionImporter();
			var universalFactory = new UniversalObjectFactory();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());

			universalTransaction.PostingJournalCollection[0].VATTaxID = new TaxID { TaxCode = TestObjectCreator.GSTFREE1.AT_Code };
			universalTransaction.BranchAddress = new OrganizationAddress { Country = new Country { Code = GlbCompany.CurrentCompany.Country.Code } };
			universalTransaction.PostingJournalCollection[0].GLAccount = new GLAccount { AccountCode = TestObjectCreator.GLHeader1.AG_AccountNum };
			universalTransaction.PostingJournalCollection[0].SetSubAccountCollection(() =>
			{
				var subAccountCollection = new List<SubAccount>();
				subAccountCollection.Add(new SubAccount { Type = new UniversalCodeDescriptionPair { Code = Constants.SubAccountType.Organization }, Code = TestObjectCreator.ABIGAS.OH_Code });

				//not valid Sub Account, SubAccount is not Mandatory
				subAccountCollection.Add(new SubAccount { Type = new UniversalCodeDescriptionPair { Code = Constants.SubAccountType.SalesGroup }, Code = "Err" });

				return subAccountCollection;
			});

			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Successfully Import Transaction", result);
			AssertContains("Warning - The system found invalid sub account info.", logger.ToString());
			if (isAR)
			{
				var invoice = universalFactory.BOFactory.Load<ARInvoice>(new ZQuery { FetchOnlyFromLocalCache = true }).FirstOrDefault();
				AssertEquals(1, invoice.Lines.Count);
				AssertEquals("SubAccounts", 1, invoice.Lines[0].SubAccounts.Count);
				AssertEquals("Not add this error Sub Account", false, invoice.Lines[0].SubAccounts.OfType<AccTransactionLineSubAccount>().Any(x => x.AL1_SubClassParentTableCode.Equals(AccGroupsSchema.Constants.Prefix)));
				AssertEquals(0, invoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Length);
			}
			else
			{
				var invoice = universalFactory.BOFactory.Load<AccTransactionHeader>(new ZQuery { FetchOnlyFromLocalCache = true }).FirstOrDefault();
				AssertStmNote(invoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
			}
		}

		public void TestImportLineObsolateSubAccountCompatibility()
		{
			var universalTransaction = CreateUniversalTransactionWithSubAccounts(true);
			SetupPostingJournal(universalTransaction);

			var message = new BusinessObjectFactory().New<EDIMessage>();
			var importer = new TransactionImporter();
			var universalFactory = new UniversalObjectFactory();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());

			universalTransaction.PostingJournalCollection[0].VATTaxID = new TaxID { TaxCode = TestObjectCreator.GSTFREE1.AT_Code };
			universalTransaction.BranchAddress = new OrganizationAddress { Country = new Country { Code = GlbCompany.CurrentCompany.Country.Code } };
			universalTransaction.PostingJournalCollection[0].GLAccount = new GLAccount { AccountCode = TestObjectCreator.GLHeader1.AG_AccountNum };
			universalTransaction.PostingJournalCollection[0].SubAccount = new SubAccount { Type = new UniversalCodeDescriptionPair { Code = Constants.SubAccountType.Organization }, Code = TestObjectCreator.ABIGAS.OH_Code };
			universalTransaction.PostingJournalCollection[0].SetSubAccountCollection(() => new List<SubAccount>());

			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);

			Assert("Successfully Import Transaction", result);
			AssertNotContains("Warning - The system found invalid sub account info.", logger.ToString());

			var savedTransaction = universalFactory.BOFactory.Load<ARInvoice>(new ZQuery { FetchOnlyFromLocalCache = true }).SingleOrDefault();
			var savedLines = savedTransaction.Lines;
			AssertEquals(1, savedLines.Count);
			AssertEquals("Should import old sub account", 1, savedLines[0].SubAccounts.Count);
			AssertEquals(Constants.SubAccountType.Organization, savedLines[0].SubAccounts[0].SubAccountTypeDisplayCode);
			AssertEquals(TestObjectCreator.ABIGAS.PK, savedLines[0].SubAccounts[0].AL1_SubClassParentId);
			AssertEquals(0, savedTransaction.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Length);
		}

		public void TestImportLineMixedSubAccountsCompatibility()
		{
			var universalTransaction = CreateUniversalTransactionWithSubAccounts(true);
			SetupPostingJournal(universalTransaction);

			var message = new BusinessObjectFactory().New<EDIMessage>();
			var importer = new TransactionImporter();
			var universalFactory = new UniversalObjectFactory();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());

			universalTransaction.PostingJournalCollection[0].VATTaxID = new TaxID { TaxCode = TestObjectCreator.GSTFREE1.AT_Code };
			universalTransaction.BranchAddress = new OrganizationAddress { Country = new Country { Code = GlbCompany.CurrentCompany.Country.Code } };
			universalTransaction.PostingJournalCollection[0].GLAccount = new GLAccount { AccountCode = TestObjectCreator.GLHeader1.AG_AccountNum };
			universalTransaction.PostingJournalCollection[0].SubAccount = new SubAccount { Type = new UniversalCodeDescriptionPair { Code = Constants.SubAccountType.Organization }, Code = TestObjectCreator.ABIGAS.OH_Code };
			universalTransaction.PostingJournalCollection[0].SetSubAccountCollection(() =>
			{
				var subAccountCollection = new List<SubAccount>();
				subAccountCollection.Add(new SubAccount { Type = new UniversalCodeDescriptionPair { Code = Constants.SubAccountType.SalesGroup }, Code = TestObjectCreator.AR1.AR_Code });

				return subAccountCollection;
			});

			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);

			Assert("Successfully Import Transaction", result);
			AssertNotContains("Warning - The system found invalid sub account info.", logger.ToString());

			var savedTransaction = universalFactory.BOFactory.Load<ARInvoice>(new ZQuery { FetchOnlyFromLocalCache = true }).SingleOrDefault();
			var savedLines = savedTransaction.Lines;
			AssertEquals(1, savedLines.Count);
			AssertEquals("Should import new sub account", 1, savedLines[0].SubAccounts.Count);
			AssertEquals(Constants.SubAccountType.SalesGroup, savedLines[0].SubAccounts[0].SubAccountTypeDisplayCode);
			AssertEquals(TestObjectCreator.AR1.PK, savedLines[0].SubAccounts[0].AL1_SubClassParentId);
			AssertEquals(0, savedTransaction.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Length);
		}

		public void TestImportLineMixedAndDuplicateSubAccountsCompatibility()
		{
			var universalTransaction = CreateUniversalTransactionWithSubAccounts(true);
			SetupPostingJournal(universalTransaction);

			var message = new BusinessObjectFactory().New<EDIMessage>();
			var importer = new TransactionImporter();
			var universalFactory = new UniversalObjectFactory();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());

			universalTransaction.PostingJournalCollection[0].VATTaxID = new TaxID { TaxCode = TestObjectCreator.GSTFREE1.AT_Code };
			universalTransaction.BranchAddress = new OrganizationAddress { Country = new Country { Code = GlbCompany.CurrentCompany.Country.Code } };
			universalTransaction.PostingJournalCollection[0].GLAccount = new GLAccount { AccountCode = TestObjectCreator.GLHeader1.AG_AccountNum };
			universalTransaction.PostingJournalCollection[0].SubAccount = new SubAccount { Type = new UniversalCodeDescriptionPair { Code = Constants.SubAccountType.Organization }, Code = TestObjectCreator.ABIGAS.OH_Code };
			universalTransaction.PostingJournalCollection[0].SetSubAccountCollection(() =>
			{
				var subAccountCollection = new List<SubAccount>();
				subAccountCollection.Add(new SubAccount { Type = new UniversalCodeDescriptionPair { Code = Constants.SubAccountType.Organization }, Code = TestObjectCreator.AALSHI.OH_Code });

				return subAccountCollection;
			});

			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);

			Assert("Successfully Import Transaction", result);
			AssertNotContains("Warning - The system found invalid sub account info.", logger.ToString());

			var savedTransaction = universalFactory.BOFactory.Load<ARInvoice>(new ZQuery { FetchOnlyFromLocalCache = true }).SingleOrDefault();
			var savedLines = savedTransaction.Lines;
			AssertEquals(1, savedLines.Count);
			AssertEquals("Should import new sub account", 1, savedLines[0].SubAccounts.Count);
			AssertEquals(Constants.SubAccountType.Organization, savedLines[0].SubAccounts[0].SubAccountTypeDisplayCode);
			AssertEquals(TestObjectCreator.AALSHI.PK, savedLines[0].SubAccounts[0].AL1_SubClassParentId);
			AssertEquals(0, savedTransaction.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Length);
		}

		public void TestLineSubAccountsWithNotValidSubAccountType()
		{
			TestLineSubAccountsWithNotValidSubAccountTypeCore(true);
			TestLineSubAccountsWithNotValidSubAccountTypeCore(false);
		}

		void TestLineSubAccountsWithNotValidSubAccountTypeCore(bool isAR)
		{
			var universalTransaction = CreateUniversalTransactionWithSubAccounts(isAR);
			SetupPostingJournal(universalTransaction);
			SetupSubAccounts(universalTransaction);

			var message = new BusinessObjectFactory().New<EDIMessage>();
			var importer = new TransactionImporter();
			var universalFactory = new UniversalObjectFactory();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());

			universalTransaction.PostingJournalCollection[0].VATTaxID = new TaxID { TaxCode = TestObjectCreator.GSTFREE1.AT_Code };
			universalTransaction.BranchAddress = new OrganizationAddress { Country = new Country { Code = GlbCompany.CurrentCompany.Country.Code } };
			universalTransaction.PostingJournalCollection[0].GLAccount = new GLAccount { AccountCode = TestObjectCreator.GLHeader2.AG_AccountNum };
			universalTransaction.PostingJournalCollection[0].SetSubAccountCollection(() =>
			{
				var subAccountCollection = new List<SubAccount>();

				//not valid Sub Account type, GlHeader contains none Sub ACCOUNT Type
				subAccountCollection.Add(new SubAccount { Type = new UniversalCodeDescriptionPair { Code = "Err" }, Code = TestObjectCreator.ABIGAS.OH_Code });

				return subAccountCollection;
			});

			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Successfully Import Transaction", result);
			if (isAR)
			{
				var invoice = universalFactory.BOFactory.Load<ARInvoice>(new ZQuery { FetchOnlyFromLocalCache = true }).FirstOrDefault();
				AssertEquals(1, invoice.Lines.Count);
				AssertEquals("SubAccounts", 0, invoice.Lines[0].SubAccounts.Count);
				AssertNotContains("Warning - The system found invalid sub account info.", logger.ToString());
				AssertEquals(0, invoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Length);
			}
			else
			{
				var invoice = universalFactory.BOFactory.Load<AccTransactionHeader>(new ZQuery { FetchOnlyFromLocalCache = true }).FirstOrDefault();
				AssertStmNote(invoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
			}

			//not valid Sub Account type, GlHeader contains more than one Sub ACCOUNT Type, and no one was Mandatory
			universalTransaction.PostingJournalCollection[0].GLAccount = new GLAccount { AccountCode = TestObjectCreator.GLHeader1.AG_AccountNum };
			universalTransaction.PostingJournalCollection[0].SubAccountCollection.Add(new SubAccount { Type = new UniversalCodeDescriptionPair { Code = "Err" }, Code = TestObjectCreator.ABIGAS.OH_Code });
			universalTransaction.PostingJournalCollection[0].SubAccountCollection.Add(new SubAccount { Type = new UniversalCodeDescriptionPair { Code = "Err" }, Code = TestObjectCreator.AR1.AR_Code });
			universalTransaction.Number = "12346";
			universalFactory = new UniversalObjectFactory();
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Successfully Import Transaction", result);
			AssertContains("Warning - The system found invalid sub account info.", logger.ToString());

			if (isAR)
			{
				var invoice = universalFactory.BOFactory.Load<ARInvoice>(new ZQuery { FetchOnlyFromLocalCache = true }).FirstOrDefault();
				AssertEquals("SubAccounts", 0, invoice.Lines[0].SubAccounts.Count);
				AssertEquals(0, invoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Length);
			}
			else
			{
				var invoice = universalFactory.BOFactory.Load<AccTransactionHeader>(new ZQuery { FetchOnlyFromLocalCache = true }).FirstOrDefault();
				AssertStmNote(invoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
			}

			//not valid Sub Account type, GlHeader contains more than one Sub ACCOUNT Type, and no one was Mandatory
			universalTransaction.Number = "12347";
			universalTransaction.PostingJournalCollection[0].GLAccount = new GLAccount { AccountCode = TestObjectCreator.GLHeader1.AG_AccountNum };
			universalTransaction.PostingJournalCollection[0].SetSubAccountCollection(() =>
			{
				var subAccountCollection = new List<SubAccount>();
				subAccountCollection.Add(new SubAccount { Code = TestObjectCreator.ABIGAS.OH_Code });
				subAccountCollection.Add(new SubAccount { Type = new UniversalCodeDescriptionPair(), Code = TestObjectCreator.ABIGAS.OH_Code });

				return subAccountCollection;
			});

			universalFactory = new UniversalObjectFactory();
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Successfully Import Transaction", result);
			if (isAR)
			{
				var invoice = universalFactory.BOFactory.Load<ARInvoice>(new ZQuery { FetchOnlyFromLocalCache = true }).FirstOrDefault();
				AssertEquals(1, invoice.Lines.Count);
				AssertContains("Warning - The system found invalid sub account info.", logger.ToString());
				AssertEquals("SubAccounts", 0, invoice.Lines[0].SubAccounts.Count);
				AssertEquals(0, invoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Length);
			}
			else
			{
				var invoice = universalFactory.BOFactory.Load<AccTransactionHeader>(new ZQuery { FetchOnlyFromLocalCache = true }).FirstOrDefault();
				AssertStmNote(invoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
			}
		}

		public void TestLineSubAccountsWithDuplicateSubAccountType()
		{
			TestLineSubAccountsWithDuplicateSubAccountType(true);
			TestLineSubAccountsWithDuplicateSubAccountType(false, true);
			TestLineSubAccountsWithDuplicateSubAccountType(false, false);
		}

		void TestLineSubAccountsWithDuplicateSubAccountType(bool isAR, bool isINV = true)
		{
			var universalTransaction = CreateUniversalTransactionWithSubAccounts(isAR, isINV);
			SetupPostingJournal(universalTransaction);
			SetupSubAccounts(universalTransaction);

			var message = new BusinessObjectFactory().New<EDIMessage>();
			var importer = new TransactionImporter();
			var universalFactory = new UniversalObjectFactory();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());

			universalTransaction.PostingJournalCollection[0].VATTaxID = new TaxID { TaxCode = TestObjectCreator.GSTFREE1.AT_Code };
			universalTransaction.BranchAddress = new OrganizationAddress { Country = new Country { Code = GlbCompany.CurrentCompany.Country.Code } };
			universalTransaction.PostingJournalCollection[0].GLAccount = new GLAccount { AccountCode = TestObjectCreator.GLHeader1.AG_AccountNum };
			universalTransaction.PostingJournalCollection[0].SetSubAccountCollection(() =>
			{
				var subAccountCollection = new List<SubAccount>();
				subAccountCollection.Add(new SubAccount { Type = new UniversalCodeDescriptionPair { Code = Constants.SubAccountType.Organization }, Code = TestObjectCreator.ABIGAS.OH_Code });
				subAccountCollection.Add(new SubAccount { Type = new UniversalCodeDescriptionPair { Code = Constants.SubAccountType.Organization }, Code = TestObjectCreator.AALSHI.OH_Code });

				return subAccountCollection;
			});

			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Successfully Import Transaction", result);
			AssertContains(@"Warning - The system found duplicate sub account values for the same sub account type and the first valid value was imported.", logger.ToString());
			if (isAR)
			{
				var invoice = universalFactory.BOFactory.Load<ARInvoice>(new ZQuery { FetchOnlyFromLocalCache = true }).FirstOrDefault();
				AssertEquals(1, invoice.Lines.Count);
				AssertEquals("SubAccounts", 1, invoice.Lines[0].SubAccounts.Count);
				AssertEquals(TestObjectCreator.ABIGAS.PK, invoice.Lines[0].SubAccounts[0].AL1_SubClassParentId);
				AssertEquals(0, invoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Length);
			}
			else
			{
				var transactionPendingAllocation = universalFactory.BOFactory.Load<TransactionPendingAllocation>(new ZQuery { FetchOnlyFromLocalCache = true });
				AssertEquals(1, transactionPendingAllocation.Length);
				var request1 = Factory.New<TransactionPendingAllocationApprovalRequest>();
				request1.Initialize(transactionPendingAllocation[0], universalTransaction.Serialize(), false);
				Factory.Save();

				AssertStmNote(transactionPendingAllocation.Single().Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);

				if (isINV)
				{
					InvoicingBase invoicingBase = TransactionAllocationConverter.ConvertUnallocatedToAP(transactionPendingAllocation[0]).Invoice;
					Assert(invoicingBase is APInvoice);
					AssertEquals(1, invoicingBase.Lines.Count);
					AssertEquals("SubAccounts", 2, invoicingBase.Lines[0].SubAccounts.Count);
					var subAccount1 = invoicingBase.Lines[0].SubAccounts.OfType<TransactionLineSubAccount>().FirstOrDefault(x => x.AL1_SubClassParentTableCode.Equals(OrgHeaderSchema.Constants.Prefix));
					var subAccount2 = invoicingBase.Lines[0].SubAccounts.OfType<TransactionLineSubAccount>().FirstOrDefault(x => x.AL1_SubClassParentTableCode.Equals(AccGroupsSchema.Constants.Prefix));
					AssertEquals("SubAccounts 1", TestObjectCreator.ABIGAS.PK, subAccount1.AL1_SubClassParentId);
					AssertEquals("SubAccounts 2", ZGuid.Empty, subAccount2.AL1_SubClassParentId);
				}
				else
				{
					InvoicingBase invoicingBase = TransactionAllocationConverter.ConvertUnallocatedToAP(transactionPendingAllocation[0]).Invoice;
					Assert(invoicingBase is APCreditNote);
					AssertEquals(1, invoicingBase.Lines.Count);
					AssertEquals("SubAccounts", 2, invoicingBase.Lines[0].SubAccounts.Count);
					var subAccount1 = invoicingBase.Lines[0].SubAccounts.OfType<TransactionLineSubAccount>().FirstOrDefault(x => x.AL1_SubClassParentTableCode.Equals(OrgHeaderSchema.Constants.Prefix));
					var subAccount2 = invoicingBase.Lines[0].SubAccounts.OfType<TransactionLineSubAccount>().FirstOrDefault(x => x.AL1_SubClassParentTableCode.Equals(AccGroupsSchema.Constants.Prefix));
					AssertEquals("SubAccounts 1", TestObjectCreator.ABIGAS.PK, subAccount1.AL1_SubClassParentId);
					AssertEquals("SubAccounts 2", ZGuid.Empty, subAccount2.AL1_SubClassParentId);
				}
			}
		}

		public void TestImportTransactionForDuplicateTransaction()
		{
			var address = TestObjectCreator.CreateAddress(TestObjectCreator.Creditor1, "123 Road Ave");
			var contact = TestObjectCreator.CreateContact(TestObjectCreator.Creditor1, "Mark", "ww@www.com");

			Factory.Save();

			var importer = new TransactionImporter();
			var universalFactory = new UniversalObjectFactory();
			var serviceLogger = new ServiceTaskLogForTesting();
			var logger = new XmlSessionTracker(serviceLogger);
			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.None),
				AddressShortCode = address.OA_Code,
				Address1 = address.OA_Address1,
				OrganizationCode = TestObjectCreator.Creditor1.OH_Code,
				Contact = contact.OC_ContactName,
				Email = contact.OC_Email
			};
			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OrganizationAddress = orgAddress,

				TransactionDate = ZDateTime.Today.AddDays(-3),
				DueDate = ZDateTime.Today.AddDays(2),
				PostDate = ZDateTime.Today.AddDays(-1),
				Number = "1234",
				CheckNumberOrPaymentRef = "Cheque#1",
				NumberOfSupportingDocuments = 5,
				Description = "Some text",
				Branch = new Branch { Code = TestObjectCreator.NonCurrentBranch.GB_Code },
				Department = new Department { Code = TestObjectCreator.NonCurrentDepartment.GE_Code },

				OSCurrency = new Currency { Code = "GBP" },
				LocalCurrency = new Currency { Code = "AUD" },
				OSExGSTVATAmount = 120,
				LocalExVATAmount = 60,
				OSGSTVATAmount = 10,
				LocalVATAmount = 5,
				Ledger = LedgerTypes.AccountsPayable
			};

			var originalMessage = new BusinessObjectFactory().New<EDIMessage>();
			AssertEquals("Message status", EDIMessage.Status.Queued, originalMessage.EM_Status);
			var result = importer.ImportTransaction(originalMessage, universalTransaction, logger, universalFactory);
			Assert("Import result", result);
			Assert("logger.HasErrors", !logger.HasErrors);

			var duplicateMessage = new BusinessObjectFactory().New<EDIMessage>();
			AssertEquals("Message status", EDIMessage.Status.Queued, duplicateMessage.EM_Status);
			result = importer.ImportTransaction(duplicateMessage, universalTransaction, logger, universalFactory);
			Assert("Import result failed", !result);
			Assert("logger.HasErrors", logger.HasErrors);
			AssertEndsWith("logger error message", $"EDI Message {duplicateMessage.EM_MessageNum} could not be imported due to duplicate Transaction Pending Allocation", logger.ToString());
		}

		public void TestImportTransactionWithPlaceOfSupply()
		{
			var address = TestObjectCreator.CreateAddress(TestObjectCreator.Creditor1, "123 Road Ave");
			var contact = TestObjectCreator.CreateContact(TestObjectCreator.Creditor1, "Mark", "ww@www.com");

			Factory.Save();

			var importer = new TransactionImporter();
			var universalFactory = new UniversalObjectFactory();
			var serviceLogger = new ServiceTaskLogForTesting();
			var logger = new XmlSessionTracker(serviceLogger);
			var message = new BusinessObjectFactory().New<EDIMessage>();

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.None),
				AddressShortCode = address.OA_Code,
				Address1 = address.OA_Address1,
				OrganizationCode = TestObjectCreator.Creditor1.OH_Code,
				Contact = contact.OC_ContactName,
				Email = contact.OC_Email
			};

			var placeOfSupply = new PlaceOfSupply();
			placeOfSupply.Location = new CodeDescriptionPair5Char { Code = "JH" };
			placeOfSupply.LocationType = new UniversalCodeDescriptionPair { Code = PlaceOfSupplyTypes.State.Code };

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OrganizationAddress = orgAddress,

				TransactionDate = ZDateTime.Today.AddDays(-3),
				DueDate = ZDateTime.Today.AddDays(2),
				PostDate = ZDateTime.Today.AddDays(-1),
				Number = "1234",
				CheckNumberOrPaymentRef = "Cheque#1",
				NumberOfSupportingDocuments = 5,
				Description = "Some text",
				Branch = new Branch { Code = TestObjectCreator.NonCurrentBranch.GB_Code },
				Department = new Department { Code = TestObjectCreator.NonCurrentDepartment.GE_Code },

				OSCurrency = new Currency { Code = "GBP" },
				LocalCurrency = new Currency { Code = "AUD" },
				OSExGSTVATAmount = 120,
				LocalExVATAmount = 60,
				OSGSTVATAmount = 10,
				LocalVATAmount = 5,
				Ledger = LedgerTypes.AccountsPayable,

				PlaceOfSupply = placeOfSupply
			};

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
				Assert("Import result", result);
				Assert("logger.HasErrors", !logger.HasErrors);
				var createdInvoice = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true });
				AssertEquals(1, createdInvoice.Length);
				var savedInvoice = new BusinessObjectFactory().Load<InvoicingBase>(createdInvoice[0].PK);
				AssertType(typeof(TransactionPendingAllocation), savedInvoice);
				AssertEquals("JH", savedInvoice.AH_PlaceOfSupply);
				AssertEquals(PlaceOfSupplyTypes.State.Code, savedInvoice.AH_PlaceOfSupplyType);
				AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
			}

			universalFactory = new UniversalObjectFactory();
			serviceLogger = new ServiceTaskLogForTesting();
			logger = new XmlSessionTracker(serviceLogger);
			universalTransaction.Number = string.Format(CultureInfo.InvariantCulture, "{0}_2", universalTransaction.Number);
			var result2 = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import result", result2);
			Assert("logger.HasErrors", !logger.HasErrors);
			var createdInvoice2 = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoice2.Length);
			var savedInvoice2 = new BusinessObjectFactory().Load<InvoicingBase>(createdInvoice2[0].PK);
			AssertType(typeof(TransactionPendingAllocation), savedInvoice2);
			AssertEquals(string.Empty, savedInvoice2.AH_PlaceOfSupply);
			AssertEquals(string.Empty, savedInvoice2.AH_PlaceOfSupplyType);
			AssertStmNote(savedInvoice2.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
		}

		public void TestImportTransaction_HeaderPlaceOfSupplyWithNullLocation()
		{
			AssertImportTransactionPlaceOfSupply_NullLocationOrLocationType(null, PlaceOfSupplyTypes.State.Code);
		}

		public void TestImportTransaction_HeaderPlaceOfSupplyWithNullLocationType()
		{
			AssertImportTransactionPlaceOfSupply_NullLocationOrLocationType(null, null);
		}

		void AssertImportTransactionPlaceOfSupply_NullLocationOrLocationType(string headerLocation, string headerLocationType)
		{
			var address = TestObjectCreator.CreateAddress(TestObjectCreator.Creditor1, "123 Road Ave");
			var contact = TestObjectCreator.CreateContact(TestObjectCreator.Creditor1, "Mark", "ww@www.com");

			Factory.Save();

			var importer = new TransactionImporter();
			var universalFactory = new UniversalObjectFactory();
			var serviceLogger = new ServiceTaskLogForTesting();
			var logger = new XmlSessionTracker(serviceLogger);
			var message = new BusinessObjectFactory().New<EDIMessage>();

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.None),
				AddressShortCode = address.OA_Code,
				Address1 = address.OA_Address1,
				OrganizationCode = TestObjectCreator.Creditor1.OH_Code,
				Contact = contact.OC_ContactName,
				Email = contact.OC_Email
			};

			var placeOfSupply = new PlaceOfSupply();
			placeOfSupply.Location = headerLocation != null ? new CodeDescriptionPair5Char { Code = headerLocation } : null;
			placeOfSupply.LocationType = headerLocationType != null ? new UniversalCodeDescriptionPair { Code = headerLocationType } : null;

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OrganizationAddress = orgAddress,

				TransactionDate = ZDateTime.Today.AddDays(-3),
				DueDate = ZDateTime.Today.AddDays(2),
				PostDate = ZDateTime.Today.AddDays(-1),
				Number = "1234",
				CheckNumberOrPaymentRef = "Cheque#1",
				NumberOfSupportingDocuments = 5,
				Description = "Some text",
				Branch = new Branch { Code = TestObjectCreator.NonCurrentBranch.GB_Code },
				Department = new Department { Code = TestObjectCreator.NonCurrentDepartment.GE_Code },

				OSCurrency = new Currency { Code = "GBP" },
				LocalCurrency = new Currency { Code = "AUD" },
				OSExGSTVATAmount = 120,
				LocalExVATAmount = 60,
				OSGSTVATAmount = 10,
				LocalVATAmount = 5,
				Ledger = LedgerTypes.AccountsPayable,

				PlaceOfSupply = placeOfSupply
			};

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				var result = false;
				AssertNoExceptionThrown(() => result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory));
				Assert("Import result", result);
				Assert("logger.HasErrors", !logger.HasErrors);
				var createdInvoice = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true });
				AssertEquals(1, createdInvoice.Length);
				var savedInvoice = new BusinessObjectFactory().Load<InvoicingBase>(createdInvoice[0].PK);
				AssertType(typeof(TransactionPendingAllocation), savedInvoice);
				AssertEquals(headerLocation ?? string.Empty, savedInvoice.AH_PlaceOfSupply);
				AssertEquals(headerLocationType ?? string.Empty, savedInvoice.AH_PlaceOfSupplyType);
				AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
			}
		}

		[TestDate(2021, 2, 15)]
		public void TestImportAPTransactionWithEmptyTransactionDate_RegistryEqualToBLK()
		{
			var universalTransaction = SetupImportTransactionWithEmptyTransactionDate(TestObjectCreator.Debtor1);
			universalTransaction.Ledger = LedgerTypes.AccountsPayable;

			var originalMessage = new BusinessObjectFactory().New<EDIMessage>();
			AssertEquals("Message status", EDIMessage.Status.Queued, originalMessage.EM_Status);

			using (AccountingMasterFilesRegistry.Instance.InvoiceDateDefaultValue.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.InvoiceDateDefaultValueCodes.Blank))
			{
				var importer = new TransactionImporter();
				var universalFactory = new UniversalObjectFactory();
				var serviceLogger = new ServiceTaskLogForTesting();
				var logger = new XmlSessionTracker(serviceLogger);

				var result = importer.ImportTransaction(originalMessage, universalTransaction, logger, universalFactory);
				Assert("Import result", !result);
				Assert("logger.HasErrors", logger.HasErrors);
				AssertContains(@"Error - Transaction Date is invalid.", logger.ToString());
			}
		}

		[TestDate(2021, 2, 15)]
		public void TestImportAPTransactionWithEmptyTransactionDate_RegistryEqualToADD()
		{
			var universalTransaction = SetupImportTransactionWithEmptyTransactionDate(TestObjectCreator.Creditor1);
			universalTransaction.Ledger = LedgerTypes.AccountsPayable;

			var originalMessage = new BusinessObjectFactory().New<EDIMessage>();
			AssertEquals("Message status", EDIMessage.Status.Queued, originalMessage.EM_Status);

			using (AccountingMasterFilesRegistry.Instance.InvoiceDateDefaultValue.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.InvoiceDateDefaultValueCodes.CurrentDate))
			{
				var importer = new TransactionImporter();
				var universalFactory = new UniversalObjectFactory();
				var serviceLogger = new ServiceTaskLogForTesting();
				var logger = new XmlSessionTracker(serviceLogger);

				var result = importer.ImportTransaction(originalMessage, universalTransaction, logger, universalFactory);
				Assert("Import result", result);
				Assert("logger.HasErrors", !logger.HasErrors);

				var createdInvoice = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true });
				AssertEquals(1, createdInvoice.Length);
				var savedInvoice = new BusinessObjectFactory().Load<InvoicingBase>(createdInvoice[0].PK);
				AssertType(typeof(TransactionPendingAllocation), savedInvoice);
				AssertEquals(ZDateTime.Today, savedInvoice.AH_InvoiceDate);
				AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
			}
		}

		[TestDate(2021, 2, 15)]
		public void TestImportARTransactionWithEmptyTransactionDate()
		{
			var universalTransaction = SetupImportTransactionWithEmptyTransactionDate(TestObjectCreator.Debtor1);
			universalTransaction.Ledger = LedgerTypes.AccountsReceivable;
			universalTransaction.TransactionType = TransactionType.INV;

			var universalLine = new PostingJournal();
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.RevenueChargeCode.AC_Code };
			universalLine.OSAmount = 120;
			universalLine.TaxDate = ZDate.Today;

			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.PostingJournalCollection.Add(universalLine);

			universalTransaction.DataContext = DataContextFactory.New();
			universalTransaction.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ORP } } });
			var originalMessage = new BusinessObjectFactory().New<EDIMessage>();
			AssertEquals("Message status", EDIMessage.Status.Queued, originalMessage.EM_Status);

			using (AccountingMasterFilesRegistry.Instance.InvoiceDateDefaultValue.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.InvoiceDateDefaultValueCodes.Blank))
			{
				var importer = new TransactionImporter();
				var universalFactory = new UniversalObjectFactory();
				var serviceLogger = new ServiceTaskLogForTesting();
				var logger = new XmlSessionTracker(serviceLogger);

				var result = importer.ImportTransaction(originalMessage, universalTransaction, logger, universalFactory);
				Assert("Import result", result);
				Assert("logger.HasErrors", !logger.HasErrors);

				var createdInvoice = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true });
				AssertEquals(1, createdInvoice.Length);
				var savedInvoice = new BusinessObjectFactory().Load<InvoicingBase>(createdInvoice[0].PK);
				AssertType(typeof(ARInvoice), savedInvoice);
				AssertEquals(ZDateTime.Today, savedInvoice.AH_InvoiceDate);
				AssertEquals(0, savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Length);
			}
		}

		TransactionInfo SetupImportTransactionWithEmptyTransactionDate(OrgHeader org)
		{
			var address = TestObjectCreator.CreateAddress(org, "123 Road Ave");
			var contact = TestObjectCreator.CreateContact(org, "Mark", "ww@www.com");

			Factory.Save();

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.None),
				AddressShortCode = address.OA_Code,
				Address1 = address.OA_Address1,
				OrganizationCode = org.OH_Code,
				Contact = contact.OC_ContactName,
				Email = contact.OC_Email
			};
			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OrganizationAddress = orgAddress,

				DueDate = ZDateTime.Today,
				PostDate = ZDateTime.Today,
				Number = "1234",
				CheckNumberOrPaymentRef = "Cheque#1",
				NumberOfSupportingDocuments = 5,
				Description = "Some text",
				Branch = new Branch { Code = TestObjectCreator.NonCurrentBranch.GB_Code },
				Department = new Department { Code = TestObjectCreator.NonCurrentDepartment.GE_Code },

				OSCurrency = new Currency { Code = "AUD" },
				LocalCurrency = new Currency { Code = "AUD" },
				OSExGSTVATAmount = 120,
				LocalExVATAmount = 120,
				OSGSTVATAmount = 10,
				LocalVATAmount = 10,
			};

			return universalTransaction;
		}

		#region TestImportUniversalTransactionFromXml

		public void TestImportUniversalAPTransactionFromXml()
		{
			AssertImportUniversalTransactionFromXml(UniversalTransactionXML, false, "ABCFRESYD", LedgerTypes.AccountsPayable, "ABCFRESYD");
		}

		public void TestImportUniversalARTransactionFromXml()
		{
			AssertImportUniversalTransactionFromXml(UniversalARTransactionXML, false, "ABCFRESYD", LedgerTypes.AccountsReceivable, "ABCFRESYD");
		}

		public void TestImportUniversalARTransactionFromXml_WithPlaceOfSupply()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				var transaction = AssertImportUniversalTransactionFromXml(UniversalARTransactionXMLWithPlaceOfSupply, false, "ABCFRESYD", LedgerTypes.AccountsReceivable, "ABCFRESYD", null);
				AssertEquals("AR", transaction.PlaceOfSupply.Location.Code);
				AssertEquals(PlaceOfSupplyTypes.State.Code, transaction.PlaceOfSupply.LocationType.Code);
				AssertEquals("AR", transaction.PostingJournalCollection[0].PlaceOfSupply.Location.Code);
				AssertEquals(PlaceOfSupplyTypes.State.Code, transaction.PostingJournalCollection[0].PlaceOfSupply.LocationType.Code);
			}
		}

		public void TestImportUniversalAPTransactionFromXml_CreditorMatching()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			{
				TestObjectCreator.CreditorTR.OH_FullName = "Test Organization";
				TestObjectCreator.CreditorTR.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "9000068418", Constants.CountryCodes.Turkey);
				TestObjectCreator.CreditorTR.CompanyData.SetARTaxApplicable(true);
				Factory.Save();

				var importer = new TransactionImporter();
				var transaction = importer.ImportUniversalTransactionFromXml(UniversalAPTransactionXML_ForCreditorMapping, Factory, false).Item1 as TransactionInfo;
				var message = new BusinessObjectFactory().New<EDIMessage>();
				message.EM_GB = testObjectCreator.NonCurrentBranch.PK;
				message.EM_GE = testObjectCreator.NonCurrentDepartment.PK;
				message.EM_MessageText = UniversalAPTransactionXML_ForCreditorMapping;

				var universalFactory = new UniversalObjectFactory();
				var serviceLogger = new ServiceTaskLogForTesting();
				var logger = new XmlSessionTracker(serviceLogger);

				var result = importer.ImportTransaction(message, transaction, logger, universalFactory);
				universalFactory.SaveForTesting();
				Assert("Logger without error", !logger.HasErrors);
				Assert("Import result", result);

				var createdInvoices = universalFactory.BOFactory.Load<TransactionPendingAllocation>(new ZQuery { FetchOnlyFromLocalCache = true });
				AssertEquals(1, createdInvoices.Length);

				var newFactory = new BusinessObjectFactory();
				var savedInvoice = newFactory.Load<TransactionPendingAllocation>(createdInvoices[0].PK);
				AssertEquals("Matched OrgHeader: ", Guid.Empty, savedInvoice.AH_OH);
				AssertContains(@"Warning - Matching 'OFC':- No match found for '[]'.
Successfully saved Unallocated Transaction.", logger.ToString());
				AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);

				var registryValue = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.OrganizationMatcherVATRegistrationNumberContextTypeList());
				registryValue.Set(OrganisationMatchingByVATRegistrationNumberContexts.Codes.Payables, true);

				using (AccountingMasterFilesRegistry.Instance.UseVATRegistrationNumberAsOrganizationMatchingCriteria.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
				{
					result = importer.ImportTransaction(message, transaction, logger, universalFactory);
					universalFactory.SaveForTesting();
					Assert("Logger without error", !logger.HasErrors);
					Assert("Import result", result);

					createdInvoices = universalFactory.BOFactory.Load<TransactionPendingAllocation>(new ZQuery { FetchOnlyFromLocalCache = true }).Where(x => x.AH_OH.IsValid).ToArray();
					AssertEquals(1, createdInvoices.Length);

					newFactory = new BusinessObjectFactory();
					savedInvoice = newFactory.Load<TransactionPendingAllocation>(createdInvoices[0].PK);
					AssertEquals("Matched OrgHeader: ", TestObjectCreator.CreditorTR.PK, savedInvoice.AH_OH);
					AssertContains(@"Matching 'OFC':- Matched to address '184 Bourke Road' on 'ZCreditorTR' by registration detail (Country/Region='TR', Type='VAT', Number='9000068418').
Successfully saved Unallocated Transaction.", logger.ToString());
					AssertStmNote(savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Single(), logger);
				}
			}
		}

		public void TestImportUniversalARTransactionFromXml_DebtorMatching()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			{
				TestObjectCreator.DebtorTR.CompanyData.SetARTaxApplicable(true);
				TestObjectCreator.DebtorTR.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "9000068418", Constants.CountryCodes.Turkey);
				Factory.Save();

				var importer = new TransactionImporter();
				var message = new BusinessObjectFactory().New<EDIMessage>();
				message.EM_MessageText = UniversalAPTransactionXML_ForCreditorMapping;
				var universalTransaction = importer.ImportUniversalTransactionFromXml(UniversalAPTransactionXML_ForDebtorMapping.Replace("{Number}", "0001").Replace("{GenericCharge}", TestObjectCreator.RevenueChargeCode.AC_Code), Factory, false).Item1 as TransactionInfo;
				var universalFactory = new UniversalObjectFactory();
				var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());

				var result = false;
				AssertExceptionThrown<MessageProcessingBusinessFailureException>("Validation errors", @"Import failed because transaction has validation errors:
Error - Terms: Please enter a Terms.
Error - Address Override: Please enter an Account.
Error - Account: Please enter an Account.
", () => result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory));

				universalFactory.SaveForTesting();
				Assert("Logger without error", !logger.HasErrors);
				Assert("Import result", !result);

				var createdInvoices = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true });
				AssertEquals(1, createdInvoices.Length);

				var newFactory = new BusinessObjectFactory();
				var savedInvoice = newFactory.Load<TransactionPendingAllocation>(createdInvoices[0].PK);
				AssertEquals("Matched OrgHeader: ", Guid.Empty, savedInvoice.AH_OH);
				AssertEquals("Warning - Matching 'OFC':- No match found for '[]'.", logger.ToString());
				AssertEquals(0, savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Length);

				var registryValue = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.OrganizationMatcherVATRegistrationNumberContextTypeList());
				registryValue.Set(OrganisationMatchingByVATRegistrationNumberContexts.Codes.Receivables, true);

				using (AccountingMasterFilesRegistry.Instance.UseVATRegistrationNumberAsOrganizationMatchingCriteria.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
				{
					var universalTransaction2 = importer.ImportUniversalTransactionFromXml(UniversalAPTransactionXML_ForDebtorMapping.Replace("{Number}", "0002").Replace("{GenericCharge}", TestObjectCreator.RevenueChargeCode.AC_Code), Factory, false).Item1 as TransactionInfo;
					logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
					result = importer.ImportTransaction(message, universalTransaction2, logger, universalFactory);
					universalFactory.SaveForTesting();
					Assert("Logger without error", !logger.HasErrors);
					Assert("Import result", result);

					createdInvoices = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true }).Where(x => x.AH_OH.IsValid).ToArray();
					AssertEquals(1, createdInvoices.Length);

					newFactory = new BusinessObjectFactory();
					savedInvoice = newFactory.Load<TransactionPendingAllocation>(createdInvoices[0].PK);
					AssertEquals("Matched OrgHeader: ", TestObjectCreator.DebtorTR.PK, savedInvoice.AH_OH);
					AssertContains(@"Matching 'OFC':- Matched to address '184 Bourke Road' on 'ZDebtorTR' by registration detail (Country/Region='TR', Type='VAT', Number='9000068418').
Successfully saved Accounts Receivable Invoice.", logger.ToString());
					AssertEquals(0, savedInvoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description).Length);
				}
			}
		}

		public void TestImportTransaction_DataTypeNotSupportShipmentShouldNotImport()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			{
				TestObjectCreator.DebtorTR.CompanyData.SetARTaxApplicable(true);
				TestObjectCreator.DebtorTR.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "9000068418", Constants.CountryCodes.Turkey);
				Factory.Save();

				var importer = new TransactionImporter();
				var message = new BusinessObjectFactory().New<EDIMessage>();
				message.EM_MessageText = UniversalAPTransactionXML_ForSupportShipmentDataType;
				var universalTransaction = importer.ImportUniversalTransactionFromXml(UniversalAPTransactionXML_ForSupportShipmentDataType, Factory, false).Item1 as TransactionInfo;
				var universalFactory = new UniversalObjectFactory();
				var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());

				importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
				AssertEquals("Should have Error message", "Error - Data Type 'Staff' import is not supported.\r\nError - Data Type 'InvaildDataSourceType' is not valid ", logger.ToString());
			}
		}

		enum ComplianceTestType
		{
			Expired,
			SparseBook
		}

		[TestDate(2023, 02, 21)]
		public void TestImportUniversalARTransactionFromXml_ComplianceSequenceValidation()
		{
			var registry = AccountingMasterFilesRegistry.Instance;
			var currComp = GlbCompany.CurrentCompany;

			using (currComp.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				var (invoices, collection, complianceSequence) = SetupDataForComplianceSequenceValidation(currComp);

				using (registry.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(currComp.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
				using (registry.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(currComp.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(currComp.PK.ToGuid(), Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.PostDate.Code))
				{
					var importer = new TransactionImporter();
					var message = new BusinessObjectFactory().New<EDIMessage>();
					var preparedXml = PrepareXmlForItalianComplianceValidation();

					var universalTransaction = importer.ImportUniversalTransactionFromXml(preparedXml, Factory, false).Item1 as TransactionInfo;
					var universalFactory = new UniversalObjectFactory();
					var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());

					var result = false;
					AssertNoExceptionThrown(() => result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory));
					Assert("Import successfull", result);

					universalFactory.SaveForTesting();
					var createdInvoices = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true }).Where(x => x.AH_OH.IsValid).ToArray();
					AssertEquals("Invoice imported", 1, createdInvoices.Length);
					AssertEquals("SubType is ARI", "ARI", createdInvoices[0].AH_ComplianceSubType);
					AssertEquals("Transaction Ref. is correct", "ARI-000000025", createdInvoices[0].AH_TransactionReference);

					AssertComplianceError(ComplianceTestType.Expired);
					AssertComplianceError(ComplianceTestType.SparseBook);

					void AssertComplianceError(ComplianceTestType testType)
					{
						var today = ZDate.Today;
						var expectedError = ZString.Empty;

						const string expiredMessageError = @"Import failed because transaction has validation errors:
Please check your Compliance Invoice Book Setups. 
 A Compliance Invoice Book for the relevant Compliance Sub-Type, Branch, Active Status and Start / Expiry Date does not exist.
";

						const string sparseBookMessageError = @"Import failed because transaction has validation errors:
Compliance Numbers cannot be allocated.
 There is some transaction with the same Compliance Sub Type ARI in earlier Post Date and Compliance Number empty.
 Please allocate Compliance Number to all transactions with Post Date < 21-Feb-23.
";
						ZExceptionReporting.ProcessWithConcurrencyHandling(() =>
						{
							switch (testType)
							{
								case ComplianceTestType.Expired:
									complianceSequence.XD_ExpiryDate = today.AddMonths(-6);
									invoices[0].AH_TransactionReference = (ZString)"ARI-000000022";
									expectedError = expiredMessageError;
									break;
								case ComplianceTestType.SparseBook:
									complianceSequence.XD_ExpiryDate = ZDate.Empty;
									invoices[0].AH_TransactionReference = ZString.Empty;
									expectedError = sparseBookMessageError;
									break;
							}

							Factory.Save();
						}, null);

						universalTransaction = importer.ImportUniversalTransactionFromXml(preparedXml.Replace("{Number}", "0002"), Factory, false).Item1 as TransactionInfo;
						result = false;
						AssertExceptionThrown<MessageProcessingBusinessFailureException>("Expected Compliance errors", expectedError, () => result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory));
						Assert("Import failed", !result);
					}
				}
			}
		}

		[TestDate(2023, 02, 21)]
		public void TestImportUniversalARTransactionFromXml_ComplianceSequenceValidation_EarlierLastDateUsed()
		{
			var registry = AccountingMasterFilesRegistry.Instance;
			var currComp = GlbCompany.CurrentCompany;

			using (currComp.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				const string subType = "ARI";
				var today = ZDate.Today;
				var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				sequence.XD_SequenceClass = subType;
				sequence.XD_Code = "ARI1";
				sequence.XD_StartNumber = 1;
				sequence.XD_EndNumber = 99;
				sequence.XD_NextNumber = 25;
				sequence.XD_MaximumNumberDigits = 9;
				sequence.XD_GC_Company = currComp.PK;
				sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
				sequence.XD_Prefix = "ARI-";

				Factory.Save();

				var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
				var config = collection.AddNew();
				config.Country = currComp.Country.Code;
				config.SubType = subType;
				config.LedgerType = LedgerTypes.AccountsReceivable;
				config.InvoiceType = TransactionTypes.Invoice;
				config.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAnAmountOfTax;
				config.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				config.OriginalRule = OriginalRuleCodes.AllTransactions;

				var org = Factory.New<OrgHeader>();
				org.OH_Code = "ORGTEST";
				org.OH_IsDebtor = true;
				org.CompanyData.SetARTaxApplicable(true);

				Factory.Save();

				var invoiceWithEarlierAllocationDate = TestObjectCreator.CreateInvoice(typeof(ARInvoice), invoiceDate: ZDateTime.Now);
				invoiceWithEarlierAllocationDate.AH_Ledger = LedgerTypes.AccountsReceivable;
				invoiceWithEarlierAllocationDate.AH_GC = currComp.PK;
				invoiceWithEarlierAllocationDate.AH_GB = GlbBranch.CurrentBranch.PK;
				invoiceWithEarlierAllocationDate.AH_GE = GlbDepartment.CurrentDepartment.PK;
				invoiceWithEarlierAllocationDate.AH_XD_ComplianceBook = sequence.PK;
				invoiceWithEarlierAllocationDate.AH_TransactionReference = "ARI-000000024";
				invoiceWithEarlierAllocationDate.AH_ComplianceSubType = "ARI";
				invoiceWithEarlierAllocationDate.AH_PostDate = ZDate.Today.AddDays(15);

				Factory.Save();

				using (registry.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(currComp.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
				using (registry.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(currComp.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(currComp.PK.ToGuid(), Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.PostDate.Code))
				{
					var importer = new TransactionImporter();
					var message = new BusinessObjectFactory().New<EDIMessage>();
					var preparedXml = PrepareXmlForItalianComplianceValidation();

					var universalTransaction = importer.ImportUniversalTransactionFromXml(preparedXml, Factory, false).Item1 as TransactionInfo;
					var universalFactory = new UniversalObjectFactory();
					var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());

					const string earlierLastDateUsedMessageError = @"Import failed because transaction has validation errors:
Compliance Numbers cannot be allocated.
 Last posted transaction with the same Compliance Sub Type ARI has Post Date = 08-Mar-23, that is greater than the current one(s).
";

					universalTransaction = importer.ImportUniversalTransactionFromXml(preparedXml.Replace("{Number}", "0002"), Factory, false).Item1 as TransactionInfo;
					var result = false;
					AssertExceptionThrown<MessageProcessingBusinessFailureException>("Expected Compliance errors", earlierLastDateUsedMessageError, () => result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory));
					Assert("Import failed", !result);
				}
			}
		}

		string PrepareXmlForItalianComplianceValidation()
		{
			var orgAddressNode = @"
<OrganizationAddress>
<OrganizationCode>ORGTEST</OrganizationCode>
";
			var osAmountNode = @"
<OSAmount>130</OSAmount>
<LocalGSTVATAmount>10.00</LocalGSTVATAmount>
<LocalTotalAmount>140.00</LocalTotalAmount>
";
			var preparedXml = UniversalAPTransactionXML_ForDebtorMapping
				.Replace("{Number}", "0001")
				.Replace("{GenericCharge}", TestObjectCreator.RevenueChargeCode.AC_Code)
				.Replace("<OrganizationAddress>", orgAddressNode)
				.Replace("<OSAmount>130</OSAmount>", osAmountNode)
				.Replace("<Code>TR</Code>", "<Code>IT</Code>")
				.Replace("Turkey", "Italy");

			return preparedXml;
		}

		(InvoicingBase[], ComplianceSubTypeAttributionRuleConfigurationCollection, AccComplianceSequence) SetupDataForComplianceSequenceValidation(GlbCompany currComp)
		{
			const string subType = "ARI";
			var today = ZDate.Today;
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_SequenceClass = subType;
			sequence.XD_Code = "ARI1";
			sequence.XD_StartNumber = 1;
			sequence.XD_EndNumber = 99;
			sequence.XD_NextNumber = 25;
			sequence.XD_MaximumNumberDigits = 9;
			sequence.XD_GC_Company = currComp.PK;
			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_Prefix = "ARI-";

			Factory.Save();

			var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			var config = collection.AddNew();
			config.Country = currComp.Country.Code;
			config.SubType = subType;
			config.LedgerType = LedgerTypes.AccountsReceivable;
			config.InvoiceType = TransactionTypes.Invoice;
			config.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAnAmountOfTax;
			config.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			config.OriginalRule = OriginalRuleCodes.AllTransactions;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ORGTEST";
			org.OH_IsDebtor = true;
			org.CompanyData.SetARTaxApplicable(true);

			Factory.Save();

			var invoices = new InvoicingBase[3];
			invoices[0] = CreateInvoice("ARI-000000022", today.AddDays(-8));
			invoices[1] = CreateInvoice("ARI-000000023", today.AddDays(-7));
			invoices[2] = CreateInvoice("ARI-000000024", today.AddDays(-6));

			Factory.Save();

			return (invoices, collection, sequence);

			InvoicingBase CreateInvoice(ZString transRef, ZDate allocationDate)
			{
				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), invoiceDate: ZDateTime.Now);
				invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
				invoice.AH_GC = currComp.PK;
				invoice.AH_GB = GlbBranch.CurrentBranch.PK;
				invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
				invoice.AH_XD_ComplianceBook = sequence.PK;
				invoice.AH_TransactionReference = transRef;
				invoice.AH_ComplianceSubType = subType;
				invoice.AH_PostDate = allocationDate;
				invoice.AH_InvoiceDate = allocationDate;
				return invoice;
			}
		}

		public void TestImportUniversalAPTransactionFromXml_CrossLedgerImportWithoutDataProvider()
		{
			AssertImportUniversalTransactionFromXml(UniversalTransactionXML, true, null, LedgerTypes.AccountsPayable);
		}

		public void TestImportUniversalAPTransactionFromXml_CrossLedgerImportWithDataProviderWithMapping()
		{
			var currentCompnay = GlbCompany.GetCurrentCompany(Factory);
			OrgPatternMatchOverride patternMatchOverride = AddMatchingRuleForOrganization(currentCompnay.OrgProxy, "SOMEPROVIDER", TestObjectCreator.Creditor1);
			Factory.Save();

			var xml = UniversalTransactionXML.Replace("<TransactionInfo>",
@"<TransactionInfo>
<DataContext>
	  <DataProvider>SOMEPROVIDER</DataProvider>
</DataContext>");
			AssertImportUniversalTransactionFromXml(xml, true, "ZCreditor1", LedgerTypes.AccountsPayable, "SOMEPROVIDER", "ZCreditor1");
		}

		public void TestImportUniversalTransactionFromXml_CrossLedgerImportWithDataProviderWithoutMapping_AnotherCompanySource()
		{
			var xml = UniversalTransactionXML.Replace("<TransactionInfo>",
@"<TransactionInfo>
<DataContext>
	  <DataProvider>SOMEPROVIDER</DataProvider>
</DataContext>");
			AssertImportUniversalTransactionFromXml(xml, true, "SOMEPROVIDER", LedgerTypes.AccountsPayable, "SOMEPROVIDER");
		}

		public void TestImportUniversalTransactionFromXml_CrossLedgerImportWithDataProviderWithoutMapping_TheSameCompanySource()
		{
			var xml = UniversalTransactionXML.Replace("<TransactionInfo>",
@"<TransactionInfo>
<DataContext>
	  <DataProvider>SOMEPROVIDER</DataProvider>
	  <Company>
		<Code>DAU</Code>
	  </Company>
	  <EnterpriseID>EDI</EnterpriseID>
	  <ServerID>DAT</ServerID>
</DataContext>");
			AssertImportUniversalTransactionFromXml(xml, true, "SOMEPROVIDER", LedgerTypes.AccountsPayable, "SOMEPROVIDER");
		}

		public void TestImportUniversalTransaction_WithValidationError()
		{
			var importer = new TransactionImporter();
			var message = new BusinessObjectFactory().New<EDIMessage>();
			var universalTransactionWithoutOrgAddress = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), Country = Country.New(RefCountry.LoadFromCountryCode(Factory, "NZ")) },
				TransactionDate = ZDateTime.Today.AddDays(-3),
				DocumentReceivedDate = ZDateTime.Today.AddDays(1),
				DueDate = ZDateTime.Today.AddDays(2),
				PostDate = ZDateTime.Today.AddDays(-1),
				Number = "1234",
				CheckNumberOrPaymentRef = "Cheque#1",
				NumberOfSupportingDocuments = 5,
				Description = "Some text",
				Branch = new Branch { Code = TestObjectCreator.NonCurrentBranch.GB_Code },
				Department = new Department { Code = TestObjectCreator.NonCurrentDepartment.GE_Code },

				LocalCurrency = new Currency { Code = "AUD" },
				OSExGSTVATAmount = 120,
				LocalExVATAmount = 60,
				OSGSTVATAmount = 10,
				LocalVATAmount = 5
			};

			universalTransactionWithoutOrgAddress.Ledger = LedgerTypes.AccountsPayable;
			var universalFactory = new UniversalObjectFactory();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var result = importer.ImportTransaction(message, universalTransactionWithoutOrgAddress, logger, universalFactory);
			Assert("Import result", result);

			var createdTransactionPendingAllocationInvoices = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdTransactionPendingAllocationInvoices.Length);
			Assert(createdTransactionPendingAllocationInvoices[0] is TransactionPendingAllocation);
			AssertEquals(ZGuid.Empty, createdTransactionPendingAllocationInvoices[0].AH_OH);
			createdTransactionPendingAllocationInvoices[0].RunPreSaveValidation();
			Assert("Precondition: invoice has errors", createdTransactionPendingAllocationInvoices[0].HasErrors);
			Assert("logger.HasErrors", !logger.HasErrors);

			universalTransactionWithoutOrgAddress.Ledger = LedgerTypes.AccountsReceivable;
			universalTransactionWithoutOrgAddress.TransactionType = TransactionType.INV;
			universalTransactionWithoutOrgAddress.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ORP } } });
			Assert("Precondition: HasRecipientRole", universalTransactionWithoutOrgAddress.HasRecipientRole(RecipientRoleType.ORP));

			universalFactory = new UniversalObjectFactory();
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			AssertExceptionThrown<MessageProcessingBusinessFailureException>(() => result = importer.ImportTransaction(message, universalTransactionWithoutOrgAddress, logger, universalFactory));
		}

		public TransactionInfo AssertImportUniversalTransactionFromXml(string xml, bool isCrossLedgerImport, string expectedOrgCode, string ledger, string expectedOrgCodeSource = null, string expectedOrgCodeMapped = null)
		{
			var importer = new TransactionImporter();
			var transaction = importer.ImportUniversalTransactionFromXml(xml, Factory, isCrossLedgerImport).Item1 as TransactionInfo;

			AssertNotNull("transaction", transaction);
			AssertEquals("Branch.Code", "BER", transaction.Branch.Code);
			AssertEquals("Department.Code", "BRN", transaction.Department.Code);
			AssertEquals("Description", string.Format("{0} INVOICE", ledger), transaction.Description);
			ZDateTime date;
			ZDateTime.TryParseISO8601Date("2015-05-01T19:09:00", out date);
			AssertEquals("DueDate", date, transaction.DueDate);
			ZDateTime.TryParseISO8601Date("2015-04-30T19:09:00", out date);
			AssertEquals("PostDate", date, transaction.PostDate);
			ZDateTime.TryParseISO8601Date("2015-04-28T19:09:00", out date);
			AssertEquals("TransactionDate", date, transaction.TransactionDate);
			ZDateTime.TryParseISO8601Date("2020-02-13T11:11:00", out date);
			AssertEquals("DocumentReceivedDate", date, transaction.DocumentReceivedDate);
			AssertEquals("Ledger", ledger, transaction.Ledger);
			AssertEquals("Number", "11112222", transaction.Number);
			AssertEquals("NumberOfSupportingDocuments", 2, transaction.NumberOfSupportingDocuments);
			AssertEquals("OrganizationAddress", expectedOrgCode, transaction.OrganizationAddress.OrganizationCode);
			if (expectedOrgCode != null)
			{
				AssertEquals("OrganizationAddress SourceValue", expectedOrgCodeSource, transaction.OrganizationAddress.OrganizationCode.Value.SourceValue);
				AssertEquals("OrganizationAddress MappedValue", expectedOrgCodeMapped, transaction.OrganizationAddress.OrganizationCode.Value.MappedValue);
			}
			AssertEquals("OSCurrency", "USD", transaction.OSCurrency.Code);
			AssertEquals("LocalExVATAmount", -60m, transaction.LocalExVATAmount);
			AssertEquals("OSExGSTVATAmount", -120m, transaction.OSExGSTVATAmount);

			AssertEquals("PostingJournalCollection", 1, transaction.PostingJournalCollection.Count);
			var line = transaction.PostingJournalCollection[0];
			AssertEquals("line.Branch", "SYD", line.Branch.Code);
			AssertEquals("line.Department", "BRN", line.Department.Code);
			AssertEquals("line.Description", "FREIGHT REVENUE ACTUAL", line.Description);
			AssertEquals("line.GLAccount", "1010.10.10", line.GLAccount.AccountCode);
			AssertEquals("line.IsFinalCharge", true, line.IsFinalCharge);
			AssertEquals("line.LocalAmount", -60m, line.LocalAmount);
			AssertEquals("line.OSAmount", -120m, line.OSAmount);
			AssertEquals("line.OSCurrency", "USD", line.OSCurrency.Code);
			AssertEquals("line.Sequence", 2, line.Sequence);

			return transaction;
		}

		#endregion

		[TestDate(2021, 2, 15)]
		public void TestImportARTransactionLinesWithoutTaxDate()
		{
			TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.GST1.PK;
			TestObjectCreator.Debtor1.OH_RL_NKClosestPort = "AUSYD";
			Factory.Save();

			var importer = new TransactionImporter();
			var invoice = new BusinessObjectFactory().New<ARInvoice>();
			invoice.AH_OH = TestObjectCreator.Debtor1.PK;
			invoice.SubmittedFromInvoicingForm = true;

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), OrganizationCode = GlbCompany.CurrentCompany.OrgProxy.OH_Code, Country = Country.New(GlbCompany.CurrentCompany.Country) };
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			var universalLine = new PostingJournal();
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.OSAmount = -10;
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var universalTransactionXml = universalTransaction.Serialize();

			importer.ImportTransactionLines(universalTransactionXml, invoice, false);

			AssertEquals(1, invoice.Lines.Count);
			var line = invoice.Lines[0];
			AssertEquals(ZDate.Today, line.AL_TaxDate);
			invoice.ReleaseAllMutexOnInvoice();
		}

		[TestDate(2021, 2, 15)]
		public void TestImportARTransactionLinesWithTaxDate()
		{
			TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.GST1.PK;
			TestObjectCreator.Debtor1.OH_RL_NKClosestPort = "AUSYD";
			Factory.Save();

			var importer = new TransactionImporter();
			var invoice = new BusinessObjectFactory().New<ARInvoice>();
			invoice.AH_OH = TestObjectCreator.Debtor1.PK;
			invoice.SubmittedFromInvoicingForm = true;

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), OrganizationCode = GlbCompany.CurrentCompany.OrgProxy.OH_Code, Country = Country.New(GlbCompany.CurrentCompany.Country) };
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			var universalLine = new PostingJournal();
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.OSAmount = -10;
			universalLine.TaxDate = ZDate.Today.AddDays(2);
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var universalTransactionXml = universalTransaction.Serialize();

			importer.ImportTransactionLines(universalTransactionXml, invoice, false);

			AssertEquals(1, invoice.Lines.Count);
			var line = invoice.Lines[0];

			AssertEquals("invoice line's tax date should be copied from the universal line.", new ZDate(2021, 2, 17), line.AL_TaxDate);
			invoice.ReleaseAllMutexOnInvoice();
		}

		[TestDate(2021, 2, 15)]
		public void TestImportARTransactionWithEarliestOfInvoiceOrTaxDate()
		{
			AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvoicePostingExchangeRateOption.EarliestOfInvoiceOrTaxDate.Code);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.GBP, Constants.ExchangeRateTypes.Code.SellRate, 1.5m, new DateTime(2021, 2, 1), new DateTime(2021, 2, 10));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.GBP, Constants.ExchangeRateTypes.Code.SellRate, 2.5m, new DateTime(2021, 2, 11), new DateTime(2021, 2, 15));
			Factory.Save();

			var orgAddress = TestObjectCreator.CreateAddress(TestObjectCreator.Debtor1, "123 Road Ave");
			var orgContact = TestObjectCreator.CreateContact(TestObjectCreator.Debtor1, "Mark", "ww@www.com");

			var organizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.None),
				AddressShortCode = orgAddress.OA_Code,
				Address1 = orgAddress.OA_Address1,
				OrganizationCode = TestObjectCreator.Debtor1.OH_Code,
				Contact = orgContact.OC_ContactName,
				Email = orgContact.OC_Email
			};

			var importer = new TransactionImporter();
			var message = new BusinessObjectFactory().New<EDIMessage>();
			var universalFactory = new UniversalObjectFactory();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());

			var universalTransaction = CreateTransactionWithLine(organizationAddress, false, false);
			universalTransaction.Ledger = LedgerTypes.AccountsReceivable;
			universalTransaction.TransactionType = TransactionType.INV;
			universalTransaction.ExchangeRate = 0;
			var universalLine = universalTransaction.PostingJournalCollection[0];
			universalLine.TaxDate = new ZDate(2021, 2, 8);

			universalTransaction.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ORP } } });

			var result = importer.ImportTransaction(message, universalTransaction, logger, universalFactory);
			Assert("Import succeed", result);
			Assert("No error in logger", !logger.HasErrors);

			var createdInvoices = universalFactory.BOFactory.Load<TransactionPendingAllocation>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(1, createdInvoices.Length);
			var savedInvoice = new BusinessObjectFactory().Load<TransactionPendingAllocation>(createdInvoices[0].PK);
			AssertEquals("when InvoicePostingExchangeRateOptionAR is EIT we expect it to use the earliest date to calculate exRate which here is the line tax date", 1.5m, savedInvoice.AH_ExchangeRate);
		}

		[TestDate(2021, 2, 15)]
		public void TestImportAPTransactionLinesWithTaxDate_CostSourceExists_UseImportedDate()
		{
			TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.GST1.PK;
			TestObjectCreator.Creditor1.OH_RL_NKClosestPort = "AUSYD";

			var consol1 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			consol1.JK_MasterBillNum = "MasterBill1";
			var shipment1 = TestObjectCreator.CreateShipment("S001", consol1);
			Factory.Save();

			var importer = new TransactionImporter();
			var invoice = new BusinessObjectFactory().New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;
			invoice.SubmittedFromInvoicingForm = true;
			invoice.AH_InvoiceDate = ZDate.Today.AddDays(-2);

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), OrganizationCode = GlbCompany.CurrentCompany.OrgProxy.OH_Code, Country = Country.New(GlbCompany.CurrentCompany.Country) };
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			var universalLine = new PostingJournal();
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.OSAmount = -10;
			universalLine.TaxDate = ZDate.Today.AddDays(2);
			universalLine.Job = new EntityReference { Key = "S001", Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			universalLine.CostSource = new EntityReference { Key = "C001", Type = nameof(DataContextType.ForwardingConsol) };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S001");
			universalShipment.DataContext.AddDataTarget(DataContextType.ForwardingShipment, "S001");
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var universalShipment2 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment2.DataContext = DataContextFactory.New();
			universalShipment2.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C001");
			universalShipment2.DataContext.AddDataTarget(DataContextType.ForwardingConsol, "C001");
			universalTransaction.ShipmentCollection.Add(universalShipment2);

			var universalTransactionXml = universalTransaction.Serialize();

			importer.ImportTransactionLines(universalTransactionXml, invoice, false);

			AssertEquals(1, invoice.Lines.Count);
			var line = invoice.Lines[0];

			AssertEquals("invoice line's tax date should be copied from the universal line.", new ZDate(2021, 2, 17), line.AL_TaxDate);

			invoice.ReleaseAllMutexOnInvoice();
		}

		[TestDate(2021, 2, 15)]
		public void TestImportAPTransactionLinesWithoutTaxDate_CostSourceExists_UseFCNConfig()
		{
			TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.GST1.PK;
			TestObjectCreator.Creditor1.OH_RL_NKClosestPort = "AUSYD";

			var consol1 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			consol1.JK_MasterBillNum = "MasterBill1";
			var shipment1 = TestObjectCreator.CreateShipment("S001", consol1);
			shipment1.JS_HouseBill = "HouseBill1";
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			Factory.Save();

			var importer = new TransactionImporter();
			var invoice = new BusinessObjectFactory().New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;
			invoice.SubmittedFromInvoicingForm = true;
			invoice.AH_InvoiceDate = ZDate.Today.AddDays(-2);

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), OrganizationCode = GlbCompany.CurrentCompany.OrgProxy.OH_Code, Country = Country.New(GlbCompany.CurrentCompany.Country) };
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			var universalLine = new PostingJournal();
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.OSAmount = -10;
			universalLine.Job = new EntityReference { Key = "S001", Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			universalLine.CostSource = new EntityReference { Key = "C001", Type = nameof(DataContextType.ForwardingConsol) };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S001");
			universalShipment.DataContext.AddDataTarget(DataContextType.ForwardingShipment, "S001");
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var universalShipment2 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment2.DataContext = DataContextFactory.New();
			universalShipment2.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C001");
			universalShipment2.DataContext.AddDataTarget(DataContextType.ForwardingConsol, "C001");
			universalTransaction.ShipmentCollection.Add(universalShipment2);

			var universalTransactionXml = universalTransaction.Serialize();

			var collection = new TaxDateDefaultingOptionCollection();
			var taxDateOption = collection.AddNew();
			taxDateOption.JobType = "FCN";
			taxDateOption.DirectionCode = "ALL";
			taxDateOption.Mode = "ALL";
			taxDateOption.Ledger = "AP";
			taxDateOption.TaxDateOption = TaxDateDefaultingOption.Code.InvoiceDate;
			using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			{
				importer.ImportTransactionLines(universalTransactionXml, invoice, false);
			}

			AssertEquals(1, invoice.Lines.Count);
			var line = invoice.Lines[0];

			AssertEquals("invoice line's tax date should be copied from the invoice date based on registry setting for FCN.", new ZDate(2021, 2, 13), line.AL_TaxDate);

			invoice.ReleaseAllMutexOnInvoice();
		}

		[TestDate(2021, 2, 15)]
		public void TestImportAPTransactionLinesWithoutTaxDate_CostSourceNotExists_UseJobConfig()
		{
			TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.GST1.PK;
			TestObjectCreator.Creditor1.OH_RL_NKClosestPort = "AUSYD";

			var consol1 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			consol1.JK_MasterBillNum = "MasterBill1";
			var shipment1 = TestObjectCreator.CreateShipment("S001", consol1);
			shipment1.JS_HouseBill = "HouseBill1";
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			shipment1.JS_E_ARV = ZDate.Today.AddDays(-10);
			Factory.Save();

			var importer = new TransactionImporter();
			var invoice = new BusinessObjectFactory().New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;
			invoice.SubmittedFromInvoicingForm = true;
			invoice.AH_InvoiceDate = ZDate.Today.AddDays(-2);

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), OrganizationCode = GlbCompany.CurrentCompany.OrgProxy.OH_Code, Country = Country.New(GlbCompany.CurrentCompany.Country) };
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			var universalLine = new PostingJournal();
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.OSAmount = -10;
			universalLine.Job = new EntityReference { Key = "S001", Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			//universalLine.CostSource = new EntityReference { Key = "C001", Type = DataContextType.ForwardingConsol.ToString() };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S001");
			universalShipment.DataContext.AddDataTarget(DataContextType.ForwardingShipment, "S001");
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var universalShipment2 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment2.DataContext = DataContextFactory.New();
			universalShipment2.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C001");
			universalShipment2.DataContext.AddDataTarget(DataContextType.ForwardingConsol, "C001");
			universalTransaction.ShipmentCollection.Add(universalShipment2);

			var universalTransactionXml = universalTransaction.Serialize();

			var collection = new TaxDateDefaultingOptionCollection();
			var taxDateOption = collection.AddNew();
			taxDateOption.JobType = "SHP";
			taxDateOption.DirectionCode = "ALL";
			taxDateOption.Mode = "ALL";
			taxDateOption.Ledger = "AP";
			taxDateOption.TaxDateOption = TaxDateDefaultingOption.Code.EstimatedArrivalDate;
			using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			{
				importer.ImportTransactionLines(universalTransactionXml, invoice, false);
			}

			AssertEquals(1, invoice.Lines.Count);
			var line = invoice.Lines[0];

			AssertEquals("invoice line's tax date should be copied from the shipment's arrival date based on registry setting for SHP.", new ZDate(2021, 2, 5), line.AL_TaxDate);

			invoice.ReleaseAllMutexOnInvoice();
		}

		public void TestConsolCostDefaultTaxRateForImportTransactionLinesWithoutTaxOverridden()
		{
			TestObjectCreator.CreateTaxOverride(TestObjectCreator.CC1, TestObjectCreator.GST2.PK, costSellAll: "COS", jobType: "FCN");
			TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.GST1.PK;
			TestObjectCreator.Creditor1.OH_RL_NKClosestPort = "AUSYD";

			var consol1 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			consol1.JK_MasterBillNum = "MasterBill1";
			var shipment1 = TestObjectCreator.CreateShipment("S001", consol1);
			Factory.Save();

			var importer = new TransactionImporter();
			var invoice = new BusinessObjectFactory().New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;
			invoice.SubmittedFromInvoicingForm = true;

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), OrganizationCode = GlbCompany.CurrentCompany.OrgProxy.OH_Code, Country = Country.New(GlbCompany.CurrentCompany.Country) };
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.OSAmount = -10;
			universalLine.CostSource = new EntityReference { Key = "Consol1", Type = nameof(DataContextType.ForwardingConsol) };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingConsol, "Consol1");
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
			universalShipment.WayBillNumber = consol1.JK_MasterBillNum;
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var universalTransactionXml = universalTransaction.Serialize();

			importer.ImportTransactionLines(universalTransactionXml, invoice, false);

			AssertEquals(1, invoice.Lines.Count);
			var line = invoice.Lines[0];

			AssertNull("Pre-condition", universalLine.VATTaxID);
			AssertNotNull("Pre-condition", line.Consol);

			AssertNotEquals(TestObjectCreator.GST1.PK, line.TaxRate.PK);
			AssertEquals(TestObjectCreator.GST2.PK, line.TaxRate.PK);

			invoice.ReleaseAllMutexOnInvoice();
		}

		public void TestImportTransactionLinesWithTaxOverridden()
		{
			TestObjectCreator.CreateTaxOverride(TestObjectCreator.CC1, TestObjectCreator.GST2.PK, costSellAll: "COS", jobType: "FCN");
			TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.GST1.PK;
			TestObjectCreator.Creditor1.OH_RL_NKClosestPort = "AUSYD";

			var consol1 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			consol1.JK_MasterBillNum = "MasterBill1";
			var shipment1 = TestObjectCreator.CreateShipment("S001", consol1);
			Factory.Save();

			var importer = new TransactionImporter();
			var invoice = new BusinessObjectFactory().New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;
			invoice.SubmittedFromInvoicingForm = true;

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), OrganizationCode = GlbCompany.CurrentCompany.OrgProxy.OH_Code, Country = Country.New(GlbCompany.CurrentCompany.Country) };
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.OSAmount = -10;
			universalLine.CostSource = new EntityReference { Key = "Consol1", Type = nameof(DataContextType.ForwardingConsol) };
			universalLine.VATTaxID = new TaxID { TaxCode = TestObjectCreator.GSTFREE1.AT_Code };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingConsol, "Consol1");
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
			universalShipment.WayBillNumber = consol1.JK_MasterBillNum;
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var universalTransactionXml = universalTransaction.Serialize();

			importer.ImportTransactionLines(universalTransactionXml, invoice, false);

			AssertEquals(1, invoice.Lines.Count);
			var line = invoice.Lines[0];

			AssertNotNull("Pre-condition", universalLine.VATTaxID);
			AssertEquals(TestObjectCreator.GSTFREE1.AT_Code, universalLine.VATTaxID.TaxCode);
			AssertNotNull("Pre-condition", line.Consol);

			AssertNotEquals(TestObjectCreator.GST1.PK, line.TaxRate.PK);
			AssertNotEquals(TestObjectCreator.GST2.PK, line.TaxRate.PK);
			AssertEquals(TestObjectCreator.GSTFREE1.PK, line.TaxRate.PK);

			invoice.ReleaseAllMutexOnInvoice();
		}

		[TestDate(2019, 12, 6)]
		public void TestOSTaxAmountShouldBeEqualsToLocalTaxAmountForLocalCurrency()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			using (PostingExRateRegistryAP.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TOD"))
			{
				var consol1 = TestObjectCreator.CreateConsol("INDEL", "NZAKL", "C001");
				consol1.JK_MasterBillNum = "MASTERBILL1";
				TestObjectCreator.CreateShipment("S001", consol1);
				TestObjectCreator.CreateShipment("S002", consol1);
				TestObjectCreator.CreateShipment("S003", consol1);
				TestObjectCreator.CreateShipment("S004", consol1);
				TestObjectCreator.CreateShipment("S005", consol1);
				Factory.Save();
				var currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

				var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
				universalTransaction.SetShipmentCollection(() => new List<Shipment>());
				universalTransaction.OSTotal = -33725;
				universalTransaction.OSExGSTVATAmount = -28581;
				universalTransaction.OSGSTVATAmount = -5144;
				universalTransaction.OSCurrency = new Currency { Code = currency };
				universalTransaction.BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), Country = Country.New(GlbCompany.CurrentCompany.Country) };

				var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);

				TestObjectCreator.CAP.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.StateGST;
				TestObjectCreator.CAP.AT_Type = AccTaxRate.Types.Rated;
				TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.CAP.PK;

				universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
				universalLine.IsFinalCharge = true;
				universalLine.OSAmount = -28581m;
				universalLine.OSTotalAmount = -33725m;
				universalLine.OSGSTVATAmount = -5144m;
				universalLine.OSCurrency = new Currency { Code = currency };

				universalLine.CostSource = new EntityReference { Key = "Consol1", Type = nameof(DataContextType.ForwardingConsol) };
				universalTransaction.PostingJournalCollection.Add(universalLine);

				var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				universalShipment.DataContext = DataContextFactory.New();
				universalShipment.DataContext.AddDataSource(DataContextType.ForwardingConsol, "Consol1");
				universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
				universalShipment.WayBillNumber = consol1.JK_MasterBillNum;
				universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
				universalTransaction.ShipmentCollection.Add(universalShipment);

				var universalTransactionXml = universalTransaction.Serialize();
				var unallocatedTransaction = TestObjectCreator.CreateTransactionPendingAllocation("INV1", TestObjectCreator.Creditor1, 100);
				var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
				request.Initialize(unallocatedTransaction, universalTransactionXml, false);
				Factory.Save();

				var importer = new TransactionImporter();
				var invoice = new BusinessObjectFactory().Load<APInvoice>(unallocatedTransaction.PK);

				importer.ImportTransactionLines(universalTransactionXml, invoice, false);
				AssertEquals("Pre-condition", 5, invoice.Lines.Count);
				AssertEquals("os tax amount on line 1", 1028.8m, invoice.Lines[0].AL_OSTaxAmount);
				AssertEquals("local tax amount on line 1", 1028.8m, invoice.Lines[0].AL_LocalTaxAmount);
				AssertEquals("os tax amount on line 2", 1028.8m, invoice.Lines[1].AL_OSTaxAmount);
				AssertEquals("local tax amount on line 2", 1028.8m, invoice.Lines[1].AL_LocalTaxAmount);
				AssertEquals("os tax amount on line 3", 1028.8m, invoice.Lines[2].AL_OSTaxAmount);
				AssertEquals("local tax amount on line 3", 1028.8m, invoice.Lines[2].AL_LocalTaxAmount);
				AssertEquals("os tax amount on line 4", 1028.8m, invoice.Lines[3].AL_OSTaxAmount);
				AssertEquals("local tax amount on line 4", 1028.8m, invoice.Lines[3].AL_LocalTaxAmount);
				AssertEquals("os tax amount on line 5", 1028.8m, invoice.Lines[4].AL_OSTaxAmount);
				AssertEquals("local tax amount on line 5", 1028.8m, invoice.Lines[4].AL_LocalTaxAmount);

				invoice.ReleaseAllMutexOnInvoice();
			}
		}

		public void TestImportTransactionsWithSameFactoryCachesUSSalesTaxCalculatorObject()
		{
			TestObjectCreator.Debtor1.CompanyData.SetARTaxApplicable(true);
			Factory.Save();

			var orgAddress = TestObjectCreator.CreateAddress(TestObjectCreator.Debtor1, "123 Road Ave");
			var orgContact = TestObjectCreator.CreateContact(TestObjectCreator.Debtor1, "Mark", "ww@www.com");

			var organizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.None),
				AddressShortCode = orgAddress.OA_Code,
				Address1 = orgAddress.OA_Address1,
				OrganizationCode = TestObjectCreator.Debtor1.OH_Code,
				Contact = orgContact.OC_ContactName,
				Email = orgContact.OC_Email
			};

			var message = new BusinessObjectFactory().New<EDIMessage>();
			var universalFactory = new UniversalObjectFactory();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());

			var t1 = CreateUniversalTransaction("T1");
			var t2 = CreateUniversalTransaction("T2");
			var t3 = CreateUniversalTransaction("T3");

			var mockCalculator = new Mock<IUSSalesTaxCalculator>();
			mockCalculator.Setup(x => x.IsEnabled(It.IsAny<GlbBranch>())).Returns(true);
			mockCalculator.Setup(x => x.ShouldSetSalesTaxOnPost(It.IsAny<InvoicingBase>())).Returns(true);
			var calculationResult = new CalculationResult(5m);
			mockCalculator.Setup(x => x.CalculateSalesTax(It.IsAny<InvoicingBase>())).Returns((calculationResult, null));
			mockCalculator.Setup(x => x.SubmitSalesTax(It.IsAny<InvoicingBase>())).Returns((calculationResult, null));

			using (ObjectFactory.Substitute(mockCalculator.Object))
			{
				var result1 = new TransactionImporter().ImportTransaction(message, t1, logger, universalFactory);
				Assert("Transaction should import without logged errors: \r\n" + logger.ToString(), !logger.HasErrors);
				Assert("Transaction should be imported with success", result1);

				var result2 = new TransactionImporter().ImportTransaction(message, t2, logger, universalFactory);
				Assert("Transaction should import without logged errors: \r\n" + logger.ToString(), !logger.HasErrors);
				Assert("Transaction should be imported with success", result2);

				var result3 = new TransactionImporter().ImportTransaction(message, t3, logger, universalFactory);
				Assert("Transaction should import without logged errors: \r\n" + logger.ToString(), !logger.HasErrors);
				Assert("Transaction should be imported with success", result3);
			}

			var createdInvoices = universalFactory.BOFactory.Load<InvoicingBase>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals(3, createdInvoices.Length);

			mockCalculator.Verify(x => x.Dispose(), Times.Never(), "One USSalesTaxCalculator should be created, and re-used for all universal XML messages processed using a single UniversalObjectFactory. As the calculator is cached, Dispose() is not called");
			mockCalculator.Verify(x => x.SetSalesTaxLineItem(It.IsAny<InvoicingBase>(), It.IsAny<decimal>()), Times.Exactly(3), "Sales tax line items should be added for each invoice");

			TransactionInfo CreateUniversalTransaction(string number)
			{
				var universalTransaction = CreateTransactionWithLine(organizationAddress);
				universalTransaction.Number = number;
				universalTransaction.CheckNumberOrPaymentRef = "Ref" + number;
				universalTransaction.Ledger = LedgerTypes.AccountsReceivable;
				universalTransaction.TransactionType = TransactionType.INV;
				universalTransaction.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ORP } } });
				Assert("Precondition: HasRecipientRole", universalTransaction.HasRecipientRole(RecipientRoleType.ORP));
				return universalTransaction;
			}
		}

		public void TestImportARTransactionWithCashAdvanceReceived()
		{
			AssertCashAdvanceWarning(LedgerTypes.AccountsReceivable);
		}

		public void TestImportAPTransactionWithCashAdvanceReceived()
		{
			AssertCashAdvanceWarning(LedgerTypes.AccountsPayable);
		}

		delegate PostingJournal AssertTransactionLinesDelegate();

		void AssertCashAdvanceWarning(String ledgerType)
		{
			var orgAddress = TestObjectCreator.CreateAddress(TestObjectCreator.Debtor1, "123 Road Ave");
			var orgContact = TestObjectCreator.CreateContact(TestObjectCreator.Debtor1, "Mark", "ww@www.com");

			var organizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.None),
				AddressShortCode = orgAddress.OA_Code,
				Address1 = orgAddress.OA_Address1,
				OrganizationCode = TestObjectCreator.Debtor1.OH_Code,
				Contact = orgContact.OC_ContactName,
				Email = orgContact.OC_Email
			};

			var importer = new TransactionImporter();
			var message = new BusinessObjectFactory().New<EDIMessage>();
			var universalFactory = new UniversalObjectFactory();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());

			var universalTransaction = CreateTransactionWithLine(organizationAddress, false, false);
			universalTransaction.Ledger = ledgerType;
			universalTransaction.TransactionType = TransactionType.INV;
			var universalLine = universalTransaction.PostingJournalCollection[0];
			universalLine.CashAdvanceAmount = 65M;

			universalTransaction.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ORP } } });

			universalTransaction.TotalCashAdvanceAmount = universalLine.CashAdvanceAmount;

			importer.ImportTransaction(message, universalTransaction, logger, universalFactory);

			Assert("logger.HasWarnings", logger.HasWarnings);
			AssertContains($"Warning - Advance Payment information is present in {ledgerType} transaction", logger.ToString());
		}

		void AssertTransactionLines(AssertTransactionLinesDelegate[] assertTransactionLinesDelegates, Action<PostingJournal, InvoicingLineBase> extraAssert = null)
		{
			var invoice = new BusinessObjectFactory().New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			var importer = new TransactionImporter();
			foreach (var assertTransactionLinesDelegate in assertTransactionLinesDelegates)
			{
				universalTransaction.PostingJournalCollection.Clear();
				invoice.Lines.RemoveAndDeleteAll();

				var universalLine = assertTransactionLinesDelegate();
				universalTransaction.PostingJournalCollection.Add(universalLine);

				var universalTransactionXml = universalTransaction.Serialize();
				AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				importer.ImportTransactionLines(universalTransactionXml, invoice, false);
				AssertEquals(1, invoice.Lines.Count);
				AssertLine(universalLine, invoice.Lines[0]);
				extraAssert?.Invoke(universalLine, invoice.Lines[0]);
			}
		}

		PostingJournal CreateTestingJournalLine()
		{
			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.Branch = new Branch { Code = TestObjectCreator.NonCurrentBranch.GB_Code };
			universalLine.Department = new Department { Code = TestObjectCreator.NonCurrentDepartment.GE_Code };
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.Description = "Some line text";
			universalLine.GovernmentReportingChargeCode = "Govt Charge Code";
			universalLine.IsFinalCharge = true;
			universalLine.Sequence = 3;
			universalLine.OSCurrency = new Currency { Code = "USD" };
			universalLine.LocalCurrency = new Currency { Code = "AUD" };
			universalLine.OSAmount = -80;
			universalLine.LocalAmount = -40;
			return universalLine;
		}

		void AssertLine(PostingJournal universalLine, InvoicingLineBase line)
		{
			AssertEquals("Branch", TestObjectCreator.NonCurrentBranch.PK, line.AL_GB);
			AssertEquals("Department", TestObjectCreator.NonCurrentDepartment.PK, line.AL_GE);
			AssertEquals("GenericCharge", TestObjectCreator.CC1.PK, line.GenericCharge);
			AssertEquals("Description", universalLine.Description, line.AL_Desc);
			AssertEquals("IsFinalCharge", universalLine.IsFinalCharge, line.AL_IsFinalCharge);
			AssertEquals("Sequence", universalLine.Sequence, (int)line.AL_Sequence);
			AssertEquals("Currency", universalLine.OSCurrency.Code, line.AL_RX_NKTransactionCurrency);
			AssertEquals("ExchangeRate", 2m, line.AL_ExchangeRate);
			AssertEquals("OSExTaxAmount", 80m, line.AL_OSExTaxAmount);
			AssertEquals("LocalExTaxAmount", 40m, line.AL_LocalExTaxAmount);

			var expectedGovernmentChargeCode = AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value
				? universalLine.GovernmentReportingChargeCode
				: ZString.Empty;
			AssertEquals("GovernmentReportingChargeCode", expectedGovernmentChargeCode, line.AL_GovtChargeCode);

			var expectedSupplyType = AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value
				? universalLine.SupplyType?.Code ?? ZString.Empty
				: ZString.Empty;
			AssertEquals("SupplyType", expectedSupplyType, line.AL_SupplyType);
		}

		void AssertStmNote(StmNote stmNote, IXmlImportLogger logger)
		{
			var noteMessage = stmNote.ST_NoteText.ToString();
			AssertNotNullOrEmpty("PreCondition.", noteMessage?.Trim());
			AssertStartsWith("StmNote should basically equal to logger message.", noteMessage, logger.ToString());
			AssertEndsWith("StmNote should end with saving result.", "Successfully saved Unallocated Transaction.", noteMessage);
		}

		#region Importer AR/AP Journals

		[TestDate(2020, 08, 08, 10, 0, 0)]
		[TestDateIncremental(seconds: 0)]
		public void TestImportTransaction_ARJournal()
		{
			var transactionInfo = GetTestTransactionInfo(LedgerTypes.AccountsReceivable);
			SetPostingJournalCollectionForARAPJournal(transactionInfo);
			AssertImportTransactionInfo(transactionInfo);
		}

		[TestDate(2020, 08, 08, 10, 0, 0)]
		[TestDateIncremental(seconds: 0)]
		public void TestImportTransaction_ARJournalWithInvalidSubAccount_EmptyTypeCode()
		{
			var transactionInfo = GetTestTransactionInfo(LedgerTypes.AccountsReceivable);
			SetPostingJournalCollectionForARAPJournal(transactionInfo, newSubAccounts: new List<SubAccount> { new SubAccount { Type = new UniversalCodeDescriptionPair { Code = "" }, Code = TestObjectCreator.ABIGAS.OH_Code } });
			AssertImportTransactionInfo(transactionInfo, hasInvalidSubAccount: true);
		}

		[TestDate(2020, 08, 08, 10, 0, 0)]
		[TestDateIncremental(seconds: 0)]
		public void TestImportTransaction_ARJournalWithInvalidSubAccount_EmptyCode()
		{
			var transactionInfo = GetTestTransactionInfo(LedgerTypes.AccountsReceivable);
			SetPostingJournalCollectionForARAPJournal(transactionInfo, newSubAccounts: new List<SubAccount> { new SubAccount { Type = new UniversalCodeDescriptionPair { Code = "ORG" }, Code = "" } });
			AssertImportTransactionInfo(transactionInfo, hasInvalidSubAccount: true);
		}

		[TestDate(2020, 08, 08, 10, 0, 0)]
		[TestDateIncremental(seconds: 0)]
		public void TestImportTransaction_ARJournalWithInvalidSubAccount_InvalidCode()
		{
			var transactionInfo = GetTestTransactionInfo(LedgerTypes.AccountsReceivable);
			SetPostingJournalCollectionForARAPJournal(transactionInfo, newSubAccounts: new List<SubAccount> { new SubAccount { Type = new UniversalCodeDescriptionPair { Code = "ORG" }, Code = "InvalidCode" } });
			AssertImportTransactionInfo(transactionInfo, hasInvalidSubAccount: true);
		}

		[TestDate(2020, 08, 08, 10, 0, 0)]
		[TestDateIncremental(seconds: 0)]
		public void TestImportTransaction_ARJournalWithInvalidSubAccount_InvalidType()
		{
			var transactionInfo = GetTestTransactionInfo(LedgerTypes.AccountsReceivable);
			SetPostingJournalCollectionForARAPJournal(transactionInfo, newSubAccounts: new List<SubAccount> { new SubAccount { Type = new UniversalCodeDescriptionPair { Code = "InvalidType" }, Code = TestObjectCreator.ABIGAS.OH_Code } });
			AssertImportTransactionInfo(transactionInfo, hasInvalidSubAccount: true);
		}

		[TestDate(2020, 08, 08, 10, 0, 0)]
		[TestDateIncremental(seconds: 0)]
		public void TestImportTransaction_ARJournalWithDuplicateSubAccount()
		{
			var transactionInfo = GetTestTransactionInfo(LedgerTypes.AccountsReceivable);
			SetPostingJournalCollectionForARAPJournal(transactionInfo, newSubAccounts: new List<SubAccount> { new SubAccount { Type = new UniversalCodeDescriptionPair { Code = "ORG" }, Code = TestObjectCreator.AALSHI.OH_Code } });
			AssertImportTransactionInfo(transactionInfo, hasDuplicateSubAccount: true);
		}

		[TestDate(2020, 08, 08, 10, 0, 0)]
		[TestDateIncremental(seconds: 0)]
		public void TestImportTransaction_APJournalWithInvalidSubAccount_EmptyTypeCode()
		{
			var transactionInfo = GetTestTransactionInfo(LedgerTypes.AccountsPayable);
			SetPostingJournalCollectionForARAPJournal(transactionInfo, newSubAccounts: new List<SubAccount> { new SubAccount { Type = new UniversalCodeDescriptionPair { Code = "" }, Code = TestObjectCreator.ABIGAS.OH_Code } });
			AssertImportTransactionInfo(transactionInfo, hasInvalidSubAccount: true);
		}

		[TestDate(2020, 08, 08, 10, 0, 0)]
		[TestDateIncremental(seconds: 0)]
		public void TestImportTransaction_APJournalWithInvalidSubAccount_EmptyCode()
		{
			var transactionInfo = GetTestTransactionInfo(LedgerTypes.AccountsPayable);
			SetPostingJournalCollectionForARAPJournal(transactionInfo, newSubAccounts: new List<SubAccount> { new SubAccount { Type = new UniversalCodeDescriptionPair { Code = "ORG" }, Code = "" } });
			AssertImportTransactionInfo(transactionInfo, hasInvalidSubAccount: true);
		}

		[TestDate(2020, 08, 08, 10, 0, 0)]
		[TestDateIncremental(seconds: 0)]
		public void TestImportTransaction_APJournalWithInvalidSubAccount_InvalidCode()
		{
			var transactionInfo = GetTestTransactionInfo(LedgerTypes.AccountsPayable);
			SetPostingJournalCollectionForARAPJournal(transactionInfo, newSubAccounts: new List<SubAccount> { new SubAccount { Type = new UniversalCodeDescriptionPair { Code = "ORG" }, Code = "InvalidCode" } });
			AssertImportTransactionInfo(transactionInfo, hasInvalidSubAccount: true);
		}

		[TestDate(2020, 08, 08, 10, 0, 0)]
		[TestDateIncremental(seconds: 0)]
		public void TestImportTransaction_APJournalWithInvalidSubAccount_InvalidType()
		{
			var transactionInfo = GetTestTransactionInfo(LedgerTypes.AccountsPayable);
			SetPostingJournalCollectionForARAPJournal(transactionInfo, newSubAccounts: new List<SubAccount> { new SubAccount { Type = new UniversalCodeDescriptionPair { Code = "InvalidType" }, Code = TestObjectCreator.ABIGAS.OH_Code } });
			AssertImportTransactionInfo(transactionInfo, hasInvalidSubAccount: true);
		}

		[TestDate(2020, 08, 08, 10, 0, 0)]
		[TestDateIncremental(seconds: 0)]
		public void TestImportTransaction_APJournalWithDuplicateSubAccount()
		{
			var transactionInfo = GetTestTransactionInfo(LedgerTypes.AccountsPayable);
			SetPostingJournalCollectionForARAPJournal(transactionInfo, newSubAccounts: new List<SubAccount> { new SubAccount { Type = new UniversalCodeDescriptionPair { Code = "ORG" }, Code = TestObjectCreator.AALSHI.OH_Code } });
			AssertImportTransactionInfo(transactionInfo, hasDuplicateSubAccount: true);
		}

		[TestDate(2020, 08, 08, 10, 0, 0)]
		[TestDateIncremental(seconds: 0)]
		public void TestImportTransaction_ARJournalHasTransactionDate()
		{
			var transactionInfo = GetTestTransactionInfo(LedgerTypes.AccountsReceivable);
			SetPostingJournalCollectionForARAPJournal(transactionInfo);
			transactionInfo.TransactionDate = new ZDateTime(2020, 08, 08, 09, 0, 0);
			AssertImportTransactionInfo(transactionInfo);
		}

		[TestDate(2020, 08, 08, 10, 0, 0)]
		[TestDateIncremental(seconds: 0)]
		public void TestImportTransaction_APJournal()
		{
			var transactionInfo = GetTestTransactionInfo(LedgerTypes.AccountsPayable);
			SetPostingJournalCollectionForARAPJournal(transactionInfo);
			AssertImportTransactionInfo(transactionInfo);
		}

		[TestDate(2020, 08, 08, 10, 0, 0)]
		[TestDateIncremental(seconds: 0)]
		public void TestImportTransaction_APJournalHasTransactionDate()
		{
			var transactionInfo = GetTestTransactionInfo(LedgerTypes.AccountsPayable);
			SetPostingJournalCollectionForARAPJournal(transactionInfo);
			transactionInfo.TransactionDate = new ZDateTime(2020, 08, 08, 09, 0, 0);
			AssertImportTransactionInfo(transactionInfo);
		}

		public void TestImportTransaction_ARJournalHasErrors()
		{
			var transactionInfo = GetTestTransactionInfo(LedgerTypes.AccountsReceivable);
			var expectedExceptionMessage = @"Import failed because journal has validation errors:
Error - GL Account: Please enter a GL Account.
";
			AssertImportTransactionInfo(transactionInfo, expectedExceptionMessage: expectedExceptionMessage);
		}

		public void TestImportTransaction_APJournalHasErrors()
		{
			var transactionInfo = GetTestTransactionInfo(LedgerTypes.AccountsPayable);
			var expectedExceptionMessage = @"Import failed because journal has validation errors:
Error - GL Account: Please enter a GL Account.
";
			AssertImportTransactionInfo(transactionInfo, expectedExceptionMessage: expectedExceptionMessage);
		}

		[TestDate(2020, 08, 08, 10, 0, 0)]
		[TestDateIncremental(seconds: 0)]
		public void TestImportTransaction_APJournalMissingMandatoryTypeSubAccountWithMultipleErrorMessage()
		{
			var transactionInfo = GetTestTransactionInfo(LedgerTypes.AccountsPayable);
			SetPostingJournalCollectionForARAPJournal(transactionInfo, needSetSubAccountCollection: false);
			TestObjectCreator.GLHeader1.SubAccountTypes.OfType<AccGLHeaderSubAccount>().ForEach(x => x.ASA_IsSubClassValidationRuleMandatory = true);
			TestObjectCreator.Factory.Save();

			var expectedExceptionMessage = @"Import failed because journal has validation errors:
Error - Sub Account: Please enter a Sub Account.
Error - Sub Account: Please enter a Sub Account.
Error - Sub Account: Please enter a Sub Account.
Error - Sub Account: Please enter a Sub Account.
";
			AssertImportTransactionInfo(transactionInfo, expectedExceptionMessage: expectedExceptionMessage, hasMissingMandatoryTypeMessage: true);
		}

		[TestDate(2020, 08, 08, 10, 0, 0)]
		[TestDateIncremental(seconds: 0)]
		public void TestImportTransaction_ARJournalMissingMandatoryTypeSubAccountWithMultipleErrorMessage()
		{
			var transactionInfo = GetTestTransactionInfo(LedgerTypes.AccountsReceivable);
			SetPostingJournalCollectionForARAPJournal(transactionInfo, needSetSubAccountCollection: false);
			TestObjectCreator.GLHeader1.SubAccountTypes.OfType<AccGLHeaderSubAccount>().ForEach(x => x.ASA_IsSubClassValidationRuleMandatory = true);
			TestObjectCreator.Factory.Save();

			var expectedExceptionMessage = @"Import failed because journal has validation errors:
Error - Sub Account: Please enter a Sub Account.
Error - Sub Account: Please enter a Sub Account.
Error - Sub Account: Please enter a Sub Account.
Error - Sub Account: Please enter a Sub Account.
";
			AssertImportTransactionInfo(transactionInfo, expectedExceptionMessage: expectedExceptionMessage, hasMissingMandatoryTypeMessage: true);
		}

		[TestDate(2020, 08, 08, 10, 0, 0)]
		[TestDateIncremental(seconds: 0)]
		public void TestImportTransaction_APJournalMissingMandatoryTypeSubAccount()
		{
			var transactionInfo = GetTestTransactionInfo(LedgerTypes.AccountsPayable);

			var newSubAccounts = new List<SubAccount>
				{
					new SubAccount { Type = new UniversalCodeDescriptionPair { Code = "ORG" }, Code = TestObjectCreator.ABIGAS.OH_Code },
					new SubAccount { Type = new UniversalCodeDescriptionPair { Code = "SGP" }, Code = TestObjectCreator.GG1.GG_Code }
				};

			SetPostingJournalCollectionForARAPJournal(transactionInfo, needSetSubAccountCollection: false, newSubAccounts: newSubAccounts);
			TestObjectCreator.GLHeader1.SubAccountTypes.OfType<AccGLHeaderSubAccount>().Where(x => x.ASA_SubClassDisplayName == "ORG" || x.ASA_SubClassDisplayName == "SEG").ForEach(x => x.ASA_IsSubClassValidationRuleMandatory = true);
			TestObjectCreator.Factory.Save();

			var expectedExceptionMessage = @"Import failed because journal has validation errors:
Error - Sub Account: Please enter a Sub Account.
";
			AssertImportTransactionInfo(transactionInfo, expectedExceptionMessage: expectedExceptionMessage, hasMissingMandatoryTypeSubAccount: true);
		}

		[TestDate(2020, 08, 08, 10, 0, 0)]
		[TestDateIncremental(seconds: 0)]
		public void TestImportTransaction_ARJournalMissingMandatoryTypeSubAccount()
		{
			var transactionInfo = GetTestTransactionInfo(LedgerTypes.AccountsReceivable);

			var newSubAccounts = new List<SubAccount>
				{
					new SubAccount { Type = new UniversalCodeDescriptionPair { Code = "ORG" }, Code = TestObjectCreator.ABIGAS.OH_Code },
					new SubAccount { Type = new UniversalCodeDescriptionPair { Code = "SGP" }, Code = TestObjectCreator.GG1.GG_Code }
				};

			SetPostingJournalCollectionForARAPJournal(transactionInfo, needSetSubAccountCollection: false, newSubAccounts: newSubAccounts);
			TestObjectCreator.GLHeader1.SubAccountTypes.OfType<AccGLHeaderSubAccount>().Where(x => x.ASA_SubClassDisplayName == "ORG" || x.ASA_SubClassDisplayName == "SEG").ForEach(x => x.ASA_IsSubClassValidationRuleMandatory = true);
			TestObjectCreator.Factory.Save();

			var expectedExceptionMessage = @"Import failed because journal has validation errors:
Error - Sub Account: Please enter a Sub Account.
";
			AssertImportTransactionInfo(transactionInfo, expectedExceptionMessage: expectedExceptionMessage, hasMissingMandatoryTypeSubAccount: true);
		}

		void SetPostingJournalCollectionForARAPJournal(TransactionInfo transactionInfo, IEnumerable<SubAccount> newSubAccounts = null, bool needSetSubAccountCollection = true)
		{
			var account = new GLAccount { AccountCode = TestObjectCreator.GLHeader1.AG_AccountNum };
			var subAccounts = new List<SubAccount>();

			if (needSetSubAccountCollection)
			{
				subAccounts = new List<SubAccount>()
				{
					new SubAccount { Type = new UniversalCodeDescriptionPair { Code = "ORG" }, Code = TestObjectCreator.ABIGAS.OH_Code },
					new SubAccount { Type = new UniversalCodeDescriptionPair { Code = "SEG" }, Code = TestObjectCreator.AR1.AR_Code },
					new SubAccount { Type = new UniversalCodeDescriptionPair { Code = "SGP" }, Code = TestObjectCreator.GG1.GG_Code },
					new SubAccount { Type = new UniversalCodeDescriptionPair { Code = "STR" }, Code = TestObjectCreator.GS1.GS_Code }
				};
			}

			if (newSubAccounts != null)
			{
				newSubAccounts.ForEach(x => subAccounts.Add(x));
			}

			var postJournal = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { GLAccount = account };

			postJournal.SetSubAccountCollection(() => subAccounts);

			transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal>() { postJournal });
		}

		void AssertImportTransactionInfo(TransactionInfo transactionInfo, string expectedExceptionMessage = "", bool hasInvalidSubAccount = false, bool hasDuplicateSubAccount = false, bool hasMissingMandatoryTypeMessage = false, bool hasMissingMandatoryTypeSubAccount = false)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var ledger = transactionInfo.Ledger ?? string.Empty;
				var address = TestObjectCreator.CreateAddress(ledger == LedgerTypes.AccountsPayable ? TestObjectCreator.Creditor1 : TestObjectCreator.Debtor1, "123 Road Ave");
				var contact = TestObjectCreator.CreateContact(ledger == LedgerTypes.AccountsPayable ? TestObjectCreator.Creditor1 : TestObjectCreator.Debtor1, "Mark", "ww@www.com");

				TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-1));

				Factory.Save();

				var document = new AttachedDocument();
				document.FileName = "Hahaha";
				document.ImageData = (SubStreamableStream)new MemoryStream(new byte[] { 1, 2, 3 });
				document.Type = new DocumentType { Code = "HHH", Description = "Hahaha" };
				transactionInfo.SetAttachedDocumentCollection(() => new List<AttachedDocument> { document });
				var imageDataToCompare = document.ImageData.Copy();

				var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(DocAddressType.None),
					AddressShortCode = address.OA_Code,
					Address1 = address.OA_Address1,
					OrganizationCode = ledger == LedgerTypes.AccountsPayable ? TestObjectCreator.Creditor1.OH_Code : TestObjectCreator.Debtor1.OH_Code,
					Contact = contact.OC_ContactName,
					Email = contact.OC_Email
				};

				transactionInfo.OrganizationAddress = orgAddress;
				var postDate = ZDateTime.Now;
				transactionInfo.PostDate = postDate;
				var message = new BusinessObjectFactory().New<EDIMessage>();
				message.EM_GB = TestObjectCreator.NonCurrentBranch.PK;
				message.EM_GE = TestObjectCreator.NonCurrentDepartment.PK;

				var universalFactory = new UniversalObjectFactory();
				var serviceLogger = new ServiceTaskLogForTesting();
				var logger = new XmlSessionTracker(serviceLogger);

				var registryValue = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.OrganizationMatcherVATRegistrationNumberContextTypeList());
				registryValue.Set(OrganisationMatchingByVATRegistrationNumberContexts.Codes.Payables, true);
				registryValue.Set(OrganisationMatchingByVATRegistrationNumberContexts.Codes.Receivables, true);

				using (AccountingMasterFilesRegistry.Instance.UseVATRegistrationNumberAsOrganizationMatchingCriteria.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
				{
					var importer = new TransactionImporter();
					if (expectedExceptionMessage.IsNullOrEmpty())
					{
						var result = importer.ImportTransaction(message, transactionInfo, logger, universalFactory);
						Assert("Import result", result);

						if (hasInvalidSubAccount)
						{
							AssertContains("Warning - The system found invalid sub account info.", logger.ToString());
						}

						if (hasDuplicateSubAccount)
						{
							AssertContains("Warning - The system found duplicate sub account values for the same sub account type and the first valid value was imported.", logger.ToString());
						}

						AssertNewCreatedJournal(ledger, transactionInfo, universalFactory, logger, address, contact, imageDataToCompare);
					}
					else
					{
						var result = false;
						AssertExceptionThrown<MessageProcessingBusinessFailureException>("expect exception with validation error in the message",
							expectedExceptionMessage, () => result = importer.ImportTransaction(message, transactionInfo, logger, universalFactory));

						if (hasMissingMandatoryTypeMessage)
						{
							AssertContains("Error - Sub Account: Please enter a Sub Account for 'Organization', 'Sales/Expense Groups', 'Staff and Resources', 'Staff Group' sub account type as it is mandatory.", logger.ToString());
						}
						else if (hasMissingMandatoryTypeSubAccount)
						{
							AssertContains("Error - Sub Account: Please enter a Sub Account for 'Sales/Expense Groups' sub account type as it is mandatory.", logger.ToString());
						}
					}
				}
			}
		}

		void AssertNewCreatedJournal(string ledger, TransactionInfo transactionInfo, UniversalObjectFactory factoryCasted, IXmlImportLogger logger, OrgAddress orgAddress, OrgContact orgContact, SubStreamableStream imageDataToCompare)
		{
			var query = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
			query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ledger);

			Journal journal;
			if (ledger == LedgerTypes.AccountsPayable)
			{
				journal = Factory.Load<APJournal>(query).SingleOrDefault();
			}
			else
			{
				journal = Factory.Load<ARJournal>(query).SingleOrDefault();
			}
			AssertEquals("PostDate", transactionInfo.PostDate, journal.AH_PostDate);
			AssertEquals("Organization", orgAddress.OA_OH, journal.AH_OH);
			AssertEquals("TransactionDate", transactionInfo.TransactionDate ?? ZDateTime.Now, journal.AH_InvoiceDate);
			AssertEquals("NumberOfSupportingDocuments", transactionInfo.NumberOfSupportingDocuments, (int)journal.AH_NumberOfSupportingDocuments);
			AssertEquals("Currency", transactionInfo.OSCurrency.Code, journal.AH_RX_NKTransactionCurrency);
			AssertEquals("ExchangeRate", 2m, journal.AH_ExchangeRate);
			AssertEquals("OSExTaxAmount", 120m, journal.AH_OSExTaxAmount);
			AssertEquals("OSTaxAmount", 0m, journal.AH_OSTaxAmount);
			AssertEquals("LocalExTaxAmount", 60m, journal.AH_LocalExTaxAmount);
			AssertEquals("LocalTaxAmount", 0m, journal.AH_LocalTaxAmount);
			AssertEquals("Branch", GlbBranch.CurrentBranch.PK, journal.AH_GB);
			AssertEquals("Department", GlbDepartment.CurrentDepartment.PK, journal.AH_GE);
			AssertEquals("GLAccount", TestObjectCreator.GLHeader1.PK, journal.AH_AG);
			AssertEquals("Description", "Description", journal.AH_Desc);
			AssertEquals("AgreedPaymentMethod", OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck, journal.AH_AgreedPaymentMethodOverride);

			AssertEquals("Has 4 SubAccount", 4, journal.SubAccounts.Count);
			var orgSubAccount = journal.SubAccounts.FirstOrDefault(x => ((AccTransactionHeaderSubAccount)x).AHS_SubClassParentTableCode == "OH") as AccTransactionHeaderSubAccount;
			AssertEquals("Has Org SubAccount", TestObjectCreator.ABIGAS.PK, orgSubAccount.AHS_SubClassParentId);
			var glbGroupSubAccount = journal.SubAccounts.FirstOrDefault(x => ((AccTransactionHeaderSubAccount)x).AHS_SubClassParentTableCode == "GG") as AccTransactionHeaderSubAccount;
			AssertEquals("Has GlbGroup SubAccount", TestObjectCreator.GG1.PK, glbGroupSubAccount.AHS_SubClassParentId);
			var accGroupSubAccount = journal.SubAccounts.FirstOrDefault(x => ((AccTransactionHeaderSubAccount)x).AHS_SubClassParentTableCode == "AR") as AccTransactionHeaderSubAccount;
			AssertEquals("Has AccGroup SubAccount", TestObjectCreator.AR1.PK, accGroupSubAccount.AHS_SubClassParentId);
			var staffSubAccount = journal.SubAccounts.FirstOrDefault(x => ((AccTransactionHeaderSubAccount)x).AHS_SubClassParentTableCode == "GS") as AccTransactionHeaderSubAccount;
			AssertEquals("Has Staff SubAccount", TestObjectCreator.GS1.PK, staffSubAccount.AHS_SubClassParentId);

			AssertEquals(1, ((IDocManagerSupport)journal).DocManagerInfo.AllEDocs.Count);
			var eDoc = ((IDocManagerSupport)journal).DocManagerInfo.AllEDocs[0];
			var document = transactionInfo.AttachedDocumentCollection[0];
			AssertEquals(document.FileName, eDoc.FileName);
			AssertArrayEqualsByElements(imageDataToCompare.ConvertToByteArrayAndCloseStream(), eDoc.GetImageDataReader().ConvertToByteArrayAndCloseStream());
			AssertEquals(document.Type.Code, eDoc.DocType);
		}

		TransactionInfo GetTestTransactionInfo(ZString ledger)
		{
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, OrgHeaderSchema.Constants.Prefix, false);
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, AccGroupsSchema.Constants.Prefix, false);
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, GlbStaffSchema.Constants.Prefix, false);
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, GlbGroupSchema.Constants.Prefix, false);
			Factory.Save();

			return new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				TransactionType = TransactionType.JNL,
				Ledger = ledger,
				Branch = new Branch { Code = "CAN" },
				Department = new Department { Code = "BRN" },
				Description = "Description",
				NumberOfSupportingDocuments = 5,
				BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None) },
				OSCurrency = new Currency { Code = "AUD" },
				LocalCurrency = new Currency { Code = "CNY" },
				OSExGSTVATAmount = 120,
				LocalExVATAmount = 60,
				OSGSTVATAmount = 10,
				LocalVATAmount = 5,
				AgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck
			};
		}

		#endregion

		#region Set up

		protected override void SetUp()
		{
			base.SetUp();
			var rate = AccTaxRate.Helper.FindTaxRate(new BusinessObjectFactory(), AccTaxRate.Helper.MainNotReportableTaxRegistryID, Env.CurrentCompanyPK);
			rate.SetRateNumerator_ForTestOnly(0);
			rate.Factory.Save();

			new AccountingPeriodTestHelper(Factory).SetupPeriods();
		}

		TransactionInfo CreateUniversalTransactionWithSubAccounts(bool isAR, bool isINV = true)
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-1));
			AssertNotNull(TestObjectCreator.AR1);
			AssertNotNull(TestObjectCreator.GS1);

			TestObjectCreator.GLHeader1.SubAccountTypes.RemoveAndDeleteAll();
			TestObjectCreator.GLHeader2.SubAccountTypes.RemoveAndDeleteAll();
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, OrgHeaderSchema.Constants.Prefix, false);
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, AccGroupsSchema.Constants.Prefix, false);
			Factory.Save();

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				Ledger = isAR ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable,
				TransactionType = isINV ? TransactionType.INV : TransactionType.CRD,
				OrganizationAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), Address1 = "street", OrganizationCode = TestObjectCreator.Debtor.OH_Code },
				Number = "1234",
				OSExGSTVATAmount = isAR ? 100 : (isINV ? -100 : 100),
				Branch = new Branch { Code = TestObjectCreator.NonCurrentBranch.GB_Code },
				Department = new Department { Code = TestObjectCreator.MiscDepartment.GE_Code },
				BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), OrganizationCode = TestObjectCreator.NonCurrentCompany.OrgProxy.OH_Code }
			};

			return universalTransaction;
		}

		void SetupPostingJournal(TransactionInfo universalTransaction)
		{
			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OSAmount = 40,
				Description = "Description",
				Branch = new Branch { Code = TestObjectCreator.NonCurrentBranch.GB_Code },
			};
			universalTransaction.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ORP } } });

			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.PostingJournalCollection.Add(universalLine);
		}

		void SetupSubAccounts(TransactionInfo universalTransaction)
		{
			var universalLine = universalTransaction.PostingJournalCollection[0];
			universalLine.SubAccount = new SubAccount { Type = new UniversalCodeDescriptionPair { Code = Constants.SubAccountType.Organization }, Code = TestObjectCreator.ABIGAS.OH_Code };
			universalLine.SetSubAccountCollection(() =>
			{
				var subAccountCollection = new List<SubAccount>();
				subAccountCollection.Add(new SubAccount { Type = new UniversalCodeDescriptionPair { Code = Constants.SubAccountType.Organization }, Code = TestObjectCreator.ABIGAS.OH_Code });
				subAccountCollection.Add(new SubAccount { Type = new UniversalCodeDescriptionPair { Code = Constants.SubAccountType.SalesGroup }, Code = TestObjectCreator.AR1.AR_Code });

				return subAccountCollection;
			});
		}

		#endregion

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		string UniversalTransactionXML => @"
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
	<Branch>
	  <Code>BER</Code>
	  <Name>EDI - Sydney Training</Name>
	</Branch>
	<Department>
	  <Code>BRN</Code>
	  <Name>Branch</Name>
	</Department>
	<Description>AP INVOICE</Description>
	<DueDate>2015-05-01T19:09:00</DueDate>
	<Ledger>AP</Ledger>
	<LocalExVATAmount>-60.0000</LocalExVATAmount>
	<Number>11112222</Number>
	<NumberOfSupportingDocuments>2</NumberOfSupportingDocuments>
	<OrganizationAddress>
	  <AddressType>None</AddressType>
	  <OrganizationCode>ABCFRESYD</OrganizationCode>
	</OrganizationAddress>
	<OSCurrency>
	  <Code>USD</Code>
	  <Description>United States Dollar</Description>
	</OSCurrency>
	<OSExGSTVATAmount>-120.0000</OSExGSTVATAmount>
	<PostDate>2015-04-30T19:09:00</PostDate>
	<TransactionDate>2015-04-28T19:09:00</TransactionDate>
	<TransactionType>INV</TransactionType>
	<DocumentReceivedDate>2020-02-13T11:11:00</DocumentReceivedDate>

	<PostingJournalCollection>
	  <PostingJournal>
		<Branch>
		  <Code>SYD</Code>
		  <Name>EDI - Sydney Training</Name>
		</Branch>
		<Department>
		  <Code>BRN</Code>
		  <Name>Branch</Name>
		</Department>
		<Description>FREIGHT REVENUE ACTUAL</Description>
		<GLAccount>
		  <AccountCode>1010.10.10</AccountCode>
		  <Description>FREIGHT REVENUE ACTUAL</Description>
		</GLAccount>
		<IsFinalCharge>true</IsFinalCharge>
		<LocalAmount>-60.0000</LocalAmount>
		<OSAmount>-120.00</OSAmount>
		<OSCurrency>
		  <Code>USD</Code>
		  <Description>United States Dollar</Description>
		</OSCurrency>
		<Sequence>2</Sequence>
	  </PostingJournal>
	</PostingJournalCollection>
  </TransactionInfo>
</UniversalTransaction>";

		string UniversalARTransactionXML => @"
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
	<Branch>
	  <Code>BER</Code>
	  <Name>EDI - Sydney Training</Name>
	</Branch>
	<Department>
	  <Code>BRN</Code>
	  <Name>Branch</Name>
	</Department>
	<Description>AR INVOICE</Description>
	<DueDate>2015-05-01T19:09:00</DueDate>
	<Ledger>AR</Ledger>
	<LocalExVATAmount>-60.0000</LocalExVATAmount>
	<Number>11112222</Number>
	<NumberOfSupportingDocuments>2</NumberOfSupportingDocuments>
	<OrganizationAddress>
	  <AddressType>None</AddressType>
	  <OrganizationCode>ABCFRESYD</OrganizationCode>
	</OrganizationAddress>
	<OSCurrency>
	  <Code>USD</Code>
	  <Description>United States Dollar</Description>
	</OSCurrency>
	<OSExGSTVATAmount>-120.0000</OSExGSTVATAmount>
	<PostDate>2015-04-30T19:09:00</PostDate>
	<TransactionDate>2015-04-28T19:09:00</TransactionDate>
	<TransactionType>INV</TransactionType>
	<DocumentReceivedDate>2020-02-13T11:11:00</DocumentReceivedDate>

	<PostingJournalCollection>
	  <PostingJournal>
		<Branch>
		  <Code>SYD</Code>
		  <Name>EDI - Sydney Training</Name>
		</Branch>
		<Department>
		  <Code>BRN</Code>
		  <Name>Branch</Name>
		</Department>
		<Description>FREIGHT REVENUE ACTUAL</Description>
		<GLAccount>
		  <AccountCode>1010.10.10</AccountCode>
		  <Description>FREIGHT REVENUE ACTUAL</Description>
		</GLAccount>
		<IsFinalCharge>true</IsFinalCharge>
		<LocalAmount>-60.0000</LocalAmount>
		<OSAmount>-120.00</OSAmount>
		<OSCurrency>
		  <Code>USD</Code>
		  <Description>United States Dollar</Description>
		</OSCurrency>
		<Sequence>2</Sequence>
	  </PostingJournal>
	</PostingJournalCollection>
  </TransactionInfo>
</UniversalTransaction>";

		string UniversalARTransactionXMLWithPlaceOfSupply => @"
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
	<Branch>
	  <Code>BER</Code>
	  <Name>EDI - Sydney Training</Name>
	</Branch>
	<Department>
	  <Code>BRN</Code>
	  <Name>Branch</Name>
	</Department>
	<Description>AR INVOICE</Description>
	<DueDate>2015-05-01T19:09:00</DueDate>
	<Ledger>AR</Ledger>
	<LocalExVATAmount>-60.0000</LocalExVATAmount>
	<Number>11112222</Number>
	<NumberOfSupportingDocuments>2</NumberOfSupportingDocuments>
	<OrganizationAddress>
	  <AddressType>None</AddressType>
	  <OrganizationCode>ABCFRESYD</OrganizationCode>
	</OrganizationAddress>
	<OSCurrency>
	  <Code>USD</Code>
	  <Description>United States Dollar</Description>
	</OSCurrency>
	<OSExGSTVATAmount>-120.0000</OSExGSTVATAmount>
	<PlaceOfSupply>
		<Location>
			<Code>AR</Code>
			<Description>Arunachal Pradesh</Description>
		</Location>
		<LocationType>
			<Code>STA</Code>
			<Description>State</Description>
		</LocationType>
	</PlaceOfSupply>
	<PostDate>2015-04-30T19:09:00</PostDate>
	<TransactionDate>2015-04-28T19:09:00</TransactionDate>
	<TransactionType>INV</TransactionType>
	<DocumentReceivedDate>2020-02-13T11:11:00</DocumentReceivedDate>

	<PostingJournalCollection>
	  <PostingJournal>
		<Branch>
		  <Code>SYD</Code>
		  <Name>EDI - Sydney Training</Name>
		</Branch>
		<Department>
		  <Code>BRN</Code>
		  <Name>Branch</Name>
		</Department>
		<Description>FREIGHT REVENUE ACTUAL</Description>
		<GLAccount>
		  <AccountCode>1010.10.10</AccountCode>
		  <Description>FREIGHT REVENUE ACTUAL</Description>
		</GLAccount>
		<IsFinalCharge>true</IsFinalCharge>
		<LocalAmount>-60.0000</LocalAmount>
		<OSAmount>-120.00</OSAmount>
		<OSCurrency>
		  <Code>USD</Code>
		  <Description>United States Dollar</Description>
		</OSCurrency>
		<PlaceOfSupply>
			<Location>
				<Code>AR</Code>
				<Description>Arunachal Pradesh</Description>
			</Location>
			<LocationType>
				<Code>STA</Code>
				<Description>State</Description>
			</LocationType>
		</PlaceOfSupply>
		<Sequence>2</Sequence>
	  </PostingJournal>
	</PostingJournalCollection>
  </TransactionInfo>
</UniversalTransaction>";

		string UniversalAPTransactionXML_ForCreditorMapping => @"
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <TransactionInfo>
	<Branch>
	  <Code>BER</Code>
	  <Name>EDI - Sydney Training</Name>
	</Branch>
	<Department>
	  <Code>BRN</Code>
	  <Name>Branch</Name>
	</Department>
	<Description>AP INVOICE</Description>
    <ComplianceSubType>EIC</ComplianceSubType>
    <ExchangeRate>1</ExchangeRate>
    <Ledger>AP</Ledger>
    <LocalCurrency>
      <Code>TRY</Code>
    </LocalCurrency>
    <LocalExVATAmount>-50</LocalExVATAmount>
    <LocalTotal>-59</LocalTotal>
    <LocalVATAmount>-9</LocalVATAmount>
    <Number>APFAT11</Number>
    <OrganizationAddress>
      <AddressType>OFC</AddressType>
      <RegistrationNumberCollection>
        <RegistrationNumber>
          <Type>
            <Code>VAT</Code>
          </Type>
          <CountryOfIssue>
            <Code>TR</Code>
          </CountryOfIssue>
          <Value>9000068418</Value>
        </RegistrationNumber>
        <RegistrationNumber>
          <Type>
            <Code>MER</Code>
          </Type>
          <CountryOfIssue>
            <Code>TR</Code>
          </CountryOfIssue>
          <Value>45566546861546</Value>
        </RegistrationNumber>
        <RegistrationNumber>
          <Type>
            <Code>TRN</Code>
          </Type>
          <CountryOfIssue>
            <Code>TR</Code>
          </CountryOfIssue>
          <Value>58799</Value>
        </RegistrationNumber>
        <RegistrationNumber>
          <Type>
            <Code>VDM</Code>
          </Type>
          <CountryOfIssue>
            <Code>TR</Code>
          </CountryOfIssue>
          <Value>BEYDAĞI</Value>
        </RegistrationNumber>
      </RegistrationNumberCollection>
    </OrganizationAddress>
    <OSCurrency>
      <Code>TRY</Code>
    </OSCurrency>
    <OSExGSTVATAmount>-50</OSExGSTVATAmount>
    <OSGSTVATAmount>-9</OSGSTVATAmount>
    <OSTotal>-59</OSTotal>
    <OutstandingAmount>-59</OutstandingAmount>
    <TransactionDate>2020-04-28</TransactionDate>
    <GovernmentAllocatedID>0ad529d9-10d7-40cd-86ff-8011e41f5d2f</GovernmentAllocatedID>
    <PostingJournalCollection>
      <PostingJournal>
        <ChargeCode>
          <Code>1</Code>
          <Description>KARELİ DEFTER</Description>
        </ChargeCode>
        <ChargeCurrency>
          <Code>TRY</Code>
        </ChargeCurrency>
        <ChargeExchangeRate>1</ChargeExchangeRate>
        <Description>KARELİ DEFTER</Description>
        <LocalAmount>-50</LocalAmount>
        <LocalCurrency>
          <Code>TRY</Code>
        </LocalCurrency>
        <LocalGSTVATAmount>-9</LocalGSTVATAmount>
        <LocalTotalAmount>-59</LocalTotalAmount>
        <OSAmount>-50</OSAmount>
        <OSCurrency>
          <Code>TRY</Code>
        </OSCurrency>
        <OSGSTVATAmount>-9</OSGSTVATAmount>
        <OSTotalAmount>-59</OSTotalAmount>
        <RevenueRecognitionType>IMM</RevenueRecognitionType>
        <TransactionCategory>STD</TransactionCategory>
        <TransactionType>CST</TransactionType>
        <VATTaxID>
          <TaxRate>18</TaxRate>
          <TaxType>
            <Code>RAT</Code>
          </TaxType>
        </VATTaxID>
      </PostingJournal>
      <PostingJournal>
        <ChargeCode>
          <Code>Note</Code>
          <ChargeType>
            <Code>CMT</Code>
          </ChargeType>
        </ChargeCode>
        <Description>Yalnız : #ElliDokuz TL #</Description>
      </PostingJournal>
    </PostingJournalCollection>
  </TransactionInfo>
</UniversalTransaction>";

		string UniversalAPTransactionXML_ForDebtorMapping => @"
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <DataContext>

      <TriggerCount>0</TriggerCount>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Trigger</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organization Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <Branch>
      <Code>SYD</Code>
    </Branch>
    <BranchAddress>
      <AddressType>None</AddressType>
      <Country>
        <Code>TR</Code>
        <Name>Turkey</Name>
      </Country>
    </BranchAddress>
    <Department>
      <Code>CEA</Code>
    </Department>
    <Description>Some text</Description>
    <DueDate>2022-05-13T00:00:00</DueDate>
    <ExchangeRate>2</ExchangeRate>
    <Ledger>AR</Ledger>
    <LocalCurrency>
      <Code>AUD</Code>
    </LocalCurrency>
    <LocalExVATAmount>60</LocalExVATAmount>
    <LocalVATAmount>5</LocalVATAmount>
    <Number>{Number}</Number>
    <NumberOfSupportingDocuments>5</NumberOfSupportingDocuments>
    <InvoiceTerm>COD</InvoiceTerm>
    <OrganizationAddress>
      <AddressType>OFC</AddressType>
      <RegistrationNumberCollection>
        <RegistrationNumber>
          <Type>
            <Code>VAT</Code>
          </Type>
          <CountryOfIssue>
            <Code>TR</Code>
          </CountryOfIssue>
          <Value>9000068418</Value>
        </RegistrationNumber>
        <RegistrationNumber>
          <Type>
            <Code>MER</Code>
          </Type>
          <CountryOfIssue>
            <Code>TR</Code>
          </CountryOfIssue>
          <Value>45566546861546</Value>
        </RegistrationNumber>
        <RegistrationNumber>
          <Type>
            <Code>TRN</Code>
          </Type>
          <CountryOfIssue>
            <Code>TR</Code>
          </CountryOfIssue>
          <Value>58799</Value>
        </RegistrationNumber>
        <RegistrationNumber>
          <Type>
            <Code>VDM</Code>
          </Type>
          <CountryOfIssue>
            <Code>TR</Code>
          </CountryOfIssue>
          <Value>BEYDAĞI</Value>
        </RegistrationNumber>
      </RegistrationNumberCollection>
    </OrganizationAddress>
    <OSCurrency>
      <Code>GBP</Code>
    </OSCurrency>
    <OSExGSTVATAmount>120</OSExGSTVATAmount>
    <OSGSTVATAmount>10</OSGSTVATAmount>
    <TransactionDate>2022-05-06T00:00:00</TransactionDate>
    <TransactionType>INV</TransactionType>

    <PostingJournalCollection>
      <PostingJournal>
        <Branch>
          <Code>SYD</Code>
        </Branch>
        <ChargeCode>
          <Code>{GenericCharge}</Code>
        </ChargeCode>
        <Department>
          <Code>CEA</Code>
        </Department>
        <Description>Some line text</Description>
        <GovernmentReportingChargeCode>Govt Charge Code</GovernmentReportingChargeCode>
        <IsFinalCharge>true</IsFinalCharge>
        <LocalAmount>65</LocalAmount>
        <LocalCurrency>
          <Code>AUD</Code>
        </LocalCurrency>
        <OSAmount>130</OSAmount>
        <OSCurrency>
          <Code>GBP</Code>
        </OSCurrency>
        <Sequence>3</Sequence>
        <SupplyType>
          <Code>DSB</Code>
        </SupplyType>
        <TaxDate>2022-05-07</TaxDate>

        <PostingJournalDetailCollection>
        </PostingJournalDetailCollection>
      </PostingJournal>
    </PostingJournalCollection>

    <ShipmentCollection>
    </ShipmentCollection>
  </TransactionInfo>
</UniversalTransaction>
";

		string UniversalAPTransactionXML_ForSupportShipmentDataType => @"
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <DataContext>

      <TriggerCount>0</TriggerCount>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Trigger</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organization Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <Branch>
      <Code>SYD</Code>
    </Branch>
    <BranchAddress>
      <AddressType>None</AddressType>
      <Country>
        <Code>TR</Code>
        <Name>Turkey</Name>
      </Country>
    </BranchAddress>
    <Department>
      <Code>CEA</Code>
    </Department>
    <Description>Some text</Description>
    <DueDate>2022-05-13T00:00:00</DueDate>
    <ExchangeRate>2</ExchangeRate>
    <Ledger>AP</Ledger>
    <LocalCurrency>
      <Code>AUD</Code>
    </LocalCurrency>
    <LocalExVATAmount>60</LocalExVATAmount>
    <LocalVATAmount>5</LocalVATAmount>
    <Number>{Number}</Number>
    <NumberOfSupportingDocuments>5</NumberOfSupportingDocuments>
    <InvoiceTerm>COD</InvoiceTerm>
    <OrganizationAddress>
      <AddressType>OFC</AddressType>
      <RegistrationNumberCollection>
        <RegistrationNumber>
          <Type>
            <Code>VAT</Code>
          </Type>
          <CountryOfIssue>
            <Code>TR</Code>
          </CountryOfIssue>
          <Value>9000068418</Value>
        </RegistrationNumber>
        <RegistrationNumber>
          <Type>
            <Code>MER</Code>
          </Type>
          <CountryOfIssue>
            <Code>TR</Code>
          </CountryOfIssue>
          <Value>45566546861546</Value>
        </RegistrationNumber>
        <RegistrationNumber>
          <Type>
            <Code>TRN</Code>
          </Type>
          <CountryOfIssue>
            <Code>TR</Code>
          </CountryOfIssue>
          <Value>58799</Value>
        </RegistrationNumber>
        <RegistrationNumber>
          <Type>
            <Code>VDM</Code>
          </Type>
          <CountryOfIssue>
            <Code>TR</Code>
          </CountryOfIssue>
          <Value>BEYDAĞI</Value>
        </RegistrationNumber>
      </RegistrationNumberCollection>
    </OrganizationAddress>
    <OSCurrency>
      <Code>GBP</Code>
    </OSCurrency>
    <OSExGSTVATAmount>120</OSExGSTVATAmount>
    <OSGSTVATAmount>10</OSGSTVATAmount>
    <TransactionDate>2022-05-06T00:00:00</TransactionDate>
    <TransactionType>INV</TransactionType>

    <PostingJournalCollection>
      <PostingJournal>
		<Job>
			<Type>Job</Type>
			<Key>S00001604</Key>
		</Job>
        <Branch>
          <Code>SYD</Code>
        </Branch>
        <ChargeCode>
          <Code>{GenericCharge}</Code>
        </ChargeCode>
        <Department>
          <Code>CEA</Code>
        </Department>
        <Description>Some line text</Description>
        <GovernmentReportingChargeCode>Govt Charge Code</GovernmentReportingChargeCode>
        <IsFinalCharge>true</IsFinalCharge>
        <LocalAmount>65</LocalAmount>
        <LocalCurrency>
          <Code>AUD</Code>
        </LocalCurrency>
        <OSAmount>130</OSAmount>
        <OSCurrency>
          <Code>GBP</Code>
        </OSCurrency>
        <Sequence>3</Sequence>
        <SupplyType>
          <Code>DSB</Code>
        </SupplyType>
        <TaxDate>2022-05-07</TaxDate>

        <PostingJournalDetailCollection>
        </PostingJournalDetailCollection>
      </PostingJournal>
      <PostingJournal>
		<Job>
			<Type>Job</Type>
			<Key>S00001605</Key>
		</Job>
        <Branch>
          <Code>SYD</Code>
        </Branch>
        <ChargeCode>
          <Code>{GenericCharge}</Code>
        </ChargeCode>
        <Department>
          <Code>CEA</Code>
        </Department>
        <Description>Some line text</Description>
        <GovernmentReportingChargeCode>Govt Charge Code</GovernmentReportingChargeCode>
        <IsFinalCharge>true</IsFinalCharge>
        <LocalAmount>65</LocalAmount>
        <LocalCurrency>
          <Code>AUD</Code>
        </LocalCurrency>
        <OSAmount>130</OSAmount>
        <OSCurrency>
          <Code>GBP</Code>
        </OSCurrency>
        <Sequence>3</Sequence>
        <SupplyType>
          <Code>DSB</Code>
        </SupplyType>
        <TaxDate>2022-05-07</TaxDate>

        <PostingJournalDetailCollection>
        </PostingJournalDetailCollection>
      </PostingJournal>
	  <PostingJournal>
		<Job>
			<Type>Job</Type>
			<Key>S00001605</Key>
		</Job>
        <Branch>
          <Code>SYD</Code>
        </Branch>
        <ChargeCode>
          <Code>{GenericCharge}</Code>
        </ChargeCode>
        <Department>
          <Code>CEA</Code>
        </Department>
        <Description>Some line text</Description>
        <GovernmentReportingChargeCode>Govt Charge Code</GovernmentReportingChargeCode>
        <IsFinalCharge>true</IsFinalCharge>
        <LocalAmount>65</LocalAmount>
        <LocalCurrency>
          <Code>AUD</Code>
        </LocalCurrency>
        <OSAmount>130</OSAmount>
        <OSCurrency>
          <Code>GBP</Code>
        </OSCurrency>
        <Sequence>3</Sequence>
        <SupplyType>
          <Code>DSB</Code>
        </SupplyType>
        <TaxDate>2022-05-07</TaxDate>

        <PostingJournalDetailCollection>
        </PostingJournalDetailCollection>
      </PostingJournal>
      <PostingJournal>
		<Job>
			<Type>Job</Type>
			<Key>S00001606</Key>
		</Job>
        <Branch>
          <Code>SYD</Code>
        </Branch>
        <ChargeCode>
          <Code>{GenericCharge}</Code>
        </ChargeCode>
        <Department>
          <Code>CEA</Code>
        </Department>
        <Description>Some line text</Description>
        <GovernmentReportingChargeCode>Govt Charge Code</GovernmentReportingChargeCode>
        <IsFinalCharge>true</IsFinalCharge>
        <LocalAmount>65</LocalAmount>
        <LocalCurrency>
          <Code>AUD</Code>
        </LocalCurrency>
        <OSAmount>130</OSAmount>
        <OSCurrency>
          <Code>GBP</Code>
        </OSCurrency>
        <Sequence>3</Sequence>
        <SupplyType>
          <Code>DSB</Code>
        </SupplyType>
        <TaxDate>2022-05-07</TaxDate>

        <PostingJournalDetailCollection>
        </PostingJournalDetailCollection>
      </PostingJournal>
      <PostingJournal>
		<Job>
			<Type>Job</Type>
			<Key></Key>
		</Job>
        <Branch>
          <Code>SYD</Code>
        </Branch>
        <ChargeCode>
          <Code>{GenericCharge}</Code>
        </ChargeCode>
        <Department>
          <Code>CEA</Code>
        </Department>
        <Description>Some line text</Description>
        <GovernmentReportingChargeCode>Govt Charge Code</GovernmentReportingChargeCode>
        <IsFinalCharge>true</IsFinalCharge>
        <LocalAmount>65</LocalAmount>
        <LocalCurrency>
          <Code>AUD</Code>
        </LocalCurrency>
        <OSAmount>130</OSAmount>
        <OSCurrency>
          <Code>GBP</Code>
        </OSCurrency>
        <Sequence>3</Sequence>
        <SupplyType>
          <Code>DSB</Code>
        </SupplyType>
        <TaxDate>2022-05-07</TaxDate>

        <PostingJournalDetailCollection>
        </PostingJournalDetailCollection>
      </PostingJournal>
    </PostingJournalCollection>

    <ShipmentCollection>
		<Shipment>
			<DataContext>
				<DataSourceCollection>
					<DataSource>
					  <Type>Project</Type>
					  <Key>S00001601</Key>
					</DataSource>
				</DataSourceCollection>
			</DataContext>
		</Shipment>
		<Shipment>
			<DataContext>
				<DataSourceCollection>
					<DataSource>
					  <Type>NewInvaildDataSourceType</Type>
					  <Key>S00001602</Key>
					</DataSource>
				</DataSourceCollection>
			</DataContext>
		</Shipment>
		<Shipment>
			<DataContext>
				<DataSourceCollection>
					<DataSource>
					  <Type>Staff</Type>
					  <Key>S00001604</Key>
					</DataSource>
				</DataSourceCollection>
			</DataContext>
		</Shipment>
		<Shipment>
			<DataContext>
				<DataSourceCollection>
					<DataSource>
					  <Type>InvaildDataSourceType</Type>
					  <Key>S00001605</Key>
					</DataSource>
				</DataSourceCollection>
			</DataContext>
		</Shipment>
	</ShipmentCollection>
  </TransactionInfo>
</UniversalTransaction>
";

		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAP => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;
		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAR => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR;
	}
}
