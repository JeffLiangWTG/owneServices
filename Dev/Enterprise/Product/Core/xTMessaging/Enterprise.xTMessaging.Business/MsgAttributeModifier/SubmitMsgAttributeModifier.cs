using Enterprise.Integration.Licensing;
using Enterprise.xTMessaging.Shared;
using Xware.Xt.Grpc.Application;

namespace Enterprise.xTMessaging.Business
{
	public class SubmitMsgAttributeModifier : ISubmitMsgAttributeModifier
	{
		public void AddParserFields(SubmitMsgMessage submitMsg)
		{
			var systemRegKey = CargoWise.Application.ObjectFactory.Get<IProductRegistration>().Key;
			submitMsg.Parserattr.Add((int)StdParserFieldId.PfSender, $"{systemRegKey.EnterpriseCode}{systemRegKey.ServerCode}");
			submitMsg.Msgattr.TryGetValue(Constants.CustomMsgAttributes.DestinationParty, out var destinationParty);
			submitMsg.Parserattr.Add((int)StdParserFieldId.PfReceiver, destinationParty?.Trim() ?? string.Empty);
		}
	}
}
