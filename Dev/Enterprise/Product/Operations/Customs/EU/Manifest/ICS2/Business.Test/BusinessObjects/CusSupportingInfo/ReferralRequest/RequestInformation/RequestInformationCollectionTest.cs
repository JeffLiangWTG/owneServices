using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(RequestInformationCollection))]
	sealed class RequestInformationCollectionTest : CusSupportingInfoCollectionTest<RequestInformation>
	{
		protected override CusSupportingInfoCollection<RequestInformation> GetCusSupportingInfoCollection()
		{
			var header = Factory.New<RequestHeader>();
			return new RequestInformationCollection(header);
		}
	}
}
