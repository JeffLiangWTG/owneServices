using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	[TestedType(typeof(GvmsItemReferenceCollection<GvmsCustomsReference>))]
	class GvmsCustomsReferenceCollectionTest : CusSupportingInfoCollectionTest<GvmsCustomsReference>
	{
		protected override CusSupportingInfoCollection<GvmsCustomsReference> GetCusSupportingInfoCollection()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return new GvmsItemReferenceCollection<GvmsCustomsReference>(header, GvmsItemReferencePartitions.CustomsReferenceCodes);
		}

		protected override CargoWise.EntityFramework.BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = base.GetNewElementToAddToTheCollection();
			((GvmsItemReference)result).CSI_Code = GvmsItemReferencePartitions.CustomsReferenceCodes.First();
			return result;
		}
	}
}
