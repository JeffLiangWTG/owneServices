using System.Linq;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SpecificCircumstanceIndicatorValidDataCombination
	{
		public SpecificCircumstanceIndicatorValidDataCombination(string applicationCode, string[] transportModeList)
		{
			this.applicationCode = applicationCode;
			this.transportModeList = transportModeList;
		}

		readonly string applicationCode;
		readonly string[] transportModeList;

		public bool Match(AsycudaManifestHeader manifestHeader)
		{
			return applicationCode == manifestHeader.AMA_ApplicationCode && transportModeList.Any(n => n == manifestHeader.AMA_TransportMode);
		}
	}
}
