using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using static Enterprise.Customs.DE.Business.BondedWarehousingHelper.Constants;

namespace Enterprise.Customs.DE.Business
{
	public abstract class InventorySelectionHeader : EU.Business.InventorySelectionHeader
	{
		protected InventorySelectionHeader(JobDeclaration declaration) : base(declaration)
		{
		}

		protected abstract void SetFormattedProcedure(JobComInvoiceLine invoiceLine, IWhsDocketLine receiveLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute);

		protected abstract BaseJobComInvoiceHeader GetMatchingInvoiceHeaderAndPopulateData(InvoiceHeaderGroupingDefinitionProvider invoiceHeaderGroupingDefinitionProvider);

		protected override void FillInvoiceLineWithInventoryDetails(EU.Business.Declaration.JobComInvoiceLine invoiceLine, IWhsDocketLine whsReceiveLine,
			IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, ZDecimal ratio, WhsInventoryWrapper inventoryWrapper = null)
		{
			base.FillInvoiceLineWithInventoryDetails(invoiceLine, whsReceiveLine, whsBondedWarehouseAttribute, ratio, inventoryWrapper);

			var addInfosDictionary = PopulateAddInfoData(whsBondedWarehouseAttribute);
			var deInvoiceLine = invoiceLine as JobComInvoiceLine;
			var bondedWhsUnit = deInvoiceLine.JI_BondedWhsUnitQty;
			var bondedWhsQuantity = deInvoiceLine.JI_BondedWhsQuantity;
			SetFormattedProcedure(deInvoiceLine, whsReceiveLine, whsBondedWarehouseAttribute);
			deInvoiceLine.JI_BondedWhsQuantity = bondedWhsQuantity;
			deInvoiceLine.JI_BondedWhsUnitQty = bondedWhsUnit;
			deInvoiceLine.JI_Weight = deInvoiceLine.JI_CustomsQuantity;
			FillNetPrice((JobComInvoiceLine)invoiceLine, ratio, addInfosDictionary);
		}

		protected override BaseJobComInvoiceHeader GetFirstOrCreateNewInvoiceHeader(IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute)
		{
			BaseJobComInvoiceHeader invoiceHeader;
			var invoiceHeaderGroupingDefinition = new InvoiceHeaderGroupingDefinitionProvider(whsBondedWarehouseAttribute, Factory);
			if (!invoiceHeaderGroupingDefinition.LinePriceCurrency.IsEmpty)
			{
				invoiceHeader = GetMatchingInvoiceHeaderAndPopulateData(invoiceHeaderGroupingDefinition);
			}
			else
			{
				invoiceHeader = base.GetFirstOrCreateNewInvoiceHeader(whsBondedWarehouseAttribute);
			}
			return invoiceHeader;
		}

		protected override ZString GetInvoiceLineCurrencyCode(BaseJobComInvoiceLine baseJobComInvoiceLine) => baseJobComInvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency;

		void FillNetPrice(JobComInvoiceLine invoiceLine, ZDecimal ratio, Dictionary<ZString, ZString> addInfosDictionary)
		{
			if (addInfosDictionary.TryGetValue(WarehouseCustomsLineDetailsAddInfoKeys.LineNetPrice, out var netPriceString)
				&& ZDecimal.TryParse(netPriceString, out var netPrice)
				&& addInfosDictionary.TryGetValue(EU.Business.BondedWarehousingHelper.Constants.LinePriceCurrency, out var linePriceCurrency))
			{
				var linePriceRefCurrency = RefCurrency.LoadFromCurrencyCode(Factory, linePriceCurrency);
				if (linePriceRefCurrency != null)
				{
					var linePriceMoney = new Money(netPrice * ratio, linePriceRefCurrency);
					invoiceLine.JI_NetPrice = invoiceLine.CurrencyConverter.ConvertExact(linePriceMoney, invoiceLine.LinePriceRefCurrency ?? invoiceLine.LocalCurrency).Amount;
				}
			}
		}

		protected override bool IsInventoryValid(IWhsInventoryView inventory)
		{
			string docketSubType = Factory.Load<IWhsDocket>(inventory.WI_WD)?.WD_DocketSubType;
			return docketSubType == "CUS";
		}

		protected override bool ShouldFillFinancialData(EU.Business.Declaration.JobComInvoiceLine invoiceLine) => true;
	}
}
