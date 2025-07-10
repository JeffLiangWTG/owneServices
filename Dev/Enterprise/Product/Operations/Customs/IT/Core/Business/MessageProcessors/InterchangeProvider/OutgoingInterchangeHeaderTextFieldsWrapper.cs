using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

public class OutgoingInterchangeHeaderTextFieldsWrapper : IOutgoingInterchangeHeaderTextFieldsProvider
{
	public OutgoingInterchangeHeaderTextFieldsWrapper(EDIInterchange interchange, CustomsInterchangeAndAccountInfo customsInterchangeAndAccountInfo)
	{
		this.interchange = Argument.NotNull(interchange, nameof(interchange));
		this.customsInterchangeAndAccountInfo = Argument.NotNull(customsInterchangeAndAccountInfo, nameof(customsInterchangeAndAccountInfo));
	}

	readonly EDIInterchange interchange;
	readonly CustomsInterchangeAndAccountInfo customsInterchangeAndAccountInfo;

	ZString IOutgoingInterchangeHeaderTextFieldsProvider.Staff => customsInterchangeAndAccountInfo.Staff;

	ZString IOutgoingInterchangeHeaderTextFieldsProvider.Node => customsInterchangeAndAccountInfo.Header?.AuthorizedUserCode ?? ZString.Empty;

	ZString IOutgoingInterchangeHeaderTextFieldsProvider.MessageType => interchange.EI_InterchangeType;

	ZString IOutgoingInterchangeHeaderTextFieldsProvider.AccountNumber => customsInterchangeAndAccountInfo.Account?.AccountNumber ?? ZString.Empty;

	ZString IOutgoingInterchangeHeaderTextFieldsProvider.CustomsInterchangeHeader => customsInterchangeAndAccountInfo.Header?.InnerText ?? ZString.Empty;
}
