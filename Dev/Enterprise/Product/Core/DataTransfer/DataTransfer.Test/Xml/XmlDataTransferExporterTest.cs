using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.DataAdapters.Testing;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class XmlDataTransferExporterTest : TestCaseWithFactory
	{
		#region Export

		[ExpectNoExceptions]
		public void TestDoesntThrowForChildren()
		{
			List<DummyEnterpriseBusinessObject> list = new List<DummyEnterpriseBusinessObject>
			{
				Factory.New<DummyEnterpriseBusinessObject>(),
				Factory.New<DummyEnterpriseBusinessObject>()
			};

			try
			{
				exporter.Export(list);
			}
			finally
			{
				DeleteIfExists(exporter.LastExportFileName);
			}
		}

		#region PromptUserAndExport

		public void TestPromptUserAndExport_PermssionCheck()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			var allowedDirector = new XmlDataTransferExporterDummy(StmALogValueObjectDataAdapter.New(null, "test"), true);
			allowedDirector.PromptUserAndExport((IList)null);
			AssertEquals("LICENCED", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var empty = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, empty);
			var disallowedDirector = new XmlDataTransferExporterDummy(StmALogValueObjectDataAdapter.New(null, "test"), true);
			disallowedDirector.PromptUserAndExport((IList)null);
			AssertEquals("Exporting should not be restricted", "LICENCED", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestPromptUserAndExport_DontSaveWhenHasChanges()
		{
			var mockGui = new Mock<IXmlDataTransferExporterGUI>();
			mockGui.Setup(g => g.ShowSaveFileDialog(It.IsAny<string>(), It.IsAny<string>())).Returns(ZDialogResult.OK);
			var file = Env.GetTempFileName();
			ObjectFactory.Substitute<IXmlDataTransferExporterGUI>(mockGui.Object);

			try
			{ File.Delete(file); }
			catch { }

			var bO = Factory.New<DummyBusinessObject>();
			bO.HasChanges = true;
			AssertEquals(true, bO.HasChanges);
			exporter.PromptUserAndExport(new[] { bO });
			AssertEquals("The file should not be created as the object has changes", false, File.Exists(file));
		}

		public void TestDefaultFileNameShowedOnDialog()
		{
			var exportor = new XmlDataTransferExporter(DataAdapter, false);
			var fileName = Path.Combine(Temp.TempPath, "test.xml");
			exportor.DefaultFileName = "test";
			var mockGui = new Mock<IXmlDataTransferExporterGUI>();
			mockGui.Setup(g => g.ShowSaveFileDialog("test", It.IsAny<string>())).Returns(ZDialogResult.OK);
			mockGui.Setup(g => g.OpenFile()).Returns(File.OpenWrite(fileName));
			ObjectFactory.Substitute<IXmlDataTransferExporterGUI>(mockGui.Object);
			var bO = Factory.New<DummyBusinessObject>();
			bO.HasChanges = false;
			try
			{
				exportor.PromptUserAndExport(new[] { bO });
				AssertEquals("The file should have been created", true, File.Exists(fileName));
			}
			finally
			{
				DeleteIfExists(fileName);
			}
			Assert("Must be saved to database to commit log", bO.IsInDatabase);
		}

		public void TestInitialDirectory()
		{
			AssertEquals("Should be empty", ZString.Empty, exporter.InitialDirectory);
			exporter.InitialDirectory = Env.TempPath;
			AssertEquals("Should be as assigned", Env.TempPath, exporter.InitialDirectory);
		}

		public void TestPromptUserAndExport_SaveWhenOKPressedOnDialog()
		{
			var mockGui = new Mock<IXmlDataTransferExporterGUI>();
			mockGui.Setup(g => g.ShowSaveFileDialog(It.IsAny<string>(), It.IsAny<string>())).Returns(ZDialogResult.OK);
			var file = Env.GetTempFileName();
			mockGui.Setup(g => g.OpenFile()).Returns(File.OpenWrite(file));
			ObjectFactory.Substitute<IXmlDataTransferExporterGUI>(mockGui.Object);

			var bO = Factory.New<DummyBusinessObject>();
			bO.HasChanges = false;
			AssertEquals("BizO should have no changes so export can happen", false, bO.HasChanges);
			exporter.PromptUserAndExport(new DummyBusinessObject[] { bO });
			AssertEquals("The file should be created", true, File.Exists(file));
			File.Delete(file);
		}

		public void TestPromptUserAndExport_DontSaveWhenCancelDialog()
		{
			var outputFile = Env.GetTempFileName();
			var mockGui = new Mock<IXmlDataTransferExporterGUI>();
			mockGui.Setup(g => g.ShowSaveFileDialog(It.IsAny<string>(), It.IsAny<string>())).Returns(ZDialogResult.Cancel);
			ObjectFactory.Substitute<IXmlDataTransferExporterGUI>(mockGui.Object);

			using (var stream = File.OpenWrite(outputFile))
			{
				stream.Write(new byte[] { 1, 1, 1 }, 0, 3);
			}
			AssertEquals("An output file should already exist for the test", 3, new FileInfo(outputFile).Length);

			var bO = Factory.New<DummyBusinessObject>();
			bO.HasChanges = false;
			exporter.PromptUserAndExport(new DummyBusinessObject[] { bO });

			AssertEquals("The already existing output file should be unchanged as the user cancelled", 3, new FileInfo(outputFile).Length);
			File.Delete(outputFile);
		}

		public void TestPromptUserAndExport_DontSaveIfThereIsAnExportError()
		{
			var mockGui = new Mock<IXmlDataTransferExporterGUI>();
			mockGui.Setup(g => g.ShowSaveFileDialog(It.IsAny<string>(), It.IsAny<string>())).Returns(ZDialogResult.Cancel);
			ObjectFactory.Substitute<IXmlDataTransferExporterGUI>(mockGui.Object);
			var file = Env.GetTempFileName();

			try
			{ File.Delete(file); }
			catch { }

			var bO = Factory.New<DummyBusinessObject>();
			bO.HasChanges = false;
			AssertEquals("BizO should have no changes so export can happen", false, bO.HasChanges);
			exporter.RaiseErrorOnExport = true;
			exporter.PromptUserAndExport(new[] { bO });
			AssertEquals("The file should not be created as the user has cancelled", false, File.Exists(file));
		}

		#endregion

		#region Export Without Prompting User

		public void TestExport_LicenceCheck()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			var allowedDirector = new XmlDataTransferExporterDummy(StmALogValueObjectDataAdapter.New(null, "test"), true);
			allowedDirector.Export(null);
			AssertEquals("LICENCED", UnitTestUserNotification.Instance.LastMessage.Text);

			var empty = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, empty);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var disallowedDirector = new XmlDataTransferExporterDummy(StmALogValueObjectDataAdapter.New(null, "test"), true);
			disallowedDirector.Export(null);
			AssertEquals("Exporting should not be restricted", "LICENCED", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestExport_DontSaveWhenHasChanges()
		{
			var bO = Factory.New<DummyBusinessObject>();
			bO.HasChanges = true;
			AssertEquals("BO should have changes", true, bO.HasChanges);

			var fileName = exporter.Export(new DummyBusinessObject[] { bO });
			AssertEquals("The file should not be created as the object has changes", false, File.Exists(fileName));
		}

		public void TestExport_DontSaveIfThereIsAnExportError()
		{
			var bO = Factory.New<DummyBusinessObject>();
			bO.HasChanges = false;
			AssertEquals("BizO should have no changes so export can happen", false, bO.HasChanges);

			exporter.RaiseErrorOnExport = true;
			var fileName = exporter.Export(new[] { bO });
			AssertEquals("The file should not be created as the user has cancelled", false, File.Exists(fileName));
		}

		public void TestExport()
		{
			var bO = Factory.New<DummyBusinessObject>();
			bO.HasChanges = false;
			AssertEquals("BizO should have no changes so export can happen", false, bO.HasChanges);

			var fileName = exporter.Export(new[] { bO });
			AssertEquals("The file should be created", true, File.Exists(fileName));
			File.Delete(fileName);
		}

		#endregion

		#region Test Export into existion file clears previous content

		public void TestExportIntoExistingFileClearsPreviousContent()
		{
			using (var tempFile = TempFile.New())
			{
				new XmlDataTransferExporterForTestExportIntoExistionFile("ABCDE").ExportFile(tempFile.Filename);
				var content = File.ReadAllText(tempFile.Filename);
				AssertEquals("ABCDE", content);
			}
		}

		class XmlDataTransferExporterForTestExportIntoExistionFile : XmlDataTransferExporter
		{
			public XmlDataTransferExporterForTestExportIntoExistionFile(string exportContent)
				: base(null, false)
			{
				this.exportContent = exportContent;
			}

			readonly string exportContent;

			public void ExportFile(string fileName)
			{
				base.ExportFile(Array.Empty<BusinessObject>(), new UnattendedXmlDataTransferExporterGUI(fileName));
			}

			protected override void DoExport(Stream file, IList selectedBusinessObjects, INotifications notifications)
			{
				using (var writer = new StreamWriter(file, Encoding.UTF8))
				{
					writer.Write(exportContent);
					writer.Flush();
				}
			}
		}

		#endregion

		#endregion

		#region Test Classes

		class XmlDataTransferExporterDummy : XmlDataTransferExporter
		{
			public XmlDataTransferExporterDummy(IValueObjectDataAdapter adapter, bool checkLicence)
				: base(adapter, checkLicence)
			{
			}
			protected override void PromptUserAndExportCore(IList selectedElements)
			{
				Globals.Message.Show("LICENCED");
			}

			protected override string ExportCore(IList selectedElements)
			{
				Globals.Message.Show("LICENCED");
				return "";
			}
		}

		class TestXmlDataTransferExporter : XmlDataTransferExporter
		{
			public TestXmlDataTransferExporter(IValueObjectDataAdapter adapter, bool checkLicence)
				: base(adapter, checkLicence)
			{
			}
			public bool RaiseErrorOnExport;

			protected override void DoExport(Stream file, IList selectedBusinessObjects, INotifications notifications)
			{
				base.DoExport(file, selectedBusinessObjects, notifications);
				if (RaiseErrorOnExport)
				{
					notifications.Notify(new ErrorNotification(ErrorType.Error, "Error"));
				}
			}

			protected override bool DoExportToFile(IList selectedElements, IXmlDataTransferExporterGUI gui)
			{
				bool exported = base.DoExportToFile(selectedElements, gui);
				LastExportFileName = gui.UnmappedFile;
				return exported;
			}

			public string LastExportFileName { get; set; }
		}

		#endregion

		#region Implementation
		TestValueObjectDataAdapter DataAdapter;
		TestXmlDataTransferExporter exporter;

		protected override void SetUp()
		{
			base.SetUp();

			DataAdapter = new TestValueObjectDataAdapter();
			exporter = new TestXmlDataTransferExporter(DataAdapter, false);
		}

		#endregion

	}
}
