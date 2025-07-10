using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class SaudiArabiaQRCodeDataProviderTest : TestCaseWithFactory
	{
		public void TestSaudiArabiaQRCodeIsEmptyWithoutTransactionPivotAndVatRegNumber()
		{
			var builder = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.SaudiArabia) as IQRCodeDataProvider;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SaudiArabia))
			{
				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				AssertNullOrEmpty(builder.GetTransactionQRCodeString(invoice));
			}
		}

		[TestDate(2022, 9, 14, 15, 09, 30, 12)]
		public void TestSaudiArabiaQRCodeDataWhenTransactionPivotStatusIsFailedAndEInvoicingIsDisabled()
		{
			var builder = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.SaudiArabia) as IQRCodeDataProvider;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SaudiArabia))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var invoice = CreateTestARInvoice("SA Company");
				var invoicePivot = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice);
				invoicePivot.AIP_Status = Constants.EInvoicingPivotState.Failed;

				var expectedResult = "AQpTQSBDb21wYW55AgYxMjM0NTYDEzIwMjItMDktMTRUMTU6MDk6MzAEBzExMDAuMDAFBjEwMC4wMA==";

				AssertEquals(expectedResult, builder.GetTransactionQRCodeString(invoice));
			}
		}

		[TestDate(2022, 9, 14, 15, 09, 30, 12)]
		public void TestSaudiArabiaQRCodeDataWhenTransactionPivotStatusIsDiscarded()
		{
			var builder = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.SaudiArabia) as IQRCodeDataProvider;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SaudiArabia))
			{
				var invoice = CreateTestARInvoice("SA Company");
				var invoicePivot = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice);
				invoicePivot.AIP_Status = Constants.EInvoicingPivotState.Discarded;

				var expectedResult = "AQpTQSBDb21wYW55AgYxMjM0NTYDEzIwMjItMDktMTRUMTU6MDk6MzAEBzExMDAuMDAFBjEwMC4wMA==";

				AssertEquals(expectedResult, builder.GetTransactionQRCodeString(invoice));
			}
		}

		public void TestSaudiArabiaQRCodeIsNullWithoutAuthorisationRecord()
		{
			var builder = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.SaudiArabia) as IQRCodeDataProvider;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SaudiArabia))
			{
				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				var invoicePivot = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice);
				invoicePivot.AIP_Status = Constants.EInvoicingPivotState.Sent;

				AssertNullOrEmpty(builder.GetTransactionQRCodeString(invoice));
			}
		}

		public void TestSaudiArabiaQRCodeDataWithAuthorisationRecord()
		{
			var builder = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.SaudiArabia) as IQRCodeDataProvider;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SaudiArabia))
			{
				var expectedResult = "h9xE0VEpvS7qw2bIhB67d6jYRfCaponFxbf50zs25AyLDuC";

				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				CreateTestAccTransactionHeaderAuthorisationRecord(invoice.PK, expectedResult);
				var invoicePivot = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice);

				var validPivotStateList = new List<string>
				{
					EInvoicingPivotState.Queued,
					EInvoicingPivotState.Batched,
					EInvoicingPivotState.BatchedWithError,
					EInvoicingPivotState.Sent,
					EInvoicingPivotState.Delivered,
					EInvoicingPivotState.Succeed,
					EInvoicingPivotState.Failed,
					EInvoicingPivotState.Pending,
					EInvoicingPivotState.AwaitingReview,
				};

				foreach (var pivotState in validPivotStateList)
				{
					invoicePivot.AIP_Status = pivotState;

					AssertEquals(expectedResult, builder.GetTransactionQRCodeString(invoice));
				}
			}
		}

		ARInvoice CreateTestARInvoice(ZString fullName)
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = fullName;
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "123456", Constants.CountryCodes.SaudiArabia);
			Factory.Save();

			branch.GB_OH_OrgProxy = orgHeader.PK;
			Factory.Save();

			invoice.AH_GB = branch.PK;
			invoice.AH_OSExTaxAmount = 1000m;
			invoice.AH_OSTaxAmount = 100m;

			return invoice;
		}

		void CreateTestAccTransactionHeaderAuthorisationRecord(ZGuid parentId, ZString verificationUrl)
		{
			var authorisationRecord = Factory.NewWithValidTestData<AccTransactionHeaderAuthorisationRecord>();
			authorisationRecord.AHF_ParentId = parentId;
			authorisationRecord.AHF_ParentTableCode = "AH";
			authorisationRecord.AHF_IDNumber = "AOIVD8801";
			authorisationRecord.AHF_VerificationUrl = verificationUrl;

			Factory.Save();
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
