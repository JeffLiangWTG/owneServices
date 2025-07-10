using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS
{
	public abstract class DeclarationInfoResponse
	{
		protected readonly XmlDocument xmlDoc;

		public DeclarationInfoResponse(ZString xml)
		{
			XML = xml;
			xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(XML);
		}

		public static DeclarationInfoResponse GetResponse(ZString xml)
		{
			var response = xml.Contains("DeclarationSearchResponse") ? new DeclarationSearchResponse(xml)
				: new DeclarationStatusResponse(xml) as DeclarationInfoResponse;

			return response;
		}
		public static CDSEDIMessagePrettier GetPrettier(CDSDeclarationInfoResponseEDIMessage responseMessage)
		{
			var prettier = responseMessage.MessageDataObject is DeclarationSearchResponse ? new CDSDeclarationSearchResponseEDIMessagePrettier(responseMessage)
				: new CDSDeclarationInfoResponseEDIMessagePrettier(responseMessage) as CDSEDIMessagePrettier
				;
			return prettier;
		}

		protected abstract ZString RootNode { get; }

		protected ZString SelectSingleNode(ZString xpath) => xmlDoc.SelectSingleNode(xpath)?.InnerText ?? ZString.Empty;
		protected XmlNodeList SelectNodes(ZString xpath) => xmlDoc.SelectNodes(xpath);
		protected XmlNodeList GetNodeListContainedInNode(IXPathNavigable node, ZString xPath) => (node as XmlNode).SelectNodes(xPath);
		protected IXPathNavigable GetNodeContainedInNode(IXPathNavigable node, ZString xPath) => (node as XmlNode).SelectSingleNode(xPath);
		protected IXPathNavigable SelectSingleXmlNode(ZString xpath) => xmlDoc.SelectSingleNode(xpath);
		protected ZString GetStringFromNode(IXPathNavigable node, ZString xPath) => (ZString)((node as XmlNode).SelectSingleNode(xPath)?.InnerText ?? string.Empty);
		protected ZDecimal GetDecimalFromNode(IXPathNavigable node, ZString xPath) => ZDecimal.ParseSafe((node as XmlNode).SelectSingleNode(xPath)?.InnerText, ZDecimal.Zero);

		public ZString XML { get; }

		protected virtual CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData.Declaration GetDeclaration(XmlNode declarationNode)
		{
			var nextSibling = declarationNode.NextSibling;
			if (declarationNode != null)
			{
				return new CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData.Declaration
				{
					FunctionCode = new DeclarationFunctionCodeType
					{
						Value = GetStringFromNode(nextSibling, FunctionCodeNodeXPath)
					},
					TypeCode = new DeclarationTypeCodeType
					{
						Value = GetStringFromNode(nextSibling, TypeCodeNodeXPath)
					},
					GoodsItemQuantity = new DeclarationGoodsItemQuantityType
					{
						Value = GetDecimalFromNode(nextSibling, GoodsItemQuantityNodeXPath)
					},
					TotalPackageQuantity = new DeclarationTotalPackageQuantityType
					{
						Value = GetDecimalFromNode(nextSibling, TotalPackageQuantityNodeXPath)
					},
					Submitter = new DeclarationSubmitter
					{
						ID = new SubmitterIdentificationIDType
						{
							Value = GetStringFromNode(nextSibling, SubmitterNodeXPath)
						}
					},
					GoodsShipment = new DeclarationGoodsShipment
					{
						PreviousDocument = GetPreviousDocList(nextSibling).ToArray(),
						UCR = new DeclarationGoodsShipmentUCR
						{
							TraderAssignedReferenceID = new UCRTraderAssignedReferenceIDType
							{
								Value = GetStringFromNode(GetNodeContainedInNode(nextSibling, UCRNodeXPath), TraderAssignedReferenceIDNodeXPath)
							}
						}
					}
				};
			}
			return new CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData.Declaration();
		}

		List<DeclarationGoodsShipmentPreviousDocument> GetPreviousDocList(XmlNode declaration)
		{
			var docsNodeList = GetNodeListContainedInNode(declaration, PreviousDocumentsNodeXPath);
			var docsList = new List<DeclarationGoodsShipmentPreviousDocument>();
			if (docsNodeList != null)
			{
				foreach (XmlNode docNode in docsNodeList)
				{
					var previousDoc = new DeclarationGoodsShipmentPreviousDocument
					{
						ID = new PreviousDocumentIdentificationIDType
						{
							Value = GetStringFromNode(docNode, DocIDTypeCodeNodeXPath),
						},
						TypeCode = new PreviousDocumentTypeCodeType
						{
							Value = GetStringFromNode(docNode, DocTypeCodeNodeXPath)
						}
					};
					docsList.Add(previousDoc);
				}
			}
			return docsList;
		}

		protected ZString DetailsNodeXPath => System.FormattableString.Invariant($"*[local-name()='{RootNode}']/*[local-name()='children']");
		protected ZString DetailsDeclarationNodeXPath => System.FormattableString.Invariant($"*[local-name()='Declaration']");
		protected ZString AcceptanceDateTimeCodeNodeXPath => System.FormattableString.Invariant($"*[local-name()='AcceptanceDateTime']");
		protected ZString IDCodeNodeXPath => System.FormattableString.Invariant($"*[local-name()='ID']");
		protected ZString VersionIDCodeNodeXPath => System.FormattableString.Invariant($"*[local-name()='VersionID']");
		protected ZString ReceivedDateTimeCodeNodeXPath => System.FormattableString.Invariant($"*[local-name()='ReceivedDateTime']");
		protected ZString GoodsReleasedDateTimeCodeNodeXPath => System.FormattableString.Invariant($"*[local-name()='GoodsReleasedDateTime']");
		protected ZString ROECodeNodeXPath => System.FormattableString.Invariant($"*[local-name()='ROE']");
		protected ZString ICSCodeNodeXPath => System.FormattableString.Invariant($"*[local-name()='ICS']");
		protected ZString FunctionCodeNodeXPath => System.FormattableString.Invariant($"*[local-name()='FunctionCode']");
		protected ZString TypeCodeNodeXPath => System.FormattableString.Invariant($"*[local-name()='TypeCode']");
		protected ZString GoodsItemQuantityNodeXPath => System.FormattableString.Invariant($"*[local-name()='GoodsItemQuantity']");
		protected ZString TotalPackageQuantityNodeXPath => System.FormattableString.Invariant($"*[local-name()='TotalPackageQuantity']");
		protected ZString SubmitterNodeXPath => System.FormattableString.Invariant($"*[local-name()='Submitter']");
		protected ZString PreviousDocumentsNodeXPath => System.FormattableString.Invariant($"*[local-name()='GoodsShipment']/*[local-name()='PreviousDocument']");
		protected ZString DocTypeCodeNodeXPath => System.FormattableString.Invariant($"*[local-name()='TypeCode']");
		protected ZString DocIDTypeCodeNodeXPath => System.FormattableString.Invariant($"*[local-name()='ID']");
		protected ZString UCRNodeXPath => System.FormattableString.Invariant($"*[local-name()='GoodsShipment']/*[local-name()='UCR']");
		protected ZString TraderAssignedReferenceIDNodeXPath => System.FormattableString.Invariant($"*[local-name()='TraderAssignedReferenceID']");
	}
}
