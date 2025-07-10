using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineCatchZone : CusCodeData
	{
		public QuarantineCatchZone(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public new class Schema : CusCodeData.Schema
		{
			public const int CY_Data_MaxLength = 35;
		}

		[MaxLength(Schema.CY_Data_MaxLength)]
		public override ZString CY_Data
		{
			get { return base.CY_Data; }
			set { base.CY_Data = value; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.NEXDOCSCatchZone;
			CY_Code = CusCodeDataTypeList.Codes.NEXDOCSCatchZone;
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(QuarantineExDocHeader));

		protected override CusCodeDataValidation GetNewValidation() => new QuarantineCatchZoneValidation(this);
	}
}
