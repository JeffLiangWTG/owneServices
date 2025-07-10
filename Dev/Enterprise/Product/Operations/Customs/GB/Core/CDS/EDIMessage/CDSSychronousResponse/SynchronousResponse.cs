using System.Xml;
using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS
{
	public class SynchronousResponse
	{
		readonly XmlDocument xmlDoc;

		public SynchronousResponse(ZString xml)
		{
			XML = xml;
			xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(XML);
		}

		public ZString Status => xmlDoc.SelectSingleNode(StatusNodeXPath)?.InnerText ?? ZString.Empty;

		public ZString Code => xmlDoc.SelectSingleNode(CodeNodeXPath)?.InnerText ?? ZString.Empty;

		public ZString Description => xmlDoc.SelectSingleNode(DescriptionodeXPath)?.InnerText ?? ZString.Empty;

		public ZBool IsAccepted => Status == "202";

		public ZString XML { get; }

		const string StatusNodeXPath = "//*[local-name()='SynchronousResponse']/*[local-name()='status']";
		const string CodeNodeXPath = "//*[local-name()='SynchronousResponse']/*[local-name()='code']";
		const string DescriptionodeXPath = "//*[local-name()='SynchronousResponse']/*[local-name()='description']";
	}
}
