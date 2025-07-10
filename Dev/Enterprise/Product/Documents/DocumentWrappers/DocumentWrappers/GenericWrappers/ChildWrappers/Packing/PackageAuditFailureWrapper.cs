using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("PartDescription"), WrapperTypeName("PackageAuditFailureWrapper")]
	public class PackageAuditFailureWrapper : GenericWrapper
	{
		public PackageAuditFailureWrapper(WhsPackageAuditLineFailure auditFailureToWrap, BusinessObjectFactory factory)
			: base(auditFailureToWrap, factory)
		{ }

		// This is used by Load method in PackageAuditFailureWrapperCollection to avoid recursive call chains to BusinessObject.Add().
		public static PackageAuditFailureWrapper New(WhsPackageAuditLineFailure auditFailureToWrap, BusinessObjectFactory factory)
		{
			return new PackageAuditFailureWrapper(auditFailureToWrap, factory);
		}

		#region Properties

		public ZDecimal AuditedQuantity
		{
			get { return AuditFailure?.WPF_AuditedQty ?? ZDecimal.Zero; }
		}

		public ZDecimal ExpectedQuantity
		{
			get { return AuditFailure?.WPF_ExpectedQty ?? ZDecimal.Zero; }
		}

		public ZString Location
		{
			get
			{
				ZString result;

				var locations = GetPickLines().Select(pl => pl.InventoryLineForAvailableInventory.Location).Distinct().ToArray();
				if (locations.Length > 0)
				{
					result = locations.Length == 1 ? locations[0].WLV_LocationString : (ZString)"MULTIPLE";
				}
				else
				{
					result = ZString.Empty;
				}

				return result;
			}
		}

		public ZDecimal Variance
		{
			get { return AuditedQuantity - ExpectedQuantity; }
		}

		public ZString PartNum
		{
			get { return AuditFailure != null ? AuditFailure.SupplierPart.OP_PartNum : ZString.Empty; }
		}

		public ZString PartDescription
		{
			get { return AuditFailure != null ? AuditFailure.SupplierPart.OP_Desc : ZString.Empty; }
		}

		public ZString Picker
		{
			get
			{
				ZString result;

				var pickers = GetPickLines().GetOriginallyPickedPickLines().Select(pl => pl.AssignedToCode).Distinct().ToArray();
				if (pickers.Length > 0)
				{
					result = pickers.Length == 1 ? pickers[0] : (ZString)"MULTIPLE";
				}
				else
				{
					result = ZString.Empty;
				}

				return result;
			}
		}

		public ZString ProductBarcode
		{
			get
			{
				ZString barcodeToProcess = new();

				var supplierPart = AuditFailure?.SupplierPart;
				if (supplierPart != null)
				{
					barcodeToProcess = supplierPart.PartBarcodes.FindUseForDocumentsPartBarcodeByPackage(supplierPart.OP_StockKeepingUnit)?.PH_Barcode ?? "";
				}

				if (barcodeToProcess.IsEmpty)
				{
					barcodeToProcess = PartNum;
				}

				const bool USE_OPTIMISED_ENCODING = true;
				const bool IS_GS1_128_BarCode = true;
				var textBarcode = new TextBarcode(barcodeToProcess, USE_OPTIMISED_ENCODING, IS_GS1_128_BarCode);
				return textBarcode.TextAs128sFontString;
			}
		}

		#endregion

		#region GetPickLines

		IEnumerable<WhsPickLine> GetPickLines()
		{
			IEnumerable<WhsPickLine> pickLines;

			if (AuditFailure != null)
			{
				var package = Package;
				var packableItems = package?.PackedItemDivots.Select(id => id.PackedItem).Where(p => p != null);
				pickLines = packableItems?.Cast<WhsPickLine>().Distinct()
					.Where(pl => pl.ProductCode == PartNum).ToArray() ?? Array.Empty<WhsPickLine>();
			}
			else
			{
				pickLines = Array.Empty<WhsPickLine>();
			}

			return pickLines;
		}

		#endregion

		#region AuditFailure

		WhsPackageAuditLineFailure AuditFailure
		{
			get { return (WhsPackageAuditLineFailure)WrappedBO; }
		}

		#endregion

		#region Package

		PkgPackage Package
		{
			get { return AuditFailure?.PackageAudit?.Order?.PackageJob?.GetAllPackagesOnJob().Where(p => p.KP_PackageID == AuditFailure.PackageAudit.WPA_PackageID).FirstOrDefault(); }
		}

		#endregion
	}
}
