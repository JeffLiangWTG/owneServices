using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(ExchangeRatesCollection))]
	public class ExchangeRatesCollection_InnerTest : BusinessObjectCollectionTestCase
	{
		public void TestIndexer()
		{
			var obj1 = TestJob.ExchangeRates.AddNew();
			AssertEquals("Index 0", obj1, TestJob.ExchangeRates[0]);

			var obj2 = TestJob.ExchangeRates.AddNew();
			AssertEquals("Index 1", obj2, TestJob.ExchangeRates[1]);
		}

		public override void TestAddNew()
		{
			base.TestAddNew();

			var obj = TestJob.ExchangeRates.AddNew();
			Assert("JF_JH.IsValid", obj.JF_JH.IsValid);
			AssertEquals("JF_JH", TestJob.PK, obj.JF_JH);
			Assert("JF_RX_NKRateCurrency", obj.JF_RX_NKRateCurrency.IsEmpty);
			Assert("JF_BaseRate", obj.JF_BaseRate.IsEmpty);
			Assert("JF_CFXPercent", obj.JF_CFXPercent.IsEmpty);
			Assert("JF_CFXMinimum", obj.JF_CFXMinimum.IsEmpty);
			Assert("JF_OrgType", obj.JF_OrgType.IsEmpty);
			Assert("JF_OH_Org", obj.JF_OH_Org.IsEmpty);
			Assert("JF_SellRate", obj.JF_SellRate.IsEmpty);
			Assert("JF_TodayRate", obj.JF_TodayRate.IsEmpty);
			Assert("JF_IsTransformed", obj.JF_IsTransformed);
		}

		#region AddRate

		public void TestAddRate()
		{
			foreach (var orgPK in new ZGuid[] { ZGuid.Empty, TestObjectCreator.ABIGAS.PK })
			{
				foreach (var orgType in new ExchangeRateOrgTypeEnum[] { ExchangeRateOrgTypeEnum.None, ExchangeRateOrgTypeEnum.Debtor, ExchangeRateOrgTypeEnum.Creditor })
				{
					AddAndAssertUsdRate(orgPK, orgType, ignoreExistingGenerics: false);
				}
			}
		}

		public void TestAddRate_IgnoreExistingGenrics()
		{
			foreach (var orgPK in new ZGuid[] { ZGuid.Empty, TestObjectCreator.ABIGAS.PK })
			{
				foreach (var orgType in new ExchangeRateOrgTypeEnum[] { ExchangeRateOrgTypeEnum.None, ExchangeRateOrgTypeEnum.Debtor, ExchangeRateOrgTypeEnum.Creditor })
				{
					var usdRate = AddAndAssertUsdRate(orgPK, orgType, ignoreExistingGenerics: true);

					if (orgPK.IsEmpty)  // Turn it into Generic Exchange Rate
					{
						usdRate.JF_IsTransformed = true;
					}
				}
			}
		}

		public void TestExchangeRateInvoiceCurrencyTypeWhenInvoiceCurrencyTypeIsNotUsedInAnyConfiguration()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = TestObjectCreator.CreateShipment("S0001000");
				var job = TestObjectCreator.CreateJob(shipment);
				Factory.Save();

				AssertEquals(0, job.ExchangeRates.Count);

				var charge = job.Charges.AddNew();
				charge.JR_AC = TestObjectCreator.CC1.PK;
				charge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
				charge.JR_LocalSellAmt = 100M;
				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;

				AssertEquals(1, job.ExchangeRates.Count);
				AssertEquals(TestObjectCreator.USD.RX_Code, job.ExchangeRates[0].JF_RX_NKRateCurrency);
				AssertEquals(InvoiceCurrencyType.Foreign, charge.InvoiceCurrencyTypeForAR);
				AssertEquals("we keep the job charge invoice currency type even if the config used has an empty invoice currency type",
					Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, job.ExchangeRates[0].EffectiveInvoiceCurrencyType);
			}
		}

		[TestDate(2023, 01, 16)]
		public void TestExchangeRateUseTheCorrectInvoiceTypeCurrencyConfiguration()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CreateRefExRate(TestObjectCreator.EUR.RX_Code, new ZDateTime(2023, 1, 1), new ZDateTime(2023, 1, 7), 1.5m);
				CreateRefExRate(TestObjectCreator.EUR.RX_Code, new ZDateTime(2023, 1, 8), new ZDateTime(2023, 1, 15), 2m);

				CreateAccExchangeRateConfiguration(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, -10);
				CreateAccExchangeRateConfiguration(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, -2);

				var shipment = TestObjectCreator.CreateShipment("S0001000");
				var job = TestObjectCreator.CreateJob(shipment);
				Factory.Save();

				var charge = job.Charges.AddNew();
				charge.JR_AC = TestObjectCreator.CC1.PK;
				charge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
				charge.JR_LocalSellAmt = 100M;
				charge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
				AssertEquals(0, job.ExchangeRates.Count);
				AssertEquals(InvoiceTypesList.Codes.FinalInvoice, charge.JR_InvoiceType);

				charge.JR_RX_NKSellCurrency = TestObjectCreator.EUR.RX_Code;
				AssertExchangeRate(true);
				ResetJobExchangeRates();

				charge.JR_RX_NKSellCurrency = TestObjectCreator.EUR.RX_Code;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.SelfBillingInvoice;
				AssertExchangeRate(false);
				ResetJobExchangeRates();

				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;
				AssertExchangeRate(false);

				void AssertExchangeRate(bool isLocalInvoiceCurrencyType)
				{
					AssertEquals(isLocalInvoiceCurrencyType, charge.IsInLocalInvoiceCurrencyForPosting(ExchangeRateValidLedgerEnum.AR));
					AssertEquals(1, job.ExchangeRates.Count);
					AssertEquals(TestObjectCreator.EUR.RX_Code, job.ExchangeRates[0].JF_RX_NKRateCurrency);

					if (isLocalInvoiceCurrencyType)
					{
						AssertEquals(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, job.ExchangeRates[0].EffectiveInvoiceCurrencyType);
						AssertEquals(1.5m, job.ExchangeRates[0].JF_BaseRate);
					}
					else
					{
						AssertEquals(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, job.ExchangeRates[0].EffectiveInvoiceCurrencyType);
						AssertEquals(2m, job.ExchangeRates[0].JF_BaseRate);
					}
				}

				void ResetJobExchangeRates()
				{
					charge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
					charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
					charge.JR_RX_NKSellInvoiceCurrency = string.Empty;
					AssertEquals(0, job.ExchangeRates.Count);
				}
			}
		}

		[TestDate(2023, 01, 16)]
		public void TestOverrideExchangeRateWithInvoiceCurrencyType()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CreateRefExRate(TestObjectCreator.EUR.RX_Code, new ZDateTime(2023, 1, 1), new ZDateTime(2023, 1, 7), 1.5m);
				CreateRefExRate(TestObjectCreator.EUR.RX_Code, new ZDateTime(2023, 1, 8), new ZDateTime(2023, 1, 15), 2m);
				CreateRefExRate(TestObjectCreator.EUR.RX_Code, new ZDateTime(2023, 1, 16), new ZDateTime(2023, 1, 16), 2.5m);

				CreateAccExchangeRateConfiguration(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, -10);
				CreateAccExchangeRateConfiguration(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, -2);

				var shipment = TestObjectCreator.CreateShipment("S0001000");
				var job = TestObjectCreator.CreateJob(shipment);
				Factory.Save();

				var jobExRate1 = job.ExchangeRates.AddNew();
				jobExRate1.JF_RX_NKRateCurrency = TestObjectCreator.EUR.RX_Code;
				AssertEquals(1, job.ExchangeRates.Count);
				AssertEquals(string.Empty, job.ExchangeRates[0].EffectiveInvoiceCurrencyType);
				AssertEquals("use ex rate from config with empty invoice currency type", 2.5m, job.ExchangeRates[0].JF_BaseRate);

				job.ExchangeRates[0].JF_InvoiceCurrencyType = Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local;
				AssertEquals("use ex rate from config with local invoice currency type", 1.5m, job.ExchangeRates[0].JF_BaseRate);

				job.ExchangeRates[0].JF_InvoiceCurrencyType = Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign;
				AssertEquals("use ex rate from config with foreign invoice currency type", 2m, job.ExchangeRates[0].JF_BaseRate);
			}
		}

		[TestDate(2023, 01, 16)]
		public void TestAddNewChargesWithExistingJobExRateWithEmptyInvoiceCurrencyType()
		{
			CreateRefExRate(TestObjectCreator.EUR.RX_Code, new ZDateTime(2023, 1, 1), new ZDateTime(2023, 1, 7), 1.5m);
			CreateRefExRate(TestObjectCreator.EUR.RX_Code, new ZDateTime(2023, 1, 8), new ZDateTime(2023, 1, 15), 2m);
			CreateRefExRate(TestObjectCreator.EUR.RX_Code, new ZDateTime(2023, 1, 16), new ZDateTime(2023, 1, 16), 2.5m);

			var shipment = TestObjectCreator.CreateShipment("S0001000");
			var job = TestObjectCreator.CreateJob(shipment);

			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var charge1 = job.Charges.AddNew();
				charge1.JR_AC = TestObjectCreator.CC1.PK;
				charge1.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
				charge1.JR_LocalSellAmt = 100M;
				charge1.JR_RX_NKSellCurrency = TestObjectCreator.EUR.RX_Code;

				AssertEquals(1, job.ExchangeRates.Count);
				AssertEquals(TestObjectCreator.EUR.RX_Code, job.ExchangeRates[0].JF_RX_NKRateCurrency);
				AssertEquals(string.Empty, job.ExchangeRates[0].EffectiveInvoiceCurrencyType);
				Assert(!job.ExchangeRates[0].JF_IsTransformed);
				AssertEquals(2.5m, job.ExchangeRates[0].JF_BaseRate);
				AssertEquals(2.5m, charge1.JR_OSSellExRate);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CreateAccExchangeRateConfiguration(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, -10);
				CreateAccExchangeRateConfiguration(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, -2);
				Factory.Save();

				TestObjectCreator.Debtor.CompanyData.AccARExchangeRateConfigurations.Load(); //reload configs

				var charge2 = job.Charges.AddNew();
				charge2.JR_AC = TestObjectCreator.CC1.PK;
				charge2.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
				charge2.JR_LocalSellAmt = 100M;
				charge2.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;

				AssertEquals("it doesn't create a new row, it is using the existing row with empty invoice currency type", 1, job.ExchangeRates.Count);
				AssertEquals(TestObjectCreator.EUR.RX_Code, job.ExchangeRates[0].JF_RX_NKRateCurrency);
				AssertEquals(string.Empty, job.ExchangeRates[0].EffectiveInvoiceCurrencyType);
				AssertEquals(2.5m, job.ExchangeRates[0].JF_BaseRate);

				var charge3 = job.Charges.AddNew();
				charge3.JR_AC = TestObjectCreator.CC1.PK;
				charge3.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
				charge3.JR_LocalSellAmt = 100M;
				charge3.JR_RX_NKSellCurrency = TestObjectCreator.EUR.RX_Code;

				AssertEquals("it doesn't create a new row, it is using the existing row with empty invoice currency type", 1, job.ExchangeRates.Count);
				AssertEquals(TestObjectCreator.EUR.RX_Code, job.ExchangeRates[0].JF_RX_NKRateCurrency);
				AssertEquals(string.Empty, job.ExchangeRates[0].EffectiveInvoiceCurrencyType);
				AssertEquals(2.5m, job.ExchangeRates[0].JF_BaseRate);
			}
		}

		[TestDate(2023, 01, 16)]
		public void TestAddNewChargesWithExistingJobExRateWithInvoiceCurrencyType()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CreateRefExRate(TestObjectCreator.EUR.RX_Code, new ZDateTime(2023, 1, 1), new ZDateTime(2023, 1, 7), 1.5m);
				CreateRefExRate(TestObjectCreator.EUR.RX_Code, new ZDateTime(2023, 1, 8), new ZDateTime(2023, 1, 15), 2m);
				CreateRefExRate(TestObjectCreator.EUR.RX_Code, new ZDateTime(2023, 1, 16), new ZDateTime(2023, 1, 16), 2.5m);

				var shipment = TestObjectCreator.CreateShipment("S0001000");
				var job = TestObjectCreator.CreateJob(shipment);

				CreateAccExchangeRateConfiguration(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, -10);
				CreateAccExchangeRateConfiguration(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, -2);
				Factory.Save();

				var charge1 = job.Charges.AddNew();
				charge1.JR_AC = TestObjectCreator.CC1.PK;
				charge1.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
				charge1.JR_LocalSellAmt = 100M;
				charge1.JR_RX_NKSellCurrency = TestObjectCreator.EUR.RX_Code;

				var charge2 = job.Charges.AddNew();
				charge2.JR_AC = TestObjectCreator.CC1.PK;
				charge2.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
				charge2.JR_LocalSellAmt = 100M;
				charge2.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;

				AssertEquals(1.5m, charge1.JR_OSSellExRate);
				AssertEquals(2m, charge2.JR_OSSellInvoiceExRate);
				AssertJobExRates();

				var charge3 = job.Charges.AddNew();
				charge3.JR_AC = TestObjectCreator.CC1.PK;
				charge3.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
				charge3.JR_LocalSellAmt = 100M;
				charge3.JR_RX_NKSellCurrency = TestObjectCreator.EUR.RX_Code;

				var charge4 = job.Charges.AddNew();
				charge4.JR_AC = TestObjectCreator.CC1.PK;
				charge4.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
				charge4.JR_LocalSellAmt = 100M;
				charge4.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;

				AssertEquals(1.5m, charge3.JR_OSSellExRate);
				AssertEquals(2m, charge4.JR_OSSellInvoiceExRate);
				AssertJobExRates();

				void AssertJobExRates()
				{
					AssertEquals(2, job.ExchangeRates.Count);
					Assert(job.ExchangeRates.Cast<ExchangeRate>().All(x => x.JF_RX_NKRateCurrency == TestObjectCreator.EUR.RX_Code));
					Assert(job.ExchangeRates.Cast<ExchangeRate>().All(x => x.OrgType == ExchangeRateOrgTypeEnum.Debtor));
					Assert(job.ExchangeRates.Cast<ExchangeRate>().All(x => !x.JF_IsTransformed));

					AssertContainsExactElementsInAnyOrder(new[] { Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local },
						job.ExchangeRates.Cast<ExchangeRate>().Select(x => x.EffectiveInvoiceCurrencyType));

					AssertEquals(2m, job.ExchangeRates.Cast<ExchangeRate>().First(x => x.EffectiveInvoiceCurrencyType == Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign).JF_BaseRate);
					AssertEquals(1.5m, job.ExchangeRates.Cast<ExchangeRate>().First(x => x.EffectiveInvoiceCurrencyType == Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local).JF_BaseRate);
				}
			}
		}

		[TestDate(2023, 01, 16)]
		public void TestMixInvoiceCurrencyType()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CreateRefExRate(TestObjectCreator.EUR.RX_Code, new ZDateTime(2023, 1, 1), new ZDateTime(2023, 1, 7), 1.5m);
				CreateRefExRate(TestObjectCreator.EUR.RX_Code, new ZDateTime(2023, 1, 8), new ZDateTime(2023, 1, 15), 2m);
				CreateRefExRate(TestObjectCreator.EUR.RX_Code, new ZDateTime(2023, 1, 16), new ZDateTime(2023, 1, 16), 2.5m);

				var shipment = TestObjectCreator.CreateShipment("S0001000");
				var job = TestObjectCreator.CreateJob(shipment);

				CreateAccExchangeRateConfiguration(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, -10);
				CreateAccExchangeRateConfiguration(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, -2);

				Factory.Save();

				var jobExRate1 = job.ExchangeRates.AddNew();
				jobExRate1.JF_RX_NKRateCurrency = TestObjectCreator.EUR.RX_Code;
				jobExRate1.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();

				var jobExRate2 = job.ExchangeRates.AddNew();
				jobExRate2.JF_RX_NKRateCurrency = TestObjectCreator.EUR.RX_Code;
				jobExRate2.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();
				jobExRate2.JF_InvoiceCurrencyType = Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign;

				var jobExRate3 = job.ExchangeRates.AddNew();
				jobExRate3.JF_RX_NKRateCurrency = TestObjectCreator.EUR.RX_Code;
				jobExRate3.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();
				jobExRate3.JF_InvoiceCurrencyType = Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local;

				AssertJobExRates();

				var charge1 = job.Charges.AddNew();
				charge1.JR_AC = TestObjectCreator.CC1.PK;
				charge1.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
				charge1.JR_LocalSellAmt = 100M;
				charge1.JR_RX_NKSellCurrency = TestObjectCreator.EUR.RX_Code;

				var charge2 = job.Charges.AddNew();
				charge2.JR_AC = TestObjectCreator.CC1.PK;
				charge2.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
				charge2.JR_LocalSellAmt = 100M;
				charge2.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;

				AssertEquals(1.5m, charge1.JR_OSSellExRate);
				AssertEquals(2m, charge2.JR_OSSellInvoiceExRate);
				AssertJobExRates();

				void AssertJobExRates()
				{
					AssertEquals(3, job.ExchangeRates.Count);
					Assert(job.ExchangeRates.Cast<ExchangeRate>().All(x => x.JF_RX_NKRateCurrency == TestObjectCreator.EUR.RX_Code));
					Assert(job.ExchangeRates.Cast<ExchangeRate>().All(x => x.OrgType == ExchangeRateOrgTypeEnum.Debtor));
					Assert(job.ExchangeRates.Cast<ExchangeRate>().All(x => x.JF_IsTransformed));

					AssertContainsExactElementsInAnyOrder(new[] { string.Empty, Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local },
						job.ExchangeRates.Cast<ExchangeRate>().Select(x => x.EffectiveInvoiceCurrencyType));

					AssertEquals(2.5m, job.ExchangeRates.Cast<ExchangeRate>().First(x => x.EffectiveInvoiceCurrencyType == string.Empty).JF_BaseRate);
					AssertEquals(2m, job.ExchangeRates.Cast<ExchangeRate>().First(x => x.EffectiveInvoiceCurrencyType == Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign).JF_BaseRate);
					AssertEquals(1.5m, job.ExchangeRates.Cast<ExchangeRate>().First(x => x.EffectiveInvoiceCurrencyType == Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local).JF_BaseRate);
				}
			}
		}

		[TestDate(2023, 01, 16)]
		public void TestUpdateExRate()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CreateRefExRate(TestObjectCreator.EUR.RX_Code, new ZDateTime(2023, 1, 1), new ZDateTime(2023, 1, 7), 1.5m);
				CreateRefExRate(TestObjectCreator.EUR.RX_Code, new ZDateTime(2023, 1, 8), new ZDateTime(2023, 1, 15), 2m);
				CreateRefExRate(TestObjectCreator.EUR.RX_Code, new ZDateTime(2023, 1, 16), new ZDateTime(2023, 1, 16), 2.5m);

				var shipment = TestObjectCreator.CreateShipment("S0001000");
				var job = TestObjectCreator.CreateJob(shipment);

				CreateAccExchangeRateConfiguration(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, -10);
				CreateAccExchangeRateConfiguration(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, -2);
				Factory.Save();

				var charge1 = job.Charges.AddNew();
				charge1.JR_AC = TestObjectCreator.CC1.PK;
				charge1.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
				charge1.JR_LocalSellAmt = 100M;
				charge1.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;

				var charge2 = job.Charges.AddNew();
				charge2.JR_AC = TestObjectCreator.CC1.PK;
				charge2.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
				charge2.JR_LocalSellAmt = 100M;
				charge2.JR_RX_NKSellCurrency = TestObjectCreator.EUR.RX_Code;

				AssertEquals(2, job.ExchangeRates.Count);
				Assert(job.ExchangeRates.Cast<ExchangeRate>().All(x => x.JF_RX_NKRateCurrency == TestObjectCreator.EUR.RX_Code));
				AssertContainsExactElementsInAnyOrder(new[] { Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local },
					job.ExchangeRates.Cast<ExchangeRate>().Select(x => x.EffectiveInvoiceCurrencyType));

				var foreignJobExRate = job.ExchangeRates.Cast<ExchangeRate>().First(x => x.EffectiveInvoiceCurrencyType == Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign);
				var localJobExRate = job.ExchangeRates.Cast<ExchangeRate>().First(x => x.EffectiveInvoiceCurrencyType == Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local);

				AssertEquals(2m, foreignJobExRate.JF_BaseRate);
				AssertEquals(1.5m, localJobExRate.JF_BaseRate);
				AssertEquals(2m, charge1.JR_OSSellInvoiceExRate);
				AssertEquals(1.5m, charge2.JR_OSSellExRate);

				foreignJobExRate.JF_BaseRate = 2.1m;
				localJobExRate.JF_BaseRate = 1.6m;
				AssertEquals(2.1m, charge1.JR_OSSellInvoiceExRate);
				AssertEquals(1.6m, charge2.JR_OSSellExRate);
			}
		}

		[TestDate(2023, 01, 16)]
		public void TestInvoiceCurrencyTypeIsBlankWhenChargeIsMissingInfo()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CreateRefExRate(TestObjectCreator.EUR.RX_Code, new ZDateTime(2023, 1, 1), new ZDateTime(2023, 1, 7), 1.5m);
				CreateRefExRate(TestObjectCreator.EUR.RX_Code, new ZDateTime(2023, 1, 8), new ZDateTime(2023, 1, 15), 2m);
				CreateRefExRate(TestObjectCreator.EUR.RX_Code, new ZDateTime(2023, 1, 16), new ZDateTime(2023, 1, 16), 2.5m);

				var shipment = TestObjectCreator.CreateShipment("S0001000");
				var job = TestObjectCreator.CreateJob(shipment);

				CreateAccExchangeRateConfiguration(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, -10);
				CreateAccExchangeRateConfiguration(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, -2);
				Factory.Save();

				var charge = job.Charges.AddNew();
				charge.JR_AC = TestObjectCreator.CC1.PK;
				charge.JR_OH_SellAccount = ZGuid.Empty;
				charge.JR_LocalSellAmt = 100M;
				charge.JR_RX_NKSellCurrency = TestObjectCreator.EUR.RX_Code;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

				AssertEquals(InvoiceCurrencyType.NotApplicable, charge.InvoiceCurrencyTypeForAR);

				AssertEquals(1, job.ExchangeRates.Count);
				AssertEquals(ExchangeRateOrgTypeEnum.Debtor.ToCode(), job.ExchangeRates.Cast<ExchangeRate>().First().JF_OrgType);
				AssertEquals(TestObjectCreator.EUR.RX_Code, job.ExchangeRates.Cast<ExchangeRate>().First().JF_RX_NKRateCurrency);
				AssertEquals(string.Empty, job.ExchangeRates.Cast<ExchangeRate>().First().EffectiveInvoiceCurrencyType);
			}
		}

		void CreateRefExRate(string currency, ZDateTime startDate, ZDateTime expiryDate, ZDecimal exRate)
		{
			var newExRate = Factory.NewWithValidTestData<RefExchangeRate>();
			newExRate.RE_GC = GlbCompany.CurrentCompany.PK;
			newExRate.RE_RX_NKExCurrency = currency;
			newExRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			newExRate.RE_StartDate = startDate;
			newExRate.RE_ExpiryDate = expiryDate;
			newExRate.RE_SellRate = exRate;
		}

		void CreateAccExchangeRateConfiguration(string invoiceCurrencyType, int offSet)
		{
			var exRateConfig = Factory.NewWithValidTestData<AccExchangeRateConfiguration>();
			exRateConfig.JCE_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			exRateConfig.JCE_InvoiceCurrencyType = invoiceCurrencyType;
			exRateConfig.JCE_Offset = offSet;

			AssertEquals(Core.Constants.JobBillingExchangeRatePreference.Code.TodaysRate, exRateConfig.JCE_Preference);
		}

		[TestDate(2023, 01, 16)]
		public void TestJF_RX_NKRateCurrencyUseTheConfigInvoiceCurrencyType()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CreateRefExRate(TestObjectCreator.USD.RX_Code, new ZDateTime(2023, 1, 1), new ZDateTime(2023, 1, 15), 1.5m);
				CreateAccExchangeRateConfiguration(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, 0);
				CreateAccExchangeRateConfiguration(string.Empty, -2);
				Factory.Save();

				var charge = CreateForeignJobCharge("S0001001");
				var job = charge.Job as Job;

				AssertEquals(1, job.ExchangeRates.Count);
				AssertEquals(TestObjectCreator.USD.RX_Code, job.ExchangeRates[0].JF_RX_NKRateCurrency);
				AssertEquals("if the config with FOR invoice currency type doesn't have an exRate then we fallback to config with empty invoice currency type but we keep FOR invoice currency type in the job exRate row",
					Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, job.ExchangeRates[0].EffectiveInvoiceCurrencyType);
				AssertEquals(1.5m, job.ExchangeRates[0].JF_BaseRate);

				CreateRefExRate(TestObjectCreator.USD.RX_Code, new ZDateTime(2023, 1, 16), new ZDateTime(2023, 1, 31), 2m);

				charge = CreateForeignJobCharge("S0001002");
				job = charge.Job as Job;

				AssertEquals(1, job.ExchangeRates.Count);
				AssertEquals(TestObjectCreator.USD.RX_Code, job.ExchangeRates[0].JF_RX_NKRateCurrency);
				AssertEquals(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, job.ExchangeRates[0].EffectiveInvoiceCurrencyType);
				AssertEquals(2m, job.ExchangeRates[0].JF_BaseRate);

				Charge CreateForeignJobCharge(string shipmentNum)
				{
					var newShipment = TestObjectCreator.CreateShipment(shipmentNum);
					var newJob = TestObjectCreator.CreateJob(newShipment);
					Factory.Save();

					var newCharge = newJob.Charges.AddNew();
					newCharge.JR_AC = TestObjectCreator.CC1.PK;
					newCharge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
					newCharge.JR_LocalSellAmt = 100M;
					newCharge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;

					return newCharge;
				}
			}
		}

		public void TestAddRateWithExistingGenerics()
		{
			var genericRate = AddAndAssertUsdRate(ZGuid.Empty, ExchangeRateOrgTypeEnum.None);
			genericRate.JF_IsTransformed = true;    // It is necessary to set it to make Ex Rate generic

			foreach (var orgPK in new ZGuid[] { ZGuid.Empty, TestObjectCreator.ABIGAS.PK })
			{
				foreach (var orgType in new ExchangeRateOrgTypeEnum[] { ExchangeRateOrgTypeEnum.None, ExchangeRateOrgTypeEnum.Debtor, ExchangeRateOrgTypeEnum.Creditor })
				{
					var orgMessage = $"OrgType: {orgType.ToCode()}, Org: {(orgPK.IsEmpty ? ZString.Empty : TestObjectCreator.ABIGAS.OH_Code)}, Ignore existing Generics: false ";

					var rate = TestJob.ExchangeRates.AddRate(USD, 0.78m, ZGuid.Empty, orgType, ignoreExistingGenerics: false);
					AssertNotNull(orgMessage + " should return a rate", rate);
					AssertEquals(orgMessage + " should be generic rate", genericRate.PK, rate.PK);
					AssertGenericExchangRate(rate, string.Empty, orgMessage);
				}
			}

			foreach (var orgType in new ExchangeRateOrgTypeEnum[] { ExchangeRateOrgTypeEnum.Debtor, ExchangeRateOrgTypeEnum.Creditor })
			{
				var orgTypeGenericRate = AddAndAssertUsdRate(ZGuid.Empty, orgType, ignoreExistingGenerics: true);
				AssertNotNull(orgTypeGenericRate);
				AssertNotEquals($"Org Type {orgType.ToCode()} new rate", genericRate.PK, orgTypeGenericRate.PK);
				orgTypeGenericRate.JF_IsTransformed = true;    // It is necessary to set it to make Ex Rate generic

				var orgMessage = $"OrgType: {orgType.ToCode()}, Org: {TestObjectCreator.ABIGAS.OH_Code}, Ignore existing Generics: false ";

				var rate = TestJob.ExchangeRates.AddRate(USD, 0.79m, TestObjectCreator.ABIGAS.PK, orgType, ignoreExistingGenerics: false);
				AssertNotNull(orgMessage + " should return a rate", rate);
				AssertEquals(orgMessage + " should be org type generic rate", orgTypeGenericRate.PK, rate.PK);
				AssertGenericExchangRate(rate, orgType.ToCode(), orgMessage);
			}
		}

		[TestDate(2025, 1, 8)]
		public void TestAddRateDoesWithExistingSystemExRateAndCfxConfiguration()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			var newFactory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(newFactory);
			creator.CreateExchangeRate(creator.USD, "BUY", 0.8833m, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			newFactory.Save();

			var shipment = TestObjectCreator.CreateShipment(TestJob.JH_JobNum);
			TestJob.Parent = shipment;
			TestObjectCreator.CreateCFXUplift(TestObjectCreator.ABIGAS.CompanyData.AccCFXConfigurations, jobType: "SHP", percentage: 4m, minimum: 2m);
			TestObjectCreator.CreateCFXUplift(TestObjectCreator.ABIGAS.CompanyData.AccCFXConfigurations, jobType: "SHP", percentage: 5m, minimum: 3m, startDate: new ZDate(2025, 1, 6), expiryDate: new ZDate(2025, 1, 9));

			var usdRate = TestJob.ExchangeRates.AddRate(USD, decimal.Zero, TestObjectCreator.ABIGAS.PK, ExchangeRateOrgTypeEnum.Debtor);
			AssertNotNull(usdRate);
			AssertEquals("JF_BaseRate", 0.8833m, usdRate.JF_BaseRate);
			AssertEquals("JF_CFXPercent", 5m, usdRate.JF_CFXPercent);
			AssertEquals("JF_CFXMinimum", 3m, usdRate.JF_CFXMinimum);
			AssertEquals("JF_SellRate", 0.839135m, usdRate.JF_SellRate);
			AssertEquals("JF_TodayRate", 0.8833m, usdRate.JF_TodayRate);
			AssertEquals("JF_IsTransformed", false, usdRate.JF_IsTransformed);

			foreach (var orgType in new ExchangeRateOrgTypeEnum[] { ExchangeRateOrgTypeEnum.Creditor, ExchangeRateOrgTypeEnum.None })
			{
				var orgMessage = $"OrgType: {orgType.ToCode()} ";

				usdRate = TestJob.ExchangeRates.AddRate(USD, decimal.Zero, TestObjectCreator.ABIGAS.PK, orgType);
				AssertNotNull(usdRate);
				AssertEquals(orgMessage + "JF_OrgType", orgType.ToCode(), usdRate.JF_OrgType);
				AssertEquals(orgMessage + "JF_BaseRate", 0.8833m, usdRate.JF_BaseRate);
				AssertEquals(orgMessage + "JF_CFXPercent", 0m, usdRate.JF_CFXPercent);
				AssertEquals(orgMessage + "JF_CFXMinimum", 0m, usdRate.JF_CFXMinimum);
				AssertEquals(orgMessage + "JF_SellRate", 0.8833m, usdRate.JF_SellRate);
				AssertEquals(orgMessage + "JF_TodayRate", 0.8833m, usdRate.JF_TodayRate);
				AssertEquals(orgMessage + "JF_IsTransformed", false, usdRate.JF_IsTransformed);
			}
		}

		public void TestAddRateSuspendsHasChagesWhenSuspendedOnJob()
		{
			using (TestJob.SuspendSettingHasChanges())
			{
				AddAndAssertUsdRate(ZGuid.Empty, ExchangeRateOrgTypeEnum.None, expectedHasChanges: false);
			}
		}

		public void TestRefreshesExchangeRatesOnDataRefreshBusUpdate()
		{
			var charge1 = TestObjectCreator.CreateCharge(TestJob, TestObjectCreator.FRT);
			charge1.JR_RX_NKCostCurrency = AUD.RX_Code;
			charge1.JR_OSCostAmt = 10;

			Factory.Save();

			AssertEquals(1, TestJob.Charges.Count);
			AssertEquals(0, TestJob.ExchangeRates.Count);

			var factory2 = new BusinessObjectFactory();

			var jobCopy = factory2.Load<Job>(TestJob.PK);
			var charge2 = TestObjectCreator.CreateCharge(jobCopy, TestObjectCreator.FRT);
			charge2.JR_RX_NKCostCurrency = USD.RX_Code;
			charge2.JR_OSCostAmt = 20;

			AssertNotNull(charge2.CostExchangeRate);

			charge2.CostExchangeRate.SetBuyRate_ForTestOnly(3m);

			AssertEquals(1, TestJob.Charges.Count);
			AssertEquals(0, TestJob.ExchangeRates.Count);

			factory2.Save();

			AssertEquals("New charge should be added to the Test Job after data refresh bus update", 2, TestJob.Charges.Count);
			AssertEquals("There is only one exchange rate after update", 1, TestJob.ExchangeRates.Count);
			AssertEquals("The exchange rate should come from data refresh bus update", 3m, TestJob.ExchangeRates.Cast<ExchangeRate>().FirstOrDefault().JF_BaseRate);
		}

		ExchangeRate AddAndAssertUsdRate(ZGuid orgPK, ExchangeRateOrgTypeEnum orgType, bool ignoreExistingGenerics = false, bool expectedHasChanges = true)
		{
			var orgMessage = $"OrgType: {orgType.ToCode()}, Org: {(orgPK.IsEmpty ? ZString.Empty : TestObjectCreator.ABIGAS.OH_Code)}, Ignore existing Generics: {ignoreExistingGenerics} ";

			AssertNull(orgMessage + "No ex Rate for Local Currency", TestJob.ExchangeRates.AddRate(TestJob.Company.LocalCurrency, 1m, orgPK, orgType, ignoreExistingGenerics: ignoreExistingGenerics));

			var usdRate = TestJob.ExchangeRates.AddRate(USD, 0.75m, orgPK, orgType, ignoreExistingGenerics: ignoreExistingGenerics);
			AssertNotNull(orgMessage, usdRate);
			AssertEquals(orgMessage + "HasChanges", expectedHasChanges, usdRate.HasChanges);
			AssertEquals(orgMessage + "JF_JH", TestJob.PK, usdRate.JF_JH);
			AssertEquals(orgMessage + "JF_RX_NKRateCurrency", "USD", usdRate.JF_RX_NKRateCurrency);
			AssertEquals(orgMessage + "JF_BaseRate", 0.75m, usdRate.JF_BaseRate);
			AssertEquals(orgMessage + "JF_CFXPercent", 0m, usdRate.JF_CFXPercent);
			AssertEquals(orgMessage + "JF_CFXMinimum", 0m, usdRate.JF_CFXMinimum);
			AssertEquals(orgMessage + "JF_OrgType", orgType.ToCode(), usdRate.JF_OrgType);
			AssertEquals(orgMessage + "JF_OH_Org", orgPK, usdRate.JF_OH_Org);
			AssertEquals(orgMessage + "JF_SellRate", 0.75m, usdRate.JF_SellRate);
			AssertEquals(orgMessage + "JF_TodayRate", 0m, usdRate.JF_TodayRate);
			AssertEquals(orgMessage + "JF_IsTransformed", false, usdRate.JF_IsTransformed);

			return usdRate;
		}

		public void AssertGenericExchangRate(ExchangeRate rate, string orgType, string orgMessage)
		{
			AssertEquals(orgMessage + "JF_JH", TestJob.PK, rate.JF_JH);
			AssertEquals(orgMessage + "JF_RX_NKRateCurrency", "USD", rate.JF_RX_NKRateCurrency);
			AssertEquals(orgMessage + "JF_BaseRate", 0.75m, rate.JF_BaseRate);
			AssertEquals(orgMessage + "JF_CFXPercent", 0m, rate.JF_CFXPercent);
			AssertEquals(orgMessage + "JF_CFXMinimum", 0m, rate.JF_CFXMinimum);
			AssertEquals(orgMessage + "JF_OrgType", orgType, rate.JF_OrgType);
			AssertEquals(orgMessage + "JF_OH_Org", ZGuid.Empty, rate.JF_OH_Org);
			AssertEquals(orgMessage + "JF_SellRate", 0.75m, rate.JF_SellRate);
			AssertEquals(orgMessage + "JF_TodayRate", 0m, rate.JF_TodayRate);
			AssertEquals(orgMessage + "JF_IsTransformed", true, rate.JF_IsTransformed);
		}

		#endregion

		#region GetExchangeRate

		public void TestGetExchangeRate()
		{
			AssertEquals(0, TestJob.ExchangeRates.Count);
			var org = TestObjectCreator.ABIGAS;

			var genericRate = AddAndAssertUsdRate(ZGuid.Empty, ExchangeRateOrgTypeEnum.None);
			var genericDebtorRate = AddAndAssertUsdRate(ZGuid.Empty, ExchangeRateOrgTypeEnum.Debtor);
			var genericCreditorRate = AddAndAssertUsdRate(ZGuid.Empty, ExchangeRateOrgTypeEnum.Creditor);

			var genericOrgRate = AddAndAssertUsdRate(org.PK, ExchangeRateOrgTypeEnum.None);
			var debtorRate = AddAndAssertUsdRate(org.PK, ExchangeRateOrgTypeEnum.Debtor);
			var creditorRate = AddAndAssertUsdRate(org.PK, ExchangeRateOrgTypeEnum.Creditor);

			foreach (var orgPK in new ZGuid[] { ZGuid.Empty, org.PK, TestObjectCreator.Debtor.PK })
			{
				foreach (var orgType in new ExchangeRateOrgTypeEnum[] { ExchangeRateOrgTypeEnum.None, ExchangeRateOrgTypeEnum.Debtor, ExchangeRateOrgTypeEnum.Creditor })
				{
					AssertNull(TestJob.ExchangeRates.GetExchangeRate(AUD.RX_Code, orgPK, orgType));
					AssertNull(TestJob.ExchangeRates.GetExchangeRate(ZString.Empty, orgPK, orgType));
					if (!orgPK.IsEmpty && orgPK != org.PK)
					{
						AssertNull(TestJob.ExchangeRates.GetExchangeRate(USD.RX_Code, orgPK, orgType));
					}
				}
			}

			AssertEquals(genericRate.PK, TestJob.ExchangeRates.GetExchangeRate(USD.RX_Code, ZGuid.Empty, ExchangeRateOrgTypeEnum.None)?.PK);
			AssertEquals(genericDebtorRate.PK, TestJob.ExchangeRates.GetExchangeRate(USD.RX_Code, ZGuid.Empty, ExchangeRateOrgTypeEnum.Debtor)?.PK);
			AssertEquals(genericCreditorRate.PK, TestJob.ExchangeRates.GetExchangeRate(USD.RX_Code, ZGuid.Empty, ExchangeRateOrgTypeEnum.Creditor)?.PK);

			AssertEquals(genericOrgRate.PK, TestJob.ExchangeRates.GetExchangeRate(USD.RX_Code, org.PK, ExchangeRateOrgTypeEnum.None)?.PK);
			AssertEquals(debtorRate.PK, TestJob.ExchangeRates.GetExchangeRate(USD.RX_Code, org.PK, ExchangeRateOrgTypeEnum.Debtor)?.PK);
			AssertEquals(creditorRate.PK, TestJob.ExchangeRates.GetExchangeRate(USD.RX_Code, org.PK, ExchangeRateOrgTypeEnum.Creditor)?.PK);
		}

		#endregion

		public void TestFindExchangeRate()
		{
			AssertEquals("Count", 0, TestJob.ExchangeRates.Count);

			var rateUSD = TestJob.ExchangeRates.AddNew();
			rateUSD.JF_RX_NKRateCurrency = USD.RX_Code;
			rateUSD.JF_BaseRate = .70M;

			var rateGBP = TestJob.ExchangeRates.AddNew();
			rateGBP.JF_RX_NKRateCurrency = GBP.RX_Code;
			rateGBP.JF_BaseRate = .39M;

			AssertEquals("Count", 2, TestJob.ExchangeRates.Count);

			var foundRate = TestJob.ExchangeRates.FindByRefCurrency(GBP);
			AssertEquals("GBP", rateGBP.PK, foundRate.PK);

			foundRate = TestJob.ExchangeRates.FindByRefCurrency(USD);
			AssertEquals("USD", rateUSD.PK, foundRate.PK);
		}

		public override void TestDelete()
		{
			AssertEquals(0, TestJob.ExchangeRates.Count);

			var usdRate = TestJob.ExchangeRates.AddNew();
			usdRate.JF_RX_NKRateCurrency = USD.RX_Code;

			var usdRate2 = TestJob.ExchangeRates.AddNew();
			usdRate2.JF_RX_NKRateCurrency = USD.RX_Code;

			var charge1 = TestJob.Charges.AddNew();
			charge1.JR_RX_NKCostCurrency = USD.RX_Code;
			charge1.JR_OSCostAmt = 10m;

			AssertEquals("Count of Exchange rates", 2, TestJob.ExchangeRates.Count);

			TestJob.ExchangeRates.RemoveAndDelete(usdRate2);
			AssertEquals("Count of Exchange rates should be 1 because two exchange rates for the same currency are not required to be in the TestJob.ExchangeRates", 1, TestJob.ExchangeRates.Count);

			var rubbishRate = TestJob.ExchangeRates.AddNew();
			AssertEquals("Count of Exchange rates", 2, TestJob.ExchangeRates.Count);
			TestJob.ExchangeRates.RemoveAndDelete(rubbishRate);
			AssertEquals("Count of Exchange rates", 1, TestJob.ExchangeRates.Count);

			var localRate = TestJob.ExchangeRates.AddNew();
			localRate.JF_RX_NKRateCurrency = AUD.RX_Code;

			var charge2 = TestJob.Charges.AddNew();
			charge2.JR_RX_NKCostCurrency = AUD.RX_Code;
			charge2.JR_OSCostAmt = 100m;

			AssertEquals("Count of Exchange rates", 2, TestJob.ExchangeRates.Count);
			TestJob.ExchangeRates.RemoveAndDelete(localRate);
			AssertEquals("Count of Exchange rates", 1, TestJob.ExchangeRates.Count);
		}

		public void TestDeleteByDataRefreshBus()
		{
			var usdRate = TestJob.ExchangeRates.AddNew();
			usdRate.JF_RX_NKRateCurrency = USD.RX_Code;
			usdRate.JF_BaseRate = 0.1m;
			usdRate.OrgType = ExchangeRateOrgTypeEnum.Debtor;
			usdRate.JF_IsTransformed = false;

			Factory.Save();

			var factory2 = new BusinessObjectFactory();

			var job2 = factory2.Load<JobForTesting>(TestJob.PK);

			AssertEquals(1, job2.ExchangeRates.Count);
			AssertEquals(usdRate.PK, job2.ExchangeRates.First().PK);
			Assert(!job2.IsRefreshChargeLinesExchangeRateBindingCalled);

			usdRate.Delete();

			Factory.Save();

			Assert(!job2.ExchangeRates.Any());
			Assert(job2.IsRefreshChargeLinesExchangeRateBindingCalled);
		}

		public void TestRemove()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.ExchangeRatesCollectionRemoveMethodInfo);

			var collection = new ExchangeRatesCollection(TestJob, Factory);
			var rate = collection.AddNew();

			Factory.SetContext(BusinessContext.DeletingExchangeRate);

			var expectMessage = $"Hash Code = {collection.GetHashCode()}, Contains ? NotInCollection";
			var actualMessage = "actual message";

			try
			{
				collection.Remove(rate);
				actualMessage = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(rate.PK, CriticalValidationInfoCollectorServiceKeyType.ExchangeRatesCollectionRemoveMethodInfo);
			}
			finally
			{
				Factory.RemoveContext(BusinessContext.DeletingExchangeRate);
			}
			AssertContains(expectMessage, actualMessage);
		}

		public void TestRemoveWhileAdding()
		{
			var collection = new ExchangeRatesCollection(TestJob, Factory);
			var rate = collection.AddNew();

			using (rate.SetTempContext(ExchangeRatesCollection.Context.AddingNewRate))
			{
				ErrorReporter.Clear();
				collection.Remove(rate);
			}
			AssertEquals("TotalErrorCount", 1, ErrorReporter.TotalErrorCount);
			AssertEquals("Error must be reported", "RemovingExRateInTheProcessOfAddingIt", ErrorReporter.LastKeyReported);
			var expectMessage = $@"Removed ExchangeRate:
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

		#region Implementation

		protected virtual Job TestJob => testJob ?? (testJob = TestObjectCreator.CreateJob(Creditor1, 0, null, 0));
		Job testJob;

		protected RefCurrency AUD => this.TestObjectCreator.LocalCurrency;
		protected RefCurrency USD => this.TestObjectCreator.USD;
		protected RefCurrency GBP => this.TestObjectCreator.GBP;

		protected OrgHeader Creditor1 => creditor1 ?? (creditor1 = TestObjectCreator.CreateOrgHeader("CREDITOR1", true, false, true, false, false, false));
		OrgHeader creditor1;

		protected TestObjectCreator TestObjectCreator;

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ExchangeRatesCollection(Factory.NewJobForTesting<Job>(), Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
		}

		class JobForTesting : Job
		{
			public JobForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool IsRefreshChargeLinesExchangeRateBindingCalled { get; private set; }

			public override void RefreshChargeLinesExchangeRateBinding()
			{
				IsRefreshChargeLinesExchangeRateBindingCalled = true;
			}
		}

		#endregion
	}
}
