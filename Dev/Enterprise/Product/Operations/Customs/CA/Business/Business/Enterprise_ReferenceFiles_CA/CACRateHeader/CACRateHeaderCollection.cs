using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class CACRateHeaderCollection : DependentBusinessObjectCollection<CACRateHeader, CACClassHeader>
	{
		#region Constructors

		public CACRateHeaderCollection(CACClassHeader master, ZString rateType)
			: base(master, AddExciseRateFilter(rateType))
		{
			this._rateType = rateType;
		}
		readonly ZString _rateType;

		#endregion

		#region Implementation

		static ZQuery AddExciseRateFilter(ZString rateType)
		{
			var query = new ZQuery();
			query.AddToFilter(CACRateHeaderSchema.ZB_RateType, rateType);
			return query;
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			base.SetCollectionRelationships(dependent);
			var child = (CACRateHeader)dependent;
			child.ZB_RateType = _rateType;
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CACRateHeaderSchema.ZB_ZA_ClassHeader;

		#endregion
	}
}
