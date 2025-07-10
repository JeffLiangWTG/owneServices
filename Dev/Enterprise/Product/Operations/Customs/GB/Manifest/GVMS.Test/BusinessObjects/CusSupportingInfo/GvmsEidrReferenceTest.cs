using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	[TestedType(typeof(GvmsEidrReference))]
	public class GvmsEidrReferenceTest : GvmsItemReferenceTest<GvmsEidrReference>
	{
		public void TestLookups()
		{
			AssertType<GvmsEidrReferenceLookups>("Lookups", gvmsItemReference.Lookups);
		}

		public void TestValidation()
		{
			AssertType<GvmsEidrReferenceValidation>("Validation", gvmsItemReference.Validation);
		}

		public void TestCSI_DescriptionMaxLength()
		{
			AssertEquals(22, gvmsItemReference.CSI_DescriptionInfo.MaxLength);
		}

		public void TestCSI_ProcedureMaxLength()
		{
			AssertEquals(4, gvmsItemReference.CSI_ProcedureInfo.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return header.GvmsEidrAndOralReferenceCollection.AddNew();
		}

		protected override IEnumerable<GvmsEidrReference> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var manifestHeader = factory.New<AsycudaManifestHeader>();
			manifestHeader.FillWithValidTestData();
			var result = manifestHeader.GvmsEidrAndOralReferenceCollection.AddNew();
			result.CSI_Code = GvmsItemReferencePartitions.EidrReferenceCodes.First();
			yield return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			gvmsItemReference = header.GvmsEidrAndOralReferenceCollection.AddNew();
		}
	}
}
