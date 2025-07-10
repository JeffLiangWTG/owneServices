using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class RatingWrapperCollection : GenericWrapperCollection<RatingWrapper>
	{
		public RatingWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public RatingWrapperCollection(OrgOpportunity opportunity, BusinessObjectFactory factory)
			: this(factory)
		{
			AddQuotationToCollection(this, opportunity.RelatedChildActivityPivotCollection);
			this.Sort(delegate(BusinessObject x, BusinessObject y)
			{
				return ((x as RatingWrapper).WrappedObject as IRatingHeader).TH_QuoteNumber.CompareTo(((y as RatingWrapper).WrappedObject as IRatingHeader).TH_QuoteNumber);
			});
		}

		void AddQuotationToCollection(BusinessObjectCollection collection, IRelatedChildActivityPivotCollection pivots)
		{
			foreach (var pivot in pivots)
			{
				if (pivot.RAP_ChildActivityTableCode.Equals(RatingHeaderSchema.Constants.Prefix))
				{
					RatingHeader bizO = Factory.Load(RatingHeaderSchema.Constants.Prefix, pivot.RAP_ChildActivityID) as RatingHeader;
					if (!bizO.IsCancelled && !bizO.TH_OneTimeQuote && CheckLoginCompanyMatches(bizO))
					{
						collection.Add(RatingWrapper.New(bizO, Factory));
					}
				}
				if (pivot.ChildActivity != null)
				{
					AddQuotationToCollection(collection, pivot.ChildActivity.RelatedChildActivityPivotCollection);
				}
			}
		}

		static bool CheckLoginCompanyMatches(RatingHeader rating)
		{
			if (rating != null)
			{
				var company = rating.Company;
				if (company != null && company.PK != Env.CurrentCompany.PK)
				{
					return false;
				}
			}
			return true;
		}
	}
}
