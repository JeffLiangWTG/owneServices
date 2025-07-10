using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ICSPermit : Customs.Business.CusCodeData
	{
		public ICSPermit(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : Customs.Business.CusCodeData.Schema
		{
			public new const int CY_DataMaxLength = 35;
		}

		[MaxLength(Schema.CY_DataMaxLength)]
		public override ZString CY_Data { get => base.CY_Data; set => base.CY_Data = value; }

		#region Parent

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(CusClassPartPivot), typeof(JobComInvoiceLine)); }
		}

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.ICSPermit;
			CY_Code = CusCodeDataTypeList.Codes.ICSPermit;
		}

		protected override Customs.Business.CusCodeDataValidation GetNewValidation() => new ICSPermitValidation(this);
	}
}
