using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.BatchProcessor;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class US
		{
			public interface IEBondMessageProcessor
			{
				void Process(BusinessObjectFactory factory, ILoggingInformation loggingInformation, ZGuid messagePk);
			}
		}
	}
}
