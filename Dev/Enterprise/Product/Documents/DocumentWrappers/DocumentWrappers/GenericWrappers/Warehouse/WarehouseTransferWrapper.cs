using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers.Warehouse
{
	public class WarehouseTransferWrapper : WarehouseDocketWrapper, IWhsDocumentInventory
	{
		public WarehouseTransferWrapper(WhsTransfer transfer, BusinessObjectFactory factory)
			: base(transfer, factory)
		{
		}

		#region GetJobLines

		protected override WarehouseGenericWrapperCollection GetJobLines()
		{
			return Transfer != null ? new WarehouseTransferLineWrapperCollection(Transfer.Lines, Factory) : new WarehouseTransferLineWrapperCollection(Factory);
		}

		#endregion

		#region SecondaryReferenceCore

		protected override LabelValuePairWrapper SecondaryReferenceCore
		{
			get
			{
				if (Transfer != null && Transfer.WD_ExternalReference != "TRANSFER")
				{
					return new LabelValuePairWrapper(Res.GetString("d8701e8e-d4df-4c43-bbb5-e3f7f38312b3", "Transfer Reference"), Transfer.WD_ExternalReference, Factory);
				}
				return LabelValuePairWrapper.Empty;
			}
		}

		#endregion

		#region TransportCoAddress

		protected override AddressWrapper TransportCoAddressCore => AddressWrapper.Empty(Factory);

		#endregion

		#region JobType

		protected override ZString JobTypeCore => Res.GetString("cb8a71d0-e12b-47c9-81f7-5ff732d07a9e", "Transfer");

		#endregion

		#region IWhsDocumentInventory Members

		void IWhsDocumentInventory.SetInventory(WhsDocumentInventory documentInventory)
		{
			JobLines.RemoveAll();

			for (int i = 0; i < documentInventory.LabelsToPrint; i++)
			{
				JobLines.Add(new WarehouseInventoryWrapper(documentInventory.Inventory.InDocketLine, Factory));
			}
		}

		#endregion

		#region Transfer

		WhsTransfer Transfer => transfer ?? (transfer = (WhsTransfer)DocketBO);
		WhsTransfer transfer;

		#endregion
	}
}
