using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

public class TariffFormatterCHTest : TestCase
{
	public void TestDisplayFormat()
	{
		CombineAssertions(() =>
		{
			AssertEquals("For 12345678", "1234.5678", Formatter.DisplayFormat("12345678"));
			AssertEquals("For 12345678912", "1234.5678 912", Formatter.DisplayFormat("12345678912"));
			AssertEquals("For 12345678000315", "1234.5678 000 315", Formatter.DisplayFormat("12345678000315"));
			AssertEquals("For 1234567844412 (incomplete statistical code)", "1234.5678 444 12", Formatter.DisplayFormat("1234567844412"));
		});
	}

	public void TestFormat()
	{
		CombineAssertions(() =>
		{
			AssertEquals("For 1234.5678", "12345678", Formatter.Format("1234.5678"));
			AssertEquals("For 1234.5678 912", "12345678912", Formatter.Format("1234.5678 912"));
			AssertEquals("For 1234.5678 000315", "12345678000315", Formatter.Format("1234.5678 000315"));
			AssertEquals("For 1234.5678 44412 (incomplete statistical code)", "1234567844412", Formatter.Format("1234.5678 44412"));
			AssertEquals("For 1234.5678 4440 (incomplete statistical code)", "123456784440", Formatter.Format("1234.5678 4440"));
		});
	}

	TariffFormatterCH Formatter => new TariffFormatterCH();
}
