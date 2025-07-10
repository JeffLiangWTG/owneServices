using System.Collections.Generic;
using System.Xml;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS
{
	public class DeclarationSearchResponse : DeclarationInfoResponse
	{
		public DeclarationSearchResponse(ZString xml) : base(xml)
		{
		}

		protected override ZString RootNode => "DeclarationSearchResponse";

		public List<DeclarationSearchResponseDeclarationSearchDetails> DeclarationSearchDetails => GetStatusDetailsList(SelectNodes(DeclarationSearchDetailsNodeXPath));

		public ZString CurrentPageNumber => GetStringFromNode(xmlDoc, CurrentPageNumberXPath);
		public ZString TotalResultsAvailable => GetStringFromNode(xmlDoc, TotalResultsAvailableXPath);
		public ZString TotalPagesAvailable => GetStringFromNode(xmlDoc, TotalPagesAvailableXPath);

		List<DeclarationSearchResponseDeclarationSearchDetails> GetStatusDetailsList(XmlNodeList searchDetailsNodeList)
		{
			var searchDetails = new List<DeclarationSearchResponseDeclarationSearchDetails>();
			if (searchDetailsNodeList != null)
			{
				foreach (XmlNode declarationStatusDetailsNode in searchDetailsNodeList)
				{
					var details = new DeclarationSearchResponseDeclarationSearchDetails
					{
						Declaration = GetDeclarationSearchResponseDeclarationSearchDetailsDeclaration(declarationStatusDetailsNode.SelectSingleNode(DetailsDeclarationNodeXPath)),
						Declaration1 = GetDeclaration(declarationStatusDetailsNode.SelectSingleNode(DetailsDeclarationNodeXPath))
					};
					searchDetails.Add(details);
				}
			}
			return searchDetails;
		}

		DeclarationSearchResponseDeclarationSearchDetailsDeclaration GetDeclarationSearchResponseDeclarationSearchDetailsDeclaration(XmlNode declarationNode)
		{
			if (declarationNode != null)
			{
				return new DeclarationSearchResponseDeclarationSearchDetailsDeclaration
				{
					ID = new DeclarationIdentificationIDType1
					{
						Value = GetStringFromNode(declarationNode, IDCodeNodeXPath)
					},
					ReceivedDateTime = new DeclarationSearchResponseDeclarationSearchDetailsDeclarationReceivedDateTime
					{
						Item = new DeclarationSearchResponseDeclarationSearchDetailsDeclarationReceivedDateTimeDateTimeString
						{
							formatCode = FormatCodeType.Item304,
							Value = GetStringFromNode(declarationNode, ReceivedDateTimeCodeNodeXPath)
						}
					},
					ROE = GetStringFromNode(declarationNode, ROECodeNodeXPath),
					ICS = GetStringFromNode(declarationNode, ICSCodeNodeXPath),
					LRN = GetStringFromNode(declarationNode, LRNCodeNodeXPath)
				};
			}
			return new DeclarationSearchResponseDeclarationSearchDetailsDeclaration();
		}

		protected override CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData.Declaration GetDeclaration(XmlNode declarationNode)
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
					Submitter = new DeclarationSubmitter
					{
						ID = new SubmitterIdentificationIDType
						{
							Value = GetStringFromNode(nextSibling, SubmitterNodeXPath)
						}
					},
					Declarant = new DeclarationDeclarant
					{
						ID = new DeclarantIdentificationIDType
						{
							Value = GetStringFromNode(nextSibling, DeclarantNodeXPath)
						}
					},
					GoodsShipment = new DeclarationGoodsShipment
					{
						Consignment = new DeclarationGoodsShipmentConsignment
						{
							GoodsLocation = new DeclarationGoodsShipmentConsignmentGoodsLocation
							{
								Name = new GoodsLocationNameTextType
								{
									Value = GetStringFromNode(GetNodeContainedInNode(nextSibling, GoodsLocationNodeXPath), GoodsLocationNameNodeXPath)
								},
								TypeCode = new GoodsLocationTypeCodeType
								{
									Value = GetStringFromNode(GetNodeContainedInNode(nextSibling, GoodsLocationNodeXPath), TypeCodeNodeXPath)
								},
								Address = new DeclarationGoodsShipmentConsignmentGoodsLocationAddress
								{
									TypeCode = new AddressTypeCodeType
									{
										Value = GetStringFromNode(GetNodeContainedInNode(nextSibling, AddressNodeXPath), TypeCodeNodeXPath)
									},
									CountryCode = new AddressCountryCodeType
									{
										Value = GetStringFromNode(GetNodeContainedInNode(nextSibling, AddressNodeXPath), CountryCodeXPath)
									}
								}
							}
						},
						Importer = new DeclarationGoodsShipmentImporter
						{
							ID = new ImporterIdentificationIDType
							{
								Value = GetStringFromNode(GetNodeContainedInNode(nextSibling, ImporterNodeXPath), IDCodeNodeXPath)
							}
						}
					},
				};
			}
			return new CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData.Declaration();
		}

		protected ZString CurrentPageNumberXPath => System.FormattableString.Invariant($"*[local-name()='{RootNode}']/*[local-name()='CurrentPageNumber']");
		protected ZString TotalResultsAvailableXPath => System.FormattableString.Invariant($"*[local-name()='{RootNode}']/*[local-name()='TotalResultsAvailable']");
		protected ZString TotalPagesAvailableXPath => System.FormattableString.Invariant($"*[local-name()='{RootNode}']/*[local-name()='TotalPagesAvailable']");
		protected ZString DeclarationSearchDetailsNodeXPath => System.FormattableString.Invariant($"*[local-name()='{RootNode}']/*[local-name()='DeclarationSearchDetails']");
		protected virtual ZString AcceptanceDateTimeNodeXPath => System.FormattableString.Invariant($"//*[local-name()='{RootNode}']/*[local-name()='DeclarationSearchResponse']");
		protected ZString LRNCodeNodeXPath => System.FormattableString.Invariant($"*[local-name()='LRN']");
		protected ZString GoodsShipmentNodeXPath => System.FormattableString.Invariant($"*[local-name()='GoodsShipment']");
		protected ZString GoodsLocationNodeXPath => System.FormattableString.Invariant($"*[local-name()='GoodsShipment']/*[local-name()='Consignment']/*[local-name()='GoodsLocation']");
		protected ZString AddressNodeXPath => System.FormattableString.Invariant($"*[local-name()='GoodsShipment']/*[local-name()='Consignment']/*[local-name()='GoodsLocation']/*[local-name()='Address']");
		protected ZString GoodsLocationNameNodeXPath => System.FormattableString.Invariant($"*[local-name()='Name']");
		protected ZString CountryCodeXPath => System.FormattableString.Invariant($"*[local-name()='CountryCode']");
		protected ZString ImporterNodeXPath => System.FormattableString.Invariant($"*[local-name()='GoodsShipment']/*[local-name()='Importer']");
		protected ZString DeclarantNodeXPath => System.FormattableString.Invariant($"*[local-name()='Declarant']");
	}
}
