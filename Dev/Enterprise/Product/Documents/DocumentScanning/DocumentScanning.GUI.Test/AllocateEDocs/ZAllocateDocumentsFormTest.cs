using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	sealed class ZAllocateDocumentsFormTest : TestCaseWithDocumentFactory
	{
#if !WINZOR
		const int UnallocatedDocumentsTabPageIndex = 1;
		const int AllocatedDocumentsTabPageIndex = 2;
#else
		const int UnallocatedDocumentsTabPageIndex = 0;
		const int AllocatedDocumentsTabPageIndex = 1;
#endif

		public void TestImagePageBufferMenuItemEnableStatusWithNoneFile()
		{
			var allocateDocumentsManager = new AllocateDocumentsManager(MasterFactory);
			using (var form = new ZAllocateDocumentsForm(allocateDocumentsManager))
			{
				form.Show();
				var menuItems = form.PreviewPane.ThumbnailPanel.ContextMenu.MenuItems;
				CombineAssertions(() =>
				{
					AssertEquals($"No. Images across page Enabled should be true", true, menuItems[0].Enabled);
					AssertEquals($"Cut Enabled should be false", false, menuItems[2].Enabled);
					AssertEquals($"Copy Enabled should be false", false, menuItems[3].Enabled);
					AssertEquals($"Paste Enabled should be false", false, menuItems[4].Enabled);
					AssertEquals($"Move to New Document Enabled should be false", false, menuItems[5].Enabled);
					AssertEquals($"Delete Enabled should be false", false, menuItems[6].Enabled);
					AssertEquals($"Select All Enabled should be false", false, menuItems[8].Enabled);
				});
			}
		}

		public void TestSetThumbnailPanelContextMenuReadOnly()
		{
			using (var control = new GraphicalDisplayControl())
			{
				var tempFile = MasterFactory.New<TempStorageDocs>();
				tempFile.SC_ImageData = new ZBlob(new byte[] { 1 });
				var documentType = AllocateEDocsHelper.GetPreviewableDocumentType(tempFile);
				control.SetThumbnailPanelContextMenuReadOnly(documentType);
				var menuItems = control.ThumbnailPanel.ContextMenu.MenuItems;

				CombineAssertions(() =>
				{
					AssertEquals($"No. Images across page Enabled should be true", true, menuItems[0].Enabled);
					AssertEquals($"Cut Enabled should be false", false, menuItems[2].Enabled);
					AssertEquals($"Copy Enabled should be true", true, menuItems[3].Enabled);
					AssertEquals($"Paste Enabled should be false", false, menuItems[4].Enabled);
					AssertEquals($"Move to New Document Enabled should be false", false, menuItems[5].Enabled);
					AssertEquals($"Delete Enabled should be false", false, menuItems[6].Enabled);
					AssertEquals($"Select All Enabled should be true", true, menuItems[8].Enabled);
				});

				var eDocsFile = MasterFactory.New<StorageDocsUnallocated>();
				eDocsFile.SC_ImageData = new ZBlob(new byte[] { 1 });
				documentType = AllocateEDocsHelper.GetPreviewableDocumentType(eDocsFile);
				control.SetThumbnailPanelContextMenuReadOnly(documentType);

				CombineAssertions(() =>
				{
					AssertEquals($"No. Images across page Enabled should be true", true, menuItems[0].Enabled);
					AssertEquals($"Cut Enabled should be true", true, menuItems[2].Enabled);
					AssertEquals($"Copy Enabled should be true", true, menuItems[3].Enabled);
					AssertEquals($"Paste Enabled should be true", true, menuItems[4].Enabled);
					AssertEquals($"Move to New Document Enabled should be true", true, menuItems[5].Enabled);
					AssertEquals($"Delete Enabled should be true", true, menuItems[6].Enabled);
					AssertEquals($"Select All Enabled should be true", true, menuItems[8].Enabled);
				});
			}
		}

#if !WINZOR
		public void TestShowPostImportMessages()
		{
			AllocateDocumentsManager allocateDocumentsManager = new AllocateDocumentsManager(MasterFactory);
			using (TempFile tempFile = TempFile.New())
			{
				using (ZAllocateDocumentsForm allocateForm = new ZAllocateDocumentsForm(allocateDocumentsManager))
				{
					allocateForm.Show();
					AssertEquals("Precond: no files in FilesNotDeletedOnImport collection", 0, allocateDocumentsManager.FileImporterForImport.FilesNotDeletedOnImport.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);

					allocateDocumentsManager.FileImporterForImport.FilesNotDeletedOnImport.Add(tempFile.Filename);
					AssertEquals("Should have one file in collection", 1, allocateDocumentsManager.FileImporterForImport.FilesNotDeletedOnImport.Count);
					allocateForm.ShowPostImportMessages(allocateDocumentsManager.FileImporterForImport, 1, 1, new List<string>());

					AssertEquals("Collection should clear", 0, allocateDocumentsManager.FileImporterForImport.FilesNotDeletedOnImport.Count);
				}
			}
		}
#endif

		public void TestColumnModuleShowingWithNull()
		{
			var allocateDocumentsManager = new AllocateDocumentsManager(MasterFactory);
			using (var allocateForm = new ZAllocateDocumentsForm(allocateDocumentsManager))
			{
				using (var grid = new ZGrid())
				{
					var currentBizObj = (BusinessObject)grid.ListManager.GetCurrent();
					var args = new FindBoxColumnModuleShowingEventArgs(null, currentBizObj, null);
					try
					{
						allocateForm.Grid_FindBoxColumnModuleShowing(null, args);
						AssertEquals(1, ErrorReporter.TotalErrorCount);
					}
					finally
					{
						ErrorReporter.Clear();
					}
				}
			}
		}

		[ExpectNoExceptions]
		public void TestColumnModuleShowingWithStorageDocsUnallocated()
		{
			var allocateDocumentsManager = new AllocateDocumentsManager(MasterFactory);
			using (var allocateForm = new ZAllocateDocumentsForm(allocateDocumentsManager))
			{
				using (var grid = new ZGrid())
				{
					var unallocatedDocument = MasterFactory.New<StorageDocsUnallocated>();
					unallocatedDocument.SC_DataType = "WPI";
					var args = new FindBoxColumnModuleShowingEventArgs(null, unallocatedDocument, null);
					allocateForm.Grid_FindBoxColumnModuleShowing(null, args);
				}
			}
		}

		public void TestUserInputtingInvalidCharacterIntoType()
		{
			var allocateDocumentsManager = new AllocateDocumentsManager(MasterFactory);
			allocateDocumentsManager.UnallocatedDocuments.RemoveAndDeleteAll();
			var document = MasterFactory.New<StorageDocsUnallocated>();
			document.FillWithValidTestData();
			allocateDocumentsManager.UnallocatedDocuments.Add(document);
			using (var form = new ZAllocateDocumentsFormForTesting(allocateDocumentsManager))
			{
				form.DocumentAllocationTabControl.SelectedIndex = UnallocatedDocumentsTabPageIndex;
				form.UnallocatedGridControl.Grid.DataSource = form.DataSource;
				form.Show();

				var curItem = (form.UnallocatedGridControl.Grid.CurrentElement) as StorageDocsUnallocated;
				curItem.SC_DataType = "*";
				AssertNoExceptionThrown(() => form.OnLoadForTesting(null)); //We just want to call UpdatePreviewPane(), which is called by event. Does not have to be onLoad!
			}
		}

		public void TestProcessCtrlA()
		{
			AllocateDocumentsManager allocateDocumentsManager = new AllocateDocumentsManager(MasterFactory);
			using (ZAllocateDocumentsForm allocateForm = new ZAllocateDocumentsForm(allocateDocumentsManager))
			{
				Keys ignoreKeyData = Keys.A;
				Assert("Key combo not ctrl + A, not processed", !allocateForm.ProcessAltA(ignoreKeyData));

				Keys correctKeyData = Keys.A | Keys.Alt;
				Assert("Key combo Ctrl + A, key combo should be procesed", allocateForm.ProcessAltA(correctKeyData));

				allocateDocumentsManager.ReadOnly = true;
				Assert("Readonly form means that Key combo is not processed", !allocateForm.ProcessAltA(correctKeyData));
			}
		}

		public void TestPressingRefreshWillSaveBeforeReloading()
		{
			StorageDocsUnallocated unallocatedDocument = MasterFactory.New<StorageDocsUnallocated>();
			unallocatedDocument.SM_Type = "UNA";
			MasterFactory.Save();
			AllocateDocumentsManager allocateDocumentsManager = new AllocateDocumentsManager(MasterFactory);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);

			using (ZAllocateDocumentsForm allocateForm = new ZAllocateDocumentsForm(allocateDocumentsManager))
			{
				allocateForm.Show();
				allocateDocumentsManager.UnallocatedDocuments[0].SC_Desc = "Hello";
				Assert("Manager object has changes", allocateDocumentsManager.HasChanges);
				allocateForm.RefreshButton.PerformClick();
				Assert("Save happend on click of the refresh button. Docmanager doesn't have changes anymore", !allocateDocumentsManager.HasChanges);
			}
		}

		public void TestPressingRefreshIfManagerDoesNotHaveChangesShouldNotPrompt()
		{
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);

			AllocateDocumentsManager allocateDocumentsManager = new AllocateDocumentsManager(MasterFactory);
			AssertEquals("Precondition: No unallocated documents", 0, allocateDocumentsManager.UnallocatedDocuments.Count);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (ZAllocateDocumentsForm allocateForm = new ZAllocateDocumentsForm(allocateDocumentsManager))
			{
				allocateForm.Show();

				DocumentFactory anotherFactory = new DocumentFactoryProvider().GetFactory(Factory);
				StorageDocsUnallocated unallocatedDocument = anotherFactory.New<StorageDocsUnallocated>();
				unallocatedDocument.SM_Type = "UNA";
				anotherFactory.Save(); // saved in a different factory to the one behind the form.

				allocateForm.RefreshButton.PerformClick();
				AssertEquals("Should now have one document in unallocated documents; no prompt required", 1, allocateDocumentsManager.UnallocatedDocuments.Count);
			}
		}

		[RequiresSTA]
		public void TestConcurrencyExceptionShouldNotThrowWhenRefreshData()
		{
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			StorageDocsUnallocated unallocatedDocument = MasterFactory.New<StorageDocsUnallocated>();
			unallocatedDocument.SM_Type = "UNA";
			MasterFactory.Save();
			AllocateDocumentsManager allocateDocumentsManager = new AllocateDocumentsManager(MasterFactory);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);

			using (ZAllocateDocumentsForm allocateForm = new ZAllocateDocumentsForm(allocateDocumentsManager))
			{
				allocateForm.Show();

				using (new TemporaryUserContext() { StaffLoginName = User.SupportUserName, BranchPK = Env.CurrentBranch.PK, DepartmentPK = Env.CurrentDepartment.PK }.Set())
				{
					DocumentFactory newFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
					StorageDocsUnallocated newunallocatedDocument = newFactory.Load<StorageDocsUnallocated>(unallocatedDocument.PK);
					newunallocatedDocument.SM_Type = "TSK";

					using (GetFactoryIsolater(newFactory))
					using (GetFactoryIsolater(MasterFactory))
					{
						newFactory.Save();
					}
				}

				allocateDocumentsManager.UnallocatedDocuments[0].SC_Desc = "Hello";
				Assert("Manager object has changes", allocateDocumentsManager.HasChanges);
				AssertNoExceptionThrown("Concurrency exception should not throw.", allocateForm.RefreshButton.PerformClick);
			}
		}

		public void TestUnallocatedDocumentsContextMenuShowsDeletePermanently()
		{
			var unallocatedDocument = MasterFactory.New<StorageDocsUnallocated>();
			unallocatedDocument.SM_Type = "UNA";
			MasterFactory.Save();
			var allocateDocumentsManager = new AllocateDocumentsManager(MasterFactory);

			using (var form = new ZAllocateDocumentsForm(allocateDocumentsManager))
			{
				var grid = form.UnallocatedGridControl.Grid;
				Assert(grid.ShowDeletePermanentlyMenuItem);
			}
		}
		public void TestUnallocatedDocumentsViewCount_WhenNotSetToAllCompanies_DoesNotIncludeOtherCompanyDocuments()
		{
			StorageDocsUnallocated unallocatedDocument = MasterFactory.New<StorageDocsUnallocated>();
			unallocatedDocument.SM_Type = "UNA";
			MasterFactory.Save();
			AllocateDocumentsManager allocateDocumentsManager = new AllocateDocumentsManager(MasterFactory);
			AssertEquals("Precondition: Not showing document even though company has not been set", 1, allocateDocumentsManager.UnallocatedDocumentsView.Count);

			var companyKey = ZGuid.NewZGuid();
			unallocatedDocument.SC_GC_Company = companyKey;
			MasterFactory.Save();

			AssertEquals("Should not show document belonging to other company", 0, allocateDocumentsManager.UnallocatedDocumentsView.Count);
		}

		public void TestUnallocatedDocumentsViewCount_WhenSetToAllCompanies_IncludesOtherCompanyDocuments()
		{
			StorageDocsUnallocated unallocatedDocument = MasterFactory.New<StorageDocsUnallocated>();
			unallocatedDocument.SM_Type = "UNA";
			var companyKey = ZGuid.NewZGuid();
			unallocatedDocument.SC_GC_Company = companyKey;
			MasterFactory.Save();
			AllocateDocumentsManager allocateDocumentsManager = new AllocateDocumentsManager(MasterFactory);

			using (ZAllocateDocumentsForm form = new ZAllocateDocumentsForm(allocateDocumentsManager))
			{
				form.DocumentAllocationTabControl.SelectedIndex = UnallocatedDocumentsTabPageIndex;
				form.Show();

				AssertEquals("Precondition: Size of grid not reflecting the view being used", allocateDocumentsManager.UnallocatedDocumentsView.Count, form.UnallocatedGridControl.Grid.List.Count);
				AssertEquals("Precondition: Shows document belonging to other company, before allowing all companies", 0, allocateDocumentsManager.UnallocatedDocumentsView.Count);

				form.UnallocatedDocumentsCompanySpecificCheckBox.Checked = true;

				AssertEquals("Precondition: Size of grid not reflecting the view being used", allocateDocumentsManager.UnallocatedDocumentsView.Count, form.UnallocatedGridControl.Grid.List.Count);
				AssertEquals("Should show document belonging to other company when requested to do so", 1, allocateDocumentsManager.UnallocatedDocumentsView.Count);
			}
		}

		public void TestUnallocatedDocumentsViewCount_WhenNotSetToAllBranches_DoesNotIncludeOtherBranchDocuments()
		{
			StorageDocsUnallocated unallocatedDocument = MasterFactory.New<StorageDocsUnallocated>();
			unallocatedDocument.SM_Type = "UNA";
			MasterFactory.Save();
			AllocateDocumentsManager allocateDocumentsManager = new AllocateDocumentsManager(MasterFactory);
			AssertEquals("Precondition: Not showing document even though branch has not been set", 1, allocateDocumentsManager.UnallocatedDocumentsView.Count);

			var branchKey = ZGuid.NewZGuid();
			unallocatedDocument.SC_GB_Branch = branchKey;
			MasterFactory.Save();

			AssertEquals("Should not show document belonging to other branch", 0, allocateDocumentsManager.UnallocatedDocumentsView.Count);
		}

		[RequiresSTA]
		public void TestUnallocatedDocumentsViewCount_WhenSetToAllBranches_IncludesOtherBranchDocuments()
		{
			StorageDocsUnallocated unallocatedDocument = MasterFactory.New<StorageDocsUnallocated>();
			unallocatedDocument.SM_Type = "UNA";
			var branchKey = ZGuid.NewZGuid();
			unallocatedDocument.SC_GB_Branch = branchKey;
			MasterFactory.Save();
			AllocateDocumentsManager allocateDocumentsManager = new AllocateDocumentsManager(MasterFactory);

			using (ZAllocateDocumentsForm form = new ZAllocateDocumentsForm(allocateDocumentsManager))
			{
				form.DocumentAllocationTabControl.SelectedIndex = UnallocatedDocumentsTabPageIndex;
				form.Show();

				AssertEquals("Precondition: Size of grid not reflecting the view being used", allocateDocumentsManager.UnallocatedDocumentsView.Count, form.UnallocatedGridControl.Grid.List.Count);
				AssertEquals("Precondition: Shows document belonging to other branch, before allowing all branches", 0, allocateDocumentsManager.UnallocatedDocumentsView.Count);

				form.UnallocatedDocumentsBranchSpecificCheckBox.Checked = true;

				AssertEquals("Precondition: Size of grid not reflecting the view being used", allocateDocumentsManager.UnallocatedDocumentsView.Count, form.UnallocatedGridControl.Grid.List.Count);
				AssertEquals("Should show document belonging to other branch when requested to do so", 1, allocateDocumentsManager.UnallocatedDocumentsView.Count);
			}
		}

		public void TestUnallocatedDocumentsViewCount_WhenNotSetToAllDepartments_DoesNotIncludeOtherDepartmentDocuments()
		{
			StorageDocsUnallocated unallocatedDocument = MasterFactory.New<StorageDocsUnallocated>();
			unallocatedDocument.SM_Type = "UNA";
			MasterFactory.Save();
			AllocateDocumentsManager allocateDocumentsManager = new AllocateDocumentsManager(MasterFactory);
			AssertEquals("Precondition: Not showing document even though department has not been set", 1, allocateDocumentsManager.UnallocatedDocumentsView.Count);

			var departmentKey = ZGuid.NewZGuid();
			unallocatedDocument.SC_GE_Department = departmentKey;
			MasterFactory.Save();

			AssertEquals("Should not show document belonging to other department", 0, allocateDocumentsManager.UnallocatedDocumentsView.Count);
		}

		public void TestUnallocatedDocumentsViewCount_WhenSetToAllDepartments_IncludesOtherDepartmentDocuments()
		{
			StorageDocsUnallocated unallocatedDocument = MasterFactory.New<StorageDocsUnallocated>();
			unallocatedDocument.SM_Type = "UNA";
			var departmentKey = ZGuid.NewZGuid();
			unallocatedDocument.SC_GE_Department = departmentKey;
			MasterFactory.Save();
			AllocateDocumentsManager allocateDocumentsManager = new AllocateDocumentsManager(MasterFactory);

			using (ZAllocateDocumentsForm form = new ZAllocateDocumentsForm(allocateDocumentsManager))
			{
				form.DocumentAllocationTabControl.SelectedIndex = UnallocatedDocumentsTabPageIndex;
				form.Show();

				AssertEquals("Precondition: Size of grid not reflecting the view being used", allocateDocumentsManager.UnallocatedDocumentsView.Count, form.UnallocatedGridControl.Grid.List.Count);
				AssertEquals("Precondition: Shows document belonging to other department, before allowing all departments", 0, allocateDocumentsManager.UnallocatedDocumentsView.Count);

				form.UnallocatedDocumentsDepartmentSpecificCheckBox.Checked = true;

				AssertEquals("Precondition: Size of grid not reflecting the view being used", allocateDocumentsManager.UnallocatedDocumentsView.Count, form.UnallocatedGridControl.Grid.List.Count);
				AssertEquals("Should show document belonging to other department when requested to do so", 1, allocateDocumentsManager.UnallocatedDocumentsView.Count);
			}
		}

		public void TestUnallocatedDocumentsViewCount_WhenNotSetToAllDeleted_DeletedDocuments()
		{
			var unallocatedDocument = MasterFactory.New<StorageDocsUnallocated>();
			unallocatedDocument.SM_Type = "UNA";
			MasterFactory.Save();
			var allocateDocumentsManager = new AllocateDocumentsManager(MasterFactory);
			AssertEquals("Precondition: Showing 1 document before deletion", 1, allocateDocumentsManager.UnallocatedDocumentsView.Count);

			unallocatedDocument.SC_IsDeleted = true;
			MasterFactory.Save();

			AssertEquals("Should not show deleted document", 0, allocateDocumentsManager.UnallocatedDocumentsView.Count);
		}

		public void TestUnallocatedDocumentsViewCount_WhenSetToAllDeleted_DeletedDocuments()
		{
			var unallocatedDocument = MasterFactory.New<StorageDocsUnallocated>();
			var unallocatedDocumentToDelete = MasterFactory.New<StorageDocsUnallocated>();
			unallocatedDocument.SM_Type = "UNA";
			unallocatedDocumentToDelete.SM_Type = "UNA";
			MasterFactory.Save();
			var allocateDocumentsManager = new AllocateDocumentsManager(MasterFactory);

			unallocatedDocumentToDelete.SC_IsDeleted = true;
			MasterFactory.Save();

			using (ZAllocateDocumentsForm form = new ZAllocateDocumentsForm(allocateDocumentsManager))
			{
				form.DocumentAllocationTabControl.SelectedIndex = UnallocatedDocumentsTabPageIndex;
				form.Show();

				AssertEquals("Precondition: Size of grid not reflecting the view being used", allocateDocumentsManager.UnallocatedDocumentsView.Count, form.UnallocatedGridControl.Grid.List.Count);
				AssertEquals("Precondition: Only showing non-deleted as all deleted has not been checked", 1, allocateDocumentsManager.UnallocatedDocumentsView.Count);

				form.UnallocatedDocumentsAllDeletedCheckBox.Checked = true;

				AssertEquals("Precondition: Size of grid not reflecting the view being used", allocateDocumentsManager.UnallocatedDocumentsView.Count, form.UnallocatedGridControl.Grid.List.Count);
				AssertEquals("Should show both non-deleted and deleted document when all deleted is checked", 2, allocateDocumentsManager.UnallocatedDocumentsView.Count);
			}
		}

		public void TestUnallocatedDocumentsViewCount_WhenNotSetToAllDeleted_DeletedPermanentlyDocuments()
		{
			var unallocatedDocument = MasterFactory.New<StorageDocsUnallocated>();
			unallocatedDocument.SM_Type = "UNA";
			MasterFactory.Save();
			var allocateDocumentsManager = new AllocateDocumentsManager(MasterFactory);
			AssertEquals("Precondition: Showing 1 document before permanent deletion", 1, allocateDocumentsManager.UnallocatedDocumentsView.Count);

			allocateDocumentsManager.DeleteDocumentsPermanently(new List<BusinessObject> { unallocatedDocument });
			MasterFactory.Save();

			AssertEquals("Should not show permanently deleted document", 0, allocateDocumentsManager.UnallocatedDocumentsView.Count);
		}

		[RequiresSTA]
		public void TestUnallocatedDocumentsViewCount_WhenSetToAllDeleted_DeletedPermanentlyDocuments()
		{
			var unallocatedDocument = MasterFactory.New<StorageDocsUnallocated>();
			unallocatedDocument.SM_Type = "UNA";
			MasterFactory.Save();
			var allocateDocumentsManager = new AllocateDocumentsManager(MasterFactory);

			allocateDocumentsManager.DeleteDocumentsPermanently(new List<BusinessObject> { unallocatedDocument });
			MasterFactory.Save();

			using (var form = new ZAllocateDocumentsForm(allocateDocumentsManager))
			{
				form.DocumentAllocationTabControl.SelectedIndex = UnallocatedDocumentsTabPageIndex;
				form.Show();

				AssertEquals("Precondition: Size of grid not reflecting the view being used", allocateDocumentsManager.UnallocatedDocumentsView.Count, form.UnallocatedGridControl.Grid.List.Count);
				AssertEquals("Precondition: Not showing as all deleted has not been checked and document is permanently deleted", 0, allocateDocumentsManager.UnallocatedDocumentsView.Count);

				form.UnallocatedDocumentsAllDeletedCheckBox.Checked = true;

				AssertEquals("Precondition: Size of grid not reflecting the view being used", allocateDocumentsManager.UnallocatedDocumentsView.Count, form.UnallocatedGridControl.Grid.List.Count);
				AssertEquals("Should still not show as document is permanently deleted", 0, allocateDocumentsManager.UnallocatedDocumentsView.Count);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImageShowsInGraphicalDisplayControlOnLoad()
		{
			var unallocatedDocument = MasterFactory.New<StorageDocsUnallocated>();
			unallocatedDocument.SM_Type = "UNA";
			unallocatedDocument.SC_ImageData = DocumentUtilities.GetFileAsBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\MultipageTifFile3.tif"));

			var allocateDocumentsManager = new AllocateDocumentsManager(MasterFactory);
			allocateDocumentsManager.UnallocatedDocuments.Add(unallocatedDocument);
			using (ZAllocateDocumentsForm form = new ZAllocateDocumentsForm(allocateDocumentsManager))
			{
				form.DocumentAllocationTabControl.SelectedIndex = UnallocatedDocumentsTabPageIndex;
				form.Show();
				AssertEquals("Should show the document", unallocatedDocument, form.PreviewPane.Document);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDeleteFileUnauthorizedAccess()
		{
			var filePath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\MultipageTifFile3.tif");
			var unallocatedDocument = MasterFactory.New<StorageDocsUnallocated>();

			unallocatedDocument.SM_Type = "UNA";
			unallocatedDocument.SC_ImageData = DocumentUtilities.GetFileAsBytes(filePath);

			var allocateDocumentsManager = new AllocateDocumentsManager(MasterFactory);
			allocateDocumentsManager.UnallocatedDocuments.Add(unallocatedDocument);

			using (var form = new ZAllocateDocumentsForm(allocateDocumentsManager))
			{
				form.DocumentAllocationTabControl.SelectedIndex = UnallocatedDocumentsTabPageIndex;
				form.Show();
				AssertEquals("Should show the document", unallocatedDocument, form.PreviewPane.Document);

				string tempfile = unallocatedDocument.TempFileName;
				File.SetAttributes(tempfile, FileAttributes.ReadOnly);
				AssertNoExceptionThrown(form.Dispose);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImageIsClearedWhenTabIndexChanged()
		{
			StorageDocsUnallocated unallocatedDocument = MasterFactory.New<StorageDocsUnallocated>();
			unallocatedDocument.SC_ImageData = DocumentUtilities.GetFileAsBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\MultipageTifFile3.tif"));
			AllocateDocumentsManager allocateDocumentsManager = new AllocateDocumentsManager(MasterFactory);
			allocateDocumentsManager.UnallocatedDocuments.Add(unallocatedDocument);
			using (ZAllocateDocumentsForm form = new ZAllocateDocumentsForm(allocateDocumentsManager))
			{
				form.DocumentAllocationTabControl.SelectedIndex = UnallocatedDocumentsTabPageIndex;
				form.Show();
				AssertEquals("should show the correct document", unallocatedDocument, form.PreviewPane.Document);
				AssertEquals("should show the correct document", 5, form.PreviewPane.PageSelector.TotalPages);

				form.DocumentAllocationTabControl.SelectedIndex = AllocatedDocumentsTabPageIndex;
				AssertNull("document should be empty", form.PreviewPane.PageSelector);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBadImage()
		{
			var unallocatedDocument = MasterFactory.New<StorageDocsUnallocated>();
			unallocatedDocument.SC_ImageData = DocumentUtilities.GetFileAsBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\UnsupportedFormat.tif"));
			var manager = new AllocateDocumentsManager(MasterFactory);
			manager.UnallocatedDocuments.Add(unallocatedDocument);

			using (var form = new ZAllocateDocumentsForm(manager))
			{
				form.DocumentAllocationTabControl.SelectedIndex = UnallocatedDocumentsTabPageIndex;
				form.Show();

				if (form.PreviewPane.Document == null)
				{
					AssertNull(form.PreviewPane.Document);
					Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("This image cannot be opened because file appears to be damaged or corrupted."));
				}
				else
				{
					AssertEquals(unallocatedDocument, form.PreviewPane.Document);
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				}
				CargoWise.Common.ErrorReporter.Clear();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRaceConditionOnLoadingImage()
		{
			var masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			masterFactory.RefreshEnabled = false;
			Factory.RefreshEnabled = false;

			StorageDocsUnallocated document = masterFactory.New<StorageDocsUnallocated>();

			document.SM_Type = "XXX";
			document.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			document.SC_DataType = "BMS";
			document.SC_Desc = "Hello";
			string onePageImageFile_2 = BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\NotCompressed.tif";
			document.SC_ImageData = DocumentUtilities.GetFileAsBytes(onePageImageFile_2);

			masterFactory.Save();

			// loads object without loading blob (lazy)
			var bizOFactory2 = MasterFactory.Load<StorageDocsUnallocated>(document.PK);

			AssertNotNull("Precondition", bizOFactory2);

			// this forces the data to be missing
			document.Delete();
			masterFactory.Save();

			Assert("Precondition", !bizOFactory2.IsDeleted);

			AllocateDocumentsManager allocateDocumentsManager = new AllocateDocumentsManager(MasterFactory);
			using (var form = new ZAllocateDocumentsForm(allocateDocumentsManager))
			{
				AssertNoExceptionThrown(form.Show);
			}
		}

#if !WINZOR
		public void TestFileImportFailedImports()
		{
			AllocateDocumentsManager allocateDocumentsManager = new AllocateDocumentsManager(MasterFactory);
			using (ZAllocateDocumentsForm form = new ZAllocateDocumentsForm(allocateDocumentsManager))
			{
				form.Show();
				form.ImportBoundButtonCore(new FileImporterFailer(MasterFactory, true));
				Assert(((UnitTestUserNotification)Globals.Message).LastMessage.ToString().Contains("Some pages in the image file (blah.jpg) have an unsupported compression type. Please make sure that all pages of the image have the same compression."));
				CargoWise.Common.ErrorReporter.Clear();
			}
		}

		public void TestManager_SingleFileImportedDoesntThrowExceptionOnExtraFiles()
		{
			using (ZAllocateDocumentsForm allocateDocumentsForm = new ZAllocateDocumentsForm(new AllocateDocumentsManager(MasterFactory)))
			using (var progressForm = new ProgressForm())
			{
				AssertNoExceptionThrown("Should not throw exception on extra files found", () => allocateDocumentsForm.Manager_SingleFileImportedForTesting(progressForm));
			}
		}
#endif

		[RequiresSTA]
		public void TestDeleteInProgressForm()
		{
			StorageDocsUnallocated unallocatedDocument = MasterFactory.New<StorageDocsUnallocated>();
			MasterFactory.Save();
			AllocateDocumentsManager allocateDocumentsManager = new AllocateDocumentsManager(MasterFactory);

			var documentsToBeDeleted = new List<BusinessObject> { unallocatedDocument };
			allocateDocumentsManager.DeleteDocumentsPermanently(documentsToBeDeleted);
			MasterFactory.Save();

			using (ZAllocateDocumentsForm allocateDocumentsForm = new ZAllocateDocumentsForm(new AllocateDocumentsManager(MasterFactory)))
			using (var progressForm = new ProgressForm())
			{
				AssertNoExceptionThrown("Displays correct message",
					() => allocateDocumentsForm.LoadDeletePermanentlyProgressFormForTesting(progressForm));
			}
		}

		public void TestShowCorruptedFileImageOnPreviewPane()
		{
			using (var allocateDocumentsForm = new ZAllocateDocumentsForm(new AllocateDocumentsManager(MasterFactory)))
			{
				var corruptedFileImageData = allocateDocumentsForm.GetCorruptedFileImageData();

				Assert("Should have returned the data from the corrupted file image", corruptedFileImageData.Any());
				AssertNull(allocateDocumentsForm.PreviewPane.Document);

				allocateDocumentsForm.ShowCorruptedFileImageOnPreviewPane();
				AssertEquals("PreviewPane.Document should have the same data as the corrupted file image", allocateDocumentsForm.PreviewPane.Document.SC_ImageData, corruptedFileImageData);
				Assert("PreviewPane should be read only", allocateDocumentsForm.PreviewPane.GetReadOnly());
			}
		}

		public void TestShowRestoreMenuItemIsSetToTrue()
		{
			using (var allocateDocumentsForm = new ZAllocateDocumentsForm(new AllocateDocumentsManager(MasterFactory)))
			{
				Assert("ShowRestoreMenuItem should be set to true", allocateDocumentsForm.UnallocatedGridControl.Grid.ShowRestoreMenuItem);
			}
		}

		#region Implementation

		public class FileImporterFailer : FileImporterExposed
		{
			public FileImporterFailer(DocumentFactory docFactory, bool isForImport)
				: base(docFactory, isForImport)
			{
			}

			public override int ImportFromDirectory(string sourceDirectory, List<String> failedImports)
			{
				failedImports.Add("Some pages in the image file (blah.jpg) have an unsupported compression type. Please make sure that all pages of the image have the same compression.");
				return 0;
			}

			public override int FileCount
			{
				get { return 1; }
			}
		}

		#endregion
	}

	[TestClass]
	sealed class ZAllocateDocumentsFormForTesting : ZAllocateDocumentsForm
	{
		public ZAllocateDocumentsFormForTesting(AllocateDocumentsManager dataSource)
			: base(dataSource)
		{
		}

		public void OnLoadForTesting(EventArgs e) => OnLoad(e);
	}
}
