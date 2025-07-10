using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ImmediateDelivery : CusCodeData
	{
		public ImmediateDelivery(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : CusCodeData.Schema
		{
			public new const int CY_DataMaxLength = 18;
		}
		public override bool SupportsNotes => false;

		[ResourceStringData("82338471-12A2-4865-B677-3A0B50532FC6", Caption = "Seq #")]
		public override ZShort CY_Order { get => base.CY_Order; set => base.CY_Order = value; }

		[MaxLength(Schema.CY_DataMaxLength)]
		[ResourceStringData("0441449C-1E3B-42AE-A059-F210C73AF462", Caption = "Immediate Delivery No")]
		public override ZString CY_Data { get => base.CY_Data; set => base.CY_Data = value; }

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobComInvoiceLine));

		protected override Customs.Business.CusCodeDataValidation GetNewValidation() => new ImmediateDeliveryValidation(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.ImmediateDelivery;
		}
	}
}
