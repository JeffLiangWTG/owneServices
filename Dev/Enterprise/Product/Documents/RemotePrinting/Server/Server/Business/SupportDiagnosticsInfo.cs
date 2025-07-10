using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.RemotePrinting.Server.Model;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.RemotePrinting.Server.Business
{
	public class SupportDiagnosticsInfo : NonPersistentBusinessObject, IObsoleteValidation
	{
		public SupportDiagnosticsInfo()
			: base(new BusinessObjectFactory())
		{
		}

		public ZString VersionNumber { get { return EnterpriseInfo.VersionNumber; } }

		public ZString VersionDate { get { return EnterpriseInfo.VersionDate; } }

		public ZString LicenseCode { get { return EnterpriseInfo.LicenceCode; } }

		public ZString Release { get { return EnterpriseInfo.Release; } }

		EnterpriseInformationRetriever EnterpriseInfo
		{
			get { return enterpriseInfo ?? (enterpriseInfo = new EnterpriseInformationRetriever()); }
		}

		EnterpriseInformationRetriever enterpriseInfo;

		public StmPrintQueueExtCollection PrintQueues
		{
			get
			{
				if (printQueues == null)
				{
					printQueues = new StmPrintQueueExtCollection(Factory);
					printQueues.Load();
				}
				return printQueues;
			}
		}

		StmPrintQueueExtCollection printQueues;

		public SignalRClientCollection SignalRClients
		{
			get
			{
				if (signalRClients == null)
				{
					signalRClients = new SignalRClientCollection();

					foreach (string webPrintServiceAddress in PrintQueues.Select(p => p.SQ_WebPrintServiceAddress).Distinct())
					{
						var clients = GetSignalRClients(webPrintServiceAddress);
						if (clients != null)
						{
							foreach (SignalRClientInfo signalRClient in clients)
							{
								var clientBizo = new SignalRClientBusinessObject();
								clientBizo.ServerName = signalRClient.ServerName;
								clientBizo.ClientId = signalRClient.ClientId;
								clientBizo.PrintersCount = signalRClient.PrintersList?.Count ?? 0;
								clientBizo.WebPrintServerAddress = signalRClient.WebServerAddress;
								clientBizo.WebPrintServerHostName = signalRClient.WebServerHostName;
								clientBizo.WebPrintClientVersionNumber = signalRClient.WebPrintClientVersionNumber;
								signalRClients.Add(clientBizo);
							}
						}
					}
				}
				return signalRClients;
			}
		}

		SignalRClientCollection signalRClients;

		public virtual SignalRClientInfo[] GetSignalRClients(string webPrintServiceAddress)
		{
			return new SupportWebClient().GetSignalRClients(webPrintServiceAddress);
		}
	}
}
