using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	class CusEntryHeaderEntryLinesToNctsAttacher : ZRecordAttacher
	{
		public CusEntryHeaderEntryLinesToNctsAttacher(NctsBill houseConsignment, IBusinessObjectCollection findBoxList)
			: base(null, findBoxList, ModuleIDs.Customs.EntryHeader)
		{
			this.houseConsignment = Argument.NotNull(houseConsignment, nameof(houseConsignment));
		}

		protected override bool AttachCore(BusinessObject bizO, System.Collections.Generic.List<BusinessObject> listToBulkAdd)
		{
			if (bizO is CusEntryHeader euEntryHeader)
			{
				var customsEntryIntegrator = houseConsignment.GetCustomsEntryIntegrator();
				customsEntryIntegrator.CopyCustomsEntry(euEntryHeader);
			}

			return true;
		}

		readonly NctsBill houseConsignment;
	}
}
