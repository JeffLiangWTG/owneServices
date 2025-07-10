using System.Collections;
using System.IO;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Testing
{
	public class MockReport : Report
	{
		int runCount;

		public MockReport(ExcelTemplate excelTemplate)
			: base(new DocumentPack(), excelTemplate)
		{
		}

		public int RunCount
		{
			get { return runCount; }
		}

		public override void Save(DocDeliveryContact deliveryContact, DocDeliveryContact mostOfficialContact, Stream fileContent)
		{
			fileContent.WriteByte((byte)'0');
			runCount++;
		}

		public void SetColumnHeadingManager(ColumnConfigurationsManager columnConfigurationsManager)
		{
			fColumnHeadingManager = columnConfigurationsManager;
		}

		readonly ArrayList FilesCreated = new ArrayList();

		#region IDisposable Members

		protected override void Dispose(bool isDisposing)
		{
			foreach (string fileName in FilesCreated)
			{
				File.Delete(fileName);
			}

			base.Dispose(isDisposing);
		}

		#endregion
	}
}
