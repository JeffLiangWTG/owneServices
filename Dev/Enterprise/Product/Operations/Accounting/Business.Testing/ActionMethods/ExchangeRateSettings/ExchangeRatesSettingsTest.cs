using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	using System.IO;
	using System.Xml;
	using System.Xml.Serialization;
	using Enterprise.Services.OperationalActions.Support.Testing;

	[TestedType(typeof(ExchangeRatesSettings))]
	internal class ExchangeRatesSettingsTest : OperationalActionMethodSettingsTest<ExchangeRatesSettings>
	{
		public void TestSerialisation()
		{
			const string expectedXml = "<Settings>" + "<ExchangeRatesSource>SSR</ExchangeRatesSource>" + "</Settings>" + "";
			ExchangeRatesSettings settings1 = new ExchangeRatesSettings(new ExRateSourceType[] { ExRateSourceType.Voyage });
			settings1.ExchangeRatesSource = ExchangeRatesSourceList.Codes.SailingSchedule;
			string xml;
			using (StringWriter stream = new StringWriter())
				using (XmlTextWriter writer = new XmlTextWriter(stream))
				{
					writer.WriteStartElement("Settings");
					((IXmlSerializable)settings1).WriteXml(writer);
					writer.WriteEndElement();
					writer.Flush();
					xml = stream.ToString();
				}

			this.AssertXMLEqualsByDiff("", expectedXml.Replace("><", ">\n<"), xml.Replace("><", ">\n<"));
			ExchangeRatesSettings settings2;
			using (StringReader stream = new StringReader(xml))
				using (XmlTextReader reader = new XmlTextReader(stream))
				{
					settings2 = new ExchangeRatesSettings(new ExRateSourceType[] { ExRateSourceType.Voyage });
					((IXmlSerializable)settings2).ReadXml(reader);
				}

			AssertEquals(ExchangeRatesSourceList.Codes.SailingSchedule, settings2.ExchangeRatesSource);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ExchangeRatesSettings(new ExRateSourceType[] { ExRateSourceType.Voyage });
		}
	}
}
