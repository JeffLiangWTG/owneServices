using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobDeclarationCollection : Customs.Business.BaseJobDeclarationCollection
	{
		public JobDeclarationCollection(BusinessObjectFactory factory)
			: base(factory, Core.Constants.CountryCodes.Australia)
		{
		}

		public JobDeclarationCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, MergeAdditionalFilter(additionalFilter))
		{
		}

		static ZQuery MergeAdditionalFilter(ZQuery additionalFilter)
		{
			var finalFilter = GetCountryQuery(Core.Constants.CountryCodes.Australia);
			finalFilter.AddToFilter(additionalFilter);
			return finalFilter;
		}

		public new JobDeclaration this[int index]
		{
			get { return (JobDeclaration)Elements[index]; }
		}

		public new JobDeclaration AddNew()
		{
			return (JobDeclaration)base.AddNew();
		}

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new JobDeclarationCollectionFetchStrategy(this);
		}
	}
}
