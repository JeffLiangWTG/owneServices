using System;

namespace CargoWise.Data
{
	sealed class NoActionConnectionErrorManager : ConnectionErrorManager
	{
		internal NoActionConnectionErrorManager(IDbReconnectionHandling connectionToHandle) : base(connectionToHandle)
		{
		}

		internal override bool HandleDisconnectionAndSecurityErrors(Exception ex)
		{
			return false;
		}

		protected override bool ReconnectIfApplicableCore(Exception e)
		{
			return false;
		}
	}
}
