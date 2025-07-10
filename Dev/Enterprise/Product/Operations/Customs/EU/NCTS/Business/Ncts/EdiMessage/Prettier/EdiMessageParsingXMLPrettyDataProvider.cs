using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using static System.FormattableString;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class EdiMessageParsingXMLPrettyDataProvider : INCTSPrettierData
	{
		public EdiMessageParsingXMLPrettyDataProvider(EDIMessage message)
		{
			Message = message ?? throw new ArgumentNullException(nameof(message));

			try
			{
				xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(message.EM_MessageText);
				LoadXmlFromCDataNodeIfApplicable(xmlDoc?.SelectSingleNode((ZArchitecture.Core.NoResString)"//*[local-name()='Event']/*[local-name()='ContextCollection']/*[local-name()='Context']/*[local-name()='Value']"));
			}
			catch (XmlException) { }
		}

		void LoadXmlFromCDataNodeIfApplicable(XmlNode node)
		{
			if (node != null && node.LastChild?.NodeType == XmlNodeType.CDATA)
			{
				xmlDoc.InnerXml = node.LastChild.Value ?? ZString.Empty;
			}
		}

		readonly XmlDocument xmlDoc;
		public EDIMessage Message { get; }
		public ZString MessageType => SelectSingleNodeInnerText(MessageTypeXPath);
		public ZString MessageRecipient => SelectSingleNodeInnerText(MessageRecipientXPath);
		public ZString LRN => SelectSingleNodeInnerText(LRNXPath);
		public ZString MRN => SelectSingleNodeInnerText(MRNXPath);
		public ZString CustomsOfficeOfDeparture => SelectSingleNodeInnerText(CustomsOfficeOfDepartureXPath);
		public ZString CustomsOfficeOfDestination => SelectSingleNodeInnerText(CustomsOfficeOfDestinationXPath);
		public ZString DeclarationType => SelectSingleNodeInnerText(DeclarationTypeXPath);
		public ZString AdditionalDeclarationType => SelectSingleNodeInnerText(AdditionalDeclarationTypeXPath);
		public ZString ReducedDatasetIndicator => SelectSingleNodeInnerText(ReducedDatasetIndicatorXPath);
		public ZString SimplifiedProcedure => SelectSingleNodeInnerText(SimplifiedProcedureXPath);
		public ZString Security => SelectSingleNodeInnerText(SecurityXPath);
		public ZString BindingItinerary => SelectSingleNodeInnerText(BindingItineraryXPath);
		public ZString PrincipleEORI => SelectSingleNodeInnerText(PrincipleEORIXPath);
		public ZString RepresentativeEORI => SelectSingleNodeInnerText(RepresentativeEORIXPath);
		public ZString ConsigneeEORI => SelectSingleNodeInnerText(ConsigneeEORIXPath);
		public ZString ConsignorEORI => SelectSingleNodeInnerText(ConsignorEORIXPath);
		public IReadOnlyCollection<INCTSPrettierGuaranteeData> Guarantees => GetGuarantees();
		public IReadOnlyCollection<INCTSPrettierConsignmentData> Consignments => GetConsignments();

		public void FillSharedFields(NCTSPrettierSharedFields sharedFields) => FillSharedFieldsCore(sharedFields);

		protected virtual void FillSharedFieldsCore(NCTSPrettierSharedFields sharedFields)
		{
			sharedFields.MessageType = MessageType;
			sharedFields.DeclarationType = DeclarationType;
			sharedFields.AdditionalDeclarationType = AdditionalDeclarationType;
			sharedFields.LRN = LRN;
			sharedFields.MRN = MRN;
			sharedFields.CustomsOfficeOfDeparture = CustomsOfficeOfDeparture;
			sharedFields.CustomsOfficeOfDestination = CustomsOfficeOfDestination;
			sharedFields.ReducedDatasetIndicator = ReducedDatasetIndicator;
			sharedFields.SimplifiedProcedure = SimplifiedProcedure;
			sharedFields.Security = Security;
			sharedFields.BindingItinerary = BindingItinerary;
			sharedFields.PrincipleEORI = PrincipleEORI;
			sharedFields.RepresentativeEORI = RepresentativeEORI;
			sharedFields.ConsigneeEORI = ConsigneeEORI;
			sharedFields.ConsignorEORI = ConsignorEORI;
		}

		public IReadOnlyCollection<INCTSPrettierAdditionalBlock> AdditionalBlocks => AdditionalBlocksCore;

		protected virtual IReadOnlyCollection<INCTSPrettierAdditionalBlock> AdditionalBlocksCore => new INCTSPrettierAdditionalBlock[] {
			new NCTSPrettierGuaranteesTable(GetGuarantees()),
			new NCTSPrettierConsignmentsTable(GetConsignments()),
		};

		protected virtual IReadOnlyCollection<INCTSPrettierGuaranteeData> GetGuarantees()
			=> SelectNodes(GuaranteeXPath).Select(node => new NCTSPrettierGuaranteeData(
				GetNodeInnerText(node, GuaranteeTypeNodeXPath),
				GetNodeInnerText(node, GuaranteeNumberNodeXPath),
				GetNodeInnerText(node, GuaranteeOtherNumberNodeXPath),
				GetNodeInnerText(node, GuaranteeAmountNodeXPath),
				GetNodeInnerText(node, GuaranteeCurrencyNodeXPath)))
				.ToArray();

		protected virtual IReadOnlyCollection<INCTSPrettierConsignmentData> GetConsignments()
		{
			var list = new List<INCTSPrettierConsignmentData>();
			var consignmentNodes = SelectNodes(ConsignmentXPath);
			foreach (var consignmentNode in consignmentNodes)
			{
				foreach (var houseNode in SelectNodes(consignmentNode, HouseConsignmentNodeXPath))
				{
					var goodsItems = GetGoodsItems(SelectNodes(houseNode, HouseConsignmentItemNodeXPath));
					list.Add(new NCTSPrettierConsignmentData(GetNodeInnerText(houseNode, HouseConsignmentUCRReferenceNodeXPath), goodsItems));
				}
			}
			return list;
		}

		protected virtual IReadOnlyCollection<INCTSPrettierGoodsItemData> GetGoodsItems(IEnumerable<XmlNode> nodes)
			=> nodes.Select(node => new NCTSPrettierGoodsItemData(
				GetNodeInnerText(node, HouseConsignmentItemNumberNodeXPath),
				GetNodeInnerText(node, HouseConsignmentItemUCRReferenceNodeXPath),
				GetNodeInnerText(node, HouseConsignmentItemCommodityDescriptionNodeXPath),
				GetNodeInnerText(node, HouseConsignmentItemCommodityCodeHarmonizedSubHeadingCodeNodeXPath)))
				.ToArray();

		#region XPath
		protected ZString SelectSingleNodeInnerText(string xPath) => xmlDoc?.SelectSingleNode(xPath)?.InnerText ?? ZString.Empty;
		protected ZString GetNodeInnerText(IXPathNavigable node, string xPath) => (node as XmlNode)?.SelectSingleNode(xPath)?.InnerText ?? ZString.Empty;
		protected IEnumerable<XmlNode> SelectNodes(string xpath) => xmlDoc?.SelectNodes(xpath)?.Cast<XmlNode>() ?? Array.Empty<XmlNode>();
		protected IEnumerable<XmlNode> SelectNodes(IXPathNavigable node, string xpath) => (node as XmlNode)?.SelectNodes(xpath)?.Cast<XmlNode>() ?? Array.Empty<XmlNode>();

		protected virtual string MessageTypeXPath => Invariant($"//*[local-name()='messageType']");
		protected virtual string MessageRecipientXPath => Invariant($"//*[local-name()='messageRecipient']");
		protected virtual string LRNXPath => Invariant($"//*[local-name()='TransitOperation']/*[local-name()='LRN']");
		protected virtual string MRNXPath => Invariant($"//*[local-name()='TransitOperation']/*[local-name()='MRN']");
		protected virtual string CustomsOfficeOfDepartureXPath => Invariant($"//*[local-name()='CustomsOfficeOfDeparture']/*[local-name()='referenceNumber']");
		protected virtual string CustomsOfficeOfDestinationXPath => Invariant($"//*[local-name()='CustomsOfficeOfDestinationDeclared']/*[local-name()='referenceNumber']");
		protected virtual string DeclarationTypeXPath => Invariant($"//*[local-name()='TransitOperation']/*[local-name()='declarationType']");
		protected virtual string AdditionalDeclarationTypeXPath => Invariant($"//*[local-name()='TransitOperation']/*[local-name()='additionalDeclarationType']");
		protected virtual string ReducedDatasetIndicatorXPath => Invariant($"//*[local-name()='TransitOperation']/*[local-name()='reducedDatasetIndicator']");
		protected virtual string SimplifiedProcedureXPath => Invariant($"//*[local-name()='TransitOperation']/*[local-name()='simplifiedProcedure']");
		protected virtual string SecurityXPath => Invariant($"//*[local-name()='TransitOperation']/*[local-name()='security']");
		protected virtual string BindingItineraryXPath => Invariant($"//*[local-name()='TransitOperation']/*[local-name()='bindingItinerary']");
		protected virtual string PrincipleEORIXPath => Invariant($"//*[local-name()='HolderOfTheTransitProcedure']/*[local-name()='identificationNumber']");
		protected virtual string RepresentativeEORIXPath => Invariant($"//*[local-name()='Representative']/*[local-name()='identificationNumber']");
		protected virtual string ConsigneeEORIXPath => Invariant($"//*[local-name()='Consignee']/*[local-name()='identificationNumber']");
		protected virtual string ConsignorEORIXPath => Invariant($"//*[local-name()='Consignor']/*[local-name()='identificationNumber']");

		protected virtual string GuaranteeXPath => Invariant($"//*[local-name()='Guarantee']");
		protected virtual string GuaranteeTypeNodeXPath => Invariant($"*[local-name()='guaranteeType']");
		protected virtual string GuaranteeNumberNodeXPath => Invariant($"*[local-name()='GuaranteeReference']/*[local-name()='GRN']");
		protected virtual string GuaranteeOtherNumberNodeXPath => Invariant($"*[local-name()='otherGuaranteeReference']");
		protected virtual string GuaranteeAmountNodeXPath => Invariant($"*[local-name()='GuaranteeReference']/*[local-name()='amountToBeCovered']");
		protected virtual string GuaranteeCurrencyNodeXPath => Invariant($"*[local-name()='GuaranteeReference']/*[local-name()='currency']");

		protected virtual string ConsignmentXPath => Invariant($"//*[local-name()='Consignment']");
		protected virtual string HouseConsignmentNodeXPath => Invariant($"*[local-name()='HouseConsignment']");
		protected virtual string HouseConsignmentUCRReferenceNodeXPath => Invariant($"*[local-name()='referenceNumberUCR']");
		protected virtual string HouseConsignmentItemNodeXPath => Invariant($"*[local-name()='ConsignmentItem']");
		protected virtual string HouseConsignmentItemNumberNodeXPath => Invariant($"*[local-name()='declarationGoodsItemNumber']");
		protected virtual string HouseConsignmentItemUCRReferenceNodeXPath => Invariant($"*[local-name()='referenceNumberUCR']");
		protected virtual string HouseConsignmentItemCommodityDescriptionNodeXPath => Invariant($"*[local-name()='Commodity']/*[local-name()='descriptionOfGoods']");
		protected virtual string HouseConsignmentItemCommodityCodeHarmonizedSubHeadingCodeNodeXPath => Invariant($"*[local-name()='Commodity']/*[local-name()='CommodityCode']/*[local-name()='harmonizedSystemSubHeadingCode']");
		#endregion
	}
}
