using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	[TestedType(typeof(GvmsItemReferenceCollection<GvmsTransitReference>))]
	class GvmsTransitReferenceCollectionTest : CusSupportingInfoCollectionTest<GvmsTransitReference>
	{
		protected override CusSupportingInfoCollection<GvmsTransitReference> GetCusSupportingInfoCollection()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return new GvmsItemReferenceCollection<GvmsTransitReference>(header, GvmsItemReferencePartitions.TransitReferenceCodes);
		}

		protected override CargoWise.EntityFramework.BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = base.GetNewElementToAddToTheCollection();
			((GvmsItemReference)result).CSI_Code = GvmsItemReferencePartitions.TransitReferenceCodes.First();
			return result;
		}
	}
}
