using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class OnlineOrder : CusCodeData
	{
		public OnlineOrder(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : CusCodeData.Schema
		{
			public const int CY_Data_MaxLength = 35;
			public const int CY_Order_MaxLength = 2;
		}
		public override bool SupportsNotes => false;

		[ResourceStringData("3C1B031B-0618-4889-8BEE-37ED890F768C", Caption = "Order No")]
		[MaxLength(Schema.CY_Data_MaxLength)]
		public override ZString CY_Data
		{
			get => base.CY_Data;
			set => base.CY_Data = value;
		}

		[ResourceStringData("A26D18ED-47B4-41B5-9943-4D4E777CF703", Caption = "Seq #")]
		[MaxLength(Schema.CY_Order_MaxLength)]
		public override ZShort CY_Order
		{
			get => base.CY_Order;
			set => base.CY_Order = value;
		}

		public override bool CY_DataAllowWesternEuropeanCharactersOnly => false;
		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(CusEntryInstruction));

		protected override Customs.Business.CusCodeDataValidation GetNewValidation() => new OnlineOrderValidation(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.OnlineOrder;
		}
	}
}
