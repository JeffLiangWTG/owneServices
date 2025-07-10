using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common.Interop;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Interop.DataObjects;
using CargoWise.IO;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.RichEdit.Testing
{
	sealed class ZRichEditOleObjectInserterTest : TestCaseWithFactory
	{
		const string RtfWithImage =
@"{\rtf1{\pict\pngblip
89504e470d0a1a0a0000000d4948445200000008000000080806000000c40fbe8b000000017352474200aece1ce90000000467414d410000b18f0bfc6105000000206348524d00007a26000080840000fa00000080e8000075300000ea6000003a98000017709cba513c0000001b49444154285363646060f80fc4780148013e8c57126cfaa0370100dd9338c98577cd310000000049454e44ae426082}}";

#if !WINZOR
		[ExpectNoExceptions]
		public void TestInsertBitmapFile_WithoutZVersionOfRichTextBox_WithoutZRichEditOleObjectInserter_TryNotUsingFileForInsertButTryMakingDataObjectWithFileFirstThenIn()
		{
			var data = new DataObject(DataFormats.Rtf, RtfWithImage);

			var dataObjGuid = typeof(System.Runtime.InteropServices.ComTypes.IDataObject).GUID;
			var pDataObj = IntPtr.Zero;
			var pDataObjectUnk = Marshal.GetIUnknownForObject(data);
			try
			{
				Marshal.QueryInterface(pDataObjectUnk, ref dataObjGuid, out pDataObj);
				using (var locator = ZComRichEditOleInterfaceLocator.GetInstance(RichEdit.RichEdit))
				{
					locator.RichEditOle.ImportDataObject(pDataObj, 0, IntPtr.Zero);
				}
			}
			finally
			{
				Marshal.Release(pDataObjectUnk);
				Marshal.Release(pDataObj);
			}
		}
#endif

		/// can't test for bitmap inserts - the zdata object changes them to bitmaps instead of filedrops 
		/// in any testing modes, so the test doesn't work as the production code would...

		[SnailTest]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInsertFileDrop()
		{
			var filename = BaseSourcePath + @"Enterprise\Architecture\Business\ZArchitecture.Business\AssemblyInfo.cs";
			using (var data = ZDataObject.FromData(DataFormats.FileDrop, new string[] { filename }))
			{
				RichEdit.InsertObject(data);
				Application.DoEvents();
				Application.DoEvents();
				Assert("Parent Form's paste event should have been called", PasteCalled);
				Assert("Parent Form's drag over event shouldn't have been called", !DragOverCalled);
				Assert("Parent Form's drag drop event shouldn't have been called", !DragDropCalled);
#if !WINZOR
				Assert("Rtf contents should change - note should be inserted to say filename added to edocs", RichEdit.Rtf.Contains(Path.GetFileNameWithoutExtension(filename)));
#else
				Assert("Rtf contents should change - note should be inserted to say filename added to edocs", RichEdit.SelectedHtml.Contains(Path.GetFileNameWithoutExtension(filename)));
#endif
			}
		}

		public void TestInsertText()
		{
#if !WINZOR
			var beforeRtf = RichEdit.Rtf;
#else
			var beforeRtf = RichEdit.SelectedHtml;
#endif
			RichEdit.InsertObject("stuff");
			Application.DoEvents();
			Application.DoEvents();
			Assert("Parent Form's paste event shouldn't have been called", !PasteCalled);
			Assert("Parent Form's drag over event shouldn't have been called", !DragOverCalled);
			Assert("Parent Form's drag drop event shouldn't have been called", !DragDropCalled);
#if !WINZOR
			Assert("Rtf should have changed due to insert of text", RichEdit.Rtf != beforeRtf);
#else
			Assert("Rtf should have changed due to insert of text", RichEdit.SelectedHtml != beforeRtf);
#endif
		}

		public void TestInsertRtfWithImage()
		{
			RichEdit.InsertObject(new DataObject(DataFormats.Rtf, RtfWithImage));
			Application.DoEvents();
			Application.DoEvents();
			Assert("Parent Form's paste event should have been called", PasteCalled);
			Assert("Parent Form's drag over event shouldn't have been called", !DragOverCalled);
			Assert("Parent Form's drag drop event shouldn't have been called", !DragDropCalled);

			PasteCalled = false;
			RichEdit.InsertObject(new DataObject(DataFormats.Rtf, @"{\rtf some text}"));
			Application.DoEvents();
			Application.DoEvents();
			Assert("Parent Form's paste event shouldn't have been called", !PasteCalled);
		}

		public void TestInsertBodgeyFile()
		{
#if !WINZOR
			var beforeRtf = RichEdit.Rtf;
#else
			var beforeRtf = RichEdit.Html;
#endif
			using (var data = ZDataObject.FromData(DataFormats.FileDrop, new string[] { Path.Combine(Temp.TempPath, "bodgeyfilename") }))
			{
				RichEdit.InsertObject(data);
				Application.DoEvents();
				Application.DoEvents();
#if !WINZOR
				Assert("Rtf should NOT have changed due to bodgey insert", RichEdit.Rtf == beforeRtf);
#else
				Assert("Rtf should NOT have changed due to bodgey insert", RichEdit.Html == beforeRtf);
#endif
				Assert("Parent Form's paste event should have been called", PasteCalled);
				Assert("Parent Form's drag over event shouldn't have been called", !DragOverCalled);
				Assert("Parent Form's drag drop event shouldn't have been called", !DragDropCalled);
			}
		}

		public void TestInsertMultipleFileObjects()
		{
			using (var tempFile = TempFile.New())
			using (var data = ZDataObject.FromData(DataFormats.FileDrop, new string[] { tempFile.Filename }))
			{
				RichEdit.InsertObject(data);
				Application.DoEvents();
				Assert("Parent Form's paste event should have been called", PasteCalled);
				Assert("Parent Form's drag over event shouldn't have been called", !DragOverCalled);
				Assert("Parent Form's drag drop event shouldn't have been called", !DragDropCalled);
#if !WINZOR
				Assert("Rtf contents should change - note should be inserted to say filename was added to edocs", RichEdit.Rtf.Contains(Path.GetFileNameWithoutExtension(tempFile.Filename)));
#else
				Assert("Rtf contents should change - note should be inserted to say filename was added to edocs", RichEdit.SelectedHtml.Contains(Path.GetFileNameWithoutExtension(tempFile.Filename)));
#endif
			}

			PasteCalled = false;
			DragDropCalled = false;
			DragOverCalled = false;
#if !WINZOR
			RichEdit.Rtf = "";
#else
			RichEdit.SelectedHtml = "";
#endif

			using (var tempFile = TempFile.New())
			using (var data = ZDataObject.FromData(DataFormats.FileDrop, new string[] { tempFile.Filename, tempFile.Filename }))
			{
				RichEdit.InsertObject(data);
				Application.DoEvents();
				Assert("Parent Form's paste event should have been called", PasteCalled);
				Assert("Parent Form's drag over event shouldn't have been called", !DragOverCalled);
				Assert("Parent Form's drag drop event shouldn't have been called", !DragDropCalled);
#if !WINZOR
				Assert("Rtf contents should change - note should be inserted to say filename was added to edocs", RichEdit.Rtf.Contains(Path.GetFileNameWithoutExtension(tempFile.Filename)));
#else
				Assert("Rtf contents should change - note should be inserted to say filename was added to edocs", RichEdit.SelectedHtml.Contains(Path.GetFileNameWithoutExtension(tempFile.Filename)));
#endif
				RichEdit.Dispose();
			}
		}

		public void TestNoInsertFileObjectsWhenReadOnlyOrDisabled()
		{
			using (var tempFile = TempFile.New())
			using (var data = ZDataObject.FromData(DataFormats.FileDrop, new string[] { tempFile.Filename }))
			{
				RichEdit.ReadOnly = true;
				RichEdit.Enabled = true;
				AssertNoInsertFileObjects(data);

				RichEdit.ReadOnly = true;
				RichEdit.Enabled = false;
				AssertNoInsertFileObjects(data);

				RichEdit.ReadOnly = false;
				RichEdit.Enabled = false;
				AssertNoInsertFileObjects(data);
			}
		}

		void AssertNoInsertFileObjects(ZDataObject data)
		{
#if !WINZOR
			var expectedRtf = RichEdit.Rtf;
#else
			var expectedRtf = RichEdit.Html;
#endif
			RichEdit.InsertObject(data);
			Application.DoEvents();
			CombineAssertions(() =>
			{
				AssertEquals("Parent Form's paste event should have been called", false, PasteCalled);
				AssertEquals("Parent Form's drag over event should have have been called", false, DragOverCalled);
				AssertEquals("Parent Form's drag drop event should have been called", false, DragDropCalled);
#if !WINZOR
				AssertEquals("Rtf contents should not change", expectedRtf, RichEdit.Rtf);
#else
				AssertEquals("Rtf contents should not change", expectedRtf, RichEdit.Html);
#endif
			});
		}

		public void TestNoBigassDataInserts()
		{
			RichEdit.MaxLength = 50;
#if !WINZOR
			var rtfBefore = RichEdit.Rtf;
#else
			var rtfBefore = RichEdit.Html;
#endif

			var bigData = "a big string that should exceed the maxlength of the rich text box which is currently set at 50 characters";
			using (var data = ZDataObject.FromData(bigData))
			{
				RichEdit.InsertObject(data);
				Assert("Parent Form's paste event should not be called yet", !PasteCalled);
				Assert("Parent Form's drag over event shouldn't have been called", !DragOverCalled);
				Assert("Parent Form's drag drop event shouldn't have been called", !DragDropCalled);
#if !WINZOR
				Assert("Text can be inserted", RichEdit.Rtf.Contains("a big string that should"));
				Assert("text can be inserted but will be truncated before the end of the string", !RichEdit.Rtf.Contains("characters"));
#else
				Assert("Text can be inserted", RichEdit.Html.Contains("a big string that should"));
				Assert("text can be inserted but will be truncated before the end of the string", !RichEdit.Html.Contains("characters"));
#endif
			}

			PasteCalled = false;
			DragDropCalled = false;
			DragOverCalled = false;

			using (var file = TempFile.New())
			{
				using (var writer = new StreamWriter(file.Filename))
				{
					writer.WriteLine("a big string that should exceed the maxlength of the rich text box which is currently set at 50 characters");
				}

				using (var data = ZDataObject.FromData(DataFormats.FileDrop, new string[] { file.Filename }))
				{
					RichEdit.InsertObject(data);
					Assert("Parent Form's paste event should have been called to handle the bitmap", PasteCalled);
					Assert("Parent Form's drag over event shouldn't have been called", !DragOverCalled);
					Assert("Parent Form's drag drop event shouldn't have been called", !DragDropCalled);
#if !WINZOR
					Assert("rtf contents should change - note added to say file was put on edocs tab", rtfBefore != RichEdit.Rtf);
#else
					Assert("rtf contents should change - note added to say file was put on edocs tab", rtfBefore != RichEdit.Html);
#endif
				}
			}
		}

		public void TestAllowPlainTextToBeInserted()
		{
			TestAllowPlainTextToBeInserted("This is some plain text$!!!=12\t3-\r\n", "This is some plain text");
			TestAllowPlainTextToBeInserted("MM 05-DEC-03 10:49: BOB HAS TH", "MM 05-DEC-03 10:49: BOB HAS TH");
		}

		public void TestAllowPlainTextToBeInserted(string testText, string expectedContainsPlainText)
		{
			var testTextAsRtf = ORtfTextUtil.TextToRtf(testText);

#if !WINZOR
			RichEdit.Rtf = testText;
			Assert("Text should be converted to RTF", RichEdit.Rtf.IndexOf(expectedContainsPlainText) != -1);
			Assert("Text should be converted to RTF", RichEdit.Rtf.IndexOf("rtf1") != -1);
#else
			RichEdit.Html = testText;
			Assert("Text should be converted to RTF", RichEdit.Html.IndexOf(expectedContainsPlainText) != -1);
			Assert("Text should be converted to RTF", RichEdit.Html.IndexOf("<p>") != -1);
#endif
		}

		public void TestIfEmptyRtfReturnEmptyString()
		{
#if !WINZOR
			RichEdit.Rtf = ORtfTextUtil.EmptyRtf;
#else
			RichEdit.Html = string.Empty;
#endif
			AssertEquals("Empty rtf should effectively be an empty string", "", RichEdit.RichEdit.Text);
		}

#if !WINZOR
		[DeveloperOnlyTest]
		public void TestInsertFilesSavedToEDocsMessage()
		{
			var action = new Action(() => SafeClipboard.SetData(DataFormats.Rtf, RtfWithImage));
			action.Invoke();
			ClipboardTestHelper.RetryIfCopyOrCutFailed<IDataObject>(action);

			RichEdit.Paste();
			Assert("Reference to picture document should be added to rtf text.", RichEdit.Rtf.Contains(") added to eDocs tab."));
		}
#endif

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInsertFileHyperlink()
		{
			var documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			var documentFactory = documentFactoryProvider.GetFactory(Factory);
			var documentFactoryForBo = (BusinessObjectFactory)documentFactory;
			var storageDocsFactory = documentFactory.GetFactory(1);
			var dummyOrgHeader = documentFactoryForBo.NewWithValidTestData(ObjectFactory.GetType<IOrgHeader>());
			dummyOrgHeader[OrgHeaderSchema.OH_Code] = "UnitTest";

			var storageMain = (BusinessObject)documentFactoryForBo.New<IStorageMain>();
			storageMain[StorageMainSchema.SM_Type] = Enterprise.Core.Constants.DocManagerCodes.Organisation;
			storageMain[StorageMainSchema.SM_ParentFK.Name] = dummyOrgHeader.PK;
			storageMain[StorageMainSchema.SM_DB.Name] = 1;

			var storageDocs = (BusinessObject)storageDocsFactory.New<IStorageDocs>();
			storageDocs[StorageDocsSchema.SC_SM.Name] = storageMain.PK;
			storageDocs[StorageDocsSchema.SC_DataType.Name] = "TIF";
			storageDocs[StorageDocsSchema.SC_DocType.Name] = "MBL";
			storageDocs[StorageDocsSchema.SC_ImageData.Name] = File.ReadAllBytes(Path.Combine(BaseSourcePath, "Enterprise", "Product", "Documents", "DocumentScanning", "DocumentScanning.Business.Test", "TestDocs", "Small.tif"));
			storageDocs[StorageDocsSchema.SC_Desc.Name] = "Testing Dummy";
			storageDocs[StorageDocsSchema.SC_IsPublished.Name] = true;

			documentFactory.Save();

			parentStorageDocsPK = ((ZGuid)storageMain[StorageMainSchema.SM_ParentFK.Name]).ToGuid();
			storageDocsPK = ((ZGuid)storageDocs[StorageDocsSchema.PK.Name]).ToGuid();

			using (var tempFile = TempFile.New())
			using (var data = ZDataObject.FromData(DataFormats.FileDrop, new string[] { tempFile.Filename }))
			{
				RichEdit.InsertObject(data);
				Application.DoEvents();

#if !WINZOR
				Assert("Rtf contents should change - note should be inserted to say filename was added to edocs", RichEdit.Rtf.Contains(ChangeFileExtensionForTest(tempFile.Filename)));
				Assert("Hyperlink should be added to rtf text.", RichEdit.Rtf.Contains("HYPERLINK \"edient:"));
				Assert("Hyperlink should contains storage PKs.", RichEdit.Rtf.Contains(string.Format(CultureInfo.InvariantCulture, "BusinessEntityPK={0}&StorageDocPK={1}", parentStorageDocsPK, storageDocsPK)));
#else
				Assert("Rtf contents should change - note should be inserted to say filename was added to edocs", RichEdit.Html.Contains(ChangeFileExtensionForTest(tempFile.Filename)));
				Assert("Hyperlink should be added to rtf text.", RichEdit.Html.Contains("HYPERLINK \"edient:"));
				Assert("Hyperlink should contains storage PKs.", RichEdit.Html.Contains(string.Format(CultureInfo.InvariantCulture, "BusinessEntityPK={0}&StorageDocPK={1}", parentStorageDocsPK, storageDocsPK)));
#endif

				using (ShowStorageDocUrlHandler.Instance.SetupStorageDocUrlHandlerForTest())
				using (var form = new KForm())
				{
					form.Show();
#if !WINZOR
					var hyperlink = RichEdit.Rtf.Substring(RichEdit.Rtf.IndexOf("edient:"));
#else
					var hyperlink = RichEdit.Html.Substring(RichEdit.Html.IndexOf("edient:"));
#endif
					hyperlink = hyperlink.Substring(0, hyperlink.IndexOf("\""));
					EnterpriseUrlHandlerService.Instance.ExecuteUrl(hyperlink, false);

					AssertNotNull("GraphicalDisplayForm was not displayed when hyperlink clicked", OpenGraphicalDisplayForm);
					OpenGraphicalDisplayForm.Dispose();
				}
			}
		}

		Form OpenGraphicalDisplayForm
		{
			get
			{
				foreach (Form form in Application.OpenForms)
				{
					if (form.Name == "GraphicalDisplayForm")
					{
						return form;
					}
				}
				return null;
			}
		}

		#region TestRetryLaterWhenRetryLaterHResultEncountered

		public void TestRetryLaterWhenRetryLaterHResultEncountered()
		{
			using (var richEdit = new ZRichTextBox())
			using (var inserter = new TestRichEditDataInserter_ForRetryLaterHResult(richEdit))
			{
				richEdit.CreateControl();
				inserter.ThrowRetryLaterHResultOnNextImportDataObject = true;

#if !WINZOR
				var rtfBefore = richEdit.Rtf;
#else
				var rtfBefore = richEdit.SelectedHtml;
#endif

				inserter.InsertObject("some text to insert");
#if !WINZOR
				var rtfAfter = richEdit.Rtf;
#else
				var rtfAfter = richEdit.SelectedHtml;
#endif

				Assert("Rtf should have changed due to a successful insert, even though the 'try later' exception was thrown at first", rtfBefore != rtfAfter);
			}
		}

		[ExpectNoExceptions]
		public void TestRetryLaterWhenRetryLaterHResultEncountered_DoesntFailEvenIfThrownSecondTime()
		{
			using (var richEdit = new ZRichTextBox())
			using (var inserter = new TestRichEditDataInserter_ForRetryLaterHResult(richEdit))
			{
				richEdit.CreateControl();
				inserter.ThrowRetryLaterHResultOnImportDataObjectAlways = true;
				inserter.InsertObject("test");
			}
		}

		class TestRichEditDataInserter_ForRetryLaterHResult : ZRichEditDataInserter
		{
			public TestRichEditDataInserter_ForRetryLaterHResult(ZRichTextBox zRichTextBox)
				: base(zRichTextBox)
			{
			}

			public bool ThrowRetryLaterHResultOnImportDataObjectAlways;
			public bool ThrowRetryLaterHResultOnNextImportDataObject;

#if !WINZOR
			protected override void ImportFormattedTextCore(IntPtr pDataObject)
			{
				if (ThrowRetryLaterHResultOnNextImportDataObject || ThrowRetryLaterHResultOnImportDataObjectAlways)
				{
					ThrowRetryLaterHResultOnNextImportDataObject = false;
					throw new ExternalException("Thank you, come again", HResult.RPC_E_SERVERCALL_RETRYLATER);
				}
				base.ImportFormattedTextCore(pDataObject);
			}
#endif
		}

		#endregion

		#region Implementation

		ZForm Form;
		TestRichTextBox RichEdit;
		bool PasteCalled;
		bool DragDropCalled;
		bool DragOverCalled;
		Guid parentStorageDocsPK = Guid.Empty;
		Guid storageDocsPK = Guid.Empty;

		protected override void SetUp()
		{
			// this thing uses loads of memory
			GC.Collect();
			System.Threading.Thread.Sleep(500);

			RichEdit = new TestRichTextBox();
			RichEdit.CreateControl();

			Form = new ZForm();
			Form.PlugIns.Add(Modules.ControllerIDs.eDocsPlugIn);
			PasteCalled = false;
			DragDropCalled = false;
			DragOverCalled = false;

			Form.DataObjectPasted +=
				(object sender, DataObjectPastedEventArgs e) =>
				{
					PasteCalled = true;
					if (e.DataToPaste.GetDataPresent(DataFormats.FileDrop))
					{
						var fileNames = (string[])e.DataToPaste.GetData(DataFormats.FileDrop);
						var no = 0;
						foreach (var fileName in fileNames)
						{
							e.AddPastedFile(fileName, ChangeFileExtensionForTest(fileName), "some description " + (++no).ToString(), parentStorageDocsPK, storageDocsPK, true);
						}
					}
				};
			Form.DragDrop += delegate
			{ DragDropCalled = true; };
			Form.DragOver += delegate
			{ DragOverCalled = true; };

			Form.Controls.Add(RichEdit);
			Application.DoEvents();
		}

		string ChangeFileExtensionForTest(string fileName)
		{
			// Changing the file extension to test if the file name is right when it's added to a eDocs, it changes some image's extension to 'tif'
			var result = fileName;
			if (Path.HasExtension(result))
			{
				result = Path.ChangeExtension(result, ".tif");
				result = Path.GetFileName(result);
			}
			return result;
		}

		protected override void TearDown()
		{
			RichEdit.Dispose();
			Form.Dispose();
			base.TearDown();
		}

		#endregion
	}
}
