using System;
using System.Globalization;
using System.IO;
using System.Xml;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Utils;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.GB.Business
{
	public static class SerializationHelper
	{
		public static ZString Serialize(Type dataType, object dataObj)
		{
			var settings = new XmlWriterSettings();
			settings.OmitXmlDeclaration = true;
			settings.Indent = true;

			using var stream = new StringWriter(CultureInfo.InvariantCulture);
			using var xmlWriter = XmlWriter.Create(stream, settings);
			using var cleanXmlWriter = new CleanXmlWriter(xmlWriter);
			{
				var serializer = ZXmlSerializer.New(dataType);
				serializer.Serialize(cleanXmlWriter, dataObj);
				return stream.ToString().EscapeFullWidthXmlSpecialCharacters();
			}
		}

		public static ZString Serialize<T>(T dataObj) => Serialize(typeof(T), dataObj);

		/// <summary>
		/// Escapes all XML special characters in full-width version to avoid automatically converting to
		/// actual special characters ("'&lt;&gt;&amp;) when saving to database as VARCHAR instead of NVARCHAR
		/// </summary>
		public static string EscapeFullWidthXmlSpecialCharacters(this string xml)
		{
			return xml.Replace("\uFF02", "&quot;")
				.Replace("\uFF07", "&apos;")
				.Replace("\uFF1C", "&lt;")
				.Replace("\uFF1E", "&gt;")
				.Replace("\uFF06", "&amp;");
		}
	}
}
