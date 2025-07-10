using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Customs.IE.Business
{
	public class InventorySelectionHeader : EU.Business.InventorySelectionHeader
	{
		public InventorySelectionHeader(BaseJobDeclaration declaration) : base(declaration)
		{
		}

		protected override void FillPreviousDocuments(EU.Business.Declaration.JobComInvoiceLine invoiceLine, IWhsDocketLine whsReceiveLine)
		{
			base.FillPreviousDocuments(invoiceLine, whsReceiveLine);
			var components = GetLowestLevelComponents(whsReceiveLine).Select(x => x.Item1);
			foreach (var component in components.Where(x => x.CustomsData?.WB_EntryKey.IsEmpty == false))
			{
				var itemNumber = component.CustomsData.WB_EntryLineNo;
				var referenceNumber = component.CustomsData.WB_EntryKey;

				if (!invoiceLine.PreviousDocuments.Cast<PreviousDocument>().Any(x => x.CSI_ItemNumber == itemNumber && x.CSI_ReferenceNumber == referenceNumber))
				{
					var prevDocument = invoiceLine.PreviousDocuments.AddNew();
					prevDocument.CSI_ItemNumber = itemNumber;
					prevDocument.CSI_ReferenceNumber = referenceNumber;
				}
			}
		}
	}
}
