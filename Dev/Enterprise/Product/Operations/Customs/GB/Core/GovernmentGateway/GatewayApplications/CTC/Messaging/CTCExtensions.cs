using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.Types;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Template")]
	public static class CTCExtensions
	{
		public static ZString Serialize<T>(T dataObj)
		{
			var settings = new XmlWriterSettings();
			settings.Encoding = Encoding.UTF8;
			using (var stream = new StringWriterWithEncoding(Encoding.UTF8, CultureInfo.InvariantCulture))
			using (var writerMessage = XmlWriter.Create(stream, settings))
			{
				var serializerMessage = ZXmlSerializer.New(typeof(T));
				serializerMessage.Serialize(writerMessage, dataObj);
				return CleanBoolean(CleanXML(stream.ToString()));
			}
		}

		public static T Deserialize<T>(ZString serializedObj)
		{
			using (var stream = new StringReader(serializedObj))
			{
				using (var reader = new XmlTextReader(stream))
				{
					var serializer = ZXmlSerializer.New(typeof(T));
					return (T)serializer.Deserialize(reader);
				}
			}
		}

		public static ZString CleanXML(string xml)
		{
			Regex myRegex = new Regex(@"(<\w+\s\/>)");
			return myRegex.Replace(xml, "");
		}

		public static ZString CleanBoolean(string xml)
		{
			return xml.Replace("<indd48>true</indd48>", "<indd48>1</indd48>")
				.Replace("<indd48>false</indd48>", "<indd48>0</indd48>");
		}

		public static ZString GetBooleanAsString(bool boolValue) => boolValue ? "1" : "0";

		public static ZString GetCodedYesNoString(string yesNoValue) => yesNoValue == "N" ? "0" : "1";

		public static ZString GetNumberAsString(INumericZType value) => value.ToString();

		public static ZString GetDecimalAsString(ZDecimal value) => value.ToString();
	}

	public class StringWriterWithEncoding : StringWriter
	{
		readonly Encoding encoding;

		public StringWriterWithEncoding(Encoding encoding, CultureInfo cultureInfo) : base(cultureInfo)
		{
			this.encoding = encoding;
		}

		public override Encoding Encoding
		{
			get { return encoding; }
		}
	}
}
