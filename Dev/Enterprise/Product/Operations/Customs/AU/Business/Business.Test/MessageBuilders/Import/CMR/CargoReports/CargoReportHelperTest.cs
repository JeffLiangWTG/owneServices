using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CargoReportHelperTest : TestCaseWithFactory
	{
		public void TestGetConsigneeABN()
		{
			AssertEquals("", CargoReportHelper.GetConsigneeABN(""));
			AssertEquals("12345", CargoReportHelper.GetConsigneeABN("12345"));
			AssertEquals("12345678901", CargoReportHelper.GetConsigneeABN("12345678901"));
			AssertEquals("12345678901", CargoReportHelper.GetConsigneeABN("1234567890123456"));
			AssertEquals("", CargoReportHelper.GetConsigneeABN("/23456"));
			AssertEquals("12345", CargoReportHelper.GetConsigneeABN("12345/23456"));
			AssertEquals("12345678901", CargoReportHelper.GetConsigneeABN("12345678901/23456"));
			AssertEquals("12345678901", CargoReportHelper.GetConsigneeABN("1234567890123456/23456"));
			AssertEquals("12345", CargoReportHelper.GetConsigneeABN("12345/"));
		}

		public void TestGetConsigneeCAC()
		{
			AssertEquals("", CargoReportHelper.GetConsigneeCAC(""));
			AssertEquals("", CargoReportHelper.GetConsigneeCAC("1234"));
			AssertEquals("123", CargoReportHelper.GetConsigneeCAC("/1234"));
			AssertEquals("123", CargoReportHelper.GetConsigneeCAC("1234/1234"));
			AssertEquals("", CargoReportHelper.GetConsigneeCAC("1234/"));
		}

		public void TestMethodsHandleABNsWithSpaces()
		{
			AssertEquals("Stored ABNs with spaces should be handled correctly", "12123123123", CargoReportHelper.GetConsigneeABN("12 123 123 123 / CAC"));
			AssertEquals("CAC values when with ABN with spaces should be handled correctly", "CAC", CargoReportHelper.GetConsigneeCAC("12 123 123 123 / CAC"));
			AssertEquals("Stored ABNs with spaces should be handled correctly", "79438295622", CargoReportHelper.GetConsigneeABN("79 438 295 622"));
		}

		[TestDate(2017, 4, 17)]
		public void TestDoICSRelease()
		{
			var hasExecuted = false;
			using (AUCustomsDataRegistry.Instance.ICSReleaseEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2017, 4, 1)))
			{
				CargoReportHelper.DoICSRelease(() =>
				{
					hasExecuted = true;
				});
				AssertEquals(true, hasExecuted);
			}

			hasExecuted = false;
			using (AUCustomsDataRegistry.Instance.ICSReleaseEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2017, 5, 1)))
			{
				CargoReportHelper.DoICSRelease(() =>
				{
					hasExecuted = true;
				});
				AssertEquals(false, hasExecuted);
			}
		}
	}
}
