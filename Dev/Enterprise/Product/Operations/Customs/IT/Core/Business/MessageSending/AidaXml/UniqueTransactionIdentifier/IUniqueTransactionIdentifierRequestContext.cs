using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.UniqueTransactionIdentifier;

public interface IUniqueTransactionIdentifierRequestContext
{
	ZString UniqueTransactionID { get; }

	IGlbExternalPassword MauCertificate { get; }

	BusinessObject RequestParent { get; }

	ZString MessageNumber { get; }
}
