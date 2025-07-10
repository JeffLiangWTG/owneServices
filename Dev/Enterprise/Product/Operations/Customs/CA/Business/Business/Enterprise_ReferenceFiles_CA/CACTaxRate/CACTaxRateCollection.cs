using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class CACTaxRateCollection : BusinessObjectCollection<CACTaxRate>
	{
		public CACTaxRateCollection(CACTaxRefNumHeader taxRefNumHeader, ZDateTime effectiveDate, ZString taxType)
			: base(taxRefNumHeader.Factory, CACTaxRate.AddEffectiveDateFilter(new ZQuery(CACTaxRateSchema.ZH_TaxRefNumber, taxRefNumHeader.RefNumbers.Select(x => x.ZE_ExciseTaxRefNumber)), taxType, effectiveDate))
		{
			this._taxType = taxType;
		}

		public CACTaxRateCollection(BusinessObjectFactory factory, ZString taxType)
			: base(factory, CACTaxRate.AddTaxTypeFilter(new ZQuery(), taxType))
		{
			this._taxType = taxType;
		}

		public CACTaxRateCollection(BusinessObjectFactory factory, ZDateTime effectiveDate, ZString taxType)
			: base(factory, CACTaxRate.AddEffectiveDateFilter(new ZQuery(), taxType, effectiveDate))
		{
			this._taxType = taxType;
		}
		readonly ZString _taxType;

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
			var taxRate = (CACTaxRate)child;
			taxRate.ZH_TaxType = _taxType;
		}
	}
}
