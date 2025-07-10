using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class CACTaxRefNumberCollection : DependentBusinessObjectCollection<CACTaxRefNumber, CACTaxRefNumHeader>
	{
		public CACTaxRefNumberCollection(CACTaxRefNumHeader master)
			: base(master)
		{
		}
		protected override SchemaGuidColumn FKSchemaColumnInDependent => CACTaxRefNumberSchema.ZE_ZD_TaxRefNumHeader;
	}
}
