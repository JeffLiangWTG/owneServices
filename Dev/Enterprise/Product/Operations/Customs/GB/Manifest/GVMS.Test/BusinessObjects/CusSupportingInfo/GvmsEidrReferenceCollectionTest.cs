using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	[TestedType(typeof(GvmsItemReferenceCollection<GvmsEidrReference>))]
	class GvmsEidrReferenceCollectionTest : CusSupportingInfoCollectionTest<GvmsEidrReference>
	{
		protected override CusSupportingInfoCollection<GvmsEidrReference> GetCusSupportingInfoCollection()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return new GvmsItemReferenceCollection<GvmsEidrReference>(header, GvmsItemReferencePartitions.EidrReferenceCodes);
		}

		protected override CargoWise.EntityFramework.BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = base.GetNewElementToAddToTheCollection();
			((GvmsItemReference)result).CSI_Code = GvmsItemReferencePartitions.EidrReferenceCodes.First();
			return result;
		}
	}
}
