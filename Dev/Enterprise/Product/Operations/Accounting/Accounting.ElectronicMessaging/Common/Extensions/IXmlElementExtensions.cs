using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public static class IXmlElementExtensions
	{
		public static string StreamToString(this IXmlElement xElement, XmlWriterSettings settings = null)
		{
			using (var ms = new MemoryStream())
			{
				xElement.WriteToXmlStream(ms, settings);
				ms.Position = 0;
				using (var reader = new StreamReader(ms, false))
				{
					return reader.ReadToEnd();
				}
			}
		}

		public static void Add(this List<IXmlElementWithSequence> xmlElementWithSequences, IXmlElement element)
		{
			if (element != null && xmlElementWithSequences != null)
			{
				var sequence = xmlElementWithSequences.Count + 1;
				var sequencedXmlElement = new IXmlElementWithSequence(element, sequence);
				xmlElementWithSequences.Add(sequencedXmlElement);
			}
		}
	}
}
