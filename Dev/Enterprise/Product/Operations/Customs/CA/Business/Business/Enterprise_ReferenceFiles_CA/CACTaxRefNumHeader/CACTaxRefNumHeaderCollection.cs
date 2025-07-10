using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class CACTaxRefNumHeaderCollection : DependentBusinessObjectCollection<CACTaxRefNumHeader, CACClassHeader>
	{
		public CACTaxRefNumHeaderCollection(CACClassHeader master)
			: base(master)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CACTaxRefNumHeaderSchema.ZD_ZA_ClassNumber;
	}
}
