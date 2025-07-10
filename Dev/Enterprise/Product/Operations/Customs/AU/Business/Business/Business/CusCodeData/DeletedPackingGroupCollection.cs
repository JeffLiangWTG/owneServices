using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class DeletedPackingGroupCollection : CusCodeDataCollection<DeletedPackingGroupData>
	{
		public DeletedPackingGroupCollection(CusEntryHeader header)
			: base(header, CusCodeDataTypeList.Codes.DeletedPackingGroupData)
		{
		}

		public DeletedPackingGroupData AddNew(ZShort number)
		{
			return AddNew(CusCodeDataTypeList.Codes.DeletedPackingGroupData, number.ToString());
		}
	}
}
