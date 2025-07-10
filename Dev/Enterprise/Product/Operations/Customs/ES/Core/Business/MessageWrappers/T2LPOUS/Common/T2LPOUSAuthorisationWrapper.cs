using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using static Enterprise.Customs.ES.Business.ESConstants;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class T2LPOUSAuthorisationWrapper : IT2LPOUSAuthorisation
{
	public T2LPOUSAuthorisationWrapper(CusAuthorizationUsage authorization)
	{
		this.authorization = Argument.NotNull(authorization, nameof(authorization));
	}
	readonly CusAuthorizationUsage authorization;

	public ZString TypeOfAuthorisation => GetMappedType();

	public ZString DecisionReferenceNumber => authorization.AGC_Number;

	public ZString HolderOfTheAuthorisation => authorization.Owner.GetIDCode();

	ZString GetMappedType()
	{
		var type = authorization.AGC_Code;
		return type == CusAuthorizationHeaderTypeList.Codes.AuthorizedIssuer ? (ZString)AESAuthorizationCodes.C511 : type;
	}
}
