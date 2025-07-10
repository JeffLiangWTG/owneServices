using System.Collections.Generic;
using CargoWise.Customs.CA.MessageContracts.CAD;
using Enterprise.Customs.CA.Registry;

namespace Enterprise.Customs.CA.Business.MessageBuilders;

public class CADDocumentMetaDataCommunicationMetaData : ICADMessageCommunicationMetaData
{
	public CADDocumentMetaDataCommunicationMetaData(JobDeclaration declaration, string versionID)
	{
		this.declaration = declaration;
		this.versionID = versionID;
	}

	readonly JobDeclaration declaration;
	readonly string versionID;
	const string WTGBusinessNumberRoleCode = "HN";
	const string BrokerBusinessNumberRoleCode = "AK";

	#region ICADMessageCommunicationMetaData

	string ICADMessageCommunicationMetaData.ApplicationReferenceID
	{
		get
		{
			var transactionNo = declaration.ParentRelatedDeclaration != null ? (string)declaration.CA_OriginalTransactionNo : declaration.TransactionNumber.AccountSecurityCode + declaration.TransactionNumber.UniqueIdentifier;
			return transactionNo + versionID + (declaration.GetSentCADMessageCountInThisVersion(versionID) + 1).ToString().PadLeft(3, '0');
		}
	}

	IEnumerable<ICADMessageCommunicationMetaDataSender> ICADMessageCommunicationMetaData.Senders
	{
		get
		{
			yield return new CADMessageCommunicationMetaDataSender(CACustomsDataRegistry.Instance.WTGBusinessNumber.Value, WTGBusinessNumberRoleCode);
			yield return new CADMessageCommunicationMetaDataSender(declaration.BrokerBusinessNumber, BrokerBusinessNumberRoleCode);
		}
	}

	#endregion
}
