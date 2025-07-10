
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.STI.Navision
{
	public class AccHeaderFlatFileConverter : AccountingFlatFileConverter
	{
		public AccHeaderFlatFileConverter(INotifications notification, BusinessObjectFactory factory) : base(notification, factory)
		{
		}

		protected override FlatFileDataRowCollection MapExport(IValueObject valueObject)
		{
			FlatFileDataRowCollection dataRows = new FlatFileDataRowCollection();
			AccHeaderFlatFileDataRow row = new AccHeaderFlatFileDataRow();
			Xsd.TxnHeader header = (Xsd.TxnHeader)valueObject;

			row.DocumentType = header.TxnType == Xsd.TxnType.INV ? "Invoice" : "Credit Note";
			row.SellToCustomerNumber = header.DebtorOrCreditor.EDICode;
			row.SellToCustomerName = header.DebtorOrCreditor.OrganisationDetails.Name;
			Xsd.OrgAddress address = header.DebtorOrCreditor.OrganisationDetails.Addresses.GetMainOrFirstAddress();
			row.SellToAddress = address.AddressLine1;
			row.SellToAddress2 = address.AddressLine2;
			row.SellToCity = address.CityOrSuburb;
			row.SellToPostCode = address.PostCode;
			row.SellToCounty = address.StateOrProvince;
			row.SellToCountryCode = address.Location.Value.Left(2);
			row.InvoiceNumber = row.ExternalDocumentNumber = header.JobInvoiceNo;
			row.BillToCustomerNumber = header.DebtorOrCreditor.EDICode;
			row.BillToName = header.DebtorOrCreditor.OrganisationDetails.Name;
			row.BillToAddress = address.AddressLine1;
			row.BillToAddress2 = address.AddressLine2;
			row.BillToCity = address.CityOrSuburb;
			row.BillToPostCode = address.PostCode;
			row.BillToCounty = address.StateOrProvince;
			row.BillToCountryCode = address.Location.Value.Left(2);
			row.YourReference = GetReference(header);
			row.PostingDate = header.PostDate;
			row.PaymentTermsCode = header.InvTerm + header.InvTermDays;
			row.DocumentDate = header.PostDate;
			row.DueDate = header.DueDate;
			row.CustomerPostingGroup = GetCustomerPostingGroup(header.DebtorOrCreditor.EDICode);
			row.Currency = header.OsInvoiceAmtInclTax.CurrencyCode;
			row.AdjustmentAppliesTo = GetAdjustmentAppliesTo(header);
			row.ShortCutDimension1Code = header.Branch;

			dataRows.Add(row);

			return dataRows;
		}

		ZString GetReference(Xsd.TxnHeader header)
		{
			ZString reference = ZString.Empty;
			if (header.TxnLines.Count > 0 && header.TxnType == Xsd.TxnType.INV)
			{
				if (header.TxnLines[0].ConsolOrJobType == Xsd.TxnLineConsolOrJobType.SHP)
				{
					CommonShipment shipment = (CommonShipment)Factory.LoadFromUniqueKey(typeof(CommonShipment), JobShipmentSchema.JS_UniqueConsignRef, header.TxnLines[0].ConsolOrJobNo);
					if (shipment != null)
					{
						reference = (shipment.Declarations == null || shipment.Declarations.Length == 0) ? shipment.JS_OrderReferences : ((BaseJobDeclaration)shipment.Declarations[0]).JE_OwnerRef;
					}
				}
				else if (header.TxnLines[0].ConsolOrJobType == Xsd.TxnLineConsolOrJobType.BRK)
				{
					BaseJobDeclaration declaration = (BaseJobDeclaration)Factory.LoadFromUniqueKey(typeof(BaseJobDeclaration), JobDeclarationSchema.JE_DeclarationReference, header.TxnLines[0].ConsolOrJobNo);
					if (declaration != null)
					{
						reference = declaration.JE_OwnerRef;
					}
				}
			}
			else
			{
				reference = header.Description;
			}
			return reference;
		}

		ZString GetAdjustmentAppliesTo(Xsd.TxnHeader invoice)
		{
			ZString result = ZString.Empty;
			if (invoice.TxnType == Xsd.TxnType.CRD)
			{
				result = (invoice.Description.StartsWith("Reversal")) ? invoice.JobInvoiceNo : invoice.Description;
			}
			return result;
		}

		ZString GetCustomerPostingGroup(ZString orgCode)
		{
			ZString result = ZString.Empty;
			OrgHeader debtor = (OrgHeader)Factory.LoadFromUniqueKey(typeof(OrgHeader), OrgHeaderSchema.OH_Code, orgCode);
			if (debtor != null && debtor.MiscServ != null)
			{
				OrgDebtorGroup debtorGroup = (OrgDebtorGroup)Factory.Load(typeof(OrgDebtorGroup), debtor.MiscServ.OM_OJ_ARDebtorGroup);
				if (debtorGroup != null)
				{
					result = debtorGroup.OJ_Code;
				}
			}
			return result;
		}

		protected override ZBool fCheckThatAllTransactionsAreExported
		{
			get { return false; }
		}
	}
}
