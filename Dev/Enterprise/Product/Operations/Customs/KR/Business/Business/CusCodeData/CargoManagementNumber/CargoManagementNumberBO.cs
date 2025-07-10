using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class CargoManagementNumberBO : CusCodeData
	{
		public CargoManagementNumberBO(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZString CY_Data
		{
			get => base.CY_Data;
			set
			{
				if (string.IsNullOrEmpty(base.CY_Data) && !string.IsNullOrEmpty(value) && Parent is Bill bill)
				{
					CY_Order = (ZShort)bill.CargoManagementNumbers.Count;
				}
				base.CY_Data = value;
			}
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(Bill));

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.CargoManagementNumber;
		}
	}
}
