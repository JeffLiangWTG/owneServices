using System.Data;
using System.Xml;
using System.Xml.XPath;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.CDS.Helpers;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSInterchange : EDIInterchange
	{
		public CDSInterchange(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static CDSInterchange CreateCdsInterchangeFromXML(BusinessObjectFactory factory, IXPathNavigable xmlDocument)
		{
			CDSInterchange interchange = null;

			var xmlDoc = ((XmlDocument)xmlDocument);

			if (xmlDoc.SelectSingleNode("processing-instruction('ccsuk')") is XmlProcessingInstruction processingInstruction)
			{
				xmlDoc.RemoveChild(processingInstruction);

				if (xmlDoc.FirstChild.NodeType == XmlNodeType.XmlDeclaration)
				{
					xmlDoc.RemoveChild(xmlDoc.FirstChild);
				}

				var ccsukProcessingInstructionHelper = new CCSUKProcessingInstructionHelper(processingInstruction.Data);

				interchange = factory.New<CDSInterchange>();
				interchange.EI_ReceiveTransmit = EDIInterchange.Status.Received;
				interchange.EI_Status = EDIInterchange.Status.Queued;
				interchange.EI_BodyText = new GBCustomsBusinessResponse(ccsukProcessingInstructionHelper.ConversationId, xmlDoc.OuterXml).Xml;
				interchange.EI_From = ccsukProcessingInstructionHelper.SenderId;
				interchange.EI_To = ccsukProcessingInstructionHelper.RecipientId;
				interchange.EI_FooterText = processingInstruction.OuterXml;
			}

			return interchange;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EI_ApplicationCode = ApplicationCodes.GbCustomsDeclarationServices;
		}

		public CCSUKProcessingInstructionHelper CCSUKProcessingInstructionHelper
		{
			get
			{
				CCSUKProcessingInstructionHelper result = null;

				var footerText = EI_FooterText;
				if (!footerText.IsEmpty)
				{
					result = new CCSUKProcessingInstructionHelper(EI_FooterText);
				}

				return result;
			}
		}

		public GBCustomsBusinessResponse GBCustomsBusinessResponse => new GBCustomsBusinessResponse(EI_BodyText);

		protected override bool ShouldSendViaEHubCore => true;
	}
}
