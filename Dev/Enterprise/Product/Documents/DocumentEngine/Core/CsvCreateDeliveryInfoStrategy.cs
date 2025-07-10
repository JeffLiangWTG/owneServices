using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine
{
	public class CsvCreateDeliveryInfoStrategy : ReportDataExportStrategy
	{
		public bool IncludeColumnHeadings { get; set; }

		MemoryStream stream;
		StreamWriter writer;

		protected override void Initialize(Report report, DocDeliveryContact deliveryContact, DeliveryInstructions deliveryInstructions)
		{
			stream = new MemoryStream();
			writer = new StreamWriter(stream, new UTF8Encoding(false));
		}

		protected override void ExportColumnHeadings(string[] columns)
		{
			if (IncludeColumnHeadings && columns != null && columns.Any())
			{
				var csvLine = new OCsvLine(columns);
				writer.WriteLine(csvLine.ToString());
			}
		}

		protected override void ExportDataRow(string[] values)
		{
			var csvLine = new OCsvLine(values);
			writer.WriteLine(csvLine.ToString());
		}

		protected override string AttachmentType
		{
			get { return AttachmentTypeList.Codes.Csv; }
		}

		protected override Stream GenerateAttachment()
		{
			writer.Flush();
			stream.Position = 0;
			return stream;
		}
	}
}
