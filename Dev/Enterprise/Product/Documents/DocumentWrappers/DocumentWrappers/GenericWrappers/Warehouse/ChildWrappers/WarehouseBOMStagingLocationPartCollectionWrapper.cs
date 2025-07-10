using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	#region WarehouseBOMStagingLocationPartWrapperCollection

	public class WarehouseBOMStagingLocationPartWrapperCollection : WarehouseGenericWrapperCollection<WarehouseBOMStagingLocationPartWrapper>
	{
		public WarehouseBOMStagingLocationPartWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WarehouseBOMStagingLocationPartWrapperCollection(WarehouseBOMStagingLocationPartCollection collectionToWrap, BusinessObjectFactory factory)
			: base(collectionToWrap, factory)
		{
		}
	}

	#endregion

	#region WarehouseBOMStagingLocationPartCollection

	public class WarehouseBOMStagingLocationPartCollection : NonPersistentBusinessObjectCollection<WarehouseBOMStagingLocationPart>
	{
		#region Constructors

		public WarehouseBOMStagingLocationPartCollection(WhsPick pick, BusinessObjectFactory factoryToWrap)
			: base(factoryToWrap)
		{
			RollupStagingLocationParts(pick);
		}

		#endregion

		#region overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new WarehouseBOMStagingLocationPart();
		}

		#endregion

		#region Rollups

		void RollupStagingLocationParts(WhsPick pick)
		{
			foreach (var order in pick.Orders.OfType<WhsWorkOrder>())
			{
				RollupPickOrder(order);
			}
		}

		void RollupPickOrder(WhsWorkOrder order)
		{
			foreach (WhsWorkOrderLine orderLine in order.Lines.Where(ol => ol.IsBOMProduct))
			{
				foreach (var childLine in orderLine.BOM.ChildComponentLines)
				{
					var location = orderLine.StagingLocationBOM;

					if (location != null)
					{
						var stagingLocationPart = FindOrCreatePart(childLine.SupplierPart, orderLine.StagingLocationBOM);
						stagingLocationPart.AddWorkOrderLine(childLine);
					}
				}
			}
		}

		WarehouseBOMStagingLocationPart FindOrCreatePart(OrgSupplierPart matchingPart, WhsLocation matchingBOMLocation)
		{
			var stagingAreaPart = Elements
				.OfType<WarehouseBOMStagingLocationPart>()
				.FirstOrDefault(sap =>
					sap.SupplierPart.PK == matchingPart.PK &&
					sap.BOMStagingLocation.PK == matchingBOMLocation.PK);

			return stagingAreaPart ?? AddNew();
		}

		#endregion
	}

	#endregion
}
