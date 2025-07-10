using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public partial class JobDeclarationCollection : Customs.Business.BaseJobDeclarationCollection
	{
		public JobDeclarationCollection(BusinessObjectFactory factory, ZGuid companyPkToFilterOn)
			: base(factory, companyPkToFilterOn)
		{
		}

		protected override bool AllowNewCore => false;

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy() => new FetchStrategies.JobDeclarationCollectionFetchStrategy(this);
	}
}
