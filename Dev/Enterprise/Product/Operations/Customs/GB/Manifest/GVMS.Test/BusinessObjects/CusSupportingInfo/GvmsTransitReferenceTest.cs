using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	[TestedType(typeof(GvmsTransitReference))]
	public class GvmsTransitReferenceTest : GvmsItemReferenceTest<GvmsTransitReference>
	{
		public void TestLookups()
		{
			AssertType<GvmsTransitReferenceLookups>("Lookups", gvmsItemReference.Lookups);
		}

		public void TestValidation()
		{
			AssertType<GvmsItemReferenceValidation>("Validation", gvmsItemReference.Validation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return header.GvmsTransitReferenceCollection.AddNew();
		}

		protected override IEnumerable<GvmsTransitReference> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var manifestHeader = factory.New<AsycudaManifestHeader>();
			manifestHeader.FillWithValidTestData();
			var result = manifestHeader.GvmsTransitReferenceCollection.AddNew();
			result.CSI_Code = GvmsItemReferencePartitions.TransitReferenceCodes.First();
			yield return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			gvmsItemReference = header.GvmsTransitReferenceCollection.AddNew();
		}
	}
}
