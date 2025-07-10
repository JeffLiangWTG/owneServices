using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration.BondedWarehouse;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class WarehouseInvoiceLink : Customs.Business.WarehouseInvoiceLink
	{
		public WarehouseInvoiceLink(JobDeclaration dec)
			: base(dec)
		{
		}

		protected override void SetupInvoiceLineFromTransactionLine(IWhsBondedWarehouseTransactionLine transactionLine, Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			base.SetupInvoiceLineFromTransactionLine(transactionLine, invoiceLine);
			JobComInvoiceLine line = (JobComInvoiceLine)invoiceLine;
			ZString addInfoString = transactionLine.AddInfo;
			line.AddInfo.LoadPropertiesFromString(addInfoString);
			line.AddInfo.ZA_WRL = transactionLine.EntryLineNumber;
			line.AddInfo.ZA_WRN = transactionLine.EntryKey;
			string tILVCurrencyCode = "";
			if (transactionLine.TILV.Currency != null)
			{
				tILVCurrencyCode = transactionLine.TILV.Currency.Code;
			}

			line.AddInfo.ZA_TILV = transactionLine.TILV.Amount.ToString() + tILVCurrencyCode;
			line.AddInfo.ZA_IsPackToBondForLine_Hidden = "N";
			line.AddInfo.ZA_ADJ = "";
			line.AddInfo.UseBondedWarehouseAutomation = true;
			if (transactionLine.Warehouse != null)
			{
				line.AddInfo.ZA_OA_WarehouseAddress_Hidden = transactionLine.Warehouse.PK;
			}

			if (line.JI_CustomsUnitQty.IsEmpty && line.JI_CustomsQuantity == 0m)
			{
				line.JI_InvoiceQuantity = transactionLine.Quantity;
				line.JI_InvoiceUQ = transactionLine.QuantityUnit;
			}

			if (HasWRQandWRU(transactionLine.AddInfo) && line.JI_CustomsQuantity > 0 && !line.JI_CustomsUnitQty.IsEmpty)
			{
				line.AddInfo.ZA_WRQ = transactionLine.CustomsQuantity;
				line.AddInfo.ZA_WRU = transactionLine.CustomsQuantityUnit;
			}

			invoiceLine.JI_BondedWarehouseLineKey = transactionLine.UniqueKey;
		}

		bool HasWRQandWRU(ZString addInfo)
		{
			return addInfo.Contains("WRU=") && addInfo.Contains("WRQ=");
		}

		protected override ZString EntryKeyTitleCore
		{
			get { return "WRN-WRL"; }
		}

		protected override ZPropertyInfo GetEntryKeyPropertyInfo(Customs.Business.BaseJobComInvoiceLine line)
		{
			return ((JobComInvoiceLine)line).AddInfo.ZA_WRNInfo;
		}

		protected override ZPropertyInfo GetWarehousePropertyInfo(Customs.Business.BaseJobComInvoiceLine line)
		{
			return ((JobComInvoiceLine)line).AddInfo.ZA_OA_WarehouseAddress_HiddenInfo;
		}

#if DEBUG
		protected override void AddErrorsAndWarnings(IWhsBondedWarehouseTransactionLine transactionLine, Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			JobComInvoiceLine line = (JobComInvoiceLine)invoiceLine;
			using (line.AddInfo.SuspendValidationTesting())
			{
				base.AddErrorsAndWarnings(transactionLine, invoiceLine);
			}
		}
#endif
	}
}
