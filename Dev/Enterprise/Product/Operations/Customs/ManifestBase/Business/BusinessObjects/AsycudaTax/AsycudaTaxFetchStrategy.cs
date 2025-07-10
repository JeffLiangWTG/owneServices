namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaTaxFetchStrategy : AsycudaFetchStrategy
	{
		public AsycudaTaxFetchStrategy(AsycudaTax tax)
			: base(tax)
		{
		}

		protected new AsycudaTax BusinessObject => (AsycudaTax)base.BusinessObject;
	}
}

