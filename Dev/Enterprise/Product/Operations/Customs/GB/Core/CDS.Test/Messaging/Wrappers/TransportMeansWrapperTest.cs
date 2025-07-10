using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Testing;

namespace Enterprise.Customs.GB.CDS.Messaging.Testing
{
	public class TransportMeansWrapperTest : TestCaseWithFactory
	{
		public void TestTransportMeansWrapperFieldsAreTruncated()
		{
			var helper = new DeclarationTestHelper(Factory);
			ITransportMeans wrapper = TransportMeansWrapper.New(helper.GetStringOfMaxSizePlusOneToTrim(27), "", "", "");
			AssertEquals("ID should be truncated to 27 characters", 27, wrapper.ID.Length);
		}
	}
}
