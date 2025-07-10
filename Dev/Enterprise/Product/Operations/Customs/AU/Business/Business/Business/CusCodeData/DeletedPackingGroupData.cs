using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class DeletedPackingGroupData : CusCodeData
	{
		public DeletedPackingGroupData(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusEntryHeader Parent
		{
			get { return (CusEntryHeader)base.Parent; }
			set { base.Parent = value; }
		}

		public ZShort Number
		{
			get { return ZShort.ParseSafe(CY_Data, 0); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.DeletedPackingGroupData;
			CY_Code = CusCodeDataTypeList.Codes.DeletedPackingGroupData;
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(CusEntryHeader)); }
		}
	}
}
