using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.JAS.Business;
using Enterprise.Client.JAS.Business.JXC;
using Enterprise.Client.JAS.Business.JXC.Export;
using Enterprise.Client.JAS.Business.JXC.Export.Validations.Testing;
using Enterprise.Client.JAS.Business.JXC.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.JAS.GUI.Testing
{
	internal class JXCMessageGUIExportDirectorTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			JXCMessageGUIExportDirector director = new JXCMessageGUIExportDirector(Exporter, ExportForm);
			AssertEquals("Should be assigned in the constructor", Exporter, director.Exporter);
			AssertEquals("Should be assigned in the constructor", ExportForm, director.ExportForm);
		}

		public void TestConstructor_NullArguments()
		{
			try
			{
				new JXCMessageGUIExportDirector(null, null);
				Fail("Should throw ArgumentNullException");
			}
			catch (ArgumentNullException ex)
			{
				AssertEquals("Exporter", ex.ParamName);
			}

			try
			{
				new JXCMessageGUIExportDirector(Exporter, null);
				Fail("Should throw ArgumentNullException");
			}
			catch (ArgumentNullException ex)
			{
				AssertEquals("ExportForm", ex.ParamName);
			}

			try
			{
				ExportForm.BusinessEntity = null;
				new JXCMessageGUIExportDirector(Exporter, ExportForm);
				Fail("Should throw ArgumentNullException");
			}
			catch (ArgumentNullException ex)
			{
				AssertEquals("ExportForm.BusinessEntity", ex.ParamName);
			}
		}

		#region EnsureMessageCanBeExported
		public void TestEnsureHasNoChangesAndInDatabase()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Exporter.HeaderData.IsInDatabaseForTest = true;
			Exporter.HeaderData.HasChanges = false;
			AdditionalPreExportCheckMethodShouldPass = true;
			Assert("Should be true", Director.EnsureMessageCanBeExported());
			Assert("HeaderData has no changes and saved in the database, should pass this check", UnitTestUserNotification.Instance.LastMessage.WasNone);
			Assert("Should call AdditionalPreExportCheckMethod if this check passes", AdditionalPreExportCheckMethodCalled);
			Assert("Should call ValidateAll if this check and AdditionalPreExportCheckMethod pass", ExportForm.ValidateAllCalled);
			AdditionalPreExportCheckMethodCalled = false;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Exporter.HeaderData.IsInDatabaseForTest = false;
			ExportForm.ValidateAllCalled = false;
			Assert("Should be false", !Director.EnsureMessageCanBeExported());
			AssertEquals("HeaderData is not in the database", "You must save before you can export data to JXC file", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("HeaderData is not in the database", UnitTestUserNotification.Instance.LastMessage.WasError);
			Assert("Should not call AdditionalPreExportCheckMethod if this check fails", !AdditionalPreExportCheckMethodCalled);
			Assert("Should not call ValidateAll if this check fails", !ExportForm.ValidateAllCalled);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Exporter.HeaderData.IsInDatabaseForTest = true;
			Exporter.HeaderData.HasChanges = true;
			Assert("Should be false", !Director.EnsureMessageCanBeExported());
			AssertEquals("HeaderData is in the database but has changes", "You must save before you can export data to JXC file", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("HeaderData is in the database but has changes", UnitTestUserNotification.Instance.LastMessage.WasError);
			Assert("Should not call AdditionalPreExportCheckMethod if this check fails", !AdditionalPreExportCheckMethodCalled);
			Assert("Should not call ValidateAll if this check fails", !ExportForm.ValidateAllCalled);
		}

		public void TestEnsureHasNoChangesAndInDatabase_ForNonPersistentBusinessObject()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ExportForm.BusinessEntity = new JXCHeaderForTest();
			AdditionalPreExportCheckMethodShouldPass = true;
			Assert("Should be true. Should not check for HasChanges and IsInDatabase for NonPersistentBusinessObject", Director.EnsureMessageCanBeExported());
		}

		public void TestAdditionalPreExportCheckMethodCalled()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AdditionalPreExportCheckMethodShouldPass = true;
			Exporter.HeaderData.IsInDatabaseForTest = true;
			Exporter.HeaderData.HasChanges = false;
			Assert("Pre-condition", !AdditionalPreExportCheckMethodCalled);
			Assert("Pre-condition", !ExportForm.ValidateAllCalled);
			Assert("Should be true", Director.EnsureMessageCanBeExported());
			Assert("Should be called", AdditionalPreExportCheckMethodCalled);
			Assert("Should be called if this check passes", ExportForm.ValidateAllCalled);
			Assert("Should not have any notifications", UnitTestUserNotification.Instance.LastMessage.WasNone);
			AdditionalPreExportCheckMethodShouldPass = false;
			AdditionalPreExportCheckMethodCalled = false;
			ExportForm.ValidateAllCalled = false;
			Assert("Should be false", !Director.EnsureMessageCanBeExported());
			Assert("Should be called", AdditionalPreExportCheckMethodCalled);
			Assert("Should not be called if this check fails", !ExportForm.ValidateAllCalled);
			Assert("Should not have any notifications", UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		public void TestEnsureHasNoErrorsAndJXCValidationWarnings()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			DummyBusinessObject dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			DummyChildBusinessObject dummyChild = dummy.Collection.AddNew();
			dummyChild.FillWithValidTestData();
			dummy.RegisterEditableChildObject(dummyChild);
			Factory.Save();
			ExportForm.BusinessEntity = dummy;
			AdditionalPreExportCheckMethodShouldPass = true;
			Exporter.ExportValidationTypeToUseForTest = JXCExportValidationType.Air;
			Assert("Dummy has no errors or JXC warnings, should be true", Director.EnsureMessageCanBeExported());
			Assert("Should not have any error dialog box", UnitTestUserNotification.Instance.LastMessage.WasNone);
			AssertNull("Should not show JXC Warning Message box", ZFormModaliser.LastFormShownDialogForTest);
			Exporter.ExportValidationTypeToUseForTest = JXCExportValidationType.None;
			dummy.MarkAsNeedingValidationIncludingChildren();
			var manager = new JXCDomainValidationManagerTest();
			Director.ManageJXCValidationForTest = (BusinessObjectFactory factory) => { manager.ManageJXCValidations_None(factory); };
			Assert("Dummy and dummy childs has JXC warnings/errors, should be false", !Director.EnsureMessageCanBeExported());
			Assert("DummyChild has errors, should show an error dialog box", UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals("There are errors that need to be corrected before JXC message can be exported", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNull("Should not show JXC Warning Message box", ZFormModaliser.LastFormShownDialogForTest);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			dummyChild.Z0_AnotherDecimal = 12;
			Factory.Save();
			Assert("Dummy has JXC warnings, should be false", !Director.EnsureMessageCanBeExported());
			Assert("Should not have any error dialog box", UnitTestUserNotification.Instance.LastMessage.WasNone);
			AssertEquals("Should show JXC Warning Message box", typeof(JXCWarningMessageBox), ZFormModaliser.LastFormShownDialogForTest.GetType());
			JXCWarningMessageBox messageBox = (JXCWarningMessageBox)ZFormModaliser.LastFormShownDialogForTest;
			List<JXCWarningInfo> warnings = new List<JXCWarningInfo>(messageBox.WarningInfoCollector);
			AssertEquals("Should show the warning from dbo.DummyBizO", 1, warnings.Count);
			AssertEquals("Should show the warning from dbo.DummyBizO", "MEH MEH", warnings[0].WarningMessage);
		}

		public void TestEnsureMessageCanBeExportedWithoutAdditionalPreExportCheckMethod()
		{
			fDirector = new JXCMessageGUIExportDirector(Exporter, ExportForm);
			Exporter.HeaderData.IsInDatabaseForTest = true;
			Exporter.HeaderData.HasChanges = false;
			Assert("Should return true", Director.EnsureMessageCanBeExported());
		}

		#endregion
		#region Export
		public void TestExport_UseDefaultDirectory()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			JASDataRegistry.Instance.QueryUserForDirectoryOnManualJXCExport = false;
			AssertEquals("Pre-condition", 0, Directory.GetFiles(DefaultExportDir).Length);
			AssertEquals("Pre-condition", 0, Directory.GetFiles(UserDir).Length);
			Exporter.ExpectedFileName = "HAHA.txt";
			Director.Export();
			string[] exportedFiles = Directory.GetFiles(DefaultExportDir);
			AssertEquals("Should be exporting it to the default export directory", 1, exportedFiles.Length);
			AssertEquals("HAHA.txt", Path.GetFileName(exportedFiles[0]));
			AssertEquals("Should be exporting it to the default export directory", 0, Directory.GetFiles(UserDir).Length);
			Assert("Should be an information dialog", UnitTestUserNotification.Instance.LastMessage.WasInformation);
			AssertEquals("Dialog message", "JXC Message for 'MEHMEH' has been successfully exported to \"" + DefaultExportDir + "\"", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestExport_QueryUserForDirectory()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			JASDataRegistry.Instance.QueryUserForDirectoryOnManualJXCExport = true;
			AssertEquals("Pre-condition", 0, Directory.GetFiles(DefaultExportDir).Length);
			AssertEquals("Pre-condition", 0, Directory.GetFiles(UserDir).Length);
			Exporter.ExpectedFileName = "TESTFILE.txt";
			ZFormModaliser.PathToSelectInShowCommonDialog = UserDir;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			Director.Export();
			string[] exportedFiles = Directory.GetFiles(UserDir);
			AssertEquals("Should be exporting it to the directory specified by user", 1, exportedFiles.Length);
			AssertEquals("TESTFILE.txt", Path.GetFileName(exportedFiles[0]));
			AssertEquals("Should be exporting it to the directory specified by user", 0, Directory.GetFiles(DefaultExportDir).Length);
			Assert("Should be an information dialog", UnitTestUserNotification.Instance.LastMessage.WasInformation);
			AssertEquals("Dialog message", "JXC Message for 'MEHMEH' has been successfully exported to \"" + UserDir + "\"", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestExport_DoNotExportIfUserCancelled()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			JASDataRegistry.Instance.QueryUserForDirectoryOnManualJXCExport = true;
			AssertEquals("Pre-condition", 0, Directory.GetFiles(DefaultExportDir).Length);
			AssertEquals("Pre-condition", 0, Directory.GetFiles(UserDir).Length);
			Exporter.ExpectedFileName = "TESTFILE.txt";
			ZFormModaliser.PathToSelectInShowCommonDialog = UserDir;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			Director.Export();
			AssertEquals("Cancelled by user, should not be exported", 0, Directory.GetFiles(DefaultExportDir).Length);
			AssertEquals("Cancelled by user, should not be exported", 0, Directory.GetFiles(UserDir).Length);
			Assert("There should be no notification dialog, JXC message not exported", UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		public void TestExportFails()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			JASDataRegistry.Instance.QueryUserForDirectoryOnManualJXCExport = false;
			Exporter.WriteToFileShouldFail = true;
			Director.Export();
			Assert("Should be an error dialog", UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals("Dialog message", "Failed to export JXC Message for 'MEHMEH'. Please refer to the JXC Export Log note in the Notes tab", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion
		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			DefaultExportDir = Path.Combine(Env.TempPath, "JXCMessageGUIExportDirectorTest\\Default");
			Directory.CreateDirectory(DefaultExportDir);
			UserDir = Path.Combine(Env.TempPath, "JXCMessageGUIExportDirectorTest\\User");
			Directory.CreateDirectory(UserDir);
			JASDataRegistry.Instance.JXCOutgoingDirectoryName = DefaultExportDir;
		}

		protected override void TearDown()
		{
			TempDirectory.DeleteDirectory(Path.Combine(Env.TempPath, "JXCMessageGUIExportDirectorTest"));
			ExportForm.Dispose();
			base.TearDown();
		}

		JXCMessageGUIExportDirector Director
		{
			get
			{
				if (fDirector == null)
				{
					fDirector = new JXCMessageGUIExportDirector(Exporter, ExportForm);
					fDirector.AdditionalPreExportCheck += AdditionalPreExportCheckMethod;
				}

				return fDirector;
			}
		}

		JXCMessageExporterForTest Exporter
		{
			get
			{
				if (fExporter == null)
				{
					fExporter = new JXCMessageExporterForTest();
				}

				return fExporter;
			}
		}

		JXCExportFormForTest ExportForm
		{
			get
			{
				if (fExportForm == null)
				{
					fExportForm = new JXCExportFormForTest();
					fExportForm.BusinessEntity = Exporter.HeaderData;
				}

				return fExportForm;
			}
		}

		CancelEventHandler AdditionalPreExportCheckMethod
		{
			get
			{
				if (fAdditionalPreExportCheckMethod == null)
				{
					fAdditionalPreExportCheckMethod = delegate(object sender, CancelEventArgs e)
					{
						AdditionalPreExportCheckMethodCalled = true;
						e.Cancel = !AdditionalPreExportCheckMethodShouldPass;
					};
				}

				return fAdditionalPreExportCheckMethod;
			}
		}

		JXCMessageGUIExportDirector fDirector;
		JXCMessageExporterForTest fExporter;
		JXCExportFormForTest fExportForm;
		CancelEventHandler fAdditionalPreExportCheckMethod;
		string DefaultExportDir;
		string UserDir;
		bool AdditionalPreExportCheckMethodCalled;
		bool AdditionalPreExportCheckMethodShouldPass;
		#region class JXCMessageExporterForTest
		class JXCMessageExporterForTest : JXCMessageExporter
		{
			public JXCMessageExporterForTest() : base(new BusinessObjectFactory().New<DummyBusinessObjectForTest>(), new NotificationBuffer())
			{
			}

			public JXCMessageExporterForTest(IJXCExportHeader exportHeader) : base(exportHeader, new NotificationBuffer())
			{
			}

			protected override MessageFileNameAndContents[] GetMessageFileNamesAndContents()
			{
				return null;
			}

			public override JXCExportValidationType ExportValidationTypeToUse
			{
				get
				{
					return ExportValidationTypeToUseForTest;
				}
			}

			public JXCExportValidationType ExportValidationTypeToUseForTest;
			protected override bool WriteToFileCore(ZString exportPath)
			{
				bool result;
				AssertEquals("Cursor has to be set to WaitCursor", Cursors.WaitCursor, Cursor.Current);
				if (!WriteToFileShouldFail)
				{
					string exportFile = Path.Combine(exportPath, ExpectedFileName);
					using (StreamWriter writer = new StreamWriter(exportFile))
					{
						writer.Write("");
					}

					result = true;
				}
				else
				{
					result = false;
				}

				return result;
			}

			public new DummyBusinessObjectForTest HeaderData
			{
				get
				{
					return (DummyBusinessObjectForTest)base.HeaderData;
				}
			}

			public bool WriteToFileShouldFail;
			public string ExpectedFileName;
		}

		#endregion
		#region class JXCExportFormForTest
		class JXCExportFormForTest : ZForm, IJXCExportForm
		{
			#region IJXCExportForm Members
			public void ValidateAll()
			{
				AssertEquals("Cursor has to be set to WaitCursor", Cursors.WaitCursor, Cursor.Current);
				ValidateAllCalled = true;
				BusinessEntity.RunPreSaveValidation();
			}

			public new BusinessObject BusinessEntity
			{
				get
				{
					return fBusinessEntity;
				}

				set
				{
					fBusinessEntity = value;
				}
			}

			public bool ValidateAllCalled;
			BusinessObject fBusinessEntity;
			#endregion
		}

		#endregion
		#region class DummyBusinessObjectForTest
		class DummyBusinessObjectForTest : DummyBaseBusinessObject, IJXCExportHeader
		{
			public DummyBusinessObjectForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override ZString HumanReadableNameCore
			{
				get
				{
					return "MEHMEH";
				}
			}

			public override bool IsInDatabase
			{
				get
				{
					return IsInDatabaseForTest;
				}
			}

			public bool IsInDatabaseForTest;
			#region IJXCExportHeader Members
			ZString IJXCExportHeader.FreightDest
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}

			JASOrgHeader IJXCExportHeader.SendingForwarder
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}

			JASOrgHeader IJXCExportHeader.ReceivingForwarder
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}
			#endregion
		}
		#endregion
		#endregion
	}
}
