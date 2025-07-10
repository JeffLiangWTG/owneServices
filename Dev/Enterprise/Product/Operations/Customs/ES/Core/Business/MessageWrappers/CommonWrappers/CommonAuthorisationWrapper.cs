using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class CommonAuthorisationWrapper : ICommonAuthorisation
{
	public CommonAuthorisationWrapper(CusAuthorizationUsage authorization, ZShort seqNum)
	{
		this.authorization = Argument.NotNull(authorization, nameof(authorization));

		SequenceNumber = seqNum.ToString();
	}
	readonly CusAuthorizationUsage authorization;

	public ZString SequenceNumber { get; }

	public ZString Type => authorization.CustomsCode.IsEmpty ? authorization.AGC_Code : authorization.CustomsCode;

	public ZString ReferenceNumber => authorization.AGC_Number;

	public ZString Holder => GetHolder();

	ZString GetHolder()
	{
		var holder = ZString.Empty;
		var type = authorization.AGC_Code;
		if (type == ESCusAuthorisationHeaderTypeList.Codes.ApplicationOrDecisionRelatingToBindingOriginInformation
			|| type == ESCusAuthorisationHeaderTypeList.Codes.ApplicationOrDecisionRelatingToBindingTariffInformation)
		{
			holder = OrgHeaderExtension.GetIDCode(authorization.Owner);
		}
		return holder;
	}
}
