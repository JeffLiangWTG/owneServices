using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaManifestHeaderTransportSupporterTest : TestCaseWithFactory
	{
		public void TestShippingLine()
		{
			var carrier1 = Factory.New<OrgHeader>();
			carrier1.OH_IsShippingLine = true;
			asycudaManifestHeader.ShippingAgentOrgPK = carrier1.PK;

			AssertEquals(carrier1.PK, supporter.ShippingLine);
		}

		public void TestDescription()
		{
			AssertEquals("DESC1", supporter.Description);
		}

		public void TestConsignmentRef()
		{
			AssertEquals("ref1", supporter.ConsignmentRef);
		}

		public void TestTransportMode()
		{
			AssertEquals("ROA", supporter.TransportMode);
		}

		public void TestContainerMode()
		{
			AssertEquals("CNT", supporter.ContainerMode);
		}

		public void TestBillOfLading()
		{
			AssertEquals("MAWB1111", supporter.BillOfLading);
		}

		public void TestDistanceCalculationCheckpoint()
		{
			AssertEquals(Env.Security.RoadDistanceCalculationServiceCustoms, supporter.DistanceCalculationCheckpoint);
		}

		public void TestSupportETD()
		{
			AssertEquals(false, supporter.SupportETD);
		}

		public void TestSupportVoyageFlight()
		{
			AssertEquals(false, supporter.SupportVoyageFlight);
		}

		protected override void SetUp()
		{
			base.SetUp();
			asycudaManifestHeader = Factory.New<AsycudaManifestHeader>();
			asycudaManifestHeader.AMA_ManifestDescription = "DESC1";
			asycudaManifestHeader.AMA_JobReference = "ref1";
			asycudaManifestHeader.AMA_TransportMode = "ROA";
			asycudaManifestHeader.AMA_ContainerMode = "CNT";
			asycudaManifestHeader.AMA_MasterBill = "MAWB1111";

			supporter = new AsycudaManifestHeaderTransportSupporter(asycudaManifestHeader);
		}

		AsycudaManifestHeader asycudaManifestHeader;
		AsycudaManifestHeaderTransportSupporter supporter;
	}
}
