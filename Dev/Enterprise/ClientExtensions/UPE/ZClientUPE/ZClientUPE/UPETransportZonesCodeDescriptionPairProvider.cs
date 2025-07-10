using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE
{
	public class UPETransportZonesCodeDescriptionPairProvider : DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public const string AllMetro = "All Metro";

		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair("ALL", AllMetro);
			var codManifestReportZoneRelatedParty = Factory.Load<OrgHeader>(UPEDataRegistry.Instance.CODManifestReportZoneRelatedPartyItem.Value);
			if (codManifestReportZoneRelatedParty != null)
			{
				var providers = Factory.Load<RateTransportProvider>(new ZQuery(RateTransportProviderSchema.TP_OH_RelatedParty, codManifestReportZoneRelatedParty.PK));

				foreach (RateTransportProvider provider in providers)
				{
					foreach (RateTransportZone zone in provider.Zones)
					{
						result.AddPairIfNotExist("Z" + zoneNumber.ToString().PadLeft(2, '0'), zone.TZ_ZoneName);
						zoneNumber++;
					}
				}
			}
			return result;
		}

		#region Implementation

		ZInt zoneNumber = 1;

		#region Factory
		BusinessObjectFactory fFactory;
		BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = new BusinessObjectFactory();
				}

				return fFactory;
			}
		}
		#endregion

		#endregion
	}
}
