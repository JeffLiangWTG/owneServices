using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsEuOfficeCodeCollection : CusCodeDataCollection<NctsEuOfficeCode>
	{
		public NctsEuOfficeCodeCollection(NctsHeader master) : base(master, EU.Business.CusCodeDataTypeList.Codes.OfficeCode)
		{
		}

		public NctsEuOfficeCodeCollection(NctsCommonMovementHeader master) : base(master, EU.Business.CusCodeDataTypeList.Codes.OfficeCode)
		{
		}

		protected override CargoWise.Schema.SchemaGuidColumn FKSchemaColumnInDependent => CusCodeDataSchema.CY_ParentID;
	}
}
