
using CargoWise.Data;
using CargoWise.DataProtection;

namespace Enterprise.ChangeDataCapture.Common
{
	class DedicatedConnectionForCDCScan : DbConnection<UnrestrictedWriterLoginCredentials>
	{
		protected DedicatedConnectionForCDCScan() : base()
		{
			SetDeadlockPriority(10);
		}

		internal static DedicatedConnectionForCDCScan New()
		{
			return new DedicatedConnectionForCDCScan();
		}

		protected override IConnectionPooling ConnectionPoolingValue => connPoolingValue = connPoolingValue ?? new NoConnectionPooling();

		public override string UserLogin => UnrestrictedWriterLoginCredentials.UserNameFor(fInitialDatabaseName);

		IConnectionPooling connPoolingValue;
	}
}
