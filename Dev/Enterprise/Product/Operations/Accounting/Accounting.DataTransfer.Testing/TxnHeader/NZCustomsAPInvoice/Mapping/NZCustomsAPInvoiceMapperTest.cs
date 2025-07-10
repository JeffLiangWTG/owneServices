using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices.Testing
{
	public class NZCustomsAPInvoiceMapperTest : TestCaseWithFactory
	{
		public void TestWithMissingChargeLines()
		{
			AccChargeCode defaultChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			defaultChargeCode.AC_Code = "DEFAULT";
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, defaultChargeCode.PK.ToGuid());

			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			AccTransactionLines line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AC = dutyChargeCode.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			charge.JR_LocalCostAmt = 15m;
			charge.JR_AL_ARLine = line.PK;
			charge.JR_AC = dutyChargeCode.PK;

			#region Missing Entry Fee & GST lines on job
			ZString rawTestData = @"GCRB,40185183G,40199816A,BFM GLOBAL LIMITED,15-Apr-11,3910735001,INV,EE,S00000001,25.00,0,5.00,0,30.00,1159,30-Apr-11,20/05/2011";

			row = new NZCustomsAPInvoiceDataRow(rawTestData);
			txnInvoice = new Xsd.TxnHeader();
			mapper.MapJobChargeLines(job, row, txnInvoice);
			Assert("Number of lines", 2 == txnInvoice.TxnLines.Count);
			Xsd.TxnLine txnLine = FindInvoiceLine("DUTY");
			AssertNotNull(txnLine);
			Assert("Amount", txnLine.OsInvoiceAmtExclTax.Value == -25m);

			txnLine = FindInvoiceLine("ENTRYFEE");
			AssertNull(txnLine);

			txnLine = FindInvoiceLine("GST");
			AssertNotNull(txnLine);
			Assert("Amount", txnLine.OsInvoiceAmtExclTax.Value == -5m);
			#endregion

			#region Missing Both Duty & Entry Fee
			job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AC = gstChargeCode.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			charge.JR_LocalCostAmt = 15m;
			charge.JR_AL_ARLine = line.PK;
			charge.JR_AC = gstChargeCode.PK;

			rawTestData = @"GCRB,40185183G,40199816A,BFM GLOBAL LIMITED,15-Apr-11,3910735001,INV,EE,S00000001,25.00,0,55.00,0,30.00,1159,30-Apr-11,20/05/2011";
			row = new NZCustomsAPInvoiceDataRow(rawTestData);
			txnInvoice = new Xsd.TxnHeader();
			mapper.MapJobChargeLines(job, row, txnInvoice);
			Assert("Number of lines", 2 == txnInvoice.TxnLines.Count);
			txnLine = FindInvoiceLine("GST");
			AssertNotNull(txnLine);
			Assert("Amount", txnLine.OsInvoiceAmtExclTax.Value == -55m);

			txnLine = FindInvoiceLine("DEFAULT");
			AssertNotNull(txnLine);
			Assert("Amount", txnLine.OsInvoiceAmtExclTax.Value == -25m);
			#endregion
		}

		public void TestMapWithAllValidSettings()
		{
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			AccTransactionLines line = Factory.NewWithValidTestData<ARInvoice>().Lines.AddNew();
			line.AL_AC = dutyChargeCode.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			charge.JR_AL_ARLine = line.PK;
			charge.JR_AC = dutyChargeCode.PK;
			charge.JR_LocalCostAmt = 15m;
			charge.JR_OSCostAmt = 15m;

			charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			line = Factory.NewWithValidTestData<ARInvoice>().Lines.AddNew();
			line.AL_AC = entryFeeChargeCode.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			charge.JR_AL_ARLine = line.PK;
			charge.JR_AC = entryFeeChargeCode.PK;
			charge.JR_LocalCostAmt = 10m;
			charge.JR_OSCostAmt = 10m;

			charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			line = Factory.NewWithValidTestData<ARInvoice>().Lines.AddNew();
			line.AL_AC = gstChargeCode.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			charge.JR_AL_ARLine = line.PK;
			charge.JR_AC = gstChargeCode.PK;
			charge.JR_LocalCostAmt = 5m - (10m * mapper.GSTTaxRate.GetRate_ForTestOnly() / 100m);
			charge.JR_OSCostAmt = charge.JR_LocalCostAmt;

			Factory.Save();

			#region Positive figures imported
			ZString rawTestData = @"GCRB,40185183G,40199816A,BFM GLOBAL LIMITED,15-Apr-11,3910735001,INV,EE,S00000001,25.00,0,5.00,0,30.00,1159,30-Apr-11,20/05/2011";

			row = new NZCustomsAPInvoiceDataRow(rawTestData);
			txnInvoice = new Xsd.TxnHeader();
			mapper.MapJobChargeLines(job, row, txnInvoice);
			Assert("Number of lines", 3 == txnInvoice.TxnLines.Count);
			Xsd.TxnLine txnLine = FindInvoiceLine("DUTY");
			AssertNotNull(txnLine);
			Assert("Branch", txnLine.Branch == GlbBranch.CurrentBranch.GB_Code);
			Assert("Department", txnLine.Department == GlbDepartment.CurrentDepartment.GE_Code);
			Assert("Currency", txnLine.OsInvoiceAmtExclTax.CurrencyCode == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			Assert("Amount", txnLine.OsInvoiceAmtExclTax.Value == -15m);
			Assert("job number", txnLine.ConsolOrJobNo == job.JH_JobNum);
			Assert("charge code", txnLine.ChargeCode == dutyChargeCode.AC_Code);

			txnLine = FindInvoiceLine("ENTRYFEE");
			AssertNotNull(txnLine);
			Assert("Branch", txnLine.Branch == GlbBranch.CurrentBranch.GB_Code);
			Assert("Department", txnLine.Department == GlbDepartment.CurrentDepartment.GE_Code);
			Assert("Currency", txnLine.OsInvoiceAmtExclTax.CurrencyCode == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			Assert("Amount", txnLine.OsInvoiceAmtExclTax.Value == -10m);
			Assert("job number", txnLine.ConsolOrJobNo == job.JH_JobNum);
			Assert("charge code", txnLine.ChargeCode == entryFeeChargeCode.AC_Code);

			txnLine = FindInvoiceLine("GST");
			AssertNotNull(txnLine);
			Assert("Branch", txnLine.Branch == GlbBranch.CurrentBranch.GB_Code);
			Assert("Department", txnLine.Department == GlbDepartment.CurrentDepartment.GE_Code);
			Assert("Currency", txnLine.OsInvoiceAmtExclTax.CurrencyCode == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			Assert("Amount", txnLine.OsInvoiceAmtExclTax.Value == -(5m - (10m * mapper.GSTTaxRate.GetRate_ForTestOnly() / 100m)));
			Assert("job number", txnLine.ConsolOrJobNo == job.JH_JobNum);
			Assert("charge code", txnLine.ChargeCode == gstChargeCode.AC_Code);
			#endregion

			#region Negative figures imported
			rawTestData = @"GCRB,40185183G,40199816A,BFM GLOBAL LIMITED,15-Apr-11,3910735001,INV,EE,S00000001,-25.00,0,-5.00,0,-30.00,1159,30-Apr-11,20/05/2011";
			row = new NZCustomsAPInvoiceDataRow(rawTestData);
			txnInvoice = new Xsd.TxnHeader();
			mapper.MapJobChargeLines(job, row, txnInvoice);
			Assert("Number of lines", 3 == txnInvoice.TxnLines.Count);
			txnLine = FindInvoiceLine("DUTY");
			AssertNotNull(txnLine);
			Assert("Amount", txnLine.OsInvoiceAmtExclTax.Value == 15m);
			Assert("charge code", txnLine.ChargeCode == dutyChargeCode.AC_Code);

			txnLine = FindInvoiceLine("ENTRYFEE");
			Assert("Amount", txnLine.OsInvoiceAmtExclTax.Value == 10m);
			Assert("charge code", txnLine.ChargeCode == entryFeeChargeCode.AC_Code);

			txnLine = FindInvoiceLine("GST");
			AssertNotNull(txnLine);
			Assert("Amount", txnLine.OsInvoiceAmtExclTax.Value == 5m - (10m * mapper.GSTTaxRate.GetRate_ForTestOnly() / 100m));
			Assert("charge code", txnLine.ChargeCode == gstChargeCode.AC_Code);
			#endregion

			#region Excess figures imported
			rawTestData = @"GCRB,40185183G,40199816A,BFM GLOBAL LIMITED,15-Apr-11,3910735001,INV,EE,S00000001,45.00,0,15.00,0,50.00,1159,30-Apr-11,20/05/2011";
			row = new NZCustomsAPInvoiceDataRow(rawTestData);
			txnInvoice = new Xsd.TxnHeader();
			mapper.MapJobChargeLines(job, row, txnInvoice);
			Assert("Number of lines", 3 == txnInvoice.TxnLines.Count);
			txnLine = FindInvoiceLine("ENTRYFEE");
			AssertNotNull(txnLine);
			Assert("Amount", txnLine.OsInvoiceAmtExclTax.Value == -10m);

			txnLine = FindInvoiceLine("DUTY");
			AssertNotNull(txnLine);
			Assert("Amount", txnLine.OsInvoiceAmtExclTax.Value == -35m);

			txnLine = FindInvoiceLine("GST");
			AssertNotNull(txnLine);
			Assert("Amount", txnLine.OsInvoiceAmtExclTax.Value == -(15m - (10m * mapper.GSTTaxRate.GetRate_ForTestOnly() / 100m)));
			#endregion
		}

		public void TestMapWithEntryFeeGstUpdatedCorrectly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			{
				var query = new ZQuery(AccTaxRateSchema.AT_Code, "GST").AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

				var gst = Factory.LoadTop1<AccTaxRate>(query);
				if (gst != null)
				{
					gst.SetRate_ForTestOnly(15, 1);
					Factory.Save();
				}

				entryFeeChargeCode.AC_AT_GSTRate = mapper.GSTTaxRate.PK;

				var charge = Factory.NewWithValidTestData<JobCharge>();
				charge.JR_JH = job.PK;
				var line = (AccTransactionLines)Factory.NewWithValidTestData<ARInvoice>().Lines.AddNew();
				line.AL_AC = entryFeeChargeCode.PK;
				line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
				charge.JR_AL_ARLine = line.PK;
				charge.JR_AC = entryFeeChargeCode.PK;
				charge.JR_LocalCostAmt = 42.81m;
				charge.JR_OSCostAmt = 42.81m;

				charge = Factory.NewWithValidTestData<JobCharge>();
				charge.JR_JH = job.PK;
				line = Factory.NewWithValidTestData<ARInvoice>().Lines.AddNew();
				line.AL_AC = gstChargeCode.PK;
				line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
				charge.JR_AL_ARLine = line.PK;
				charge.JR_AC = gstChargeCode.PK;
				charge.JR_LocalCostAmt = 1880.25;
				charge.JR_OSCostAmt = charge.JR_LocalCostAmt;

				Factory.Save();

				var rawTestData = @"GCRB,40185183G,40199816A,BFM GLOBAL LIMITED,15-Apr-11,3910735001,INV,EE,S00000002,42.81,0,1886.68,0,1929.49,1429,30-Apr-11,20/05/2011";
				row = new NZCustomsAPInvoiceDataRow(rawTestData);
				txnInvoice = new Xsd.TxnHeader();

				mapper.MapJobChargeLines(job, row, txnInvoice);
				Assert("Number of lines", 2 == txnInvoice.TxnLines.Count);

				var txnLine = FindInvoiceLine("ENTRYFEE");
				AssertNotNull(txnLine);
				Assert("Branch", txnLine.Branch == GlbBranch.CurrentBranch.GB_Code);
				Assert("Department", txnLine.Department == GlbDepartment.CurrentDepartment.GE_Code);
				Assert("Currency", txnLine.OsInvoiceAmtExclTax.CurrencyCode == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
				Assert("Amount", txnLine.OsInvoiceAmtExclTax.Value == -42.81m);
				Assert("job number", txnLine.ConsolOrJobNo == job.JH_JobNum);
				Assert("charge code", txnLine.ChargeCode == entryFeeChargeCode.AC_Code);

				txnLine = FindInvoiceLine("GST");
				AssertNotNull(txnLine);
				Assert("Branch", txnLine.Branch == GlbBranch.CurrentBranch.GB_Code);
				Assert("Department", txnLine.Department == GlbDepartment.CurrentDepartment.GE_Code);
				Assert("Currency", txnLine.OsInvoiceAmtExclTax.CurrencyCode == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
				Assert("Amount", txnLine.OsInvoiceAmtExclTax.Value == -1880.25m);
				Assert("job number", txnLine.ConsolOrJobNo == job.JH_JobNum);
				Assert("charge code", txnLine.ChargeCode == gstChargeCode.AC_Code);
			}
		}

		public void TestDateForDutyRateForDeclaration()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			{
				var declaration = TestObjectCreator.CreateDeclaration();

				var job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job1.JH_GB = GlbBranch.CurrentBranch.PK;
				job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job1.JH_JobNum = "S00000001";
				job1.JH_ParentID = declaration.PK;

				SetTaxRate();
				entryFeeChargeCode.AC_AT_GSTRate = mapper.GSTTaxRate.PK;
				CreateCharge(job1);

				CreateRawDataAndAssert(job1, declaration, ZDate.Today.AddDays(6), 6);
				CreateRawDataAndAssert(job1, declaration, ZDate.Empty, 11);
			}
		}

		public void TestDateForDutyRateForShipment()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			{
				var shipment = TestObjectCreator.CreateShipment("SH001", false);
				var declaration = TestObjectCreator.CreateDeclaration();
				declaration.JE_JS = shipment.PK;

				var job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job1.JH_GB = GlbBranch.CurrentBranch.PK;
				job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job1.JH_JobNum = "S00000001";
				job1.JH_ParentID = shipment.PK;

				SetTaxRate();
				entryFeeChargeCode.AC_AT_GSTRate = mapper.GSTTaxRate.PK;
				CreateCharge(job1);

				CreateRawDataAndAssert(job1, declaration, ZDate.Today.AddDays(6), 6);
				CreateRawDataAndAssert(job1, declaration, ZDate.Empty, 11);
			}
		}

		void CreateRawDataAndAssert(JobHeader job, IBaseJobDeclaration declaration, ZDate date, ZDecimal taxRate)
		{
			declaration[JobDeclarationSchema.JE_ExportDate.Name] = date;
			var rawTestData = @"GCRB,40185183G,40199816A,BFM GLOBAL LIMITED,15-Apr-11,3910735001,INV,EE,S00000002,42.81,0,1886.68,0,1929.49,1429,30-Apr-11,20/05/2011";
			row = new NZCustomsAPInvoiceDataRow(rawTestData);
			txnInvoice = new Xsd.TxnHeader();

			mapper.MapJobChargeLines(job, row, txnInvoice);
			var txnLine = FindInvoiceLine("GST");
			AssertNotNull(txnLine);

			var expectedValue = CalculateAmountAfterGST(1886.68, 42.81, taxRate);

			AssertEquals("Resulting GST value", expectedValue, txnLine.OsInvoiceAmtExclTax.Value);
		}

		ZDecimal CalculateAmountAfterGST(ZDecimal totalGST, ZDecimal valueExcludingGST, ZDecimal taxRate)
		{
			var gstAmount = Math.Round((valueExcludingGST * (taxRate / 100)), 4);
			return -(totalGST - gstAmount);
		}

		void CreateCharge(JobHeader job)
		{
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			var line = (AccTransactionLines)Factory.NewWithValidTestData<ARInvoice>().Lines.AddNew();
			line.AL_AC = entryFeeChargeCode.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			charge.JR_AL_ARLine = line.PK;
			charge.JR_AC = entryFeeChargeCode.PK;
			charge.JR_LocalCostAmt = 42.81m;
			charge.JR_OSCostAmt = 42.81m;

			charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			line = Factory.NewWithValidTestData<ARInvoice>().Lines.AddNew();
			line.AL_AC = gstChargeCode.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			charge.JR_AL_ARLine = line.PK;
			charge.JR_AC = gstChargeCode.PK;
			charge.JR_LocalCostAmt = 1880.25;
			charge.JR_OSCostAmt = charge.JR_LocalCostAmt;

			Factory.Save();
		}

		void SetTaxRate()
		{
			var query = new ZQuery(AccTaxRateSchema.AT_Code, "GST").AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var gst = Factory.LoadTop1<AccTaxRate>(query);
			if (gst != null)
			{
				gst.SetRate_ForTestOnly(15, 1, ZDate.Today.AddDays(-30), ZDate.Today.AddDays(-1));
				gst.SetRate_ForTestOnly(11, 1, ZDate.Today, ZDate.Today);
				gst.SetRate_ForTestOnly(18, 3, ZDate.Today.AddDays(1), ZDate.Today.AddDays(10));
				Factory.Save();
			}
		}

		JobHeader job;
		Xsd.TxnHeader txnInvoice;
		NotificationBuffer notifications;
		NZCustomsAPInvoiceMapper mapper;
		NZCustomsAPInvoiceDataRow row;
		AccChargeCode dutyChargeCode;
		AccChargeCode entryFeeChargeCode;
		AccChargeCode gstChargeCode;

		protected override void SetUp()
		{
			TestObjectCreator = new TestObjectCreator(Factory);

			notifications = new NotificationBuffer();
			mapper = new NZCustomsAPInvoiceMapper(Factory, notifications);

			dutyChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			dutyChargeCode.AC_Code = "DUTY";
			dutyChargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			dutyChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.CustomsDuty;

			entryFeeChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			entryFeeChargeCode.AC_Code = "ENTRYFEE";
			entryFeeChargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			entryFeeChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.CustomsDuty;

			gstChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			gstChargeCode.AC_Code = "GST";
			gstChargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			gstChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.CustomsDuty;

			var declaration = (IJobInvoicingPlugIn)TestObjectCreator.CreateDeclaration();
			job = TestObjectCreator.CreateJob(declaration, createWithMutex: false);
			job.JH_JobNum = "S00000001";

			Factory.Save();

			AddToRegistry(NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.GST, gstChargeCode);
			AddToRegistry(NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.Duty, dutyChargeCode);
			AddToRegistry(NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.EntryFee, entryFeeChargeCode);

			Factory.Save();

			mapper = new NZCustomsAPInvoiceMapper(Factory, notifications);

			base.SetUp();
		}

		Xsd.TxnLine FindInvoiceLine(ZString chargeCode)
		{
			foreach (Xsd.TxnLine line in txnInvoice.TxnLines)
			{
				if (line.ChargeCode == chargeCode)
				{
					return line;
				}
			}

			return null;
		}

		void AddToRegistry(ZString chargeType, AccChargeCode chargeCode)
		{
			EntryChargeTypeSettingCollection chargeTypesAndCodes = RatingDataRegistry.Instance.EntryChargeTypesAndCodes.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

			EntryChargeTypeSetting typeAndCode1 = chargeTypesAndCodes.AddNew();
			typeAndCode1.ChargeType = chargeType;
			typeAndCode1.AC_ChargeCode = chargeCode.PK;

			RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeTypesAndCodes);
		}

		TestObjectCreator TestObjectCreator;
	}
}
