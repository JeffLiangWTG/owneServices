using System;

namespace CargoWise.Data.Testing
{
	sealed class DummyConnectionErrorManager : ConnectionErrorManager
	{
		public DummyConnectionErrorManager(IDbReconnectionHandling connectionToHandle)
			: base(connectionToHandle)
		{
		}

		protected override bool ReconnectIfApplicableCore(Exception e)
		{
			return false;
		}
	}
}
