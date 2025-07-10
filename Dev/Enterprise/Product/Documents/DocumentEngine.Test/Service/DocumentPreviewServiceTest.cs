using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Service.Testing
{
	sealed class DocumentPreviewServiceTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetDocumentCommand()
		{
			var businesssObject = Factory.New<ICommonShipment>() as BusinessObject;
			var documentCommand = BuildDocumentCommand();
			Factory.Save();

			var previewService = new DocumentPreviewService();
			var result = previewService.GetDocumentCommand(documentCommand.PK.ToGuid(), businesssObject.TablePrefix, businesssObject.PK.ToGuid());

			AssertNotNull("Document Command", result);
			AssertNotNull("Document Command Parent", result.Parent);
		}

		public void TestGetDocumentCommand_InvalidDocumentPK()
		{
			var businesssObject = Factory.New<ICommonShipment>() as BusinessObject;
			Factory.Save();

			var previewService = new DocumentPreviewService();
			var result = previewService.GetDocumentCommand(Guid.NewGuid(), businesssObject.TablePrefix, businesssObject.PK.ToGuid());

			AssertNull("Document Command", result);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetDocumentCommand_InvalidBusinessObjectPK()
		{
			var documentCommand = BuildDocumentCommand();
			Factory.Save();

			var previewService = new DocumentPreviewService();
			AssertExceptionThrown<ArgumentException>(
				"Should throw when parent is not found",
				"Invalid document command parent.\r\nParameter name: Parent",
				() => previewService.GetDocumentCommand(documentCommand.PK.ToGuid(), "XX", Guid.NewGuid()));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetDocumentCommand_InvalidBusinessObjectType()
		{
			var businesssObject = Factory.New<DummyBusinessObject>() as BusinessObject;
			var documentCommand = BuildDocumentCommand();
			Factory.Save();

			var previewService = new DocumentPreviewService();
			AssertExceptionThrown<ArgumentException>(
				"Should throw when parent does not implement IDocumentSupportable",
				"Invalid document command parent.\r\nParameter name: Parent",
				() => previewService.GetDocumentCommand(documentCommand.PK.ToGuid(), businesssObject.TablePrefix, businesssObject.PK.ToGuid()));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWriteDocumentPreview()
		{
			var businesssObject = Factory.New<ICommonShipment>() as BusinessObject;
			var documentCommand = BuildDocumentCommand();
			Factory.Save();

			var previewService = new DocumentPreviewService();
			var dc = previewService.GetDocumentCommand(documentCommand.PK.ToGuid(), businesssObject.TablePrefix, businesssObject.PK.ToGuid());

			using (var previewStream = new MemoryStream())
			{
				previewService.WriteDocumentPreview(dc, previewStream);

				AssertGreaterThan("Document Preview should not be empty", previewStream.Length, 0);
				Assert("Document Preview should be a pdf", ImageToPDFConverter.IsPDF(previewStream.ToArray()));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestWriteDocumentPreview_FormBuilderDocument()
		{
			var shipment = (BusinessObject)Factory.New<ICommonShipment>();
			var documentCommand = BuildDocumentCommand(Core.Constants.StmMenuItemTypes.Forms);

			Factory.Save();

			var previewService = new DocumentPreviewService();
			var command = previewService.GetDocumentCommand(documentCommand.PK.ToGuid(), shipment.TablePrefix, shipment.PK.ToGuid());

			var mock = new Mock<IDocumentPDFWriter>();
			mock.Setup(m => m.WriteToStream(
				It.Is<BusinessObject>(biz => biz.PK == shipment.PK),
				It.Is<IStmMenuItem>(mi => mi.PK == command.PK),
				It.IsAny<Stream>()))
				.Returns(true);

			using (ObjectFactory.Substitute(mock.Object))
			using (var previewStream = new MemoryStream())
			{
				previewService.WriteDocumentPreview(command, previewStream);
			}

			mock.VerifyAll();
		}

		public void TestWriteDocumentPreview_NullDocumentCommand()
		{
			var previewService = new DocumentPreviewService();
			using (var previewStream = new MemoryStream())
			{
				AssertExceptionThrown<ArgumentNullException>(
					"Should throw when document command is null",
					"Value cannot be null.\r\nParameter name: documentCommand",
					() => previewService.WriteDocumentPreview(null, previewStream));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWriteDocumentPreview_NullParent()
		{
			var documentCommand = BuildDocumentCommand();
			var previewService = new DocumentPreviewService();
			using (var previewStream = new MemoryStream())
			{
				AssertExceptionThrown<ArgumentNullException>(
					"Should throw when parent is not found",
					"Value cannot be null.\r\nParameter name: Parent",
					() => previewService.WriteDocumentPreview(documentCommand, previewStream));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWriteDocumentPreview_NullOutputStream()
		{
			var businesssObject = Factory.New<ICommonShipment>() as BusinessObject;
			var documentCommand = BuildDocumentCommand();
			Factory.Save();

			var previewService = new DocumentPreviewService();
			var dc = previewService.GetDocumentCommand(documentCommand.PK.ToGuid(), businesssObject.TablePrefix, businesssObject.PK.ToGuid());

			AssertExceptionThrown<ArgumentNullException>(
				"Should throw when output stream is null",
				"Value cannot be null.\r\nParameter name: outputStream",
				() => previewService.WriteDocumentPreview(dc, null));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWriteDocumentPreview_CustomizedDocumentPrintSet()
		{
			var businesssObject = Factory.New<DummyBODocSupportableWithCustomizedDocumentPrintSet>();
			var documentCommand = BuildDocumentCommand();
			documentCommand.Parent = businesssObject;

			var previewService = new DocumentPreviewService();
			using (var previewStream = new MemoryStream())
			{
				previewService.WriteDocumentPreview(documentCommand, previewStream);

				var documentSupporter = ((DummyBODocSupportableDocumentSupporterWithCustomizedDocumentPrintSet)businesssObject.DocumentSupporter);
				Assert("Should use customized document printset", documentSupporter.documentPrintSetWithStreamingWasCalled);
			}
		}

		public void TestMergeDocumentPacks_PacksIncludeAnEDocAndAnDocument()
		{
			var documentSupportable = Factory.New<DummyBODocSupportable>();

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = documentSupportable;
			documentCommand.SU_MenuName = "Test";

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			var docType = Factory.New<RefDocType>();
			docType.RT_DocType = "TST";
			docType.RT_ReferenceType = "ALL";
			documentCommand.AddEDoc(docType);

			using (var bitmap = new Bitmap(1, 1))
			{
				var stream = new CargoWise.IO.Shim.SubStreamableStream(new MemoryStream());
				bitmap.Save(stream, ImageFormat.Bmp);
				documentSupportable.DocManagerInfo.AddFileOrDocument(stream, "Test", docType.RT_DocType);
			}

			var previewService = new DocumentPreviewService();
			using (var previewStream = new MemoryStream())
			{
				previewService.WriteDocumentPreview(documentCommand, previewStream);
				AssertGreaterThan("Document Preview should not be empty", previewStream.Length, 0);
			}
		}

		public void TestWriteDocumentPreview_FlexCelException()
		{
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
			documentCommand.SU_IsPublished = true;
			documentCommand.SU_IsSystemDefined = true;
			documentCommand.SU_MenuName = "Pub System Shipment Document";
			documentCommand.SU_MenuIndex = 1;
			documentCommand.SU_MenuPath = "";
			documentCommand.SU_MenuShortcut = "CtrlF1";

			var template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = nameof(Core.Constants.DataContext.Shipment);
			template.SO_IsSystemDefined = true;
			template.SO_Name = "System Document Elements";
			template.SO_Template = Array.Empty<byte>();

			var templatePivot = Factory.New<StmMenuTemplatePivotBase>();
			templatePivot.SI_SO = template.PK;
			templatePivot.SI_SU = documentCommand.PK;
			templatePivot.SI_Index = 1;
			templatePivot.SI_DocumentTitle = "Pub System Shipment Document 1";
			var documentConfig = templatePivot.DocConfigs.AddNew();
			documentConfig.ConfigItems.AddNew();

			var businesssObject = Factory.New<ICommonShipment>() as BusinessObject;

			Factory.Save();

			var previewService = new DocumentPreviewService();
			var dc = previewService.GetDocumentCommand(documentCommand.PK.ToGuid(), businesssObject.TablePrefix, businesssObject.PK.ToGuid());

			using (var previewStream = new MemoryStream())
			{
				AssertExceptionThrown<DocumentPreviewException>(
					"Should re-throw when FlexCelXlsAdapterException",
					"The selected file could not be loaded. It may be damaged or an unsupported format. Please check the file and try again.",
					() => previewService.WriteDocumentPreview(dc, previewStream));
			}
		}

		DocumentCommand BuildDocumentCommand(string menuType = Core.Constants.StmMenuItemTypes.Documents)
		{
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
			documentCommand.SU_IsPublished = true;
			documentCommand.SU_IsSystemDefined = true;
			documentCommand.SU_MenuName = "Pub System Shipment Document";
			documentCommand.SU_MenuIndex = 1;
			documentCommand.SU_MenuPath = "";
			documentCommand.SU_MenuShortcut = "CtrlF1";
			documentCommand.SU_MenuType = menuType;

			var template1 = Factory.New<StmTemplateBase>();
			template1.SO_DataContext = nameof(Core.Constants.DataContext.Shipment);
			template1.SO_IsSystemDefined = true;
			template1.SO_Name = "System Shipment Template 1";
			template1.SO_Template = new ExcelTemplateForUnitTesting("UDF with tabs.xls", TestFilesSubFolder.ReportTestFiles).GetAsByteArray();

			var templatePivot1 = Factory.New<StmMenuTemplatePivot>();
			templatePivot1.SI_SO = template1.PK;
			templatePivot1.SI_SU = documentCommand.PK;
			templatePivot1.SI_Index = 1;
			templatePivot1.SI_DocumentTitle = "Pub System Shipment Document 1";

			var template2 = Factory.New<StmTemplateBase>();
			template2.SO_DataContext = nameof(Core.Constants.DataContext.Shipment);
			template2.SO_IsSystemDefined = true;
			template2.SO_Name = "System Shipment Template 2";
			template2.SO_Template = new ExcelTemplateForUnitTesting("UDF with tabs2.xls", TestFilesSubFolder.ReportTestFiles).GetAsByteArray();

			var templatePivot2 = Factory.New<StmMenuTemplatePivot>();
			templatePivot2.SI_SO = template2.PK;
			templatePivot2.SI_SU = documentCommand.PK;
			templatePivot2.SI_Index = 2;
			templatePivot2.SI_DocumentTitle = "Pub System Shipment Document 2";

			return documentCommand;
		}
	}
}
