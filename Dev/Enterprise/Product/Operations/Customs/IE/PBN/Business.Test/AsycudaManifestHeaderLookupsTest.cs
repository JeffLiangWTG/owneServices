using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.IE.PBN.Business.Testing
{
	public class AsycudaManifestHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestNatures()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<AsycudaManifestHeader>();
				var list = header.Lookups.Natures;
				AssertEquals("EXP, IMP", list.CodesAsString);
				AssertSame("List is cached", list, header.Lookups.Natures);
			});
		}

		public void TestTransportModeList()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<AsycudaManifestHeader>();
				var list = header.Lookups.TransportModeList;
				AssertContainsExactElementsInAnyOrder(new string[] { Core.Constants.TransportModes.Road, Core.Constants.TransportModes.Sea }, list.GetAllCodes());
				AssertSame("List is cached", list, header.Lookups.TransportModeList);
			});
		}

		public void TestMessageStatusList()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<AsycudaManifestHeader>();
				var list = header.Lookups.MessageStatusList;
				AssertType<LogicalStatusList>(list);
				AssertSame("List is cached", list, header.Lookups.MessageStatusList);
			});
		}

		public void TestRegistrationStatusList()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<AsycudaManifestHeader>();
				var list = header.Lookups.RegistrationStatusList;
				AssertType<PBNCustomsStatusList>(list);
				AssertSame("List is cached", list, header.Lookups.RegistrationStatusList);
			});
		}
	}
}
