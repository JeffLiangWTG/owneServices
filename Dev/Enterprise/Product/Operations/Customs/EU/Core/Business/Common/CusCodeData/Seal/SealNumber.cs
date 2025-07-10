using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business
{
	public class SealNumber : Customs.Business.CusCodeData
	{
		public SealNumber(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : Customs.Business.CusCodeData.Schema
		{
			public new const int CY_DataMaxLength = 20;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.Seal;
			CY_Code = CusCodeDataTypeList.Codes.Seal;
		}

		[MaxLength(Schema.CY_DataMaxLength)]
		[ResourceStringData("692E3D88-C83D-4A4A-8DB1-38AE0B3F7372", Caption = "Seal Number")]
		public override ZString CY_Data { get => base.CY_Data; set => base.CY_Data = value; }

		protected override Customs.Business.CusCodeDataValidation GetNewValidation() => new SealNumberValidation(this);

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(CusEntryInstruction));
	}
}
