using System;
using System.Windows.Forms;
using CargoWise.Data;
using Enterprise.DocumentScanning.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	[TestedType(typeof(DocumentDbManagerForm))]
	internal sealed class DocumentDbManagerFormBasherTest : ZFormBasherTest
	{
		public void TestSaveWhenS3IsEnabledAndMakeDBReadOnly()
		{
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			{
				var manager = new DocumentDbManager(Factory);
				var storageDatabase = manager.StorageDatabaseCollection.AddNew();
				storageDatabase.DatabaseName = Db.DatabaseName.Trim() + "_SD001";
				using (var form = new DocumentDbManagerForm(manager))
				{
					form.Show();
					var firstStorageDatabase = manager.StorageDatabaseCollection[0];
					firstStorageDatabase.NewReadOnly = true;
					form.okButton.PerformClick();

					AssertEquals("There are errors that need to be corrected before save.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new DocumentDbManagerForm(new DocumentDbManager(Factory));
		}

		#endregion
	}
}
