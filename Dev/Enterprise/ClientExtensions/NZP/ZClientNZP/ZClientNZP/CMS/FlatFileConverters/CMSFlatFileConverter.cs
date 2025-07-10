using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.NZP.CMS
{
	public abstract class CMSFlatFileConverter : AccountingFlatFileConverter
	{
		public CMSFlatFileConverter(INotifications notify, BusinessObjectFactory factory) : base(notify, factory)
		{
		}

		protected override FlatFileDataRowCollection MapExport(IValueObject valueObject)
		{
			FlatFileDataRowCollection dataRows = new FlatFileDataRowCollection();
			Xsd.TxnHeader header = (Xsd.TxnHeader)valueObject;
			if (IsOkToExport(header))
			{
				foreach (Xsd.TxnLine line in header.TxnLines)
				{
					CMSFlatFileDataRow row = GetExportedRowThatContainSalesGroup(dataRows, line.ChargeCodeSalesGroup);
					if (row == null)
					{
						dataRows.Add(Export(header, line));
					}
					else
					{
						AddUpTotalsAndPrice(row, line);
					}
				}
				AfterExport(dataRows);
			}

			return dataRows;
		}

		protected abstract bool IsOkToExport(Xsd.TxnHeader header);
		protected abstract string TransactionCode(Xsd.TxnHeader header);

		#region Implementation

		protected CMSFlatFileDataRow Export(Xsd.TxnHeader header, Xsd.TxnLine line)
		{
			CMSFlatFileDataRow row = new CMSFlatFileDataRow
			{
				RecordType = Constants.RecordType,
				AccountCode = header.DebtorOrCreditor.EDICode,
				WarehouseCode = NZPDataRegistry.Instance.GetCMSWarehouseCode(header.Branch),
				TransactionCode = TransactionCode(header),
				ProductCode = line.ChargeCodeSalesGroup,
				TransactionQuantity = Constants.Quantity,
				FileGeneratedDate = ZDateTime.Now,
				TaxTotal = line.LocalTaxAmount.Value,
				LineTotal = line.LocalInvoiceAmtInclTax.Value,
				PricingIndicator = Constants.PricingIndicator,
				FinalPrice = line.LocalInvoiceAmtExclTax.Value,
				Comment = GetComment(header),
				BatchNumber = BatchNumber,
				Reference = header.TxnNumber,
				LineItem = Constants.LineItem,
				BatchDate = ZDateTime.Now,
				TransactionDate = header.PostDate,
				Weight = GetWeightOrVolume(header.TxnLines, true),
				Volume = GetWeightOrVolume(header.TxnLines, false),
				UserReference1 = GetInvoiceNumber(header)
			};

			return row;
		}

		//this will only work when client doesn't customize the job number and accounting trans #	
		ZString GetInvoiceNumber(Xsd.TxnHeader header)
		{
			ZString result = header.JobInvoiceNo;

			if (!result.IsEmpty)
			{
				result = (result.Length <= 10) ? result : header.JobInvoiceNo.Remove(1, 1);
			}
			else
			{
				result = header.TxnNumber;
				result = (result.Length <= 10) ? result : result.Right(10);
			}

			return result;
		}

		protected virtual void AfterExport(FlatFileDataRowCollection dataRows)
		{
		}

		void AddUpTotalsAndPrice(CMSFlatFileDataRow row, Xsd.TxnLine line)
		{
			row.TaxTotal += line.LocalTaxAmount.Value;
			row.LineTotal += line.LocalInvoiceAmtInclTax.Value;
			row.FinalPrice += line.LocalInvoiceAmtExclTax.Value;
		}

		CMSFlatFileDataRow GetExportedRowThatContainSalesGroup(FlatFileDataRowCollection dataRows, ZString salesGroup)
		{
			CMSFlatFileDataRow result = null;
			foreach (CMSFlatFileDataRow row in dataRows)
			{
				if (row.ProductCode == salesGroup)
				{
					result = row;
					break;
				}
			}
			return result;
		}

		ZDecimal GetWeightOrVolume(Xsd.TxnLineCollection lines, bool getWeight)
		{
			ZDecimal result = 0m;
			if (lines.Count > 0)
			{
				result = getWeight ? lines[0].Weight.Value : lines[0].Volume.Value;
			}
			return result;
		}

		string GetComment(Xsd.TxnHeader header)
		{
			string result = "";
			if (header.TxnLines.Count > 0)
			{
				result = header.TxnLines[0].OriginPortCode.Value + "-" + header.TxnLines[0].DestinationPortCode.Value;
			}
			return result;
		}

		string BatchNumber
		{
			get
			{
				if (fBatchNumber == null)
				{
					using (var transactionManager = Db.Connection.BeginTransactionWithManager())
					{
						fBatchNumber = CMSBatchNumberFountain.New().GetGenerateFileID(Db.Connection, true);
						transactionManager.CommitTransaction();
					}
				}
				return fBatchNumber;
			}
		}
		string fBatchNumber;

		protected override ZBool fCheckThatAllTransactionsAreExported
		{
			get { return ZBool.False; }
		}

		#endregion
	}
}
