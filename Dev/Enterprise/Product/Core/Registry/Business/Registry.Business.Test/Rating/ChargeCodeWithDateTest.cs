using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ChargeCodeWithDate))]
	sealed class ChargeCodeWithDateTest : RegistryBusinessObjectTemplateTestCase
	{
		[TestDate(2020, 11, 9, 12, 10, 0)]
		public void TestActiveTimeUtc()
		{
			var chargeCodeWithDate = new ChargeCodeWithDate();
			AssertEquals(ZDateTime.Empty, chargeCodeWithDate.ActiveTimeUtc);

			var iXmlSerializable = chargeCodeWithDate as IXmlSerializable;

			var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ChargeCodeWithDate>
	<ChargeCode>4651f483-a468-4790-ac40-0fc9d841a834</ChargeCode>
	<ActiveTimeUtc>2020-11-10 01:26:01</ActiveTimeUtc>
</ChargeCodeWithDate>";
			var value = System.Text.Encoding.UTF8.GetBytes(xml);

			using (var stream = new MemoryStream(value))
			using (var reader = new XmlTextReader(stream))
			{
				reader.Read();
				reader.ReadStartElement("ChargeCodeWithDate");
				iXmlSerializable.ReadXml(reader);
				AssertEquals(new ZDateTime(2020, 11, 10, 1, 26, 1), chargeCodeWithDate.ActiveTimeUtc);
			}

			using (var stream = new MemoryStream())
			using (var writer = new XmlTextWriter(stream, Encoding.UTF8))
			{
				writer.WriteStartElement("ChargeCodeWithDate");
				iXmlSerializable.WriteXml(writer);
				writer.WriteEndElement();
				writer.Flush();
				var result = System.Text.Encoding.Default.GetString(stream.ToArray());
				AssertContains("2020-11-10 01:26:01", result);
			}

			chargeCodeWithDate.ActiveTimeUtc = ZDateTime.Empty;
			using (var stream = new MemoryStream())
			using (var writer = new XmlTextWriter(stream, Encoding.UTF8))
			{
				writer.WriteStartElement("ChargeCodeWithDate");
				iXmlSerializable.WriteXml(writer);
				writer.WriteEndElement();
				writer.Flush();
				var result = System.Text.Encoding.Default.GetString(stream.ToArray());
				AssertContains("2020-11-09 12:10:00", result);
			}
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var chargeCodeWithDate = new ChargeCodeWithDate();
			chargeCodeWithDate.ChargeCode = ZGuid.BrettsGuid;
			chargeCodeWithDate.ActiveTimeUtc = ZDateTime.Today;
			return chargeCodeWithDate;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		#endregion
	}
}
