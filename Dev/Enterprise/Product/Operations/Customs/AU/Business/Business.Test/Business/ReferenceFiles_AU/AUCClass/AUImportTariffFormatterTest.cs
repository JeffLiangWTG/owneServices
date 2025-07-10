using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUImportTariffFormatterTest : TestCase
	{
		public void TestFormatCompleteTariff()
		{
			AssertEquals("9999.99.99 99", tariffFormatter.Format("9999.99.99 99"));
			AssertEquals("9999.99.99 99", tariffFormatter.Format("9999999999"));
		}

		public void TestFormatInCompleteTariff()
		{
			AssertEquals("9999.99", tariffFormatter.Format("9999.99"));
			AssertEquals("9999.99", tariffFormatter.Format("999999"));
		}

		public void TestFormatEmptyTariff()
		{
			AssertEquals(ZString.Empty, tariffFormatter.Format(string.Empty));
		}

		TariffFormatter tariffFormatter;
		protected override void SetUp()
		{
			base.SetUp();
			tariffFormatter = new AUImportTariffFormatter();
		}
	}

	sealed class AUImportTariffUniversalFormatterTest : TransactionedTestCase
	{
		public void TestFormatCompleteTariff()
		{
			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("9999.99.99 99", tariffFormatter.Format("9999.99.99 99"));
				AssertEquals("9999.99.99 99", tariffFormatter.Format("9999999999"));
			}

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("9999999999", tariffFormatter.Format("9999.99.99 99"));
				AssertEquals("9999999999", tariffFormatter.Format("9999999999"));
			}
		}

		public void TestFormatIncompleteTariff()
		{
			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("9999.99", tariffFormatter.Format("9999.99"));
				AssertEquals("9999.99", tariffFormatter.Format("999999"));
			}

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("999999", tariffFormatter.Format("9999.99"));
				AssertEquals("999999", tariffFormatter.Format("999999"));
			}
		}

		public void TestFormatEmptyTariff()
		{
			AssertEquals(ZString.Empty, tariffFormatter.Format(string.Empty));
		}

		public void TestFormatDotted()
		{
			IAUTariffFormatter formatter = new AUImportTariffUniversalFormatter();
			AssertEquals("9999.99.99 99", formatter.FormatDotted("9999.99.99 99"));
			AssertEquals("9999.99.99 99", formatter.FormatDotted("9999999999"));
		}

		TariffFormatter tariffFormatter;
		protected override void SetUp()
		{
			base.SetUp();
			tariffFormatter = new AUImportTariffUniversalFormatter();
		}
	}
}
