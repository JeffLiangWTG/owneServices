using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.Test
{
	public class GBWhsDataTestHelper : WhsDataTestHelper<JobDeclaration, OrgSupplierPart, EU.Business.MasterFiles.CusClassification, EU.Business.MasterFiles.CusClassPartPivot>
	{
		public GBWhsDataTestHelper()
		{
		}

		public GBWhsDataTestHelper(BusinessObjectFactory factory, ZString dataGroupCode)
			: base(factory)
		{
			this.dataGroupCode = dataGroupCode;
		}

		readonly ZString dataGroupCode;

		protected override string CountryCode => dataGroupCode;

		public CusEntryHeader GetNewEntryHeader(ZString messageType, ZString declarationReference, ZString procedureCode, ZString entryNumber, ZString previousProcedureCode, ZDecimal quantity, ZString applicationCode, bool isVirtualWarehouse = true)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = applicationCode;
			declaration.JE_MessageType = messageType;
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_DeclarationReference = declarationReference;
			declaration.JE_CustomsOffice = "GB0001";
			declaration.JE_GS_NKCusAgent = CurrentStaff.GS_Code;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = quantity * 100m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedKingdom;
			AddInvoiceLine(invoice, Part, quantity, procedureCode, previousProcedureCode, entryNumber, 1);
			declaration.DoMerge();
			var entry = declaration.ActiveEntryHeaders[0] as CusEntryHeader;

			var warehouseOrg = Factory.New<OrgHeader>();
			warehouseOrg.OH_Code = "WH1";
			var warehouseAddress = warehouseOrg.Addresses.AddNewMainAddress();
			warehouseAddress.Address1 = "Warehouse St";

			var warehouse = GetNewWhsWarehouse(warehouseAddress.PK, true, "N10");

			if (applicationCode == Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services)
			{
				var entryInstruction = entry.EntryInstruction;
				entryInstruction.CEI_OA_Warehouse = warehouseAddress.PK;
				entryInstruction.CEI_OA_Warehouse2 = warehouseAddress.PK;
				entryInstruction.CEI_Style = "H2";
				entryInstruction.CEI_Description = "HELLO";
			}

			entry.Declaration.WarehouseDocAddress.E2_OA_Address = warehouseAddress.PK;

			if (entry.IsIntoWarehouseWarehousing)
			{
				entry.EntryNumber = entryNumber;
			}
			return entry;
		}

		public JobComInvoiceLine AddInvoiceLine(JobComInvoiceHeader invoice, OrgSupplierPart part, ZDecimal quantity, ZString procedureCode, ZString previousProcedureCode, ZString previousEntryNumber, ZShort previousEntryLineNumber)
		{
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.JI_Procedure = procedureCode + previousProcedureCode;
			invoiceLine.JI_InvoiceQuantity = quantity;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_BondedWhsQuantity = quantity;
			invoiceLine.JI_BondedWhsUnitQty = "NO";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = quantity * 10m;
			invoiceLine.JI_LinePrice = quantity * 100m;
			if (invoiceLine.CusProcedure?.IsOutOfWarehouse() ?? false)
			{
				invoiceLine.JI_PreviousEntryNumber = previousEntryNumber;
				invoiceLine.JI_PreviousEntryLineNumber = previousEntryLineNumber;
			}
			return invoiceLine;
		}
	}
}
