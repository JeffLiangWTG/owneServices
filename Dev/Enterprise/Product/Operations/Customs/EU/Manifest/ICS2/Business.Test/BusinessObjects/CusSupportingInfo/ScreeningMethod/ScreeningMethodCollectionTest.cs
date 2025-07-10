using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(ScreeningMethodCollection))]
	sealed class ScreeningMethodCollectionTest : CusSupportingInfoCollectionTest<ScreeningMethod>
	{
		protected override CusSupportingInfoCollection<ScreeningMethod> GetCusSupportingInfoCollection()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return new ScreeningMethodCollection(header);
		}

		public void TestMessageError_WhenCollectionHasOnlyOneScreeningMethod()
		{
			Collection.AddNew();
			Assert("show message error when collection has only one element", Collection.First().RowMessageErrors.Any(n => n.Message == "Please add at lest 2 Screen Methods"));

			Collection.AddNew();
			Assert("no message error when collection has more than one elements", !Collection.First().HasRowMessageErrors);
		}
	}
}
