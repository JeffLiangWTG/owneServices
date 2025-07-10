using System;
using System.IO;
using System.Text;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine
{
	public class TxtCreateDeliveryInfoStrategy : ReportDataExportStrategy
	{
		public string Delimiter;
		MemoryStream stream;
		StreamWriter writer;

		public TxtCreateDeliveryInfoStrategy(string delimiter) : base()
		{
			Delimiter = delimiter;
		}

		protected override void Initialize(Report report, DocDeliveryContact deliveryContact, DeliveryInstructions deliveryInstructions)
		{
			stream = new MemoryStream();
			writer = new StreamWriter(stream, new UTF8Encoding(false));
		}

		protected override void ExportColumnHeadings(string[] columns)
		{
		}

		protected override void ExportDataRow(string[] values)
		{
			writer.WriteLine(String.Join(Delimiter, values));
		}

		protected override string AttachmentType
		{
			get { return AttachmentTypeList.Codes.Txt; }
		}

		protected override Stream GenerateAttachment()
		{
			writer.Flush();
			stream.Position = 0;
			return stream;
		}
	}
}
