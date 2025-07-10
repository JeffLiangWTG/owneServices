using System.IO;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class ProductXmlDataTransferExporterTest : TestCaseWithFactory
	{
		[TestDate(2007, 1, 15, 12, 0, 0)]
		public void TestExportToFile()
		{
			var product = Factory.New<OrgSupplierPart>();
			var expectedFileName = Path.Combine(Temp.TempPath, product.OP_PartNum + "_" + ZDateTime.Now.ToString("yyyyMMddhhmmss") + ".xml");

			var mockGui = new Mock<IXmlDataTransferExporterGUI>();
			mockGui.Setup(g => g.ShowSaveFileDialog(It.IsAny<string>(), It.IsAny<string>())).Returns(ZDialogResult.OK);
			mockGui.Setup(g => g.OpenFile()).Returns(File.OpenWrite(expectedFileName));
			ObjectFactory.Substitute<IXmlDataTransferExporterGUI>(mockGui.Object);

			var adapter = new ProductValueObjectDataAdapter();
			var director = new TestProductXmlDataTransferExporter(adapter);
			try
			{
				director.PromptUserAndExport(new BusinessObject[] { product });
				Assert(File.Exists(expectedFileName));
			}
			finally
			{
				DeleteIfExists(expectedFileName);
			}
		}

		public void TestAdapter()
		{
			var adapter = new ProductValueObjectDataAdapter();
			var director = new TestProductXmlDataTransferExporter(adapter);
			AssertEquals("Adapter", typeof(ProductValueObjectDataAdapter), director.Adapter.GetType());
		}

		class TestProductXmlDataTransferExporter : ProductXmlDataTransferExporter
		{
			public TestProductXmlDataTransferExporter(ProductValueObjectDataAdapter adapter)
				: base(adapter)
			{
			}

			public new IValueObjectDataAdapter Adapter
			{
				get { return base.Adapter; }
			}
		}
	}
}
