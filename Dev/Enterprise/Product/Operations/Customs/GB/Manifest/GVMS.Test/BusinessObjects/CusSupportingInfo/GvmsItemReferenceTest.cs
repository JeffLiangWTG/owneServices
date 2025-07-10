using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	[TestedType(typeof(GvmsItemReference))]
	public abstract class GvmsItemReferenceTest<T> : CusSupportingInfoTest<T> where T : GvmsItemReference
	{
		public void TestHumanReadableName()
		{
			var itemRef = Factory.New<T>();
			AssertEquals("Customs Reference", itemRef.HumanReadableName);
		}

		public void TestReadOnly()
		{
			var itemRef = Factory.New<T>();
			Assert(!itemRef.ReadOnly);

			itemRef.CSI_IssuerType = "SYS";
			Assert(itemRef.ReadOnly);
		}

		public void TestSetDefaultValues()
		{
			var itemRef = Factory.New<T>();
			var type = itemRef.CSI_Type;
			AssertEquals("GVM", type);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeader>();
			header.FillWithValidTestData();
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
		}

		protected AsycudaManifestHeader header;
		protected GvmsItemReference gvmsItemReference;
	}
}
