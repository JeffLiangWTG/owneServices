using System.Windows.Forms;
using CargoWise.IO;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	[TestedType(typeof(ZAllocateDocumentsForm))]
	sealed class ZAllocateDocumentsFormBasherWithAllTypesTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			SetupSavedTestData();
			return new ZAllocateDocumentsForm(new AllocateDocumentsManager(new DocumentFactoryProvider().GetFactory(Factory)));
		}

		void SetupSavedTestData()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(typeof(StorageDocsBaseTest).Assembly))
			{
				var testImage = resourceRetriever.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif");
				DocumentFactory masterFactory = new DocumentFactoryProvider().GetFactory(Factory);

				foreach (var pair in AssemblyDataLookup.AllAssemblyData)
				{
					IAssemblyData element = pair.Value;

					if (element.IsAllowedForUnallocatedeDocs)
					{
						StorageDocsUnallocated document = masterFactory.New<StorageDocsUnallocated>();
						document.SM_Type = element.ReferenceType;
						document.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
						document.SC_Desc = "Hello";
						document.SC_ImageData = testImage;
					}
				}

				masterFactory.Save();
			}
		}
	}
}
