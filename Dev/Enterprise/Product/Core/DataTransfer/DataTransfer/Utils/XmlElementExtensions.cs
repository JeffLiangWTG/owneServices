using System.IO;
using System.Text;
using System.Xml;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Xml
{
	public static class XmlElementExtensions
	{
		public static string OuterXmlLowMemory(this XmlElement element)
		{
			return LowMemoryHelper(element.WriteTo);
		}

		public static string InnerXmlLowMemory(this XmlElement element)
		{
			return LowMemoryHelper(element.WriteContentTo);
		}

		public static XmlReader CreateReader(this XmlElement element)
		{
			Stream tempFileStream = TempFile.CreateWithDeleteOnClose(FileOptions.SequentialScan);
			var settings = new XmlWriterSettings()
			{
				ConformanceLevel = ConformanceLevel.Fragment,
				Encoding = Encoding.UTF8
			};
			using (var writer = XmlWriter.Create(tempFileStream, settings))
			{
				RemoveEmptyNamespaceIfFound(element);
				element.WriteTo(writer);
			}

			tempFileStream.Position = 0;
			return new XmlTextReader(tempFileStream);
		}

		/// <summary>
		/// Addition of an empty namespace within an object that already has namespaces is invalid
		/// This will remove from the xml for successful writing using the xmlWriter
		/// </summary>
		/// <param name="element"></param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		static void RemoveEmptyNamespaceIfFound(XmlElement element)
		{
			if (element.Attributes.Count > 0)
			{
				var attribute = element.Attributes[0];
				if (attribute.Name == "xmlns" && string.IsNullOrEmpty(attribute.Value) && element.NamespaceURI != attribute.Value)
				{
					element.RemoveAttributeAt(0);
				}
			}
		}

		delegate void WriteContentDelegate(XmlWriter writer);

		static string LowMemoryHelper(WriteContentDelegate methodDelegate)
		{
			using (Stream tempFileStream = TempFile.CreateWithDeleteOnClose(FileOptions.SequentialScan))
			{
				var settings = new XmlWriterSettings()
				{
					CheckCharacters = false,
					ConformanceLevel = ConformanceLevel.Fragment,
				};
				using (var writer = XmlWriter.Create(tempFileStream, settings))
				{
					methodDelegate(writer);
				}
				tempFileStream.Position = 0;
				using (var sr = new StreamReader(tempFileStream))
				{
					return sr.ReadToEnd();
				}
			}
		}
	}
}
