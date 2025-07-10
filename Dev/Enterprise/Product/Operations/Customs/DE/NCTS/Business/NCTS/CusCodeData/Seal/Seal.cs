using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class Seal : EU.NCTS.Business.Seal
	{
		public Seal(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public new class Schema : Customs.Business.CusCodeData.Schema
		{
			public new const int CY_DataMaxLength = 20;
		}

		[MaxLength(Schema.CY_DataMaxLength)]
		public override ZString CY_Data { get => base.CY_Data; set => base.CY_Data = value; }

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(NctsHeader));
	}
}
