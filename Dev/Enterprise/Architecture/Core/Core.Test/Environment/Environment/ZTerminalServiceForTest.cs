using Enterprise.RemoteDesktopServices;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public sealed class ZTerminalServiceForTest : TerminalService
	{
		public ZTerminalServiceForTest(bool isRemoteAppSession = true, bool isWTSSession = true, bool isCitrixICA = false)
		{
			this.isRemoteAppSession = isRemoteAppSession;
			this.isCitrixICA = isCitrixICA;
			this.isWTSSession = isWTSSession;
		}

		public override bool IsRemoteAppSession => isRemoteAppSession;
		readonly bool isRemoteAppSession;

		public override bool IsCitrixICA => isCitrixICA;
		readonly bool isCitrixICA;

		public override bool IsWTSSession => isWTSSession;
		readonly bool isWTSSession;
	}
}
