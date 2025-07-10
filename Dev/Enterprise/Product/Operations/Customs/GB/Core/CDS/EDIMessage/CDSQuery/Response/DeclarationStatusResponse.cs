using System.Collections.Generic;
using System.Xml;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS
{
	public class DeclarationStatusResponse : DeclarationInfoResponse
	{
		public DeclarationStatusResponse(ZString xml) : base(xml)
		{
		}

		protected override ZString RootNode => "DeclarationStatusResponse";

		public List<DeclarationStatusResponseDeclarationStatusDetails> DeclarationStatusDetails => GetStatusDetailsList(SelectNodes(DeclarationStatusDetailsNodeXPath));

		List<DeclarationStatusResponseDeclarationStatusDetails> GetStatusDetailsList(XmlNodeList statusDetailsNodeList)
		{
			var statusDetails = new List<DeclarationStatusResponseDeclarationStatusDetails>();
			if (statusDetailsNodeList != null)
			{
				foreach (XmlNode declarationStatusDetailsNode in statusDetailsNodeList)
				{
					var details = new DeclarationStatusResponseDeclarationStatusDetails
					{
						Declaration = GetDeclarationStatusResponseDeclarationStatusDetailsDeclaration(declarationStatusDetailsNode.SelectSingleNode(DetailsDeclarationNodeXPath)),
						Declaration1 = GetDeclaration(declarationStatusDetailsNode.SelectSingleNode(DetailsDeclarationNodeXPath))
					};
					statusDetails.Add(details);
				}
			}
			return statusDetails;
		}

		DeclarationStatusResponseDeclarationStatusDetailsDeclaration GetDeclarationStatusResponseDeclarationStatusDetailsDeclaration(XmlNode declarationNode)
		{
			if (declarationNode != null)
			{
				return new DeclarationStatusResponseDeclarationStatusDetailsDeclaration
				{
					AcceptanceDateTime = new DeclarationAcceptanceDateTimeType
					{
						Item = new DeclarationAcceptanceDateTimeTypeDateTimeString
						{
							formatCode = FormatCodeType.Item304,
							Value = GetStringFromNode(declarationNode, AcceptanceDateTimeCodeNodeXPath)
						}
					},
					ID = new DeclarationIdentificationIDType1
					{
						Value = GetStringFromNode(declarationNode, IDCodeNodeXPath)
					},
					VersionID = new DeclarationVersionIDType
					{
						Value = GetStringFromNode(declarationNode, VersionIDCodeNodeXPath)
					},
					ReceivedDateTime = new DeclarationStatusResponseDeclarationStatusDetailsDeclarationReceivedDateTime
					{
						Item = new DeclarationStatusResponseDeclarationStatusDetailsDeclarationReceivedDateTimeDateTimeString
						{
							formatCode = FormatCodeType.Item304,
							Value = GetStringFromNode(declarationNode, ReceivedDateTimeCodeNodeXPath)
						}
					},
					GoodsReleasedDateTime = new DeclarationStatusResponseDeclarationStatusDetailsDeclarationGoodsReleasedDateTime
					{
						Item = new DeclarationStatusResponseDeclarationStatusDetailsDeclarationGoodsReleasedDateTimeDateTimeString
						{
							formatCode = FormatCodeType.Item304,
							Value = GetStringFromNode(declarationNode, GoodsReleasedDateTimeCodeNodeXPath)
						}
					},
					ROE = GetStringFromNode(declarationNode, ROECodeNodeXPath),
					ICS = GetStringFromNode(declarationNode, ICSCodeNodeXPath),
					IRC = GetStringFromNode(declarationNode, IRCCodeNodeXPath)
				};
			}
			return new DeclarationStatusResponseDeclarationStatusDetailsDeclaration();
		}
		
		protected ZString DeclarationStatusDetailsNodeXPath => System.FormattableString.Invariant($"*[local-name()='{RootNode}']/*[local-name()='DeclarationStatusDetails']");
		protected virtual ZString AcceptanceDateTimeNodeXPath => System.FormattableString.Invariant($"//*[local-name()='{RootNode}']/*[local-name()='DeclarationStatusResponse']");
		protected ZString IRCCodeNodeXPath => System.FormattableString.Invariant($"*[local-name()='IRC']");
	}
}
		
