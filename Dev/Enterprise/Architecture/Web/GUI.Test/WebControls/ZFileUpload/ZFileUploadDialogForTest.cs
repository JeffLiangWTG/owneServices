namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public sealed class ZFileUploadDialogForTest : ZFileUploadDialog
	{
		protected override void ProcessUploadedFile(byte[] contents, string fileName)
		{
			ProcessedFileNameForTest = fileName;
			base.ProcessUploadedFile(contents, fileName);
		}

		protected override string UploadedFileName => UploadedFileNameForTest;

		protected override byte[] GetUploadedFileData() => new byte[] { 0x20 };

		public string ProcessedFileNameForTest { get; set; }

		public string UploadedFileNameForTest { get; set; }
	}
}
