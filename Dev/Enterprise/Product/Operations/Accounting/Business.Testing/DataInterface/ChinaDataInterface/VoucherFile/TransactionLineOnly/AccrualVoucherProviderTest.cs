using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	[TestedType(typeof(AccrualVoucherProvider))]
	public class AccrualVoucherProviderTest : LineOnlyVoucherTestCase
	{
		public void Testconstructor()
		{
			AccrualVoucherProvider testProvider = AccrualVoucherProvider.New(Factory, new ZDateTime(2004, 3, 16), new ZDateTime(2004, 3, 31));
			AssertNotNull(testProvider);
		}

		public void TestLineCount()
		{
			VoucherDataSource lineToReturn = new VoucherDataSource(Factory, GetDummyDataRow(Guid.Empty, "", 0.0m));
			var mockCollection = new Mock<WIPAccrualDataSourceCollection>();
			mockCollection.Setup(m => m.GetCount()).Returns(3);
			mockCollection.Setup(m => m.GetVoucherData(0)).Returns(lineToReturn);
			mockCollection.Setup(m => m.GetVoucherData(1)).Returns(lineToReturn);
			mockCollection.Setup(m => m.GetVoucherData(2)).Returns(lineToReturn);
			AccrualVoucherProvider testProvider = new AccrualVoucherProvider(mockCollection.Object, 0);
			AssertEquals(4, testProvider.VoucherLines.Length);
		}

		public void TestLineDetail()
		{
			ChargeCode1.AC_AG_AccrualAccount = GLAccount1.PK;
			var mockCollection = new Mock<WIPAccrualDataSourceCollection>(Factory) { CallBase = true };
			VoucherDataSource lineToReturn = new VoucherDataSource(Factory, GetDummyDataRow(GLAccount1.PK.ToGuid(), "", 0.0m));
			mockCollection.Setup(m => m.GetCount()).Returns(3);
			mockCollection.Setup(m => m.GetVoucherData(0)).Returns(lineToReturn);
			mockCollection.Setup(m => m.GetVoucherData(1)).Returns(lineToReturn);
			mockCollection.Setup(m => m.GetVoucherData(2)).Returns(lineToReturn);
			AccrualVoucherProvider testProvider = new AccrualVoucherProvider(mockCollection.Object, 0);
			AssertEquals(LocalAccountNumber1, testProvider.VoucherLines[0].AccountNumber);
		}

		public void TestDebitCredit()
		{
			VoucherDataSource lineToReturn = new VoucherDataSource(Factory, GetDummyDataRow(Guid.Empty, "", 120.0m));
			var mockCollection = new Mock<WIPAccrualDataSourceCollection>();
			mockCollection.Setup(m => m.GetCount()).Returns(1);
			mockCollection.Setup(m => m.GetVoucherData(0)).Returns(lineToReturn);
			AccrualVoucherProvider testProvider = new AccrualVoucherProvider(mockCollection.Object, 0);
			AssertEquals(120m, testProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0.0m, testProvider.VoucherLines[0].CreditAmount);
			AssertEquals("Exchange Rate", 1M, testProvider.VoucherLines[0].ExchangeRate);
			AssertEquals(120m, testProvider.VoucherLines[0].ForeignCurrencyAmount);
			AssertEquals(1m, testProvider.VoucherLines[0].ExchangeRate);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, testProvider.VoucherLines[0].CurrencyCode);
		}

		public void TestControlAccount()
		{
			VoucherDataSource lineToReturn1 = new VoucherDataSource(Factory, GetDummyDataRow(Guid.Empty, TransactionLineTypes.Accrual, 120.0m));
			VoucherDataSource lineToReturn2 = new VoucherDataSource(Factory, GetDummyDataRow(Guid.Empty, TransactionLineTypes.Accrual, 70.0m));
			VoucherDataSource lineToReturn3 = new VoucherDataSource(Factory, GetDummyDataRow(Guid.Empty, TransactionLineTypes.Accrual, 80.0m));
			var mockCollection = new Mock<WIPAccrualDataSourceCollection>(Factory) { CallBase = true };
			mockCollection.Setup(m => m.GetCount()).Returns(3);
			mockCollection.Setup(m => m.GetVoucherData(0)).Returns(lineToReturn1);
			mockCollection.Setup(m => m.GetVoucherData(1)).Returns(lineToReturn2);
			mockCollection.Setup(m => m.GetVoucherData(2)).Returns(lineToReturn3);
			AccrualVoucherProvider testProvider = new AccrualVoucherProvider(mockCollection.Object, 0);
			AssertEquals(4, testProvider.VoucherLines.Length);
			AssertEquals(0.0m, testProvider.VoucherLines[3].DebitAmount);
			AssertEquals(270.0m, testProvider.VoucherLines[3].CreditAmount);
			AssertEquals(270.0m, testProvider.VoucherLines[3].ForeignCurrencyAmount);
			AssertEquals(1m, testProvider.VoucherLines[3].ExchangeRate);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, testProvider.VoucherLines[3].CurrencyCode);
		}

		public void TestVoucherDate()
		{
			ChargeCode1.AC_AG_WIPAccount = GLAccount1.PK;
			ZDateTime endDate = new ZDateTime(2004, 2, 28);
			ZInt period = 200402;
			AccPeriodManagement period1 = Factory.NewWithValidTestData(typeof(AccPeriodManagement)) as AccPeriodManagement;
			period1.AM_Period = period;
			period1.AM_EndDate = endDate;
			period1.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			var mockCollection = new Mock<WIPAccrualDataSourceCollection>(Factory) { CallBase = true };
			VoucherDataSource lineToReturn = new VoucherDataSource(Factory, GetDummyDataRow(ChargeCode1.PK.ToGuid(), "", 0.0m));
			mockCollection.Setup(m => m.GetCount()).Returns(3);
			mockCollection.Setup(m => m.GetVoucherData(0)).Returns(lineToReturn);
			mockCollection.Setup(m => m.GetVoucherData(1)).Returns(lineToReturn);
			mockCollection.Setup(m => m.GetVoucherData(2)).Returns(lineToReturn);
			AccrualVoucherProvider testProvider = new AccrualVoucherProvider(mockCollection.Object, period);
			AssertEquals(endDate, testProvider.VoucherLines[0].VoucherDate);
			AssertEquals(endDate, testProvider.VoucherLines[1].VoucherDate);
			AssertEquals(endDate, testProvider.VoucherLines[2].VoucherDate);
		}

		protected override VoucherProvider GetVoucherProvider(AccTransactionHeader transaction)
		{
			return new AccrualVoucherProvider(new WIPAccrualDataSourceCollection(Factory), 200401);
		}

		public override void TestSourceIdentifierProvider()
		{
			var provider = GetVoucherProvider(null) as DocumentEngineIntegration.ISourceIdentifierProvider;

			AssertNotNull(provider);
			AssertEquals(ZGuid.Empty, provider.SourceIdentifier);
		}

		protected override AccTransactionHeader GetTestTransaction()
		{
			return null;
		}

		protected override ZInt GetExpectedAttachementNo()
		{
			return 0;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AccrualVoucherProvider(new WIPAccrualDataSourceCollection(Factory), 0);
		}

		protected override bool OrganisationCodeIsApplicableToThisVoucher
		{
			get
			{
				return false;
			}
		}

		protected override void AssertValuesForFirstLine(VoucherProvider standardProvider, VoucherProvider providerWithOptionalFields)
		{
			Assert(true);
		}

		protected override void AssertValuesForControlLine(VoucherProvider standardProvider, VoucherProvider providerWithOptionalFields)
		{
			Assert(true);
		}
	}
}
