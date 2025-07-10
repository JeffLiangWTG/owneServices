using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	sealed class UnsatisfactoryReasonTest : DataProviderTestCase<ReasonProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ReasonProvider(null));
		}

		public void TestReasonCode()
		{
			AssertEquals("3", dataProvider.ReasonCode);
		}

		public void TestComplementaryInformation()
		{
			AssertEquals("COMPLEMENTARY INFORMATION OF REPORT OF RECEIPT REASON", dataProvider.ComplementaryInformation.Text);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var invoiceLine = Factory.New<EMCSJobComInvoiceLine>();
			var reportOfReceiptReason = invoiceLine.Outturn.ReportOfReceiptReasons.AddNew();
			reportOfReceiptReason.CY_Code = "3";
			reportOfReceiptReason.CY_Data = "COMPLEMENTARY INFORMATION OF REPORT OF RECEIPT REASON";
			dataProvider = new ReasonProvider(reportOfReceiptReason);
		}
		ReasonProvider dataProvider;

		protected override ReasonProvider GetProvider() => dataProvider;

		protected override IEnumerable<Expression<Func<ReasonProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.ComplementaryInformation;
		}
	}
}
