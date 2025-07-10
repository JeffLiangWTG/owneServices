using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public sealed class WhsDataTestHelper : Customs.Business.Testing.WhsDataTestHelper<JobDeclaration, AUOrgSupplierPart, Classification, CusClassPartPivot>
	{
		public WhsDataTestHelper()
		{
		}

		public WhsDataTestHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override JobDeclaration GetNewDeclarationCore(ZString messageType, ZString declarationReference, ZString entryNumber, ZDecimal quantity)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = declarationReference;
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_MessageType = messageType;
			declaration.WarehouseDocAddress.E2_OA_Address = WhsWarehouse.WW_OA_WarehouseAddress;

			declaration.Invoices.DeleteAll();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = quantity * 100m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			AddInvoiceLine(invoice, Part, quantity, entryNumber, 1);

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			if (!declaration.IsExWarehouse)
			{
				declaration.ActiveEntryHeaders[0].EntryNumber = entryNumber;
			}
			return declaration;
		}

		public JobComInvoiceLine AddInvoiceLine(JobComInvoiceHeader invoice, AUOrgSupplierPart part, ZDecimal quantity, ZString entryNumber, ZInt wrl)
		{
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.JI_InvoiceQuantity = quantity;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = quantity * 10m;
			invoiceLine.JI_LinePrice = quantity * 100m;
			var declaration = invoice.JobDeclaration;
			if (declaration.IsExWarehouse)
			{
				invoiceLine.UseBondedWarehouseAutomation = true;
				invoiceLine.AddInfo.ZA_WRN = entryNumber;
				invoiceLine.AddInfo.ZA_WRL = wrl;
			}
			else
			{
				invoiceLine.JI_IsPackToBondForLine = true;
			}
			return invoiceLine;
		}

		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}

		protected override ZString PivotChildType => Customs.Business.ClassificationTypeList.Codes.HTI;
	}
}
