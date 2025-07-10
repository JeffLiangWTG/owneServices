using System.IO;

namespace Enterprise.DocumentEngine.DeliveryMethods.Testing
{
	sealed class ExcelPreviewForTesting : ExcelPreview
	{
		internal ExcelPreviewForTesting()
			: this(false)
		{
		}

		internal ExcelPreviewForTesting(bool useBaseMethod)
			: base(null)
		{
			UseBaseMethod = useBaseMethod;
		}

		protected override void DeliverToExcel(Stream xlsFileContent)
		{
			DeliverToExcelCalled = true;
			if (UseBaseMethod)
			{
				base.DeliverToExcel(xlsFileContent);
			}
		}

		protected override void PreviewInExcel(Stream xlsFileContent)
		{
			PreviewInExcelCalled = true;
		}

		public bool DeliverToExcelCalled;
		public bool PreviewInExcelCalled;
		public bool UseBaseMethod;
	}
}
