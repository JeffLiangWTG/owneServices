using System.IO;
using System.Linq;
using System.Xml;

namespace CargoWise.Common
{
	/// <summary>
	/// Contains the utility methods for XML manipulations.
	/// </summary>
	public static class XmlUtils
	{
		/// <summary>
		/// Checks whether an XML text contains invalid characters.
		/// </summary>
		/// <param name="xmlText">The XML text.</param>
		/// <returns>Whether the XML text contains invalid characters.</returns>
		public static bool IsValidXml(string xmlText)
		{
			Argument.NotNull(xmlText, nameof(xmlText));
			var result = xmlText.All(XmlConvert.IsXmlChar);
			if (result)
			{
				var xmlDocument = new XmlDocument();
				try
				{
					xmlDocument.LoadXml(xmlText);
				}
				catch (XmlException)
				{
					result = false;
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		public static bool IsValidXml(string xmlText, out XmlDocument xmlDocument)
		{
			Argument.NotNull(xmlText, nameof(xmlText));
			var result = xmlText.All(XmlConvert.IsXmlChar);
			xmlDocument = new XmlDocument();
			if (result)
			{
				try
				{
					xmlDocument.LoadXml(xmlText);
				}
				catch (XmlException)
				{
					result = false;
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		public static bool IsValidXml(TextReader textReader, out XmlDocument xmlDocument)
		{
			Argument.NotNull(textReader, nameof(textReader));
			var result = true;
			xmlDocument = new XmlDocument();
			try
			{
				xmlDocument.Load(textReader);
			}
			catch (XmlException)
			{
				result = false;
			}
			return result;
		}
	}
}
