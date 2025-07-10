using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.MFI.AutoeDocAllocation.Testing
{
	public class DocumentAttacherTest : TestCaseWithFactory
	{
		public void TestGetDocumentReference()
		{
			AssertEquals("Reference value expected", "C000012345", TestDocAttacher.GetDocumentRef("C.C000012345.pdf"));
			AssertEquals("Reference value expected", "MFIAKL", TestDocAttacher.GetDocumentRef("Org.MFIAKL.1.doc"));
			AssertEquals("Reference value expected", "JAYSCH.Order591786", TestDocAttacher.GetDocumentRef("ORD.JAYSCH.Order591786.20060805.xls"));
			AssertEquals("Reference value expected", "INBU3669004", TestDocAttacher.GetDocumentRef("AGI.INBU3669004.CHX15459.pdf"));
			AssertEquals("Reference value expected", "HKGSYDH08706H01", TestDocAttacher.GetDocumentRef("TLX.HKGSYDH08706H01.tif"));
		}

		public void TestFileDoesNotAttachWhenRefNotFound()
		{
			string fileName = "BL.CSCLON0301001.freighted.txt";
			var testFile = new FileInfo(fileName);
			AssertEquals("Expecting document not to attach as object reference will not be found", false, TestDocAttacher.AttachFile(testFile));
			using (EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string testFilePath = resourceRetriever.SaveResourceToFile(fileName);
				var testFileInfo = new FileInfo(testFilePath);
				var testShipment = Factory.New<ForwardingShipment>();
				testShipment.JS_HouseBill = "CSCLON0301001";
				Factory.Save();
				AssertEquals("Text Document should now attach to Shipment via HAWB ref", true, TestDocAttacher.AttachFile(testFileInfo));
				var parent = MasterFactory.GetStorageMainForPK(testShipment.PK);
				AssertEquals("eDocs count should be 1", 1, parent.eDocs.Count);
				AssertEquals("File name should be the same as what was added", fileName, parent.eDocs[0].SC_FileNameWithExtension);
				AssertEquals("DocType should be set", "USA", parent.eDocs[0].SC_DocType);
			}
		}

		public void TestAttachFileFromDeclarationHawbRef()
		{
			string fileName = "BL.CSCLON0301001.freighted.txt";
			using (EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string testFilePath = resourceRetriever.SaveResourceToFile(fileName);
				var testFileInfo = new FileInfo(testFilePath);
				var testDec = BaseJobDeclaration.New(Factory);
				testDec.JE_HouseBill = "CSCLON0301001";
				Factory.Save();
				AssertEquals("Text Document should now attach to Shipment via HAWB ref", true, TestDocAttacher.AttachFile(testFileInfo));
				var parent = MasterFactory.GetStorageMainForPK(testDec.PK);
				AssertEquals("eDocs count should be 1", 1, parent.eDocs.Count);
				AssertEquals("File name should be the same as what was added", fileName, parent.eDocs[0].SC_FileNameWithExtension);
				AssertEquals("DocType should be set", "USA", parent.eDocs[0].SC_DocType);
			}
		}

		public void TestAttachFileFromOrganisationRef()
		{
			string fileName = "ORG.MFITEST.pdf";
			using (EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string testFilePath = resourceRetriever.SaveResourceToFile(fileName);
				var testFileInfo = new FileInfo(testFilePath);
				var testOrg = OrgHeader.New(Factory);
				testOrg.OH_RL_NKClosestPort = "AUSYD";
				testOrg.OH_Code = "MFITEST";
				Factory.Save();
				AssertEquals("PDF Document should attach to organisation", true, TestDocAttacher.AttachFile(testFileInfo));
				var parent = MasterFactory.GetStorageMainForPK(testOrg.PK);
				AssertEquals("eDocs count should be 1", 1, parent.eDocs.Count);
				AssertEquals("File name should be the same as what was added", fileName, parent.eDocs[0].SC_FileNameWithExtension);
				AssertEquals("DocType should be set", "ORG", parent.eDocs[0].SC_DocType);
			}
		}

		public void TestAttachDocFromMawbRef()
		{
			string fileName = "OBL.CSCL0495883.doc";
			using (EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string testFilePath = resourceRetriever.SaveResourceToFile(fileName);
				var testFileInfo = new FileInfo(testFilePath);
				var testConsol = CommonConsol.New(Factory);
				testConsol.JK_UniqueConsignRef = "C00001000";
				testConsol.JK_MasterBillNum = "CSCL0495883";
				Factory.Save();
				AssertEquals("Document should attach to Consol", true, TestDocAttacher.AttachFile(testFileInfo));
				var parent = MasterFactory.GetStorageMainForPK(testConsol.PK);
				AssertEquals("eDocs count should be 1", 1, parent.eDocs.Count);
				AssertEquals("File name should be the same as what was added", fileName, parent.eDocs[0].SC_FileNameWithExtension);
				AssertEquals("DocType should be set", "OBL", parent.eDocs[0].SC_DocType);
			}
		}

		public void TestAttachDocFromDeclarationMawbRef()
		{
			string fileName = "OBL.CSCL0495883.doc";
			using (EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string testFilePath = resourceRetriever.SaveResourceToFile(fileName);
				var testFileInfo = new FileInfo(testFilePath);
				var testDec = BaseJobDeclaration.New(Factory);
				testDec.JE_DeclarationReference = "B00001000";
				testDec.JE_MasterBill = "CSCL0495883";
				Factory.Save();
				AssertEquals("Document should attach to Declaration", true, TestDocAttacher.AttachFile(testFileInfo));
				var tarent = MasterFactory.GetStorageMainForPK(testDec.PK);
				AssertEquals("eDocs count should be 1", 1, tarent.eDocs.Count);
				AssertEquals("File name should be the same as what was added", fileName, tarent.eDocs[0].SC_FileNameWithExtension);
				AssertEquals("DocType should be set", "OBL", tarent.eDocs[0].SC_DocType);
			}
		}

		public void TestCOMOBLDocAttachesToDeclaration()
		{
			string fileName = "COMOBL.JJ0938JMND30.doc";
			using (EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string testFilePath = resourceRetriever.SaveResourceToFile(fileName);
				var testFileInfo = new FileInfo(testFilePath);
				var testDec = BaseJobDeclaration.New(Factory);
				testDec.JE_DeclarationReference = "B00001000";
				testDec.JE_MasterBill = "JJ0938JMND30";
				Factory.Save();
				AssertEquals("Document should attach to Declaration", true, TestDocAttacher.AttachFile(testFileInfo));
				var parent = MasterFactory.GetStorageMainForPK(testDec.PK);
				AssertEquals("eDocs count should be 1", 1, parent.eDocs.Count);
				AssertEquals("File name should be the same as what was added", fileName, parent.eDocs[0].SC_FileNameWithExtension);
				AssertEquals("DocType should be set", "COM", parent.eDocs[0].SC_DocType);
			}
		}

		public void TestConNoteAttachesToShipment()
		{
			string fileName = "TSP.TSTConNote.POD.msg";
			using (EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string testFilePath = resourceRetriever.SaveResourceToFile(fileName);
				var testFileInfo = new FileInfo(testFilePath);
				var testShipment = Factory.New<ForwardingShipment>();
				testShipment.DocsAndCartage.JP_CustomAttrib1 = "TSTConNote";
				Factory.Save();
				AssertEquals("email msg should now attach to Shipment via Con Note ref", true, TestDocAttacher.AttachFile(testFileInfo));
				var parent = MasterFactory.GetStorageMainForPK(testShipment.PK);
				AssertEquals("eDocs count should be 1", 1, parent.eDocs.Count);
				AssertEquals("File name should be the same as what was added", fileName, parent.eDocs[0].SC_FileNameWithExtension);
				AssertEquals("DocType should be set correctly", "POD", parent.eDocs[0].SC_DocType);
			}
		}

		public void TestConNoteAttachesToDeclaration()
		{
			string fileName = "TSP.TSTConNote.POD.msg";
			using (EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string testFilePath = resourceRetriever.SaveResourceToFile(fileName);
				var testFileInfo = new FileInfo(testFilePath);
				var testDec = BaseJobDeclaration.New(Factory);
				testDec.JE_DeclarationReference = "B00001000";
				testDec.DocsAndCartage.JP_CustomAttrib1 = "TSTConNote";
				Factory.Save();
				AssertEquals("Document should attach to Declaration", true, TestDocAttacher.AttachFile(testFileInfo));
				var parent = MasterFactory.GetStorageMainForPK(testDec.PK);
				AssertEquals("eDocs count should be 1", 1, parent.eDocs.Count);
				AssertEquals("File name should be the same as what was added", fileName, parent.eDocs[0].SC_FileNameWithExtension);
				AssertEquals("DocType should be set correctly", "POD", parent.eDocs[0].SC_DocType);
			}
		}

		public void TestStripDateTimeFromRefWhenFileAttachedAfterHeld()
		{
			string fileName = "OBL.CSCL0495883.doc_20061101060024";
			using (EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string testFilePath = resourceRetriever.SaveResourceToFile(fileName);
				var testFileInfo = new FileInfo(testFilePath);
				var testDec = BaseJobDeclaration.New(Factory);
				testDec.JE_DeclarationReference = "B00001000";
				testDec.JE_MasterBill = "CSCL0495883";
				Factory.Save();
				AssertEquals("Document should attach to Declaration", true, TestDocAttacher.AttachFile(testFileInfo));
				var parent = MasterFactory.GetStorageMainForPK(testDec.PK);
				AssertEquals("eDocs count should be 1", 1, parent.eDocs.Count);
				AssertEquals("File name should be stripped of the Date/Time component", "OBL.CSCL0495883.doc", parent.eDocs[0].SC_FileNameWithExtension);
			}
		}

		public void TestAttachDocToCorrectOrder()
		{
			string fileName = "ORD.MFITEST.A18374XG.May06.txt";
			using (EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string testFilePath = resourceRetriever.SaveResourceToFile(fileName);
				var testFileInfo = new FileInfo(testFilePath);
				var testOrder3 = Order.New(Factory);
				testOrder3.JD_OrderDate = ZDate.Today;
				testOrder3.JD_OrderNumber = "A18374XG";
				testOrder3.BuyerPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
				var buyerOrg = Factory.NewWithValidTestData<OrgHeader>();
				buyerOrg.OH_RL_NKClosestPort = "AUSYD";
				buyerOrg.OH_Code = "MFITEST";
				var testOrder1 = Order.New(Factory);
				testOrder1.JD_OrderDate = ZDate.Today;
				testOrder1.JD_OrderNumber = "A18374XG";
				testOrder1.BuyerPK = buyerOrg.PK;
				var testOrder2 = Order.New(Factory);
				testOrder2.JD_OrderDate = ZDate.Today;
				testOrder2.JD_OrderNumber = "A18395MH";
				testOrder2.BuyerPK = buyerOrg.PK;
				Factory.Save();
				AssertEquals("Document should attach to Order", true, TestDocAttacher.AttachFile(testFileInfo));
				var parent = MasterFactory.GetStorageMainForPK(testOrder1.PK);
				AssertEquals("ORD", parent.SM_Type);
				AssertEquals("Document should attach to correct buyer/order - (TestOrder1)", 1, parent.eDocs.Count);
				AssertEquals("File name should be the same as what was added", fileName, parent.eDocs[0].SC_FileNameWithExtension);
				AssertEquals("DocType should be set", "COM", parent.eDocs[0].SC_DocType);
			}
		}

		public void TestAttachHBLFromTLXtif()
		{
			string fileName = "TLX.HKGSYDH08706H01.tif";
			using (EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string testFilePath = resourceRetriever.SaveResourceToFile(fileName);
				var testFileInfo = new FileInfo(testFilePath);
				var testShipment = Factory.New<ForwardingShipment>();
				testShipment.JS_HouseBill = "HKGSYDH08706H01";
				Factory.Save();
				AssertEquals("Text Document should attach to Shipment via HAWB ref", true, TestDocAttacher.AttachFile(testFileInfo));
				var parent = MasterFactory.GetStorageMainForPK(testShipment.PK);
				AssertEquals("eDocs count should be 1", 1, parent.eDocs.Count);
				AssertEquals("DocType should be set", "EBL", parent.eDocs[0].SC_DocType);
				AssertEquals("Shipment Release type should have been updated", "EBL", testShipment.JS_ReleaseType);
			}
		}

		DocumentFactory MasterFactory;
		readonly DocumentAttacher TestDocAttacher = new DocumentAttacher();
		protected NotificationBuffer Notify = new NotificationBuffer();
		protected override void SetUp()
		{
			base.SetUp();
			MasterFactory = new DocumentFactoryProvider().GetFactory(this.Factory);
		}
	}
}
