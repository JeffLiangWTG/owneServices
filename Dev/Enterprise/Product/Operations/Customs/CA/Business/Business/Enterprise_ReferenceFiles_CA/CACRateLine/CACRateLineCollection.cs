using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class CACRateLineCollection : DependentBusinessObjectCollection<CACRateLine, CACRate>
	{
		public CACRateLineCollection(CACRate master)
			: base(master)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CACRateLineSchema.ZR_ZC_Rate;
	}
}
