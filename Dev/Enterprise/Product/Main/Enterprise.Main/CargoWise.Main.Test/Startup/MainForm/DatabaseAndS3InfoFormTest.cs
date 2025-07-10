using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.Windows.UI;
using Enterprise.DocumentScanning.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core.Diagnostics;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	[TestedType(typeof(DatabaseAndS3InfoForm))]
	sealed class DatabaseAndS3InfoFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new DatabaseAndS3InfoForm();
		}

		[RequiresSTA]
		public void TestDatabaseInfo()
		{
			var sizes = new[] { new DbGroupSize { DbGroup = DbGroupEnum.Main, DiskSizeMb = 1000, UsedSizeMb = 500 }, new DbGroupSize { DbGroup = DbGroupEnum.eDocs, DiskSizeMb = 2000, UsedSizeMb = 100 } };
			AssertEquals(false, EnvProxy.IsHostedWithCargowise);
			AssertDatabaseInfo(@"Database | Data (MB) | Reserved (MB) | Total (MB) | Total (GB)
Main | 500 | 500 | 1,000 | 0.98
eDocs | 100 | 1,900 | 2,000 | 1.95
Total | 600 | 2,400 | 3,000 | 2.93", sizes);

			EnvProxy.SetHostedLocationForTest("SYD");
			AssertEquals(true, EnvProxy.IsHostedWithCargowise);
			AssertDatabaseInfo(@"Database | Data (MB) | Reserved (MB) | Total (MB) | Total (GB)
Main | 500 | 100 | 600 | 0.59
eDocs | 100 | 20 | 120 | 0.12
Total | 600 | 120 | 720 | 0.70", sizes);
		}

		[RequiresSTA]
		public void TestDatabaseAndStorageInfoProvider_S3InfoOfEdiClient()
		{
			var persisterMock = new Mock<IExternalPersister>();
			persisterMock.Setup(x => x.GetBucketSizeInMb()).Returns(100);
			var persisterProviderMock = new Mock<IExternalPersisterProvider>();
			persisterProviderMock.Setup(x => x.GetExternalPersister(Core.Constants.EDocsStorageProviders.Code.S3)).Returns(persisterMock.Object);
			ObjectFactory.Substitute(persisterProviderMock.Object);

			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			using (var form = DatabaseAndS3InfoForm.New(new DatabaseAndS3InfoProvider()))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				ZFormModaliser.ShowDialogWithoutDispose(form);

				var lastShownDialog = ZFormModaliser.LastFormShownDialogForTest as DatabaseAndS3InfoForm;
				AssertNotNull(lastShownDialog);
				AssertContains("S3Bucket | 100 | 0 | 100 | 0.10", GetDiskUsageListViewText(lastShownDialog));

				ZFormModaliser.LastFormShownDialogForTest = null;
				lastShownDialog.Close();
			}
		}

		[RequiresSTA]
		public void TestDatabaseAndStorageInfoProvider_S3InfoOfHostedClient()
		{
			var persisterMock = new Mock<IExternalPersister>();
			persisterMock.Setup(x => x.GetBucketSizeInMb()).Returns(100);
			var persisterProviderMock = new Mock<IExternalPersisterProvider>();
			persisterProviderMock.Setup(x => x.GetExternalPersister(Core.Constants.EDocsStorageProviders.Code.S3)).Returns(persisterMock.Object);
			ObjectFactory.Substitute(persisterProviderMock.Object);
			EnvProxy.SetHostedLocationForTest("SYD");

			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			using (var form = DatabaseAndS3InfoForm.New(new DatabaseAndS3InfoProvider()))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				ZFormModaliser.ShowDialogWithoutDispose(form);

				var lastShownDialog = ZFormModaliser.LastFormShownDialogForTest as DatabaseAndS3InfoForm;
				AssertNotNull(lastShownDialog);
				AssertContains("S3Bucket | 100 | 20 | 120 | 0.12", GetDiskUsageListViewText(lastShownDialog));

				ZFormModaliser.LastFormShownDialogForTest = null;
				lastShownDialog.Close();
			}
		}

		public void TestS3BuckedInfoWithoutS3AndHostedLocation()
		{
			VerifyS3BucketNotConfigured();
		}

		public void TestS3BucketInfoWithoutS3()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			VerifyS3BucketNotConfigured();
		}

		public void TestS3BucketInfoWithoutHostedLocation()
		{
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			{
				VerifyS3BucketNotConfigured();
			}
		}

		[RequiresSTA]
		public void TestS3BucketInfoWithBucketSizeError()
		{
			VerifyS3BucketInfo("error");
		}

		public void TestS3BucketInfoWithNoBucketSizeError()
		{
			VerifyS3BucketInfo(null);
			VerifyS3BucketInfo(string.Empty);
		}

		[RequiresSTA]
		public void TestFormDisposedIfFailedInNew()
		{
			var provider = new Mock<IDatabaseAndS3InfoProvider>();
			provider.Setup(x => x.S3BucketName).Returns("bucketName");
			provider.Setup(x => x.S3BucketServiceUrl).Returns("bucketServiceURL");
			provider.Setup(x => x.S3BucketTimeout).Returns(30);
			provider.Setup(x => x.S3BucketSizeError).Throws(() => new InvalidOperationException());
			provider.Setup(x => x.ShouldShowS3BucketInfo).Returns(true);

			AssertExceptionThrown<InvalidOperationException>("There should be no exceptions other than InvalidOperationException",
				() => DatabaseAndS3InfoForm.New(provider.Object));
		}

		void VerifyS3BucketInfo(string bucketSizeError)
		{
			var provider = new Mock<IDatabaseAndS3InfoProvider>();
			provider.Setup(x => x.S3BucketName).Returns("bucketName");
			provider.Setup(x => x.S3BucketServiceUrl).Returns("bucketServiceURL");
			provider.Setup(x => x.S3BucketTimeout).Returns(30);
			provider.Setup(x => x.S3BucketSizeError).Returns(bucketSizeError);
			provider.Setup(x => x.ShouldShowS3BucketInfo).Returns(true);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			var form = DatabaseAndS3InfoForm.New(provider.Object);
			ZFormModaliser.ShowDialogWithoutDispose(form);
			var lastShownDialog = ZFormModaliser.LastFormShownDialogForTest as DatabaseAndS3InfoForm;
			AssertNotNull(lastShownDialog);

			var s3BucketInfoLabel = lastShownDialog.Controls.Find("S3BucketInfoLabel", true)[0] as ZLabel;
			var s3BucketSizeErrorLabel = lastShownDialog.Controls.Find("S3BucketSizeErrorLabel", true)[0] as ZLabel;

			AssertEquals("Bucket Name: bucketName\r\n\r\nService URL: bucketServiceURL\r\n\r\nTimeout: 30\r\n\r\n", s3BucketInfoLabel.Text);
			AssertEquals(bucketSizeError ?? string.Empty, s3BucketSizeErrorLabel.Text);

			ZFormModaliser.LastFormShownDialogForTest = null;
			lastShownDialog.Close();
		}

		void VerifyS3BucketNotConfigured()
		{
			var provider = new Mock<IDatabaseAndS3InfoProvider>();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			var form = DatabaseAndS3InfoForm.New(provider.Object);
			ZFormModaliser.ShowDialogWithoutDispose(form);
			var lastShownDialog = ZFormModaliser.LastFormShownDialogForTest as DatabaseAndS3InfoForm;
			AssertNotNull(lastShownDialog);

			var s3BucketInfoLabel = lastShownDialog.Controls.Find("S3BucketInfoLabel", true)[0] as ZLabel;
			var s3BucketSizeErrorLabel = lastShownDialog.Controls.Find("S3BucketSizeErrorLabel", true)[0] as ZLabel;
			var s3BucketLabel = lastShownDialog.Controls.Find("S3BucketLabel", true)[0] as ZLabel;

			Assert("S3 section not visible", !s3BucketInfoLabel.Visible);
			Assert("S3 section not visible", !s3BucketSizeErrorLabel.Visible);
			Assert("S3 section not visible", !s3BucketLabel.Visible);

			ZFormModaliser.LastFormShownDialogForTest = null;
			lastShownDialog.Close();
		}

		void AssertDatabaseInfo(string expectedUsagesAsText, IEnumerable<DbGroupSize> dbGroupSizes)
		{
			var provider = new Mock<IDatabaseAndS3InfoProvider>();
			provider.Setup(x => x.ServerAlias).Returns("serverAlias");
			provider.Setup(x => x.ServerMachine).Returns("serverMachine");
			provider.Setup(x => x.MainDbName).Returns("mainDbName");
			provider.Setup(x => x.MainConnectionSpid).Returns(1900);
			provider.Setup(x => x.DbGroupSizes).Returns(dbGroupSizes);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			var form = DatabaseAndS3InfoForm.New(provider.Object);
			ZFormModaliser.ShowDialogWithoutDispose(form);

			var lastShownDialog = ZFormModaliser.LastFormShownDialogForTest as DatabaseAndS3InfoForm;
			AssertNotNull(lastShownDialog);
			AssertEquals(expectedUsagesAsText, GetDiskUsageListViewText(lastShownDialog));

			var databaseInfoText = lastShownDialog.Controls.Find("ConnectionInfoLabel", true)[0] as ZLabel;
			AssertEquals("Server Alias: serverAlias\r\n\r\nServer Machine Name: serverMachine\r\n\r\nDatabase Name: mainDbName\r\n\r\nSession ID (SPID): 1900\r\n\r\n", databaseInfoText.Text);

			ZFormModaliser.LastFormShownDialogForTest = null;
			lastShownDialog.Close();
		}

		string GetDiskUsageListViewText(DatabaseAndS3InfoForm lastShownDialog)
		{
			var diskUsageListView = lastShownDialog.Controls.Find("DiskUsageListView", true)[0] as KListView;
#if WINZOR
			var headers = string.Join(" | ", diskUsageListView.Columns.OfType<ColumnHeader>().Select(x => x.Text));
			var body = string.Join("\r\n", diskUsageListView.Items.OfType<ListViewItem>().Select(x => string.Concat($"{x.Text} | ", string.Join(" | ", x.SubItems.OfType<ListViewItem.ListViewSubItem>().Select(y => y.Text)))));
			var rowsAsText = string.Concat(headers, "\r\n", body);
#else
			var rowsAsText = string.Join("\r\n", new[] { string.Join(" | ", diskUsageListView.Columns.OfType<ColumnHeader>().Select(x => x.Text)) }
				.Concat(diskUsageListView.Items.OfType<ListViewItem>().Select(x => string.Join(" | ", x.SubItems.OfType<ListViewItem.ListViewSubItem>().Select(y => y.Text)))));
#endif
			return rowsAsText;
		}
	}
}
