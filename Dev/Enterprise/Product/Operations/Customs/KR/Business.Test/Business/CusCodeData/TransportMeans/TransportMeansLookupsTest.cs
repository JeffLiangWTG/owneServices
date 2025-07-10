using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class TransportMeansLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRefVessels()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;

			var transportMean = declaration.TransportMeans.AddNew();
			var lookups = transportMean.Lookups;
			AssertEquals(typeof(RefVesselCollection), lookups.RefVessels.GetType());
		}
	}
}
