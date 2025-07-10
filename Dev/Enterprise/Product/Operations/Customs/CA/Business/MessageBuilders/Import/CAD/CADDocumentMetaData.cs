using CargoWise.Customs.CA.MessageContracts.CAD;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;

namespace Enterprise.Customs.CA.Business.MessageBuilders;

public class CADDocumentMetaData : ICADMessageMetaData
{
	public CADDocumentMetaData(JobDeclaration declaration, MessageSubTypes messageSubType, string versionID, CADCorrectionMessageSendingActionCollection amendmentActions)
	{
		this.declaration = declaration;
		this.messageSubType = messageSubType;
		this.amendmentActions = amendmentActions;
		this.versionID = versionID;
	}

	readonly ZString versionID;
	readonly JobDeclaration declaration;
	readonly MessageSubTypes messageSubType;
	readonly CADCorrectionMessageSendingActionCollection amendmentActions;

	#region ICADMessageMetaData

	string ICADMessageMetaData.ResponsibleAgencyName => "CBSA";

	string ICADMessageMetaData.AgencyAssignedCustomizationCode => "CAD";

	string ICADMessageMetaData.AgencyAssignedCustomizationVersionCode => "001";

	string ICADMessageMetaData.FunctionalDefinition => "CAD-IN";

	ICADMessageCommunicationMetaData ICADMessageMetaData.CommunicationMetaData
	{
		get
		{
			if (cadDocumentMetaDataCommunicationMetaData == null)
			{
				cadDocumentMetaDataCommunicationMetaData = new CADDocumentMetaDataCommunicationMetaData(declaration, versionID);
			}
			return cadDocumentMetaDataCommunicationMetaData;
		}
	}
	ICADMessageCommunicationMetaData cadDocumentMetaDataCommunicationMetaData;

	ICADMessageDeclaration ICADMessageMetaData.Declaration
	{
		get
		{
			if (cadDeclaration == null)
			{
				cadDeclaration = new CADDeclaration(declaration, messageSubType, versionID, this.amendmentActions);
			}
			return cadDeclaration;
		}
	}
	ICADMessageDeclaration cadDeclaration;

	#endregion
}
