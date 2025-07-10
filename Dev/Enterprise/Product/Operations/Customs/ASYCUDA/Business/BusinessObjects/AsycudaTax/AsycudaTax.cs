using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaTax : ManifestBase.AsycudaTax, Integration.Customs.ASYCUDA.IAsycudaTax
	{
		public AsycudaTax(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new static readonly AsycudaTaxTypeDecider TypeDecider = new AsycudaTaxTypeDecider();
		public new AsycudaTaxValidation Validation => (AsycudaTaxValidation)base.Validation;
		protected override ManifestBase.AsycudaTaxValidation GetNewValidation() => new AsycudaTaxValidation(this);
		public new AsycudaTaxLookups Lookups => (AsycudaTaxLookups)base.Lookups;
		protected override ManifestBase.AsycudaTaxLookups GetNewLookups() => new AsycudaTaxLookups(this);
		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new AsycudaTaxFetchStrategy(this);
		public new AsycudaBill Bill => (AsycudaBill)base.Bill;
	}
}
