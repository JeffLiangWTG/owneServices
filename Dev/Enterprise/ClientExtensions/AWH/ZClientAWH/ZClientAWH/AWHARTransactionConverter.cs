using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.AWH
{
	public class AWHARTransactionConverter : AccountingFlatFileConverter
	{
		public AWHARTransactionConverter(INotifications notify, BusinessObjectFactory factory)
			: base(notify, factory)
		{
		}

		protected override FlatFileDataRowCollection MapExport(IValueObject valueObject)
		{
			AWHFlatFileDataRowCollection flatFileRows = new AWHFlatFileDataRowCollection();
			Xsd.TxnHeader txnHeader = valueObject as Xsd.TxnHeader;

			if (txnHeader != null)
			{
				ZString clientCode = GetLegacySystemCode(txnHeader.DebtorOrCreditor);
				ZDateTime invDate = txnHeader.InvoiceDate;
				ZString invNumber = txnHeader.TxnNumber;

				foreach (Xsd.TxnLine line in txnHeader.TxnLines)
				{
					if (line.LocalInvoiceAmtExclTax.Value != 0)
					{
						AWHFlatFileDataRow dataRow = GenerateFlatFileDataRow(line, clientCode, invDate, invNumber);
						if (dataRow != null)
						{
							flatFileRows.Add(dataRow);
						}
					}
				}
			}

			return flatFileRows;
		}

		ZString GetLegacySystemCode(Xsd.Organisation org)
		{
			ZString result = org.OrganisationDetails.RegistrationNumbers.FindOrCreate(Xsd.RegistrationNumberTypes.LSC, GlbCompany.CurrentCompany.GC_RN_NKCountryCode).Number;

			if (result.IsEmpty)
			{
				throw new AWHException(org.EDICode, AWHException.AWHExceptionType.LegacySystemCodeNotSet);
			}

			return result;
		}

		ZString GetAWHDeptCode(ZString eDIDept)
		{
			ZString result = AWHDataRegistry.Instance.DepartmentList.GetDescriptionFromCode(eDIDept);

			if (result.IsEmpty)
			{
				throw new AWHException(eDIDept, AWHException.AWHExceptionType.DeptCodeMappingNotSet);
			}

			return result;
		}

		ZString GetAWHBrhCode(ZString eDIBrh)
		{
			ZString result = AWHDataRegistry.Instance.BranchList.GetDescriptionFromCode(eDIBrh);

			if (result.IsEmpty)
			{
				throw new AWHException(eDIBrh, AWHException.AWHExceptionType.BranchCodeMappingNotSet);
			}

			return result;
		}

		AWHFlatFileDataRow GenerateFlatFileDataRow(Xsd.TxnLine line, ZString clientCode, ZDateTime invoiceDate, ZString invoiceNum)
		{
			ZString aWHBrhCode = GetAWHBrhCode(line.Branch).SubstringSafe(0, 3);
			ZString aWHDeptCode = GetAWHDeptCode(line.Department).SubstringSafe(0, 1);

			AWHFlatFileDataRow result = new AWHFlatFileDataRow();
			result.ClientAccount = clientCode;
			result.Invoice = invoiceNum;
			result.Date = invoiceDate;
			result.GLAccount = aWHBrhCode + aWHDeptCode + line.GLAccount.KeepNumericCharacters().SubstringSafe(0, 6);
			result.Desc = line.Description.Replace(',', ' ').Replace("\r\n", " ");
			result.Amount = line.LocalInvoiceAmtInclTax.Value * 100;
			result.TaxCode = line.LocalTaxAmount.Value != 0 ? AWHConstants.GSTInc : AWHConstants.GSTExc;
			result.Year = invoiceDate;

			return result;
		}

		protected override ZBool fCheckThatAllTransactionsAreExported
		{
			get { return ZBool.False; }
		}
	}
}
