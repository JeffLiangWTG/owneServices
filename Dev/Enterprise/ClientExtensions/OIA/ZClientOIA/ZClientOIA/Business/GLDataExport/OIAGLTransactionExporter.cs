using System;
using System.Data;
using System.IO;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.GLJournals;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.OIA.Business
{
	internal class OIAGLTransactionExporter : GLTransactionExporter
	{
		public OIAGLTransactionExporter(OIAGLTransactionBusinessObject bizObj, NotificationBuffer notify)
			: base(bizObj, notify)
		{
		}

		public OIAGLTransactionExporter(OIAGLTransactionBusinessObject bizObj, NotificationBuffer notify, bool isAutomaticExport) : this(bizObj, notify)
		{
			IsAutomaticExport = isAutomaticExport;
		}

		protected override string GLTransactionsSPName
		{
			get
			{
				return "Client_OIA_GLTransactionsBatch";
			}
		}

		protected override bool IncludeNewFields
		{
			get { return false; }
		}

		protected override int FieldCapacity
		{
			get
			{
				return base.FieldCapacity + OIAGLDataRow.Schema.FieldCapacity;
			}
		}

		protected override string HeadingLine
		{
			get { return !headingLine.IsEmpty ? headingLine : (headingLine = string.Format("{0},{1}", base.HeadingLine, FileFormat.ConvertToLine(HeadingRow))); }
		}
		ZString headingLine;

		protected override FlatFileDataRow ConvertDataReaderToRow(IDataReader reader)
		{
			TransactionsExported++;

			// Client Specific fields...
			OIAGLDataRow cspRow = new OIAGLDataRow();
			cspRow.LocalClientOrgCode = GetValueAsString(reader, cspRow.Heading.LocalClientOrgCode);
			cspRow.ARAccountGroup = GetValueAsString(reader, cspRow.Heading.ARAccountGroup);
			cspRow.CustomisableText1 = GetValueAsString(reader, cspRow.Heading.CustomisableText1);
			cspRow.CustomisableText2 = GetValueAsString(reader, cspRow.Heading.CustomisableText2);

			FlatFileDataRow row = base.ConvertDataReaderToRow(reader);
			foreach (FlatFileFieldProperty property in cspRow.FieldProperties)
			{
				row.SetField(FieldCapacity - OIAGLDataRow.Schema.FieldCapacity + property.Name, cspRow.GetField(property));
			}
			return row;
		}

		ZString GetValueAsString(IDataReader reader, ZString fieldName)
		{
			object valueReturned = reader[fieldName];
			return (valueReturned is DBNull) ? ZString.Empty : (new ZString(reader[fieldName])).Replace("\"", "\"\"");
		}

		protected override string FileName
		{
			get
			{
				return Path.Combine(BizObj.ExportDirectory, string.Format(filenameFormatMask, GlbCompany.CurrentCompany.GC_Code,
					ZDateTime.UtcNow,
					((BizObj.CreateAndExportBatch || BizObj.ExportExistingBatch) ?  new ZString("_" + BizObj.BatchNumber.ToString()) : ZString.Empty)
					) + "." + FileFormat.FileExtensionForExport.ToString().ToLower());
			}
		}

		public override bool IsHighWaterMarkEnabled
		{
			get { return false; }
		}

		OIAGLFlatFileFormat FileFormat
		{
			get { return fileFormat ?? (fileFormat = new OIAGLFlatFileFormat()); }
		}
		OIAGLFlatFileFormat fileFormat;

		OIAGLHeadingDataRow HeadingRow
		{
			get { return headingRow ?? (headingRow = new OIAGLHeadingDataRow()); }
		}
		OIAGLHeadingDataRow headingRow;

		internal int TransactionsExported
		{
			get { return transactionsExported; }
			private set { transactionsExported = value; }
		}
		int transactionsExported;

		const string filenameFormatMask = "{0}_{1:yyyyMMddhhmmss}{2}";
	}
}
