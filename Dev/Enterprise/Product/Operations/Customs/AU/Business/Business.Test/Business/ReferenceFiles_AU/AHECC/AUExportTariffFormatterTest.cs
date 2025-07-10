using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUExportTariffFormatterTest : TestCase
	{
		public void TestFormatCompleteTariff()
		{
			AssertEquals("1234.56.78", tariffFormatter.Format("1234.56.78"));
			AssertEquals("1234.56.78", tariffFormatter.Format("12345678"));
		}

		public void TestFormatIncompleteTariff()
		{
			AssertEquals("1234.56", tariffFormatter.Format("1234.56"));
			AssertEquals("1234.56", tariffFormatter.Format("123456"));
		}

		public void TestFormatEmptyTariff()
		{
			AssertEquals(ZString.Empty, tariffFormatter.Format(string.Empty));
		}

		TariffFormatter tariffFormatter;
		protected override void SetUp()
		{
			base.SetUp();
			tariffFormatter = new AUExportTariffFormatter();
		}
	}

	sealed class AUExportTariffUniversalFormatterTest : TransactionedTestCase
	{
		public void TestFormatCompleteTariff()
		{
			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("1234.56.78", tariffFormatter.Format("1234.56.78"));
				AssertEquals("1234.56.78", tariffFormatter.Format("12345678"));
			}

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("12345678", tariffFormatter.Format("1234.56.78"));
				AssertEquals("12345678", tariffFormatter.Format("12345678"));
			}
		}

		public void TestFormatIncompleteTariff()
		{
			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("1234.56", tariffFormatter.Format("1234.56"));
				AssertEquals("1234.56", tariffFormatter.Format("123456"));
			}

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("123456", tariffFormatter.Format("1234.56"));
				AssertEquals("123456", tariffFormatter.Format("123456"));
			}
		}

		public void TestFormatEmptyTariff()
		{
			AssertEquals(ZString.Empty, tariffFormatter.Format(string.Empty));
		}

		public void TestFormatDotted()
		{
			IAUTariffFormatter formatter = new AUExportTariffUniversalFormatter();
			AssertEquals("1234.56.78", formatter.FormatDotted("1234.56.78"));
			AssertEquals("1234.56.78", formatter.FormatDotted("12345678"));
		}

		TariffFormatter tariffFormatter;
		protected override void SetUp()
		{
			base.SetUp();
			tariffFormatter = new AUExportTariffUniversalFormatter();
		}
	}
}
