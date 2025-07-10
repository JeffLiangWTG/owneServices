using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

public interface IMessageSendingObject
{
	BusinessObjectFactory Factory { get; }
	ZString ApplicationCode { get; }
	ZString MessageTypeForEDIMessage { get; }
	ZString MessageSubTypeForEDIMessage { get; }
	ZString ToMessageString();
	ZString GetApplicationReference();
	ZGuid GetCredentialPK();
}
