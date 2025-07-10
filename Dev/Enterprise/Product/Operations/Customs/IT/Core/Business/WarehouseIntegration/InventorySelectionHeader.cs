using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Customs.IT.Business;

sealed class InventorySelectionHeader : EU.Business.InventorySelectionHeader
{
	public InventorySelectionHeader(JobDeclaration declaration) : base(declaration)
	{
	}

	protected override IInventorySelectionLineCollection<Customs.Business.InventorySelectionLine> GetNewInventorySelectionLineCollection() => new InventorySelectionLineCollection(this);

	protected override void SetPartNoDependentData(BaseJobComInvoiceLine invoiceLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute)
	{
		base.SetPartNoDependentData(invoiceLine, whsBondedWarehouseAttribute);
		if (invoiceLine is JobComInvoiceLine itInvoiceLine)
		{
			FillProcedure(itInvoiceLine, whsBondedWarehouseAttribute);
		}
	}

	protected override void FillInvoiceLineWithInventoryDetails(EU.Business.Declaration.JobComInvoiceLine invoiceLine, IWhsDocketLine whsReceiveLine,
		IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, ZDecimal ratio, WhsInventoryWrapper inventoryWrapper = null)
	{
		base.FillInvoiceLineWithInventoryDetails(invoiceLine, whsReceiveLine, whsBondedWarehouseAttribute, ratio, inventoryWrapper);
		if (invoiceLine is JobComInvoiceLine itInvoiceLine)
		{
			new InventorySelectionHeaderPreviousDocumentFiller(itInvoiceLine, inventoryWrapper).AddPreviousDocumentIfRequired();
		}
		ClearInvoiceQuantityAndUQ(invoiceLine);
		ClearBondedDetailsIfRequired(invoiceLine);
	}

	void FillProcedure(JobComInvoiceLine invoiceLine, IWhsBondedWarehouseAttribute bondedWarehouseAttribute)
	{
		var entryInstruction = invoiceLine.EntryInstruction;
		if (entryInstruction is null)
		{
			return;
		}

		var procedureCodeResolver = new OutwardInvoiceLineProcedureCodeResolver(entryInstruction, bondedWarehouseAttribute);
		var procedureCode = procedureCodeResolver.GetProcedureCode();

		if (!procedureCode.IsEmpty)
		{
			invoiceLine.JI_Procedure = procedureCode;
		}
	}

	void ClearInvoiceQuantityAndUQ(EU.Business.Declaration.JobComInvoiceLine invoiceLine)
	{
		invoiceLine.JI_InvoiceQuantity = ZDecimal.Zero;
		invoiceLine.JI_InvoiceUQ = ZString.Empty;
	}

	void ClearBondedDetailsIfRequired(BaseJobComInvoiceLine invoiceLine)
	{
		if (!invoiceLine.IsIntoOrOutOfRegimeProcedure)
		{
			invoiceLine.JI_BondedWhsQuantity = ZDecimal.Zero;
			invoiceLine.JI_BondedWhsUnitQty = ZString.Empty;
		}

		if (!invoiceLine.HasOutOfRegimeProcedure)
		{
			invoiceLine.JI_PreviousEntryNumber = ZString.Empty;
			invoiceLine.JI_PreviousEntryLineNumber = ZShort.Zero;
		}
	}
}
