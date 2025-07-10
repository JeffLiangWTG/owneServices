using System;
using System.Xml;
using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS
{
	public class DocumentUploadConfirmationResponse
	{
		public DocumentUploadConfirmationResponse(ZString xml)
		{
			try
			{
				xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(xml);
			}
			catch
			{
			}
		}

		readonly XmlDocument xmlDoc;

		public ZString FileName => xmlDoc?.SelectSingleNode(FileNameNodeXPath)?.InnerText ?? ZString.Empty;
		public ZBool IsSuccess => xmlDoc?.SelectSingleNode(OutcomeNodeXPath)?.InnerText == "SUCCESS";
		public ZString Details => xmlDoc?.SelectSingleNode(DetailsNodeXPath)?.InnerText ?? ZString.Empty;

		ZString FileNameNodeXPath => FormattableString.Invariant($"//*[local-name()='Root']/*[local-name()='FileName']");
		ZString OutcomeNodeXPath => FormattableString.Invariant($"//*[local-name()='Root']/*[local-name()='Outcome']");
		ZString DetailsNodeXPath => FormattableString.Invariant($"//*[local-name()='Root']/*[local-name()='Details']");
	}
}
