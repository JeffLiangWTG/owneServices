using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using DocumentFactory = Enterprise.DocumentScanning.Business.DocumentFactory;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	sealed class DocumentsZGridNoSetupTest : TestCaseWithDocumentFactory
	{
#if !WINZOR
		readonly DocumentFactory masterFactory = GetDocumentFactory();

		static DocumentFactory GetDocumentFactory(DbConnection connection = null)
			=> new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory(connection ?? Db.Connection));

		public void TestConcurrencyErrorsWhenMerging()
		{
			masterFactory.NameForDebugging = nameof(masterFactory);
			masterFactory.RefreshEnabled = false;

			var dummyData = CreateDummyImage();
			var d1 = CreateDummyDoc(dummyData);
			var d2 = CreateDummyDoc(dummyData);

			masterFactory.Save();

			using (var otherConnection = Db.NewExtraConnectionToMainDb())
			{
				var otherFactory = GetDocumentFactory(otherConnection);
				otherFactory.NameForDebugging = nameof(otherFactory);
				otherFactory.RefreshEnabled = false;

				using (var form = new ZChildForm())
				using (var grid = new DocumentsZGridForTesting())
				{
					grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(StorageDocsSchema.SC_Desc.Name, 69));

					form.Controls.Add(grid);
					form.Show();

					var collection = new StorageDocsCollection(otherFactory);
					collection.Load();

					AssertEquals("Collection should contain the two bizos from before", 2, collection.Count);
					grid.SetDataBinding(collection, string.Empty);
					Application.DoEvents();

					grid.UnSelectAll();
					grid.Select(0);
					Application.DoEvents();

					masterFactory.Load<StorageDocs>(grid.SelectedElements.Single().PK).Delete();
					masterFactory.Save();

					grid.DoDragDrop();

					AssertEquals("Should have told the user something happened", "While you were working, another user has modified this document. Please reload this form.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestConcurrencyErrorsWhenDragDrop_StorageFile()
		{
			var doc = StorageFile.NewWithParent_DEBUG(masterFactory);
			doc.SC_ImageData = new ZBlob(File.ReadAllBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\", @"Sample.PDF")));
			doc.SC_DataType = "PDF";
			masterFactory.Save();

			var otherFactory = GetDocumentFactory();
			otherFactory.RefreshEnabled = false;
			var collection = new StorageFileCollection(otherFactory);
			collection.Load();
			AssertEquals("Collection should contain the one bizos from before", 1, collection.Count);

			using var grid = new DocumentsZGridForTesting();
			using var form = new ZChildForm();
			form.Controls.Add(grid);

			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(StorageDocsSchema.SC_Desc.Name, 69));
			grid.SetDataBinding(collection, string.Empty);

			// Delete the edoc from one factory and drag on another one
			doc.Delete();
			masterFactory.Save();

			grid.UnSelectAll();
			grid.Select(0);
			grid.DoDragDrop();
			AssertEquals("Exception should be handled with the designated message.", "While you were working, another user has modified this document. Please reload this form.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		static byte[] CreateDummyImage()
		{
			using (var bmp = new Bitmap(500, 500))
			using (var ms = new MemoryStream())
			{
				bmp.Save(ms, ImageFormat.Bmp);
				return ms.ToArray();
			}
		}

		StorageDocs CreateDummyDoc(byte[] imageData)
		{
			var unallocatedParent = masterFactory.LoadTop1<StorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, null)) ??
				masterFactory.NewWithValidTestData<StorageMain>();

			var doc = masterFactory.New<StorageDocs>();
			doc.SC_ImageData = imageData;
			doc.SC_SM = unallocatedParent.PK;

			return doc;
		}
#endif
	}
}
