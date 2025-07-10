using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class Seal : CusCodeData
	{
		public Seal(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("73BCE926-AB33-4062-A00D-76D2EEEE38E3", Caption = "Seal Number")]
		public override ZString CY_Data { get => base.CY_Data; set => base.CY_Data = value; }

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(NctsHeader));

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.Seal;
			CY_Code = CusCodeDataTypeList.Codes.Seal;
		}

		protected override Customs.Business.CusCodeDataLookups GetNewLookups() => new CusCodeDataLookups(this);
	}
}
