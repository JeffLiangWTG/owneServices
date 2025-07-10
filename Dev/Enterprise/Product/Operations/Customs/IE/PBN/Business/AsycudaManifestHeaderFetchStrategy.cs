namespace Enterprise.Customs.IE.PBN.Business
{
	public class AsycudaManifestHeaderFetchStrategy : ASYCUDA.Business.AsycudaManifestHeaderFetchStrategy
	{
		public AsycudaManifestHeaderFetchStrategy(AsycudaManifestHeader header) : base(header)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(ManifestBase.AutoAsycudaBill.Schema.TableName, BusinessObject.PK);
		}
	}
}
