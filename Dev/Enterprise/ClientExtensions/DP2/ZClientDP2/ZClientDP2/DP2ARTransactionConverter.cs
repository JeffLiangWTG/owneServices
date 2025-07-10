using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.DP2
{
	public class DP2ARTransactionConverter : AccountingFlatFileConverter
	{
		public DP2ARTransactionConverter(INotifications notify, BusinessObjectFactory factory)
			: base(notify, factory)
		{
		}

		protected override FlatFileDataRowCollection MapExport(IValueObject valueObject)
		{
			DP2FlatFileDataRowCollection flatFileRows = new DP2FlatFileDataRowCollection();
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
						DP2FlatFileDataRow dataRow = GenerateFlatFileDataRow(line, clientCode, invDate, invNumber);
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
				throw new DP2Exception(org.EDICode, DP2Exception.DP2ExceptionType.LegacySystemCodeNotSet);
			}

			return result;
		}

		ZString GetDP2DeptCode(ZString eDIDept)
		{
			ZString result = DP2DataRegistry.Instance.DepartmentList.GetDescriptionFromCode(eDIDept);

			if (result.IsEmpty)
			{
				throw new DP2Exception(eDIDept, DP2Exception.DP2ExceptionType.DeptCodeMappingNotSet);
			}

			return result;
		}

		ZString GetDP2BrhCode(ZString eDIBrh)
		{
			ZString result = DP2DataRegistry.Instance.BranchList.GetDescriptionFromCode(eDIBrh);

			if (result.IsEmpty)
			{
				throw new DP2Exception(eDIBrh, DP2Exception.DP2ExceptionType.BranchCodeMappingNotSet);
			}

			return result;
		}

		DP2FlatFileDataRow GenerateFlatFileDataRow(Xsd.TxnLine line, ZString clientCode, ZDateTime invoiceDate, ZString invoiceNum)
		{
			ZString dP2BrhCode = GetDP2BrhCode(line.Branch).SubstringSafe(0, 3);
			ZString dP2DeptCode = GetDP2DeptCode(line.Department).SubstringSafe(0, 1);

			DP2FlatFileDataRow result = new DP2FlatFileDataRow();
			result.ClientAccount = clientCode;
			result.Invoice = invoiceNum;
			result.Date = invoiceDate;
			result.GLAccount = dP2BrhCode + dP2DeptCode + line.GLAccount.KeepNumericCharacters().SubstringSafe(0, 6);
			result.Desc = line.Description.Replace(',', ' ').Replace("\r\n", " ");
			result.Amount = line.LocalInvoiceAmtInclTax.Value * 100;
			result.TaxCode = line.LocalTaxAmount.Value != 0 ? DP2Constants.GSTInc : DP2Constants.GSTExc;
			result.Year = invoiceDate;

			return result;
		}

		protected override ZBool fCheckThatAllTransactionsAreExported
		{
			get { return ZBool.False; }
		}
	}
}
