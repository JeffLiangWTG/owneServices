using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business
{
	public class ImportInventorySelectionHeader : InventorySelectionHeader
	{
		public ImportInventorySelectionHeader(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override void SetFormattedProcedure(JobComInvoiceLine invoiceLine, IWhsDocketLine receiveLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute)
			=> invoiceLine.JI_FormattedProcedure = (invoiceLine.EntryInstruction?.CEI_Procedure ?? (ZString)ImportMainProcedureCodeList.Codes._40) + receiveLine.CustomsData.WB_InwardProcedure.LeftOrNull(2);

		protected override BaseJobComInvoiceHeader GetMatchingInvoiceHeaderAndPopulateData(InvoiceHeaderGroupingDefinitionProvider invoiceHeaderGroupingDefinitionProvider)
		{
			var invoiceHeader = Declaration.Invoices.FirstOrDefault(x =>
				x.JZ_RX_NKInvoice_Currency == invoiceHeaderGroupingDefinitionProvider.LinePriceCurrency
				&& x.IncoTerm == invoiceHeaderGroupingDefinitionProvider.IncotermCode
				&& x.JZ_IncoTermPlace == invoiceHeaderGroupingDefinitionProvider.IncotermPlace
				&& x.JZ_ValuationCode == invoiceHeaderGroupingDefinitionProvider.TransNature)
					?? Declaration.Invoices.AddNew();

			invoiceHeader.JZ_RX_NKInvoice_Currency = invoiceHeaderGroupingDefinitionProvider.LinePriceCurrency;
			invoiceHeader.JZ_IncoTerm = invoiceHeaderGroupingDefinitionProvider.IncotermCode;
			invoiceHeader.JZ_IncoTermPlace = invoiceHeaderGroupingDefinitionProvider.IncotermPlace;
			invoiceHeader.JZ_ValuationCode = invoiceHeaderGroupingDefinitionProvider.TransNature;
			return invoiceHeader;
		}

		protected override void FillInvoiceLineWithInventoryDetails(EU.Business.Declaration.JobComInvoiceLine invoiceLine, IWhsDocketLine whsReceiveLine,
			IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, ZDecimal ratio, WhsInventoryWrapper inventoryWrapper = null)
		{
			base.FillInvoiceLineWithInventoryDetails(invoiceLine, whsReceiveLine, whsBondedWarehouseAttribute, ratio, inventoryWrapper);
			var deInvoiceLine = (JobComInvoiceLine)invoiceLine;

			FillSupportingDocumentsHeader(deInvoiceLine.InvoiceHeader, whsBondedWarehouseAttribute);
			FillSupportingDocumentsLine(deInvoiceLine, whsBondedWarehouseAttribute);
			PopulateSecondQuantityFromInventory(deInvoiceLine, whsBondedWarehouseAttribute);
		}

		void PopulateSecondQuantityFromInventory(JobComInvoiceLine invoiceLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute)
		{
			if (invoiceLine.JI_CustomsSecondUnitQty.IsEmpty)
			{
				var tariffCode = invoiceLine.JI_Tariff;
				var tariff = Factory.LoadTop1<TariffView>(new ZQuery(TariffViewSchema.ZZ1_TariffCode, tariffCode).AddToFilter(TariffViewSchema.ZZ1_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.Germany));

				if (tariff != null)
				{
					var additionalUnitOfMeasure = tariff.UnitsOfMeasure.FirstOrDefault(x => x.ZZ8_Type == Constants.UnitOfMeasureTypes.AdditionalUOMType);
					if (additionalUnitOfMeasure != null)
					{
						invoiceLine.JI_CustomsSecondUnitQty = additionalUnitOfMeasure.ZZ8_UOM.Left(invoiceLine.JI_CustomsSecondUnitQtyInfo.MaxLength);
						if (invoiceLine.JI_CustomsSecondUnitQty == invoiceLine.JI_BondedWhsUnitQty)
						{
							invoiceLine.JI_CustomsSecondQuantity = invoiceLine.JI_BondedWhsQuantity;
						}
					}
				}
			}
		}

		protected override void PopulateCountrySpecificInvoiceLineData(BaseJobComInvoiceLine invoiceLine, WhsInventoryWrapper inventoryWrapper, IWhsDocketLine whsReceiveLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, Dictionary<ZString, ZString> addInfos, ZDecimal invoiceQuantity, ZDecimal ratio)
		{
			base.PopulateCountrySpecificInvoiceLineData(invoiceLine, inventoryWrapper, whsReceiveLine, whsBondedWarehouseAttribute, addInfos, invoiceQuantity, ratio);
			if (inventoryWrapper.Order is IWhsDocket order && inventoryWrapper.OrderLines.SingleOrDefault() is IWhsDocketLine orderLine)
			{
				invoiceLine.JI_BondedWHSOrderNumber = order.WD_DocketID;
				invoiceLine.JI_BondedWHSOrderLineNumber = orderLine.WE_LineNo;
			}
		}

		void FillSupportingDocumentsHeader(JobComInvoiceHeader invoiceHeader, IWhsBondedWarehouseAttribute source)
		{
			var query = new ZQuery(CusAddInfoSchema.B7_ParentID, source.PK);
			query.AddToFilter(CusAddInfoSchema.B7_ParentTableCode, WhsBondedWarehouseAttributeSchema.Constants.Prefix);
			query.AddToFilter(CusAddInfoSchema.B7_Type, BondedWarehousingHelper.Constants.InvoiceSupportingDocumentAddInfoTypes.InvoiceHeader);

			var warehouseCustomsAddInfos = Factory.Load<IWarehouseCustomsAddInfo>(query);

			foreach (var warehouseCustomsAddInfo in warehouseCustomsAddInfos)
			{
				var provider = new InvoiceSupportingDocumentAddInfoDefinitionProvider(warehouseCustomsAddInfo);

				var supportingDocumentExists = invoiceHeader.SupportingDocuments
					.Cast<SupportingDocument>()
					.Any(d => d.CSI_Code == provider.Type && d.CSI_ReferenceNumber == provider.Reference && d.CSI_DateOfIssue == provider.DateOfIssue);

				if (!supportingDocumentExists)
				{
					var supportingDocument = invoiceHeader.SupportingDocuments.AddNew(provider.Type, provider.Reference);
					supportingDocument.CSI_DateOfIssue = provider.DateOfIssue;
				}
			}
		}

		void FillSupportingDocumentsLine(JobComInvoiceLine invoiceLine, IWhsBondedWarehouseAttribute source)
		{
			var query = new ZQuery(CusAddInfoSchema.B7_ParentID, source.PK);
			query.AddToFilter(CusAddInfoSchema.B7_ParentTableCode, WhsBondedWarehouseAttributeSchema.Constants.Prefix);
			query.AddToFilter(CusAddInfoSchema.B7_Type, BondedWarehousingHelper.Constants.InvoiceSupportingDocumentAddInfoTypes.InvoiceLine);

			var warehouseCustomsAddInfos = Factory.Load<IWarehouseCustomsAddInfo>(query);

			foreach (var warehouseCustomsAddInfo in warehouseCustomsAddInfos)
			{
				var provider = new InvoiceSupportingDocumentAddInfoDefinitionProvider(warehouseCustomsAddInfo);

				var supportingDocumentExists = invoiceLine.SupportingDocuments
					.Cast<SupportingDocument>()
					.Any(d => d.KeyToDeterimeUniqueness == provider.Type + provider.Reference);

				if (!supportingDocumentExists)
				{
					var supportingDocument = invoiceLine.SupportingDocuments.AddNew(provider.Type, provider.Reference);
					supportingDocument.CSI_DateOfIssue = provider.DateOfIssue;

					supportingDocument.CSI_Status = provider.Available;
					supportingDocument.CSI_Quantity = provider.UnitOfMeasure.IsEmpty ? 0 : invoiceLine.JI_BondedWhsQuantity;
					supportingDocument.CSI_UnitOfQuantity = provider.UnitOfMeasure;
				}
			}
		}

		[ChildEditable]
		public new InventorySelectionLineCollection SelectionLines => (InventorySelectionLineCollection)base.SelectionLines;

		protected override IInventorySelectionLineCollection<Customs.Business.InventorySelectionLine> GetNewInventorySelectionLineCollection() => new InventorySelectionLineCollection(this);
	}
}
