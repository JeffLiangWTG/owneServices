using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Diagnostics;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	public partial class DatabaseAndS3InfoForm : ZChildForm
	{
		public DatabaseAndS3InfoForm()
		{
			InitializeComponent();
		}

		public static void ShowDialogAndDispose()
			=> ZFormModaliser.ShowDialogAndDispose(New(ObjectFactory.Get<IDatabaseAndS3InfoProvider>()));

		public static DatabaseAndS3InfoForm New(IDatabaseAndS3InfoProvider provider)
		{
			var dbInfoForm = new DatabaseAndS3InfoForm();
			try
			{
				dbInfoForm.SetConnectionInfoControl(provider.ServerAlias, provider.ServerMachine, provider.MainDbName, provider.MainConnectionSpid);
				dbInfoForm.SetDiskUsageGrid(provider.DbGroupSizes);

				if (provider.ShouldShowS3BucketInfo)
				{
					dbInfoForm.SetS3BucketInfoControl(provider.S3BucketName, provider.S3BucketServiceUrl, provider.S3BucketTimeout, provider.S3BucketSizeError);
				}
				else
				{
					dbInfoForm.S3BucketLabel.Visible = false;
					dbInfoForm.S3BucketInfoLabel.Visible = false;
					dbInfoForm.S3BucketSizeErrorLabel.Visible = false;
				}
			}
			catch
			{
				dbInfoForm.Dispose();
				throw;
			}

			return dbInfoForm;
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			if (!Globals.IsTest)
			{
				// Update height in Test would cause failure of "Control positioned incorrectly (outside the bounds of the parent control)",
				// because the S3 info section is hidden and form is shrinked.
				SetFormHeight();
			}
		}

		/// <summary>
		/// Server Alias: server.alias
		/// Server Machine Name: ActualMachineName
		/// Database Name: DatabaseName
		/// Session ID (SPID): 999
		/// </summary>
		void SetConnectionInfoControl(string serverAlias, string serverMachine, string mainDbName, int mainConnectionSpid)
		{
			ConnectionInfoLabel.Text = String.Format(
				"{0}: {1}\r\n\r\n{2}: {3}\r\n\r\n{4}: {5}\r\n\r\n{6}: {7}\r\n\r\n",
				SqlServerAliasLabel,
				serverAlias,
				SqlServerActualMachineNameLabel,
				serverMachine,
				MainDatabaseNameLabel,
				mainDbName,
				ConnectionSpidLabel,
				mainConnectionSpid.ToString()
			);
		}

		void SetS3BucketInfoControl(string bucketName, string serviceUrl, int timeout, string bucketSizeError)
		{
			S3BucketInfoLabel.Text = String.Format(
				"{0}: {1}\r\n\r\n{2}: {3}\r\n\r\n{4}: {5}\r\n\r\n",
				BucketNameLabel,
				bucketName,
				ServiceUrlLabel,
				serviceUrl,
				TimeoutLabel,
				timeout.ToString()
			);

			if (!string.IsNullOrEmpty(bucketSizeError))
			{
				S3BucketSizeErrorLabel.Text = bucketSizeError;
			}
		}

		void SetDiskUsageGrid(IEnumerable<DbGroupSize> dbGroupSizes)
		{
			DiskUsageListView.Items.Clear();

			var isHostedWithCargowise = EnvProxy.IsHostedWithCargowise;
			long usedMbSum = 0;
			long reservedMbSum = 0;
			long totalMbSum = 0;

			foreach (var dbGroupSize in dbGroupSizes)
			{
				string dbGroupText = dbGroupSize.DbGroup.ToString();
				long usedMb = dbGroupSize.UsedSizeMb;
				long reservedMb = 0;
				long totalMb = 0;

				if (isHostedWithCargowise)
				{
					reservedMb = dbGroupSize.AllocationBufferMb;
					totalMb = dbGroupSize.UsedSizeMb + dbGroupSize.AllocationBufferMb;
				}
				else
				{
					reservedMb = dbGroupSize.DiskSizeMb - dbGroupSize.UsedSizeMb;
					totalMb = dbGroupSize.DiskSizeMb;
				}

				AddItemToGrid(dbGroupText, usedMb, reservedMb, totalMb);
				usedMbSum += usedMb;
				reservedMbSum += reservedMb;
				totalMbSum += totalMb;
			}

			AddItemToGrid((NoResString)"Total", usedMbSum, reservedMbSum, totalMbSum);
		}

		void AddItemToGrid(string dbGroupText, long usedMb, long reservedMb, long totalMb)
		{
			const string thousandSeparatorFormat = "{0:n0}";
			const string thousandSeparatorFormatWith2Decimals = "{0:n2}";
			const int mbPerGB = 1024;

			var item = new ListViewItem(dbGroupText);
			item.SubItems.Add(String.Format(CultureInfo.InvariantCulture, thousandSeparatorFormat, usedMb));
			item.SubItems.Add(String.Format(CultureInfo.InvariantCulture, thousandSeparatorFormat, reservedMb));
			item.SubItems.Add(String.Format(CultureInfo.InvariantCulture, thousandSeparatorFormat, totalMb));
			item.SubItems.Add(String.Format(CultureInfo.InvariantCulture, thousandSeparatorFormatWith2Decimals, Utilities.Round((decimal)totalMb / mbPerGB, 2)));
			DiskUsageListView.Items.Add(item);
		}

		void SetFormHeight()
		{
			var height = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(DiskUsageListViewPanel.Location.Y + DiskUsageListViewPanel.Size.Height + DiskUsageListViewPanel.Margin.Bottom);
			ControlDpiScalingHelper.SetHeight(this, height, true);
		}

		#region Resource Strings

		static string SqlServerAliasLabel
		{
			get { return Res.GetString("974EF6CF-886F-49D8-8CD3-31AB6B27DDAF", "Server Alias"); }
		}

		static string SqlServerActualMachineNameLabel
		{
			get { return Res.GetString("324B3477-7A6D-4C6B-9199-21DD66EB7A27", "Server Machine Name"); }
		}

		static string MainDatabaseNameLabel
		{
			get { return Res.GetString("eef815ec-99e9-4689-bb83-ba38db3d5253", "Database Name"); }
		}

		static string ConnectionSpidLabel
		{
			get { return Res.GetString("C7D67F5F-2629-4EF4-A228-EDB1F3040095", "Session ID (SPID)"); }
		}

		static string BucketNameLabel
		{
			get { return Res.GetString("B6CFB609-EFA4-4DD9-98DB-528440C9780C", "Bucket Name"); }
		}

		static string ServiceUrlLabel
		{
			get { return Res.GetString("0E81770E-C465-4A06-ADB9-72E91D85C9AF", "Service URL"); }
		}

		static string TimeoutLabel
		{
			get { return Res.GetString("AE11E96D-C53F-411E-92E2-E76A704C6CC2", "Timeout"); }
		}

		#endregion
	}
}
