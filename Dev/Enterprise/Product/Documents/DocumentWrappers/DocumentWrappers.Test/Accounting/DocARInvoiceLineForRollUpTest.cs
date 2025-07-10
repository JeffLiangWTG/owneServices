using System;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Accounting.DocRollUpSort;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocARInvoiceLineForRollUp))]
	sealed class DocARInvoiceLineForRollUpTest : AccountingDocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocARInvoiceLineForRollUp.New(Factory) };
		}

		public void TestContainerNumbers()
		{
			AssertEquals("ContainerNumbers", ZString.Empty, InvoiceLineWrapper.ContainerNumbers);
		}

		public void TestExchangeRateAndAmount()
		{
			AssertEquals("ExchangeRateAndAmount", ZString.Empty, InvoiceLineWrapper.ExchangeRateAndAmount);
		}

		public void TestChargeOSAmountAndCurrency()
		{
			AssertEquals("ChargeOSAmountAndCurrency", ZString.Empty, InvoiceLineWrapper.ChargeOSAmountAndCurrency);
		}

		public void TestChargeOSAmountForCLC()
		{
			AssertEquals("ChargeOSAmountForCLC", 0M, InvoiceLineWrapper.ChargeOSAmountForCLC);
		}

		public void TestChargeExchangeRate()
		{
			AssertEquals("ChargeExchangeRate", 0M, InvoiceLineWrapper.ChargeExchangeRate);
		}

		public void TestLineDescription()
		{
			ZString lineDescription = "LineDescription";
			InvoiceLineWrapper.SetLineDescription(lineDescription);
			AssertEquals("LineDescription", lineDescription, InvoiceLineWrapper.LineDescription);
			AssertEquals("LineDescriptionAndExchangeRate", lineDescription + " ", InvoiceLineWrapper.LineDescriptionAndExchangeRate);
		}

		public void TestOSExTaxAmount()
		{
			ZDecimal oSExTaxAmount = 123.00M;
			InvoiceLineWrapper.OSExTaxAmount = oSExTaxAmount;
			AssertEquals("OSExTaxAmount", oSExTaxAmount, InvoiceLineWrapper.OSExTaxAmount);
		}

		public void TestSequence()
		{
			ZShort sequence = 1;
			InvoiceLineWrapper.Sequence = sequence;
			AssertEquals("Sequence", sequence, InvoiceLineWrapper.Sequence);
		}

		public void TestOSTaxDisplay()
		{
			ZString oSTaxDisplay = new ZString("OSTaxDisplay");
			InvoiceLineWrapper.OSTaxDisplay = oSTaxDisplay;
			AssertEquals("OSTaxDisplay", oSTaxDisplay, InvoiceLineWrapper.OSTaxDisplay);
		}

		public void TestTaxAmountDisplay()
		{
			ZString taxAmountDisplay = new ZString("TaxAmountDisplay");
			InvoiceLineWrapper.TaxAmountDisplay = taxAmountDisplay;
			AssertEquals("TaxAmountDisplay", taxAmountDisplay, InvoiceLineWrapper.TaxAmountDisplay);
		}

		public void TestTaxRateAsterisks()
		{
			AssertEquals("TaxRateAsterisks", ZString.Empty, InvoiceLineWrapper.TaxRateAsterisks);
		}

		public void TestTaxRateAsterisksAsNumbers()
		{
			AssertEquals("TaxRateAsterisksAsNumbers", ZString.Empty, InvoiceLineWrapper.TaxRateAsterisksAsNumbers);
		}

		public void TestOSTaxDisplayNoAsterisks()
		{
			ZString oSTaxDisplay = new ZString("OSTaxDisplay");
			InvoiceLineWrapper.OSTaxDisplay = oSTaxDisplay;
			AssertEquals("OSTaxDisplay", oSTaxDisplay, InvoiceLineWrapper.OSTaxDisplayNoAsterisks);
		}

		public void TestJobNumber()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var invoiceLineWrapper = DocARInvoiceLineForRollUp.New(Factory);
			Line.AL_JH = Guid.Empty;
			AssertEquals("The JobNumber should be empty because there is no JobHeader", ZString.Empty, invoiceLineWrapper.JobNumber);
			Line.AL_JH = job.PK;
			invoiceLineWrapper.JobHeader = DocJobHeader.New(job, Factory);
			AssertEquals("The JobNumber should now has value", Line.Job.JH_JobNum, invoiceLineWrapper.JobNumber);
		}

		#region IDocARInvoiceLine Members

		public void TestIsCommentLine()
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_ChargeType = "CMT";
			DocChargeCode docChargeCode = DocChargeCode.New(chargeCode, Factory);
			var invoiceLineWrapper = DocARInvoiceLineForRollUp.New(Factory);
			invoiceLineWrapper.ChargeCode = docChargeCode;
			AssertEquals(true, invoiceLineWrapper.IsCommentLine);
			AssertEquals("", invoiceLineWrapper.Quantity);

			chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_ChargeType = "FRT";
			docChargeCode = DocChargeCode.New(chargeCode, Factory);
			invoiceLineWrapper = DocARInvoiceLineForRollUp.New(Factory);
			invoiceLineWrapper.ChargeCode = docChargeCode;
			AssertEquals(false, invoiceLineWrapper.IsCommentLine);
			AssertEquals("", invoiceLineWrapper.Quantity);

			invoiceLineWrapper = DocARInvoiceLineForRollUp.New(Factory);
			invoiceLineWrapper.ChargeCode = null;
			AssertEquals(false, invoiceLineWrapper.IsCommentLine);
			AssertEquals("", invoiceLineWrapper.Quantity);
		}

		public void TestCreatingUserName()
		{
			AssertEquals("CreatingUserName", ZString.Empty, InvoiceLineWrapper.CreatingUserName);
		}

		public void TestCreatedDate()
		{
			AssertEquals("CreatedDate", ZDateTime.Empty, InvoiceLineWrapper.CreatedDate);
		}

		public void TestChargeCode()
		{
			AssertNull("ChargeCode", InvoiceLineWrapper.ChargeCode);
			var chargeCode = DocChargeCode.New(Factory.NewWithValidTestData<AccChargeCode>(), Factory);
			InvoiceLineWrapper.ChargeCode = chargeCode;
			AssertEquals("ChargeCode", chargeCode, InvoiceLineWrapper.ChargeCode);
		}

		public void TestGLAccount()
		{
			AssertNull("GLAccount", InvoiceLineWrapper.GLAccount);
		}

		public void TestPercentOfGLAccount()
		{
			AssertNull("PercentOfGLAccount", InvoiceLineWrapper.PercentOfGLAccount);
		}

		public void TestInvoice()
		{
			AssertNull("Invoice", InvoiceLineWrapper.Invoice);
		}

		public void TestTaxRate()
		{
			AssertNull("TaxRate", InvoiceLineWrapper.TaxRate);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "AU";
			AccTaxRate rate = AccTaxRate.GetNOTREPORTTaxID(Factory, GlbCompany.CurrentCompany);
			AssertNotNull("rate", rate);

			InvoiceLineWrapper.TaxRate = DocTaxRate.New(rate, Factory);
			AssertNotNull("TaxRate", InvoiceLineWrapper.TaxRate);
			AssertEquals("TaxRate.AccTaxRate", rate, InvoiceLineWrapper.TaxRate.AccTaxRate);
		}

		public void TestWithholdingTaxRate()
		{
			AssertNull("WithholdingTaxRate", InvoiceLineWrapper.WithholdingTaxRate);
		}

		public void TestExchangeRate()
		{
			AssertEquals("ExchangeRate", 0M, InvoiceLineWrapper.ExchangeRate);
		}

		public void TestBranch()
		{
			AssertNull("Branch", InvoiceLineWrapper.Branch);
		}

		public void TestDepartment()
		{
			AssertNull("Department", InvoiceLineWrapper.Department);
		}

		public void TestGSTVAT()
		{
			AssertEquals("GSTVAT", 0M, InvoiceLineWrapper.GSTVAT);
		}

		public void TestOSTaxAmount()
		{
			AssertEquals("OSTaxAmount", 0M, InvoiceLineWrapper.OSTaxAmount);
		}

		public void TestJobHeader()
		{
			JobHeader fJobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			DocJobHeader fDocJobHeader = DocJobHeader.New(fJobHeader, Factory);
			InvoiceLineWrapper.JobHeader = fDocJobHeader;
			AssertEquals("JobHeader", fDocJobHeader, InvoiceLineWrapper.JobHeader);
		}

		public void TestJobTypeForPeriodicInvoice()
		{
			AssertEquals("JobTypeForPeriodicInvoice", ZString.Empty, InvoiceLineWrapper.JobTypeForPeriodicInvoice);
		}

		public void TestAmountSplittedByChargeCode()
		{
		}

		public void TestOperationsJob()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var chargeCodePK = objectCreator.CC1.PK;
			DocARInvoiceLineForRollUp invoiceLineWrapper = null;
			Action<IJobInvoicingPlugIn> createNewJob = (IJobInvoicingPlugIn plugin) =>
			{
				objectCreator = new TestObjectCreator(Factory);
				JobHeader jobHeader = objectCreator.CreateJob(plugin, false);
				invoiceLineWrapper = DocARInvoiceLineForRollUp.New(Factory);
				invoiceLineWrapper.JobHeader = DocJobHeader.New(jobHeader, Factory);
			};
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S1000";
			createNewJob(shipment);
			AssertEquals(typeof(FreightWrapperFromShipment), invoiceLineWrapper.OperationsJob.GetType());
			AssertEquals("S1000", invoiceLineWrapper.OperationsJob.JobNumber);

			CFSShipment cfsShipment = Factory.New<CFSShipment>();
			cfsShipment.JS_UniqueConsignRef = "S2000";
			cfsShipment.JS_IsCFSRegistered = true;
			cfsShipment.JS_IsForwardRegistered = false;
			createNewJob(cfsShipment);
			AssertEquals(typeof(FreightWrapperFromShipment), invoiceLineWrapper.OperationsJob.GetType());
			AssertEquals("S2000", invoiceLineWrapper.OperationsJob.JobNumber);

			CFSLoadListConsol loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_UniqueConsignRef = "C1000";
			createNewJob(loadList);
			AssertEquals(typeof(FreightWrapperFromCFSLoadList), invoiceLineWrapper.OperationsJob.GetType());
			AssertEquals("C1000", invoiceLineWrapper.OperationsJob.JobNumber);

			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_ConsignmentID = "3333";
			createNewJob(cartage);
			AssertEquals(typeof(FreightWrapperFromCartage), invoiceLineWrapper.OperationsJob.GetType());
			AssertEquals("3333", invoiceLineWrapper.OperationsJob.JobNumber);

			var containerRegistrationJob = Factory.NewWithValidTestData<CFSContainer>();
			containerRegistrationJob.JC_ContainerJobID = "D000010001";
			createNewJob(containerRegistrationJob);
			AssertEquals(typeof(FreightWrapperFromCFSContainer), invoiceLineWrapper.OperationsJob.GetType());
			AssertEquals("D000010001", invoiceLineWrapper.OperationsJob.JobNumber);
		}

		#region TestOperationsJob_Warehouse

		public void TestOperationsJob_Warehouse()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ExternalReference = "External Reference";
			helper.CreateAccountingDataWithNoCharge(receive);

			var job = new Job.Loader(receive).Load(true);
			var arLine1 = CreateARLine(job);
			arLine1.AL_AC = chargeCode.PK;

			var charge1 = objectCreator.CreateCharge(arLine1);
			var attrib1 = charge1.JobChargeAttributes.AddNew();
			attrib1.EC_Name = JobChargeAttribTypeList.Codes.DocketReference;
			attrib1.EC_Value = "External Reference";

			var receiveWrapper = DocARInvoiceLineForRollUp.New(Factory);
			receiveWrapper.JobHeader = DocJobHeader.New(receive.JobHeader, Factory);
			var charge1ParentWrapper = receiveWrapper.OperationsJob as FreightWrapperFromWhsBO;
			AssertEquals(receive, charge1ParentWrapper.WrappedObject);

			var invoice = Factory.NewWithValidTestData<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			helper.CreateAccountingDataWithCharge(invoice);

			var arLine2 = CreateARLine(invoice.JobHeader);
			arLine2.AL_AC = chargeCode.PK;
			var charge2 = objectCreator.CreateCharge(arLine2);

			var attrib2 = charge2.JobChargeAttributes.AddNew();
			attrib2.EC_Name = JobChargeAttribTypeList.Codes.DocketReference;
			attrib2.EC_Value = "External Reference";

			var invoiceWrapper = DocARInvoiceLineForRollUp.New(Factory);
			invoiceWrapper.JobHeader = DocJobHeader.New(invoice.JobHeader, Factory);
			var invoiceParentWrapper = invoiceWrapper.OperationsJob as FreightWrapperFromWhsInvoice;
			AssertEquals(invoice, invoiceParentWrapper.WrappedObject);
		}

		ARInvoiceLine CreateARLine(JobHeader job)
		{
			var invoice = Factory.New<ARInvoice>();
			var line = (ARInvoiceLine)invoice.Lines.AddNew();
			line.AL_LineType = TransactionLineTypes.Revenue;
			line.AL_JH = job.PK;
			return line;
		}

		#endregion

		public void TestLineAmount()
		{
			AssertEquals("LineAmount", 0M, InvoiceLineWrapper.LineAmount);
		}

		public void TestLineType()
		{
			AssertEquals("LineType", ZString.Empty, InvoiceLineWrapper.LineType);
		}

		public void TestOrganisation()
		{
			AssertNull("Organisation", InvoiceLineWrapper.Organisation);
		}

		public void TestOSAmount()
		{
			AssertEquals("OSAmount", 0M, InvoiceLineWrapper.OSAmount);
		}

		public void TestOSUnitPrice()
		{
			AssertEquals("OSUnitPrice", 0M, InvoiceLineWrapper.OSUnitPrice);
		}

		public void TestPercentageOfPeriod()
		{
			AssertEquals("PercentageOfPeriod", 0, InvoiceLineWrapper.PercentageOfPeriod);
		}

		public void TestPostDate()
		{
			AssertEquals("PostDate", ZDateTime.Empty, InvoiceLineWrapper.PostDate);
		}

		public void TestPostPeriod()
		{
			AssertEquals("PostPeriod", 0, InvoiceLineWrapper.PostPeriod);
		}

		public void TestPostToGL()
		{
			AssertEquals("PostToGL", ZBool.False, InvoiceLineWrapper.PostToGL);
		}

		public void TestPreventInvoicePrintGrouping()
		{
			AssertEquals("PreventInvoicePrintGrouping", ZBool.False, InvoiceLineWrapper.PreventInvoicePrintGrouping);
		}

		public void TestReverseDate()
		{
			AssertEquals("ReverseDate", ZDateTime.Empty, InvoiceLineWrapper.ReverseDate);
		}

		public void TestReversePeriod()
		{
			AssertEquals("ReversePeriod", 0, InvoiceLineWrapper.ReversePeriod);
		}

		public void TestReverseToGL()
		{
			AssertEquals("ReverseToGL", ZBool.False, InvoiceLineWrapper.ReverseToGL);
		}

		public void TestCurrency()
		{
			AssertNull("Currency", InvoiceLineWrapper.Currency);

			InvoiceLineWrapper.Currency = DocCurrency.New(GlbCompany.CurrentCompany.LocalCurrency, Factory);
			AssertNotNull("Currency", InvoiceLineWrapper.Currency);
			AssertEquals("Should be the same currency", GlbCompany.CurrentCompany.LocalCurrency.RX_Code, InvoiceLineWrapper.Currency.Code);
		}

		public void TestUnitPrice()
		{
			AssertEquals("UnitPrice", 0M, InvoiceLineWrapper.UnitPrice);
		}

		public void TestUnitQty()
		{
			AssertEquals("UnitQty", 0, InvoiceLineWrapper.UnitQty);
		}

		public void TestWithholdingTax()
		{
			AssertEquals("WithholdingTax", 0M, InvoiceLineWrapper.WithholdingTax);
		}

		public void TestShipment()
		{
			AssertNull("Shipment", InvoiceLineWrapper.Shipment);
		}

		public void TestFKToShipment()
		{
			AssertEquals("FKToShipment", ZString.Empty, InvoiceLineWrapper.FKToShipment);
		}

		public void TestHeaderOrganisation()
		{
			AssertNull("HeaderOrganisation", InvoiceLineWrapper.HeaderOrganisation);
		}

		public void TestHeaderCurrency()
		{
			AssertNull("HeaderCurrency", InvoiceLineWrapper.HeaderCurrency);
		}

		public void TestLocalAmountAndTax()
		{
			AssertEquals("WithholdingTax", 0M, InvoiceLineWrapper.LocalAmountAndTax);
		}

		public void TestCharge()
		{
			AssertNull("Charge", InvoiceLineWrapper.Charge);
		}

		public void TestOSExTaxAmountForTotal()
		{
			ZDecimal oSExTaxAmount = 123.00M;
			InvoiceLineWrapper.OSExTaxAmount = oSExTaxAmount;
			AssertEquals("OSExTaxAmountForTotal", oSExTaxAmount, InvoiceLineWrapper.OSExTaxAmountForTotal);

			PrepareInvoiceLineForTotalAmount(InvoiceLineWrapper, setCommentChargeCode: true, setSpacerLine: false, setSubTotalLine: false);
			AssertEquals("OSExTaxAmountForTotal", 0M, InvoiceLineWrapper.OSExTaxAmountForTotal);

			PrepareInvoiceLineForTotalAmount(InvoiceLineWrapper, setCommentChargeCode: false, setSpacerLine: true, setSubTotalLine: false);
			AssertEquals("OSExTaxAmountForTotal", 0M, InvoiceLineWrapper.OSExTaxAmountForTotal);

			PrepareInvoiceLineForTotalAmount(InvoiceLineWrapper, setCommentChargeCode: false, setSpacerLine: false, setSubTotalLine: true);
			AssertEquals("OSExTaxAmountForTotal", 0M, InvoiceLineWrapper.OSExTaxAmountForTotal);
		}

		public void TestOSAmountForTotal()
		{
			ZDecimal oSExTaxAmount = 123.00M;
			InvoiceLineWrapper.OSExTaxAmount = oSExTaxAmount;
			AssertEquals("OSAmountForTotal", oSExTaxAmount, InvoiceLineWrapper.OSAmountForTotal);

			PrepareInvoiceLineForTotalAmount(InvoiceLineWrapper, setCommentChargeCode: true, setSpacerLine: false, setSubTotalLine: false);
			AssertEquals("OSAmountForTotal", 0M, InvoiceLineWrapper.OSAmountForTotal);

			PrepareInvoiceLineForTotalAmount(InvoiceLineWrapper, setCommentChargeCode: false, setSpacerLine: true, setSubTotalLine: false);
			AssertEquals("OSAmountForTotal", 0M, InvoiceLineWrapper.OSAmountForTotal);

			PrepareInvoiceLineForTotalAmount(InvoiceLineWrapper, setCommentChargeCode: false, setSpacerLine: false, setSubTotalLine: true);
			AssertEquals("OSAmountForTotal", 0M, InvoiceLineWrapper.OSAmountForTotal);
		}

		public void TestLineAmountForTotal()
		{
			InvoiceLineWrapper.LineAmount = 123.00M;
			AssertEquals("LineAmountForTotal", 123.00M, InvoiceLineWrapper.LineAmountForTotal);

			PrepareInvoiceLineForTotalAmount(InvoiceLineWrapper, setCommentChargeCode: true, setSpacerLine: false, setSubTotalLine: false);
			AssertEquals("LineAmountForTotal", 0M, InvoiceLineWrapper.LineAmountForTotal);

			PrepareInvoiceLineForTotalAmount(InvoiceLineWrapper, setCommentChargeCode: false, setSpacerLine: true, setSubTotalLine: false);
			AssertEquals("LineAmountForTotal", 0M, InvoiceLineWrapper.LineAmountForTotal);

			PrepareInvoiceLineForTotalAmount(InvoiceLineWrapper, setCommentChargeCode: false, setSpacerLine: false, setSubTotalLine: true);
			AssertEquals("LineAmountForTotal", 0M, InvoiceLineWrapper.LineAmountForTotal);
		}

		public void TestGSTVATForTotal()
		{
			InvoiceLineWrapper.GSTVAT = 123.00M;
			AssertEquals("GSTVATForTotal", 123.00M, InvoiceLineWrapper.GSTVATForTotal);

			PrepareInvoiceLineForTotalAmount(InvoiceLineWrapper, setCommentChargeCode: true, setSpacerLine: false, setSubTotalLine: false);
			AssertEquals("GSTVATForTotal", 0M, InvoiceLineWrapper.GSTVATForTotal);

			PrepareInvoiceLineForTotalAmount(InvoiceLineWrapper, setCommentChargeCode: false, setSpacerLine: true, setSubTotalLine: false);
			AssertEquals("GSTVATForTotal", 0M, InvoiceLineWrapper.GSTVATForTotal);

			PrepareInvoiceLineForTotalAmount(InvoiceLineWrapper, setCommentChargeCode: false, setSpacerLine: false, setSubTotalLine: true);
			AssertEquals("GSTVATForTotal", 0M, InvoiceLineWrapper.GSTVATForTotal);
		}

		public void TestLocalAmountAndTaxForTotal()
		{
			InvoiceLineWrapper.LineAmount = 123.00M;
			AssertEquals("LocalAmountAndTaxForTotal", 123.00M, InvoiceLineWrapper.LocalAmountAndTaxForTotal);

			PrepareInvoiceLineForTotalAmount(InvoiceLineWrapper, setCommentChargeCode: true, setSpacerLine: false, setSubTotalLine: false);
			AssertEquals("LocalAmountAndTaxForTotal", 0M, InvoiceLineWrapper.LocalAmountAndTaxForTotal);

			PrepareInvoiceLineForTotalAmount(InvoiceLineWrapper, setCommentChargeCode: false, setSpacerLine: true, setSubTotalLine: false);
			AssertEquals("LocalAmountAndTaxForTotal", 0M, InvoiceLineWrapper.LocalAmountAndTaxForTotal);

			PrepareInvoiceLineForTotalAmount(InvoiceLineWrapper, setCommentChargeCode: false, setSpacerLine: false, setSubTotalLine: true);
			AssertEquals("LocalAmountAndTaxForTotal", 0M, InvoiceLineWrapper.LocalAmountAndTaxForTotal);
		}

		public void TestTaxAmountDisplayForTotal()
		{
			InvoiceLineWrapper.TaxAmountDisplay = "***";
			AssertEquals("TaxAmountDisplayForTotal", "***", InvoiceLineWrapper.TaxAmountDisplayForTotal);

			PrepareInvoiceLineForTotalAmount(InvoiceLineWrapper, setCommentChargeCode: true, setSpacerLine: false, setSubTotalLine: false);
			AssertEquals("TaxAmountDisplayForTotal", "", InvoiceLineWrapper.TaxAmountDisplayForTotal);

			PrepareInvoiceLineForTotalAmount(InvoiceLineWrapper, setCommentChargeCode: false, setSpacerLine: true, setSubTotalLine: false);
			AssertEquals("TaxAmountDisplayForTotal", "", InvoiceLineWrapper.TaxAmountDisplayForTotal);

			PrepareInvoiceLineForTotalAmount(InvoiceLineWrapper, setCommentChargeCode: false, setSpacerLine: false, setSubTotalLine: true);
			AssertEquals("TaxAmountDisplayForTotal", "", InvoiceLineWrapper.TaxAmountDisplayForTotal);
		}

		public void TestTaxGroupCode()
		{
			Assert(InvoiceLineWrapper.TaxGroupCode.IsEmpty);
		}

		public void TestOSTaxDisplayForTotal()
		{
			InvoiceLineWrapper.OSTaxDisplay = "***";
			AssertEquals("OSTaxDisplayForTotal", "***", InvoiceLineWrapper.OSTaxDisplayForTotal);

			PrepareInvoiceLineForTotalAmount(InvoiceLineWrapper, setCommentChargeCode: true, setSpacerLine: false, setSubTotalLine: false);
			AssertEquals("OSTaxDisplayForTotal", "", InvoiceLineWrapper.OSTaxDisplayForTotal);

			PrepareInvoiceLineForTotalAmount(InvoiceLineWrapper, setCommentChargeCode: false, setSpacerLine: true, setSubTotalLine: false);
			AssertEquals("OSTaxDisplayForTotal", "", InvoiceLineWrapper.OSTaxDisplayForTotal);

			PrepareInvoiceLineForTotalAmount(InvoiceLineWrapper, setCommentChargeCode: false, setSpacerLine: false, setSubTotalLine: true);
			AssertEquals("OSTaxDisplayForTotal", "", InvoiceLineWrapper.OSTaxDisplayForTotal);
		}

		void PrepareInvoiceLineForTotalAmount(DocARInvoiceLineForRollUp invoiceLineWrapper, bool setCommentChargeCode, bool setSpacerLine, bool setSubTotalLine)
		{
			invoiceLineWrapper.ChargeCode = setCommentChargeCode ? DocChargeCode.New(CommentChargeCode, Factory) : DocChargeCode.New(NonCommentChargeCode, Factory);
			invoiceLineWrapper.IsSpacerLine = setSpacerLine;
			invoiceLineWrapper.IsSubTotalLine = setSubTotalLine;
		}

		public void TestFPOSPropertyValues()
		{
			var arDocLine = DocARInvoiceLineForRollUp.New(Factory);
			//Default value should be empty
			AssertEquals("", arDocLine.FixedPlaceOfSupply);
			AssertEquals("", arDocLine.FixedPlaceOfSupplyLabel);

			arDocLine.FixedPlaceOfSupply = "Delhi";
			arDocLine.FixedPlaceOfSupplyLabel = "State of Supply";
			//Checking assigned values
			AssertEquals("Delhi", arDocLine.FixedPlaceOfSupply);
			AssertEquals("State of Supply", arDocLine.FixedPlaceOfSupplyLabel);
		}

		#endregion

		#region Implementation

		ARInvoiceLine line;

		ARInvoiceLine Line
		{
			get { return line ?? (line = Factory.New<ARInvoiceLine>()); }
		}

		DocARInvoiceLineForRollUp invoiceLineWrapper;

		DocARInvoiceLineForRollUp InvoiceLineWrapper
		{
			get { return invoiceLineWrapper ?? (invoiceLineWrapper = DocARInvoiceLineForRollUp.New(Factory)); }
		}

		AccChargeCode CommentChargeCode
		{
			get
			{
				AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				chargeCode.AC_ChargeType = "CMT";
				return chargeCode;
			}
		}

		AccChargeCode NonCommentChargeCode
		{
			get
			{
				AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				chargeCode.AC_ChargeType = "FRT";
				return chargeCode;
			}
		}

		#endregion
	}
}
