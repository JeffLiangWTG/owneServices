using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	[TestedType(typeof(GvmsCustomsReference))]
	public class GvmsCustomsReferenceTest : GvmsItemReferenceTest<GvmsCustomsReference>
	{
		public void TestLookups()
		{
			AssertType<GvmsCustomsReferenceLookups>("Lookups", gvmsItemReference.Lookups);
		}

		public void TestValidation()
		{
			AssertType<GvmsCustomsReferenceValidation>("Validation", gvmsItemReference.Validation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return header.GvmsCustomsReferenceCollection.AddNew();
		}

		protected override IEnumerable<GvmsCustomsReference> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var manifestHeader = factory.New<AsycudaManifestHeader>();
			manifestHeader.FillWithValidTestData();
			var result = manifestHeader.GvmsCustomsReferenceCollection.AddNew();
			result.CSI_Code = GvmsItemReferencePartitions.CustomsReferenceCodes.First();
			yield return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			gvmsItemReference = header.GvmsCustomsReferenceCollection.AddNew();
		}
	}
}
