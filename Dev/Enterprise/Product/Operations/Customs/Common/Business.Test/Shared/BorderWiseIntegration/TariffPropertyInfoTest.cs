using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Common.Testing
{
	public class TariffPropertyInfoTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestProperties()
		{
			var date = new ZDateTime(2010, 10, 10);
			var tariffInfo = new TariffPropertyInfo(TariffType.Import, date, "1234");
			AssertTariffInfo(tariffInfo, TariffType.Import, date, "1234");
			NUnit.Framework.Assert.That(tariffInfo.ToString(), Is.EqualTo("Type: 'Import', Code: '1234'"), "ToString");
		}

		[ExpectNoExceptions]
		public static void AssertTariffInfo(TariffPropertyInfo tariffInfo, TariffType expectedType, ZDateTime expectedDateForDutyRate, string expectedCode)
		{
			NUnit.Framework.Assert.That(tariffInfo.TariffType, Is.EqualTo(expectedType), "TariffType");
			NUnit.Framework.Assert.That(tariffInfo.DateForDutyRate, Is.EqualTo(expectedDateForDutyRate), "DateForDutyRate");
			NUnit.Framework.Assert.That(tariffInfo.TariffCode, Is.EqualTo(expectedCode), "TariffCode");
		}
	}

	class TariffInfoConverterTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestOverrides()
		{
			var tariffInfoConverter = new TariffInfoConverter();
			NUnit.Framework.Assert.That(tariffInfoConverter.GetPropertiesSupported(null), Is.EqualTo(true), "GetPropertiesSupported");
			var properties = tariffInfoConverter.GetProperties(null, null, null);
			NUnit.Framework.Assert.That(properties[0].Name, Is.EqualTo("TariffType"), "GetProperties - TariffType");
			NUnit.Framework.Assert.That(properties[0].DisplayName, Is.EqualTo("Tariff Type"), "TariffType DisplayName");
			NUnit.Framework.Assert.That(properties[1].Name, Is.EqualTo("TariffCode"), "GetProperties - TariffCode");
			NUnit.Framework.Assert.That(properties[1].DisplayName, Is.EqualTo("Tariff Code Start With"), "TariffCode DisplayName");
		}
	}
}
