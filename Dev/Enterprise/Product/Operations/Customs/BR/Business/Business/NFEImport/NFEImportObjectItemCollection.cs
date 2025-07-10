using CargoWise.Common;
using CargoWise.Customs.BR.MessageDefinitions.Export.Incoming;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public class NFEImportObjectItemCollection : NonPersistentBusinessObjectCollection<NFEImportObjectItem>
	{
		public NFEImportObjectItemCollection(NFEImportObject parent)
			: base(parent.Factory)
		{
			this.parent = Argument.NotNull(parent, nameof(parent));
		}

		readonly NFEImportObject parent;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new NFEImportObjectItem(Factory);
		}

		public void AddNewItems(nfeProcNFeInfNFeDet[] items)
		{
			if (items != null && parent != null)
			{
				foreach (var item in items)
				{
					var newElement = AddNew();
					newElement.NfeItemNumber = item.nItem ?? ZString.Empty;
					newElement.ProductCode = item.prod?.cProd?.Length <= NFEImportObjectItem.Schema.ProductCodeMaxLength ? (ZString)item.prod?.cProd : ZString.Empty;
					newElement.GoodsDescription = item.prod?.xProd ?? ZString.Empty;
					newElement.TariffCode = item.prod?.NCM ?? ZString.Empty;
					newElement.InvoiceQuantityUQ = item.prod?.uCom ?? ZString.Empty;
					newElement.InvoiceQuantity = item.prod?.qCom ?? ZDecimal.Zero;
					newElement.CustomsQuantityUQ = item.prod?.uTrib ?? ZString.Empty;
					newElement.CustomsQuantity = item.prod?.qTrib ?? ZDecimal.Zero;
					newElement.ComplementartDescription = item.infAdProd ?? ZString.Empty;
					newElement.FreteValue = item.prod?.vFrete ?? ZDecimal.Zero;
					newElement.SegValue = item.prod?.vSeg ?? ZDecimal.Zero;
					newElement.OutroValue = item.prod?.vOutro ?? ZDecimal.Zero;
					newElement.DescValue = item.prod?.vDesc ?? ZDecimal.Zero;

					var vProd = item.prod?.vProd ?? ZDecimal.Zero;
					newElement.TotalNfeValue = vProd + newElement.FreteValue + newElement.SegValue + newElement.OutroValue - newElement.DescValue;
					newElement.TotalValue = vProd;
				}
			}
		}
	}
}
