namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	public class AsycudaBillCollectionForTest : AsycudaBillCollection
	{
		public AsycudaBillCollectionForTest(AsycudaManifestHeaderForTest master) : base(master)
		{
		}

		public new AsycudaBillForTest this[int index] => (AsycudaBillForTest)Elements[index];

		public new AsycudaBillForTest AddNew() => (AsycudaBillForTest)base.AddNew();
	}
}
