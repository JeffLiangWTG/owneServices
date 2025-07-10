using System.Reflection;
using System.Windows.Forms;
using Enterprise.Customs.CA.Business.DataImport;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(DataImportForm))]
	sealed class DataImportFormBasherTest : ZFormBasherTest
	{
		public void TestSubcribersOfDataImpoterShouldBeDetached()
		{
			var dataImporter = new CACRefFilesDataImporter();
			using (var form = new DataImportForm(null, dataImporter))
			{
			}

			var onImportStartField = dataImporter.GetType().BaseType.GetField("OnImportStart", BindingFlags.Instance | BindingFlags.NonPublic);
			var onImportStart = onImportStartField.GetValue(dataImporter);
			AssertNull("DataImporter_OnImportStart should be detached", onImportStart);

			var onShowNotificationField = dataImporter.GetType().BaseType.GetField("OnShowNotification", BindingFlags.Instance | BindingFlags.NonPublic);
			var onShowNotification = onShowNotificationField.GetValue(dataImporter);
			AssertNull("DataImporter_OnShowMessage should be detached", onShowNotification);

			var onProgressField = dataImporter.GetType().BaseType.GetField("OnProgress", BindingFlags.Instance | BindingFlags.NonPublic);
			var onProgress = onProgressField.GetValue(dataImporter);
			AssertNull("DataImporter_OnProgress should be detached", onProgress);

			var onImportFinishedField = dataImporter.GetType().BaseType.GetField("OnImportFinished", BindingFlags.Instance | BindingFlags.NonPublic);
			var onImportFinished = onImportFinishedField.GetValue(dataImporter);
			AssertNull("DataImporter_OnImportFinished should be detached", onImportFinished);
		}

		protected override Form GetFormToBashCore() => new DataImportForm(null);
	}
}
