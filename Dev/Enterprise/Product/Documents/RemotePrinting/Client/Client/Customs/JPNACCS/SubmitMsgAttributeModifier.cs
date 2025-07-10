using Enterprise.xTMessaging.Shared;
using Xware.Xt.Grpc.Application;

namespace Enterprise.RemotePrinting.Client;

sealed class SubmitMsgAttributeModifier : ISubmitMsgAttributeModifier
{
	public SubmitMsgAttributeModifier(IJPNACCSClientApplicationSetting setting)
	{
		this.setting = setting;
	}

	readonly IJPNACCSClientApplicationSetting setting;

	void ISubmitMsgAttributeModifier.AddParserFields(SubmitMsgMessage msgToSubmit)
	{
		msgToSubmit.Parserattr.Add((int)StdParserFieldId.PfSender, NACCSConstants.WebPrintParty);
		msgToSubmit.Parserattr.Add((int)StdParserFieldId.PfReceiver, NACCSUtils.GetReceiver(setting));
	}
}
