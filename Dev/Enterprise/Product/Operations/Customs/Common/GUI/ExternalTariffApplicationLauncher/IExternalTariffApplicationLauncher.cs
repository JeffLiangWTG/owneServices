using System.Net.Http;
using CargoWise.Types;

namespace Enterprise.Customs.Common.GUI
{
	public interface IExternalTariffApplicationLauncher
	{
		void LaunchExternalApplication(BorderWiseFilters filters, ZGuid businessEntityPk, ZGuid? webSocketClientId = null, string jobPk = null);
		void LaunchExternalApplication(BorderWiseFilters filters);
		HttpMessageHandler HttpMessageHandler { get; set; }
	}
}
