using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.DataTransfer.AccBatchRequest.Testing;
using Enterprise.Accounting.DataTransfer.Universal;
using Enterprise.Accounting.Export.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Testing.Universal
{
	public class JobPaymentBasesTest : TestCaseWithFactory
	{
		readonly TestObjectCreator TestObjectCreator;

		public JobPaymentBasesTest()
		{
			TestObjectCreator = new TestObjectCreator(Factory);
		}

		List<PaymentBasis> TestPaymentBases
		{
			get
			{
				var sourceBases = new List<PaymentBasis>();
				var chargeable = new Quantity(10, "KG", reference: "DESC");

				var flatBasis = new PaymentBasis(chargeable, RateInfo.CreateFLT(100m, "AUD"), AdapterType.Shipment, "SHP1");
				var perUnitBasis = new PaymentBasis(chargeable, RateInfo.CreateUNT(5m, "KG", "AUD"), AdapterType.Shipment, "SHP1");
				var percentageInfo = RateInfo.CreatePER(10, "AUD");
				var percentageBasis = PaymentBasis.PercentageFromOriginal(flatBasis, percentageInfo);
				var containerBasis = new PaymentBasis(new Quantity(2, "20GP"), RateInfo.CreateUNT(10m, "CN", "AUD"), AdapterType.Shipment, "SHP1");

				sourceBases.Add(flatBasis);
				sourceBases.Add(perUnitBasis);
				sourceBases.Add(percentageBasis);
				sourceBases.Add(containerBasis);

				return sourceBases;
			}
		}

		public void TestGenerateJobCostingWithPaymentBases()
		{
			var job = Factory.NewJobForTesting<Job>();
			job.JH_JobNum = "TESTJOB1";
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var jobCharge1 = job.Charges.AddNew();
				jobCharge1.JR_AC = TestObjectCreator.CC2.PK;
				jobCharge1.JR_GB = GlbBranch.CurrentBranch.PK;
				jobCharge1.JR_GE = GlbDepartment.CurrentDepartment.PK;
				jobCharge1.CostCurrency.RX_Code = "AUD";
				jobCharge1.CostCurrency.RX_Desc = "Australia, Dollars";
				jobCharge1.JR_OSCostAmt = 200m;
				jobCharge1.JR_LocalCostAmt = 200m;
				jobCharge1.JR_OH_CostAccount = TestObjectCreator.ABIGAS.PK;
				jobCharge1.SellCurrency.RX_Code = "AUD";
				jobCharge1.SellCurrency.RX_Desc = "Australia, Dollars";
				jobCharge1.JR_OSSellAmt = 200m;
				jobCharge1.JR_LocalSellAmt = 200m;
				jobCharge1.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
				jobCharge1.JR_InvoiceType = "FID";
				jobCharge1.JR_DisplaySequence = 1;

				jobCharge1.AddPaymentBases(TestPaymentBases.Take(3), true);
				jobCharge1.AddPaymentBases(TestPaymentBases.Skip(3).Take(1), false);

				Factory.Save();

				JobCostingAdapter jobSummaryAdapter = new JobCostingAdapter();
				JobCosting loadedJob = jobSummaryAdapter.GenerateForTesting(job.PK);

				AssertUniversalPaymentBasisConversion(jobCharge1, loadedJob.ChargeLineCollection[0].CostRatingBasisCollection, true);
				AssertUniversalPaymentBasisConversion(jobCharge1, loadedJob.ChargeLineCollection[0].SellRatingBasisCollection, false);
			}
		}

		public void TestGenerateConsolCostLinesWithPaymentBases()
		{
			TestObjectCreator.CC1.AC_ChargeGroup = "BRK";
			TestObjectCreator.CC2.AC_ChargeGroup = "BRK";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			IJobInvoicingPlugIn shipment = consol.Shipments.AddNew();
			Factory.Save();

			var apportionments = new ApportionmentListing(Factory, consol);
			var cost1 = apportionments.CostsCollection.TryAddNew();
			cost1.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cost1.E6_OSCostAmount = 60m;
			cost1.E6_LocalCostAmount = 60m;
			cost1.E6_AH_APInvoice = ZGuid.Empty;
			cost1.E6_InvoiceNum = "1234321";
			cost1.E6_InvoiceDate = DateTime.Now;
			cost1.E6_PaymentDate = DateTime.Now;
			cost1.E6_OH_Creditor = TestObjectCreator.Creditor1.PK;
			cost1.Creditor.CompanyData.OB_APExternalCreditorCode = "AAA";
			cost1.ApportionmentCharges[0].JR_OSCostAmt = 60m;
			cost1.E6_CostReference = "ABC666777";
			cost1.E6_ExchangeRate = 1;
			cost1.E6_PPDCLT = "ALL";
			cost1.E6_ApportionmentMethod = AllocationMethod.Shipment;
			cost1.E6_SellGovtChargeCode = "Sell Govt Chg Code 1";
			cost1.E6_CostGovtChargeCode = "Cost Govt Chg Code 1";

			TestPaymentBases.Take(3).ConvertToJobPaymentBases(true, cost1.PaymentBases.AddNew);

			Factory.Save();

			var consolCostsAdapter = new ConsolCostsAdapter();

			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var generatedConsolCosts = consolCostsAdapter.Generate(consol, DefaultDataObjectWriterStrategy.TestInstance);

				AssertUniversalPaymentBasisConversion(cost1, generatedConsolCosts.ConsolCostLineCollection[0].CostRatingBasisCollection, true);
			}
		}

		void AssertUniversalPaymentBasisConversion(IPaymentBasisViewCharge charge, List<RatingBasis> universalPaymentBases, bool isCost)
		{
			var paymentBasisViews = isCost ? charge.CostPaymentBasesView : charge.SellPaymentBasesView;

			AssertEquals(paymentBasisViews.Count, universalPaymentBases.Count);

			for (var i = 0; i < universalPaymentBases.Count; i++)
			{
				AssertEquals(paymentBasisViews[i].PBS_AdapterType, universalPaymentBases[i].OriginType);
				AssertEquals(paymentBasisViews[i].PBS_AdapterID, universalPaymentBases[i].OriginKey);
				AssertEquals(paymentBasisViews[i].PBS_FlatRate, universalPaymentBases[i].FlatRate);
				AssertEquals(paymentBasisViews[i].PBS_PerUnitRate, universalPaymentBases[i].PerUnitRate);
				AssertEquals(paymentBasisViews[i].PBS_MinRate, universalPaymentBases[i].MinimumRate);
				AssertEquals(paymentBasisViews[i].PBS_MaxRate, universalPaymentBases[i].MaximumRate);
				AssertEquals(paymentBasisViews[i].PBS_ChargeableDescription, universalPaymentBases[i].OriginAdditionalReference);

				//Flat/Min/Max
				if (paymentBasisViews[i].PBS_FlatRate != 0 || paymentBasisViews[i].PBS_MinRate != 0 || paymentBasisViews[i].PBS_MaxRate != 0)
				{
					AssertEquals(paymentBasisViews[i].PBS_RX_NKRateCurrency, universalPaymentBases[i].Currency.Code);
					AssertEquals(null, universalPaymentBases[i].OriginQuantity);
					AssertEquals(null, universalPaymentBases[i].OriginQuantityUnit);
					AssertEquals(null, universalPaymentBases[i].RateUnit);
				}
				//Percentage and Per Unit
				else
				{
					if (paymentBasisViews[i].PBS_RateUnit == "100")
					{
						AssertEquals(null, universalPaymentBases[i].Currency);
					}
					else
					{
						AssertEquals(paymentBasisViews[i].PBS_RX_NKRateCurrency, universalPaymentBases[i].Currency.Code);
					}

					AssertEquals(paymentBasisViews[i].PBS_ChargeableAmount, universalPaymentBases[i].OriginQuantity);
					AssertEquals(paymentBasisViews[i].PBS_ChargeableUnit, universalPaymentBases[i].OriginQuantityUnit.Code);
					AssertQuantityUnitType(paymentBasisViews[i].PBS_ChargeableUnitType, universalPaymentBases[i].OriginQuantityUnit);

					AssertEquals(paymentBasisViews[i].PBS_RateUnit, universalPaymentBases[i].RateUnit.Code);
					AssertQuantityUnitType(paymentBasisViews[i].PBS_RateUnitType, universalPaymentBases[i].RateUnit);
				}
			}
		}

		void AssertQuantityUnitType(string chargeableUnitType, RatingUnit unit)
		{
			if (unit.Class == RatingUnitClass.None)
			{
				AssertEquals(string.Empty, chargeableUnitType);
			}
			else
			{
				AssertEquals(chargeableUnitType.ToLower(), unit.Class.ToString().ToLower());
			}
		}

		public void TestRatingBasisCollection_XUS()
		{
			SetupJobPaymentBasis();

			var jobSummaryAdapter = new JobCostingAdapter();
			var loadedJob = jobSummaryAdapter.GenerateForTesting(job.PK);

			AssertUniversalPaymentBasisConversion(jobCharge, loadedJob.ChargeLineCollection[0].CostRatingBasisCollection, true);
			AssertUniversalPaymentBasisConversion(jobCharge, loadedJob.ChargeLineCollection[0].SellRatingBasisCollection, false);
		}

		[TestDate(2019, 8, 22)]
		public void TestRatingBasisCollection_XUT()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2019);

			var glHeader = TestObjectCreator.GLHeader1;
			glHeader.AG_AccountNum = "1234.56.78";
			glHeader.AG_Description = "Test GL Header";
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeader.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeader.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeader.PK.ToGuid());
			Factory.Save();

			SetupJobPaymentBasis();
			var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			var nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";
			var connection = ((IDbConnectionInternals)base.TestConnection).ADOConnection;
			var transaction = ((IDbConnectionInternals)base.TestConnection).ADOTransaction;
			var dataAccess = new BatchExportDataAccess(connection, transaction);
			var exporter = new AccountingTransactionEAdaptorExporterForTest(dataAccess);
			var writerStrategy = new AccountingTransactionDataObjectWriterStrategy(context: "eAdaptor");
			var batchResponse = exporter.CreateAndExportBatch(writerStrategy, loginCompanyCode, nameSpace);
			AssertNotNull(batchResponse);
			var batch = batchResponse.DataObject as TransactionBatch;
			AssertNotNull(batch);
			AssertEquals("TransactionCollection: Count", 2, batch.TransactionCollection.Count);
			foreach (var transactionInfo in batch.TransactionCollection)
			{
				AssertEquals("TransactionCollection: PostingJournalCollection: Count", 1, transactionInfo.PostingJournalCollection.Count);
				AssertEquals("TransactionCollection: PostingJournalCollection: RatingBasisCollection: Count", 1, transactionInfo.PostingJournalCollection[0].RatingBasisCollection.Count);
				if (transactionInfo.TransactionType.HasValue)
				{
					var isCost = transactionInfo.TransactionType.Value.ToString() == TransactionLineTypes.Accrual;
					AssertUniversalPaymentBasisConversion(jobCharge, transactionInfo.PostingJournalCollection[0].RatingBasisCollection, isCost);
				}
				else
				{
					Assert(false);
				}
			}
		}

		void SetupJobPaymentBasis()
		{
			var shipmentID = "S00001007";
			job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment(shipmentID));
			TestObjectCreator.SetExchangeRate(job, TestObjectCreator.AUD, 1);

			jobCharge = job.Charges.AddNew();
			jobCharge.JR_AC = TestObjectCreator.CC1.PK;
			jobCharge.JR_LocalSellAmt = 100m;
			jobCharge.JR_OSSellAmt = 100m;
			jobCharge.JR_LocalCostAmt = 100m;
			jobCharge.JR_OSCostAmt = 100m;
			jobCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			jobCharge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;

			var jobPaymentBasis1 = CreateShipmentJobPaymentBasis(jobCharge.PK.ToGuid(), true, shipmentID, 1M);
			jobPaymentBasis1.PBS_ChargeableDescription = "This is cost payment basis.";
			jobPaymentBasis1.PBS_MinRate = 1M;
			jobPaymentBasis1.PBS_MaxRate = 10M;
			jobPaymentBasis1.PBS_FlatRate = 11M;
			jobPaymentBasis1.PBS_PerUnitRate = 0M;
			jobPaymentBasis1.PBS_RateUnit = "1";
			var jobPaymentBasis2 = CreateShipmentJobPaymentBasis(jobCharge.PK.ToGuid(), false, shipmentID, 200M);
			jobPaymentBasis2.PBS_ChargeableDescription = "This is sell payment basis.";
			jobPaymentBasis2.PBS_MinRate = 0M;
			jobPaymentBasis2.PBS_MaxRate = 0M;
			jobPaymentBasis2.PBS_FlatRate = 0M;
			jobPaymentBasis2.PBS_PerUnitRate = 22M;
			jobPaymentBasis2.PBS_RateUnit = "2";

			Factory.Save();

			jobCharge = new BusinessObjectFactory().Load<Charge>(jobCharge.PK);
		}

		JobPaymentBasis CreateShipmentJobPaymentBasis(Guid jobChargePK, bool isCost, string adapterID, decimal chargeableAmount)
		{
			var jobPaymentBasis = Factory.NewWithValidTestData<JobPaymentBasis>();
			jobPaymentBasis.PBS_JR = jobChargePK;
			jobPaymentBasis.PBS_IsCost = isCost;
			jobPaymentBasis.PBS_AdapterType = "Shipment";
			jobPaymentBasis.PBS_AdapterID = adapterID;
			jobPaymentBasis.PBS_ChargeableAmount = chargeableAmount;
			jobPaymentBasis.PBS_ChargeableUnit = "KG";
			jobPaymentBasis.PBS_ChargeableUnitType = "Weight";
			jobPaymentBasis.PBS_RX_NKRateCurrency = "AUD";
			jobPaymentBasis.PBS_RateUnitType = "custom";

			return jobPaymentBasis;
		}

		Job job;
		Charge jobCharge;
	}
}
