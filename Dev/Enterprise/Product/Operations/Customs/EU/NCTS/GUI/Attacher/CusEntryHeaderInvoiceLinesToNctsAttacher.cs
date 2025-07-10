using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.CommonGoodsItemsIntegration;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	class CusEntryHeaderInvoiceLinesToNctsAttacher : ZRecordAttacher
	{
		public CusEntryHeaderInvoiceLinesToNctsAttacher(NctsHeader nctsHeader, IBusinessObjectCollection findBoxList)
			: base(null, findBoxList, ModuleIDs.Customs.EntryHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(NctsHeader));
		}

		public int TargetConsignmentIndex { get; set; }

		readonly NctsHeader nctsHeader;

		protected override bool AttachCore(BusinessObject bizO, System.Collections.Generic.List<BusinessObject> listToBulkAdd)
		{
			if (bizO is ICommonGoodsItemsIntegratorProvider provider)
			{
				var commonGoodsItems = provider.CommonGoodsItemsIntegrator.GetCommonGoodsItemsForIntegration();
				nctsHeader.CommonGoodsItemsIntegrator.CopyCommonGoodsItems(commonGoodsItems, TargetConsignmentIndex);
			}

			return true;
		}
	}
}
