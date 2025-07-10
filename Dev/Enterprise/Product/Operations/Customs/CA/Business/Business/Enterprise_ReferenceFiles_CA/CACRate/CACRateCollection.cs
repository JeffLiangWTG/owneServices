using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class CACRateCollection : DependentBusinessObjectCollection<CACRate, BusinessObject>
	{
		public CACRateCollection(BusinessObject master) : base(master)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CACRateSchema.ZC_ParentID;
	}
}
