using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(TestingExchangeRate))]
	public class ExchangeRateTest : EnterpriseBusinessObjectTestCase
	{
		public void TestExchangeRateIsDeletedWhenShouldDeleteDuplicateExchangeRateBeforeSaveIsTrueAndExchangeRateNotInDBAndMatchingExchangeRateAlreadyInDB()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = TestObjectCreator.CreateShipment("S0001000");
				var job = TestObjectCreator.CreateJob(shipment);
				Factory.Save();

				var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
				var jobInFactory1 = (Job)factory1.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.PK, job.PK));
				var exchangeRateInFactory1 = jobInFactory1.ExchangeRates.AddNew();
				exchangeRateInFactory1.JF_OH_Org = TestObjectCreator.Creditor1.PK;
				exchangeRateInFactory1.JF_OrgType = ExchangeRateOrgTypeEnum.Creditor.ToCode();
				exchangeRateInFactory1.JF_RX_NKRateCurrency = TestObjectCreator.USD.Code;
				exchangeRateInFactory1.JF_BaseRate = 0.85m;
				exchangeRateInFactory1.JF_InvoiceCurrencyType = Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign;

				var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
				var jobInFactory2 = (Job)factory2.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.PK, job.PK));
				var exchangeRateInFactory2 = jobInFactory2.ExchangeRates.AddNew();
				exchangeRateInFactory2.JF_OH_Org = TestObjectCreator.Creditor1.PK;
				exchangeRateInFactory2.JF_OrgType = ExchangeRateOrgTypeEnum.Creditor.ToCode();
				exchangeRateInFactory2.JF_RX_NKRateCurrency = TestObjectCreator.USD.Code;
				exchangeRateInFactory2.JF_BaseRate = 0.85m;
				exchangeRateInFactory2.JF_InvoiceCurrencyType = Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign;

				factory1.Save();
				Assert(exchangeRateInFactory1.IsInDatabase);

				factory2.SetContext(BusinessContext.ConvertingAmountsForExportAWBHeader);
				exchangeRateInFactory2.ShouldDeleteDuplicateExchangeRateBeforeSave = true;
				AssertNoExceptionThrown(() => factory2.Save());
			}
		}

		public void TestExchangeRateIsNotDeletedWhenShouldDeleteDuplicateExchangeRateBeforeSaveIsFalseAndAndExchangeRateNotInDBAndMatchingExchangeRateAlreadyInDB()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = TestObjectCreator.CreateShipment("S0001000");
				var job = TestObjectCreator.CreateJob(shipment);
				Factory.Save();

				var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
				var jobInFactory1 = (Job)factory1.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.PK, job.PK));
				var exchangeRateInFactory1 = jobInFactory1.ExchangeRates.AddNew();
				exchangeRateInFactory1.JF_OH_Org = TestObjectCreator.Creditor1.PK;
				exchangeRateInFactory1.JF_OrgType = ExchangeRateOrgTypeEnum.Creditor.ToCode();
				exchangeRateInFactory1.JF_RX_NKRateCurrency = TestObjectCreator.USD.Code;
				exchangeRateInFactory1.JF_BaseRate = 0.85m;
				exchangeRateInFactory1.JF_InvoiceCurrencyType = Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign;

				var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
				var jobInFactory2 = (Job)factory2.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.PK, job.PK));
				var exchangeRateInFactory2 = jobInFactory2.ExchangeRates.AddNew();
				exchangeRateInFactory2.JF_OH_Org = TestObjectCreator.Creditor1.PK;
				exchangeRateInFactory2.JF_OrgType = ExchangeRateOrgTypeEnum.Creditor.ToCode();
				exchangeRateInFactory2.JF_RX_NKRateCurrency = TestObjectCreator.USD.Code;
				exchangeRateInFactory2.JF_BaseRate = 0.85m;
				exchangeRateInFactory2.JF_InvoiceCurrencyType = Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign;

				factory1.Save();
				Assert(exchangeRateInFactory1.IsInDatabase);

				exchangeRateInFactory2.ShouldDeleteDuplicateExchangeRateBeforeSave = false;
				var expectedMessage = $"Cannot insert duplicate key row in object 'dbo.JobExRate' with unique index 'FK_UX__JF_OH_Org_JF_JH_JF_OrgType_JF_RX_NKRateCurrency_JF_InvoiceCurrencyType'.";
				try
				{
					factory2.Save();
					Fail("ZSaveException should have been thrown");
				}
				catch (ZSaveException ex)
				{
					AssertContains(expectedMessage, ex.Message);
				}
			}
		}

		public void TestExchangeRateIsNotDeletedWhenShouldDeleteDuplicateExchangeRateBeforeSaveIsTrueAndExchangeRateNotInDBAndNoMatchingExchangeRateInDB()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = TestObjectCreator.CreateShipment("S0001000");
				var job = TestObjectCreator.CreateJob(shipment);
				Factory.Save();

				var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
				var jobInFactory1 = (Job)factory1.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.PK, job.PK));
				var exchangeRateInFactory1 = jobInFactory1.ExchangeRates.AddNew();
				exchangeRateInFactory1.JF_OH_Org = TestObjectCreator.Creditor1.PK;
				exchangeRateInFactory1.JF_OrgType = ExchangeRateOrgTypeEnum.Creditor.ToCode();
				exchangeRateInFactory1.JF_RX_NKRateCurrency = TestObjectCreator.USD.Code;
				exchangeRateInFactory1.JF_BaseRate = 0.85m;
				exchangeRateInFactory1.JF_InvoiceCurrencyType = Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign;

				var query = new ZQuery(JobExRateSchema.JF_OH_Org, exchangeRateInFactory1.JF_OH_Org);
				query.AddToFilter(JobExRateSchema.JF_JH, job.PK);
				query.AddToFilter(JobExRateSchema.JF_OrgType, exchangeRateInFactory1.JF_OrgType);
				query.AddToFilter(JobExRateSchema.JF_RX_NKRateCurrency, exchangeRateInFactory1.JF_RX_NKRateCurrency);
				query.FetchOnlyFromLocalCache = false;
				var matchingExRateInDb = Factory.LoadTop1<ExchangeRate>(query);
				AssertNull("There is no matching exchange rate in database.", matchingExRateInDb);

				exchangeRateInFactory1.ShouldDeleteDuplicateExchangeRateBeforeSave = true;
				Assert(!exchangeRateInFactory1.IsInDatabase);

				factory1.Save();

				Assert(exchangeRateInFactory1.IsInDatabase);
			}
		}

		public void TestZSaveExceptionContainsRequiredMessages()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.ExchangeRateConstuctorCallStack);
			var shipment = TestObjectCreator.CreateShipment("S0001000");
			var job = TestObjectCreator.CreateJob(shipment);
			var rate = job.ExchangeRates.AddNew();
			rate.JF_RX_NKRateCurrency = TestObjectCreator.USD.Code;
			rate.JF_BaseRate = 0.85m;
			var charge = TestObjectCreator.CreateCharge(rate.ParentJob, TestObjectCreator.CC1, 100M, 100M);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var secondRate = newFactory.New<ExchangeRate>();
			secondRate.JF_JH = rate.JF_JH;
			secondRate.JF_RX_NKRateCurrency = rate.JF_RX_NKRateCurrency;
			secondRate.JF_BaseRate = 0.75m;

			var message = ZString.Empty;
			try
			{
				newFactory.Save();
				Fail("ZSaveException should have been thrown");
			}
			catch (ZSaveException e)
			{
				message = e.Message;
			}

			AssertContains("Constructor call stack",
$@"Business object additional info: 
PK = {secondRate.PK}:
ExchangeRateConstuctorCallStack:
   at System.Environment.GetStackTrace", message);
			AssertContains("Charge info", $@"

Charges:
	PK = {charge.PK}", message);
			AssertContains("Exchange rate info", $@"
ExchangeRates:
	PK = ", message);
			AssertContains("Exchange rate info", $@"
	PK = {rate.PK}", message);
			AssertContains("Exchange rate info", $@"
	PK = {secondRate.PK}", message);

			newFactory.RefreshEnabled = false;
			Factory.RefreshEnabled = false;
			var rateInNewFactory = newFactory.Load<ExchangeRate>(rate.PK);
			rate.JF_BaseRate += 1;
			rateInNewFactory.Delete();
			Factory.Save();

			try
			{
				newFactory.Save();
				Fail("ZSaveException should have been thrown");
			}
			catch (DeletedRowInaccessibleException)
			{
				Fail("If row is deleted, should not access row");
			}
			catch (ZSaveException e)
			{
				AssertNotContains("Should not add call stacks if Row is deleted", "ExchangeRateConstuctorCallStack:", e.Message);
			}
		}

		public void TestExchangeRatesDataRefreshAreNotReflectedForExistingCharges()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001001", true);
			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var exchangeRateValue = new ZDecimal(0.7777M);
			var debtorExchangeRate = TestObjectCreator.SetExchangeRate(job, TestObjectCreator.USD, exchangeRateValue, TestObjectCreator.Debtor.PK, ExchangeRateOrgTypeEnum.Debtor);
			var creditorExchangeRate = TestObjectCreator.SetExchangeRate(job, TestObjectCreator.USD, exchangeRateValue, TestObjectCreator.Creditor1.PK, ExchangeRateOrgTypeEnum.Creditor);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.USD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.USD, 100m, TestObjectCreator.Debtor);
			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.Code;
			Factory.Save();
			var newCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge2", TestObjectCreator.USD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.USD, 100m, TestObjectCreator.Debtor);
			newCharge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.Code;

			AssertEquals("Revenue Exchange Rate", exchangeRateValue, charge.JR_OSSellExRate);
			AssertEquals("Cost Exchange Rate", exchangeRateValue, charge.JR_OSCostExRate);
			AssertEquals("Sell Invoice Exchange Rate", exchangeRateValue, charge.JR_OSSellInvoiceExRate);
			AssertEquals("Revenue Exchange Rate", exchangeRateValue, newCharge.JR_OSSellExRate);
			AssertEquals("Cost Exchange Rate", exchangeRateValue, newCharge.JR_OSCostExRate);
			AssertEquals("Sell Invoice Exchange Rate", exchangeRateValue, newCharge.JR_OSSellInvoiceExRate);

			var updateFactory = new BusinessObjectFactory();
			var existingJob = updateFactory.Load<Job>(job.PK);
			AssertEquals("Charge Count", 1, existingJob.Charges.Count);
			var existingCharge = existingJob.Charges[0];
			AssertNotNull("Existing Charge", existingCharge);
			AssertEquals("Revenue Exchange Rate", exchangeRateValue, existingCharge.JR_OSSellExRate);
			AssertEquals("Cost Exchange Rate", exchangeRateValue, existingCharge.JR_OSCostExRate);
			AssertEquals("Sell Invoice Exchange Rate", exchangeRateValue, existingCharge.JR_OSSellInvoiceExRate);

			var newExchangeRateValue = new ZDecimal(0.8M);
			existingJob.ExchangeRates.ForEach(e => ((ExchangeRate)e).JF_BaseRate = newExchangeRateValue);
			AssertEquals("Revenue Exchange Rate", newExchangeRateValue, existingCharge.JR_OSSellExRate);
			AssertEquals("Cost Exchange Rate", newExchangeRateValue, existingCharge.JR_OSCostExRate);
			AssertEquals("Sell Invoice Exchange Rate", newExchangeRateValue, existingCharge.JR_OSSellInvoiceExRate);

			var testObjCreatorForUpdateFactory = new TestObjectCreator(updateFactory);
			var arInvoice = testObjCreatorForUpdateFactory.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.USD, 1m, TestObjectCreator.Debtor);
			var apInvoice = testObjCreatorForUpdateFactory.CreateInvoice(typeof(APInvoice), "AP001", TestObjectCreator.USD, 1m, TestObjectCreator.Creditor1);
			var sellLine = testObjCreatorForUpdateFactory.CreateRevenueLine(existingCharge, arInvoice.PK);
			var costLine = testObjCreatorForUpdateFactory.CreateCostLine(existingCharge, apInvoice.PK);
			updateFactory.Save();

			Assert("Has Changes", !charge.HasChanges);
			Assert("Revenue Posted", charge.IsRevenuePosted);
			Assert("Cost Posted", charge.IsCostPosted);
			AssertEquals("Revenue Exchange Rate", newExchangeRateValue, charge.JR_OSSellExRate);
			AssertEquals("Cost Exchange Rate", newExchangeRateValue, charge.JR_OSCostExRate);
			AssertEquals("Sell Invoice Exchange Rate", newExchangeRateValue, charge.JR_OSSellInvoiceExRate);
			AssertEquals("Revenue Exchange Rate", newExchangeRateValue, newCharge.JR_OSSellExRate);
			AssertEquals("Cost Exchange Rate", newExchangeRateValue, newCharge.JR_OSCostExRate);
			AssertEquals("Sell Invoice Exchange Rate", newExchangeRateValue, newCharge.JR_OSSellInvoiceExRate);

			// The below portion of the test case is just in case the assertions above do not fail as it is not guaranteed to fail always
			// Since the BusinessObjectsToPublish via DataRefresh have random ordering for Unrelated BusinessObjects, we are enforcing that only ExchangeRate updates are explicitly sent
			// Before fix - we used to take ExchangeRate updates also for Charges IsInDatabase = true
			// After fix - we check Factory.HasContext(BusinessContext.IsUpdatedDueToChangesInDB) && Charge.IsInDatabase. Hence we skip updates for existing Charges on DataRefresh
			Factory.Save();
			var temp = exchangeRateValue;
			exchangeRateValue = newExchangeRateValue;
			newExchangeRateValue = temp;
			charge = newCharge;
			newCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge3", TestObjectCreator.USD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.USD, 100m, TestObjectCreator.Debtor);
			newCharge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.Code;

			updateFactory = new BusinessObjectFactory();
			var existingDebtorExchangeRate = updateFactory.Load<ExchangeRate>(debtorExchangeRate.PK);
			AssertNotNull("Existing Exchange Rate", existingDebtorExchangeRate);
			var existingCreditorExchangeRate = updateFactory.Load<ExchangeRate>(creditorExchangeRate.PK);
			AssertNotNull("Existing Exchange Rate", existingCreditorExchangeRate);
			existingDebtorExchangeRate.JF_BaseRate = newExchangeRateValue;
			existingCreditorExchangeRate.JF_BaseRate = newExchangeRateValue;

			Assert("Charge IsInDatabase", charge.IsInDatabase);
			AssertEquals("Revenue Exchange Rate", exchangeRateValue, charge.JR_OSSellExRate);
			AssertEquals("Cost Exchange Rate", exchangeRateValue, charge.JR_OSCostExRate);
			AssertEquals("Sell Invoice Exchange Rate", exchangeRateValue, charge.JR_OSSellInvoiceExRate);
			AssertEquals("Revenue Exchange Rate", exchangeRateValue, newCharge.JR_OSSellExRate);
			AssertEquals("Cost Exchange Rate", exchangeRateValue, newCharge.JR_OSCostExRate);
			AssertEquals("Sell Invoice Exchange Rate", exchangeRateValue, newCharge.JR_OSSellInvoiceExRate);
			updateFactory.Save();

			Assert("Has Changes", !charge.HasChanges);
			AssertEquals("Revenue Exchange Rate", exchangeRateValue, charge.JR_OSSellExRate);
			AssertEquals("Cost Exchange Rate", exchangeRateValue, charge.JR_OSCostExRate);
			AssertEquals("Sell Invoice Exchange Rate", exchangeRateValue, charge.JR_OSSellInvoiceExRate);
			AssertEquals("Revenue Exchange Rate", newExchangeRateValue, newCharge.JR_OSSellExRate);
			AssertEquals("Cost Exchange Rate", newExchangeRateValue, newCharge.JR_OSCostExRate);
			AssertEquals("Sell Invoice Exchange Rate", newExchangeRateValue, newCharge.JR_OSSellInvoiceExRate);
		}

		public void TestResettingHasChangesOnChargeWhenJR_OSSellExRateHasBeenModified()
		{
			var factory1 = new BusinessObjectFactory();
			var objectCreator1 = new TestObjectCreator(factory1);
			var shipmentInFactory1 = objectCreator1.CreateShipment("S0001", "AUSYD", "USLAX");
			var jobInFactory1 = objectCreator1.CreateJob(shipmentInFactory1, objectCreator1.LocalClient, 0, objectCreator1.Agent, 0);
			var exRateInFactory1 = jobInFactory1.ExchangeRates.AddNew();
			exRateInFactory1.JF_RX_NKRateCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			exRateInFactory1.JF_BaseRate = 1.23m;
			AssertEquals("Should be generic ex rate", ZGuid.Empty, exRateInFactory1.JF_OH_Org);
			factory1.Save();

			var chargeInFactory1 = objectCreator1.CreateCharge(jobInFactory1, objectCreator1.CC1, 0m, 0m);
			chargeInFactory1.JR_OH_SellAccount = objectCreator1.Debtor.PK;
			chargeInFactory1.JR_RX_NKSellCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			Assert(chargeInFactory1.JR_OSSellAmt.IsEmpty);
			AssertEquals(1.23m, chargeInFactory1.JR_OSSellExRate);

			var factory2 = new BusinessObjectFactory();
			var jobInFactory2 = factory2.Load<Job>(jobInFactory1.PK);
			var exRateInFactory2 = jobInFactory2.ExchangeRates[0];
			exRateInFactory2.JF_RX_NKRateCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			exRateInFactory2.JF_BaseRate = 1.56m;
			exRateInFactory2.JF_OrgType = "DEB";
			exRateInFactory2.JF_OH_Org = objectCreator1.Debtor.PK;
			factory2.Save();
			AssertEquals("Sell Ex Rate of Charge in Factory 1 should be updated by data refresh bus", 1.56m, chargeInFactory1.JR_OSSellExRate);
			ErrorReporter.Clear();
			factory1.Save();
			AssertEquals("ChargesInDbModifiedWithoutHasChangesSet_4 key should not be reported", string.Empty, ErrorReporter.LastKeyReported);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Shouldn't be fired on this business object", true);
		}

		public interface IPlugInForTesting : IJobInvoicingPlugIn, IBusiness { }

		public void TestZDecimalsHaveCorrectDecimalPlacesExchangeRate()
		{
			var exList = new List<string> {
					nameof(TestExRate.JF_BaseRate),
					nameof(TestExRate.JF_TodayRate),
				};

			var tester = new DecimalPlacesAttributeTester(TestExRate);
			tester.CheckExchangeRate(exList, nameof(TestExRate.ExchangeRateDecimals));
		}

		public void TestCopyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder()
		{
			var excludedFields = new[]
			{
					JobExRateSchema.JF_JH.Name,
					JobExRateSchema.JF_SystemCreateTimeUtc.Name,
					JobExRateSchema.JF_SystemCreateUser.Name,
					JobExRateSchema.JF_SystemLastEditTimeUtc.Name,
					JobExRateSchema.JF_SystemLastEditUser.Name
				};

			var job = TestObjectCreator.CreateJob("S0001", null, 0, null, 0);
			var expectedFieldValuesByNames = new Dictionary<string, IZType>
			{
					{ JobExRateSchema.JF_BaseRate.Name, (ZDecimal)2 },
					{ JobExRateSchema.JF_CFXMinimum.Name, (ZDecimal)2 },
					{ JobExRateSchema.JF_JH.Name, job.PK },
					{ JobExRateSchema.JF_RX_NKRateCurrency.Name, (ZString)"USD" },
					{ JobExRateSchema.JF_CFXPercent.Name, (ZDecimal)4 },
					{ JobExRateSchema.JF_OH_Org.Name, ZGuid.Empty },
					{ JobExRateSchema.JF_OrgType.Name, (ZString)"DEB" },
					{ JobExRateSchema.JF_IsTransformed.Name, (ZBool)true },
					{ JobExRateSchema.JF_SystemCreateTimeUtc.Name, ZDateTime.Today },
					{ JobExRateSchema.JF_SystemCreateUser.Name, (ZString)"NEW" },
					{ JobExRateSchema.JF_SystemLastEditTimeUtc.Name, ZDateTime.Today },
					{ JobExRateSchema.JF_SystemLastEditUser.Name, (ZString)"NEW" },
					{ JobExRateSchema.JF_InvoiceCurrencyType.Name, (ZString)"" }
				};

			var sourceExRate = Factory.New<ExchangeRate>();
			var destinationExRate = Factory.New<ExchangeRate>();

			this.AssertCopyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(destinationExRate, sourceExRate, ExchangeRate.PersistentFieldsToCopyAndInValidOrder, excludedFields, expectedFieldValuesByNames,
				(destination, source) => destination.CopyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(source));
		}

		public void TestDontOverrideExchangeRateWithJobRateWhenResettingCurrency()
		{
			TestExRate.JF_BaseRate = 1.245m;

			var mockSupporter = new Mock<IJobInvoicingSupporter>();
			mockSupporter.Setup(m => m.ConsumerType).Returns(JobInvoicingConsumerTypes.Shipment);
			mockSupporter.Setup(m => m.IsImport).Returns(true);
			mockSupporter.Setup(m => m.IsExport).Returns(false);
			mockSupporter.Setup(m => m.IsDomestic).Returns(false);
			mockSupporter.Setup(m => m.TransportMode).Returns("AIR");
			mockSupporter.Setup(m => m.OverriddenDepartmentPK).Returns(ZGuid.Empty);
			mockSupporter.Setup(m => m.GetConsolExchangeRate(It.IsAny<ZString>())).Returns(0m);
			mockSupporter.Setup(m => m.ATA).Returns(DateTime.Now);
			mockSupporter.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_AgentCollectAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);

			var mockInvoicingPlugIn = new Mock<IPlugInForTesting>();
			mockInvoicingPlugIn.Setup(m => m.InvoicingSupporter).Returns(mockSupporter.Object);
			mockInvoicingPlugIn.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			mockInvoicingPlugIn.Setup(m => m.IsDeleted).Returns(false);
			TestJob.Parent = mockInvoicingPlugIn.Object;

			AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			RefCurrency currency = newFactory.NewWithValidTestData<RefCurrency>();
			RefExchangeRate rate = currency.ExchangeRates.AddNew();
			rate.RE_ExpiryDate = ZDateTime.Now.AddDays(-1).Date;
			rate.RE_StartDate = ZDateTime.Now.AddDays(-5).Date;
			rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			rate.RE_SellRate = 1.456m;
			newFactory.Save();

			ExchangeRateReader.GetReaderInstance().ClearCache();

			TestExRate.JF_RX_NKRateCurrency = currency.RX_Code;
			AssertEquals("Rate should be calculated from last valid date", 1.456m, TestExRate.JF_BaseRate);

			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "ALL", "ALL", preference: Constants.JobBillingExchangeRatePreference.Code.ConsolExchangeRate);

			TestExRate.JF_RX_NKRateCurrency = "XXX";
			TestExRate.JF_RX_NKRateCurrency = currency.RX_Code;
			TestExRate.JF_BaseRate = 0m;
			AssertEquals("Rate should be zero now", 0m, TestExRate.JF_BaseRate);
		}

		public void TestJF_InvoiceCurrencyTypeReadOnly()
		{
			foreach (var orgType in new[] { ExchangeRateOrgTypeEnum.Debtor.ToCode(), ExchangeRateOrgTypeEnum.Creditor.ToCode(), ExchangeRateOrgTypeEnum.None.ToCode() })
			{
				foreach (var boolValue in new[] { true, false })
				{
					TestExRate.JF_OrgType = orgType;
					TestExRate.JF_IsTransformed = boolValue;

					if (orgType == ExchangeRateOrgTypeEnum.Debtor.ToCode() && boolValue)
					{
						Assert("Invoice Currency Type is editable only when org type is debtor and row added manually", !TestExRate.JF_InvoiceCurrencyTypeInfo.ReadOnly);
					}
					else
					{
						Assert(TestExRate.JF_InvoiceCurrencyTypeInfo.ReadOnly);
					}
				}
			}
		}

		public void TestJF_InvoiceCurrencyType_ResetField()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestExRate.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();
				TestExRate.JF_IsTransformed = true;
				Assert(!TestExRate.JF_InvoiceCurrencyTypeInfo.ReadOnly);
				TestExRate.JF_InvoiceCurrencyType = Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign;

				TestExRate.JF_OrgType = ExchangeRateOrgTypeEnum.Creditor.ToCode();
				Assert(TestExRate.JF_InvoiceCurrencyTypeInfo.ReadOnly);
				AssertEquals("Invoice Currency Type field value is reset when org type is changed to something different from debtor", string.Empty, TestExRate.EffectiveInvoiceCurrencyType);

				TestExRate.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();
				TestExRate.JF_InvoiceCurrencyType = Constants.InvoicePostingExchangeRateCurrencyType.Code.Local;
				TestExRate.JF_OrgType = string.Empty;
				AssertEquals(string.Empty, TestExRate.EffectiveInvoiceCurrencyType);
			}
		}

		public void TestJF_RX()
		{
			TestExRate.JF_RX_NKRateCurrency = TestCode;
			AssertEquals(TestCode, TestExRate.JF_RX_NKRateCurrency);
		}

		public void TestJF_FXChangesJF_BaseRate()
		{
			TestExRate.JF_BaseRate = -1;
			TestExRate.JF_RX_NKRateCurrency = "AAA";
			Assert(TestExRate.JF_BaseRate != -1);
		}

		public void TestJF_CFXSetsJF_SellRate()
		{
			TestExRate.JF_RX_NKRateCurrency = "BBB";
			TestExRate.JF_BaseRate = 0.5m;

			TestExRate.SetIsReciprocal(true);
			TestExRate.JF_CFXPercent = 10;
			AssertEquals(0.55m, TestExRate.JF_SellRate);

			TestExRate.SetIsReciprocal(false);
			TestExRate.JF_CFXPercent = 10;
			AssertEquals(0.45m, TestExRate.JF_SellRate);
		}

		public void TestJF_BaseRateSetsJF_SellRate()
		{
			TestExRate.JF_RX_NKRateCurrency = "CCC";
			TestExRate.JF_CFXPercent = 10m;

			TestExRate.SetIsReciprocal(true);
			TestExRate.JF_BaseRate = 0.5m;
			AssertEquals(0.55m, TestExRate.JF_SellRate);

			TestExRate.SetIsReciprocal(false);
			TestExRate.JF_BaseRate = 0.5m;
			AssertEquals(0.45m, TestExRate.JF_SellRate);
		}

		public void TestValidateJF_RX()
		{
			TestExRate.JF_RX_NKRateCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals(1, TestExRate.JF_RX_NKRateCurrencyInfo.GetErrors().Count());
		}

		public void TestValidateJF_BaseRate()
		{
			var mockSupporter = new Mock<IJobInvoicingSupporter>();
			mockSupporter.Setup(m => m.IsImport).Returns(true);
			mockSupporter.Setup(m => m.IsExport).Returns(false);
			mockSupporter.Setup(m => m.IsDomestic).Returns(false);
			mockSupporter.Setup(m => m.TransportMode).Returns("AIR");
			mockSupporter.Setup(m => m.OverriddenDepartmentPK).Returns(ZGuid.Empty);
			mockSupporter.Setup(m => m.GetConsolExchangeRate(It.IsAny<ZString>())).Returns(0m);
			mockSupporter.Setup(m => m.ATA).Returns(ZDateTime.Today.AddDays(10));
			mockSupporter.Setup(m => m.ATD).Returns(ZDateTime.Today.AddDays(10));
			mockSupporter.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_AgentCollectAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);

			var mockInvoicingPlugIn = new Mock<IPlugInForTesting>();
			mockInvoicingPlugIn.Setup(m => m.InvoicingSupporter).Returns(mockSupporter.Object);
			mockInvoicingPlugIn.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			mockInvoicingPlugIn.Setup(m => m.IsDeleted).Returns(false);
			TestJob.Parent = mockInvoicingPlugIn.Object;
			TestJob.JH_SystemCreateTimeUtc = ZDateTime.Now;

			var newFactory = new BusinessObjectFactory();
			var currency = newFactory.NewWithValidTestData<RefCurrency>();
			var rateBuy = currency.ExchangeRates.AddNew();
			rateBuy.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
			rateBuy.RE_StartDate = ZDateTime.Today.AddDays(-1);
			rateBuy.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			rateBuy.RE_SellRate = 0.4m;
			newFactory.Save();
			Enterprise.ZArchitecture.Core.ExchangeRateReader.GetReaderInstance().ClearCache();

			TestExRate.JF_RX_NKRateCurrency = currency.Code;
			Assert("Pre-condition", !TestExRate.JF_TodayRate.IsEmpty);
			TestExRate.ZAccExchangeRate_ForTestOnly.Rate = TestExRate.JF_TodayRate + 1m;
			AssertEquals(1, TestExRate.JF_BaseRateInfo.GetWarnings().GetUniqueMessageList().Length);
			TestExRate.ZAccExchangeRate_ForTestOnly.Rate = TestExRate.JF_TodayRate;
			AssertEquals(0, TestExRate.JF_BaseRateInfo.GetWarnings().GetUniqueMessageList().Length);
		}

		public void TestValidateJF_TodayRate()
		{
			var mockSupporter = new Mock<IJobInvoicingSupporter>();
			mockSupporter.Setup(m => m.IsImport).Returns(true);
			mockSupporter.Setup(m => m.IsExport).Returns(false);
			mockSupporter.Setup(m => m.IsDomestic).Returns(false);
			mockSupporter.Setup(m => m.TransportMode).Returns("AIR");
			mockSupporter.Setup(m => m.OverriddenDepartmentPK).Returns(ZGuid.Empty);
			mockSupporter.Setup(m => m.GetConsolExchangeRate(It.IsAny<ZString>())).Returns(0m);
			mockSupporter.Setup(m => m.ATA).Returns(ZDateTime.Today.AddDays(10));
			mockSupporter.Setup(m => m.ATD).Returns(ZDateTime.Today.AddDays(10));
			mockSupporter.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_AgentCollectAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);

			var mockInvoicingPlugIn = new Mock<IPlugInForTesting>();
			mockInvoicingPlugIn.Setup(m => m.InvoicingSupporter).Returns(mockSupporter.Object);
			mockInvoicingPlugIn.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			mockInvoicingPlugIn.Setup(m => m.IsDeleted).Returns(false);
			TestJob.Parent = mockInvoicingPlugIn.Object;
			TestJob.JH_SystemCreateTimeUtc = ZDateTime.Now;

			AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var newFactory = new BusinessObjectFactory();
			var currency = newFactory.NewWithValidTestData<RefCurrency>();
			var rateSell = currency.ExchangeRates.AddNew();
			rateSell.RE_ExpiryDate = ZDateTime.Today.AddDays(11);
			rateSell.RE_StartDate = ZDateTime.Today.AddDays(9);
			rateSell.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			rateSell.RE_SellRate = 0.9250m;
			var rateBuy = currency.ExchangeRates.AddNew();
			rateBuy.RE_ExpiryDate = ZDateTime.Today.AddDays(11);
			rateBuy.RE_StartDate = ZDateTime.Today.AddDays(9);
			rateBuy.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			rateBuy.RE_SellRate = 0.4000m;
			var rateSellToday = currency.ExchangeRates.AddNew();
			rateSellToday.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
			rateSellToday.RE_StartDate = ZDateTime.Today.AddDays(-1);
			rateSellToday.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			rateSellToday.RE_SellRate = 0.5000m;
			var rateBuyToday = currency.ExchangeRates.AddNew();
			rateBuyToday.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
			rateBuyToday.RE_StartDate = ZDateTime.Today.AddDays(-1);
			rateBuyToday.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			rateBuyToday.RE_SellRate = 0.6000m;
			newFactory.Save();

			Enterprise.ZArchitecture.Core.ExchangeRateReader.GetReaderInstance().ClearCache();
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("ALL", "ALL", "ALL", "SEL", Constants.JobBillingExchangeRatePreference.Code.TodaysRate);
			GlbCompany.CurrentCompany.Factory.Save();

			TestExRate.JF_RX_NKRateCurrency = "XXX";
			TestExRate.JF_RX_NKRateCurrency = currency.RX_Code;
			AssertEquals(0.5000m, TestExRate.JF_BaseRate);
			AssertEquals(0.5000m, TestExRate.JF_SellRate);
			AssertEquals(0.5000m, TestExRate.JF_TodayRate);

			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("ALL", "ALL", "ALL", "BUY", Constants.JobBillingExchangeRatePreference.Code.TodaysRate);
			GlbCompany.CurrentCompany.Factory.Save();
			TestExRate.JF_RX_NKRateCurrency = "XXX";
			TestExRate.JF_RX_NKRateCurrency = currency.RX_Code;
			AssertEquals(0.6000m, TestExRate.JF_BaseRate);
			AssertEquals(0.6000m, TestExRate.JF_SellRate);
			AssertEquals(0.6000m, TestExRate.JF_TodayRate);
		}

		public void TestJF_BaseRate_WithLocalClient()
		{
			var mockSupporter = new Mock<IJobInvoicingSupporter>();
			mockSupporter.Setup(m => m.IsImport).Returns(true);
			mockSupporter.Setup(m => m.IsExport).Returns(false);
			mockSupporter.Setup(m => m.IsDomestic).Returns(false);
			mockSupporter.Setup(m => m.TransportMode).Returns("AIR");
			mockSupporter.Setup(m => m.OverriddenDepartmentPK).Returns(ZGuid.Empty);
			mockSupporter.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_AgentCollectAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);

			var mockInvoicingPlugIn = new Mock<IPlugInForTesting>();
			mockInvoicingPlugIn.Setup(m => m.InvoicingSupporter).Returns(mockSupporter.Object);
			mockInvoicingPlugIn.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			mockInvoicingPlugIn.Setup(m => m.IsDeleted).Returns(false);
			TestJob.Parent = mockInvoicingPlugIn.Object;
			TestJob.LocalZAddressWithContact.OrgPK = TestObjectCreator.LocalClient.PK;

			AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var newFactory = new BusinessObjectFactory();
			var currency = newFactory.NewWithValidTestData<RefCurrency>();
			var rateSell = currency.ExchangeRates.AddNew();
			rateSell.RE_ExpiryDate = ZDateTime.Today.AddDays(-10);
			rateSell.RE_StartDate = ZDateTime.Today.AddDays(-5);
			rateSell.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			rateSell.RE_OH_Client = TestObjectCreator.LocalClient.PK;
			rateSell.RE_SellRate = 0.5000m;
			var rateBuy = currency.ExchangeRates.AddNew();
			rateBuy.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
			rateBuy.RE_StartDate = ZDateTime.Today.AddDays(-1);
			rateBuy.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			rateBuy.RE_SellRate = 0.4000m;
			var rateCus = currency.ExchangeRates.AddNew();
			rateCus.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
			rateCus.RE_StartDate = ZDateTime.Today.AddDays(-1);
			rateCus.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			rateCus.RE_SellRate = 0.3000m;
			newFactory.Save();

			Enterprise.ZArchitecture.Core.ExchangeRateReader.GetReaderInstance().ClearCache();
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("ALL", "ALL", "ALL", Core.Constants.ExchangeRateTypes.Code.SellRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);
			GlbCompany.CurrentCompany.Factory.Save();

			TestExRate.JF_RX_NKRateCurrency = "XXX";
			TestExRate.JF_RX_NKRateCurrency = currency.RX_Code;
			AssertEquals("Should use the SEL rate.", 0.3000m, TestExRate.JF_BaseRate);
			AssertHasWarning(TestExRate.JF_BaseRateInfo, "No SEL rate is available for this client. Standard exchange rate will be used.");
			AssertEquals("Should use the SEL rate.", 0.3000m, TestExRate.JF_SellRate);
			AssertEquals(0.3000m, TestExRate.JF_TodayRate);

			Enterprise.ZArchitecture.Core.ExchangeRateReader.GetReaderInstance().ClearCache();

			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("ALL", "ALL", "ALL", Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);
			GlbCompany.CurrentCompany.Factory.Save();

			TestExRate.JF_RX_NKRateCurrency = "XXX";
			TestExRate.JF_RX_NKRateCurrency = currency.RX_Code;
			AssertEquals("Should use the BUY rate for the specified client.", 0.5000m, TestExRate.JF_BaseRate);
			AssertEquals("Should use the BUY rate for the specified client.", 0.5000m, TestExRate.JF_SellRate);
			AssertEquals(0.4000m, TestExRate.JF_TodayRate);

			Enterprise.ZArchitecture.Core.ExchangeRateReader.GetReaderInstance().ClearCache();

			TestJob.LocalZAddressWithContact.OrgPK = TestObjectCreator.ActiveOrg.PK;
			TestExRate.JF_RX_NKRateCurrency = "XXX";
			TestExRate.JF_RX_NKRateCurrency = currency.RX_Code;
			AssertEquals("The rate with client should only be used for the specified one.", 0.4000m, TestExRate.JF_BaseRate);
			AssertEquals("The rate with client should only be used for the specified one.", 0.4000m, TestExRate.JF_SellRate);
			AssertEquals(0.4000m, TestExRate.JF_TodayRate);
		}

		public void TestIsBuyRateEqualsTodayRate()
		{
			TestExRate.JF_BaseRate = TestExRate.JF_TodayRate + 1m;
			Assert("IsBuyRateEqualsTodayRate should be true", !TestExRate.IsBuyRateEqualsTodayRate);

			TestExRate.JF_BaseRate = TestExRate.JF_TodayRate;
			Assert("IsBuyRateEqualsTodayRate should be false", TestExRate.IsBuyRateEqualsTodayRate);
		}

		public void TestValidateJF_BaseRateShowsErrorsWhenBuyRateIsZero()
		{
			RefCurrency testCurrency = Factory.NewWithValidTestData<RefCurrency>();
			TestExRate.JF_RX_NKRateCurrency = testCurrency.RX_Code;
			TestExRate.JF_BaseRate = -0.1M;
			AssertHasError(TestExRate.JF_BaseRateInfo, "Base Rate must be greater than zero.");
			TestExRate.JF_BaseRate = 0M;
			AssertNoErrors(TestExRate.JF_BaseRateInfo);
			AssertHasWarning(TestExRate.JF_BaseRateInfo, "Base Rate must be greater than zero.");

			TestExRate.RunPreSaveValidation();
			AssertNoErrors("Used to be an error from ZExchangeRate additional validation", TestExRate.JF_BaseRateInfo);
			AssertHasWarning("Now we should have just warning", TestExRate.JF_BaseRateInfo, "Base Rate must be greater than zero.");

			TestExRate.JF_BaseRate = 0.1M;
			AssertNoError(TestExRate.JF_BaseRateInfo, "Base Rate must be greater than zero.");
			AssertNoWarning(TestExRate.JF_BaseRateInfo, "Base Rate must be greater than zero.");
		}

		public void TestValidateJF_TodayRateShowsWarningWhenTodaysRateCannotBeFound()
		{
			var mockSupporter = new Mock<IJobInvoicingSupporter>();
			mockSupporter.Setup(m => m.IsImport).Returns(true);
			mockSupporter.Setup(m => m.IsExport).Returns(false);
			mockSupporter.Setup(m => m.IsDomestic).Returns(false);
			mockSupporter.Setup(m => m.TransportMode).Returns("AIR");
			mockSupporter.Setup(m => m.OverriddenDepartmentPK).Returns(ZGuid.Empty);
			mockSupporter.Setup(m => m.GetConsolExchangeRate(It.IsAny<ZString>())).Returns(0m);
			mockSupporter.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_AgentCollectAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);

			var mockInvoicingPlugIn = new Mock<IPlugInForTesting>();
			mockInvoicingPlugIn.Setup(m => m.InvoicingSupporter).Returns(mockSupporter.Object);
			mockInvoicingPlugIn.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			mockInvoicingPlugIn.Setup(m => m.IsDeleted).Returns(false);
			TestJob.Parent = mockInvoicingPlugIn.Object;

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			RefCurrency testCurrency = newFactory.NewWithValidTestData<RefCurrency>();
			newFactory.Save();

			Enterprise.ZArchitecture.Core.ExchangeRateReader.GetReaderInstance().ClearCache();

			TestExRate.JF_RX_NKRateCurrency = testCurrency.RX_Code;
			AssertEquals(1m, TestExRate.JF_BaseRate);
			TestExRate.JF_BaseRate = 0m; // for tests we always set to 1m insted of 0m so need to set it here explicitly

			AssertEquals("Should calculate nothing", 0m, TestExRate.JF_BaseRate);
			Assert("Should have warning: 'Could not find Today's exchange rate.'", TestExRate.JF_TodayRateInfo.HasWarning("Could not find Today's exchange rate."));

			RefExchangeRate testExchangeRate = testCurrency.ExchangeRates.AddNew();
			testExchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-1);
			testExchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
			testExchangeRate.RE_SellRate = 0.8m;
			testExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			testExchangeRate.RE_RX_NKExCurrency = testCurrency.RX_Code;
			testExchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
			newFactory.Save();

			Enterprise.ZArchitecture.Core.ExchangeRateReader.GetReaderInstance().ClearCache();

			TestExRate.JF_RX_NKRateCurrency = ZString.Empty;
			TestExRate.JF_RX_NKRateCurrency = testCurrency.RX_Code;

			AssertEquals("Should calculate correct ex rate", 0.8m, TestExRate.JF_BaseRate);
			Assert("Should NOT have warning: 'Could not find Today's exchange rate.'", !TestExRate.JF_TodayRateInfo.HasWarning("Could not find Today's exchange rate."));
		}

		public void TestExchangeRateFallback()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			bool previousExRateFallback = AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.Value;
			try
			{
				AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
				RefExchangeRate exRate = Factory.NewWithValidTestData<RefExchangeRate>();
				exRate.RE_GC = GlbCompany.CurrentCompany.PK;
				exRate.RE_RX_NKExCurrency = currency.RX_Code;
				exRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
				exRate.RE_StartDate = ZDateTime.Today.AddDays(-4);
				exRate.RE_ExpiryDate = ZDateTime.Today.AddDays(-3);
				exRate.RE_SellRate = 0.355m;

				TestExRate.JF_BaseRate = 0m;
				TestExRate.JF_RX_NKRateCurrency = currency.RX_Code;
				AssertEquals("Buy Rate is 0.355", 0.355m, TestExRate.JF_BaseRate);
				AssertHasWarning("Buy Rate should have the following warning:", TestExRate.JF_BaseRateInfo, "The base rate on this job is not the same as today's rate.");

				AssertEquals("Today's Rate should not be 0.355", 0m, TestExRate.JF_TodayRate);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, previousExRateFallback);
			}
		}

		public void TestBuyRateValidationExpiryDateWarning()
		{
			bool previousExRateFallback = AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.Value;
			try
			{
				AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
				RefExchangeRate exRate = Factory.NewWithValidTestData<RefExchangeRate>();
				exRate.RE_GC = GlbCompany.CurrentCompany.PK;
				exRate.RE_RX_NKExCurrency = currency.RX_Code;
				exRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
				exRate.RE_StartDate = ZDateTime.Today.AddDays(-3);
				exRate.RE_ExpiryDate = ZDateTime.Today.AddDays(-2);
				exRate.RE_SellRate = 34.22m;

				TestExRate.ZAccExchangeRate_ForTestOnly.Currency = currency.RX_Code;
				TestExRate.Validation.ValidateJF_BaseRate();
				Assert("JF_BaseRate should have the exchange rate expired warning", TestExRate.JF_BaseRateInfo.HasWarning(TestExRate.ZAccExchangeRate_ForTestOnly.ExpiryDateWarning));
				TestExRate.ZAccExchangeRate_ForTestOnly.Rate = 20.00m;
				TestExRate.Validation.ValidateJF_BaseRate();
				Assert("JF_BaseRate should not have the exchange rate expired warning", !TestExRate.JF_BaseRateInfo.HasWarning(TestExRate.ZAccExchangeRate_ForTestOnly.ExpiryDateWarning));
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, previousExRateFallback);
			}
		}

		public void TestRoundingTheSellAndBuy()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			TestObjectCreator testObjectCreator = new TestObjectCreator(newFactory);

			TestExRate.JF_BaseRate = 0.131506272M;
			AssertEquals(0.131506m, TestExRate.JF_BaseRate);

			AssertEquals(0.131506m, TestExRate.JF_SellRate);

			RefExchangeRate rate = testObjectCreator.AUD.ExchangeRates.AddNew();
			rate.RE_StartDate = ZDateTime.Today;
			rate.RE_ExpiryDate = ZDateTime.Today;
			rate.RE_ExRateType = Constants.ExchangeRateTypes.Code.BuyRate;
			rate.RE_SellRate = 0.131506272M;

			rate = testObjectCreator.AUD.ExchangeRates.AddNew();
			rate.RE_StartDate = ZDateTime.Today;
			rate.RE_ExpiryDate = ZDateTime.Today;
			rate.RE_ExRateType = Constants.ExchangeRateTypes.Code.SellRate;
			rate.RE_SellRate = 0.137765715M;
			newFactory.Save();

			AssertEquals(0.131506m, TestExRate.GetExchangeRate(testObjectCreator.AUD.RX_Code));
		}

		public void TestAdjustBuyRateForCFXHandlesZDecimalOverflow()
		{
			AssertEquals("As overflow occurs 0 should be returned", 0m, ExchangeRateHelper.GetBaseRateAdjustedByCFX(TestExRate.JF_RX_NKRateCurrency, decimal.MaxValue, decimal.MaxValue, GlbCompany.CurrentCompany));
			AssertNotEquals("As No overflow occurs a NonZero value should be returned", 0m, ExchangeRateHelper.GetBaseRateAdjustedByCFX(TestExRate.JF_RX_NKRateCurrency, 256.3625, 20.369, GlbCompany.CurrentCompany));
		}

		public void TestTodayRateIsGottenFromJob()
		{
			void setupConfig(AccExchangeRateConfigurationCollection configCol, string exRateType)
			{
				var config = configCol.AddNew();
				config.JCE_JobType = "ALL";
				config.JCE_ServiceDirection = Constants.FreightShipmentDirection.Code.All;
				config.JCE_TransportMode = Constants.TransportModes.All;
				config.GetCurrencyConfig(ZString.Empty, ZDate.Empty).JCT_ExRateType = exRateType;
			}

			ExchangeRateReader.GetReaderInstance().ClearCache();
			var rateFactory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(rateFactory);
			creator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, 0.1m, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			creator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.SellRate, 0.2m, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			creator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.IATARate, 0.3m, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			rateFactory.Save();

			setupConfig(GlbCompany.CurrentCompany.AccExchangeRateConfigurations, Constants.ExchangeRateTypes.Code.BuyRate);
			setupConfig(TestObjectCreator.AALSHI.CompanyData.AccAPExchangeRateConfigurations, Constants.ExchangeRateTypes.Code.SellRate);
			setupConfig(TestObjectCreator.AALSHI.CompanyData.AccARExchangeRateConfigurations, Constants.ExchangeRateTypes.Code.IATARate);

			TestJob.PlugInData = TestObjectCreator.GetTestShipmentPlugIn();
			TestExRate.JF_RX_NKRateCurrency = TestObjectCreator.USD.RX_Code;

			void TestCase(OrgHeader org, ExchangeRateOrgTypeEnum orgType, ZDecimal expectedTodaysRate)
			{
				TestExRate.JF_OH_Org = org?.PK ?? ZGuid.Empty;
				TestExRate.OrgType = orgType;
				AssertEquals(expectedTodaysRate, TestExRate.JF_TodayRate);
			}

			TestCase(null, ExchangeRateOrgTypeEnum.None, 0.1m);
			TestCase(null, ExchangeRateOrgTypeEnum.Creditor, 0.1m);
			TestCase(null, ExchangeRateOrgTypeEnum.Debtor, 0.1m);

			TestCase(TestObjectCreator.AALSHI, ExchangeRateOrgTypeEnum.None, 0.1m);
			TestCase(TestObjectCreator.AALSHI, ExchangeRateOrgTypeEnum.Creditor, 0.2m);
			TestCase(TestObjectCreator.AALSHI, ExchangeRateOrgTypeEnum.Debtor, 0.3m);

			TestCase(TestObjectCreator.Debtor, ExchangeRateOrgTypeEnum.Creditor, 0.1m);
			TestCase(TestObjectCreator.ABIGAS, ExchangeRateOrgTypeEnum.Debtor, 0.1m);
		}

		public void TestOnChangeRaisedOnCFXorRateUpdate()
		{
			// Arrange
			bool onChangeRaised = false;

			void OnRateChanged(object sender, EventArgs e)
			{
				onChangeRaised = true;
			}

			var rate = TestExRate;
			rate.JF_RX_NKRateCurrency = TestObjectCreator.USD.RX_Code;

			rate.Changed += OnRateChanged;

			void AssertOnChangeRaised(Action<ExchangeRate> action)
			{
				onChangeRaised = false;
				action(rate);
				Assert(onChangeRaised);
			}

			try
			{
				//Act & Assert
				AssertOnChangeRaised(r => r.JF_BaseRate = 0.7m);
				AssertOnChangeRaised(r => r.JF_CFXMinimum = 10m);
				AssertOnChangeRaised(r => r.JF_CFXPercent = 50m);
			}
			finally
			{
				rate.Changed -= OnRateChanged;
			}
		}

		public void TestOrgOrOrgTypeOrCurrencyChangeCausesRatesRefresh()
		{
			// Arrange
			var rate = TestExRate;
			rate.JF_IsTransformed = true;

			void AssertRatesRefreshed(Action<ExchangeRate> action)
			{
				Assert(!rate.HasRowErrors);
				action(rate);
				Assert(TestJob.IsRefreshChargeLinesExchangeRateBindingCalled);
				TestJob.ResetIsCalledFlags();
			}

			// Act & Assert
			AssertRatesRefreshed(r => r.JF_RX_NKRateCurrency = "USD");
			AssertRatesRefreshed(r => r.JF_OH_Org = TestObjectCreator.AALSHI.PK);
			AssertRatesRefreshed(r => r.OrgType = ExchangeRateOrgTypeEnum.Debtor);
		}

		[TestDate(2025, 1, 8)]
		[DisableZeroExchangeRateOverriding]
		public void TestOrgOrOrgTypeOrCurrencyChangeCausesCFXandRateRecalc()
		{
			// Arrange
			var rate = TestExRate;
			var org1 = TestObjectCreator.AALSHI;
			var org2 = TestObjectCreator.ABIGAS;

			var org1CfxUSDDefault = TestObjectCreator.CreateCFXUplift(org1.CompanyData.AccCFXConfigurations, currencyCode: "USD", percentage: 0.11m, minimum: 11m);
			var org1CfxUSDExpired = TestObjectCreator.CreateCFXUplift(org1.CompanyData.AccCFXConfigurations, currencyCode: "USD", percentage: 0.12m, minimum: 12m, startDate: new ZDate(2025, 1, 3), expiryDate: new ZDate(2025, 1, 5));
			var org1CfxUSDActive = TestObjectCreator.CreateCFXUplift(org1.CompanyData.AccCFXConfigurations, currencyCode: "USD", percentage: 0.13m, minimum: 13m, startDate: new ZDate(2025, 1, 6), expiryDate: new ZDate(2025, 1, 9));
			var org1CfxEURDefault = TestObjectCreator.CreateCFXUplift(org1.CompanyData.AccCFXConfigurations, currencyCode: "EUR", percentage: 0.21m, minimum: 21m);
			var org2CfxUSDDefault = TestObjectCreator.CreateCFXUplift(org2.CompanyData.AccCFXConfigurations, currencyCode: "USD", percentage: 0.22m, minimum: 22m);
			var org2CfxEURDefault = TestObjectCreator.CreateCFXUplift(org2.CompanyData.AccCFXConfigurations, currencyCode: "EUR", percentage: 0.23m, minimum: 23m);

			rate.SetContext(BusinessContext.InvoicingPlugInGUI);

			// Act
			rate.JF_RX_NKRateCurrency = "USD";
			rate.OrgType = ExchangeRateOrgTypeEnum.Debtor;
			rate.JF_OH_Org = org1.PK;

			// Assert
			AssertEquals(org1CfxUSDActive.JCF_CFXPercentage, rate.JF_CFXPercent);
			AssertEquals(org1CfxUSDActive.JCF_CFXMinimum, rate.JF_CFXMinimum);
			AssertEquals(0m, rate.JF_BaseRate);

			// Act
			rate.JF_BaseRate = 0.7m;
			rate.JF_CFXPercent = 78m;
			rate.JF_CFXMinimum = 16m;
			rate.OrgType = ExchangeRateOrgTypeEnum.None;
			rate.OrgType = ExchangeRateOrgTypeEnum.Debtor;

			// Assert
			AssertEquals(org1CfxUSDActive.JCF_CFXPercentage, rate.JF_CFXPercent);
			AssertEquals(org1CfxUSDActive.JCF_CFXMinimum, rate.JF_CFXMinimum);
			AssertEquals(0m, rate.JF_BaseRate);

			// Act
			rate.JF_BaseRate = 0.7m;
			rate.JF_RX_NKRateCurrency = "EUR";

			// Assert
			AssertEquals(org1CfxEURDefault.JCF_CFXPercentage, rate.JF_CFXPercent);
			AssertEquals(org1CfxEURDefault.JCF_CFXMinimum, rate.JF_CFXMinimum);
			AssertEquals(0m, rate.JF_BaseRate);

			// Act
			rate.JF_BaseRate = 0.7m;
			rate.JF_OH_Org = org2.PK;

			// Assert
			AssertEquals(org2CfxEURDefault.JCF_CFXPercentage, rate.JF_CFXPercent);
			AssertEquals(org2CfxEURDefault.JCF_CFXMinimum, rate.JF_CFXMinimum);
			AssertEquals(0m, rate.JF_BaseRate);
		}

		[TestDate(2025, 1, 8)]
		[DisableZeroExchangeRateOverriding]
		public void TestRefreshCFXMinimum()
		{
			// Arrange
			var rate = TestExRate;
			var org1 = TestObjectCreator.AALSHI;

			var org1CfxUSDDefault = TestObjectCreator.CreateCFXUplift(org1.CompanyData.AccCFXConfigurations, currencyCode: "USD", percentage: 0.11m, minimum: 11m);
			var org1CfxUSDExpired = TestObjectCreator.CreateCFXUplift(org1.CompanyData.AccCFXConfigurations, currencyCode: "USD", percentage: 0.12m, minimum: 12m, startDate: new ZDate(2025, 1, 3), expiryDate: new ZDate(2025, 1, 5));
			var org1CfxUSDActive = TestObjectCreator.CreateCFXUplift(org1.CompanyData.AccCFXConfigurations, currencyCode: "USD", percentage: 0.13m, minimum: 13m, startDate: new ZDate(2025, 1, 6), expiryDate: new ZDate(2025, 1, 9));

			rate.SetContext(BusinessContext.InvoicingPlugInGUI);

			rate.JF_RX_NKRateCurrency = "USD";
			rate.OrgType = ExchangeRateOrgTypeEnum.Debtor;
			rate.JF_OH_Org = org1.PK;

			AssertEquals(org1CfxUSDActive.JCF_CFXPercentage, rate.JF_CFXPercent);
			AssertEquals(org1CfxUSDActive.JCF_CFXMinimum, rate.JF_CFXMinimum);
			AssertEquals(0m, rate.JF_BaseRate);

			// Act
			rate.JF_BaseRate = 0.7m;
			rate.JF_CFXPercent = 78m;
			rate.JF_CFXMinimum = 0m;
			rate.RefreshCFXMinimum();

			// Assert
			AssertEquals(78m, rate.JF_CFXPercent);
			AssertEquals(org1CfxUSDActive.JCF_CFXMinimum, rate.JF_CFXMinimum);
		}

		public void TestSkipsOnChangeOrRefreshInCaseOfRowError()
		{
			// Arrange
			var rate = TestExRate;
			rate.JF_IsTransformed = true;

			rate.AddRowError("Test Row Error");
			rate.SetContext(BusinessContext.InvoicingPlugInGUI);

			rate.Changed += OnRateChanged;

			bool onChangeRaised = false;

			void OnRateChanged(object sender, EventArgs e)
			{
				onChangeRaised = true;
			}

			void AssertNoUpdates(Action<ExchangeRate> action)
			{
				TestJob.ResetIsCalledFlags();
				Assert(rate.HasRowErrors);
				action(rate);
				Assert(!TestJob.IsRefreshChargeLinesExchangeRateBindingCalled);
				Assert(!onChangeRaised);
			}

			try
			{
				// Act & Assert
				AssertNoUpdates(r => r.JF_RX_NKRateCurrency = "USD");
				AssertNoUpdates(r => r.JF_RX_NKRateCurrency = "EUR");
				AssertNoUpdates(r => r.JF_OH_Org = TestObjectCreator.AALSHI.PK);
				AssertNoUpdates(r => r.OrgType = ExchangeRateOrgTypeEnum.Debtor);
				AssertNoUpdates(r => r.JF_BaseRate = 0.7m);
				AssertNoUpdates(r => r.JF_CFXMinimum = 10m);
				AssertNoUpdates(r => r.JF_CFXPercent = 50m);
			}
			finally
			{
				rate.Changed -= OnRateChanged;
			}
		}

		public void TestCanDelete()
		{
			var rate = TestExRate;
			Assert(rate.CanDelete);

			void OnRateChanged(object sender, EventArgs e)
			{
			}

			rate.Changed += OnRateChanged;

			Assert(!rate.CanDelete);
			AssertEquals("Cannot remove the selected Currency as it is being used by at least one of the charge line.", rate.ReasonForNotAbleToDelete);
		}

		public void TestReadOnlyCFXAppliesCorrectly()
		{
			var rate = TestExRate;

			rate.JF_IsTransformed = true;
			rate.OrgType = ExchangeRateOrgTypeEnum.None;
			rate.JF_OH_Org = ZGuid.Empty;

			Assert(rate.JF_CFXMinimumInfo.ReadOnly);
			Assert(rate.JF_CFXPercentInfo.ReadOnly);
			Assert(rate.JF_IsTransformedInfo.ReadOnly);
			Assert(!rate.JF_OH_OrgInfo.ReadOnly);
			Assert(!rate.JF_OrgTypeInfo.ReadOnly);
			Assert(!rate.JF_BaseRateInfo.ReadOnly);

			rate.OrgType = ExchangeRateOrgTypeEnum.Debtor;
			rate.JF_OH_Org = TestObjectCreator.AALSHI.PK;
			rate.JF_IsTransformed = false;

			TestJob.CFXOverrideEnabled = true;

			Assert(!rate.JF_CFXMinimumInfo.ReadOnly);
			Assert(!rate.JF_CFXPercentInfo.ReadOnly);
			Assert(rate.JF_IsTransformedInfo.ReadOnly);
			Assert(rate.JF_OH_OrgInfo.ReadOnly);
			Assert(rate.JF_OrgTypeInfo.ReadOnly);
			Assert(!rate.JF_BaseRateInfo.ReadOnly);

			TestJob.CFXOverrideEnabled = false;

			Assert(rate.JF_CFXMinimumInfo.ReadOnly);
			Assert(rate.JF_CFXPercentInfo.ReadOnly);
			Assert(rate.JF_IsTransformedInfo.ReadOnly);
			Assert(rate.JF_OH_OrgInfo.ReadOnly);
			Assert(rate.JF_OrgTypeInfo.ReadOnly);
			Assert(!rate.JF_BaseRateInfo.ReadOnly);
		}

		public void TestIsGeneric()
		{
			Assert(!TestExRate.IsGenericRate);

			TestExRate.JF_IsTransformed = true;
			TestExRate.OrgType = ExchangeRateOrgTypeEnum.None;
			TestExRate.JF_OH_Org = ZGuid.Empty;

			Assert(TestExRate.IsGenericRate);
		}

		public void TestIsDataVersionsAutoLogged_True()
		{
			Assert("Data Versions should be logged", TestExRate.IsDataVersionsAutoLogged_ForTestOnly);
		}

		public void TestUpdateCfxAndRateWhenOrgOrOrgTypeChangedWithoutParentJob()
		{
			TestExRate.JF_RX_NKRateCurrency = TestObjectCreator.AUD.Code;
			TestExRate.JF_JH = ZGuid.Empty;
			TestExRate.SetContext(BusinessContext.InvoicingPlugInGUI);
			TestExRate.OrgType = ExchangeRateOrgTypeEnum.Debtor;
			AssertNoExceptionThrown("Should not throw null-reference exception when update exchange rate", () => TestExRate.JF_RX_NKRateCurrency = TestObjectCreator.USD.Code);
		}

		public void TestOnChangedWithoutParentJob()
		{
			TestExRate.JF_OrgType = ExchangeRateOrgTypeEnum.Creditor.ToCode();
			TestExRate.JF_JH = ZGuid.Empty;
			TestExRate.JF_IsTransformed = true;
			AssertEquals("Pre-condition", false, TestExRate.HasContext(BusinessContext.InvoicingPlugInGUI));
			AssertNoExceptionThrown("Should not throw null-reference exception when key property changed.", () => TestExRate.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode());
		}

		public void TestGetAdditionalInfoForZSaveExceptionWithoutParentJob()
		{
			TestExRate.JF_RX_NKRateCurrency = TestObjectCreator.AUD.Code;
			TestExRate.JF_JH = ZGuid.Empty;
			AssertNoExceptionThrown("Should not throw null-reference exception when get ZSaveException info.", () => TestExRate.GetAdditionalInfoForZSaveException());
		}

		public void TestErrorIsReportedWhenDeletedExchangeRate()
		{
			ErrorReporter.Clear();

			var rate = Factory.New<ExchangeRate>();
			AssertEquals(false, rate.IsDeleted);

			rate.Delete();
			AssertEquals(true, rate.IsDeleted);

			_ = rate.JF_JH;

			Assert(ErrorReporter.LastMessageReported.Contains("Access property from a detached rate."));
			Assert(ErrorReporter.LastExceptionReported.Message.Contains("This row has been removed from a table and does not have any data.  BeginEdit() will allow creation of new data in this row."));

			ErrorReporter.Clear();
		}

		public void TestExchangeRateReadonly()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001000");
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var exRate = job.ExchangeRates.AddNew();
				AssertEquals(Env.Security.MaintainShipmentJobInvoicing.Code, job.PlugInData.InvoicingSupporter.JobInvoicingSecurity.Code);
				var securityCheckPoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideBaseExchangeRate);

				securityCheckPoint.IsAllowed = true;
				AssertEquals(false, exRate.JF_BaseRate_ReadOnly);

				securityCheckPoint.IsAllowed = false;
				AssertEquals(true, exRate.JF_BaseRate_ReadOnly);
			}
		}

		public void TestExchangeRateReadonly_EmptyJob()
		{
			var exRate = Factory.New<ExchangeRate>();
			AssertNull(exRate.ParentJob?.PlugInData?.InvoicingSupporter?.JobInvoicingSecurity);
			AssertEquals(false, exRate.JF_BaseRate_ReadOnly);
		}

		#region Auto Delete

		public void TestAutoRateGetsDeletedWhenNoChargeUseIt()
		{
			TestAutoDeletion(false);
		}

		public void TestManualRateIsNotDeletedWhenNoChargeUseIt()
		{
			TestAutoDeletion(true);
		}

		void TestAutoDeletion(bool isTransformed)
		{
			// Arrange
			var rate = TestExRate;
			void OnRateChanged(object sender, EventArgs e)
			{
			}

			rate.JF_IsTransformed = isTransformed;
			rate.Changed += OnRateChanged;
			Assert(!rate.IsDeleted);

			//Act
			rate.Changed -= OnRateChanged;

			//Assert;
			AssertEquals(!isTransformed, rate.IsDeleted);
		}

		#endregion

		#region SellRateWithCFXIsRounded (Non-Reciprocal)

		public void TestSellRateWithCFXIsRoundedForNonReciprocal()
		{
			TestExRate.SetIsReciprocal(false);
			TestExRate.JF_BaseRate = 0.5342m;
			TestExRate.JF_CFXPercent = 3m;
			AssertEquals("Sell Rate should be rounded to 4 decimal places", 0.518174m, TestExRate.JF_SellRate);
		}

		#endregion

		#region SellRateWithCFX (Reciprocal)

		public void TestSellRateWithCFXForReciprocal()
		{
			TestExRate.SetIsReciprocal(true);
			TestExRate.JF_BaseRate = 0.384978m;
			TestExRate.JF_CFXPercent = 3m;
			AssertEquals("Sell Rate should be rounded to 6 decimal places", 0.396527m, TestExRate.JF_SellRate);
		}

		#endregion

		#region CurrencyIsExcludedFromCFXCalculation

		public void TestCurrencyIsExcludedFromCFXCalculation()
		{
			TestExRate.SetIsReciprocal(true);
			TestExRate.JF_BaseRate = 0.327073m;
			TestExRate.JF_CFXPercent = 7m;
			AssertEquals("Sell Rate should consider CFX", 0.349968m, TestExRate.JF_SellRate);

			var currency = Factory.NewWithValidTestData<RefCurrency>();
			currency.RX_Code = TestExRate.JF_RX_NKRateCurrency;
			TestExRate.RateCurrency.RX_IsExcludedCFXCalculation = true;
			Factory.Save();
			AssertEquals("Precondition: Currency is excluded from CFX calculation", true, TestExRate.RateCurrency.RX_IsExcludedCFXCalculation);
			AssertEquals("Sell Rate should not consider CFX", 0.327073m, TestExRate.JF_SellRate);
		}

		#endregion

		#region HasCHanges and IsSavedByFactory

		public void TestHasChanges_DependsOnRate()
		{
			TestExRate.JF_BaseRate = 1m;
			Assert("Pre-condition", TestExRate.HasChanges);

			TestExRate.JF_BaseRate = ZDecimal.Zero;
			Assert("For zero rate", !TestExRate.HasChanges);

			TestExRate.JF_BaseRate = 0.000001m;
			Assert("For greater than zero rate", TestExRate.HasChanges);

			TestExRate.JF_BaseRate = -0.000001m;
			Assert("For less than zero rate", TestExRate.HasChanges);

			TestExRate.JF_BaseRate = ZDecimal.Zero;
			TestExRate.Delete();
			Assert("For deleted zero rate", TestExRate.HasChanges);
		}

		public void TestIsSavedByFactory_DependsOnRate()
		{
			TestExRate.JF_BaseRate = 1m;
			Assert("Pre-condition", TestExRate.IsSavedByFactory);

			TestExRate.JF_BaseRate = ZDecimal.Zero;
			Assert("For zero rate", !TestExRate.IsSavedByFactory);

			TestExRate.JF_BaseRate = 0.000001m;
			Assert("For greater than zero rate", TestExRate.IsSavedByFactory);

			TestExRate.JF_BaseRate = -0.000001m;
			Assert("For less than zero rate", TestExRate.IsSavedByFactory);

			TestExRate.JF_BaseRate = ZDecimal.Zero;
			TestExRate.Delete();
			Assert("For deleted zero rate", TestExRate.IsSavedByFactory);
		}

		public void TestErrorIsReportedWhenDeletedExchangeRateRemainsInBusinessObjectCollection()
		{
			AssertErrorIsReportedWhenDeletedExchangeRateRemainsInBusinessObjectCollection(mastersAreDeleted: false);
		}

		public void TestErrorIsReportedWhenDeletedExchangeRateRemainsInBusinessObjectCollection_MasterDeleted()
		{
			AssertErrorIsReportedWhenDeletedExchangeRateRemainsInBusinessObjectCollection(mastersAreDeleted: true);
		}

		public void TestErrorIsReportedWhenDeletedByDateRefreshExchangeRateRemainsInBusinessObjectCollection()
		{
			AssertErrorIsReportedWhenDeletedExchangeRateRemainsInBusinessObjectCollection(mastersAreDeleted: false, deleteByDataRefresh: true);
		}

		public void TestErrorIsReportedWhenDeletedByDateRefreshExchangeRateRemainsInBusinessObjectCollection_MasterDeleted()
		{
			AssertErrorIsReportedWhenDeletedExchangeRateRemainsInBusinessObjectCollection(mastersAreDeleted: true, deleteByDataRefresh: true);
		}

		void AssertErrorIsReportedWhenDeletedExchangeRateRemainsInBusinessObjectCollection(bool mastersAreDeleted, bool deleteByDataRefresh = false)
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.ExchangeRatesCollectionRemoveMethodInfo);

			var rate = Factory.New<ExchangeRate>();
			var job = Factory.New<Job>();

			var collection1 = new MyDummyBizObjCollectionWithInternalOverrides(Factory);
			collection1.MastersAreDeleted = mastersAreDeleted;
			collection1.Add(rate);
			AssertEquals(1, collection1.Count);

			var collection2 = new ExchangeRatesCollection(job, Factory);
			collection2.Add(rate);
			AssertEquals(1, collection2.Count);

			AssertEquals(2, ((IBusinessObjectInternals)rate).ParentCollections.Length);

			var expectedKey = deleteByDataRefresh
				? "Business_Object_Collections_With_DeletedByDataRefresh_ExchangeRate"
				: "Business_Object_Collections_With_Deleted_ExchangeRate";

			ErrorReporter.Clear();

			if (deleteByDataRefresh)
			{
				collection1.SetIsRefreshingByDataRefreshBus();
				((IBusiness)rate).DeleteForDataRefresh();
			}
			else
			{
				rate.Delete();
			}

			Assert("Postcondition: Collection1 contains deleted charge", collection1.Contains(rate));
			Assert("Postcondition: Collection2 does not contain deleted charge", !collection2.Contains(rate));

			if (mastersAreDeleted)
			{
				AssertEquals("TotalErrorCount", 0, ErrorReporter.TotalErrorCount);
			}
			else
			{
				AssertEquals("TotalErrorCount", 1, ErrorReporter.TotalErrorCount);
				AssertEquals("Error must be reported", expectedKey, ErrorReporter.LastKeyReported);
				AssertContains($@"Deleted ExchangeRate:
	PK = {rate.PK}
	Type = ExchangeRate", ErrorReporter.LastMessageReported);
				AssertContains(@"Original property values for deleted and not previously saved BusinessObject are not accessible

ExchangeRatesCollectionRemoveMethodInfo:", ErrorReporter.LastMessageReported);
				AssertContains(@"Before Delete:
Parent collections:
BusinessObjectCollection Info:
	Collection Type = Enterprise.Accounting.Business.JobInvoicing.Testing.ExchangeRateTest+MyDummyBizObjCollectionWithInternalOverrides
	Element Type = CargoWise.EntityFramework.Testing.DummyBusinessObject", ErrorReporter.LastMessageReported);
				AssertContains(@"BusinessObjectCollection Info:
	Collection Type = Enterprise.Accounting.Business.JobInvoicing.ExchangeRatesCollection
	Element Type = Enterprise.Accounting.Business.JobInvoicing.ExchangeRate", ErrorReporter.LastMessageReported);
				AssertContains(@"After Delete:
Parent collections:
BusinessObjectCollection Info:
	Collection Type = Enterprise.Accounting.Business.JobInvoicing.Testing.ExchangeRateTest+MyDummyBizObjCollectionWithInternalOverrides
	Element Type = CargoWise.EntityFramework.Testing.DummyBusinessObject", ErrorReporter.LastMessageReported);
				AssertContains("Contains ? NotInCollection", ErrorReporter.LastMessageReported);

				if (deleteByDataRefresh)
				{
					AssertContains(@"Deleted via DataRefreshBus.
Data Refresh on the following collection(s):
Parent collections:
BusinessObjectCollection Info:
	Collection Type = Enterprise.Accounting.Business.JobInvoicing.Testing.ExchangeRateTest+MyDummyBizObjCollectionWithInternalOverrides", ErrorReporter.LastMessageReported);
					AssertContains(@"Business Contexts = None

BizObj Level Enterprise.Accounting.Business.JobInvoicing.ExchangeRatesCollection+Context : DeletedByDataRefresh", ErrorReporter.LastMessageReported);
				}
				else
				{
					AssertContains(@"Business Contexts = Factory Level : (DeletingExchangeRate)

None Enterprise.Accounting.Business.JobInvoicing.ExchangeRatesCollection+Context", ErrorReporter.LastMessageReported);
				}

				ErrorReporter.Clear();
			}
		}

		public void TestDeleteWhileAdding()
		{
			var collection = new ExchangeRatesCollection(TestJob, Factory);
			var rate = collection.AddNew();
			collection.Remove(rate);

			using (rate.SetTempContext(ExchangeRatesCollection.Context.AddingNewRate))
			{
				ErrorReporter.Clear();
				rate.Delete();
			}
			AssertEquals("TotalErrorCount", 1, ErrorReporter.TotalErrorCount);
			AssertEquals("Error must be reported", "DeletingExRateInTheProcessOfAddingIt", ErrorReporter.LastKeyReported);
			var expectMessage = $@"Deleted ExchangeRate:
	PK = {rate.PK}
	Type = ExchangeRate
	Types around row = ExchangeRate
	Factory Instance = {Factory._Instance}
	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = False
	HasChanges = False
	HasErrors = False
	IsDeleting = False

Business Contexts = None";
			AssertContains(expectMessage, ErrorReporter.LastMessageReported);
			AssertContains("BizObj Level Enterprise.Accounting.Business.JobInvoicing.ExchangeRatesCollection+Context : AddingNewRate", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		class MyDummyBizObjCollectionWithInternalOverrides : DummyBusinessObjectCollection, IBusinessObjectCollectionInternals
		{
			public MyDummyBizObjCollectionWithInternalOverrides(BusinessObjectFactory factory)
			: base(factory) { }

			bool IBusinessObjectCollectionInternals.MastersAreDeleted
			{
				get { return MastersAreDeleted; }
			}

			public override void Remove(BusinessObject elementToRemove)
			{
			}

			public bool MastersAreDeleted { get; set; }

			public void SetIsRefreshingByDataRefreshBus()
			{
				IsUpdatingByDataRefreshBus = true;
				Assert("IsRefreshingByDataRefreshBus", IsRefreshingByDataRefreshBus);
			}
		}

		#endregion

		#region UniqueIndexFailureHandler 

		public void TestUniqueIndexFailureHandler()
		{
			var notification = new NotificationHandlerForTest();

			// Start with generic ExchangeRate
			var rate = Factory.NewWithValidTestData<ExchangeRate>();
			rate.ParentJob.JH_JobNum = "S0001";
			rate.JF_RX_NKRateCurrency = TestObjectCreator.USD.Code;
			rate.JF_BaseRate = 0.85m;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var secondRate = newFactory.New<ExchangeRate>();
			secondRate.JF_JH = rate.JF_JH;
			secondRate.JF_RX_NKRateCurrency = rate.JF_RX_NKRateCurrency;
			secondRate.JF_BaseRate = 0.75m;

			var failureHandler = secondRate.UniqueIndexFailureHandler_ForTestOnly;
			AssertNotNull(failureHandler);

			AssertExceptionThrown("ZSaveException should be thrown", typeof(ZSaveException), () =>
			{
				newFactory.Save();
			});

			notification.Reset();
			failureHandler.NotifyUserAndAttemptToResolve(notification, failureHandler.HandledUniqueIndexNames.Single());
			AssertContains(@"While you were working, another user has created a duplicate Job Exchange Rate.
Job: S0001, currency: USD, organization type: , organization: , invoice currency type: .
Please cancel your changes and reload the form.", notification.Message);
			AssertContains("-Duplicate Job Exchange Rate", notification.Message);
			Assert("Error should be reported", notification.ReportErrorCount == 1 && notification.ReportInformationCount == 0);

			// Then Debtor generic ExchangeRate
			rate.OrgType = ExchangeRateOrgTypeEnum.Debtor;
			Factory.Save();

			secondRate.JF_OrgType = rate.JF_OrgType;

			AssertExceptionThrown("ZSaveException should be thrown", typeof(ZSaveException), () =>
			{
				newFactory.Save();
			});

			notification.Reset();
			failureHandler.NotifyUserAndAttemptToResolve(notification, failureHandler.HandledUniqueIndexNames.Single());
			AssertContains(@"While you were working, another user has created a duplicate Job Exchange Rate.
Job: S0001, currency: USD, organization type: DEB, organization: , invoice currency type: .
Please cancel your changes and reload the form.", notification.Message);
			AssertContains("-Duplicate Job Exchange Rate", notification.Message);
			Assert("Error should be reported", notification.ReportErrorCount == 1 && notification.ReportInformationCount == 0);

			// Org specific ExchangeRate
			rate.JF_OH_Org = TestObjectCreator.Debtor.PK;
			Factory.Save();

			secondRate.JF_OH_Org = rate.JF_OH_Org;

			AssertExceptionThrown("ZSaveException should be thrown", typeof(ZSaveException), () =>
			{
				newFactory.Save();
			});

			notification.Reset();
			failureHandler.NotifyUserAndAttemptToResolve(notification, failureHandler.HandledUniqueIndexNames.Single());
			AssertContains(@"While you were working, another user has created a duplicate Job Exchange Rate.
Job: S0001, currency: USD, organization type: DEB, organization: ZDEBTOR, invoice currency type: .
Please cancel your changes and reload the form.", notification.Message);
			AssertContains("-Duplicate Job Exchange Rate", notification.Message);
			Assert("Error should be reported", notification.ReportErrorCount == 1 && notification.ReportInformationCount == 0);

			// Try case when duplicate ExchangeRate was not found for some reason. THen we will use the current one for preparing the message
			rate.JF_OH_Org = ZGuid.Empty;
			Factory.Save();

			notification.Reset();
			failureHandler.NotifyUserAndAttemptToResolve(notification, failureHandler.HandledUniqueIndexNames.Single());
			AssertContains(@"While you were working, another user has created a duplicate Job Exchange Rate.
Job: S0001, currency: USD, organization type: DEB, organization: ZDEBTOR, invoice currency type: .
Please cancel your changes and reload the form.", notification.Message);
			AssertContains("-Duplicate Job Exchange Rate", notification.Message);
			Assert("Error should be reported", notification.ReportErrorCount == 1 && notification.ReportInformationCount == 0);

			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				rate.JF_OH_Org = TestObjectCreator.Debtor.PK;
				rate.OrgType = ExchangeRateOrgTypeEnum.Debtor;
				rate.JF_InvoiceCurrencyType = Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign;
				Factory.Save();

				secondRate.JF_OH_Org = rate.JF_OH_Org;
				secondRate.JF_OrgType = rate.JF_OrgType;
				secondRate.JF_InvoiceCurrencyType = rate.EffectiveInvoiceCurrencyType;

				AssertExceptionThrown("ZSaveException should be thrown", typeof(ZSaveException), () =>
				{
					newFactory.Save();
				});

				notification.Reset();
				failureHandler.NotifyUserAndAttemptToResolve(notification, failureHandler.HandledUniqueIndexNames.Single());
				AssertContains(@"While you were working, another user has created a duplicate Job Exchange Rate.
Job: S0001, currency: USD, organization type: DEB, organization: ZDEBTOR, invoice currency type: FOR.
Please cancel your changes and reload the form.", notification.Message);
				AssertContains("-Duplicate Job Exchange Rate", notification.Message);
				Assert("Error should be reported", notification.ReportErrorCount == 1 && notification.ReportInformationCount == 0);
			}
		}

		#endregion

		#region Implementation

		protected class TestingExchangeRate : ExchangeRate
		{
			public TestingExchangeRate(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			public override Job ParentJob => Factory.Load<TestingJob>(JF_JH);

			public void SetIsReciprocal(bool value)
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = value;
				Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).GC_IsReciprocal = value;
			}

			public ZDecimal GetExchangeRate(ZString currencyNK)
			{
				ZDecimal result = 0M;

				if (!currencyNK.IsEmpty)
				{
					result = Utilities.Round(Env.CurrentCompany.ExchangeRate.TodaysRate(currencyNK, ExchangeRateType.Buy), ExchangeRateDecimals);
				}

				return result;
			}
		}

		public class NotificationHandlerForTest : INotificationHandler
		{
			public NotificationHandlerForTest()
			{
				Message = string.Empty;
			}

			public string Message
			{
				get;
				private set;
			}

			public int ReportErrorCount
			{
				get;
				private set;
			}

			public int ReportInformationCount
			{
				get;
				private set;
			}

			public void Reset()
			{
				Message = string.Empty;
				ReportErrorCount = 0;
				ReportInformationCount = 0;
			}

			public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
			{
				ReportErrorCount++;
				Message += string.Format("ReportError={0}-{1}\r\n", message, caption);
			}
			public void ReportInformation(string message, string caption)
			{
				ReportInformationCount++;
				Message += string.Format("ReportInformation={0}-{1}\r\n", message, caption);
			}
		}

		protected TestingExchangeRate TestExRate;
		protected TestingJob TestJob;

		protected string TestString = "test";
		protected Guid TestGuid = Guid.NewGuid();
		protected ZString TestCode = "CCC";
		protected bool EventFired;

		protected override void SetUp()
		{
			base.SetUp();
			EventFired = false;

			TestJob = Factory.NewJobForTesting<TestingJob>();
			ExchangeRatesCollection collection = new ExchangeRatesCollection(TestJob, Factory);

			TestExRate = (TestingExchangeRate)GetNewBusinessObject();
			TestJob.ExchangeRates.Add(TestExRate);
			TestExRate.JF_RX_NKRateCurrency = "XXX";
			TestJob.JH_GB = GlbBranch.CurrentBranch.PK;
			TestJob.JH_JobNum = "CWG";
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		protected class TestingJob : Job
		{
			public TestingJob(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			internal bool IsRefreshChargeLinesExchangeRateBindingCalled { get; private set; }

			public override void RefreshChargeLinesExchangeRateBinding()
			{
				IsRefreshChargeLinesExchangeRateBindingCalled = true;
				base.RefreshChargeLinesExchangeRateBinding();
			}

			public void ResetIsCalledFlags()
			{
				IsRefreshChargeLinesExchangeRateBindingCalled = false;
			}

			public bool CFXOverrideEnabled { get; set; }

			internal override bool InvoicingAllowOverrideofCFX => CFXOverrideEnabled;
		}
		#endregion
	}
}
