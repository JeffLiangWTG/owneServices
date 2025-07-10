using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using AccGenericCharge = Enterprise.Accounting.Business.GenericCharge.GenericCharge;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	public abstract class TransactionHeaderWithLinesTest : TransactionHeaderTest
	{
		public virtual void TestRevenueRecognitionTypeNotEmptyWhenJobUpdatedByDataRefreshAfterSetOnLine()
		{
			HeaderWithLines.AH_TransactionNum = "1212341";
			HeaderWithLines.AH_OH = TestObjectCreator.TestOrganisation.PK;
			if (!(HeaderWithLines is JobRevenueJournal))
			{
				DependentLine2.Delete();
			}

			var revRecConfigCollection = new RevenueRecognitionCollection();
			var revRecConfig = revRecConfigCollection.AddNew();
			revRecConfig.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			var expectedDirection = Constants.FreightShipmentDirection.Code.Import;
			revRecConfig.DirectionCode = expectedDirection;
			revRecConfig.Mode = Constants.TransportModes.All;
			revRecConfig.BrokerCode = RevenueRecognitionLookups.BrokerCodes.All;
			revRecConfig.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.EstimatedArrivalDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, revRecConfigCollection);

			var newFactory = new BusinessObjectFactory();
			var testObjectCreatorForNewnewFactory = new TestObjectCreator(newFactory);
			var shipmentInNewFactory = testObjectCreatorForNewnewFactory.CreateShipment("S00001000", "NZAKL", "NZAKL");
			var expectedRevRecDate = ZDateTime.Today.AddDays(10);
			shipmentInNewFactory.JS_E_ARV = expectedRevRecDate;
			var jobInNewFactory = testObjectCreatorForNewnewFactory.CreateJob(shipmentInNewFactory, false, false);
			newFactory.Save();

			AssertNotEquals("Precondition: Direction", expectedDirection, jobInNewFactory.Direction);
			DependentLine1.AL_JH = jobInNewFactory.PK;
			DependentLine1.AL_OSExTaxAmount = 100;

			if (DependentLine1 is InvoicingLineBase invoiceLine)
			{
				invoiceLine.FillWithValidTestData();
				var chargeList = invoiceLine.ChargeList;
				chargeList.Load();
				invoiceLine.GenericCharge = chargeList.Cast<AccGenericCharge>().First(x => !x.VC_IsGLAccount && x.VC_DepartmentFilterList == "ALL").PK;
				DependentLine1.AL_AT = TestObjectCreator.GST1WithDates.PK;
				((InvoicingBase)HeaderWithLines).SubmittedFromInvoicingForm = true;
				if (invoiceLine is ARInvoiceLine || invoiceLine is ARCreditNoteLine)
				{
					testObjectCreatorForNewnewFactory.CreateCharge(invoiceLine);
				}
			}
			else
			{
				DependentLine1.AL_AC = TestObjectCreator.FRT.PK;
				if (!DependentLine2.IsDeleted)
				{
					DependentLine2.AL_JH = jobInNewFactory.PK;
					DependentLine2.AL_AC = TestObjectCreator.FRT.PK;
					DependentLine2.AL_OSExTaxAmount = -100;
				}
			}

			AssertEquals("Precondition: AL_RevRecognitionType", "", DependentLine1.AL_RevRecognitionType);
			if (HeaderWithLines.AH_Ledger != LedgerTypes.AccountsReceivable && !(HeaderWithLines is JobRevenueJournal))
			{
				AssertHasError("Precondition: AL_JHInfo", DependentLine1.AL_JHInfo, "You have not setup Revenue Recognition for this job type. Go to Registry -> Accounting -> Job Invoicing -> Revenue Recognition Setup to configure Revenue Recognition.");
			}

			shipmentInNewFactory.JS_RL_NKDestination = "AUSYD";
			newFactory.Save();
			AssertEquals("Precondition: Direction", expectedDirection, jobInNewFactory.Direction);
			AssertEquals("Precondition: AL_RevRecognitionType remains empty", "", DependentLine1.AL_RevRecognitionType);

			HeaderWithLines.RunPreSaveValidation();
			AssertNoErrors("Precondition: HeaderWithLines", HeaderWithLines);
			BusinessObjectFactory.SaveTogether(newFactory, Factory);
			AssertEquals("AL_RevRecognitionType", RevenueRecognitionLookups.RecognitionDateOptionCodes.EstimatedArrivalDate, DependentLine1.AL_RevRecognitionType);
			AssertEquals("AL_ReverseDate", expectedRevRecDate, DependentLine1.AL_ReverseDate);
		}

		[TestDate(2019, 02, 01)]
		public void TestRevenueRecognitionDate_FutrueRecog_WithCurrentDateRegistryOff()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			Factory.Save();

			AccountingConfigurationRegistry.Instance.RecognizeRevenueUsingCurrentDateWhenRevenueRecognitionDateInFuture.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertRevenueRecognitionTypeNotEmptyWhenJobUpdatedByDataRefreshAfterSetOnLine(ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(10));
		}

		[TestDate(2019, 02, 01)]
		public void TestRevenueRecognitionDate_PastRecog_WithCurrentDateRegistryOff()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			Factory.Save();

			AccountingConfigurationRegistry.Instance.RecognizeRevenueUsingCurrentDateWhenRevenueRecognitionDateInFuture.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertRevenueRecognitionTypeNotEmptyWhenJobUpdatedByDataRefreshAfterSetOnLine(ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(-2));
		}

		[TestDate(2019, 02, 01)]
		public void TestRevenueRecognitionDate_FutrueRecog_WithCurrentDateRegistryOn()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			Factory.Save();

			AccountingConfigurationRegistry.Instance.RecognizeRevenueUsingCurrentDateWhenRevenueRecognitionDateInFuture.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertRevenueRecognitionTypeNotEmptyWhenJobUpdatedByDataRefreshAfterSetOnLine(ZDateTime.Today.AddDays(10), ZDateTime.Now);
		}

		[TestDate(2019, 02, 01)]
		public void TestRevenueRecognitionDate_PastRecog_WithCurrentDateRegistryOn()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			Factory.Save();

			AccountingConfigurationRegistry.Instance.RecognizeRevenueUsingCurrentDateWhenRevenueRecognitionDateInFuture.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertRevenueRecognitionTypeNotEmptyWhenJobUpdatedByDataRefreshAfterSetOnLine(ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(-2));
		}

		void AssertRevenueRecognitionTypeNotEmptyWhenJobUpdatedByDataRefreshAfterSetOnLine(ZDateTime recognitionDate, ZDateTime expectedDate)
		{
			if (!IsJobRelatedTransaction)
			{
				Assert("Not Job related transaction", true);
				return;
			}

			var revRecConfigCollection = new RevenueRecognitionCollection();
			var revRecConfig = revRecConfigCollection.AddNew();
			revRecConfig.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			revRecConfig.DirectionCode = Constants.FreightShipmentDirection.Code.Import;
			revRecConfig.Mode = Constants.TransportModes.All;
			revRecConfig.BrokerCode = RevenueRecognitionLookups.BrokerCodes.All;
			revRecConfig.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.EstimatedArrivalDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, revRecConfigCollection);

			Header = (TransactionHeader)GetNewBusinessObject();
			HeaderWithLines.AH_TransactionNum = $"1212341";
			HeaderWithLines.AH_OH = TestObjectCreator.TestOrganisation.PK;

			CreateHeaderLines();
			if (!(HeaderWithLines is JobRevenueJournal))
			{
				DependentLine2.Delete();
			}

			var shipment = TestObjectCreator.CreateShipment($"S000010001", "NZAKL", "AUSYD");
			shipment.JS_E_ARV = recognitionDate;
			var job = TestObjectCreator.CreateJob(shipment, false, false);
			AssertEquals("Precondition: Direction", Constants.FreightShipmentDirection.Code.Import, job.Direction);

			DependentLine1.AL_JH = job.PK;
			DependentLine1.AL_OSExTaxAmount = 100;

			if (DependentLine1 is InvoicingLineBase invoiceLine)
			{
				invoiceLine.FillWithValidTestData();
				var chargeList = invoiceLine.ChargeList;
				chargeList.Load();
				invoiceLine.GenericCharge = chargeList.Cast<AccGenericCharge>().First(x => !x.VC_IsGLAccount && x.VC_DepartmentFilterList == "ALL").PK;
				DependentLine1.AL_AT = TestObjectCreator.GST1WithDates.PK;
				((InvoicingBase)HeaderWithLines).SubmittedFromInvoicingForm = true;
				if (invoiceLine is ARInvoiceLine || invoiceLine is ARCreditNoteLine)
				{
					TestObjectCreator.CreateCharge(invoiceLine);
				}
			}
			else
			{
				DependentLine1.AL_AC = TestObjectCreator.FRT.PK;
				if (!DependentLine2.IsDeleted)
				{
					DependentLine2.AL_JH = job.PK;
					DependentLine2.AL_AC = TestObjectCreator.FRT.PK;
					DependentLine2.AL_OSExTaxAmount = -100;
				}
			}

			HeaderWithLines.RunPreSaveValidation();
			AssertNoErrors("Precondition: HeaderWithLines", HeaderWithLines);

			Factory.Save();
			AssertEquals("AL_RevRecognitionType", RevenueRecognitionLookups.RecognitionDateOptionCodes.EstimatedArrivalDate, DependentLine1.AL_RevRecognitionType);
			AssertEquals("AL_ReverseDate", expectedDate.Date, DependentLine1.AL_ReverseDate.Date);
		}

		protected virtual bool IsJobRelatedTransaction => true;

		public override void TestIsTaxReportable()
		{
			AssertNull(DependentLine1.TaxRate);
			AssertNull(DependentLine2.TaxRate);
			AssertEquals(false, HeaderWithLines.IsTaxReportable);

			DependentLine1.AL_AT = Factory.New<AccTaxRate>().PK;
			AssertEquals(false, HeaderWithLines.IsTaxReportable);

			DependentLine1.TaxRate.AT_Type = AccTaxRate.Types.NotReportable;
			AssertEquals(false, HeaderWithLines.IsTaxReportable);

			DependentLine1.TaxRate.AT_Type = AccTaxRate.Types.Exempt;
			AssertEquals(true, HeaderWithLines.IsTaxReportable);

			DependentLine1.TaxRate.AT_Type = AccTaxRate.Types.ExcludedFromTheTaxBase;
			AssertEquals(false, HeaderWithLines.IsTaxReportable);

			DependentLine2.AL_AT = Factory.New<AccTaxRate>().PK;
			AssertEquals(false, HeaderWithLines.IsTaxReportable);

			DependentLine2.TaxRate.AT_Type = AccTaxRate.Types.ExcludedFromTheTaxBase;
			AssertEquals(false, HeaderWithLines.IsTaxReportable);

			DependentLine2.TaxRate.AT_Type = AccTaxRate.Types.Rated;
			AssertEquals(true, HeaderWithLines.IsTaxReportable);

			DependentLine1.TaxRate.AT_Type = AccTaxRate.Types.CapitalRated;
			AssertEquals(true, HeaderWithLines.IsTaxReportable);
		}

		#region Total Calculations called by Dependent Lines

		public void TestGetSumOfLines()
		{
			DependentLine1.AL_LineAmount = 100.00m;
			DependentLine1.AL_GSTVAT = 55.5m;
			DependentLine2.AL_LineAmount = 75.00m;
			DependentLine2.AL_GSTVAT = 66.6m;

			HeaderWithLines.Lines.Add(DependentLine1);
			HeaderWithLines.Lines.Add(DependentLine2);

			AssertEquals(AccTransactionLinesSchema.Constants.AL_LineAmount, 175m, HeaderWithLines.GetSumOfLines(AccTransactionLinesSchema.Constants.AL_LineAmount));
			AssertEquals(AccTransactionLinesSchema.Constants.AL_GSTVAT, 122.1m, HeaderWithLines.GetSumOfLines(AccTransactionLinesSchema.Constants.AL_GSTVAT));
		}

		public void TestGetSumOfLinesTwoFields()
		{
			DependentLine1.AL_LineAmount = 100.00m;
			DependentLine1.AL_GSTVAT = 50.25m;
			DependentLine2.AL_LineAmount = 75.00m;
			DependentLine2.AL_GSTVAT = 49.75m;

			HeaderWithLines.Lines.Add(DependentLine1);
			HeaderWithLines.Lines.Add(DependentLine2);

			AssertEquals("Should Return Sum Of LineAmount And GSTVAT For All Lines", 275m, HeaderWithLines.GetSumOfLines(AccTransactionLinesSchema.Constants.AL_LineAmount, AccTransactionLinesSchema.Constants.AL_GSTVAT));
		}

		public void TestHeaderAmountsUpdateSuspenderDoesNotUpdateAllAmountsOnDispose()
		{
			DependentLine1.AL_OSExTaxAmount = 100.00m;
			DependentLine1.AL_LocalExTaxAmount = 100.00m;
			DependentLine1.AL_OSTaxAmount = 10.00m;
			DependentLine1.AL_LocalTaxAmount = 10.00m;
			DependentLine1.AL_OSAmount = 118.00m;
			DependentLine1.AL_OSWHTAmount = 5.00m;
			DependentLine1.AL_LocalWHTAmount = 5.00m;
			DependentLine1.AL_OSExtraTaxAmount = 3.00m;
			DependentLine1.AL_LocalExtraTaxAmount = 3.00m;

			HeaderWithLines.Lines.Add(DependentLine1);

			HeaderWithLines.AH_OSExTaxAmount = 0.00m;
			HeaderWithLines.AH_LocalExTaxAmount = 0.00m;
			HeaderWithLines.AH_OSTaxAmount = 0.00m;
			HeaderWithLines.AH_LocalTaxAmount = 0.00m;
			HeaderWithLines.AH_OSTotalAmount = 0.00m;
			HeaderWithLines.AH_OSWHTAmount = 0.00m;
			HeaderWithLines.AH_LocalWHTAmount = 0.00m;
			HeaderWithLines.AH_OSExtraTaxAmount = 0.00m;
			HeaderWithLines.AH_LocalExtraTaxAmount = 0.00m;

			AssertEquals("AH_OSExTaxAmount", 0.00m, HeaderWithLines.AH_OSExTaxAmount);
			AssertEquals("AH_LocalExTaxAmount", 0.00m, HeaderWithLines.AH_LocalExTaxAmount);
			AssertEquals("AH_OSTaxAmount", 0.00m, HeaderWithLines.AH_OSTaxAmount);
			AssertEquals("AH_LocalTaxAmount", 0.00m, HeaderWithLines.AH_LocalTaxAmount);
			AssertEquals("AH_OSTotalAmount", 0.00m, HeaderWithLines.AH_OSTotalAmount);
			AssertEquals("AH_OSWHTAmount", 0.00m, HeaderWithLines.AH_OSWHTAmount);
			AssertEquals("AH_LocalWHTAmount", 0.00m, HeaderWithLines.AH_LocalWHTAmount);
			AssertEquals("AH_OSExtraTaxAmount", 0.00m, HeaderWithLines.AH_OSExtraTaxAmount);
			AssertEquals("AH_LocalExtraTaxAmount", 0.00m, HeaderWithLines.AH_LocalExtraTaxAmount);

			using (HeaderWithLines.GetHeaderAmountsUpdateSuspender())
			{
				// Do nothing just dispose Suspender
			}

			AssertEquals("AH_OSExTaxAmount should not be forcefully updated on disposing Suspender", 0.00m, HeaderWithLines.AH_OSExTaxAmount);
			AssertEquals("AH_LocalExTaxAmount should not be forcefully updated on disposing Suspender", 0.00m, HeaderWithLines.AH_LocalExTaxAmount);
			AssertEquals("AH_OSTaxAmount should not be forcefully updated on disposing Suspender", 0.00m, HeaderWithLines.AH_OSTaxAmount);
			AssertEquals("AH_LocalTaxAmount should not be forcefully updated on disposing Suspender", 0.00m, HeaderWithLines.AH_LocalTaxAmount);
			AssertEquals("AH_OSTotalAmount should not be forcefully updated on disposing Suspender", 0.00m, HeaderWithLines.AH_OSTotalAmount);
			AssertEquals("AH_OSWHTAmount should not be forcefully updated on disposing Suspender", 0.00m, HeaderWithLines.AH_OSWHTAmount);
			AssertEquals("AH_LocalWHTAmount should not be forcefully updated on disposing Suspender", 0.00m, HeaderWithLines.AH_LocalWHTAmount);
			AssertEquals("AH_OSExtraTaxAmount should not be forcefully updated on disposing Suspender", 0.00m, HeaderWithLines.AH_OSExtraTaxAmount);
			AssertEquals("AH_LocalExtraTaxAmount should not be forcefully updated on disposing Suspender", 0.00m, HeaderWithLines.AH_LocalExtraTaxAmount);
		}

		#region TEST: UpdateAH_LocalExTaxAmount

		public void TestUpdateAH_LocalExTaxAmount()
		{
			DependentLine1.AL_LocalExTaxAmount = 100.00m;
			DependentLine2.AL_LocalExTaxAmount = 75.00m;

			HeaderWithLines.Lines.Add(DependentLine1);
			HeaderWithLines.Lines.Add(DependentLine2);

			HeaderWithLines.AH_LocalExTaxAmount = 0m;

			using (HeaderWithLines.GetHeaderAmountsUpdateSuspender())
			{
				HeaderWithLines.UpdateAH_LocalExTaxAmount();
				AssertEquals("Should not been updated yet", 0m, HeaderWithLines.AH_LocalExTaxAmount);
			}

			AssertEquals("Local Ex Tax Total", 175.00m, HeaderWithLines.AH_LocalExTaxAmount);
		}

		#endregion

		#region TEST: UpdateAH_LocalExTaxAmount With New Default Amount

		public void TestUpdateAH_LocalExTaxAmountWithDefaultNewAmount()
		{
			DependentLine1.AL_LocalExTaxAmount = 100.00m;
			DependentLine2.AL_LocalExTaxAmount = 75.00m;

			HeaderWithLines.Lines.Add(DependentLine1);
			HeaderWithLines.Lines.Add(DependentLine2);

			HeaderWithLines.AH_LocalExTaxAmount = 0;

			HeaderWithLines.UpdateAH_LocalExTaxAmount(65m);

			AssertEquals("Local GST Total", 240.00m, HeaderWithLines.AH_LocalExTaxAmount);
		}

		#endregion

		#region TEST: UpdateAH_LocalTaxAmount

		public void TestUpdateAH_LocalTaxAmount()
		{
			DependentLine1.AL_LocalTaxAmount = 100.00m;
			DependentLine2.AL_LocalTaxAmount = 75.00m;

			HeaderWithLines.Lines.Add(DependentLine1);
			HeaderWithLines.Lines.Add(DependentLine2);

			HeaderWithLines.AH_LocalTaxAmount = 0m;

			using (HeaderWithLines.GetHeaderAmountsUpdateSuspender())
			{
				HeaderWithLines.UpdateAH_LocalTaxAmount();
				AssertEquals("Should not been updated yet", 0m, HeaderWithLines.AH_LocalTaxAmount);
			}

			AssertEquals("Local GST Total", 175.00m, HeaderWithLines.AH_LocalTaxAmount);
		}

		#endregion

		#region TEST: UpdateAH_LocalTaxAmount With New Default Amount

		public void TestUpdateAH_LocalTaxAmountWithDefaultNewAmount()
		{
			DependentLine1.AL_LocalTaxAmount = 100.00m;
			DependentLine2.AL_LocalTaxAmount = 75.00m;

			HeaderWithLines.Lines.Add(DependentLine1);
			HeaderWithLines.Lines.Add(DependentLine2);

			HeaderWithLines.AH_LocalTaxAmount = 0;

			HeaderWithLines.UpdateAH_LocalTaxAmount(85m);

			AssertEquals("Local GST Total", 260.00m, HeaderWithLines.AH_LocalTaxAmount);
		}

		#endregion

		#region TEST: UpdateAH_LocalWHTAmount

		public void TestUpdateAH_LocalWHTAmount()
		{
			DependentLine1.AL_LocalWHTAmount = 100.00m;
			DependentLine2.AL_LocalWHTAmount = 75.00m;

			HeaderWithLines.Lines.Add(DependentLine1);
			HeaderWithLines.Lines.Add(DependentLine2);

			HeaderWithLines.AH_LocalWHTAmount = 0;

			using (HeaderWithLines.GetHeaderAmountsUpdateSuspender())
			{
				HeaderWithLines.UpdateAH_LocalWHTAmount();
				AssertEquals("Should not been updated yet", 0m, HeaderWithLines.AH_LocalWHTAmount);
			}

			AssertEquals("Local WHT Total", 175.00m, HeaderWithLines.AH_LocalWHTAmount);
		}

		#endregion

		#region TEST: UpdateAH_LocalWHTAmount With New Default Amount

		public void TestUpdateAH_LocalWHTAmountWithDefaultNewAmount()
		{
			DependentLine1.AL_LocalWHTAmount = 100.00m;
			DependentLine2.AL_LocalWHTAmount = 75.00m;

			HeaderWithLines.Lines.Add(DependentLine1);
			HeaderWithLines.Lines.Add(DependentLine2);

			HeaderWithLines.AH_LocalWHTAmount = 0;

			HeaderWithLines.UpdateAH_LocalWHTAmount(65M);

			AssertEquals("Local WHT Total", 240.00M, HeaderWithLines.AH_LocalWHTAmount);
		}

		#endregion

		#region TEST: UpdateAH_OSExTaxAmount

		public void TestUpdateAH_OSExTaxAmount()
		{
			DependentLine1.AL_OSExTaxAmount = 200.00m;
			DependentLine2.AL_OSExTaxAmount = 50.00m;

			HeaderWithLines.Lines.Add(DependentLine1);
			HeaderWithLines.Lines.Add(DependentLine2);

			HeaderWithLines.AH_OSExTaxAmount = 0m;

			using (HeaderWithLines.GetHeaderAmountsUpdateSuspender())
			{
				HeaderWithLines.UpdateAH_OSExTaxAmount();
				AssertEquals("Should not been updated yet", 0m, HeaderWithLines.AH_OSExTaxAmount);
			}

			AssertEquals("OS Ex Tax Total", 250.00m, HeaderWithLines.AH_OSExTaxAmount);
		}

		#endregion

		#region TEST: UpdateAH_OSExTaxAmount With New Default Amount

		public void TestUpdateAH_OSExTaxAmountWithDefaultNewAmount()
		{
			DependentLine1.AL_OSExTaxAmount = 200.00m;
			DependentLine2.AL_OSExTaxAmount = 50.00m;

			HeaderWithLines.Lines.Add(DependentLine1);
			HeaderWithLines.Lines.Add(DependentLine2);

			HeaderWithLines.AH_OSExTaxAmount = 0;

			HeaderWithLines.UpdateAH_OSExTaxAmount(35.00m);

			AssertEquals("OS Ex Tax Total", 285.00m, HeaderWithLines.AH_OSExTaxAmount);
		}

		#endregion

		#region TEST: UpdateAH_OSTaxAmount

		public void TestUpdateAH_OSTaxAmount()
		{
			DependentLine1.AL_OSTaxAmount = 200.00m;
			DependentLine2.AL_OSTaxAmount = 50.00m;

			HeaderWithLines.Lines.Add(DependentLine1);
			HeaderWithLines.Lines.Add(DependentLine2);

			HeaderWithLines.AH_OSTaxAmount = 0m;

			using (HeaderWithLines.GetHeaderAmountsUpdateSuspender())
			{
				HeaderWithLines.UpdateAH_OSTaxAmount();
				AssertEquals("Should not been updated yet", 0m, HeaderWithLines.AH_OSTaxAmount);
			}

			AssertEquals("OS Tax Total", 250.00m, HeaderWithLines.AH_OSTaxAmount);
		}

		#endregion

		#region TEST: UpdateAH_OSTaxAmount With New Default Amount

		public void TestUpdateAH_OSTaxAmountWithDefaultNewAmount()
		{
			DependentLine1.AL_OSTaxAmount = 150.00m;
			DependentLine2.AL_OSTaxAmount = 60.00m;

			HeaderWithLines.Lines.Add(DependentLine1);
			HeaderWithLines.Lines.Add(DependentLine2);

			HeaderWithLines.AH_OSTaxAmount = 0;

			HeaderWithLines.UpdateAH_OSTaxAmount(30.00m);

			AssertEquals("OS Tax Total", 240.00m, HeaderWithLines.AH_OSTaxAmount);
		}

		#endregion

		#region TEST: UpdateAH_OSWHTAmount

		public void TestUpdateAH_OSWHTAmount()
		{
			DependentLine1.AL_LocalWHTAmount = 100.00m;
			DependentLine2.AL_LocalWHTAmount = 75.00m;

			HeaderWithLines.Lines.Add(DependentLine1);
			HeaderWithLines.Lines.Add(DependentLine2);

			HeaderWithLines.AH_OSWHTAmount = 0m;

			using (HeaderWithLines.GetHeaderAmountsUpdateSuspender())
			{
				HeaderWithLines.UpdateAH_OSWHTAmount();
				AssertEquals("Should not been updated yet", 0m, HeaderWithLines.AH_OSWHTAmount);
			}

			AssertEquals("OS WHT Total", 175.00m, HeaderWithLines.AH_OSWHTAmount);
		}

		#endregion

		#region TEST: UpdateAH_OSWHTAmount With New Default Amount

		public void TestUpdateAH_OSWHTAmountWithDefaultNewAmount()
		{
			DependentLine1.AL_LocalWHTAmount = 100.00m;
			DependentLine2.AL_LocalWHTAmount = 75.00m;

			HeaderWithLines.Lines.Add(DependentLine1);
			HeaderWithLines.Lines.Add(DependentLine2);

			HeaderWithLines.AH_OSWHTAmount = 0;

			HeaderWithLines.UpdateAH_OSWHTAmount(65M);
			AssertEquals("OS WHT Total", 240.00M, HeaderWithLines.AH_OSWHTAmount);
		}

		#endregion

		#region TEST: UpdateAH_OSTotalAmount

		public void TestUpdateAH_OSTotalAmount()
		{
			HeaderWithLines.ExchangeRate.Currency = TestObjectCreator.USD.RX_Code;
			DependentLine1.AL_OverseasTotal = -400.00m;
			DependentLine2.AL_OverseasTotal = -95.00m;

			HeaderWithLines.Lines.Add(DependentLine1);
			HeaderWithLines.Lines.Add(DependentLine2);

			HeaderWithLines.AH_OSTotalAmount = 0m;
			using (HeaderWithLines.GetHeaderAmountsUpdateSuspender())
			{
				HeaderWithLines.UpdateAH_OSTotalAmount();
				AssertEquals("Should not been updated yet", 0m, HeaderWithLines.AH_OSTotalAmount);
			}
			AssertEquals("Local OS Total", -495.00m, HeaderWithLines.AH_OSTotalAmount);

			HeaderWithLines.ExchangeRate.Currency = TestObjectCreator.AUD.RX_Code;
			DependentLine1.AL_OverseasTotal = -400.00m;
			DependentLine2.AL_OverseasTotal = -95.00m;
			HeaderWithLines.AH_OSTotalAmount = 0;
			HeaderWithLines.UpdateAH_OSTotalAmount();
			AssertEquals("Local OS Total", 0.00m, HeaderWithLines.AH_OSTotalAmount);

			DependentLine1.AL_LocalExTaxAmount = DependentLine1.AL_OverseasTotal;
			DependentLine2.AL_LocalExTaxAmount = DependentLine2.AL_OverseasTotal;
			HeaderWithLines.AH_OSTotalAmount = 0;
			HeaderWithLines.UpdateAH_OSTotalAmount();
			AssertEquals("Local OS Total", -495.00m, HeaderWithLines.AH_OSTotalAmount);
		}

		#endregion

		#region TEST: UpdateAH_OSTotalAmount With New Default Amount

		public void TestUpdateAH_OSTotalAmountWithDefaultNewAmount()
		{
			HeaderWithLines.ExchangeRate.Currency = TestObjectCreator.USD.RX_Code;
			DependentLine1.AL_OverseasTotal = -400.00m;
			DependentLine2.AL_OverseasTotal = -95.00m;

			HeaderWithLines.Lines.Add(DependentLine1);
			HeaderWithLines.Lines.Add(DependentLine2);

			HeaderWithLines.AH_OSTotalAmount = 0;
			HeaderWithLines.UpdateAH_OSTotalAmount(25m);
			AssertEquals("Local OS Total", -470.00m, HeaderWithLines.AH_OSTotalAmount);

			HeaderWithLines.ExchangeRate.Currency = TestObjectCreator.AUD.RX_Code;
			DependentLine1.AL_OverseasTotal = -400.00m;
			DependentLine2.AL_OverseasTotal = -95.00m;
			HeaderWithLines.AH_OSTotalAmount = 0;
			HeaderWithLines.UpdateAH_OSTotalAmount(25m);
			AssertEquals("Local OS Total", 25.00m, HeaderWithLines.AH_OSTotalAmount);

			DependentLine1.AL_LocalExTaxAmount = DependentLine1.AL_OverseasTotal;
			DependentLine2.AL_LocalExTaxAmount = DependentLine2.AL_OverseasTotal;
			HeaderWithLines.AH_OSTotalAmount = 0;
			HeaderWithLines.UpdateAH_OSTotalAmount(25m);
			AssertEquals("Local OS Total", -470.00m, HeaderWithLines.AH_OSTotalAmount);
		}

		#endregion

		public virtual void TestUpdateAH_OSTotalAmountCountOnOtherTaxesAmount()
		{
			HeaderWithLines.AH_OSTaxAmountOtherTaxes = 123 * HeaderWithLines.Multiplier_ForTestOnly;
			HeaderWithLines.ExchangeRate.Currency = TestObjectCreator.AUD.RX_Code;
			DependentLine1.AL_OverseasTotal = 400;
			DependentLine1.AL_LocalExTaxAmount = DependentLine1.AL_OverseasTotal;
			HeaderWithLines.Lines.Add(DependentLine1);
			HeaderWithLines.AH_OSTotalAmount = 0;
			HeaderWithLines.UpdateAH_OSTotalAmount();
			AssertEquals("AH_OSTotalAmount", 523m, HeaderWithLines.AH_OSTotalAmount);
		}

		public virtual void TestUpdateTotalsCallAmount()
		{
			Action<string, Action, int, int> assert = (actionName, action, expectedGetSumOfLinesCallAmount, expectedValidateAH_OSTotalAmountCallCount) =>
			{
				HeaderWithLines.GetSumOfLinesCallAmount_ForTestOnly = 0;
				HeaderWithLines.ValidateAH_OSTotalAmountCallCount_ForTestOnly = 0;

				action();

				var realGetSumOfLinesCallAmount = HeaderWithLines.GetSumOfLinesCallAmount_ForTestOnly;
				Assert(string.Format("Amount of GetSumOfLines calls for {0} is {1}, but expected not more then {2}", actionName,
					realGetSumOfLinesCallAmount, expectedGetSumOfLinesCallAmount),
					realGetSumOfLinesCallAmount <= expectedGetSumOfLinesCallAmount);

				var realValidateAH_OSTotalAmountCallCount = HeaderWithLines.ValidateAH_OSTotalAmountCallCount_ForTestOnly;
				Assert(string.Format("Amount of ValidateAH_OSTotalAmount calls for {0} is {1}, but expected not more then {2}", actionName,
					realValidateAH_OSTotalAmountCallCount, expectedValidateAH_OSTotalAmountCallCount),
					realValidateAH_OSTotalAmountCallCount <= expectedValidateAH_OSTotalAmountCallCount);
			};

			HeaderWithLines.AH_RX_NKTransactionCurrency = Constants.CurrencyCodes.Ukraine;
			assert("AH_RX_NKTransactionCurrency", () => HeaderWithLines.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code, 2, 1);
			assert("AL_OSExTaxAmount", () => DependentLine1.AL_OSExTaxAmount = 10, 10, 1);
			assert("AL_LocalExTaxAmount", () => DependentLine1.AL_LocalExTaxAmount = 10, 4, 1);
			assert("AL_LocalTaxAmount", () => DependentLine1.AL_LocalTaxAmount = 10, 6, 1);
			assert("AL_LocalExtraTaxAmount", () => DependentLine1.AL_LocalExtraTaxAmount = 10, 2, 1);
			assert("AL_LocalWHTAmount", () => DependentLine1.AL_LocalWHTAmount = 10, 3, 1);

			if (HeaderWithLines is GLJournal)
			{
				DependentLine2.AL_OSExTaxAmount = -10;
			}
			assert("AL_OSExtraTaxAmount", () => DependentLine1.AL_OSExtraTaxAmount = 20, 3, 1);
			assert("AL_OSTaxAmount", () => DependentLine1.AL_OSTaxAmount = 20, 7, 1);
			assert("AL_OSWHTAmount", () => DependentLine1.AL_OSWHTAmount = 10, 3, 1);
			assert("Factory.Save", () => Factory.Save(), 8, 1);
		}

		#endregion

		public void TestZDecimalsHaveCorrectDecimalPlacesTransactionHeaderWithlines()
		{
			var header = (TransactionHeaderWithLines)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());

			var localList = new List<string>
				{
					nameof(header.AH_LocalExtraTaxAmount),
					nameof(header.AH_LocalExtraTax),
					nameof(header.AH_LocalEDUPrimaryAmount),
					nameof(header.AH_LocalEDUSecondaryAmount)
				};

			var osList = new List<string>
				{
					nameof(header.AH_OSExtraTaxAmount),
					nameof(header.AH_OSExtraTax),
					nameof(header.AH_OSEDUPrimaryAmount),
					nameof(header.AH_OSEDUSecondaryAmount)
				};

			var tester = new DecimalPlacesAttributeTester(header, header.Company);
			tester.CheckLocalCurrency(localList, nameof(header.LocalCurrencyDecimals));
			tester.CheckNonLocalCurrency(osList, nameof(header.OSCurrencyDecimals), nameof(header.AH_RX_NKTransactionCurrency), header);
		}

		public void TestEnforceBranchLevelPosting()
		{
			if ((HeaderWithLines.Ledger_ForTestOnly == LedgerTypes.AccountsReceivable || HeaderWithLines.Ledger_ForTestOnly == LedgerTypes.AccountsPayable || HeaderWithLines.Ledger_ForTestOnly == LedgerTypes.CashBook || HeaderWithLines.Ledger_ForTestOnly == LedgerTypes.UnapprovedPayableTransactions)
				&& (HeaderWithLines.TransactionType_ForTestOnly == TransactionTypes.Invoice || HeaderWithLines.TransactionType_ForTestOnly == TransactionTypes.CreditNote || HeaderWithLines.TransactionType_ForTestOnly == TransactionTypes.AdjustmentNote
					|| HeaderWithLines.TransactionType_ForTestOnly == TransactionTypes.DirectPayment || HeaderWithLines.TransactionType_ForTestOnly == TransactionTypes.DirectReceipt))
			{
				AssertNotNull(HeaderWithLines.EnforceBranchLevelPostingRegistryItem);
				if (HeaderWithLines.Ledger_ForTestOnly == LedgerTypes.AccountsReceivable || HeaderWithLines.TransactionType_ForTestOnly == TransactionTypes.DirectReceipt)
				{
					AssertEquals(AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting, HeaderWithLines.EnforceBranchLevelPostingRegistryItem);
				}
				else
				{
					AssertEquals(AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting, HeaderWithLines.EnforceBranchLevelPostingRegistryItem);
				}
			}
			else
			{
				Assert(string.Format("For {0}-{1} the system does not prevent having mixture of branch lines", HeaderWithLines.AH_Ledger, HeaderWithLines.AH_TransactionType), true);
			}
		}

		public virtual void TestAllLinesHaveSameBranch()
		{
			var transaction = (TransactionHeaderWithLines)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());

			GlbBranch branch1 = Factory.New<GlbBranch>();
			GlbBranch branch2 = Factory.New<GlbBranch>();

			AssertEquals("All Lines Have Same Branch", false, transaction.LinesHaveSameBranch);

			var line1 = transaction.Lines.AddNew();
			line1.AL_GB = branch1.PK;

			var line2 = transaction.Lines.AddNew();
			line2.AL_GB = branch1.PK;

			var line3 = transaction.Lines.AddNew();
			line3.AL_GB = branch2.PK;
			AssertEquals("All Lines Have Same Branch", false, transaction.LinesHaveSameBranch);

			line3.AL_GB = branch1.PK;
			AssertEquals("All Lines Have Same Branch", true, transaction.LinesHaveSameBranch);

			line2.AL_GB = branch2.PK;
			AssertEquals("All Lines Have Same Branch", false, transaction.LinesHaveSameBranch);

			transaction.Lines.RemoveAndDelete(line2);
			AssertEquals("All Lines Have Same Branch", true, transaction.LinesHaveSameBranch);
		}

		public void TestAH_OSTaxAmountWithDifferentExchangeRateLines_Reciprocal()
		{
			TestObjectCreator.SetCurrentCompanyReciprocal(true);
			TestAH_OSTaxAmountWithDifferentExchangeRateLines(true, 50m);
		}

		public void TestAH_OSTaxAmountWithDifferentExchangeRateLines_NotReciprocal()
		{
			TestObjectCreator.SetCurrentCompanyReciprocal(false);
			TestAH_OSTaxAmountWithDifferentExchangeRateLines(false, 20m);
		}

		void TestAH_OSTaxAmountWithDifferentExchangeRateLines(bool isReciprocal, ZDecimal expectedOSTaxAmount)
		{
			var header = (TransactionHeader)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			var headerWithLines2 = header as TransactionHeaderWithLines;
			if (headerWithLines2 is JobRevenueJournal || headerWithLines2 is GLJournal || headerWithLines2 is CashBook.Transfer.BankTransferCharge || header is TransactionPendingAllocation)
			{
				Assert(true);
			}
			else
			{
				headerWithLines2.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

				DependentTransactionLine dependentLine3 = headerWithLines2.Lines.AddNew();
				DependentTransactionLine dependentLine4 = headerWithLines2.Lines.AddNew();

				dependentLine3.AL_AG = TestObjectCreator.GLHeader1.PK;
				dependentLine4.AL_AG = TestObjectCreator.GLHeader1.PK;
				dependentLine3.AL_OSExTaxAmount = 100m;
				dependentLine3.AL_AT = TestObjectCreator.GST1.PK;
				dependentLine4.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
				dependentLine4.AL_ExchangeRate = 2m;
				dependentLine4.AL_OSExTaxAmount = 200m;
				dependentLine4.AL_AT = TestObjectCreator.GST1.PK;

				Factory.Save();

				BusinessObjectFactory newFactoryForLoad = new BusinessObjectFactory();

				TransactionHeaderWithLines loadedInvoice = (TransactionHeaderWithLines)newFactoryForLoad.Load(GetExpectedBusinessObjectType(), headerWithLines2.PK);
				AssertEquals(isReciprocal, headerWithLines2.Company.GC_IsReciprocal);
				AssertEquals(expectedOSTaxAmount, loadedInvoice.AH_OSTaxAmount);
			}
		}

		public void TestAH_OSTaxAmountDoNotHitDatabaseOfViewWhenCreateNewInvoice()
		{
			var tableHits = new Dictionary<string, int>();
			tableHits.Add(vw_AccTransactionHeaderTaxSchema.Constants.TableName, 0);
			tableHits.Add(AccPeriodManagementSchema.Constants.TableName, 3);

			var testFactory = new BusinessObjectFactory();
			TransactionHeader header = testFactory.NewWithValidTestData<GLJournal>();
			TransactionHeaderWithLines headerWithLines = header as TransactionHeaderWithLines;
			var amount = headerWithLines.AH_OSTaxAmount;
			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			AssertDbHits(tableHits, testFactory);
		}

		public void TestAH_OSTaxAmountUsesTransactionCompanyNotCurrentCompany()
		{
			var companyOfNonReciprocalCountry = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_IsReciprocal, false));
			var companyOfReciprocalCountry = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_IsReciprocal, true));

			if (HeaderWithLines is JobRevenueJournal || HeaderWithLines is GLJournal || HeaderWithLines is CashBook.Transfer.BankTransferCharge)
			{
				Assert(true);
			}
			else
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, companyOfNonReciprocalCountry.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					HeaderWithLines.AH_GC = companyOfNonReciprocalCountry.PK;
					HeaderWithLines.AH_RX_NKTransactionCurrency = companyOfReciprocalCountry.GC_RX_NKLocalCurrency;
					HeaderWithLines.AH_ExchangeRate = 2m;

					DependentLine1.AL_OSExTaxAmount = 500m;
					DependentLine1.AL_AT = TestObjectCreator.GST1.PK;
					DependentLine1.AL_ExchangeRate = 2m;
					DependentLine2.AL_OSExTaxAmount = 500m;
					DependentLine2.AL_AT = TestObjectCreator.GST1.PK;
					DependentLine2.AL_ExchangeRate = 2m;

					HeaderWithLines.Lines.Add(DependentLine1);
					HeaderWithLines.Lines.Add(DependentLine2);

					Factory.Save();

					BusinessObjectFactory newFactoryForLoad = new BusinessObjectFactory();

					TransactionHeaderWithLines loadedInvoice = (TransactionHeaderWithLines)newFactoryForLoad.Load(GetExpectedBusinessObjectType(), HeaderWithLines.PK);
					AssertHeaderWithLines(loadedInvoice);
				}
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, companyOfReciprocalCountry.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					BusinessObjectFactory newFactoryForLoad2 = new BusinessObjectFactory();

					TransactionHeaderWithLines loadedInvoice = (TransactionHeaderWithLines)newFactoryForLoad2.Load(GetExpectedBusinessObjectType(), HeaderWithLines.PK);
					AssertHeaderWithLines(loadedInvoice);
				}
			}
		}

		void AssertHeaderWithLines(TransactionHeaderWithLines loadedInvoice)
		{
			{
				AssertEquals(500m, loadedInvoice.Lines[0].AL_OSExTaxAmount);
				AssertEquals(50m, loadedInvoice.Lines[0].AL_OSTaxAmount);
				AssertEquals(550m, loadedInvoice.Lines[0].AL_OverseasTotal);
				AssertEquals(2, loadedInvoice.Lines.Count);
				AssertEquals(500m, loadedInvoice.Lines[1].AL_OSExTaxAmount);
				AssertEquals(50m, loadedInvoice.Lines[1].AL_OSTaxAmount);
				AssertEquals(550m, loadedInvoice.Lines[1].AL_OverseasTotal);
			}

			AssertEquals(100m, loadedInvoice.AH_OSTaxAmount);
			AssertEquals(1000m, loadedInvoice.AH_OSExTaxAmount);
			AssertEquals(1100m, loadedInvoice.AH_OSTotalAmount);
		}

		public void TestAH_OSTaxAmountWithLocalCurrencyAndHighPrecisionLineExchangeRate()
		{
			var originalGC_IsReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			GlbCompany.CurrentCompany.Factory.Save();

			var header = (TransactionHeader)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			var headerWithLines = header as TransactionHeaderWithLines;
			if (headerWithLines is JobRevenueJournal || headerWithLines is GLJournal || headerWithLines is CashBook.Transfer.BankTransferCharge || header is TransactionPendingAllocation)
			{
				Assert(true);
			}
			else
			{
				headerWithLines.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				var taxRate = TestObjectCreator.CreateTaxRate("tax", "test", 23);
				var line = headerWithLines.Lines.AddNew();
				line.AL_AT = taxRate.PK;
				line.AL_RX_NKTransactionCurrency = "EUR";
				line.ExchangeRate.Rate = 4.435769M;
				line.AL_OSExTaxAmount = 0.65M;
				line.AL_AG = TestObjectCreator.GLHeader1.PK;
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var invoice = (TransactionHeaderWithLines)newFactory.Load(GetExpectedBusinessObjectType(), headerWithLines.PK);
				AssertEquals(4.430769231M, invoice.Lines[0].AL_ExchangeRate);
				AssertEquals(0.67M, invoice.AH_OSTaxAmount);
			}

			GlbCompany.CurrentCompany.GC_IsReciprocal = originalGC_IsReciprocal;
		}

		public virtual void TestAH_GB_TaxBranch()
		{
			var header = (TransactionHeader)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			var headerWithLines = header as TransactionHeaderWithLines;

			var line = headerWithLines.Lines.AddNew();
			line.AL_GB_TaxBranch = TestObjectCreator.NonCurrentBranch.PK;

			AssertEquals("PreCondition", TestObjectCreator.NonCurrentBranch.PK, line.AL_GB_TaxBranch);

			header.AH_GB_TaxBranch = TestObjectCreator.NonCurrentCompanyBranch.PK;
			AssertEquals("Header's TaxBranch setting will apply to line's AL_GB_TaxBranch"
				, TestObjectCreator.NonCurrentCompanyBranch.PK
				, line.AL_GB_TaxBranch);
		}

		public void TestAH_Calc_TaxBranchName()
		{
			var header = Factory.NewWithValidTestData(GetExpectedBusinessObjectType()) as TransactionHeaderWithLines;
			header.AH_GB_TaxBranch = GlbBranch.CurrentBranch.PK;
			AssertEquals(DependentTransactionLine.Schema.BranchName, GlbBranch.CurrentBranch.GB_BranchName, header.AH_Calc_TaxBranchName);
			AssertEquals("should be read only", true, header.AH_Calc_TaxBranchNameInfo.ReadOnly);
		}

		public virtual void TestAH_GB_TaxBranchReadOnlyWithSecurity()
		{
			using (TestObjectCreator.SetUpTaxBranchRegistry(true))
			{
				TestObjectCreator.ResetSecurityCore();

				TestObjectCreator.TestOrganisation.CompanyData.SetARTaxApplicable(true);
				TestObjectCreator.TestOrganisation.CompanyData.SetAPTaxApplicable(true);

				var header = Factory.NewWithValidTestData(GetExpectedBusinessObjectType()) as TransactionHeaderWithLines;
				header.AH_OH = TestObjectCreator.TestOrganisation.PK;

				Assert(!header.AH_GB_TaxBranchInfo.ReadOnly);

				GetOverrideTaxBranchSecurity(header.AH_Ledger).IsAllowed = false;
				Assert(header.AH_GB_TaxBranchInfo.ReadOnly);

				GetOverrideTaxBranchSecurity(header.AH_Ledger).IsAllowed = true;
				Assert(!header.AH_GB_TaxBranchInfo.ReadOnly);
			}
		}

		protected virtual SecurityCheckpoint GetOverrideTaxBranchSecurity(string ledger)
		{
			return ledger == LedgerTypes.AccountsReceivable ? Env.Security.NewReceivablesOverrideTaxBranchAllows : Env.Security.NewPayablesOverrideTaxBranchAllows;
		}

		public void TestLinesHaveBeenLoaded()
		{
			AssertEquals("Lines have been loaded on a created header", true, ((TransactionHeaderWithLines)Header).LinesHaveBeenLoaded);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var header = (TransactionHeaderWithLines)newFactory.Load(this.ExpectedBusinessObjectType, Header.PK);
			AssertEquals("Lines have been loaded", false, header.LinesHaveBeenLoaded);
			var poke = header.Lines;
			AssertEquals("Lines have been loaded", true, header.LinesHaveBeenLoaded);
		}

		public void TestComplianceRelatedLines()
		{
			var type = GetExpectedBusinessObjectType();
			if (type == typeof(ARInvoice) || type == typeof(APInvoice) || type == typeof(ARCreditNote) || type == typeof(APCreditNote))
			{
				var vat3 = TestObjectCreator.CreateTaxRate("VAT3", "VAT3", 3);
				vat3.AT_PostingGroupId = 0;

				var vat5 = TestObjectCreator.CreateTaxRate("VAT5", "VAT5", 5);
				vat5.AT_PostingGroupId = 1;

				var ac1 = TestObjectCreator.CreateChargeCode("AC1");
				ac1.AC_AT_GSTRate = vat3.PK;
				ac1.AC_Desc = "desc";

				var ac2 = TestObjectCreator.CreateChargeCode("AC2");
				ac2.AC_AT_GSTRate = vat5.PK;
				ac2.AC_Desc = "desc";

				var testInv1 = Header as InvoicingBase;
				var line = (InvoicingLineBase)testInv1.Lines.AddNew();
				line.AL_JH = TestObjectCreator.Job1.PK;
				line.AL_AC = ac1.PK;
				line.AL_AT = vat3.PK;

				var charge = TestObjectCreator.CreateJobCharge(line, TestObjectCreator.Job1, ac1);

				var line1 = testInv1.Lines.AddNew();
				line1.AL_JH = TestObjectCreator.Job1.PK;
				line1.AL_AC = ac2.PK;
				line1.AL_AT = vat5.PK;
				line1.AL_AG = TestObjectCreator.GLHeader1.PK;

				var charge1 = TestObjectCreator.CreateJobCharge(line1, TestObjectCreator.Job1, ac2);

				var complianceHeader = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(testInv1.Ledger_ForTestOnly, "desc", "00001014", "NTC", "lineDesc", line);
				var complianceHeader1 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(testInv1.Ledger_ForTestOnly, "desc", "00001015", "NTC", "lineDesc", line1);

				Factory.Save();

				var collection = testInv1.GetComplianceRelatedLines(complianceHeader);
				Assert(collection.Contains(line));
				Assert(!collection.Contains(line1));
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCalculatingExtraTaxDoesntSetHasChanges()
		{
			DependentLine1.AL_AT = TaxRateGSTANDQST.PK;
			DependentLine1.AL_OSExTaxAmount = DependentLine1.AL_LocalExTaxAmount = 100m;
			DependentLine2.AL_AT = TaxRateGSTANDEDU.PK;
			var line2ExTaxAmount = 50m;
			if (HeaderWithLines is GLJournal || HeaderWithLines is JobRevenueJournal)
			{
				line2ExTaxAmount = -100m;
			}
			DependentLine2.AL_OSExTaxAmount = DependentLine2.AL_LocalExTaxAmount = line2ExTaxAmount;

			HeaderWithLines.Lines.Add(DependentLine1);
			HeaderWithLines.Lines.Add(DependentLine2);

			Factory.Save();

			Assert("Shouldn't have changes", !HeaderWithLines.HasChanges);

			TransactionHeaderWithLines headerWithLinesInNewFactory = (TransactionHeaderWithLines)new BusinessObjectFactory().Load(GetExpectedBusinessObjectType(), HeaderWithLines.PK);

			ZDecimal dummyValue = headerWithLinesInNewFactory.AH_LocalExtraTaxAmount;
			dummyValue = headerWithLinesInNewFactory.AH_OSExtraTaxAmount;

			Assert("Still shouldn't have changes", !headerWithLinesInNewFactory.HasChanges);
		}

		public void TestSetTransactionLinesCurrency()
		{
			DependentTransactionLine testLine1 = HeaderWithLines.Lines.AddNew();
			DependentTransactionLine testLine2 = HeaderWithLines.Lines.AddNew();

			HeaderWithLines.SetTransactionLinesCurrency_ForTestOnly(TestObjectCreator.USD.RX_Code);
			AssertEquals("USD", testLine1.AL_RX_NKTransactionCurrency);
			AssertEquals("USD", testLine2.AL_RX_NKTransactionCurrency);

			HeaderWithLines.SetTransactionLinesCurrency_ForTestOnly(TestObjectCreator.AUD.RX_Code);
			AssertEquals("AUD", testLine1.AL_RX_NKTransactionCurrency);
			AssertEquals("AUD", testLine2.AL_RX_NKTransactionCurrency);
		}

		public void TestSetTransactionLinesExchangeRate()
		{
			ZDecimal testExchangeRate = 0.5m;

			HeaderWithLines.AH_ExchangeRate = testExchangeRate;

			DependentTransactionLine testLine1 = HeaderWithLines.Lines.AddNew();
			DependentTransactionLine testLine2 = HeaderWithLines.Lines.AddNew();

			HeaderWithLines.SetTransactionLinesExchangeRate_ForTestOnly(testExchangeRate);
			AssertEquals(testExchangeRate, testLine1.AL_ExchangeRate);
			AssertEquals(testExchangeRate, testLine2.AL_ExchangeRate);

			testExchangeRate = 0.7m;
			HeaderWithLines.AH_ExchangeRate = testExchangeRate;
			HeaderWithLines.SetTransactionLinesExchangeRate_ForTestOnly(testExchangeRate);
			AssertEquals(testExchangeRate, testLine1.AL_ExchangeRate);
			AssertEquals(testExchangeRate, testLine2.AL_ExchangeRate);
		}

		public void TestIsMiscServTaxApplicable()
		{
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			OrgMiscServ orgMiscServ = Factory.New<OrgMiscServ>();
			orgMiscServ.OM_OH = orgHeader.PK;

			TransactionHeaderWithLines testHeader = (TransactionHeaderWithLines)Factory.New(GetExpectedBusinessObjectType());
			testHeader.AH_OH = orgHeader.PK;
			Assert(!testHeader.IsMiscServTaxApplicable);

			if (testHeader.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				orgHeader.CompanyData.SetARTaxApplicable(ZBool.True);
			}
			else
			{
				orgHeader.CompanyData.SetAPTaxApplicable(ZBool.True);
			}
			Assert(testHeader.IsMiscServTaxApplicable);
		}

		public virtual void TestPostDateOnLinesSameAsHeader()
		{
			ZDateTime postDate = PeriodManagementTestHelper.PreviousOpenPeriod.AM_EndDate;
			HeaderWithLines.AH_PostDate = postDate;

			Assert("Precondition: Initial post date on line not equal header date", postDate != DependentLine1.AL_PostDate);
			Assert("Precondition: Initial post date on line not equal header date", postDate != DependentLine2.AL_PostDate);

			HeaderWithLines.Factory.Save();

			AssertEquals("both lines should have the correct post date", postDate, DependentLine1.AL_PostDate);
			AssertEquals("both lines should have the correct post date", postDate, DependentLine2.AL_PostDate);

			HeaderWithLines.AH_PostDate = ZDateTime.Now.AddDays(-20);
			using (ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.PostDateCriticalValidationSuspenderService>.GetSuspender(HeaderWithLines.Factory))
			{
				HeaderWithLines.Factory.Save();
			}

			AssertEquals("both lines should have the same post date - should not be altered on saving", postDate, DependentLine1.AL_PostDate);
			AssertEquals("both lines should have the same post date - should not be altered on saving", postDate, DependentLine2.AL_PostDate);
		}

		public virtual void TestLoadedLinesOrderedBySequence()
		{
			DependentLine1.AL_Sequence = (short)1;
			DependentLine2.AL_Sequence = (short)2;
			DependentTransactionLine line3 = HeaderWithLines.Lines.AddNew();
			line3.AL_AG = TestObjectCreator.GLHeader1.PK;
			line3.AL_Sequence = (short)3;
			Factory.Save();

			DependentLine1.AL_Sequence = (short)3;
			line3.AL_Sequence = (short)1;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			TransactionHeaderWithLines loadedTransaction = (TransactionHeaderWithLines)newFactory.Load(GetExpectedBusinessObjectType(), HeaderWithLines.PK);
			AssertEquals("First line should be Line3", line3.AL_Sequence, loadedTransaction.Lines[0].AL_Sequence);
			AssertEquals("Second line should be Line2", DependentLine2.AL_Sequence, loadedTransaction.Lines[1].AL_Sequence);
			AssertEquals("Third line should be Line1", DependentLine1.AL_Sequence, loadedTransaction.Lines[2].AL_Sequence);
		}

		public void TestUpdateAH_OSExtraTaxAmount()
		{
			DependentLine1.AL_OSExtraTaxAmount = 100.00m;
			DependentLine2.AL_OSExtraTaxAmount = 75.00m;

			HeaderWithLines.Lines.Add(DependentLine1);
			HeaderWithLines.Lines.Add(DependentLine2);

			HeaderWithLines.AH_OSExtraTaxAmount = 0;

			using (HeaderWithLines.GetHeaderAmountsUpdateSuspender())
			{
				HeaderWithLines.UpdateAH_OSExtraTaxAmount();
				AssertEquals("Should not been updated yet", 0m, HeaderWithLines.AH_OSExtraTaxAmount);
			}

			AssertEquals("OS EDU Total", 175.00m, HeaderWithLines.AH_OSExtraTaxAmount);
			AssertEquals("OS EDU Primary Total", 116.67m, HeaderWithLines.AH_OSEDUPrimaryAmount);
			AssertEquals("OS EDU Secondary Total", 58.33m, HeaderWithLines.AH_OSEDUSecondaryAmount);
		}

		public void TestUpdateAH_LocalExtraTaxAmount()
		{
			DependentLine1.AL_LocalExtraTaxAmount = 100.00m;
			DependentLine2.AL_LocalExtraTaxAmount = 75.00m;

			HeaderWithLines.Lines.Add(DependentLine1);
			HeaderWithLines.Lines.Add(DependentLine2);

			HeaderWithLines.AH_LocalExtraTaxAmount = 0m;

			using (HeaderWithLines.GetHeaderAmountsUpdateSuspender())
			{
				HeaderWithLines.UpdateAH_LocalExtraTaxAmount();
				AssertEquals("Should not been updated yet", 0m, HeaderWithLines.AH_LocalExtraTaxAmount);
			}

			AssertEquals("Local EDU Total", 175.00m, HeaderWithLines.AH_LocalExtraTaxAmount);
			AssertEquals("Local EDU Amount Primary", 116.67m, HeaderWithLines.AH_LocalEDUPrimaryAmount);
			AssertEquals("Local EDU Amount Secondary", 58.33m, HeaderWithLines.AH_LocalEDUSecondaryAmount);
		}

		public virtual void TestQSTAmountUpdatedOnLoad()
		{
			DependentLine1.AL_AT = TaxRateGSTANDQST.PK;
			DependentLine1.AL_OSExTaxAmount = DependentLine1.AL_LocalExTaxAmount = 100m;
			DependentLine2.AL_AT = TaxRateGSTANDQST.PK;
			DependentLine2.AL_OSExTaxAmount = DependentLine2.AL_LocalExTaxAmount = 50m;

			HeaderWithLines.Lines.Add(DependentLine1);
			HeaderWithLines.Lines.Add(DependentLine2);

			Factory.Save();

			BusinessObjectFactory newFactoryForLoad = new BusinessObjectFactory();

			TransactionHeaderWithLines loadedInvoice = (TransactionHeaderWithLines)newFactoryForLoad.Load(GetExpectedBusinessObjectType(), HeaderWithLines.PK);

			AssertEquals("OS QST Amount on Load", 11.82m, loadedInvoice.AH_OSExtraTaxAmount);
			AssertEquals("Local QST Amount on Load", 11.82m, loadedInvoice.AH_LocalExtraTaxAmount);
		}

		public virtual void TestEDUAmountUpdatedOnLoad()
		{
			DependentLine1.AL_AT = TaxRateGSTANDEDU.PK;
			DependentLine1.AL_OSExTaxAmount = DependentLine1.AL_LocalExTaxAmount = 100m;
			DependentLine2.AL_AT = TaxRateGSTANDEDU.PK;
			DependentLine2.AL_OSExTaxAmount = DependentLine2.AL_LocalExTaxAmount = 50m;

			HeaderWithLines.Lines.Add(DependentLine1);
			HeaderWithLines.Lines.Add(DependentLine2);

			Factory.Save();

			BusinessObjectFactory newFactoryForLoad = new BusinessObjectFactory();

			TransactionHeaderWithLines loadedInvoice = (TransactionHeaderWithLines)newFactoryForLoad.Load(GetExpectedBusinessObjectType(), HeaderWithLines.PK);

			AssertEquals("OS EDU Amount on Load", 0.45m, loadedInvoice.AH_OSExtraTaxAmount);
			AssertEquals("OS EDU Primary on Load", 0.3m, loadedInvoice.AH_OSEDUPrimaryAmount);
			AssertEquals("OS EDU Secondary on Load", 0.15m, loadedInvoice.AH_OSEDUSecondaryAmount);
			AssertEquals("Local EDU Amount on Load", 0.45m, loadedInvoice.AH_LocalExtraTaxAmount);
			AssertEquals("Local EDU Amount Primary on Load", 0.3m, loadedInvoice.AH_LocalEDUPrimaryAmount);
			AssertEquals("Local EDU Amount Secondary on Load", 0.15m, loadedInvoice.AH_LocalEDUSecondaryAmount);
		}

		public virtual void TestRETAmountUpdatedOnLoad()
		{
			DependentLine1.AL_AT = TaxRateRET.PK;
			DependentLine1.AL_OSExTaxAmount = DependentLine1.AL_LocalExTaxAmount = 100m;
			DependentLine2.AL_AT = TaxRateRET.PK;
			DependentLine2.AL_OSExTaxAmount = DependentLine2.AL_LocalExTaxAmount = 50m;

			HeaderWithLines.Lines.Add(DependentLine1);
			HeaderWithLines.Lines.Add(DependentLine2);

			Factory.Save();

			BusinessObjectFactory newFactoryForLoad = new BusinessObjectFactory();

			TransactionHeaderWithLines loadedInvoice = (TransactionHeaderWithLines)newFactoryForLoad.Load(GetExpectedBusinessObjectType(), HeaderWithLines.PK);

			AssertEquals("OS RET Amount on Load", -6m, loadedInvoice.AH_OSExtraTaxAmount);
			AssertEquals("Local RET Amount on Load", -6m, loadedInvoice.AH_LocalExtraTaxAmount);
		}

		public virtual void TestGSTAndQSTBasedOnQCTAmountUpdatedOnLoad()
		{
			DependentLine1.AL_AT = TaxRateGSTAndQSTBasedOnQCT.PK;
			DependentLine1.AL_OSExTaxAmount = DependentLine1.AL_LocalExTaxAmount = 100m;
			DependentLine2.AL_AT = TaxRateGSTAndQSTBasedOnQCT.PK;
			DependentLine2.AL_OSExTaxAmount = DependentLine2.AL_LocalExTaxAmount = 50m;

			HeaderWithLines.Lines.Add(DependentLine1);
			HeaderWithLines.Lines.Add(DependentLine2);

			Factory.Save();

			BusinessObjectFactory newFactoryForLoad = new BusinessObjectFactory();

			TransactionHeaderWithLines loadedInvoice = (TransactionHeaderWithLines)newFactoryForLoad.Load(GetExpectedBusinessObjectType(), HeaderWithLines.PK);

			AssertEquals("OS QCT Amount on Load", 14.97m, loadedInvoice.AH_OSExtraTaxAmount);
			AssertEquals("Local QCT Amount on Load", 14.97m, loadedInvoice.AH_LocalExtraTaxAmount);
		}

		public virtual void TestOTO6AmountUpdatedOnLoad()
		{
			DependentLine1.AL_AT = TaxRateOTO6.PK;
			DependentLine1.AL_OSExTaxAmount = DependentLine1.AL_LocalExTaxAmount = 100m;
			DependentLine2.AL_AT = TaxRateOTO6.PK;
			DependentLine2.AL_OSExTaxAmount = DependentLine2.AL_LocalExTaxAmount = 50m;

			HeaderWithLines.Lines.Add(DependentLine1);
			HeaderWithLines.Lines.Add(DependentLine2);

			Factory.Save();

			BusinessObjectFactory newFactoryForLoad = new BusinessObjectFactory();

			TransactionHeaderWithLines loadedInvoice = (TransactionHeaderWithLines)newFactoryForLoad.Load(GetExpectedBusinessObjectType(), HeaderWithLines.PK);

			AssertEquals("OS OTO Amount on Load", 9m, loadedInvoice.AH_OSExtraTaxAmount);
			AssertEquals("Local OTO Amount on Load", 9m, loadedInvoice.AH_LocalExtraTaxAmount);
		}

		public void TestOSTotalTaxAmountIsEqualToTotalOSTaxLines()
		{
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("Trans" + TestObjectCreator.GetRandomString(5), TestObjectCreator.USD, 9077M, TestObjectCreator.AALSHI);

			var line1 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.USD, 9078.4387M, 170947M, 0M, 0M);
			var line2 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.USD, 9077.3810M, 61000M, 6100M, 0M);
			Factory.Save();

			BusinessObjectFactory newFactoryForLoad = new BusinessObjectFactory();
			ARInvoice loadedHeader = newFactoryForLoad.Load<ARInvoice>(invoice.PK);

			AssertEquals(loadedHeader.AH_OSTaxAmount, 6100M);
		}

		public virtual void TestOSTaxAmountIsAlwaysZeroWhenLineTaxIsZero()
		{
			TestObjectCreator.SetCurrentCompanyReciprocal(true);

			HeaderWithLines.AH_RX_NKTransactionCurrency = "IDR";
			HeaderWithLines.AH_ExchangeRate = 0.000085;

			DependentLine1.AL_OSExTaxAmount = 7189000;
			DependentLine1.AL_OSTaxAmount = 0;
			DependentLine1.AL_ExchangeRate = 0.000085;
			var line2OSExTaxAmount = 75000;
			if (HeaderWithLines is GLJournal)
			{
				line2OSExTaxAmount = -7189000;
			}
			DependentLine2.AL_OSExTaxAmount = line2OSExTaxAmount;
			DependentLine2.AL_OSTaxAmount = 0;
			DependentLine2.AL_ExchangeRate = 0.000085;

			HeaderWithLines.Lines.Add(DependentLine1);
			HeaderWithLines.Lines.Add(DependentLine2);
			Factory.Save();

			BusinessObjectFactory newFactoryForLoad = new BusinessObjectFactory();
			TransactionHeader loadedHeader = (TransactionHeader)newFactoryForLoad.Load(GetExpectedBusinessObjectType(), HeaderWithLines.PK);

			if (!(HeaderWithLines is JobRevenueJournal) && !(HeaderWithLines is GLJournal))
			{
				AssertEquals(617.45M, loadedHeader.AH_LocalTotalAmount);
			}
			AssertEquals(0M, loadedHeader.AH_OSTaxAmount);
		}

		protected abstract Type GetExpectedBusinessObjectLineType();

		public override void TestInternalOSAmountFieldsSetOnLoadCorrectly()
		{
			SetupHeaderExRatesAndAmounts(0.57m, 1000, 200.453m, 15.02m);
			SetupForSave();
			PrepareMiscellaneousTransactionForSaving();

			Header.Factory.Save();

			DependentTransactionLine line = (DependentTransactionLine)Factory.NewWithValidTestData(GetExpectedBusinessObjectLineType());
			HeaderWithLines.Lines.Add(line);
			line.AL_RX_NKTransactionCurrency = Header.AH_RX_NKTransactionCurrency;
			line.AL_ExchangeRate = Header.AH_ExchangeRate;
			line.AL_OSExTaxAmount = 200.453m;
			line.AL_OSTaxAmount = 15.02m;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			Header.AH_FullyPaidDate = ZDateTime.Empty;
			Factory.Save();

			BusinessObjectFactory newFactoryForLoad = new BusinessObjectFactory();

			TransactionHeader loadedHeader = (TransactionHeader)newFactoryForLoad.Load(GetExpectedBusinessObjectType(), Header.PK);

			AssertEquals("OS Ex Tax Amount on Load", 200.453m, loadedHeader.AH_OSExTaxAmount);
			AssertEquals("OS Tax Amount on Load", 15.02m, loadedHeader.AH_OSTaxAmount);
		}

		#region Implementation

		AccTaxRate fTaxRateGSTANDQST;
		protected AccTaxRate TaxRateGSTANDQST
		{
			get
			{
				if (fTaxRateGSTANDQST == null)
				{
					fTaxRateGSTANDQST = Factory.New<AccTaxRate>();
					fTaxRateGSTANDQST.AT_Code = AccTaxRate.LocaliseTaxCode("GSTANDQST", GlbCompany.CurrentCompany.Country);
					fTaxRateGSTANDQST.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					fTaxRateGSTANDQST.SetRateNumerator_ForTestOnly(5);
					fTaxRateGSTANDQST.AT_Type = AccTaxRate.Types.Rated;
					fTaxRateGSTANDQST.SetExtraRate_ForTestOnly(75, 10);
					fTaxRateGSTANDQST.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;
				}

				return fTaxRateGSTANDQST;
			}
		}

		AccTaxRate fTaxRateGSTANDEDU;
		protected AccTaxRate TaxRateGSTANDEDU
		{
			get
			{
				if (fTaxRateGSTANDEDU == null)
				{
					fTaxRateGSTANDEDU = Factory.New<AccTaxRate>();
					fTaxRateGSTANDEDU.AT_Code = AccTaxRate.LocaliseTaxCode("GSTANDEDU", GlbCompany.CurrentCompany.Country);
					fTaxRateGSTANDEDU.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					fTaxRateGSTANDEDU.SetRateNumerator_ForTestOnly(10);
					fTaxRateGSTANDEDU.AT_Type = AccTaxRate.Types.Rated;
					fTaxRateGSTANDEDU.SetExtraRate_ForTestOnly(3, 1);
					fTaxRateGSTANDEDU.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
				}

				return fTaxRateGSTANDEDU;
			}
		}

		AccTaxRate fTaxRateRET;
		protected AccTaxRate TaxRateRET
		{
			get
			{
				if (fTaxRateRET == null)
				{
					fTaxRateRET = Factory.New<AccTaxRate>();
					fTaxRateRET.AT_Code = AccTaxRate.LocaliseTaxCode("RET", GlbCompany.CurrentCompany.Country);
					fTaxRateRET.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					fTaxRateRET.SetRateNumerator_ForTestOnly(16);
					fTaxRateRET.AT_Type = AccTaxRate.Types.Rated;
					fTaxRateRET.SetExtraRate_ForTestOnly(4, 1);
					fTaxRateRET.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;
				}

				return fTaxRateRET;
			}
		}

		AccTaxRate fTaxRateGSTAndQSTBasedOnQCT;
		protected AccTaxRate TaxRateGSTAndQSTBasedOnQCT
		{
			get
			{
				if (fTaxRateGSTAndQSTBasedOnQCT == null)
				{
					fTaxRateGSTAndQSTBasedOnQCT = Factory.New<AccTaxRate>();
					fTaxRateGSTAndQSTBasedOnQCT.AT_Code = AccTaxRate.LocaliseTaxCode("GSTANDQS2", GlbCompany.CurrentCompany.Country);
					fTaxRateGSTAndQSTBasedOnQCT.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					fTaxRateGSTAndQSTBasedOnQCT.SetRateNumerator_ForTestOnly(5);
					fTaxRateGSTAndQSTBasedOnQCT.AT_Type = AccTaxRate.Types.Rated;
					fTaxRateGSTAndQSTBasedOnQCT.SetExtraRate_ForTestOnly(9975, 1000);
					fTaxRateGSTAndQSTBasedOnQCT.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
				}

				return fTaxRateGSTAndQSTBasedOnQCT;
			}
		}

		AccTaxRate fTaxRateOTO6;
		protected AccTaxRate TaxRateOTO6
		{
			get
			{
				if (fTaxRateOTO6 == null)
				{
					fTaxRateOTO6 = Factory.New<AccTaxRate>();
					fTaxRateOTO6.AT_Code = AccTaxRate.LocaliseTaxCode("OTO6", GlbCompany.CurrentCompany.Country);
					fTaxRateOTO6.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					fTaxRateOTO6.SetRateNumerator_ForTestOnly(0);
					fTaxRateOTO6.AT_Type = AccTaxRate.Types.Rated;
					fTaxRateOTO6.SetExtraRate_ForTestOnly(6, 1);
					fTaxRateOTO6.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.ChinaInputVATOffsetAgainstOutputTax;
				}

				return fTaxRateOTO6;
			}
		}

		protected DependentTransactionLine DependentLine1
		{
			get { return dependentLine1; }
		}
		DependentTransactionLine dependentLine1;

		protected DependentTransactionLine DependentLine2
		{
			get { return dependentLine2; }
		}
		DependentTransactionLine dependentLine2;

		protected TransactionHeaderWithLines HeaderWithLines
		{
			get { return (TransactionHeaderWithLines)Header; }
		}

		protected override void SetUp()
		{
			base.SetUp();

			CreateHeaderLines();
		}

		protected virtual void CreateHeaderLines()
		{
			dependentLine1 = HeaderWithLines.Lines.AddNew();
			dependentLine1.AL_AG = TestObjectCreator.GLHeader1.PK;
			dependentLine2 = HeaderWithLines.Lines.AddNew();
			dependentLine2.AL_AG = TestObjectCreator.GLHeader1.PK;
		}

		#endregion
	}
}
