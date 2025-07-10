using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Customs.FR.Business
{
	public class InventorySelectionHeader : EU.Business.InventorySelectionHeader
	{
		public InventorySelectionHeader(BaseJobDeclaration declaration) : base(declaration)
		{
		}

		protected override void FillPreviousDocuments(EU.Business.Declaration.JobComInvoiceLine invoiceLine, IWhsDocketLine inventory)
		{
			base.FillPreviousDocuments(invoiceLine, inventory);
			var components = GetLowestLevelComponents(inventory).Select(x => x.Item1);
			foreach (var component in components.Where(x => x.CustomsData?.WB_EntryKey.IsEmpty == false))
			{
				var entryDate = component.CustomsData.WB_EntryDate;
				var referenceNumber = component.CustomsData.WB_EntryKey;

				if (!invoiceLine.PreviousDocuments.Cast<PreviousDocument>().Any(x => x.CSI_Code == PreviousDocumentCodeList.Codes.IM && x.CSI_DateOfIssue == entryDate && x.CSI_ReferenceNumber == referenceNumber))
				{
					var prevDocument = invoiceLine.PreviousDocuments.AddNew();
					prevDocument.CSI_Code = PreviousDocumentCodeList.Codes.IM;
					prevDocument.CSI_DateOfIssue = entryDate;
					prevDocument.CSI_ReferenceNumber = referenceNumber;
				}
			}
		}

		protected override void PopulateComponentInventory(WhsInventoryWrapper inventoryWrapper, BaseJobComInvoiceLine invoiceLine)
		{
			if (!inventoryWrapper.AllocationKey.IsEmpty)
			{
				base.PopulateComponentInventory(inventoryWrapper, invoiceLine);
			}
		}
	}
}
