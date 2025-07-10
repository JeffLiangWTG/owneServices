using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Client.UPE.Business
{
	public class UPEPrintBatchItemCollection : DependentBusinessObjectCollection<UPEPrintBatchItem, UPEPrintBatch>
	{
		public UPEPrintBatchItemCollection(UPEPrintBatch printBatch)
			: base(printBatch)
		{
		}

		public UPEPrintBatchItem AddNew(DocumentEngine.Business.StmMenuItemBase menuItem, IDocumentSupportable bizObj)
		{
			return AddNew(menuItem.PK, bizObj);
		}

		public UPEPrintBatchItem AddNew(ZGuid menuItemPK, IDocumentSupportable bizObj)
		{
			UPEPrintBatchItem result = AddNew();
			result.T6_SU = menuItemPK;
			result.T6_ParentID = ((IBusiness)bizObj).Identifier;
			return result;
		}
	}
}
