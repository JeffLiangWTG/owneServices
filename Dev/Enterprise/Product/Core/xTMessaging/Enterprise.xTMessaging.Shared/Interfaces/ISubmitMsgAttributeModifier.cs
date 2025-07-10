using Xware.Xt.Grpc.Application;

namespace Enterprise.xTMessaging.Shared
{
	public interface ISubmitMsgAttributeModifier
	{
		void AddParserFields(SubmitMsgMessage msgToSubmit);
	}
}
