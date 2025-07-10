using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.FR
{
	public static class Extensions
	{
		public static ZString Serialize<T>(T dataObj)
		{
			var settings = new XmlWriterSettings();
			settings.Encoding = Encoding.UTF8;
			using (var stream = new StringWriterWithEncoding(settings.Encoding, CultureInfo.InvariantCulture))
			using (var writerMessage = XmlWriter.Create(stream, settings))
			{
				var serializerMessage = ZXmlSerializer.New(typeof(T));
				serializerMessage.Serialize(writerMessage, dataObj);
				return GetFlatXml(XmlHeader.Utf8 + CleanXML(stream.ToString()));
			}
		}

		public static ZString SerializeUsingUtf16<T>(T dataObj) => Serialize(dataObj).Replace(XmlHeader.Utf8, XmlHeader.Utf16);

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
			bool HasContent(XElement element)
			{
				if (!string.IsNullOrEmpty(element.Value))
				{
					return true;
				}

				foreach (var attribute in element.Attributes())
				{
					if (!string.IsNullOrEmpty(attribute.Value))
					{
						return true;
					}
				}

				foreach (var descendant in element.Descendants())
				{
					if (HasContent(descendant))
					{
						return true;
					}
				}

				return false;
			}

			XElement doc = XElement.Parse(xml);
			doc.Descendants().Where(e => !HasContent(e)).Remove();
			return doc.ToString();
		}

		public static bool EmptyApplicant(this IArticle article)
		{
			return article.ApplicantInwardNature.IsEmpty
				&& article.ApplicantDescription.IsEmpty
				&& article.ApplicantConditions.IsEmpty
				&& article.ApplicantPurOffice.IsEmpty
				&& article.ApplicantInwardLocation.IsEmpty
				&& article.ApplicantTransFormality.IsEmpty;
		}

		public static string GetFlatXml(string xmlString)
		{
			return xmlString.Replace("\r\n", "").Replace("  ", "").Replace("    ", "").Replace("\t", "");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "unstranslatable XML Header")]
		static class XmlHeader
		{
			internal const string Utf8 = @"<?xml version=""1.0"" encoding=""utf-8""?>";
			internal const string Utf16 = @"<?xml version=""1.0"" encoding=""utf-16""?>";
		}
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
