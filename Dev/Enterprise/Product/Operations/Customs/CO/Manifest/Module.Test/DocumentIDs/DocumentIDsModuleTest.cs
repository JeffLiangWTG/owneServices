using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CO.Manifest.Module.Testing
{
	[TestedType(typeof(DocumentIDsModule))]
	class DocumentIDsModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CO.DocumentIDs;

		protected override string CountryCode => Core.Constants.CountryCodes.Colombia;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			return (CusTransactionNumber)base.GetNewBusinessObjectForHelperFilterTests(factory, businessObjectType);
		}

		public void TestAllowNewDeleteEditAndWorkflowTypeAndSupportsWorkflow()
		{
			using (var module = new DocumentIDsModule())
			{
				AssertEquals(false, module.SupportsWorkflow);
				AssertEquals(false, module.AllowDelete);
				AssertEquals(false, module.AllowEdit);
				AssertEquals(false, module.AllowNew);
			}
		}

		public void TestGetNewFilterControl()
		{
			using (DocumentIDsModuleForTest module = new DocumentIDsModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is DocumentIDsFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (DocumentIDsModuleForTest module = new DocumentIDsModuleForTest())
			{
				IBusinessObjectCollection collection = module.NewGridCollection;
				Assert("Invalid type", collection is DocumentIDsCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (DocumentIDsModuleForTest module = new DocumentIDsModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is DocumentIDsFilterBusinessObject);
			}
		}

		public void TestImportNumbersFile()
		{
			using (DocumentIDsModuleForTest module = new DocumentIDsModuleForTest())
			using (var form = new ZForm())
			using (var tempFile = TempFile.New(Temp.TempPath, "txt"))
			{
				try
				{
					var filterControl = module.EmbeddedControl;
					form.Controls.Add(filterControl);
					form.Show();

					Application.DoEvents();

					var dataTransferMenu = module.DataTransferMenuItem;
					dataTransferMenu.OnPopup(EventArgs.Empty);

					var importNumbersFileMenu = module.DataTransferMenuItem.MenuItems.FindByText("Import number's file");
					AssertNotNull("Import nummber's file menu is visible", importNumbersFileMenu);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;

					importNumbersFileMenu.PerformClick();
					AssertEquals("The file is empty.", UnitTestUserNotification.Instance.LastMessage.Text);

					using (TextWriter writer = new StreamWriter(tempFile.Filename))
					{
						writer.WriteLine("[11667803932049, 11667803932056]");
						writer.Flush();
					}

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;

					importNumbersFileMenu.PerformClick();
					AssertEquals("2 Document ID(s) have been saved.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;

					importNumbersFileMenu.PerformClick();
					AssertEquals("We can not upload this file. We found at least one document id in the system.\nCheck if it has been uploaded before.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
				finally
				{
					DeleteIfExists(tempFile.Filename);
				}

				AssertEquals("Document Id 11667803932049 is in the system", ZBool.True, FindDocumentId("11667803932049"));
				AssertEquals("Document Id 11667803932056 is in the system", ZBool.True, FindDocumentId("11667803932056"));
			}
		}

		ZBool FindDocumentId(ZString docId)
		{
			var query = new ZDBOnlyQuery(typeof(CusTransactionNumber));
			query.AddToFilter(CusTransactionNumberSchema.TN_Type, CusTransactionNumberTypeList.Codes.ColombiaManifest);
			query.AddToFilter(CusTransactionNumberSchema.TN_IsUsed, ZBool.False);
			query.AddToFilter(CusTransactionNumberSchema.TN_TransactionReference, docId);
			return Factory.Load<CusTransactionNumber>(query)?.Length > 0;
		}
	}

	#region DocumentIDsModuleForTest

	public class DocumentIDsModuleForTest : DocumentIDsModule
	{
		public DocumentIDsModuleForTest() { }

		public IFilterControl NewFilterControl => GetNewFilterControl();
		public IBusinessObjectCollection NewGridCollection => GetNewGridCollection();
		public FilterBusinessObject NewFilterBusinessObject => GetNewFilterBusinessObject();
	}

	#endregion
}
