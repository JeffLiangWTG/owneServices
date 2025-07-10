using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehouseAdjustmentConfirmationWrapper : WarehouseDocketWrapper, IWhsDocumentInventory
	{
		#region Constructor

		public WarehouseAdjustmentConfirmationWrapper(WhsAdjustment adjustment, BusinessObjectFactory factoryToWrap)
			: base(adjustment, factoryToWrap)
		{
		}

		#endregion

		#region Static Memebers

		public static WarehouseAdjustmentConfirmationWrapper New(WhsAdjustment adjustment, BusinessObjectFactory factoryToWrap)
		{
			WarehouseAdjustmentConfirmationWrapper result = null;
			if (adjustment != null)
			{
				result = new WarehouseAdjustmentConfirmationWrapper(adjustment, factoryToWrap);
			}
			return result;
		}

		#endregion

		#region DocumentTitle

		protected override ZString DocumentTitleCore => Res.GetString("99598fd6-b9e5-41dc-8eaa-da7f2df81cb3", "Adjustment Confirmation");

		#endregion

		#region SecondaryReference

		protected override LabelValuePairWrapper SecondaryReferenceCore => Adjustment != null ? new LabelValuePairWrapper(Res.GetString("44e667eb-7432-48b6-b754-cd0b25138d9f", "Reference"), Adjustment.WD_ExternalReference, Factory) : LabelValuePairWrapper.Empty;

		#endregion

		#region GetJobLines

		protected override WarehouseGenericWrapperCollection GetJobLines()
		{
			WhsAdjustmentLineCollection adjustmentLines = (Adjustment != null) ? Adjustment.Lines : null;
			return new WarehouseAdjustmentLineWrapperCollection(adjustmentLines, Factory);
		}

		#endregion

		#region TransportCoAddress

		protected override AddressWrapper TransportCoAddressCore => AddressWrapper.Empty(Factory);

		#endregion

		#region Adjustment

		WhsAdjustment Adjustment => (WhsAdjustment)WrappedObject;

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

		#region JobType

		protected override ZString JobTypeCore => Res.GetString("497abd82-ee29-4dcd-8b3e-d2f0e2dc79f6", "Adjustment");

		#endregion
	}
}
