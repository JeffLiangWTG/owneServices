using System.Text;
using System.Xml;

namespace CargoWise.ResourceStrings.Cache.Testing
{
	public class TranslationErrorFormatter
	{
		public TranslationErrorFormatter()
		{
			errorBuilder = new StringBuilder();
			writer = XmlWriter.Create(errorBuilder, new XmlWriterSettings { OmitXmlDeclaration = true });
		}

		readonly StringBuilder errorBuilder;
		readonly XmlWriter writer;

		public void AddTranslationError(string language, string key, string message, string source, string translation)
		{
			if (errorBuilder.Length == 0)
			{
				writer.WriteStartElement("script");
				writer.WriteAttributeString("id", "translationErrors");
				writer.WriteAttributeString("type", "text/xmldata");
				writer.WriteStartElement("TranslationErrors");
			}
			writer.WriteStartElement("TranslationError");
			writer.WriteStartElement("ErrorMessage");
			writer.WriteString(message);
			writer.WriteEndElement();
			writer.WriteStartElement("Source");
			writer.WriteStartElement("Res");
			writer.WriteStartElement("Key");
			writer.WriteString(key);
			writer.WriteEndElement();
			writer.WriteStartElement("Caption");
			writer.WriteString(source);
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteStartElement("Translation");
			writer.WriteAttributeString("Language", language);
			writer.WriteStartElement("Res");
			writer.WriteStartElement("Key");
			writer.WriteString(key);
			writer.WriteEndElement();
			writer.WriteStartElement("Caption");
			writer.WriteString(translation);
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.Flush();
		}

		public string GetFormattedTranslationError()
		{
			try
			{
				writer.WriteEndElement();
				writer.WriteEndElement();
				writer.Flush();
				return "Machine readable resource string error data recorded here, check other failure messages for error details.\r\n" + errorBuilder.ToString();
			}
			finally
			{
				writer.Dispose();
			}
		}

		public bool HasError
		{
			get
			{
				return errorBuilder.Length > 0;
			}
		}
	}
}
