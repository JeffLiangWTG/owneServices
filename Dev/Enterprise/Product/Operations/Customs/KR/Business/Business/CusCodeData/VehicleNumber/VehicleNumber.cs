using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class VehicleNumber : CusCodeData
	{
		public VehicleNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : CusCodeData.Schema
		{
			public const int VIN_MaxLength = 20;
			public const int Sequence_MaxLength = 3;
		}

		public override bool SupportsNotes => false;

		[MaxLength(Schema.VIN_MaxLength)]
		public override ZString CY_Data
		{
			get => base.CY_Data;
			set => base.CY_Data = value;
		}

		[MaxLength(Schema.Sequence_MaxLength)]
		[ReadOnly(true)]
		public override ZShort CY_Order
		{
			get => base.CY_Order;
			set => base.CY_Order = value;
		}
		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobComInvoiceLine));

		protected override Customs.Business.CusCodeDataValidation GetNewValidation() => new VehicleNumberValidation(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.VehicleNumber;
			CY_Code = CusCodeDataTypeList.Codes.VehicleNumber;
		}
	}
}
