using System.Collections.Specialized;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.Client.AWH
{
	public class AWHARInvoiceDataExporter : FlatFileAccountingTransactionExporter
	{
		public AWHARInvoiceDataExporter(BusinessObjectFactory factory)
			: base(factory)
		{
			FilterProvider.IncludeAccrualsPosting = false;
			FilterProvider.IncludeAccrualsReversing = false;
			FilterProvider.IncludeWIPsPosting = false;
			FilterProvider.IncludeWIPsReversing = false;

			FilterProvider.IncludeAPAdjustmentNotes = false;
			FilterProvider.IncludeAPCreditNotes = false;
			FilterProvider.IncludeAPInvoices = false;

			FilterProvider.IncludeARAdjustmentNotes = true;
			FilterProvider.IncludeARCreditNotes = true;
			FilterProvider.IncludeARInvoices = true;

			FilterProvider.ExcludeNonJobRelatedTransactionsForAP = true;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAR = false;
			FilterProvider.ExcludeJobRelatedTransactionsForAR = false;
			FilterProvider.ExcludeJobRelatedTransactionsForAP = true;
			FilterProvider.CurrentBatchNo = 0;

			OrganisationsWithoutLscCodes = new StringCollection();
			BranchWithoutCodeMap = new StringCollection();
			DeptWithoutCodeMap = new StringCollection();
		}

		protected override FlatFileFormat Format
		{
			get { return new CsvFlatFileFormat(false); }
		}

		protected override AccountingFlatFileConverter Converter
		{
			get
			{
				if (fConverter == null)
				{
					fConverter = new AWHARTransactionConverter(Notify, Factory);
				}
				return fConverter;
			}
		}
		AccountingFlatFileConverter fConverter;

		public ZString GenerateFileName()
		{
			ZString currentBatchNumber = FilterProvider.CurrentBatchNo.ToString();
			return AWHDataRegistry.Instance.ExportFilePrefix + "_" + currentBatchNumber.Right(4).PadLeft(4, '0') + "_" + ZDateTime.Now.ToString("yyyyMMddHHmmss") + FileExtension;
		}

		public ZString FileExtension
		{
			get { return "." + Format.FileExtensionForExport.ToString().ToLower(); }
		}

		protected override void Convert(FlatFileConverter converter, IValueObject valueObject, FlatFileFormat format, TextWriter documentWriter)
		{
			try
			{
				base.Convert(converter, valueObject, format, documentWriter);
			}
			catch (AWHException e)
			{
				if (e.Type == AWHException.AWHExceptionType.BranchCodeMappingNotSet && !BranchWithoutCodeMap.Contains(e.BizObjName))
				{
					BranchWithoutCodeMap.Add(e.BizObjName);
				}
				else if (e.Type == AWHException.AWHExceptionType.DeptCodeMappingNotSet && !DeptWithoutCodeMap.Contains(e.BizObjName))
				{
					DeptWithoutCodeMap.Add(e.BizObjName);
				}
				else if (e.Type == AWHException.AWHExceptionType.LegacySystemCodeNotSet && !OrganisationsWithoutLscCodes.Contains(e.BizObjName))
				{
					OrganisationsWithoutLscCodes.Add(e.BizObjName);
				}
			}
		}

		protected override ZBool ErrorHasOccuredCore
		{
			get { return false; }
		}
		public readonly StringCollection OrganisationsWithoutLscCodes;
		public readonly StringCollection BranchWithoutCodeMap;
		public readonly StringCollection DeptWithoutCodeMap;
	}
}
