using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APInvoiceChargesApprovalRequest))]
	public class APInvoiceChargesApprovalRequestTest : InvoicingBaseApprovalRequestTest<APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails>
	{
		public void TestCodeAndDescriptionProperty()
		{
			var request = Factory.NewWithValidTestData<APInvoiceChargesApprovalRequest>();
			request.XP_RequestID = "CODE123";
			request.XP_ReasonDescription = "Test Description";

			CombineAssertions(() =>
			{
				AssertEquals(APInvoiceChargesApprovalRequest.Schema.XP_RequestID, CodePropertyAttribute.CodePropertyNameFromType(typeof(APInvoiceChargesApprovalRequest)));
				AssertEquals(APInvoiceChargesApprovalRequest.Schema.XP_ReasonDescription, DescriptionPropertyAttribute.DescriptionPropertyNameFromType(typeof(APInvoiceChargesApprovalRequest)));

				AssertEquals(request.XP_RequestID, CodePropertyAttribute.CodeFromBusinessObject(request));
				AssertEquals(request.XP_ReasonDescription, DescriptionPropertyAttribute.DescriptionFromBusinessObject(request));
			});
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesAPInvoiceChargesApprovalRequest()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1", organisation: TestObjectCreator.Creditor1);
			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.GLHeader1.PK, 100);
			var request = Factory.New<APInvoiceChargesApprovalRequest>();
			request.InitializeInvoiceRelated(invoice);
			AssertNotNull("Company should not be null", invoice.Company);

			var localList = new List<string>
			{
				nameof(request.InvoiceLocalTotalAmount),
				nameof(request.InvoiceLocalTaxAmount),
				nameof(request.InvoiceLocalExTaxAmount)
			};

			var osList = new List<string>
			{
				nameof(request.InvoiceOSTotalAmount)
			};

			var tester = new DecimalPlacesAttributeTester(request, invoice.Company);
			tester.CheckLocalCurrency(localList, nameof(request.LocalDecimals));
			tester.CheckNonLocalCurrency(osList, nameof(request.OSDecimals), nameof(invoice.AH_RX_NKTransactionCurrency), invoice);
		}

		public override void TestIsPostingActionTheSame()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1", organisation: TestObjectCreator.Creditor1);
			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.GLHeader1.PK, 100);
			TestApprovalRequest.InitializeInvoiceRelated(invoice);

			var request2 = Factory.New<APInvoiceChargesApprovalRequest>();
			request2.InitializeInvoiceRelated(invoice);
			Assert("The same invoice, the same posting action.", TestApprovalRequest.IsPostingActionTheSame(request2));

			invoice.AH_TransactionNum += "2";
			request2.InitializeInvoiceRelated(invoice);
			Assert("Invoice number change doesn't make invoice related posting action different,", TestApprovalRequest.IsPostingActionTheSame(request2));

			invoice.AH_OH = TestObjectCreator.Creditor2.PK;
			request2.InitializeInvoiceRelated(invoice);
			Assert("Creditor change doesn't make invoice related posting action different,", TestApprovalRequest.IsPostingActionTheSame(request2));

			var jobPK = ZGuid.NewZGuid();
			var invoiceCharges = new APInvoiceCharges("Creditor1", "INV1", jobPK, "JH", null);
			TestApprovalRequest.InitializeJobRelated(invoiceCharges, jobPK, "JH");
			request2.InitializeJobRelated(invoiceCharges, jobPK, "JH");
			Assert("The same invoice charges, the same posting action.", TestApprovalRequest.IsPostingActionTheSame(request2));

			invoiceCharges = new APInvoiceCharges("Creditor1", "INV2", jobPK, "JH", null);
			request2.InitializeJobRelated(invoiceCharges, jobPK, "JH");
			Assert("Invoice number change makes job related posting action different,", !TestApprovalRequest.IsPostingActionTheSame(request2));

			invoiceCharges = new APInvoiceCharges("Creditor2", "INV1", jobPK, "JH", null);
			request2.InitializeJobRelated(invoiceCharges, jobPK, "JH");
			Assert("Creditor change makes job related posting action different,", !TestApprovalRequest.IsPostingActionTheSame(request2));

			invoiceCharges = new APInvoiceCharges("Creditor1", "INV1", jobPK, "JH", null);
			request2.InitializeJobRelated(invoiceCharges, jobPK, "JH");
			Assert("The same Creditor and Number make job related posting action the same.", TestApprovalRequest.IsPostingActionTheSame(request2));
		}

		public override void TestPostingOptionForDisplay()
		{
			foreach (JobInvoicingPostingOption x in Enum.GetValues(typeof(JobInvoicingPostingOption)))
			{
				TestApprovalRequest.PostingDetails.PostingOption = x.ToString();
				AssertEquals(AccountingUtils.ConvertPostingOptionToHumanReadableName(x, TestApprovalRequest.IsConsolRelated), TestApprovalRequest.PostingOptionForDisplay);
			}
		}

		protected override ZString ExpectedApprovalRequestID()
		{
			return "00001000";
		}

		protected override bool ShouldSetRequestIDFromNumberFountain
		{
			get { return true; }
		}

		public void TestTransactionType()
		{
			var statusesWithoutPosting = typeof(Constants.GenApprovalRequestApprovalStatus).GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (string)x.GetValue(null)).
				Where(x => x != Constants.GenApprovalRequestApprovalStatus.Posted).ToArray();

			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C001");
			var job = TestObjectCreator.CreateJob("S001", TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			TestApprovalRequest.XP_ParentID = job.PK;
			TestApprovalRequest.XP_ParentTableCode = job.TablePrefix;
			foreach (var status in statusesWithoutPosting)
			{
				TestApprovalRequest.XP_ApprovalStatus = status;
				AssertEquals("UAI", TestApprovalRequest.TransactionType);
			}
			TestApprovalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;
			AssertEquals("INV", TestApprovalRequest.TransactionType);

			TestApprovalRequest.XP_ParentID = consol.PK;
			TestApprovalRequest.XP_ParentTableCode = consol.TablePrefix;
			foreach (var status in statusesWithoutPosting)
			{
				TestApprovalRequest.XP_ApprovalStatus = status;
				AssertEquals("UAI", TestApprovalRequest.TransactionType);
			}
			TestApprovalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;
			AssertEquals("INV", TestApprovalRequest.TransactionType);

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1");
			TestApprovalRequest.XP_ParentID = invoice.PK;
			TestApprovalRequest.XP_ParentTableCode = invoice.TablePrefix;
			foreach (var status in statusesWithoutPosting)
			{
				TestApprovalRequest.XP_ApprovalStatus = status;
				AssertEquals("UAI", TestApprovalRequest.TransactionType);
			}
			TestApprovalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;
			AssertEquals("INV", TestApprovalRequest.TransactionType);

			invoice.MoveToIncompleteLedger();
			TestApprovalRequest.XP_ParentID = invoice.PK;
			TestApprovalRequest.XP_ParentTableCode = invoice.TablePrefix;
			foreach (var status in statusesWithoutPosting)
			{
				TestApprovalRequest.XP_ApprovalStatus = status;
				AssertEquals("UAI", TestApprovalRequest.TransactionType);
			}
			TestApprovalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;
			AssertEquals("INV", TestApprovalRequest.TransactionType);

			var creditNote = TestObjectCreator.CreateInvoice(typeof(APCreditNote), "CRD1");
			TestApprovalRequest.XP_ParentID = creditNote.PK;
			TestApprovalRequest.XP_ParentTableCode = creditNote.TablePrefix;
			foreach (var status in statusesWithoutPosting)
			{
				TestApprovalRequest.XP_ApprovalStatus = status;
				AssertEquals("UAC", TestApprovalRequest.TransactionType);
			}
			TestApprovalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;
			AssertEquals("CRD", TestApprovalRequest.TransactionType);

			creditNote.MoveToIncompleteLedger();
			TestApprovalRequest.XP_ParentID = creditNote.PK;
			TestApprovalRequest.XP_ParentTableCode = creditNote.TablePrefix;
			foreach (var status in statusesWithoutPosting)
			{
				TestApprovalRequest.XP_ApprovalStatus = status;
				AssertEquals("UAC", TestApprovalRequest.TransactionType);
			}
			TestApprovalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;
			AssertEquals("CRD", TestApprovalRequest.TransactionType);
		}

		public void TestXP_ApprovalRequestData()
		{
			TestApprovalRequest.PostingDetails.MaxAmountToApprove = 200.12M;
			TestApprovalRequest.PostingDetails.Creditor = "ORG1";
			TestApprovalRequest.PostingDetails.TransactionNumber = "1234";
			TestApprovalRequest.RequisitionStatus = "TST";
			TestApprovalRequest.PostingDetails.Description = "Desc1";
			TestApprovalRequest.PostingDetails.InvoiceTerm = Constants.InvoiceTerms.FromCustomsClearanceDate;
			TestApprovalRequest.PostingDetails.InvoiceTermDays = 3;

			var requisitionDate = new ZDateTime(2016, 3, 14);
			TestApprovalRequest.RequisitionDate = requisitionDate;
			TestApprovalRequest.PostingDetails.PostingOption = "COSTS";
			var charge1 = TestApprovalRequest.PostingDetails.Charges.AddNew();
			charge1.JobNumber = "S00001234";
			charge1.ChargeCode = "FRT";
			charge1.Branch = "SYD";
			charge1.Department = "FES";
			charge1.CostCurrency = "USD";
			charge1.OSCostAmount = 100.1M;
			charge1.LocalCostAmount = 100.1M;

			var charge2 = TestApprovalRequest.PostingDetails.Charges.AddNew();
			charge2.JobNumber = "S00001234";
			charge2.ChargeCode = "BAF";
			charge2.Branch = "SYD";
			charge2.Department = "FES";
			charge2.CostCurrency = "USD";
			charge2.OSCostAmount = 200.12M;
			charge2.LocalCostAmount = 200.12M;
			charge2.Description = "Desc2";
			var taxMessage = Factory.NewWithValidTestData<AccInvMsg>();
			charge2.AccInvMsgPK = taxMessage.PK;
			charge2.TaxDate = new ZDate(2016, 3, 13);
			Factory.Save();

			var testApprovalRequest_inNewFactory = new BusinessObjectFactory().Load<APInvoiceChargesApprovalRequest>(TestApprovalRequest.PK);
			string expectedXML = $"\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><APInvoiceChargesApprovalRequestDetails><PostingOption>COSTS</PostingOption><Creditor>ORG1</Creditor><TransactionNumber>1234</TransactionNumber><MaxAmountToApprove>200.12</MaxAmountToApprove><Description>Desc1</Description><InvoiceTerm>CUS</InvoiceTerm><InvoiceTermDays>3</InvoiceTermDays><ArrayOfAPInvoiceChargesApprovalRequestChargeDetails xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><APInvoiceChargesApprovalRequestChargeDetails><JobNumber>S00001234</JobNumber><ChargeCode>FRT</ChargeCode><Branch>SYD</Branch><Department>FES</Department><Description /><AccInvMsgPK>00000000-0000-0000-0000-000000000000</AccInvMsgPK><TaxDate /><CostCurrency>USD</CostCurrency><OSCostAmount>100.1</OSCostAmount><LocalCostAmount>100.1</LocalCostAmount></APInvoiceChargesApprovalRequestChargeDetails><APInvoiceChargesApprovalRequestChargeDetails><JobNumber>S00001234</JobNumber><ChargeCode>BAF</ChargeCode><Branch>SYD</Branch><Department>FES</Department><Description>Desc2</Description><AccInvMsgPK>{taxMessage.PK}</AccInvMsgPK><TaxDate>13-Mar-16</TaxDate><CostCurrency>USD</CostCurrency><OSCostAmount>200.12</OSCostAmount><LocalCostAmount>200.12</LocalCostAmount></APInvoiceChargesApprovalRequestChargeDetails></ArrayOfAPInvoiceChargesApprovalRequestChargeDetails></APInvoiceChargesApprovalRequestDetails>";
			this.AssertXMLEqualsByDiff("XML saved in XP_ApprovalRequestData", expectedXML, Encoding.Unicode.GetString(testApprovalRequest_inNewFactory.XP_ApprovalRequestData));

			AssertEquals("Creditor", "ORG1", testApprovalRequest_inNewFactory.PostingDetails.Creditor);
			AssertEquals("TransactionNumber", "1234", testApprovalRequest_inNewFactory.PostingDetails.TransactionNumber);
			AssertEquals("RequisitionStatus", "TST", testApprovalRequest_inNewFactory.RequisitionStatus);
			AssertEquals("RequisitionDate", requisitionDate, testApprovalRequest_inNewFactory.RequisitionDate);
			AssertEquals("MaxAmountToApprove", 200.12M, testApprovalRequest_inNewFactory.PostingDetails.MaxAmountToApprove);
			AssertEquals("PostingOption", "COSTS", testApprovalRequest_inNewFactory.PostingDetails.PostingOption);
			AssertEquals("Charges.Count", 2, testApprovalRequest_inNewFactory.PostingDetails.Charges.Count);
			var charge_inNewFactory = testApprovalRequest_inNewFactory.PostingDetails.Charges[0];
			AssertEquals("JobNumber", "S00001234", charge_inNewFactory.JobNumber);
			AssertEquals("ChargeCode", "FRT", charge_inNewFactory.ChargeCode);
			AssertEquals("Branch", "SYD", charge_inNewFactory.Branch);
			AssertEquals("Department", "FES", charge_inNewFactory.Department);
			AssertEquals("CostCurrency", "USD", charge_inNewFactory.CostCurrency);
			AssertEquals("OSCostAmount", 100.1M, charge_inNewFactory.OSCostAmount);
			AssertEquals("LocalCostAmount", 100.1M, charge_inNewFactory.LocalCostAmount);
			AssertEquals(string.Empty, charge_inNewFactory.Description);
			AssertEquals(ZDate.Empty, charge_inNewFactory.TaxDate);
			AssertEquals(ZGuid.Empty, charge_inNewFactory.AccInvMsgPK);

			charge_inNewFactory = testApprovalRequest_inNewFactory.PostingDetails.Charges[1];
			AssertEquals("JobNumber", "S00001234", charge_inNewFactory.JobNumber);
			AssertEquals("ChargeCode", "BAF", charge_inNewFactory.ChargeCode);
			AssertEquals("Branch", "SYD", charge_inNewFactory.Branch);
			AssertEquals("Department", "FES", charge_inNewFactory.Department);
			AssertEquals("CostCurrency", "USD", charge_inNewFactory.CostCurrency);
			AssertEquals("OSCostAmount", 200.12M, charge_inNewFactory.OSCostAmount);
			AssertEquals("LocalCostAmount", 200.12M, charge_inNewFactory.LocalCostAmount);
			AssertEquals("Desc2", charge_inNewFactory.Description);
			AssertEquals(new ZDate(2016, 3, 13), charge_inNewFactory.TaxDate);
			AssertEquals(taxMessage.PK, charge_inNewFactory.AccInvMsgPK);

			TestApprovalRequest.PostingDetails.Creditor = "ORG2";
			charge1.OSCostAmount = 300.05M;
			charge2.CostCurrency = "GBP";
			Factory.Save();
			var messagePrefix = "Update through data refresh bus: ";
			AssertEquals(messagePrefix + "Creditor", "ORG2", testApprovalRequest_inNewFactory.PostingDetails.Creditor);
			AssertEquals(messagePrefix + "TransactionNumber", "1234", testApprovalRequest_inNewFactory.PostingDetails.TransactionNumber);
			AssertEquals(messagePrefix + "RequisitionStatus", "TST", testApprovalRequest_inNewFactory.RequisitionStatus);
			AssertEquals(messagePrefix + "RequisitionDate", requisitionDate, testApprovalRequest_inNewFactory.RequisitionDate);
			AssertEquals(messagePrefix + "MaxAmountToApprove", 200.12M, testApprovalRequest_inNewFactory.PostingDetails.MaxAmountToApprove);
			AssertEquals(messagePrefix + "PostingOption", "COSTS", testApprovalRequest_inNewFactory.PostingDetails.PostingOption);
			charge_inNewFactory = testApprovalRequest_inNewFactory.PostingDetails.Charges[0];
			AssertEquals(messagePrefix + "JobNumber", "S00001234", charge_inNewFactory.JobNumber);
			AssertEquals(messagePrefix + "ChargeCode", "FRT", charge_inNewFactory.ChargeCode);
			AssertEquals(messagePrefix + "Branch", "SYD", charge_inNewFactory.Branch);
			AssertEquals(messagePrefix + "Department", "FES", charge_inNewFactory.Department);
			AssertEquals(messagePrefix + "CostCurrency", "USD", charge_inNewFactory.CostCurrency);
			AssertEquals(messagePrefix + "OSCostAmount", 300.05M, charge_inNewFactory.OSCostAmount);
			AssertEquals(messagePrefix + "LocalCostAmount", 100.1M, charge_inNewFactory.LocalCostAmount);
			charge_inNewFactory = testApprovalRequest_inNewFactory.PostingDetails.Charges[1];
			AssertEquals(messagePrefix + "JobNumber", "S00001234", charge_inNewFactory.JobNumber);
			AssertEquals(messagePrefix + "ChargeCode", "BAF", charge_inNewFactory.ChargeCode);
			AssertEquals(messagePrefix + "Branch", "SYD", charge_inNewFactory.Branch);
			AssertEquals(messagePrefix + "Department", "FES", charge_inNewFactory.Department);
			AssertEquals(messagePrefix + "CostCurrency", "GBP", charge_inNewFactory.CostCurrency);
			AssertEquals(messagePrefix + "OSCostAmount", 200.12M, charge_inNewFactory.OSCostAmount);
			AssertEquals(messagePrefix + "LocalCostAmount", 200.12M, charge_inNewFactory.LocalCostAmount);

			testApprovalRequest_inNewFactory.PostingDetails.Creditor = "ORG3";
			TestApprovalRequest.PostingDetails.Creditor = "ORG4";
			charge1.OSCostAmount = 100M;
			charge2.CostCurrency = "USD";
			Factory.Save();
			messagePrefix = "Update through data refresh bus should be done as object in current factory is changed: ";
			AssertEquals(messagePrefix + "PostingOption", "ORG3", testApprovalRequest_inNewFactory.PostingDetails.Creditor);
			AssertEquals(messagePrefix + "TransactionNumber", "1234", testApprovalRequest_inNewFactory.PostingDetails.TransactionNumber);
			AssertEquals(messagePrefix + "RequisitionStatus", "TST", testApprovalRequest_inNewFactory.RequisitionStatus);
			AssertEquals(messagePrefix + "RequisitionDate", requisitionDate, testApprovalRequest_inNewFactory.RequisitionDate);
			AssertEquals(messagePrefix + "MaxAmountToApprove", 200.12M, testApprovalRequest_inNewFactory.PostingDetails.MaxAmountToApprove);
			AssertEquals(messagePrefix + "PostingOption", "COSTS", testApprovalRequest_inNewFactory.PostingDetails.PostingOption);
			charge_inNewFactory = testApprovalRequest_inNewFactory.PostingDetails.Charges[0];
			AssertEquals(messagePrefix + "JobNumber", "S00001234", charge_inNewFactory.JobNumber);
			AssertEquals(messagePrefix + "ChargeCode", "FRT", charge_inNewFactory.ChargeCode);
			AssertEquals(messagePrefix + "Branch", "SYD", charge_inNewFactory.Branch);
			AssertEquals(messagePrefix + "Department", "FES", charge_inNewFactory.Department);
			AssertEquals(messagePrefix + "CostCurrency", "USD", charge_inNewFactory.CostCurrency);
			AssertEquals(messagePrefix + "OSCostAmount", 300.05M, charge_inNewFactory.OSCostAmount);
			AssertEquals(messagePrefix + "LocalCostAmount", 100.1M, charge_inNewFactory.LocalCostAmount);
			charge_inNewFactory = testApprovalRequest_inNewFactory.PostingDetails.Charges[1];
			AssertEquals(messagePrefix + "JobNumber", "S00001234", charge_inNewFactory.JobNumber);
			AssertEquals(messagePrefix + "ChargeCode", "BAF", charge_inNewFactory.ChargeCode);
			AssertEquals(messagePrefix + "Branch", "SYD", charge_inNewFactory.Branch);
			AssertEquals(messagePrefix + "Department", "FES", charge_inNewFactory.Department);
			AssertEquals(messagePrefix + "CostCurrency", "GBP", charge_inNewFactory.CostCurrency);
			AssertEquals(messagePrefix + "OSCostAmount", 200.12M, charge_inNewFactory.OSCostAmount);
			AssertEquals(messagePrefix + "LocalCostAmount", 200.12M, charge_inNewFactory.LocalCostAmount);
		}

		public void TestInitializeJobRelated_FixedPlaceOfSupply()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			using (GlbCompany.CurrentCompany.TemporarilySetCurrency(TestObjectCreator.AUD.RX_Code))
			{
				var placeofSupplyCode = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany)[0].Code;
				var placeofSupplyCodeType = PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(GlbCompany.CurrentCompany, placeofSupplyCode);

				var shipment = TestObjectCreator.CreateShipment("S0001");
				var job = TestObjectCreator.CreateJob(shipment);
				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Charge 01", TestObjectCreator.AUD, 100M, TestObjectCreator.Creditor1, invoiceNum: "INV#001");
				charge1.JR_CostPlaceOfSupply = placeofSupplyCode;
				var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "Charge 02", TestObjectCreator.AUD, 200M, TestObjectCreator.Creditor1, invoiceNum: "INV#001");
				charge2.JR_CostPlaceOfSupply = placeofSupplyCode;
				Factory.Save();

				var invoiceCharges = new APInvoiceCharges(TestObjectCreator.Creditor1.OH_Code, "INV#001", job.PK, JobShipmentSchema.Constants.Prefix, null, placeofSupplyCode);
				invoiceCharges.Charges.AddRange(new[] { charge1, charge2 });
				TestApprovalRequest.InitializeJobRelated(invoiceCharges, shipment.PK, JobShipmentSchema.Constants.Prefix);
				invoiceCharges.PostingGUIProvider.ShowApprovalFormToSetDescription(TestApprovalRequest);
				var expectedRequisitionDate = ZDateTime.Today.AddDays(20);
				invoiceCharges.RequisitionDate = expectedRequisitionDate;
				invoiceCharges.RequisitionStatus = "AAA";

				var postedInvoices = TestObjectCreator.PostJobAsBillingTab(job, JobInvoicingPostingOption.Costs).GetAllAPInvoicesAndCreditNotes();

				AssertEquals("XP_ParentID", shipment.PK, TestApprovalRequest.XP_ParentID);
				AssertEquals("XP_ParentTableCode", JobShipmentSchema.Constants.Prefix, TestApprovalRequest.XP_ParentTableCode);
				AssertEquals("PostingDetails.Charges.Count", 2, TestApprovalRequest.PostingDetails.Charges.Count);

				var postingDetailsCharge = TestApprovalRequest.PostingDetails.Charges[0];
				AssettFPOS(postingDetailsCharge.PlaceOfSupply, postingDetailsCharge.PlaceOfSupplyType);
				AssertEquals("ChargeCode", TestObjectCreator.CC1.AC_Code, postingDetailsCharge.ChargeCode);

				postingDetailsCharge = TestApprovalRequest.PostingDetails.Charges[1];
				AssertEquals("ChargeCode", TestObjectCreator.CC2.AC_Code, postingDetailsCharge.ChargeCode);
				AssettFPOS(postingDetailsCharge.PlaceOfSupply, postingDetailsCharge.PlaceOfSupplyType);

				AssertEquals(1, postedInvoices.Length);
				var postedInvoice = postedInvoices[0];
				AssertEquals("Invoice Line Count", 2, postedInvoice.Lines.Count);
				AssettFPOS(postedInvoice.AH_PlaceOfSupply, postedInvoice.AH_PlaceOfSupplyType);
				AssettFPOS(postedInvoice.Lines[0].AL_PlaceOfSupply, postedInvoice.Lines[1].AL_PlaceOfSupplyType);
				AssettFPOS(postedInvoice.Lines[1].AL_PlaceOfSupply, postedInvoice.Lines[1].AL_PlaceOfSupplyType);

				void AssettFPOS(ZString fpos, ZString fposType)
				{
					AssertEquals("PlaceOfSupply", placeofSupplyCode, fpos);
					AssertEquals("placeofSupplyCodeType", placeofSupplyCodeType, fposType);
				}
			}
		}

		public void TestInitializeJobRelated_MultipleJobs()
		{
			var taxRate1 = AccTaxRate.LoadExistingOrCreateNewTaxRate(new BusinessObjectFactory(), "TAX1", "RAT", 400, 6);
			var taxRate2 = AccTaxRate.LoadExistingOrCreateNewTaxRate(taxRate1.Factory, "TAX2", "RAT", 500, 15);
			taxRate1.Factory.Save();

			var job1 = Factory.NewJobForTesting<Job>();
			job1.JH_GB = TestObjectCreator.CreateBranch("BBB", GlbCompany.CurrentCompany).PK;
			job1.JH_GE = TestObjectCreator.FEADepartment.PK;
			job1.JH_JobNum = "JOB1";

			var charge1 = job1.Charges.AddNew();
			charge1.JR_JH = job1.PK;
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
			charge1.JR_GE = TestObjectCreator.FESDepartment.PK;
			charge1.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			charge1.JR_OSCostExRate = 0.5M;
			charge1.JR_OSCostAmt = 6M;
			charge1.JR_AT_CostGSTRate = taxRate1.PK;
			AssertEquals("Precondition: charge1.JR_OSCostGSTAmt_Calc ", 4M, charge1.JR_OSCostGSTAmt_Calc);
			var expectedInvoiceDate = ZDateTime.Today.AddDays(-1);
			charge1.JR_APInvoiceDate = expectedInvoiceDate;
			var expectedDocumentReceivedDate = ZDateTime.Today.AddDays(-2);
			charge1.JR_APDocumentReceivedDate = expectedDocumentReceivedDate;
			var expectedDueDate = ZDateTime.Today.AddDays(5);
			charge1.JR_PaymentDate = expectedDueDate;

			var job2 = Factory.NewJobForTesting<Job>();
			job2.JH_JobNum = "JOB2";

			var charge2 = job2.Charges.AddNew();
			charge2.JR_JH = job2.PK;
			charge2.JR_AC = TestObjectCreator.CC2.PK;
			charge2.JR_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
			charge2.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;
			charge2.JR_RX_NKCostCurrency = TestObjectCreator.GBP.RX_Code;
			charge2.JR_OSCostExRate = 0.5M;
			charge2.JR_OSCostAmt = 15M;
			charge2.JR_AT_CostGSTRate = taxRate2.PK;
			AssertEquals("Precondition: charge2.JR_OSCostGSTAmt_Calc", 5M, charge2.JR_OSCostGSTAmt_Calc);
			charge2.JR_LocalCostAmt = 3M;

			APInvoiceCharges invoiceCharges = new APInvoiceCharges("ZLOCCLT", "12345", ZGuid.Empty, "", null);
			invoiceCharges.Charges.AddRange(new[] { charge1, charge2 });
			ZGuid expectedParentID = ZGuid.NewZGuid();
			TestApprovalRequest.InitializeJobRelated(invoiceCharges, expectedParentID, "XX");
			invoiceCharges.PostingGUIProvider.ShowApprovalFormToSetDescription(TestApprovalRequest);
			var expectedRequisitionDate = ZDateTime.Today.AddDays(20);
			invoiceCharges.RequisitionDate = expectedRequisitionDate;
			invoiceCharges.RequisitionStatus = "AAA";

			AssertEquals("XP_ParentID", expectedParentID, TestApprovalRequest.XP_ParentID);
			AssertEquals("XP_ParentTableCode", "XX", TestApprovalRequest.XP_ParentTableCode);

			AssertEquals("Branch", "BBB", TestApprovalRequest.Branch);
			AssertEquals("Department", "FEA", TestApprovalRequest.Department);
			AssertEquals("InvoiceDate", expectedInvoiceDate, TestApprovalRequest.InvoiceDate);
			AssertEquals("DocumentReceivedDate", expectedDocumentReceivedDate, TestApprovalRequest.DocumentReceivedDate);
			AssertEquals("DueDate", expectedDueDate, TestApprovalRequest.DueDate);
			AssertEquals("InvoiceCurrency", "AUD", TestApprovalRequest.InvoiceCurrency);
			AssertEquals("InvoiceOSTotalAmount", -24m, TestApprovalRequest.InvoiceOSTotalAmount);
			AssertEquals("InvoiceLocalTotalAmount", -24m, TestApprovalRequest.InvoiceLocalTotalAmount);
			AssertEquals("InvoiceLocalExTaxAmount", -15m, TestApprovalRequest.InvoiceLocalExTaxAmount);
			AssertEquals("InvoiceLocalTaxAmount", -9m, TestApprovalRequest.InvoiceLocalTaxAmount);

			AssertEquals("RequisitionDate", expectedRequisitionDate, TestApprovalRequest.RequisitionDate);
			AssertEquals("RequisitionStatus", "AAA", TestApprovalRequest.RequisitionStatus);

			AssertEquals("PostingDetails.Creditor", "ZLOCCLT", TestApprovalRequest.PostingDetails.Creditor);
			AssertEquals("PostingDetails.TransactionNumber", "12345", TestApprovalRequest.PostingDetails.TransactionNumber);
			AssertEquals("PostingDetails.MaxAmountToApprove", 24M, TestApprovalRequest.PostingDetails.MaxAmountToApprove);
			AssertEquals("PostingDetails.PostingOption", "", TestApprovalRequest.PostingDetails.PostingOption);
			AssertEquals("PostingOptionForDisplay", "", TestApprovalRequest.PostingOptionForDisplay);

			AssertEquals("PostingDetails.Charges.Count", 2, TestApprovalRequest.PostingDetails.Charges.Count);
			var postingDetailsCharge = TestApprovalRequest.PostingDetails.Charges[0];
			AssertEquals("JobNumber", "JOB1", postingDetailsCharge.JobNumber);
			AssertEquals("ChargeCode", TestObjectCreator.CC1.AC_Code, postingDetailsCharge.ChargeCode);
			AssertEquals("Branch", TestObjectCreator.NonCurrentBranch.GB_Code, postingDetailsCharge.Branch);
			AssertEquals("Department", TestObjectCreator.FESDepartment.GE_Code, postingDetailsCharge.Department);
			AssertEquals("CostCurrency", "USD", postingDetailsCharge.CostCurrency);
			AssertEquals("OSCostAmount", 10M, postingDetailsCharge.OSCostAmount);
			AssertEquals("LocalCostAmount", 20M, postingDetailsCharge.LocalCostAmount);

			postingDetailsCharge = TestApprovalRequest.PostingDetails.Charges[1];
			AssertEquals("JobNumber", "JOB2", postingDetailsCharge.JobNumber);
			AssertEquals("ChargeCode", TestObjectCreator.CC2.AC_Code, postingDetailsCharge.ChargeCode);
			AssertEquals("Branch", TestObjectCreator.NonCurrentCompanyBranch.GB_Code, postingDetailsCharge.Branch);
			AssertEquals("Department", TestObjectCreator.NonCurrentDepartment.GE_Code, postingDetailsCharge.Department);
			AssertEquals("CostCurrency", "GBP", postingDetailsCharge.CostCurrency);
			AssertEquals("OSCostAmount", 2M, postingDetailsCharge.OSCostAmount);
			AssertEquals("LocalCostAmount", 4M, postingDetailsCharge.LocalCostAmount);
		}

		public void TestInitializeJobRelated_SingleJob()
		{
			var taxRate1 = AccTaxRate.LoadExistingOrCreateNewTaxRate(new BusinessObjectFactory(), "TAX1", "RAT", 400, 6);
			var taxRate2 = AccTaxRate.LoadExistingOrCreateNewTaxRate(taxRate1.Factory, "TAX2", "RAT", 500, 15);
			taxRate1.Factory.Save();

			var job1 = Factory.NewJobForTesting<Job>();
			job1.JH_JobNum = "JOB1";
			job1.JH_GB = TestObjectCreator.CreateBranch("BBB", GlbCompany.CurrentCompany).PK;
			job1.JH_GE = TestObjectCreator.FEADepartment.PK;

			var charge1 = job1.Charges.AddNew();
			charge1.JR_JH = job1.PK;
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
			charge1.JR_GE = TestObjectCreator.FESDepartment.PK;
			charge1.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			charge1.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			charge1.JR_OSCostAmt = 6M;
			charge1.JR_AT_CostGSTRate = taxRate1.PK;
			AssertEquals("Precondition: charge1.JR_OSCostGSTAmt_Calc ", 4M, charge1.JR_OSCostGSTAmt_Calc);
			charge1.CostExchangeRate.SetBuyRate_ForTestOnly(0.5m);
			charge1.JR_APInvoiceNum = "12345";
			var expectedInvoiceDate = ZDateTime.Today.AddDays(-1);
			charge1.JR_APInvoiceDate = expectedInvoiceDate;
			var expectedDocumentReceivedDate = ZDateTime.Today.AddDays(-2);
			charge1.JR_APDocumentReceivedDate = expectedDocumentReceivedDate;
			var expectedDueDate = ZDateTime.Today.AddDays(5);
			charge1.JR_PaymentDate = expectedDueDate;

			var charge2 = job1.Charges.AddNew();
			charge2.JR_JH = job1.PK;
			charge2.JR_AC = TestObjectCreator.CC2.PK;
			charge2.JR_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
			charge2.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;
			charge2.JR_OH_CostAccount = charge1.JR_OH_CostAccount;
			charge2.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			charge2.JR_OSCostAmt = 15M;
			charge2.JR_AT_CostGSTRate = taxRate2.PK;
			charge2.JR_OSCostGSTAmt_Calc = 5M;
			AssertEquals("Precondition: charge2.JR_OSCostGSTAmt_Calc", 5M, charge2.JR_OSCostGSTAmt_Calc);
			charge2.CostExchangeRate.SetBuyRate_ForTestOnly(0.5m);
			charge2.JR_APInvoiceNum = charge1.JR_APInvoiceNum;
			charge2.JR_APInvoiceDate = charge1.JR_APInvoiceDate;
			charge2.JR_APDocumentReceivedDate = charge1.JR_APDocumentReceivedDate;
			charge2.JR_PaymentDate = charge1.JR_PaymentDate;

			APInvoiceCharges invoiceCharges = new APInvoiceCharges("ZLOCCLT", "12345", ZGuid.Empty, "", null);
			invoiceCharges.Charges.AddRange(new[] { charge1, charge2 });
			ZGuid expectedParentID = ZGuid.NewZGuid();
			TestApprovalRequest.InitializeJobRelated(invoiceCharges, expectedParentID, "XX");
			invoiceCharges.PostingGUIProvider.ShowApprovalFormToSetDescription(TestApprovalRequest);
			var expectedRequisitionDate = ZDateTime.Today.AddDays(20);
			invoiceCharges.RequisitionDate = expectedRequisitionDate;
			invoiceCharges.RequisitionStatus = "AAA";

			var postedInvoices = TestObjectCreator.PostJobAsBillingTab(job1, JobInvoicingPostingOption.Costs).GetAllAPInvoicesAndCreditNotes();
			AssertEquals(1, postedInvoices.Length);
			var postedInvoice = postedInvoices[0];

			AssertEquals("XP_ParentID", expectedParentID, TestApprovalRequest.XP_ParentID);
			AssertEquals("XP_ParentTableCode", "XX", TestApprovalRequest.XP_ParentTableCode);

			AssertEquals("Branch", "BBB", TestApprovalRequest.Branch);
			AssertEquals("Branch the same as in posted invoice", postedInvoice.Branch.GB_Code, TestApprovalRequest.Branch);
			AssertEquals("Department", "FEA", TestApprovalRequest.Department);
			AssertEquals("Department the same as in posted invoice", postedInvoice.Department.GE_Code, TestApprovalRequest.Department);
			AssertEquals("InvoiceDate", expectedInvoiceDate, TestApprovalRequest.InvoiceDate);
			AssertEquals("InvoiceDate the same as in posted invoice", postedInvoice.AH_InvoiceDate, TestApprovalRequest.InvoiceDate);
			AssertEquals("DocumentReceivedDate", expectedDocumentReceivedDate, TestApprovalRequest.DocumentReceivedDate);
			AssertEquals("DocumentReceivedDate the same as in posted invoice", postedInvoice.AH_DocumentReceivedDate, TestApprovalRequest.DocumentReceivedDate);
			AssertEquals("DueDate", expectedDueDate, TestApprovalRequest.DueDate);
			AssertEquals("DueDate the same as in posted invoice", postedInvoice.AH_DueDate, TestApprovalRequest.DueDate);
			AssertEquals("InvoiceCurrency", "USD", TestApprovalRequest.InvoiceCurrency);
			AssertEquals("InvoiceCurrency the same as in posted invoice", postedInvoice.AH_RX_NKTransactionCurrency, TestApprovalRequest.InvoiceCurrency);
			AssertEquals("InvoiceOSTotalAmount", -30m, TestApprovalRequest.InvoiceOSTotalAmount);
			AssertEquals("InvoiceOSTotalAmount the same as in posted invoice", postedInvoice.AH_OSTotal, TestApprovalRequest.InvoiceOSTotalAmount);
			AssertEquals("InvoiceLocalTotalAmount", -60m, TestApprovalRequest.InvoiceLocalTotalAmount);
			AssertEquals("InvoiceLocalTotalAmount the same as in posted invoice", postedInvoice.AH_LocalTotal, TestApprovalRequest.InvoiceLocalTotalAmount);
			AssertEquals("InvoiceLocalExTaxAmount", -42m, TestApprovalRequest.InvoiceLocalExTaxAmount);
			AssertEquals("InvoiceLocalExTaxAmount the same as in posted invoice", postedInvoice.AH_InvoiceAmount, TestApprovalRequest.InvoiceLocalExTaxAmount);
			AssertEquals("InvoiceLocalTaxAmount", -18m, TestApprovalRequest.InvoiceLocalTaxAmount);
			AssertEquals("InvoiceLocalTaxAmount the same as in posted invoice", postedInvoice.AH_GSTAmount, TestApprovalRequest.InvoiceLocalTaxAmount);

			AssertEquals("RequisitionDate", expectedRequisitionDate, TestApprovalRequest.RequisitionDate);
			AssertEquals("RequisitionStatus", "AAA", TestApprovalRequest.RequisitionStatus);

			AssertEquals("PostingDetails.Creditor", "ZLOCCLT", TestApprovalRequest.PostingDetails.Creditor);
			AssertEquals("PostingDetails.TransactionNumber", "12345", TestApprovalRequest.PostingDetails.TransactionNumber);
			AssertEquals("PostingDetails.MaxAmountToApprove", 60M, TestApprovalRequest.PostingDetails.MaxAmountToApprove);
			AssertEquals("PostingDetails.PostingOption", "", TestApprovalRequest.PostingDetails.PostingOption);
			AssertEquals("PostingOptionForDisplay", "", TestApprovalRequest.PostingOptionForDisplay);

			AssertEquals("PostingDetails.Charges.Count", 2, TestApprovalRequest.PostingDetails.Charges.Count);
			var postingDetailsCharge = TestApprovalRequest.PostingDetails.Charges[0];
			AssertEquals("JobNumber", "JOB1", postingDetailsCharge.JobNumber);
			AssertEquals("ChargeCode", TestObjectCreator.CC1.AC_Code, postingDetailsCharge.ChargeCode);
			AssertEquals("Branch", TestObjectCreator.NonCurrentBranch.GB_Code, postingDetailsCharge.Branch);
			AssertEquals("Department", TestObjectCreator.FESDepartment.GE_Code, postingDetailsCharge.Department);
			AssertEquals("CostCurrency", "USD", postingDetailsCharge.CostCurrency);
			AssertEquals("OSCostAmount", 10M, postingDetailsCharge.OSCostAmount);
			AssertEquals("LocalCostAmount", 20M, postingDetailsCharge.LocalCostAmount);

			postingDetailsCharge = TestApprovalRequest.PostingDetails.Charges[1];
			AssertEquals("JobNumber", "JOB1", postingDetailsCharge.JobNumber);
			AssertEquals("ChargeCode", TestObjectCreator.CC2.AC_Code, postingDetailsCharge.ChargeCode);
			AssertEquals("Branch", TestObjectCreator.NonCurrentCompanyBranch.GB_Code, postingDetailsCharge.Branch);
			AssertEquals("Department", TestObjectCreator.NonCurrentDepartment.GE_Code, postingDetailsCharge.Department);
			AssertEquals("CostCurrency", "USD", postingDetailsCharge.CostCurrency);
			AssertEquals("OSCostAmount", 20M, postingDetailsCharge.OSCostAmount);
			AssertEquals("LocalCostAmount", 40M, postingDetailsCharge.LocalCostAmount);
		}

		public void TestInitializeJobRelated_SingleJob_MultiCurrency()
		{
			var taxRate1 = AccTaxRate.LoadExistingOrCreateNewTaxRate(new BusinessObjectFactory(), "TAX1", "RAT", 400, 6);
			var taxRate2 = AccTaxRate.LoadExistingOrCreateNewTaxRate(taxRate1.Factory, "TAX2", "RAT", 500, 15);
			taxRate1.Factory.Save();

			var job1 = Factory.NewJobForTesting<Job>();
			job1.JH_JobNum = "JOB1";
			job1.JH_GB = TestObjectCreator.CreateBranch("BBB", GlbCompany.CurrentCompany).PK;
			job1.JH_GE = TestObjectCreator.FEADepartment.PK;

			var charge1 = job1.Charges.AddNew();
			charge1.JR_JH = job1.PK;
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
			charge1.JR_GE = TestObjectCreator.FESDepartment.PK;
			charge1.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			charge1.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			charge1.JR_OSCostAmt = 6M;
			charge1.JR_AT_CostGSTRate = taxRate1.PK;
			AssertEquals("Precondition: charge1.JR_OSCostGSTAmt_Calc ", 4M, charge1.JR_OSCostGSTAmt_Calc);
			charge1.CostExchangeRate.SetBuyRate_ForTestOnly(0.5m);
			charge1.JR_APInvoiceNum = "12345";
			var expectedInvoiceDate = ZDateTime.Today.AddDays(-1);
			charge1.JR_APInvoiceDate = expectedInvoiceDate;
			var expectedDocumentReceivedDate = ZDateTime.Today.AddDays(-2);
			charge1.JR_APDocumentReceivedDate = expectedDocumentReceivedDate;
			var expectedDueDate = ZDateTime.Today.AddDays(5);
			charge1.JR_PaymentDate = expectedDueDate;

			var charge2 = job1.Charges.AddNew();
			charge2.JR_JH = job1.PK;
			charge2.JR_AC = TestObjectCreator.CC2.PK;
			charge2.JR_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
			charge2.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;
			charge2.JR_OH_CostAccount = charge1.JR_OH_CostAccount;
			charge2.JR_RX_NKCostCurrency = TestObjectCreator.GBP.RX_Code;
			charge2.JR_OSCostAmt = 15M;
			charge2.JR_AT_CostGSTRate = taxRate2.PK;
			AssertEquals("Precondition: charge2.JR_OSCostGSTAmt_Calc", 5M, charge2.JR_OSCostGSTAmt_Calc);
			charge2.CostExchangeRate.SetBuyRate_ForTestOnly(0.5m);
			charge2.JR_APInvoiceNum = charge1.JR_APInvoiceNum;
			charge2.JR_APInvoiceDate = charge1.JR_APInvoiceDate;
			charge2.JR_PaymentDate = charge1.JR_PaymentDate;

			APInvoiceCharges invoiceCharges = new APInvoiceCharges("ZLOCCLT", "12345", ZGuid.Empty, "", null);
			invoiceCharges.Charges.AddRange(new[] { charge1, charge2 });
			ZGuid expectedParentID = ZGuid.NewZGuid();
			TestApprovalRequest.InitializeJobRelated(invoiceCharges, expectedParentID, "XX");
			invoiceCharges.PostingGUIProvider.ShowApprovalFormToSetDescription(TestApprovalRequest);
			var expectedRequisitionDate = ZDateTime.Today.AddDays(20);
			invoiceCharges.RequisitionDate = expectedRequisitionDate;
			invoiceCharges.RequisitionStatus = "AAA";

			var postedInvoices = TestObjectCreator.PostJobAsBillingTab(job1, JobInvoicingPostingOption.Costs).GetAllAPInvoicesAndCreditNotes();
			AssertEquals(1, postedInvoices.Length);
			var postedInvoice = postedInvoices[0];

			AssertEquals("XP_ParentID", expectedParentID, TestApprovalRequest.XP_ParentID);
			AssertEquals("XP_ParentTableCode", "XX", TestApprovalRequest.XP_ParentTableCode);

			AssertEquals("Branch", "BBB", TestApprovalRequest.Branch);
			AssertEquals("Branch the same as in posted invoice", postedInvoice.Branch.GB_Code, TestApprovalRequest.Branch);
			AssertEquals("Department", "FEA", TestApprovalRequest.Department);
			AssertEquals("Department the same as in posted invoice", postedInvoice.Department.GE_Code, TestApprovalRequest.Department);
			AssertEquals("InvoiceDate", expectedInvoiceDate, TestApprovalRequest.InvoiceDate);
			AssertEquals("InvoiceDate the same as in posted invoice", postedInvoice.AH_InvoiceDate, TestApprovalRequest.InvoiceDate);
			AssertEquals("DocumentReceivedDate", expectedDocumentReceivedDate, TestApprovalRequest.DocumentReceivedDate);
			AssertEquals("DocumentReceivedDate the same as in posted invoice", postedInvoice.AH_DocumentReceivedDate, TestApprovalRequest.DocumentReceivedDate);
			AssertEquals("DueDate", expectedDueDate, TestApprovalRequest.DueDate);
			AssertEquals("DueDate the same as in posted invoice", postedInvoice.AH_DueDate, TestApprovalRequest.DueDate);
			AssertEquals("InvoiceCurrency", "AUD", TestApprovalRequest.InvoiceCurrency);
			AssertEquals("InvoiceCurrency the same as in posted invoice", postedInvoice.AH_RX_NKTransactionCurrency, TestApprovalRequest.InvoiceCurrency);
			AssertEquals("InvoiceOSTotalAmount", -60m, TestApprovalRequest.InvoiceOSTotalAmount);
			AssertEquals("InvoiceOSTotalAmount the same as in posted invoice", postedInvoice.AH_OSTotal, TestApprovalRequest.InvoiceOSTotalAmount);
			AssertEquals("InvoiceLocalTotalAmount", -60m, TestApprovalRequest.InvoiceLocalTotalAmount);
			AssertEquals("InvoiceLocalTotalAmount the same as in posted invoice", postedInvoice.AH_LocalTotal, TestApprovalRequest.InvoiceLocalTotalAmount);
			AssertEquals("InvoiceLocalExTaxAmount", -42m, TestApprovalRequest.InvoiceLocalExTaxAmount);
			AssertEquals("InvoiceLocalExTaxAmount the same as in posted invoice", postedInvoice.AH_InvoiceAmount, TestApprovalRequest.InvoiceLocalExTaxAmount);
			AssertEquals("InvoiceLocalTaxAmount", -18m, TestApprovalRequest.InvoiceLocalTaxAmount);
			AssertEquals("InvoiceLocalTaxAmount the same as in posted invoice", postedInvoice.AH_GSTAmount, TestApprovalRequest.InvoiceLocalTaxAmount);

			AssertEquals("RequisitionDate", expectedRequisitionDate, TestApprovalRequest.RequisitionDate);
			AssertEquals("RequisitionStatus", "AAA", TestApprovalRequest.RequisitionStatus);

			AssertEquals("PostingDetails.Creditor", "ZLOCCLT", TestApprovalRequest.PostingDetails.Creditor);
			AssertEquals("PostingDetails.TransactionNumber", "12345", TestApprovalRequest.PostingDetails.TransactionNumber);
			AssertEquals("PostingDetails.MaxAmountToApprove", 60M, TestApprovalRequest.PostingDetails.MaxAmountToApprove);
			AssertEquals("PostingDetails.PostingOption", "", TestApprovalRequest.PostingDetails.PostingOption);
			AssertEquals("PostingOptionForDisplay", "", TestApprovalRequest.PostingOptionForDisplay);

			AssertEquals("PostingDetails.Charges.Count", 2, TestApprovalRequest.PostingDetails.Charges.Count);
			var postingDetailsCharge = TestApprovalRequest.PostingDetails.Charges[0];
			AssertEquals("JobNumber", "JOB1", postingDetailsCharge.JobNumber);
			AssertEquals("ChargeCode", TestObjectCreator.CC1.AC_Code, postingDetailsCharge.ChargeCode);
			AssertEquals("Branch", TestObjectCreator.NonCurrentBranch.GB_Code, postingDetailsCharge.Branch);
			AssertEquals("Department", TestObjectCreator.FESDepartment.GE_Code, postingDetailsCharge.Department);
			AssertEquals("CostCurrency", "USD", postingDetailsCharge.CostCurrency);
			AssertEquals("OSCostAmount", 10M, postingDetailsCharge.OSCostAmount);
			AssertEquals("LocalCostAmount", 20M, postingDetailsCharge.LocalCostAmount);

			postingDetailsCharge = TestApprovalRequest.PostingDetails.Charges[1];
			AssertEquals("JobNumber", "JOB1", postingDetailsCharge.JobNumber);
			AssertEquals("ChargeCode", TestObjectCreator.CC2.AC_Code, postingDetailsCharge.ChargeCode);
			AssertEquals("Branch", TestObjectCreator.NonCurrentCompanyBranch.GB_Code, postingDetailsCharge.Branch);
			AssertEquals("Department", TestObjectCreator.NonCurrentDepartment.GE_Code, postingDetailsCharge.Department);
			AssertEquals("CostCurrency", "GBP", postingDetailsCharge.CostCurrency);
			AssertEquals("OSCostAmount", 20M, postingDetailsCharge.OSCostAmount);
			AssertEquals("LocalCostAmount", 40M, postingDetailsCharge.LocalCostAmount);
		}

		public void TestInitializeInvoiceRelated_Invoice()
		{
			var job1 = Factory.NewJobForTesting<Job>();
			job1.JH_JobNum = "JOB1";
			var invoice = Factory.New<APInvoice>();
			invoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			invoice.AH_GB = TestObjectCreator.CreateBranch("BBB", GlbCompany.CurrentCompany).PK;
			invoice.AH_GE = TestObjectCreator.FEADepartment.PK;
			invoice.AH_OH = TestObjectCreator.LocalClient.PK;
			invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			invoice.AH_TransactionNum = "12345";
			var expectedInvoiceDate = ZDateTime.Today.AddDays(-1);
			invoice.AH_InvoiceDate = expectedInvoiceDate;
			var expectedDueDate = ZDateTime.Today.AddDays(5);
			invoice.AH_DueDate = expectedDueDate;
			var expectedRequisitionDate = ZDateTime.Today.AddDays(20);
			invoice.AH_RequisitionDate = expectedRequisitionDate;
			invoice.AH_RequisitionStatus = "AAA";

			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_JH = job1.PK;
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_GB = TestObjectCreator.NonCurrentBranch.PK;
			line.AL_GE = TestObjectCreator.FESDepartment.PK;
			line.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			line.AL_OSAmount = -10M;
			line.AL_LineAmount = -3M;
			line.AL_GSTVAT = -2M;
			var job2 = Factory.NewJobForTesting<Job>();
			job2.JH_JobNum = "JOB2";
			line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_JH = job2.PK;
			line.AL_AC = TestObjectCreator.CC2.PK;
			line.AL_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
			line.AL_GE = TestObjectCreator.NonCurrentDepartment.PK;
			line.AL_RX_NKTransactionCurrency = TestObjectCreator.GBP.RX_Code;
			line.AL_OSAmount = -20M;
			line.AL_LineAmount = -10M;
			line.AL_GSTVAT = -5M;
			invoice.Lines.UpdateHeaderAmounts();
			TestApprovalRequest.InitializeInvoiceRelated(invoice);

			AssertEquals("XP_ParentID", invoice.PK, TestApprovalRequest.XP_ParentID);
			AssertEquals("XP_ParentTableCode", invoice.TablePrefix, TestApprovalRequest.XP_ParentTableCode);

			AssertEquals("Branch", "BBB", TestApprovalRequest.Branch);
			AssertEquals("Department", "FEA", TestApprovalRequest.Department);
			AssertEquals("DueDate", expectedDueDate, TestApprovalRequest.DueDate);
			AssertEquals("InvoiceCurrency", "USD", TestApprovalRequest.InvoiceCurrency);
			AssertEquals("InvoiceOSTotalAmount", -30m, TestApprovalRequest.InvoiceOSTotalAmount);
			AssertEquals("InvoiceLocalTotalAmount", -20m, TestApprovalRequest.InvoiceLocalTotalAmount);
			AssertEquals("InvoiceLocalExTaxAmount", -13m, TestApprovalRequest.InvoiceLocalExTaxAmount);
			AssertEquals("InvoiceLocalTaxAmount", -7m, TestApprovalRequest.InvoiceLocalTaxAmount);
			AssertEquals("RequisitionDate", expectedRequisitionDate, TestApprovalRequest.RequisitionDate);
			AssertEquals("RequisitionStatus", "AAA", TestApprovalRequest.RequisitionStatus);

			AssertEquals("PostingDetails.Creditor", "ZLOCCLT", TestApprovalRequest.PostingDetails.Creditor);
			AssertEquals("PostingDetails.TransactionNumber", "12345", TestApprovalRequest.PostingDetails.TransactionNumber);
			AssertEquals("PostingDetails.MaxAmountToApprove - always positive amount needed here to allow security level selector to work", 20M, TestApprovalRequest.PostingDetails.MaxAmountToApprove);
			AssertEquals("PostingDetails.PostingOption", ZString.Empty, TestApprovalRequest.PostingDetails.PostingOption);
			AssertEquals("PostingDetails.Charges.Count", 2, TestApprovalRequest.PostingDetails.Charges.Count);

			var charge = TestApprovalRequest.PostingDetails.Charges[0];
			AssertEquals("JobNumber", "JOB1", charge.JobNumber);
			AssertEquals("ChargeCode", TestObjectCreator.CC1.AC_Code, charge.ChargeCode);
			AssertEquals("Branch", TestObjectCreator.NonCurrentBranch.GB_Code, charge.Branch);
			AssertEquals("Department", TestObjectCreator.FESDepartment.GE_Code, charge.Department);
			AssertEquals("CostCurrency", "USD", charge.CostCurrency);
			AssertEquals("OSCostAmount", 10M, charge.OSCostAmount);
			AssertEquals("LocalCostAmount", 5M, charge.LocalCostAmount);

			charge = TestApprovalRequest.PostingDetails.Charges[1];
			AssertEquals("JobNumber", "JOB2", charge.JobNumber);
			AssertEquals("ChargeCode", TestObjectCreator.CC2.AC_Code, charge.ChargeCode);
			AssertEquals("Branch", TestObjectCreator.NonCurrentCompanyBranch.GB_Code, charge.Branch);
			AssertEquals("Department", TestObjectCreator.NonCurrentDepartment.GE_Code, charge.Department);
			AssertEquals("CostCurrency", "GBP", charge.CostCurrency);
			AssertEquals("OSCostAmount", 20M, charge.OSCostAmount);
			AssertEquals("LocalCostAmount", 15M, charge.LocalCostAmount);
		}

		public void TestInitializeInvoiceRelated_CreditNote()
		{
			var job1 = Factory.NewJobForTesting<Job>();
			job1.JH_JobNum = "JOB1";
			var invoice = Factory.New<APCreditNote>();
			invoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			invoice.AH_OH = TestObjectCreator.LocalClient.PK;
			invoice.AH_TransactionNum = "12345";
			invoice.AH_LocalExTaxAmount = 2M;
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_JH = job1.PK;
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_GB = TestObjectCreator.NonCurrentBranch.PK;
			line.AL_GE = TestObjectCreator.FESDepartment.PK;
			line.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			line.AL_OSAmount = -10M;
			line.AL_LineAmount = -3M;
			line.AL_GSTVAT = -2M;
			var job2 = Factory.NewJobForTesting<Job>();
			job2.JH_JobNum = "JOB2";
			line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_JH = job2.PK;
			line.AL_AC = TestObjectCreator.CC2.PK;
			line.AL_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
			line.AL_GE = TestObjectCreator.NonCurrentDepartment.PK;
			line.AL_RX_NKTransactionCurrency = TestObjectCreator.GBP.RX_Code;
			line.AL_OSAmount = -20M;
			line.AL_LineAmount = -10M;
			line.AL_GSTVAT = -5M;
			TestApprovalRequest.InitializeInvoiceRelated(invoice);

			AssertEquals("XP_ParentID", invoice.PK, TestApprovalRequest.XP_ParentID);
			AssertEquals("XP_ParentTableCode", invoice.TablePrefix, TestApprovalRequest.XP_ParentTableCode);
			AssertEquals("PostingDetails.Creditor", "ZLOCCLT", TestApprovalRequest.PostingDetails.Creditor);
			AssertEquals("PostingDetails.TransactionNumber", "12345", TestApprovalRequest.PostingDetails.TransactionNumber);
			AssertEquals("PostingDetails.MaxAmountToApprove - always positive amount needed here to allow security level selector to work", 2M, TestApprovalRequest.PostingDetails.MaxAmountToApprove);
			AssertEquals("PostingDetails.PostingOption", ZString.Empty, TestApprovalRequest.PostingDetails.PostingOption);
			AssertEquals("PostingDetails.Charges.Count", 2, TestApprovalRequest.PostingDetails.Charges.Count);

			var charge = TestApprovalRequest.PostingDetails.Charges[0];
			AssertEquals("JobNumber", "JOB1", charge.JobNumber);
			AssertEquals("ChargeCode", TestObjectCreator.CC1.AC_Code, charge.ChargeCode);
			AssertEquals("Branch", TestObjectCreator.NonCurrentBranch.GB_Code, charge.Branch);
			AssertEquals("Department", TestObjectCreator.FESDepartment.GE_Code, charge.Department);
			AssertEquals("CostCurrency", "USD", charge.CostCurrency);
			AssertEquals("OSCostAmount", 10M, charge.OSCostAmount);
			AssertEquals("LocalCostAmount", 5M, charge.LocalCostAmount);

			charge = TestApprovalRequest.PostingDetails.Charges[1];
			AssertEquals("JobNumber", "JOB2", charge.JobNumber);
			AssertEquals("ChargeCode", TestObjectCreator.CC2.AC_Code, charge.ChargeCode);
			AssertEquals("Branch", TestObjectCreator.NonCurrentCompanyBranch.GB_Code, charge.Branch);
			AssertEquals("Department", TestObjectCreator.NonCurrentDepartment.GE_Code, charge.Department);
			AssertEquals("CostCurrency", "GBP", charge.CostCurrency);
			AssertEquals("OSCostAmount", 20M, charge.OSCostAmount);
			AssertEquals("LocalCostAmount", 15M, charge.LocalCostAmount);
		}

		public void TestInitializeInvoiceRelated_FixedPlaceOfSupply()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			using (GlbCompany.CurrentCompany.TemporarilySetCurrency(TestObjectCreator.AUD.RX_Code))
			{
				var placeofSupplyCode = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany)[0].Code;
				var placeofSupplyCodeType = PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(GlbCompany.CurrentCompany, placeofSupplyCode);

				var shipment1 = TestObjectCreator.CreateShipment("S0001");
				var job1 = TestObjectCreator.CreateJob(shipment1);

				var shipment2 = TestObjectCreator.CreateShipment("S0002");
				var job2 = TestObjectCreator.CreateJob(shipment2);

				Factory.Save();

				var invoice = Factory.NewWithValidTestData<APInvoice>();
				var line1 = TestObjectCreator.CreateAPInvoiceLine(invoice, job1, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0M, "Line 01", 250M);
				line1.AL_PlaceOfSupply = placeofSupplyCode;
				var line2 = TestObjectCreator.CreateAPInvoiceLine(invoice, job2, TestObjectCreator.CC2, TestObjectCreator.AUD, 1.0M, "Line 02", 150M);
				line2.AL_PlaceOfSupply = placeofSupplyCode;

				TestApprovalRequest.InitializeInvoiceRelated(invoice);

				AssertEquals("XP_ParentID", invoice.PK, TestApprovalRequest.XP_ParentID);
				AssertEquals("XP_ParentTableCode", invoice.TablePrefix, TestApprovalRequest.XP_ParentTableCode);
				AssertEquals("PostingDetails.Charges.Count", 2, TestApprovalRequest.PostingDetails.Charges.Count);

				var postingDetailsCharge = TestApprovalRequest.PostingDetails.Charges[0];
				AssertEquals("ChargeCode", TestObjectCreator.CC1.AC_Code, postingDetailsCharge.ChargeCode);
				AssertEquals("JobNumber", job1.JH_JobNum, postingDetailsCharge.JobNumber);
				AssettFPOS(postingDetailsCharge.PlaceOfSupply, postingDetailsCharge.PlaceOfSupplyType);

				postingDetailsCharge = TestApprovalRequest.PostingDetails.Charges[1];
				AssertEquals("ChargeCode", TestObjectCreator.CC2.AC_Code, postingDetailsCharge.ChargeCode);
				AssertEquals("JobNumber", job2.JH_JobNum, postingDetailsCharge.JobNumber);
				AssettFPOS(postingDetailsCharge.PlaceOfSupply, postingDetailsCharge.PlaceOfSupplyType);

				TestApprovalRequest.PrepareFoSaving();
				Factory.Save();

				AssertEquals("Invoice Line Count", 2, invoice.Lines.Count);
				AssettFPOS(invoice.AH_PlaceOfSupply, invoice.AH_PlaceOfSupplyType);
				AssettFPOS(invoice.Lines[0].AL_PlaceOfSupply, invoice.Lines[1].AL_PlaceOfSupplyType);
				AssettFPOS(invoice.Lines[1].AL_PlaceOfSupply, invoice.Lines[1].AL_PlaceOfSupplyType);

				void AssettFPOS(ZString fpos, ZString fposType)
				{
					AssertEquals("PlaceOfSupply", placeofSupplyCode, fpos);
					AssertEquals("placeofSupplyCodeType", placeofSupplyCodeType, fposType);
				}
			}
		}

		public void TestSavingInvoiceRelatedRequest()
		{
			var invoiceFactory = new BusinessObjectFactory();
			var objectCreatorForInvoiceFactory = new TestObjectCreator(invoiceFactory);
			var shipment = objectCreatorForInvoiceFactory.CreateShipment("S001");
			var job = objectCreatorForInvoiceFactory.CreateJob(shipment, createWithMutex: false);
			invoiceFactory.Save();

			var dummyBizoToCheckFactorySaving = invoiceFactory.New<DummyBusinessObject>();
			var invoice = objectCreatorForInvoiceFactory.CreateInvoice(typeof(APInvoice));
			var line = objectCreatorForInvoiceFactory.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 100);
			TestApprovalRequest.InitializeInvoiceRelated(invoice);
			TestApprovalRequest.PrepareFoSaving();

			Assert("Precondition: Invoice should not be converted to incomplete on this stage.", !invoice.IsIncompleteInvoice);
			Factory.Save();
			Assert("TestApprovalRequest.IsInDatabase.", TestApprovalRequest.IsInDatabase);
			Assert("Invoice should be saved as incomplete and on request saving.", invoice.IsIncompleteInvoice);
			Assert("Invoice.IsInDatabase.", invoice.IsInDatabase);
			Assert("dummyBizoToCheckFactorySaving.IsInDatabase as proof that invoice factory was not saved.", !dummyBizoToCheckFactorySaving.IsInDatabase);
			var incompleteInvoice = objectCreatorForInvoiceFactory.CreateInvoice(typeof(APInvoice));
			objectCreatorForInvoiceFactory.CreateInvoiceLine(incompleteInvoice, job, TestObjectCreator.CC1, 100);
			incompleteInvoice.SaveAsIncomplete();

			AssertEquals("AH_TransactionCount for transaction with approval request should be increased to allow to save incomplete invoice with the save transaction number as validation allows this.",
				incompleteInvoice.AH_TransactionCount + 1, invoice.AH_TransactionCount);

			string lastErrorMessage = "";
			var (linkedInvoice, restoreSavedDataResult) = TestApprovalRequest.GetLinkedInvoice();
			if (restoreSavedDataResult != null && restoreSavedDataResult.Result != InvoicingBase.RestoreSavedDataResult.ResultType.Success)
			{
				lastErrorMessage = restoreSavedDataResult.Error;
			}
			AssertEquals("Postcondition: No errors on invoice restoring", "", lastErrorMessage);
			AssertNotNull(linkedInvoice);
			Assert("linked invoice should be incomplete invoice", linkedInvoice.IsIncompleteInvoice);
			AssertEquals("linked invoice should be restored and so have lines", 1, linkedInvoice.Lines.Count);

			linkedInvoice.MoveFromIncompleteToPayableLedger();
			Factory.Save();
			Assert("Precondition: linked invoice is already completed.", !linkedInvoice.IsIncompleteInvoice);
			(linkedInvoice, restoreSavedDataResult) = TestApprovalRequest.GetLinkedInvoice();
			if (restoreSavedDataResult != null && restoreSavedDataResult.Result != InvoicingBase.RestoreSavedDataResult.ResultType.Success)
			{
				lastErrorMessage = restoreSavedDataResult.Error;
			}
			AssertNotNull("GetLinkedInvoice for posted invoices still should return the invoice, just without restoring.", linkedInvoice);
		}

		public void TestSavingInvoiceRelatedRequest_InvalidSaving()
		{
			var invoiceFactory = new BusinessObjectFactory();
			var objectCreatorForInvoiceFactory = new TestObjectCreator(invoiceFactory);
			var shipment = objectCreatorForInvoiceFactory.CreateShipment("S001");
			var job = objectCreatorForInvoiceFactory.CreateJob(shipment, createWithMutex: false);
			invoiceFactory.Save();

			var dummyBizoToCheckFactorySaving = invoiceFactory.New<DummyBusinessObject>();
			var invoice = objectCreatorForInvoiceFactory.CreateInvoice(typeof(APInvoice));
			var line = objectCreatorForInvoiceFactory.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 100);
			TestApprovalRequest.InitializeInvoiceRelated(invoice);

			Factory.ForceCriticalValidationErrorForTestOnly(CriticalValidationErrorType.TransactionHeaderIncorrectOutstandingAmount_12);
			Assert("Precondition: Invoice should not be converted to incomplete on this stage.", !invoice.IsIncompleteInvoice);
			try
			{
				Factory.Save();
				Fail("Exception must be thrown.");
			}
			catch (OnSavingCriticalCheckException)
			{
				ErrorReporter.Clear();
			}
			Assert("Unsuccessful saving: TestApprovalRequest.IsInDatabase.", !TestApprovalRequest.IsInDatabase);
			Assert("Unsuccessful saving: Invoice should not be saved as incomplete as saving was unsuccessful.", !invoice.IsIncompleteInvoice);
			Assert("Unsuccessful saving: Invoice.IsInDatabase.", !invoice.IsInDatabase);
			Assert("Unsuccessful saving: dummyBizoToCheckFactorySaving.IsInDatabase as proof that invoice factory was not saved.", !dummyBizoToCheckFactorySaving.IsInDatabase);
			var incompleteInvoice = objectCreatorForInvoiceFactory.CreateInvoice(typeof(APInvoice));
			objectCreatorForInvoiceFactory.CreateInvoiceLine(incompleteInvoice, job, TestObjectCreator.CC1, 100);
			incompleteInvoice.SaveAsIncomplete();
			AssertEquals("Unsuccessful saving: AH_TransactionCount for transaction with approval request after unsuccessful saving should not be increased.",
				incompleteInvoice.AH_TransactionCount, invoice.AH_TransactionCount);
		}

		public void TestFinalizeCancelling()
		{
			var invoiceFactory = new BusinessObjectFactory();
			var objectCreatorForInvoiceFactory = new TestObjectCreator(invoiceFactory);
			var invoice = objectCreatorForInvoiceFactory.CreateInvoice(typeof(APInvoice), "INV1", organisation: TestObjectCreator.Creditor1);
			objectCreatorForInvoiceFactory.CreateInvoiceLine(invoice, TestObjectCreator.GLHeader1.PK, 100);
			TestApprovalRequest.InitializeInvoiceRelated(invoice);
			TestApprovalRequest.PrepareFoSaving();
			Factory.Save();

			Assert("Precondition: AH_IsCancelled", !invoice.AH_IsCancelled);
			var prevTransactionCountValue = invoice.AH_TransactionCount;
			TestApprovalRequest.FinalizeCancelling();
			Factory.Save();
			Assert("AH_IsCancelled", invoice.AH_IsCancelled);
			AssertEquals("AH_TransactionCount", prevTransactionCountValue + 1, invoice.AH_TransactionCount);

			var anotherInvoiceWithRequest = TestObjectCreator.CreateAPInvoiceWithApprovalRequest<APInvoice>(TestObjectCreator.Creditor1, 10);
			byte transactionCount = 123;
			anotherInvoiceWithRequest.AH_TransactionNum = invoice.AH_TransactionNum;
			anotherInvoiceWithRequest.AH_TransactionCount = transactionCount;
			Factory.Save();
			TestApprovalRequest.FinalizeCancelling();
			Factory.Save();
			AssertEquals("AH_TransactionCount", transactionCount + 1, invoice.AH_TransactionCount);

			anotherInvoiceWithRequest.AH_TransactionCount = byte.MaxValue;
			Factory.Save();
			TestApprovalRequest.FinalizeCancelling();
			Factory.Save();
			AssertEquals("AH_TransactionCount", (byte)1, invoice.AH_TransactionCount);
			AssertEquals(@"Invoice AH_TransactionCount has reached a maximum value. As such invoice parameters won't be unique, no more invoices can be created with: 
Transaction Ledger: 'IN', Type: 'INI', Number: 'INV1', Organisation: 'ZCreditor1', Company: 'EDI'.

Approach with incrementing AH_TransactionCount on approval requests cancelling may require reconsideration.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestGetLinkedInvoiceErrorHandling()
		{
			var invoiceFactory = new BusinessObjectFactory();
			var objectCreatorForInvoiceFactory = new TestObjectCreator(invoiceFactory);
			var invoice = objectCreatorForInvoiceFactory.CreateInvoice(typeof(APInvoice));
			var line = objectCreatorForInvoiceFactory.CreateInvoiceLine(invoice, TestObjectCreator.GLHeader1.PK, 100);
			TestApprovalRequest.InitializeInvoiceRelated(invoice);
			TestApprovalRequest.PrepareFoSaving();
			TestApprovalRequest.Factory.Save();
			Assert("Postcondition: TestApprovalRequest.IsInDatabase.", TestApprovalRequest.IsInDatabase);
			Assert("Postcondition: Invoice should be saved as incomplete and on request saving.", invoice.IsIncompleteInvoice);
			Assert("Postcondition: Invoice.IsInDatabase.", invoice.IsInDatabase);

			BusinessObjectFactory factoryToDelete = new BusinessObjectFactory();
			factoryToDelete.LoadTop1<StmNote>(new ZQuery(StmNoteSchema.ST_ParentID, invoice.PK).AddToFilter(StmNoteSchema.ST_Table, invoice.TableName)).Delete();
			factoryToDelete.Save();

			string lastErrorMessage = "";
			var (linkedInvoice, restoreSavedDataResult) = TestApprovalRequest.GetLinkedInvoice();
			if (restoreSavedDataResult != null && restoreSavedDataResult.Result != InvoicingBase.RestoreSavedDataResult.ResultType.Success)
			{
				lastErrorMessage = restoreSavedDataResult.Error;
			}
			AssertEquals("Incomplete invoice restoration use error message handler", "There is a problem with the transaction and this transaction can no longer be used. You will need to delete this transaction from your system", lastErrorMessage);
			AssertNull(linkedInvoice);
		}

		public void TestGetLinkedInvoice_ReturnsNull_WhenRestoreSavedDataReturnsSuccessWithErrors()
		{
			var invoiceFactory = new BusinessObjectFactory();
			var objectCreatorForInvoiceFactory = new TestObjectCreator(invoiceFactory);
			var invoice = objectCreatorForInvoiceFactory.CreateInvoice(typeof(APInvoice));
			var testChargeCode = objectCreatorForInvoiceFactory.CreateChargeCode("TESTCODE", createWithoutZZ: true);
			var line = objectCreatorForInvoiceFactory.CreateInvoiceLine(invoice, testChargeCode.PK, 100);
			TestApprovalRequest.InitializeInvoiceRelated(invoice);
			TestApprovalRequest.PrepareFoSaving();
			TestApprovalRequest.Factory.Save();

			Assert("Pre-condition: Is incompleteInvoice", invoice.IsIncompleteInvoice);

			BusinessObjectFactory factoryToDelete = new BusinessObjectFactory();
			factoryToDelete.Load<AccChargeCode>(testChargeCode.PK).Delete();
			factoryToDelete.Save();

			string lastErrorMessage = "";
			var (linkedInvoice, restoreSavedDataResult) = TestApprovalRequest.GetLinkedInvoice();
			if (restoreSavedDataResult != null && restoreSavedDataResult.Result != InvoicingBase.RestoreSavedDataResult.ResultType.Success)
			{
				lastErrorMessage = restoreSavedDataResult.Error;
			}
			AssertNull("Ideally this case should result in returning an invoice and users notified of the errors, but the current functionality is to return null for invoice.", linkedInvoice);
			AssertEquals("Reported error", "Could not find generic charge TESTCODE", lastErrorMessage);
		}

		public void TestFormattedChargeDetails()
		{
			AssertEquals("", TestApprovalRequest.FormattedChargeDetails);

			var charge1 = TestApprovalRequest.PostingDetails.Charges.AddNew();
			charge1.ChargeCode = "CUR1";
			charge1.LocalCostAmount = 1M;
			var charge2 = TestApprovalRequest.PostingDetails.Charges.AddNew();
			charge2.ChargeCode = "CUR2";
			charge2.LocalCostAmount = 10M;
			var charge3 = TestApprovalRequest.PostingDetails.Charges.AddNew();
			charge3.ChargeCode = "CUR1";
			charge3.LocalCostAmount = 100M;

			var charge3_2 = TestApprovalRequest.PostingDetails.Charges.AddNew();
			charge3_2.ChargeCode = "CUR1";
			charge3_2.LocalCostAmount = 200M;
			var charge1_2 = TestApprovalRequest.PostingDetails.Charges.AddNew();
			charge1_2.ChargeCode = "CUR1";
			charge1_2.LocalCostAmount = 2M;
			var charge2_2 = TestApprovalRequest.PostingDetails.Charges.AddNew();
			charge2_2.ChargeCode = "CUR2";
			charge2_2.LocalCostAmount = 20M;
			Factory.Save();

			var testApprovalRequest_InAnotherFactory = new BusinessObjectFactory().Load<APInvoiceChargesApprovalRequest>(TestApprovalRequest.PK);
			AssertEquals("CUR1 303.00, CUR2 30.00", testApprovalRequest_InAnotherFactory.FormattedChargeDetails);

			charge3_2.LocalCostAmount = 300M;
			Factory.Save();

			AssertEquals("CUR1 403.00, CUR2 30.00", testApprovalRequest_InAnotherFactory.FormattedChargeDetails);
		}

		public override void TestSetDefaultValues()
		{
			base.TestSetDefaultValues();

			var approvalForTest = (APInvoiceChargesApprovalRequest)GetNewBusinessObject();
			AssertEquals("XP_ApprovalType", Constants.GenApprovalRequestApprovalType.APInvoiceCharges, approvalForTest.XP_ApprovalType);
		}

		public void TestSecurityRight_AllowUntickAutoTickedFinalFlag()
		{
			const string expectedError = @"You do not have appropriate security rights to un-tick this flag.
Please contact your system administrator for the following security right: Manage > Payables > Payables Transactions > New Transactions > Invoice > Allow Untick Auto-ticked Final Flag";
			Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed = true;
			Env.Security.AllowUntickAutoTickedFinalFlag.IsAllowed = false;
			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.JobAndChargeCode);

			var invoiceFactory = new BusinessObjectFactory();
			var objectCreatorForInvoiceFactory = new TestObjectCreator(invoiceFactory);
			var shipment = objectCreatorForInvoiceFactory.CreateShipment("S00001004", "AUSYD", "NZAKL");
			var job = objectCreatorForInvoiceFactory.CreateJob(shipment, false);
			invoiceFactory.Save();

			var invoice = objectCreatorForInvoiceFactory.CreateInvoice(typeof(APInvoice), "INV1", organisation: objectCreatorForInvoiceFactory.Creditor1);
			var line1 = (APInvoiceLine)invoice.Lines.AddNew();
			line1.AL_AC = objectCreatorForInvoiceFactory.CC1.PK;
			line1.AL_JH = job.PK;
			line1.AL_LocalExTaxAmount = 150m;
			Assert("Should not be ticked, 150 > 100", !line1.AL_IsFinalCharge);
			line1.AL_IsFinalCharge = true;

			var line2 = (APInvoiceLine)invoice.Lines.AddNew();
			line2.AL_AC = objectCreatorForInvoiceFactory.CC2.PK;
			line2.AL_JH = job.PK;
			line2.AL_LocalExTaxAmount = 50m;
			Assert("Should be ticked, 50 < 100", line2.AL_IsFinalCharge);

			TestApprovalRequest.InitializeInvoiceRelated(invoice);
			TestApprovalRequest.XP_ReasonDescription = "Desc";
			TestApprovalRequest.PrepareFoSaving();
			Factory.Save();

			var testApprovalRequest_InAnotherFactory = new BusinessObjectFactory().Load<APInvoiceChargesApprovalRequest>(TestApprovalRequest.PK);
			string lastErrorMessage = "";
			var (linkedInvoice, restoreSavedDataResult) = TestApprovalRequest.GetLinkedInvoice();
			if (restoreSavedDataResult != null && restoreSavedDataResult.Result != RestoreSavedDataResult.ResultType.Success)
			{
				lastErrorMessage = restoreSavedDataResult.Error;
			}
			AssertEquals("Postcondition: No errors on invoice restoring", "", lastErrorMessage);
			AssertNotNull(linkedInvoice);
			Assert("linked invoice should be incomplete invoice", linkedInvoice.IsIncompleteInvoice);
			AssertEquals("linked invoice should be restored and so have lines", 2, linkedInvoice.Lines.Count);

			var linkedInvoiceLine1 = linkedInvoice.Lines.Cast<InvoicingLineBase>().FirstOrDefault(x => x.AL_LocalExTaxAmount == 150m);
			var linkedInvoiceLine2 = linkedInvoice.Lines.Cast<InvoicingLineBase>().FirstOrDefault(x => x.AL_LocalExTaxAmount == 50m);
			Assert("Should be equal to the value in XML", linkedInvoiceLine1.AL_IsFinalCharge);
			AssertNoErrors("Shouldn't have any error", linkedInvoiceLine1.AL_IsFinalChargeInfo);
			Assert("Should be equal to the value in XML", linkedInvoiceLine2.AL_IsFinalCharge);
			AssertNoErrors("Shouldn't have any error", linkedInvoiceLine2.AL_IsFinalChargeInfo);

			linkedInvoiceLine1.AL_IsFinalCharge = false;
			AssertNoErrors("Shouldn't have any error, because the flag is ticked manually", linkedInvoiceLine1.AL_IsFinalChargeInfo);
			linkedInvoiceLine2.AL_IsFinalCharge = false;
			AssertHasError("Should have the error, because we don't have the security right to untick the automatically ticked flag", linkedInvoiceLine2.AL_IsFinalChargeInfo, expectedError);

			Env.Security.AllowUntickAutoTickedFinalFlag.IsAllowed = true;
			linkedInvoiceLine2.Validation.ValidateAL_IsFinalCharge();
			AssertNoErrors("Shouldn't have any error, because we have the security right", linkedInvoiceLine2.AL_IsFinalChargeInfo);
		}

		public void TestDraftInvoiceWillBeSavedWhenSavingUnapprovelInvoice()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1", organisation: TestObjectCreator.Creditor1);
			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.GLHeader1.PK, 100);

			var mockStatusUpdater = new Mock<IAccDraftInvoiceStatusChangeValidator>();
			mockStatusUpdater.Setup(h => h.CanChangeStatusTo(It.IsAny<AccDraftInvoiceHeader>(), It.IsAny<string>())).Returns(new AccDraftInvoiceStatusUpdateValidationResult { CanUpdate = true });
			ObjectFactory.Substitute(mockStatusUpdater.Object);

			var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			draftInvoice.AIH_AH_PostedTransactionHeader = invoice.PK;
			draftInvoice.AIH_Status = Core.Constants.AccDraftInvoiceHeaderStatus.Processed;

			TestApprovalRequest.InitializeInvoiceRelated(invoice);
			TestApprovalRequest.PrepareFoSaving();
			Factory.Save();

			AssertEquals("PreCondition", true, TestApprovalRequest.IsInDatabase);
			AssertEquals("PreCondition", true, invoice.IsInDatabase);

			CombineAssertions("DraftInvoice should be saved.", () => {
				AssertEquals("IsInDatabase", true, draftInvoice.IsInDatabase);
				var sourceDraftInvoice = new BusinessObjectFactory().Load<AccDraftInvoiceHeader>(draftInvoice.PK);
				AssertEquals("AIH_AH_PostedTransactionHeader", invoice.PK, sourceDraftInvoice.AIH_AH_PostedTransactionHeader);
				AssertEquals("AIH_Status", Core.Constants.AccDraftInvoiceHeaderStatus.Processed, sourceDraftInvoice.AIH_Status);
			});
		}

		protected override string GetExpectedEmailSubjectForTestSendEmail(APInvoiceChargesApprovalRequest request) => "AP Invoice (Creditor: 'Org1' and number: '123') approval request for Job Number 'S001'";

		protected override void SetupRequestForTestSendEmail(APInvoiceChargesApprovalRequest request)
		{
			base.SetupRequestForTestSendEmail(request);

			TestApprovalRequest.PostingDetails.Creditor = "Org1";
			TestApprovalRequest.PostingDetails.TransactionNumber = "123";
			TestApprovalRequest.PostingDetails.MaxAmountToApprove = 33;

			var charge = TestApprovalRequest.PostingDetails.Charges.AddNew();
			charge.LocalCostAmount = 11;

			charge = TestApprovalRequest.PostingDetails.Charges.AddNew();
			charge.LocalCostAmount = 22;
		}

		public void TestRegisterInvoiceAndRelatedBizoNotToBeSaved()
		{
			var bizoList = new List<BusinessObject>();
			bizoList.Add(Factory.NewWithValidTestData<AccTransactionHeader>());
			bizoList.Add(Factory.NewWithValidTestData<AccTransactionLines>());
			bizoList.Add(Factory.NewWithValidTestData<JobCharge>());
			bizoList.Add(Factory.NewJobWithValidTestDataForTesting<JobHeader>());
			bizoList.Add(Factory.NewWithValidTestData<JobChargeRevRecognition>());
			bizoList.Add(Factory.NewWithValidTestData<JobConsolCost>());
			bizoList.Add(Factory.NewWithValidTestData<ExchangeRate>());

			var approvalRequest = Factory.NewWithValidTestData<APInvoiceChargesApprovalRequest>();
			approvalRequest.RegisterInvoiceAndRelatedBizoNotToBeSaved();
			Factory.Save();

			CombineAssertions(() =>
			{
				Assert("approval request is saved", approvalRequest.IsInDatabase);
				foreach (var bizo in bizoList)
				{
					Assert("bizo is not saved", !bizo.IsInDatabase);
				}
			});
		}

		protected override ZBool ShouldAddLogsOnTransactionWithApprovalRequestStatusChange => true;
		protected override ZString Ledger => LedgerTypes.IncompleteTransactions;
		protected override ZString InvoiceBasedTransactionType => TransactionTypes.IncompleteInvoice;
		protected override ZString CreditNoteBasedTransactionType => TransactionTypes.IncompleteCreditNote;
		protected override ZString RequestApprovedReference => "Approved For Posting";

		protected override (InvoicingBase, GenApprovalRequest) CreateTransactionAndApprovalRequest(ZString transactionType, ZString transactionNumber)
		{
			Type type = typeof(BusinessObject);
			if (InvoiceBasedTransactionType == transactionType)
			{
				type = typeof(APInvoice);
			}
			else if (CreditNoteBasedTransactionType == transactionType)
			{
				type = typeof(APCreditNote);
			}
			InvoicingBase invoice = TestObjectCreator.CreateAPInvoiceForApprovalRequest(type, TestObjectCreator.Creditor1, 100, transactionNumber);
			var approvalRequestInOtherFactory = TestObjectCreator.CreateAndSaveApprovalRequestForInvoiceInNewFactoryAsInProduction(invoice);
			var approvalRequest = Factory.Load<APInvoiceChargesApprovalRequest>(approvalRequestInOtherFactory.PK);

			return (invoice, approvalRequest);
		}
	}
}
