using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Core.Environment.Semaphores.Testing;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI
{
	[TestedType(typeof(DocumentDbMergerForm))]
	sealed class DocumentDbMergerFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new DocumentDbMergerForm(new DocumentDbMerger(new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory())));
		}

		public void TestRefreshStatusTable_BarChartLabelText()
		{
			// Arrange
			using (SystemDataRegistry.Instance.DocManagerDataFileSizeThresholdGb.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 8))
			using (var form = new DocumentDbMergerFormForTest(new DocumentDbMerger(new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()))))
			{
				var fakeDbMergeInfo = new Dictionary<int, DbMergeInfo>();
				var dbMergeInfoCollection = new DbMergeInfoCollectionForTesting(fakeDbMergeInfo, "TestMainDb");

				// Perform
				form.RefreshStatusTableForTest(dbMergeInfoCollection);
				var control = form.Controls.Find("barChartLabel", true).FirstOrDefault();

				// Assert
				AssertNotNull(control);
				AssertEquals("TestMainDb DocManager database sizes - Max Size 8192 MB (approx.)", control.Text);
			}
		}

		public void TestRefreshStatusTable_EveryColumnHasAStyle()
		{
			// Arrange
			using (var form = new DocumentDbMergerFormForTest(new DocumentDbMerger(new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()))))
			{
				var fakeDbMergeInfo = new Dictionary<int, DbMergeInfo>
					{
						{ 1, new DbMergeInfo(1, "1", 1, false) },
						{ 2, new DbMergeInfo(2, "2", 2, false) },
						{ 3, new DbMergeInfo(3, "3", 3, true) }
					};
				var dbMergeInfoCollection = new DbMergeInfoCollectionForTesting(fakeDbMergeInfo, "TestMainDb");

				// Perform
				form.RefreshStatusTableForTest(dbMergeInfoCollection);
				var statusTableLayoutPanel = form.Controls.Find("statusTableLayoutPanel", true).FirstOrDefault() as KTableLayoutPanel;

				// Assert
				AssertNotNull(statusTableLayoutPanel);
				AssertEquals(4, statusTableLayoutPanel.ColumnCount);
				AssertEquals(4, statusTableLayoutPanel.ColumnStyles.Count);

				// Arrange
				fakeDbMergeInfo = new Dictionary<int, DbMergeInfo>
				{
					{ 1, new DbMergeInfo(1, "1", 1, false) }
				};
				dbMergeInfoCollection = new DbMergeInfoCollectionForTesting(fakeDbMergeInfo, "TestMainDb");

				// Perform
				form.RefreshStatusTableForTest(dbMergeInfoCollection);

				// Assert
				AssertEquals(2, statusTableLayoutPanel.ColumnCount);
				AssertEquals(2, statusTableLayoutPanel.ColumnStyles.Count);
				AssertEquals(3, statusTableLayoutPanel.Controls.Count);
			}
		}

		public void TestRefreshStatusTable_StatusTableLayoutPanelShouldHaveMultipleSDDatabaseRows()
		{
			var fakeDbMergeInfo = new Dictionary<int, DbMergeInfo>();
			for (var i = 1; i <= 120; i++)
			{
				fakeDbMergeInfo.Add(i, new DbMergeInfo(i, i.ToString(), i, false));
			}

			using (var form = new DocumentDbMergerFormForTest(new DocumentDbMerger(new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()))))
			{
				var dbMergeInfoCollection = new DbMergeInfoCollectionForTesting(fakeDbMergeInfo, "TestMainDb");

				// Perform
				form.RefreshStatusTableForTest(dbMergeInfoCollection);
				var statusTableLayoutPanel = form.Controls.Find("statusTableLayoutPanel", true).FirstOrDefault() as KTableLayoutPanel;

				// Assert
				AssertNotNull(statusTableLayoutPanel);
				AssertEquals(101, statusTableLayoutPanel.ColumnCount);
				AssertEquals(101, statusTableLayoutPanel.ColumnStyles.Count);
				AssertEquals(4, statusTableLayoutPanel.RowCount);
				AssertEquals(4, statusTableLayoutPanel.RowStyles.Count);

				for (var key = 120; key > 100; key--)
				{
					_ = dbMergeInfoCollection.internalDictionary.Remove(key);
				}

				// Perform
				form.RefreshStatusTableForTest(dbMergeInfoCollection);
				AssertEquals(101, statusTableLayoutPanel.ColumnCount);
				AssertEquals(101, statusTableLayoutPanel.ColumnStyles.Count);
				AssertEquals(2, statusTableLayoutPanel.RowCount);
				AssertEquals(2, statusTableLayoutPanel.RowStyles.Count);
			}
		}

		public void TestShowMessage()
		{
			using (var form = new DocumentDbMergerFormForTest(new DocumentDbMerger(new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()))))
			{
				form.ShowMessageForTest("Test message line 1");
				form.ShowMessageForTest("Test message line 2");

				AssertEquals(@"Test message line 1
Test message line 2
", form.ConsoleTextBox.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestDocumentDbMergerRunFromNewThread()
		{
			var mockHeartbeatInfo = new Mock<IHeartbeatInfo>();
			mockHeartbeatInfo.Setup(m => m.UserPk).Returns(GlbStaff.CurrentUser.PK.ToGuid());

			var mockSemaphore = new Mock<ISemaphoreHandle>();
			mockSemaphore.Setup(m => m.Success).Returns(false);

			var mockSemaphoreInfo = new Mock<ISemaphoreInfo>();
			mockSemaphoreInfo.Setup(m => m.OwnerSession).Returns(mockHeartbeatInfo.Object);

			var mockSemaphoreProvider = new Mock<ISemaphoreProvider>();
			mockSemaphoreProvider.Setup(m => m.CreateSemaphoreHandle(It.IsAny<ISemaphoreType>())).Returns(mockSemaphore.Object);
			mockSemaphoreProvider.Setup(m => m.GetActiveSemaphoreHandles(It.IsAny<ISemaphoreType>())).Returns(new[] { mockSemaphoreInfo.Object });

			var previousTestProvider = TestSemaphoreProviderAttribute.TestProvider;
			TestSemaphoreProviderAttribute.TestProvider = mockSemaphoreProvider.Object;

			try
			{
				using (var form = new DocumentDbMergerFormForTest(new DocumentDbMerger(new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()))))
				{
					form.Show();
					form.Start();
				}
			}
			finally
			{
				TestSemaphoreProviderAttribute.TestProvider = previousTestProvider;
			}

			mockHeartbeatInfo.VerifyAll();
			mockSemaphore.VerifyAll();
			mockSemaphoreInfo.VerifyAll();
			mockSemaphoreProvider.VerifyAll();
		}

		public void TestConsoleTextBoxIsReadOnly()
		{
			// Arrange
			using (var form = new DocumentDbMergerFormForTest(new DocumentDbMerger(new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()))))
			{
				// Act
				// Assert
				AssertEquals("consoleTextBox.ReadOnly should be True", true, form.ConsoleTextBox.ReadOnly);
			}
		}

		class DocumentDbMergerFormForTest : DocumentDbMergerForm
		{
			public DocumentDbMergerFormForTest(DocumentDbMerger businessEntity)
			: base(businessEntity)
			{
			}

			public void ShowMessageForTest(string message) => ShowMessage(message);

			public void Start()
			{
				StartButton.PerformClick();
				Thread.Sleep(100);
			}

			public void RefreshStatusTableForTest(DbMergeInfoCollection dbInfoCollection) => RefreshStatusTable(dbInfoCollection);

			ZButton StartButton => startButton;

			public Enterprise.ZArchitecture.ZTextBox ConsoleTextBox => consoleTextBox;
		}
	}
}
