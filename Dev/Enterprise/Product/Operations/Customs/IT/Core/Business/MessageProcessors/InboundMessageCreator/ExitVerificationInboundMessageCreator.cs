using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

sealed class ExitVerificationInboundMessageCreator : IInboundMessageCreator
{
	void IInboundMessageCreator.CreateMessagesForInterchange(EDIInterchange interchange)
	{
		Argument.NotNull(interchange, nameof(interchange));

		var progressiveAnnualNumber = GetProgressiveAnnualNumber(interchange);

		var singleMessageCreator = new InboundSingleMessageCreator();
		var message = singleMessageCreator.CreateMessageForInterchange(interchange);
		message.EM_MessageNum = progressiveAnnualNumber;
	}

	ZString GetProgressiveAnnualNumber(EDIInterchange interchange)
	{
		var progressiveAnnualNumber = MessageProcessorHelper.RetrieveValueOfXmlNode(interchange.EI_HeaderText, TagPAN);
		if (progressiveAnnualNumber.IsEmpty)
		{
			throw new UnableToInterpretInterchangeException(GetExceptionMessage(TagPAN));
		}
		return progressiveAnnualNumber;
	}

	ZString GetExceptionMessage(ZString tag) => Res.GetString("D7DB46DF-BE8B-463B-A135-139A50E63AED", "Invalid <{0}> tag value", tag);

	const string TagPAN = "PAN";
}
