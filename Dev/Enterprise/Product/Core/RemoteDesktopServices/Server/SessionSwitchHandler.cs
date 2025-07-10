using CargoWise.Common;
using Enterprise.RemoteDesktopServices.MessageElements;

namespace Enterprise.RemoteDesktopServices.Server
{
	public class SessionSwitchHandler : XmlMessageHandler<SessionSwitchMessage>
	{
		public SessionSwitchHandler(IWtsSessionSwitchCallback callback)
		{
			Argument.NotNull(callback, nameof(callback));

			this.callback = callback;
		}

		readonly IWtsSessionSwitchCallback callback;

		protected override void Handle(IEnterpriseChannel channel, SessionSwitchMessage message)
		{
			callback.OnSessionSwitch(message.Reason);
		}
	}
}
