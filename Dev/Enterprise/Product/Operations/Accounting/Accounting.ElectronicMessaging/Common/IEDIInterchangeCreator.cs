using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	interface IEDIInterchangeCreator
	{
		ZString EInvoicingServicePoint { get; }

		IEDICommunicationsMode CommunicationsMode { get; }

		void Process(ILogger logger);
	}
}
