using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	[TestedType(typeof(WIPVoucherProvider))]
	public class WIPVoucherProviderTest : LineOnlyVoucherTestCase
	{
		protected override VoucherProvider GetVoucherProvider(AccTransactionHeader transaction)
		{
			return new WIPVoucherProvider(new WIPAccrualDataSourceCollection(Factory), 200401);
		}

		protected override AccTransactionHeader GetTestTransaction()
		{
			return null;
		}

		protected override ZInt GetExpectedAttachementNo()
		{
			return 0;
		}

		public void Testconstructor()
		{
			WIPVoucherProvider testProvider = WIPVoucherProvider.New(Factory, new ZDateTime(2004, 2, 16), new ZDateTime(2004, 2, 28));
			AssertNotNull(testProvider);
		}

		public void TestControlAccount()
		{
			VoucherDataSource lineToReturn1 = new VoucherDataSource(Factory, GetDummyDataRow(Guid.Empty, TransactionLineTypes.WIP, -120.0m));
			VoucherDataSource lineToReturn2 = new VoucherDataSource(Factory, GetDummyDataRow(Guid.Empty, TransactionLineTypes.WIP, -70.0m));
			VoucherDataSource lineToReturn3 = new VoucherDataSource(Factory, GetDummyDataRow(Guid.Empty, TransactionLineTypes.WIP, -80.0m));

			var mockCollection = new Mock<WIPAccrualDataSourceCollection>(Factory);
			mockCollection.Setup(m => m.GetCount()).Returns(3);
			mockCollection.Setup(m => m.GetVoucherData(0)).Returns(lineToReturn1);
			mockCollection.Setup(m => m.GetVoucherData(1)).Returns(lineToReturn2);
			mockCollection.Setup(m => m.GetVoucherData(2)).Returns(lineToReturn3);
			WIPVoucherProvider testProvider = new WIPVoucherProvider(mockCollection.Object, 0);
			AssertEquals(4, testProvider.VoucherLines.Length);
			AssertEquals(270.0m, testProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, testProvider.VoucherLines[0].CreditAmount);
			AssertEquals(270.0m, testProvider.VoucherLines[0].ForeignCurrencyAmount);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, testProvider.VoucherLines[0].CurrencyCode);
			AssertEquals(1m, testProvider.VoucherLines[0].ExchangeRate);
			AssertEquals(270.0m, testProvider.VoucherLines[0].OSDebitAmount);
			AssertEquals(0m, testProvider.VoucherLines[0].OSCreditAmount);
		}

		public void TestControlAccountEmpty()
		{
			VoucherDataSource lineToReturn1 = new VoucherDataSource(Factory, GetDummyDataRow(Guid.Empty, "", 0));
			VoucherDataSource lineToReturn2 = new VoucherDataSource(Factory, GetDummyDataRow(Guid.Empty, "", 0));
			VoucherDataSource lineToReturn3 = new VoucherDataSource(Factory, GetDummyDataRow(Guid.Empty, "", 0));
			var mockCollection = new Mock<WIPAccrualDataSourceCollection>();
			mockCollection.Setup(m => m.GetCount()).Returns(3);
			mockCollection.Setup(m => m.GetVoucherData(0)).Returns(lineToReturn1);
			mockCollection.Setup(m => m.GetVoucherData(1)).Returns(lineToReturn2);
			mockCollection.Setup(m => m.GetVoucherData(2)).Returns(lineToReturn3);
			var mockControlAccount = new Mock<IControlAccountProvider>();
			mockControlAccount.Setup(m => m.SetTransaction(It.IsAny<AccTransactionHeader>()));
			mockControlAccount.Setup(m => m.PK).Returns(ZGuid.Empty);
			WIPVoucherProvider testProvider = new WIPVoucherProvider(mockCollection.Object, 0);
			AssertEquals(4, testProvider.VoucherLines.Length);
			AssertEquals("", testProvider.VoucherLines[3].AccountNumber);
		}

		public void TestDebitCredit()
		{
			VoucherDataSource lineToReturn = new VoucherDataSource(Factory, GetDummyDataRow(Guid.Empty, "", -120.0m));
			var mockCollection = new Mock<WIPAccrualDataSourceCollection>();
			mockCollection.Setup(m => m.GetCount()).Returns(1);
			mockCollection.Setup(m => m.GetVoucherData(0)).Returns(lineToReturn);
			WIPVoucherProvider testProvider = new WIPVoucherProvider(mockCollection.Object, 0);
			AssertEquals(0m, testProvider.VoucherLines[1].DebitAmount);
			AssertEquals(120.0m, testProvider.VoucherLines[1].CreditAmount);
			AssertEquals(120.0m, testProvider.VoucherLines[1].ForeignCurrencyAmount);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, testProvider.VoucherLines[1].CurrencyCode);
			AssertEquals(1m, testProvider.VoucherLines[1].ExchangeRate);
			AssertEquals(0m, testProvider.VoucherLines[1].OSDebitAmount);
			AssertEquals(120.0m, testProvider.VoucherLines[1].OSCreditAmount);
		}

		public void TestLineAccountNumber()
		{
			ChargeCode1.AC_AG_WIPAccount = GLAccount1.PK;
			var mockCollection = new Mock<WIPAccrualDataSourceCollection>(Factory);
			VoucherDataSource lineToReturn = new VoucherDataSource(Factory, GetDummyDataRow(GLAccount1.PK.ToGuid(), "", 0));
			mockCollection.Setup(m => m.GetCount()).Returns(3);
			mockCollection.Setup(m => m.GetVoucherData(0)).Returns(lineToReturn);
			mockCollection.Setup(m => m.GetVoucherData(1)).Returns(lineToReturn);
			mockCollection.Setup(m => m.GetVoucherData(2)).Returns(lineToReturn);
			WIPVoucherProvider testProvider = new WIPVoucherProvider(mockCollection.Object, 0);
			AssertEquals(LocalAccountNumber1, testProvider.VoucherLines[0].AccountNumber);
		}

		public void TestLineCount()
		{
			VoucherDataSource lineToReturn = new VoucherDataSource(Factory, GetDummyDataRow(Guid.Empty, "", 0));
			var mockCollection = new Mock<WIPAccrualDataSourceCollection>();
			mockCollection.Setup(m => m.GetCount()).Returns(3);
			mockCollection.Setup(m => m.GetVoucherData(0)).Returns(lineToReturn);
			mockCollection.Setup(m => m.GetVoucherData(1)).Returns(lineToReturn);
			mockCollection.Setup(m => m.GetVoucherData(2)).Returns(lineToReturn);
			WIPVoucherProvider testProvider = new WIPVoucherProvider(mockCollection.Object, 0);
			AssertEquals(4, testProvider.VoucherLines.Length);
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
			var mockCollection = new Mock<WIPAccrualDataSourceCollection>(Factory);
			VoucherDataSource lineToReturn = new VoucherDataSource(Factory, GetDummyDataRow(ChargeCode1.PK.ToGuid(), "", 0));
			mockCollection.Setup(m => m.GetCount()).Returns(3);
			mockCollection.Setup(m => m.GetVoucherData(0)).Returns(lineToReturn);
			mockCollection.Setup(m => m.GetVoucherData(1)).Returns(lineToReturn);
			mockCollection.Setup(m => m.GetVoucherData(2)).Returns(lineToReturn);
			WIPVoucherProvider testProvider = new WIPVoucherProvider(mockCollection.Object, period);
			AssertEquals(endDate, testProvider.VoucherLines[0].VoucherDate);
			AssertEquals(endDate, testProvider.VoucherLines[1].VoucherDate);
			AssertEquals(endDate, testProvider.VoucherLines[2].VoucherDate);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new WIPVoucherProvider(new WIPAccrualDataSourceCollection(Factory), 0);
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

		public override void TestSourceIdentifierProvider()
		{
			var provider = GetVoucherProvider(null) as DocumentEngineIntegration.ISourceIdentifierProvider;

			AssertNotNull(provider);
			AssertEquals(ZGuid.Empty, provider.SourceIdentifier);
		}
	}
}
