using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	[TestedType(typeof(AccTaxRecordTransactionLinePivot))]
	class AccTaxRecordTransactionLinePivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLinkLine()
		{
			var taxRecord = Factory.New<AccTaxTransaction>();
			taxRecord.ATT_GC = GlbCompany.CurrentCompany.PK;
			var linePK = ZGuid.NewZGuid();
			var lineTaxableTransactionMock = new Mock<ITaxableTransactionLine>();
			lineTaxableTransactionMock.SetupGet(f => f.Factory).Returns(Factory);
			lineTaxableTransactionMock.SetupGet(f => f.PK).Returns(linePK);
			lineTaxableTransactionMock.SetupGet(f => f.BaseOSAmount).Returns(100m);
			lineTaxableTransactionMock.SetupGet(f => f.LocalAmount).Returns(200m);

			var accTaxRecordTransactionLinePivot = Factory.New<AccTaxRecordTransactionLinePivot>();
#if NETFRAMEWORK
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: line", () => accTaxRecordTransactionLinePivot.LinkLine(null));
#else
			var ex = AssertExceptionThrown<ArgumentNullException>(() => accTaxRecordTransactionLinePivot.LinkLine(null));
			Assert(ex.Message.Equals("Value cannot be null. (Parameter 'line')"));
			Assert(ex.ParamName.Equals("line"));
#endif
			AssertExceptionThrown(typeof(InvalidOperationException), "It is not valid to link TransactionLine to Pivot without setting Tax Record (ATP_ATT) first.", () => accTaxRecordTransactionLinePivot.LinkLine(lineTaxableTransactionMock.Object));

			accTaxRecordTransactionLinePivot.ATP_ATT = taxRecord.PK;
			taxRecord.ATT_RX_NKOSTaxCurrency = CurrencyCodes.Australia;
			Assert(taxRecord.IsOSTaxCurrencyLocal);

			accTaxRecordTransactionLinePivot.LinkLine(lineTaxableTransactionMock.Object);
			AssertEquals(linePK, accTaxRecordTransactionLinePivot.ATP_AL_TransactionLine);
			AssertEquals(200m, accTaxRecordTransactionLinePivot.BaseOSAmount);
			AssertEquals(200m, accTaxRecordTransactionLinePivot.LocalTaxBaseAmount);

			linePK = ZGuid.NewZGuid();
			lineTaxableTransactionMock.SetupGet(f => f.PK).Returns(linePK);
			taxRecord.ATT_RX_NKOSTaxCurrency = CurrencyCodes.UnitedStates;
			Assert(!taxRecord.IsOSTaxCurrencyLocal);

			accTaxRecordTransactionLinePivot.LinkLine(lineTaxableTransactionMock.Object);
			AssertEquals(linePK, accTaxRecordTransactionLinePivot.ATP_AL_TransactionLine);
			AssertEquals(100m, accTaxRecordTransactionLinePivot.BaseOSAmount);
			AssertEquals(200m, accTaxRecordTransactionLinePivot.LocalTaxBaseAmount);
		}

		public void TestReportErrorIfTransactionLinePKSetDirectly()
		{
			var taxRecord = Factory.New<AccTaxTransaction>();
			taxRecord.ATT_GC = GlbCompany.CurrentCompany.PK;
			var linePK = ZGuid.NewZGuid();
			var lineTaxableTransactionMock = new Mock<ITaxableTransactionLine>();
			lineTaxableTransactionMock.SetupGet(f => f.Factory).Returns(Factory);
			lineTaxableTransactionMock.SetupGet(f => f.PK).Returns(linePK);
			lineTaxableTransactionMock.SetupGet(f => f.BaseOSAmount).Returns(100m);
			lineTaxableTransactionMock.SetupGet(f => f.LocalAmount).Returns(200m);

			var accTaxRecordTransactionLinePivot = Factory.New<AccTaxRecordTransactionLinePivot>();
			accTaxRecordTransactionLinePivot.ATP_ATT = taxRecord.PK;
			taxRecord.ATT_RX_NKOSTaxCurrency = CurrencyCodes.Australia;
			Assert(taxRecord.IsOSTaxCurrencyLocal);

			accTaxRecordTransactionLinePivot.LinkLine(lineTaxableTransactionMock.Object);
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			AssertEquals(linePK, accTaxRecordTransactionLinePivot.ATP_AL_TransactionLine);
			AssertEquals(200m, accTaxRecordTransactionLinePivot.BaseOSAmount);
			AssertEquals(200m, accTaxRecordTransactionLinePivot.LocalTaxBaseAmount);

			linePK = ZGuid.NewZGuid();
			taxRecord.ATT_RX_NKOSTaxCurrency = CurrencyCodes.UnitedStates;
			Assert(!taxRecord.IsOSTaxCurrencyLocal);

			accTaxRecordTransactionLinePivot.ATP_AL_TransactionLine = linePK;
			CombineAssertions(() =>
			{
				AssertEquals("AccTaxRecordTransactionLinePivot_TransactionLineSetWithoutLinkLine", ErrorReporter.LastKeyReported);
				AssertContains("ATP_AL_TransactionLine is being set on AccTaxRecordTransactionLinePivot without using LinkLine method", ErrorReporter.LastMessageReported);
			});
			ErrorReporter.Clear();
			AssertEquals(linePK, accTaxRecordTransactionLinePivot.ATP_AL_TransactionLine);
			AssertEquals(200m, accTaxRecordTransactionLinePivot.BaseOSAmount);
			AssertEquals(200m, accTaxRecordTransactionLinePivot.LocalTaxBaseAmount);
		}
	}
}
