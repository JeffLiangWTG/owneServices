using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentEngine.DocumentDelivery;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DeliveryMethods.Testing
{
	sealed class ExcelPreviewTest : TestCaseWithFactory
	{
		[GuiTest]
		public void TestConstructor()
		{
			var mockIDeliverCapableForm = new Mock<IDeliverCapableForm>();
			var preview = new ExcelPreview(mockIDeliverCapableForm.Object);
			AssertEquals("preview.Parent", mockIDeliverCapableForm.Object, preview.Parent);
		}

		public void TestDocumentPreviewAndModifyInExcel()
		{
			var mockIPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
			using (new PrintTaskUIProviderFactory.OverriderForTesting(mockIPrintTaskUIProvider.Object))
			{
				var deliveryInfo = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
				var emptyAndValidTemplateStream = resourceRetriever.Value.GetStream("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls");
				deliveryInfo.SetFileContents(emptyAndValidTemplateStream, "xls");
				var delvInstruction = new DeliveryInstructions();
				delvInstruction.AllowModifyAndPreviewInExcel = true;
				deliveryInfo.Instructions = delvInstruction;

				var deliveryMethod = new ExcelPreviewForTesting(true);
				mockIPrintTaskUIProvider.Setup(m => m.ShowPreview(null, null, null))
					.Callback(delegate
					{ });

				deliveryMethod.AddFile(deliveryInfo);
				deliveryMethod.Deliver();

				AssertEquals("Preview in excel should have been called for XLS file", true, deliveryMethod.PreviewInExcelCalled);
			}
		}

		public void TestDeliverWithTIFAndXLS()
		{
			var tIFInfo = new DeliveryInfo(DeliveryInfo.DeliveryFormats.TIFF);
			tIFInfo.SetFileContents(TIFPage, "tif");
			var xLSInfo = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			xLSInfo.SetFileContents(EmptyAndValidTemplateXls, "xls");

			var method = new ExcelPreviewForTesting();
			method.AddFile(tIFInfo);
			method.AddFile(xLSInfo);
			method.Deliver();

			AssertEquals("Deliver to excel should have been called for XLS file", true, method.DeliverToExcelCalled);
		}

		public void TestDeliverWithTIFOnly()
		{
			var tIFInfo = new DeliveryInfo(DeliveryInfo.DeliveryFormats.TIFF);
			tIFInfo.SetFileContents(TIFPage, "tif");

			var method = new ExcelPreviewForTesting();
			method.AddFile(tIFInfo);
			method.Deliver();

			AssertEquals("Deliver to excel should NOT have been called for TIF file", false, method.DeliverToExcelCalled);
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		Stream EmptyAndValidTemplateXls => resourceRetriever.Value.GetStream("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls");

		Stream TIFPage => resourceRetriever.Value.GetStream("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.TIFPage.tif");
	}
}
