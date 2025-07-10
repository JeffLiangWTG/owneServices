using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehouseBOMStagingLocationPartWrapper : WarehouseGenericLineWrapper
	{
		#region Constructors

		public WarehouseBOMStagingLocationPartWrapper(WarehouseBOMStagingLocationPart objectToWrap, BusinessObjectFactory factoryToWrap)
			: base(objectToWrap, factoryToWrap)
		{
		}

		#endregion

		#region Properties

		public OrgSupplierPart SupplierPart
		{
			get { return WarehouseBOMStagingLocationPart.SupplierPart; }
		}

		public WhsLocation BOMStagingLocation
		{
			get { return WarehouseBOMStagingLocationPart.BOMStagingLocation; }
		}

		public WarehouseBOMStagingLocationPart WarehouseBOMStagingLocationPart
		{
			get { return (WarehouseBOMStagingLocationPart)WrappedBO; }
		}

		#endregion

		#region Wrapped Properties

		protected override ZDecimal UnitsCore
		{
			get { return WarehouseBOMStagingLocationPart != null ? WarehouseBOMStagingLocationPart.TotalUnits : new ZDecimal(0m); }
		}

		protected override ZString ProductDescriptionCore
		{
			get
			{
				var productDescription = ZString.Empty;
				if (WarehouseBOMStagingLocationPart != null && WarehouseBOMStagingLocationPart.SupplierPart != null)
				{
					productDescription = WarehouseBOMStagingLocationPart.SupplierPart.OP_Desc;
				}
				return productDescription;
			}
		}

		protected override ZString ProductCodeCore
		{
			get
			{
				var productCode = ZString.Empty;
				if (WarehouseBOMStagingLocationPart != null && WarehouseBOMStagingLocationPart.SupplierPart != null)
				{
					productCode = WarehouseBOMStagingLocationPart.SupplierPart.OP_PartNum;
				}
				return productCode;
			}
		}

		protected override ZString StagingAreaNameCore
		{
			get
			{
				var stagingLocationInfo = ZString.Empty;
				if (WarehouseBOMStagingLocationPart != null && WarehouseBOMStagingLocationPart.BOMStagingLocation != null)
				{
					stagingLocationInfo = WarehouseBOMStagingLocationPart.BOMStagingLocation.WLV_LocationString;
				}
				return stagingLocationInfo;
			}
		}

		protected override ZString UnitsUQCore
		{
			get { return WarehouseBOMStagingLocationPart != null ? WarehouseBOMStagingLocationPart.PackType : ZString.Empty; }
		}

		#endregion

		#region Static

		public static WarehouseBOMStagingLocationPartWrapper New(WarehouseBOMStagingLocationPart part, BusinessObjectFactory factoryToWrap)
		{
			return new WarehouseBOMStagingLocationPartWrapper(part, factoryToWrap);
		}

		#endregion
	}

	public class WarehouseBOMStagingLocationPart : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Implementation

		public void AddWorkOrderLine(WhsWorkOrderLine workOrderLine)
		{
			if (workOrderLine != null && workOrderLine.ParentLine.IsBOMProduct && workOrderLine.ParentLine.StagingLocationBOM != null)
			{
				orderLines.Add(workOrderLine);
			}
		}

		#endregion

		#region Properties

		// Total the units across all pick lines
		public ZDecimal TotalUnits
		{
			get
			{
				ZDecimal totalUnits = 0;
				if (orderLines != null && orderLines.Count > 0)
				{
					totalUnits = orderLines.Sum(ol => ol.PickLineQuantity);
				}

				return totalUnits;
			}
		}

		// All picks are carried out in base units so we can just look at the first packtype
		public ZString PackType
		{
			get
			{
				ZString packType = ZString.Empty;
				if (orderLines != null && orderLines.Count > 0)
				{
					packType = orderLines[0].SupplierPart.OP_StockKeepingUnit;
				}
				return packType;
			}
		}

		public OrgSupplierPart SupplierPart
		{
			get
			{
				if (supplierPart == null)
				{
					if (orderLines != null && orderLines.Count > 0)
					{
						supplierPart = orderLines[0].SupplierPart;
					}
				}
				return supplierPart;
			}
		}

		public WhsLocation BOMStagingLocation
		{
			get
			{
				if (bomStagingLocation == null)
				{
					if (orderLines != null && orderLines.Count > 0 && orderLines[0].ParentLine != null && orderLines[0].ParentLine.StagingLocationBOM != null)
					{
						bomStagingLocation = orderLines[0].ParentLine.StagingLocationBOM;
					}
				}
				return bomStagingLocation;
			}
		}

		#endregion

		#region Internals

		OrgSupplierPart supplierPart;
		WhsLocation bomStagingLocation;
		readonly List<WhsWorkOrderLine> orderLines = new List<WhsWorkOrderLine>();

		#endregion
	}
}
