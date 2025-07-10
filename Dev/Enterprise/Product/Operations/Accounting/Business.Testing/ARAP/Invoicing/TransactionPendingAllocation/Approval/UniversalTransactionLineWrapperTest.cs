using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.Testing;
using Enterprise.Accounting.Export;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Order = Enterprise.Freight.Forwarding.Orders.Business.Order;
using UniversalCodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(UniversalTransactionLineWrapper))]
	class UniversalTransactionLineWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestInitialize()
		{
			var universalLine1 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine1.Branch = new Branch() { Code = "line branch" };
			universalLine1.Department = new Department() { Code = "line department" };
			universalLine1.ChargeCode = new ChargeCode() { Code = new ZCodeMappedZString("charge code") { MappedValue = "mapped charge code" } };
			universalLine1.GLAccount = new GLAccount() { AccountCode = "account number" };
			universalLine1.Description = "line text";
			universalLine1.IsFinalCharge = true;
			universalLine1.Sequence = 3;
			universalLine1.OSCurrency = new Currency() { Code = "USD" };
			universalLine1.LocalCurrency = new Currency() { Code = "AUD" };
			universalLine1.OSAmount = -80;
			universalLine1.OSGSTVATAmount = -10;
			universalLine1.LocalAmount = -40;
			universalLine1.LocalGSTVATAmount = -5;
			universalLine1.OSWHTAmount = -6;
			universalLine1.VATTaxID = new TaxID() { TaxCode = "tax" };
			universalLine1.WithholdingTaxID = new TaxID() { TaxCode = "WHT tax" };
			universalLine1.SubAccount = new SubAccount() { Code = "sub account code", Type = new UniversalCodeDescriptionPair() { Code = "sub account type" } };

			universalLine1.Job = new EntityReference() { Key = "job number" };
			universalLine1.CostSource = new EntityReference() { Key = "consol number" };
			universalLine1.PlaceOfSupply = new PlaceOfSupply()
			{
				Location = new CodeDescriptionPair5Char() { Code = "NSW", Description = "New South Wales" },
				LocationType = new UniversalCodeDescriptionPair() { Code = "STA", Description = "State" }
			};

			universalLine1.SetSubAccountCollection(() =>
			{
				var subAccountCollection = new List<SubAccount>();

				subAccountCollection.Add(new SubAccount() { Code = "SEG1", Type = new UniversalCodeDescriptionPair() { Code = "SEG" } });
				subAccountCollection.Add(new SubAccount() { Code = "XXX1", Type = new UniversalCodeDescriptionPair() { Code = "XXX" } });
				subAccountCollection.Add(new SubAccount() { Code = "STR1", Type = new UniversalCodeDescriptionPair() { Code = "STR" } });
				subAccountCollection.Add(new SubAccount() { Code = "SGP1", Type = new UniversalCodeDescriptionPair() { Code = "SGP" } });
				subAccountCollection.Add(new SubAccount() { Code = "ORG1", Type = new UniversalCodeDescriptionPair() { Code = "ORG" } });
				subAccountCollection.Add(new SubAccount() { Code = "TypeNull", Type = null });
				subAccountCollection.Add(new SubAccount() { Code = "TypeCodeNull", Type = new UniversalCodeDescriptionPair() { Code = null } });

				return subAccountCollection;
			});

			var wrapper = new UniversalTransactionLineWrapper(universalLine1, null, null);
			AssertEquals("Branch", "line branch", wrapper.Branch);
			AssertEquals("Department", "line department", wrapper.Department);
			AssertEquals("ChargeCode", "mapped charge code", wrapper.ChargeCode);
			AssertEquals("ChargeCode", "charge code", wrapper.ChargeCodeSource);
			AssertEquals("GLAccount", "account number", wrapper.GLAccount);
			AssertEquals("Description", "line text", wrapper.Description);
			AssertEquals("IsFinalCharge", true, wrapper.IsFinalCharge);
			AssertEquals("Sequence", 3, wrapper.Sequence);
			AssertEquals("OSCurrency", "USD", wrapper.OSCurrency);
			AssertEquals("LocalCurrency", "AUD", wrapper.LocalCurrency);
			AssertEquals("OSAmount", -80m, wrapper.OSAmount);
			AssertEquals("OSGSTVATAmount", -10m, wrapper.OSGSTVATAmount);
			AssertEquals("LocalAmount", -40m, wrapper.LocalAmount);
			AssertEquals("LocalGSTVATAmount", -5m, wrapper.LocalGSTVATAmount);
			AssertEquals("OSWHTAmount", -6m, wrapper.OSWHTAmount);
			AssertEquals("VATTaxID", "tax", wrapper.VATTaxID);
			AssertEquals("WithholdingTaxID", "WHT tax", wrapper.WithholdingTaxID);
			AssertEquals("SubAccount", "sub account code", wrapper.SubAccount);
			AssertEquals("SubAccountType", "sub account type", wrapper.SubAccountType);
			AssertEquals("Job", "job number", wrapper.Job);
			AssertEquals("Consol", "consol number", wrapper.Consol);
			AssertEquals("PlaceOfSupply", "NSW", wrapper.PlaceOfSupply);
			AssertEquals("PlaceOfSupplyType", "STA", wrapper.PlaceOfSupplyType);
			AssertEquals("SubAccounts", "ORG: ORG1, SEG: SEG1, STR: STR1, SGP: SGP1, XXX: XXX1, : TypeNull, : TypeCodeNull", wrapper.SubAccounts);
		}

		public void TestInitialize_PlaceOfSupplyNull()
		{
			var universalLine1 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine1.PlaceOfSupply = new PlaceOfSupply()
			{
				Location = null,
				LocationType = new UniversalCodeDescriptionPair() { Code = "STA", Description = "State" }
			};

			var wrapper = new UniversalTransactionLineWrapper(universalLine1, null, null);
			AssertEquals("PlaceOfSupply", string.Empty, wrapper.PlaceOfSupply);
			AssertEquals("PlaceOfSupplyType", "STA", wrapper.PlaceOfSupplyType);
		}

		public void TestInitialize_PlaceOfSupplyTypeNull()
		{
			var universalLine1 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine1.PlaceOfSupply = new PlaceOfSupply()
			{
				Location = new CodeDescriptionPair5Char() { Code = "NSW", Description = "New South Wales" },
				LocationType = null
			};

			var wrapper = new UniversalTransactionLineWrapper(universalLine1, null, null);
			AssertEquals("PlaceOfSupply", "NSW", wrapper.PlaceOfSupply);
			AssertEquals("PlaceOfSupplyType", string.Empty, wrapper.PlaceOfSupplyType);
		}

		#region TestJobConsolXMLDataForJob

		public void TestJobConsolXMLDataForJob_EdgeCases()
		{
			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<UniversalShipment>());

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode() { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.OSAmount = -30;
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var unallocatedTransaction = TestObjectCreator.CreateTransactionPendingAllocation("INV1", TestObjectCreator.Creditor1, 100);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(unallocatedTransaction);
			Factory.Save();

			var invoice = Factory.Load<APInvoice>(unallocatedTransaction.PK);
			invoice.SubmittedFromInvoicingForm = true;
			invoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			var line = (APInvoiceLine)invoice.Lines.AddNew();
			AssertEquals("", line.JobConsolXMLData);

			request.Initialize(unallocatedTransaction, universalTransaction.Serialize(), false);
			line.IndexOfImportedUniversalTransactionLine = 0;
			Action<string> assert = expectedValue =>
				{
					AssertEquals("line", expectedValue, line.JobConsolXMLData);
					AssertEquals("unallicatedLine", expectedValue, request.PostingDetails.UniversalTransaction.Lines[0].JobConsolXMLData);
				};
			assert("");

			universalLine.Job = new EntityReference() { Key = "S001", Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			request.Initialize(unallocatedTransaction, universalTransaction.Serialize(), false);
			assert(
@"Job S001
");

			universalLine.Job = new EntityReference() { Key = "S002", Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			request.Initialize(unallocatedTransaction, universalTransaction.Serialize(), false);
			assert(
@"Job S002
");
		}

		public void TestJobConsolXMLDataForJob_JobTraget()
		{
			var shipment = TestObjectCreator.CreateShipment("S001");
			shipment.JS_HouseBill = "";
			var job = TestObjectCreator.CreateJob(shipment, createWithMutex: false);
			var invoiceToExport = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1");
			var lineToExport = TestObjectCreator.CreateInvoiceLine(invoiceToExport, job, TestObjectCreator.CC1, 100);
			TestObjectCreator.CreateCharge(lineToExport);
			Factory.Save();

			var schema = UniversalXmlSchema.Version_2011_11;
			var universalTransaction = TestObjectCreator.GetUniversalTransactionDataObject(invoiceToExport, schema);
			var universalShipment = universalTransaction.ShipmentCollection[0];
			universalShipment.DataContext.AddDataTarget(DataContextType.ForwardingConsol, "S123");

			var unallocatedTransaction = TestObjectCreator.CreateTransactionPendingAllocation("INV1", TestObjectCreator.Creditor1, 100);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(unallocatedTransaction, universalTransaction.Serialize(schema), false);
			Factory.Save();

			var invoice = Factory.Load<APInvoice>(unallocatedTransaction.PK);
			invoice.SubmittedFromInvoicingForm = true;
			invoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			var line = (APInvoiceLine)invoice.Lines.AddNew();
			line.IndexOfImportedUniversalTransactionLine = 0;
			Action<string> assert = expectedValue =>
			{
				AssertEquals("line", expectedValue, line.JobConsolXMLData);
				AssertEquals("unallicatedLine", expectedValue, request.PostingDetails.UniversalTransaction.Lines[0].JobConsolXMLData);
			};

			assert(
@"Job S001
Job Target #: S123");

			schema = UniversalXmlSchema.Version_2012_11_DO_NOT_USE;
			universalTransaction = TestObjectCreator.GetUniversalTransactionDataObject(invoiceToExport, schema);
			universalShipment = universalTransaction.ShipmentCollection[0];
			universalShipment.DataContext.AddDataTarget(DataContextType.ForwardingConsol, "S124");
			request.Initialize(unallocatedTransaction, universalTransaction.Serialize(schema), false);
			assert(
@"Job S001
Job Target #: S124");
		}

		public void TestJobConsolXMLDataForJob_HouseBill()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S001";
			shipment.JS_HouseBill = "A12345";
			Factory.Save();

			AssertJobConsolXMLDataForJob(shipment,
@"Job S001
House Bill #: A12345",
				() =>
				{
					shipment.JS_HouseBill = "A123456";
					Factory.Save();
				},
@"Job S001
House Bill #: A123456");
		}

		public void TestJobConsolXMLDataForJob_FlightVoyageAndVessel()
		{
			var consol = GetNewConsol("tvfvf1", "AUMEL", "SGSIN");
			var transport = consol.Transports[0];
			transport.JW_Vessel = "Some ship";
			transport.JW_VoyageFlight = "2222";
			Factory.Save();

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S001";
			Factory.Save();

			AssertJobConsolXMLDataForJob(shipment,
@"Job S001
Flight/Voyage # and Vessel: 2222 / Some ship",
				() =>
				{
					transport.JW_Vessel = "Another ship";
					transport.JW_VoyageFlight = "3333";
					Factory.Save();
				},
@"Job S001
Flight/Voyage # and Vessel: 3333 / Another ship");
		}

		public void TestJobConsolXMLDataForJob_CustomsEntryFromShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S001";
			var number1 = shipment.Numbers.AddNew();
			number1.CE_EntryType = "COC";
			number1.CE_EntryNum = "111";
			number1.CE_Category = "CUS";
			number1.CE_IssueDate = new ZDateTime(2007, 1, 1);
			var number2 = shipment.Numbers.AddNew();
			number2.CE_EntryType = "COC";
			number2.CE_EntryNum = "222";
			number2.CE_Category = "CUS";
			number2.CE_IssueDate = new ZDateTime(2007, 1, 2);
			Factory.Save();

			AssertJobConsolXMLDataForJob(shipment,
@"Job S001
Customs Entry #: 111, 222",
				() =>
				{
					number1.CE_EntryNum = "333";
					number2.CE_EntryNum = "444";
					Factory.Save();
				},
@"Job S001
Customs Entry #: 333, 444");
		}

		public void TestJobConsolXMLDataForJob_CustomsEntryFromDeclaration()
		{
			var declaration = Factory.NewWithValidTestData<Customs.Business.BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "D001";
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();

			var number1 = declaration.AdditionalReferenceNumbers.AddNew();
			number1.CE_ParentID = entryHeader1.PK;
			number1.CE_ParentTable = entryHeader1.TableName;
			number1.CE_EntryType = "COC";
			number1.CE_EntryNum = "333";
			number1.CE_Category = "CUS";
			number1.CE_IssueDate = new ZDateTime(2007, 1, 1);
			var number2 = declaration.AdditionalReferenceNumbers.AddNew();
			number2.CE_EntryType = "COC";
			number2.CE_EntryNum = "444";
			number2.CE_Category = "CUS";
			number2.CE_IssueDate = new ZDateTime(2007, 1, 2);
			number2.CE_ParentID = entryHeader2.PK;
			number2.CE_ParentTable = entryHeader2.TableName;
			Factory.Save();
			AssertJobConsolXMLDataForJob((IJobInvoicingPlugIn)declaration,
@"Job D001
Customs Entry #: 333, 444",
				() =>
				{
					number1.CE_EntryNum = "111";
					number2.CE_EntryNum = "222";
					Factory.Save();
				},
@"Job D001
Customs Entry #: 111, 222");
		}

		public void TestJobConsolXMLDataForJob_Order()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S001";

			var order1 = Factory.NewWithValidTestData<Order>();
			var order2 = Factory.NewWithValidTestData<Order>();
			order1.JD_JS = shipment.PK;
			order2.JD_JS = shipment.PK;
			order1.JD_OrderNumber = "909090";
			order2.JD_OrderNumber = "909091";
			Factory.Save();

			AssertJobConsolXMLDataForJob(shipment,
@"Job S001
Order #: 909090, 909091",
				() =>
				{
					order1.JD_OrderNumber = "909092";
					order2.JD_OrderNumber = "909093";
					Factory.Save();
				},
@"Job S001
Order #: 909092, 909093");
		}

		public void TestJobConsolXMLDataForJob_TransportBookingReference()
		{
			var consolidationBooking = Factory.New<IDtbBookingConsolidation>();
			var booking = Factory.New<IDtbBooking>();
			booking.KM_JobID = "TB001";
			booking.KM_KB_Booking = consolidationBooking.PK;
			booking.KM_TransportReference = "A1";
			Factory.Save();

			AssertJobConsolXMLDataForJob((IJobInvoicingPlugIn)booking,
@"Job TB001
Transport Booking Reference: A1",
				() =>
				{
					booking.KM_TransportReference = "A2";
					Factory.Save();
				},
@"Job TB001
Transport Booking Reference: A2");
		}

		public void TestJobConsolXMLDataForJob_Container()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "A123";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "B123";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_IsForwardRegistered = true;
			shipment.JS_UniqueConsignRef = "S001";

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container);
			packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container2);
			packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container);
			packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container2);

			Factory.Save();

			AssertJobConsolXMLDataForJob(shipment,
@"Job S001
Container #: A123, B123",
				() =>
				{
					container.JC_ContainerNum = "C123";
					container2.JC_ContainerNum = "D123";
					Factory.Save();
				},
@"Job S001
Container #: C123, D123");
		}

		public void TestJobHeader()
		{
			var shipmentNumber = "S001";

			var shipment = TestObjectCreator.CreateShipment(shipmentNumber, false);
			TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			Factory.Save();

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<UniversalShipment>());

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddress.AddressType = nameof(MasterFiles.Integration.DocAddressType.None);
			orgAddress.OrganizationCode = TestObjectCreator.AALSHI.OH_Code;
			universalTransaction.OrganizationAddress = orgAddress;

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.Branch = new Branch { Code = GlbBranch.CurrentBranch.GB_Code };
			universalLine.Department = new Department { Code = GlbDepartment.CurrentDepartment.GE_Code };
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC3.AC_Code };
			universalLine.Description = "Some line text";
			universalLine.IsFinalCharge = true;
			universalLine.Sequence = 3;
			universalLine.OSCurrency = new Currency { Code = "AUD" };
			universalLine.OSAmount = -100M;
			universalLine.OSGSTVATAmount = -100M;
			universalLine.OSTotalAmount = universalLine.OSAmount + universalLine.OSGSTVATAmount;
			universalLine.Job = new EntityReference { Key = shipmentNumber, Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, shipmentNumber);
			universalShipment.DataContext.AddDataTarget(DataContextType.ForwardingShipment, shipmentNumber);
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var schema = UniversalXmlSchema.Version_2011_11;
			var unallocatedTransaction = TestObjectCreator.CreateTransactionPendingAllocation("INV1", TestObjectCreator.AALSHI, 100);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(unallocatedTransaction, universalTransaction.Serialize(schema), false);

			AssertNotNull(request.PostingDetails.UniversalTransaction.Lines[0].JobHeader);
			AssertEquals(shipmentNumber, request.PostingDetails.UniversalTransaction.Lines[0].JobHeader.JH_JobNum);
		}

		public void TestJobConsol()
		{
			var consolNumber = "C001";
			var shipmentNumber = "S001";

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", consolNumber);
			consol.JK_MasterBillNum = consolNumber;
			var shipment = TestObjectCreator.CreateShipment(shipmentNumber, consol);
			shipment.JS_HouseBill = shipmentNumber;
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			var apportionemntListing = new ApportionmentListing(Factory, consol);
			var consolCost = apportionemntListing.CostsCollection.TryAddNew();
			consolCost.E6_AC_ChargeCode = TestObjectCreator.CC3.PK;
			consolCost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
			consolCost.E6_ApportionmentMethod = "SHP";
			consolCost.E6_OSCostAmount = 100M;

			Factory.Save();

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<UniversalShipment>());

			universalTransaction.JobInvoiceNumber = "Consol1/AA";

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddress.AddressType = nameof(MasterFiles.Integration.DocAddressType.None);
			orgAddress.OrganizationCode = TestObjectCreator.AALSHI.OH_Code;
			universalTransaction.OrganizationAddress = orgAddress;

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC3.AC_Code };
			universalLine.IsFinalCharge = true;
			universalLine.OSCurrency = new Currency { Code = "AUD" };
			universalLine.OSAmount = -100M;
			universalLine.OSGSTVATAmount = -100M;
			universalLine.OSTotalAmount = universalLine.OSAmount + universalLine.OSGSTVATAmount;
			universalLine.Job = new EntityReference { Key = shipmentNumber, Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			universalLine.CostSource = new EntityReference { Key = consolNumber, Type = nameof(DataContextType.ForwardingConsol) };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, shipmentNumber);
			universalShipment.DataContext.AddDataTarget(DataContextType.ForwardingShipment, shipmentNumber);
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			universalShipment.WayBillNumber = shipmentNumber;
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var universalConsol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalConsol.DataContext = DataContextFactory.New();
			universalConsol.DataContext.AddDataSource(DataContextType.ForwardingConsol, consolNumber);
			universalConsol.DataContext.AddDataTarget(DataContextType.ForwardingConsol, consolNumber);
			universalConsol.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			universalTransaction.ShipmentCollection.Add(universalConsol);

			var schema = UniversalXmlSchema.Version_2011_11;
			var unallocatedTransaction = TestObjectCreator.CreateTransactionPendingAllocation("INV1", TestObjectCreator.AALSHI, 100);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(unallocatedTransaction, universalTransaction.Serialize(schema), false);

			AssertNotNull(request.PostingDetails.UniversalTransaction.Lines[0].JobConsol);
			AssertEquals(consolNumber, request.PostingDetails.UniversalTransaction.Lines[0].JobConsol.JK_UniqueConsignRef);
		}

		void AssertJobConsolXMLDataForJob<T>(T savedTestBizo, string expectedJobConsolXMLDataValue, Action setAnotherTestValueAndSaveShipment, string anoterExpectedJobConsolXMLDataValue)
			where T : class, IJobInvoicingPlugIn
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			var testObjectCreatorInNewFactory = new TestObjectCreator(newFactory);
			var testBizoInNewFactory = newFactory.Load(savedTestBizo.GetType(), savedTestBizo.PK) as T;
			var job = testObjectCreatorInNewFactory.CreateJob(testBizoInNewFactory, createWithMutex: false);
			var invoiceToExport = testObjectCreatorInNewFactory.CreateInvoice(typeof(APInvoice), organisation: testObjectCreatorInNewFactory.AALSHI);
			invoiceToExport.FillWithValidTestData();
			var lineToExport = testObjectCreatorInNewFactory.CreateInvoiceLine(invoiceToExport, job, testObjectCreatorInNewFactory.CC1, 100);
			testObjectCreatorInNewFactory.CreateCharge(lineToExport);
			newFactory.Save();

			var schema = UniversalXmlSchema.Version_2011_11;
			var universalTransaction = testObjectCreatorInNewFactory.GetUniversalTransactionDataObject(invoiceToExport, schema);
			var unallocatedTransaction = testObjectCreatorInNewFactory.CreateTransactionPendingAllocation("INV1", testObjectCreatorInNewFactory.AALSHI, 100);
			var request = newFactory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(unallocatedTransaction, universalTransaction.Serialize(schema), false);
			newFactory.Save();

			var invoice = newFactory.Load<APInvoice>(unallocatedTransaction.PK);
			invoice.SubmittedFromInvoicingForm = true;
			invoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			var line = (APInvoiceLine)invoice.Lines.AddNew();
			line.IndexOfImportedUniversalTransactionLine = 0;
			Action<string> assert = expectedValue =>
			{
				AssertEquals("line", expectedValue, line.JobConsolXMLData);
				AssertEquals("unallicatedLine", expectedValue, request.PostingDetails.UniversalTransaction.Lines[0].JobConsolXMLData);
			};

			assert(expectedJobConsolXMLDataValue);

			setAnotherTestValueAndSaveShipment();

			schema = UniversalXmlSchema.Version_2012_11_DO_NOT_USE;
			universalTransaction = TestObjectCreator.GetUniversalTransactionDataObject(invoiceToExport, schema);
			request.Initialize(unallocatedTransaction, universalTransaction.Serialize(schema), false);
			assert(anoterExpectedJobConsolXMLDataValue);
		}

		#endregion

		#region TestJobConsolXMLDataForConsol

		public void TestJobConsolXMLDataForConsol_EdgeCases()
		{
			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<UniversalShipment>());

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode() { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.OSAmount = -30;
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var unallocatedTransaction = TestObjectCreator.CreateTransactionPendingAllocation("INV1", TestObjectCreator.Creditor1, 100);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(unallocatedTransaction);
			Factory.Save();

			var invoice = Factory.Load<APInvoice>(unallocatedTransaction.PK);
			invoice.SubmittedFromInvoicingForm = true;
			invoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			var line = (APInvoiceLine)invoice.Lines.AddNew();
			AssertEquals("", line.JobConsolXMLData);

			request.Initialize(unallocatedTransaction, universalTransaction.Serialize(), false);
			line.IndexOfImportedUniversalTransactionLine = 0;
			Action<string> assert = expectedValue =>
			{
				AssertEquals("line", expectedValue, line.JobConsolXMLData);
				AssertEquals("unallicatedLine", expectedValue, request.PostingDetails.UniversalTransaction.Lines[0].JobConsolXMLData);
			};
			assert("");

			universalLine.CostSource = new EntityReference() { Key = "C001", Type = nameof(DataContextType.ForwardingConsol) };
			request.Initialize(unallocatedTransaction, universalTransaction.Serialize(), false);
			assert(
@"Consol C001
");

			universalLine.CostSource = new EntityReference() { Key = "C002", Type = nameof(DataContextType.ForwardingConsol) };
			request.Initialize(unallocatedTransaction, universalTransaction.Serialize(), false);
			assert(
@"Consol C002
");
		}

		public void TestJobConsolXMLDataForConsol_ConsolTarget()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C001";
			var invoiceToExport = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1");
			TestObjectCreator.CreateInvoiceLine(invoiceToExport, TestObjectCreator.GLHeader1.PK, 100);
			Factory.Save();

			var schema = UniversalXmlSchema.Version_2011_11;
			var universalTransaction = TestObjectCreator.GetUniversalTransactionDataObject(invoiceToExport, schema);
			var universalConsol = TestObjectCreator.GetUniversalShipmentDataObject(consol, schema, consol.JK_UniqueConsignRef);
			universalConsol.DataContext.AddDataTarget(DataContextType.ForwardingConsol, "C123");
			universalTransaction.SetShipmentCollection(() => new List<UniversalShipment>());
			universalTransaction.ShipmentCollection.Add(universalConsol);
			universalTransaction.PostingJournalCollection[0].CostSource = new EntityReference() { Key = "C001", Type = nameof(DataContextType.ForwardingConsol) };
			var unallocatedTransaction = TestObjectCreator.CreateTransactionPendingAllocation("INV1", TestObjectCreator.Creditor1, 100);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(unallocatedTransaction, universalTransaction.Serialize(schema), false);
			Factory.Save();

			var invoice = Factory.Load<APInvoice>(unallocatedTransaction.PK);
			invoice.SubmittedFromInvoicingForm = true;
			invoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			var line = (APInvoiceLine)invoice.Lines.AddNew();
			line.IndexOfImportedUniversalTransactionLine = 0;
			Action<string> assert = expectedValue =>
			{
				AssertEquals("line", expectedValue, line.JobConsolXMLData);
				AssertEquals("unallicatedLine", expectedValue, request.PostingDetails.UniversalTransaction.Lines[0].JobConsolXMLData);
			};

			assert(
@"Consol C001
Consol Target #: C123");

			schema = UniversalXmlSchema.Version_2012_11_DO_NOT_USE;
			universalTransaction = TestObjectCreator.GetUniversalTransactionDataObject(invoiceToExport, schema);
			universalConsol = TestObjectCreator.GetUniversalShipmentDataObject(consol, schema, consol.JK_UniqueConsignRef);
			universalConsol.DataContext.AddDataTarget(DataContextType.ForwardingConsol, "C124");
			universalTransaction.SetShipmentCollection(() => new List<UniversalShipment>());
			universalTransaction.ShipmentCollection.Add(universalConsol);
			universalTransaction.PostingJournalCollection[0].CostSource = new EntityReference() { Key = "C001", Type = nameof(DataContextType.ForwardingConsol) };
			request.Initialize(unallocatedTransaction, universalTransaction.Serialize(schema), false);
			assert(
@"Consol C001
Consol Target #: C124");
		}

		public void TestJobConsolXMLDataForConsol_BookingReference()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C001";
			consol.JK_BookingReference = "1234567890";
			consol.JK_AgentType = Enterprise.Core.Constants.AgentType.Direct;
			Factory.Save();

			AssertJobConsolXMLDataForConsol(consol,
@"Consol C001
Booking Reference #: 1234567890",
				() =>
				{
					consol.JK_BookingReference = "0987654321";
					Factory.Save();
				},
@"Consol C001
Booking Reference #: 0987654321");
		}

		public void TestJobConsolXMLDataForConsol_Container()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C001";
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "AAA";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "BBB";
			Factory.Save();

			AssertJobConsolXMLDataForConsol(consol,
@"Consol C001
Container #: AAA, BBB",
				() =>
				{
					container1.JC_ContainerNum = "CCC";
					container2.JC_ContainerNum = "DDD";
					Factory.Save();
				},
@"Consol C001
Container #: CCC, DDD");
		}

		public void TestJobConsolXMLDataForConsol_CoLoadMasterBill()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C001";
			consol.JK_BookingReference = "1234567890";
			consol.JK_AgentType = Enterprise.Core.Constants.AgentType.CoLoad;
			Factory.Save();

			AssertJobConsolXMLDataForConsol(consol,
@"Consol C001
Co-Load Master Bill #: 1234567890",
				() =>
				{
					consol.JK_BookingReference = "0987654321";
					Factory.Save();
				},
@"Consol C001
Co-Load Master Bill #: 0987654321");
		}

		public void TestJobConsolXMLDataForConsol_MasterBill()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C001";
			consol.JK_MasterBillNum = "1234567890";
			Factory.Save();

			AssertJobConsolXMLDataForConsol(consol,
@"Consol C001
Master Bill: 1234567890",
				() =>
				{
					consol.JK_MasterBillNum = "0987654321";
					Factory.Save();
				},
@"Consol C001
Master Bill: 0987654321");
		}

		public void TestJobConsolXMLDataForConsol_FlightVoyageAndVessel()
		{
			var consol = GetNewConsol("C001", "AUMEL", "SGSIN");
			var transport = consol.Transports[0];
			transport.JW_Vessel = "Some ship";
			transport.JW_VoyageFlight = "2222";
			Factory.Save();

			AssertJobConsolXMLDataForConsol(consol,
@"Consol C001
Flight/Voyage # and Vessel: 2222 / Some ship",
				() =>
				{
					transport.JW_Vessel = "Another ship";
					transport.JW_VoyageFlight = "333";
					Factory.Save();
				},
@"Consol C001
Flight/Voyage # and Vessel: 333 / Another ship");
		}

		public void TestJobConsolXMLDataForConsol_ETA()
		{
			var consol = GetNewConsol("C001", "USNYC", "SGSIN", "AUSYD", "NZAKA");
			consol.Transports[0][JobConsolTransportSchema.JW_ETA.Name] = new ZDateTime(2007, 1, 1, 10, 20, 30);
			consol.Transports[1][JobConsolTransportSchema.JW_ETA.Name] = new ZDateTime(2007, 1, 3, 10, 20, 30);
			Factory.Save();

			AssertJobConsolXMLDataForConsol(consol,
@"Consol C001
ETA: 01-Jan-07 10:20, 03-Jan-07 10:20",
				() =>
				{
					consol.Transports[2][JobConsolTransportSchema.JW_ETA.Name] = new ZDateTime(2007, 1, 5, 10, 20, 30);
					Factory.Save();
				},
@"Consol C001
ETA: 01-Jan-07 10:20, 03-Jan-07 10:20, 05-Jan-07 10:20");
		}

		public void TestJobConsolXMLDataForConsol_ATA()
		{
			var consol = GetNewConsol("C001", "USNYC", "SGSIN", "AUSYD", "NZAKA");
			consol.Transports[0][JobConsolTransportSchema.JW_ATA.Name] = new ZDateTime(2007, 1, 1, 10, 20, 30);
			consol.Transports[2][JobConsolTransportSchema.JW_ATA.Name] = new ZDateTime(2007, 1, 5, 10, 20, 30);
			Factory.Save();

			AssertJobConsolXMLDataForConsol(consol,
@"Consol C001
ATA: 01-Jan-07 10:20, 05-Jan-07 10:20",
				() =>
				{
					consol.Transports[1][JobConsolTransportSchema.JW_ATA.Name] = new ZDateTime(2007, 1, 3, 10, 20, 30);
					Factory.Save();
				},
@"Consol C001
ATA: 01-Jan-07 10:20, 03-Jan-07 10:20, 05-Jan-07 10:20");
		}

		public void TestJobConsolXMLDataForConsol_ETD()
		{
			var consol = GetNewConsol("C001", "USNYC", "SGSIN", "AUSYD", "NZAKA");
			consol.Transports[1][JobConsolTransportSchema.JW_ETD.Name] = new ZDateTime(2007, 1, 3, 10, 20, 30);
			Factory.Save();

			AssertJobConsolXMLDataForConsol(consol,
@"Consol C001
ETD: 03-Jan-07 10:20",
				() =>
				{
					consol.Transports[0][JobConsolTransportSchema.JW_ETD.Name] = new ZDateTime(2007, 1, 1, 10, 20, 30);
					consol.Transports[2][JobConsolTransportSchema.JW_ETD.Name] = new ZDateTime(2007, 1, 5, 10, 20, 30);
					Factory.Save();
				},
@"Consol C001
ETD: 01-Jan-07 10:20, 03-Jan-07 10:20, 05-Jan-07 10:20");
		}

		public void TestJobConsolXMLDataForConsol_ATD()
		{
			var consol = GetNewConsol("C001", "USNYC", "SGSIN", "AUSYD", "NZAKA");
			consol.Transports[0][JobConsolTransportSchema.JW_ATD.Name] = new ZDateTime(2007, 1, 1, 10, 20, 30);
			consol.Transports[1][JobConsolTransportSchema.JW_ATD.Name] = new ZDateTime(2007, 1, 3, 10, 20, 30);
			consol.Transports[2][JobConsolTransportSchema.JW_ATD.Name] = new ZDateTime(2007, 1, 5, 10, 20, 30);
			Factory.Save();

			AssertJobConsolXMLDataForConsol(consol,
@"Consol C001
ATD: 01-Jan-07 10:20, 03-Jan-07 10:20, 05-Jan-07 10:20",
				() =>
				{
					consol.Transports[0][JobConsolTransportSchema.JW_ATD.Name] = ZDateTime.Empty;
					consol.Transports[1][JobConsolTransportSchema.JW_ATD.Name] = ZDateTime.Empty;
					Factory.Save();
				},
@"Consol C001
ATD: 05-Jan-07 10:20");
		}

		ForwardingConsol GetNewConsol(string number, string port1, string port2, params string[] otherports)
		{
			var consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			consol.JK_UniqueConsignRef = number;
			consol.JK_RL_NKLoadPort = port1;
			consol.JK_RL_NKDischargePort = (otherports.Length == 0 ? port2 : otherports[otherports.Length - 1]);

			var lastTransport = consol.Transports[0];
			lastTransport.JW_RL_NKLoadPort = port1;
			lastTransport.JW_RL_NKDiscPort = port2;

			foreach (string nextPort in otherports)
			{
				string lastPort = lastTransport.JW_RL_NKDiscPort;
				lastTransport = consol.Transports.AddNew();
				lastTransport.JW_RL_NKLoadPort = lastPort;
				lastTransport.JW_RL_NKDiscPort = nextPort;
			}

			return consol;
		}

		void AssertJobConsolXMLDataForConsol(ForwardingConsol savedTestConsol, string expectedJobConsolXMLDataValue, Action setAnotherTestValueAndSaveShipment, string anoterExpectedJobConsolXMLDataValue)
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			var testObjectCreatorInNewFactory = new TestObjectCreator(newFactory);
			var testConsolInNewFactory = newFactory.Load<ForwardingConsol>(savedTestConsol.PK);
			var invoiceToExport = testObjectCreatorInNewFactory.CreateInvoice(typeof(APInvoice), organisation: testObjectCreatorInNewFactory.AALSHI);
			invoiceToExport.FillWithValidTestData();
			testObjectCreatorInNewFactory.CreateInvoiceLine(invoiceToExport, testObjectCreatorInNewFactory.GLHeader1.PK, 100);
			newFactory.Save();

			var schema = UniversalXmlSchema.Version_2011_11;
			var universalTransaction = testObjectCreatorInNewFactory.GetUniversalTransactionDataObject(invoiceToExport, schema);
			var universalConsol = TestObjectCreator.GetUniversalShipmentDataObject(testConsolInNewFactory, schema, testConsolInNewFactory.JK_UniqueConsignRef);
			universalTransaction.SetShipmentCollection(() => new List<UniversalShipment>());
			universalTransaction.ShipmentCollection.Add(universalConsol);
			universalTransaction.PostingJournalCollection[0].CostSource = new EntityReference() { Key = testConsolInNewFactory.JK_UniqueConsignRef, Type = nameof(DataContextType.ForwardingConsol) };
			var unallocatedTransaction = testObjectCreatorInNewFactory.CreateTransactionPendingAllocation("INV1", testObjectCreatorInNewFactory.AALSHI, 100);
			var request = newFactory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(unallocatedTransaction, universalTransaction.Serialize(schema), false);
			newFactory.Save();

			var invoice = newFactory.Load<APInvoice>(unallocatedTransaction.PK);
			invoice.SubmittedFromInvoicingForm = true;
			invoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			var line = (APInvoiceLine)invoice.Lines.AddNew();
			line.IndexOfImportedUniversalTransactionLine = 0;
			Action<string> assert = expectedValue =>
			{
				AssertEquals("line", expectedValue, line.JobConsolXMLData);
				AssertEquals("unallicatedLine", expectedValue, request.PostingDetails.UniversalTransaction.Lines[0].JobConsolXMLData);
			};

			assert(expectedJobConsolXMLDataValue);

			setAnotherTestValueAndSaveShipment();

			schema = UniversalXmlSchema.Version_2012_11_DO_NOT_USE;
			universalTransaction = TestObjectCreator.GetUniversalTransactionDataObject(invoiceToExport, schema);
			universalConsol = TestObjectCreator.GetUniversalShipmentDataObject(testConsolInNewFactory, schema, testConsolInNewFactory.JK_UniqueConsignRef);
			universalTransaction.SetShipmentCollection(() => new List<UniversalShipment>());
			universalTransaction.ShipmentCollection.Add(universalConsol);
			universalTransaction.PostingJournalCollection[0].CostSource = new EntityReference() { Key = testConsolInNewFactory.JK_UniqueConsignRef, Type = nameof(DataContextType.ForwardingConsol) };
			request.Initialize(unallocatedTransaction, universalTransaction.Serialize(schema), false);
			assert(anoterExpectedJobConsolXMLDataValue);
		}

		#endregion

		public void TestJobConsolXMLData()
		{
			var consol = GetNewConsol("C001", "USNYC", "SGSIN", "AUSYD", "NZAKA");
			consol.JK_MasterBillNum = "2323";
			consol.JK_BookingReference = "1234567890";
			consol.JK_AgentType = Enterprise.Core.Constants.AgentType.Direct;
			consol.Transports[0][JobConsolTransportSchema.JW_ETA.Name] = new ZDateTime(2007, 1, 1, 10, 20, 30);
			consol.Transports[1][JobConsolTransportSchema.JW_ETA.Name] = new ZDateTime(2007, 1, 3, 10, 20, 30);
			consol.Transports[0][JobConsolTransportSchema.JW_ATA.Name] = new ZDateTime(2007, 2, 1, 10, 20, 30);
			consol.Transports[2][JobConsolTransportSchema.JW_ATA.Name] = new ZDateTime(2007, 2, 5, 10, 20, 30);
			consol.Transports[1][JobConsolTransportSchema.JW_ETD.Name] = new ZDateTime(2006, 1, 3, 10, 20, 30);
			consol.Transports[0][JobConsolTransportSchema.JW_ATD.Name] = new ZDateTime(2006, 3, 1, 10, 20, 30);
			consol.Transports[1][JobConsolTransportSchema.JW_ATD.Name] = new ZDateTime(2006, 3, 3, 10, 20, 30);
			consol.Transports[2][JobConsolTransportSchema.JW_ATD.Name] = new ZDateTime(2006, 3, 5, 10, 20, 30);

			var transport = consol.Transports[0];
			transport.JW_Vessel = "Some ship";
			transport.JW_VoyageFlight = "2222";
			Factory.Save();

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S001";
			shipment.JS_HouseBill = "A12345";
			shipment.JS_IsForwardRegistered = true;

			var number1 = shipment.Numbers.AddNew();
			number1.CE_EntryType = "COC";
			number1.CE_EntryNum = "111";
			number1.CE_Category = "CUS";
			number1.CE_IssueDate = new ZDateTime(2007, 1, 1);
			var number2 = shipment.Numbers.AddNew();
			number2.CE_EntryType = "COC";
			number2.CE_EntryNum = "222";
			number2.CE_Category = "CUS";
			number2.CE_IssueDate = new ZDateTime(2007, 1, 2);

			var order1 = Factory.NewWithValidTestData<Order>();
			var order2 = Factory.NewWithValidTestData<Order>();
			order1.JD_JS = shipment.PK;
			order2.JD_JS = shipment.PK;
			order1.JD_OrderNumber = "909090";
			order2.JD_OrderNumber = "909091";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "A123";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "B123";

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container);
			packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container2);
			packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container);
			packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container2);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			var testObjectCreatorInNewFactory = new TestObjectCreator(newFactory);
			var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
			var consolInNewFactory = newFactory.Load<ForwardingConsol>(consol.PK);
			var job = testObjectCreatorInNewFactory.CreateJob(shipmentInNewFactory, createWithMutex: false);
			var invoiceToExport = testObjectCreatorInNewFactory.CreateInvoice(typeof(APInvoice), organisation: testObjectCreatorInNewFactory.AALSHI);
			invoiceToExport.FillWithValidTestData();
			var lineToExport = testObjectCreatorInNewFactory.CreateInvoiceLine(invoiceToExport, job, testObjectCreatorInNewFactory.CC1, 100);
			testObjectCreatorInNewFactory.CreateCharge(lineToExport);
			newFactory.Save();

			var schema = UniversalXmlSchema.Version_2011_11;
			var universalTransaction = testObjectCreatorInNewFactory.GetUniversalTransactionDataObject(invoiceToExport, schema);
			var universalConsol = TestObjectCreator.GetUniversalShipmentDataObject(consolInNewFactory, schema, consolInNewFactory.JK_UniqueConsignRef);
			universalTransaction.ShipmentCollection.Add(universalConsol);
			universalTransaction.PostingJournalCollection[0].CostSource = new EntityReference() { Key = consolInNewFactory.JK_UniqueConsignRef, Type = nameof(DataContextType.ForwardingConsol) };
			var unallocatedTransaction = testObjectCreatorInNewFactory.CreateTransactionPendingAllocation("INV1", testObjectCreatorInNewFactory.AALSHI, 100);
			var request = newFactory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(unallocatedTransaction, universalTransaction.Serialize(schema), false);
			newFactory.Save();

			var invoice = newFactory.Load<APInvoice>(unallocatedTransaction.PK);
			invoice.SubmittedFromInvoicingForm = true;
			invoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			var line = (APInvoiceLine)invoice.Lines.AddNew();
			line.IndexOfImportedUniversalTransactionLine = 0;
			Action<string> assert = expectedValue =>
			{
				AssertEquals("line", expectedValue, line.JobConsolXMLData);
				AssertEquals("unallicatedLine", expectedValue, request.PostingDetails.UniversalTransaction.Lines[0].JobConsolXMLData);
			};

			assert(
@"Job S001
House Bill #: A12345
Flight/Voyage # and Vessel: 2222 / Some ship
Customs Entry #: 111, 222
Order #: 909090, 909091
Container #: A123, B123

Consol C001
Booking Reference #: 1234567890
Container #: A123, B123
Master Bill: 2323
Flight/Voyage # and Vessel: 2222 / Some ship
ETA: 01-Jan-07 10:20, 03-Jan-07 10:20
ATA: 01-Feb-07 10:20, 05-Feb-07 10:20
ETD: 03-Jan-06 10:20
ATD: 01-Mar-06 10:20, 03-Mar-06 10:20, 05-Mar-06 10:20");

			consol.JK_MasterBillNum = "3232";
			consol.JK_BookingReference = "0987654321";
			consol.Transports[2][JobConsolTransportSchema.JW_ETA.Name] = new ZDateTime(2007, 1, 5, 10, 20, 30);
			consol.Transports[1][JobConsolTransportSchema.JW_ATA.Name] = new ZDateTime(2007, 2, 3, 10, 20, 30);
			consol.Transports[0][JobConsolTransportSchema.JW_ETD.Name] = new ZDateTime(2006, 1, 1, 10, 20, 30);
			consol.Transports[2][JobConsolTransportSchema.JW_ETD.Name] = new ZDateTime(2006, 1, 5, 10, 20, 30);
			consol.Transports[0][JobConsolTransportSchema.JW_ATD.Name] = ZDateTime.Empty;
			consol.Transports[1][JobConsolTransportSchema.JW_ATD.Name] = ZDateTime.Empty;

			transport.JW_Vessel = "Another ship";
			transport.JW_VoyageFlight = "3333";
			shipment.JS_HouseBill = "A123456";
			number1.CE_EntryNum = "333";
			number2.CE_EntryNum = "444";
			order1.JD_OrderNumber = "909092";
			order2.JD_OrderNumber = "909093";
			container.JC_ContainerNum = "C123";
			container2.JC_ContainerNum = "D123";
			Factory.Save();

			schema = UniversalXmlSchema.Version_2012_11_DO_NOT_USE;
			universalTransaction = TestObjectCreator.GetUniversalTransactionDataObject(invoiceToExport, schema);
			universalConsol = TestObjectCreator.GetUniversalShipmentDataObject(consolInNewFactory, schema, consolInNewFactory.JK_UniqueConsignRef);
			universalTransaction.ShipmentCollection.Add(universalConsol);
			universalTransaction.PostingJournalCollection[0].CostSource = new EntityReference() { Key = consolInNewFactory.JK_UniqueConsignRef, Type = nameof(DataContextType.ForwardingConsol) };
			request.Initialize(unallocatedTransaction, universalTransaction.Serialize(schema), false);
			assert(
@"Job S001
House Bill #: A123456
Flight/Voyage # and Vessel: 3333 / Another ship
Customs Entry #: 333, 444
Order #: 909092, 909093
Container #: C123, D123

Consol C001
Booking Reference #: 0987654321
Container #: C123, D123
Master Bill: 3232
Flight/Voyage # and Vessel: 3333 / Another ship
ETA: 01-Jan-07 10:20, 03-Jan-07 10:20, 05-Jan-07 10:20
ATA: 01-Feb-07 10:20, 03-Feb-07 10:20, 05-Feb-07 10:20
ETD: 01-Jan-06 10:20, 03-Jan-06 10:20, 05-Jan-06 10:20
ATD: 05-Mar-06 10:20");
		}

		void setupControlAccounts()
		{
			var arSuspenseControlAccount = TestObjectCreator.CreateARSuspenseControlAccount();
			var apSuspenseControlAccount = TestObjectCreator.CreateAPSuspenseControlAccount();
			var jobRevenueJournalControlAccount = TestObjectCreator.CreateJobRevenueJournalControlAccount();
			var cfxAccount = TestObjectCreator.CreateCFXAccount();
			var pendingInputTaxAccount = TestObjectCreator.CreateInputTaxReceivablePendingAccount();
			var pendingOutputTaxAccount = TestObjectCreator.CreateOutputTaxPayablePendingAccount();
			Factory.Save();

			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, arSuspenseControlAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, apSuspenseControlAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, jobRevenueJournalControlAccount.PK.ToGuid());
			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CES"));
			AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(Guid.Empty, Guid.Empty, department.PK.ToGuid(), cfxAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pendingInputTaxAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pendingOutputTaxAccount.PK.ToGuid());
		}

		protected override void SetUp()
		{
			base.SetUp();

			setupControlAccounts();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UniversalTransactionLineWrapper(null, null, null);
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
