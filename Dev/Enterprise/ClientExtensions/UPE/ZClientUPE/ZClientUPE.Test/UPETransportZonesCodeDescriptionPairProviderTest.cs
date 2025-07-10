using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Testing
{
	public class UPETransportZonesCodeDescriptionPairProviderTest : TestCaseWithFactory
	{
		public void TestIsReturningCorrectCollection()
		{
			using (UPEDataRegistry.Instance.EnableUPECustomisationsItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				UPETransportZonesCodeDescriptionPairProvider pairProvider = new UPETransportZonesCodeDescriptionPairProvider();
				ReadOnlyCodeDescriptionPairList pairList = pairProvider.GetCodeDescriptionPairList();
				Assert("should contain default", pairList.ContainsCode("ALL"));
				Assert("should contain default", UPETransportZonesCodeDescriptionPairProvider.AllMetro == pairList.GetDescriptionFromCode("ALL"));

				OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
				RateTransportProvider provider = Factory.NewWithValidTestData<RateTransportProvider>();
				provider.TP_OH_RelatedParty = org.PK;
				RateTransportZone zone = provider.Zones.AddNew();
				zone.TZ_ZoneName = "zone 1";
				zone = provider.Zones.AddNew();
				zone.TZ_ZoneName = "zone 2";
				zone = provider.Zones.AddNew();
				zone.TZ_ZoneName = "zone 3";
				Factory.Save();

				using (UPEDataRegistry.Instance.CODManifestReportZoneRelatedPartyItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, org.PK.ToGuid()))
				{
					pairList = pairProvider.GetCodeDescriptionPairList();
					Assert("should contain default", pairList.ContainsCode("Z01"));
					Assert("should contain default", "zone 1" == pairList.GetDescriptionFromCode("Z01"));

					Assert("should contain default", pairList.ContainsCode("Z02"));
					Assert("should contain default", "zone 2" == pairList.GetDescriptionFromCode("Z02"));

					Assert("should contain default", pairList.ContainsCode("Z03"));
					Assert("should contain default", "zone 3" == pairList.GetDescriptionFromCode("Z03"));
				}
			}
		}
	}
}
