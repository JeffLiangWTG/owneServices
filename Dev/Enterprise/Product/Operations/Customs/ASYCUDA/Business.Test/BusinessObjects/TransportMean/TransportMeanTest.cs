using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(TransportMean))]
	sealed class TransportMeanTest : EnterpriseBusinessObjectTestCase
	{
		public void TestMaxLength()
		{
			var transport = Factory.NewWithValidTestData<TransportMean>();
			AssertEquals("VehicleCountry MaxLength", 2, transport.VehicleCountryInfo.MaxLength);
			AssertEquals("TruckKind MaxLength", 4, transport.TruckKindInfo.MaxLength);
		}

		public void TestCaptions()
		{
			AssertCaptions("JW_LegOrder", "Sequence Number", "Seq.");
			AssertCaptions("JW_ETA", "ETA");
			AssertCaptions("JW_ATD", "ATD");
			AssertCaptions("JW_RL_NKDiscPort", "Arrival Port");
			AssertCaptions("JW_Vessel", "Vehicle Registration ID", "Vehicle ID");
			AssertCaptions("VehicleCountry", "Vehicle Registration Country", "Vehicle Country");
			AssertCaptions("TruckKind", "Truck Type");
		}

		public void TestLookups()
		{
			var transport = Factory.NewWithValidTestData<TransportMean>();
			AssertType<TransportMeanLookups>(transport.Lookups);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			return header.TransportMeans.AddNew();
		}

		void AssertCaptions(string propertyName, string caption, string shortCaption = null)
		{
			AssertEquals($"{propertyName} Caption", caption, DataBoundResourceStrings.GetDataForProperty(typeof(TransportMean), propertyName).Caption);
			if (shortCaption != null)
			{
				AssertEquals($"{propertyName} Short Caption", shortCaption, DataBoundResourceStrings.GetDataForProperty(typeof(TransportMean), propertyName).ShortCaption);
			}
		}
	}
}
