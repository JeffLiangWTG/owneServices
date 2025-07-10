using CargoWise.Customs.CA.MessageContracts.CAD;

namespace Enterprise.Customs.CA.Business.MessageBuilders;

public class CADMessageCommunicationMetaDataSender : ICADMessageCommunicationMetaDataSender
{
	public CADMessageCommunicationMetaDataSender(string id, string roleCode)
	{
		this.id = id;
		this.roleCode = roleCode;
	}

	readonly string id;
	readonly string roleCode;

	#region ICADMessageCommunicationMetaDataSender

	string ICADMessageCommunicationMetaDataSender.ID => id;

	string ICADMessageCommunicationMetaDataSender.RoleCode => roleCode;

	#endregion
}
