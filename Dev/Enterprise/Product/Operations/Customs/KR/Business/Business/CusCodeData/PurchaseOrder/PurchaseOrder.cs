using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class PurchaseOrder : CusCodeData
	{
		public PurchaseOrder(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : CusCodeData.Schema
		{
			public const int CY_Data_MaxLength = 40;
		}
		public override bool SupportsNotes => false;

		[MaxLength(Schema.CY_Data_MaxLength)]
		public override ZString CY_Data
		{
			get => base.CY_Data;
			set => base.CY_Data = value;
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobComInvoiceHeader));

		protected override Customs.Business.CusCodeDataValidation GetNewValidation() => new PurchaseOrderValidation(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.PurchaseOrder;
		}
	}
}
