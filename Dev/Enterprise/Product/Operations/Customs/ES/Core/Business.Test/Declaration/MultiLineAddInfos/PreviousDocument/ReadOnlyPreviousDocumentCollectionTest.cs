using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(ReadOnlyPreviousDocumentCollection))]
	class ReadOnlyPreviousDocumentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ReadOnlyPreviousDocumentCollection>
	{
		public void TestReadOnlyPreviousDocumentCollectionWithNoPreviousDocuments()
		{
			var readOnlyPreviousDocumentCollection = new ReadOnlyPreviousDocumentCollection(entryLine);
			readOnlyPreviousDocumentCollection.LoadNew();
			AssertEquals("When there are no previous documents, ReadOnlyPreviousDocumentCollection.Count", 0, readOnlyPreviousDocumentCollection.Count);
		}

		public void TestIsLoaded()
		{
			var readOnlyPreviousDocumentCollection = new ReadOnlyPreviousDocumentCollection(entryLine);
			CombineAssertions(() =>
			{
				AssertEquals("Not Loaded", false, readOnlyPreviousDocumentCollection.IsLoaded);
				readOnlyPreviousDocumentCollection.LoadNew();
				AssertEquals("Loaded", true, readOnlyPreviousDocumentCollection.IsLoaded);
			});
		}

		public void TestReadOnlyPreviousDocumentCollectionInAllLevelsIncludingEntryLinesForExport()
		{
			var prevDoc1 = GetPreviousDoc(1, "REF111");
			declaration.PreviousDocuments.Add(prevDoc1);
			var prevDoc2 = GetPreviousDoc(2, "REF222");
			invoice.PreviousDocuments.Add(prevDoc2);
			var prevDoc3 = GetPreviousDoc(3, "REF333");
			invoiceLine.PreviousDocuments.Add(prevDoc3);
			var prevDoc4 = GetPreviousDoc(4, "REF444");
			prevDoc4.CSI_ParentID = entryLine.PK;
			prevDoc4.CSI_ParentTableCode = entryLine.TablePrefix;

			var readOnlyPreviousDocumentCollection = new ReadOnlyPreviousDocumentCollection(entryLine);
			readOnlyPreviousDocumentCollection.LoadNew();
			AssertEquals("ReadOnlyPreviousDocumentCollection.Count", 2, readOnlyPreviousDocumentCollection.Count);

			var readOnlyPreviousList = readOnlyPreviousDocumentCollection.Cast<ReadOnlyPreviousDocument>();
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333"), prevDoc3);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF444"), prevDoc4);
		}

		public void TestReadOnlyPreviousDocumentCollectionInAllLevelsIncludingEntryLinesForImport()
		{
			var (declarationImport, entryHeaderImport, entryLineImport, invoiceImport, invoiceLineImport, entryInstruction) = SetUpImport();
			var prevDoc1 = GetPreviousDoc(1, "REF111");
			declarationImport.PreviousDocuments.Add(prevDoc1);
			var prevDoc2 = GetPreviousDoc(2, "REF222");
			invoiceImport.PreviousDocuments.Add(prevDoc2);
			var prevDoc3 = GetPreviousDoc(3, "REF333");
			invoiceLineImport.PreviousDocuments.Add(prevDoc3);
			var prevDoc4 = GetPreviousDoc(4, "REF444");
			prevDoc4.CSI_ParentID = entryLineImport.PK;
			prevDoc4.CSI_ParentTableCode = entryLineImport.TablePrefix;
			Factory.Save();

			var readOnlyPreviousDocumentCollection = new ReadOnlyPreviousDocumentCollection(entryLineImport);
			readOnlyPreviousDocumentCollection.LoadNew();
			AssertEquals("ReadOnlyPreviousDocumentCollection.Count", 2, readOnlyPreviousDocumentCollection.Count);

			var readOnlyPreviousList = readOnlyPreviousDocumentCollection.Cast<ReadOnlyPreviousDocument>();
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333"), prevDoc3);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF444"), prevDoc4);
		}

		public void TestReadOnlyPreviousDocumentCollectionInAllLevelsIncludingEntryLinesForImportSubStyleT2l()
		{
			var (declarationImport, entryHeaderImport, entryLineImport, invoiceImport, invoiceLineImport, entryInstruction) = SetUpImport();
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.T2L;

			var prevDoc1 = GetPreviousDoc(1, "REF111", code: "AA");
			declarationImport.PreviousDocuments.Add(prevDoc1);
			var prevDoc2 = GetPreviousDoc(2, "REF222", code: "BB");
			invoiceImport.PreviousDocuments.Add(prevDoc2);
			var prevDoc3 = GetPreviousDoc(3, "REF333", code: "CC");
			invoiceImport.PreviousDocuments.Add(prevDoc3);
			var prevDoc4 = GetPreviousDoc(4, "REF444", code: "DD");
			prevDoc4.CSI_ParentID = entryLineImport.PK;
			prevDoc4.CSI_ParentTableCode = entryLineImport.TablePrefix;
			var prevDoc5 = GetPreviousDoc(1, "REF111", code: "AA");
			prevDoc5.CSI_ParentID = entryLineImport.PK;
			prevDoc5.CSI_ParentTableCode = entryLineImport.TablePrefix;

			var readOnlyPreviousDocumentCollection = new ReadOnlyPreviousDocumentCollection(entryLineImport);
			readOnlyPreviousDocumentCollection.LoadNew();
			AssertEquals("ReadOnlyPreviousDocumentCollection.Count", 4, readOnlyPreviousDocumentCollection.Count);

			var readOnlyPreviousList = readOnlyPreviousDocumentCollection.Cast<ReadOnlyPreviousDocument>();
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF111"), prevDoc1);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222"), prevDoc2);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333"), prevDoc3);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF444"), prevDoc4);
		}

		public void TestReadOnlyPreviousDocumentCollectionInAllLevelsIncludingEntryLinesForImportSubStyleT2C()
		{
			var (declarationImport, entryHeaderImport, entryLineImport, invoiceImport, invoiceLineImport, entryInstruction) = SetUpImport();
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.T2C;

			var prevDoc1 = GetPreviousDoc(1, "REF111", code: "AA");
			declarationImport.PreviousDocuments.Add(prevDoc1);
			var prevDoc2 = GetPreviousDoc(2, "REF222", code: "BB");
			invoiceImport.PreviousDocuments.Add(prevDoc2);
			var prevDoc3 = GetPreviousDoc(3, "REF333", code: "CC");
			invoiceImport.PreviousDocuments.Add(prevDoc3);
			var prevDoc4 = GetPreviousDoc(4, "REF444", code: "DD");
			prevDoc4.CSI_ParentID = entryLineImport.PK;
			prevDoc4.CSI_ParentTableCode = entryLineImport.TablePrefix;
			var prevDoc5 = GetPreviousDoc(1, "REF111", code: "AA");
			prevDoc5.CSI_ParentID = entryLineImport.PK;
			prevDoc5.CSI_ParentTableCode = entryLineImport.TablePrefix;

			var readOnlyPreviousDocumentCollection = new ReadOnlyPreviousDocumentCollection(entryLineImport);
			readOnlyPreviousDocumentCollection.LoadNew();
			AssertEquals("ReadOnlyPreviousDocumentCollection.Count", 4, readOnlyPreviousDocumentCollection.Count);

			var readOnlyPreviousList = readOnlyPreviousDocumentCollection.Cast<ReadOnlyPreviousDocument>();
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF111"), prevDoc1);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222"), prevDoc2);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333"), prevDoc3);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF444"), prevDoc4);
		}

		public void TestReadOnlyPreviousDocumentCollectionInAllLevelsIncludingEntryLinesForImportStyleH2()
		{
			var (declarationImport, entryHeaderImport, entryLineImport, invoiceImport, invoiceLineImport, entryInstruction) = SetUpImport();
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.A;
			entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;

			var prevDoc1 = GetPreviousDoc(1, "REF111", code: "AA");
			declarationImport.PreviousDocuments.Add(prevDoc1);
			var prevDoc2 = GetPreviousDoc(2, "REF222", code: "BB");
			invoiceImport.PreviousDocuments.Add(prevDoc2);
			var prevDoc3 = GetPreviousDoc(3, "REF333", code: "CC");
			invoiceLineImport.PreviousDocuments.Add(prevDoc3);
			var prevDoc4 = GetPreviousDoc(4, "REF444", code: "DD");
			prevDoc4.CSI_ParentID = entryLineImport.PK;
			prevDoc4.CSI_ParentTableCode = entryLineImport.TablePrefix;
			var prevDoc5 = GetPreviousDoc(1, "REF111", code: "AA");
			prevDoc5.CSI_ParentID = entryLineImport.PK;
			prevDoc5.CSI_ParentTableCode = entryLineImport.TablePrefix;

			var readOnlyPreviousDocumentCollection = new ReadOnlyPreviousDocumentCollection(entryLineImport);
			readOnlyPreviousDocumentCollection.LoadNew();
			AssertEquals("ReadOnlyPreviousDocumentCollection.Count", 4, readOnlyPreviousDocumentCollection.Count);

			var readOnlyPreviousList = readOnlyPreviousDocumentCollection.Cast<ReadOnlyPreviousDocument>();
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF111"), prevDoc1);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222"), prevDoc2);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333"), prevDoc3);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF444"), prevDoc4);
		}

		public void TestReadOnlyPreviousDocumentCollectionInvoiceLinesForImport()
		{
			var (declarationImport, entryHeaderImport, entryLineImport, invoiceImport, invoiceLineImport, entryInstruction) = SetUpImport();
			var prevDoc1 = GetPreviousDoc(1, "REF111");
			declarationImport.PreviousDocuments.Add(prevDoc1);
			var prevDoc2 = GetPreviousDoc(2, "REF222");
			invoiceImport.PreviousDocuments.Add(prevDoc2);
			var prevDoc3 = GetPreviousDoc(3, "REF333");
			invoiceLineImport.PreviousDocuments.Add(prevDoc3);
			var prevDoc4 = GetPreviousDoc(4, "REF444");
			invoiceLineImport.PreviousDocuments.Add(prevDoc4);

			var readOnlyPreviousDocumentCollection = new ReadOnlyPreviousDocumentCollection(entryLineImport);
			readOnlyPreviousDocumentCollection.LoadNew();
			AssertEquals("ReadOnlyPreviousDocumentCollection.Count", 2, readOnlyPreviousDocumentCollection.Count);

			var readOnlyPreviousList = readOnlyPreviousDocumentCollection.Cast<ReadOnlyPreviousDocument>();
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333"), prevDoc3);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF444"), prevDoc4);
		}

		public void TestReadOnlyPreviousDocumentCollectionForImport_NoExceptionThrownWhenEntryInstructionIsNull()
		{
			var (declarationImport, entryHeaderImport, entryLineImport, invoiceImport, invoiceLineImport, entryInstruction) = SetUpImport();
			entryInstruction = null;
			invoiceLineImport.JI_CEI = ZGuid.Empty;
			AssertNoExceptionThrown(() =>
			{
				var readOnlyPreviousDocumentCollection = new ReadOnlyPreviousDocumentCollection(entryLineImport);
				readOnlyPreviousDocumentCollection.LoadNew();
			});
		}

		public void TestReadOnlyPreviousDocumentCollectionInvoiceLinesForExport()
		{
			var prevDoc1 = GetPreviousDoc(1, "REF111");
			declaration.PreviousDocuments.Add(prevDoc1);
			var prevDoc2 = GetPreviousDoc(2, "REF222");
			invoice.PreviousDocuments.Add(prevDoc2);
			var prevDoc3 = GetPreviousDoc(3, "REF333");
			invoiceLine.PreviousDocuments.Add(prevDoc3);
			var prevDoc4 = GetPreviousDoc(4, "REF444");
			invoiceLine.PreviousDocuments.Add(prevDoc4);

			var readOnlyPreviousDocumentCollection = new ReadOnlyPreviousDocumentCollection(entryLine);
			readOnlyPreviousDocumentCollection.LoadNew();
			AssertEquals("ReadOnlyPreviousDocumentCollection.Count", 2, readOnlyPreviousDocumentCollection.Count);

			var readOnlyPreviousList = readOnlyPreviousDocumentCollection.Cast<ReadOnlyPreviousDocument>();
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333"), prevDoc3);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF444"), prevDoc4);
		}

		public void TestReadOnlyPreviousDocumentCollectionInvoiceHeaderForImport()
		{
			var (declarationImport, entryHeaderImport, entryLineImport, invoiceImport, invoiceLineImport, entryInstruction) = SetUpImport();
			var prevDoc1 = GetPreviousDoc(1, "REF111");
			declarationImport.PreviousDocuments.Add(prevDoc1);
			var prevDoc2 = GetPreviousDoc(2, "REF222");
			invoiceImport.PreviousDocuments.Add(prevDoc2);
			var prevDoc3 = GetPreviousDoc(3, "REF333");
			invoiceImport.PreviousDocuments.Add(prevDoc3);

			var readOnlyPreviousDocumentCollection = new ReadOnlyPreviousDocumentCollection(entryLineImport);
			readOnlyPreviousDocumentCollection.LoadNew();
			AssertEquals("ReadOnlyPreviousDocumentCollection.Count", 2, readOnlyPreviousDocumentCollection.Count);

			var readOnlyPreviousList = readOnlyPreviousDocumentCollection.Cast<ReadOnlyPreviousDocument>();
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222"), prevDoc2);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333"), prevDoc3);
		}

		public void TestReadOnlyPreviousDocumentCollectionInvoiceHeaderForExport()
		{
			var prevDoc1 = GetPreviousDoc(1, "REF111");
			declaration.PreviousDocuments.Add(prevDoc1);
			var prevDoc2 = GetPreviousDoc(2, "REF222");
			invoice.PreviousDocuments.Add(prevDoc2);
			var prevDoc3 = GetPreviousDoc(3, "REF333");
			invoice.PreviousDocuments.Add(prevDoc3);

			var readOnlyPreviousDocumentCollection = new ReadOnlyPreviousDocumentCollection(entryLine);
			readOnlyPreviousDocumentCollection.LoadNew();
			AssertEquals("ReadOnlyPreviousDocumentCollection.Count", 2, readOnlyPreviousDocumentCollection.Count);

			var readOnlyPreviousList = readOnlyPreviousDocumentCollection.Cast<ReadOnlyPreviousDocument>();
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222"), prevDoc2);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333"), prevDoc3);
		}

		public void TestReadOnlyPreviousDocumentCollectionDeclarationForImport()
		{
			var (declarationImport, entryHeaderImport, entryLineImport, invoiceImport, invoiceLineImport, entryInstruction) = SetUpImport();
			var prevDoc1 = GetPreviousDoc(1, "REF111");
			declarationImport.PreviousDocuments.Add(prevDoc1);
			var prevDoc2 = GetPreviousDoc(2, "REF222");
			declarationImport.PreviousDocuments.Add(prevDoc2);

			var readOnlyPreviousDocumentCollection = new ReadOnlyPreviousDocumentCollection(entryLineImport);
			readOnlyPreviousDocumentCollection.LoadNew();
			AssertEquals("ReadOnlyPreviousDocumentCollection.Count", 2, readOnlyPreviousDocumentCollection.Count);

			var readOnlyPreviousList = readOnlyPreviousDocumentCollection.Cast<ReadOnlyPreviousDocument>();
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF111"), prevDoc1);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222"), prevDoc2);
		}

		public void TestReadOnlyPreviousDocumentCollectionDeclarationForExport()
		{
			var prevDoc1 = GetPreviousDoc(1, "REF111");
			declaration.PreviousDocuments.Add(prevDoc1);
			var prevDoc2 = GetPreviousDoc(2, "REF222");
			declaration.PreviousDocuments.Add(prevDoc2);

			var readOnlyPreviousDocumentCollection = new ReadOnlyPreviousDocumentCollection(entryLine);
			readOnlyPreviousDocumentCollection.LoadNew();
			AssertEquals("ReadOnlyPreviousDocumentCollection.Count", 2, readOnlyPreviousDocumentCollection.Count);

			var readOnlyPreviousList = readOnlyPreviousDocumentCollection.Cast<ReadOnlyPreviousDocument>();
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF111"), prevDoc1);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222"), prevDoc2);
		}

		public void TestReadOnlyPreviousDocumentCollectionWithDuplicatesForImport()
		{
			var (declarationImport, entryHeaderImport, entryLineImport, invoiceImport, invoiceLineImport, entryInstruction) = SetUpImport();
			var prevDocDec1 = GetPreviousDoc(1, "REF111");
			declarationImport.PreviousDocuments.Add(prevDocDec1);
			var prevDocDec2 = GetPreviousDoc(1, "REF222");
			declarationImport.PreviousDocuments.Add(prevDocDec2);
			var prevDocInv1 = GetPreviousDoc(2, "REF111");
			invoiceImport.PreviousDocuments.Add(prevDocInv1);
			var prevDocInv2 = GetPreviousDoc(2, "REF222");
			invoiceImport.PreviousDocuments.Add(prevDocInv2);
			var prevDocInvLin1 = GetPreviousDoc(1, "REF111");
			invoiceLineImport.PreviousDocuments.Add(prevDocInvLin1);
			var prevDocInvLin2 = GetPreviousDoc(1, "REF222");
			invoiceLineImport.PreviousDocuments.Add(prevDocInvLin2);
			var prevDocInvLin3 = GetPreviousDoc(3, "REF333");
			invoiceLineImport.PreviousDocuments.Add(prevDocInvLin3);
			var prevDocEntry = GetPreviousDoc(3, "REF333");
			prevDocEntry.CSI_ParentID = entryLineImport.PK;
			prevDocEntry.CSI_ParentTableCode = entryLineImport.TablePrefix;
			prevDocEntry.CSI_Status = "ACC";

			var readOnlyPreviousDocumentCollection = new ReadOnlyPreviousDocumentCollection(entryLineImport);
			readOnlyPreviousDocumentCollection.LoadNew();
			AssertEquals("ReadOnlyPreviousDocumentCollection.Count", 3, readOnlyPreviousDocumentCollection.Count);

			var readOnlyPreviousList = readOnlyPreviousDocumentCollection.Cast<ReadOnlyPreviousDocument>();
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF111"), prevDocInvLin1);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222"), prevDocInvLin2);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333"), prevDocEntry);
		}

		public void TestReadOnlyPreviousDocumentCollectionWithDuplicatesForImportStyleH2()
		{
			var (declarationImport, entryHeaderImport, entryLineImport, invoiceImport, invoiceLineImport, entryInstruction) = SetUpImport();
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.A;
			entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;

			var prevDocDec1 = GetPreviousDoc(1, "REFD111");
			declarationImport.PreviousDocuments.Add(prevDocDec1);
			var prevDocDec2 = GetPreviousDoc(2, "REFD222");
			declarationImport.PreviousDocuments.Add(prevDocDec2);
			var prevDocInv1 = GetPreviousDoc(3, "REFI111");
			invoiceImport.PreviousDocuments.Add(prevDocInv1);
			var prevDocInv2 = GetPreviousDoc(4, "REF222");
			invoiceImport.PreviousDocuments.Add(prevDocInv2);
			var prevDocInvLin1 = GetPreviousDoc(5, "REFL111");
			invoiceLineImport.PreviousDocuments.Add(prevDocInvLin1);
			var prevDocInvLin2 = GetPreviousDoc(4, "REF222");
			invoiceLineImport.PreviousDocuments.Add(prevDocInvLin2);
			var prevDocInvLin3 = GetPreviousDoc(7, "REF333");
			invoiceLineImport.PreviousDocuments.Add(prevDocInvLin3);
			var prevDocEntry = GetPreviousDoc(7, "REF333");
			prevDocEntry.CSI_ParentID = entryLineImport.PK;
			prevDocEntry.CSI_ParentTableCode = entryLineImport.TablePrefix;
			prevDocEntry.CSI_Status = "ACC";

			var readOnlyPreviousDocumentCollection = new ReadOnlyPreviousDocumentCollection(entryLineImport);
			readOnlyPreviousDocumentCollection.LoadNew();
			AssertEquals("ReadOnlyPreviousDocumentCollection.Count", 6, readOnlyPreviousDocumentCollection.Count);

			var readOnlyPreviousList = readOnlyPreviousDocumentCollection.Cast<ReadOnlyPreviousDocument>();
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REFD111"), prevDocDec1);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REFD222"), prevDocDec2);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REFI111"), prevDocInv1);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REFL111"), prevDocInvLin1);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222"), prevDocInvLin2);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333"), prevDocEntry);
		}

		public void TestReadOnlyPreviousDocumentCollectionWithDuplicatesForImportStyleT2l()
		{
			var (declarationImport, entryHeaderImport, entryLineImport, invoiceImport, invoiceLineImport, entryInstruction) = SetUpImport();
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.T2L;

			var prevDocDec1 = GetPreviousDoc(1, "REFD111");
			declarationImport.PreviousDocuments.Add(prevDocDec1);
			var prevDocDec2 = GetPreviousDoc(2, "REFD222");
			declarationImport.PreviousDocuments.Add(prevDocDec2);
			var prevDocInv1 = GetPreviousDoc(3, "REFI111");
			invoiceImport.PreviousDocuments.Add(prevDocInv1);
			var prevDocInv2 = GetPreviousDoc(4, "REF222");
			invoiceImport.PreviousDocuments.Add(prevDocInv2);
			var prevDocInvLin1 = GetPreviousDoc(5, "REFL111");
			invoiceLineImport.PreviousDocuments.Add(prevDocInvLin1);
			var prevDocInvLin2 = GetPreviousDoc(4, "REF222");
			invoiceLineImport.PreviousDocuments.Add(prevDocInvLin2);
			var prevDocInvLin3 = GetPreviousDoc(7, "REF333");
			invoiceLineImport.PreviousDocuments.Add(prevDocInvLin3);
			var prevDocEntry = GetPreviousDoc(7, "REF333");
			prevDocEntry.CSI_ParentID = entryLineImport.PK;
			prevDocEntry.CSI_ParentTableCode = entryLineImport.TablePrefix;
			prevDocEntry.CSI_Status = "ACC";

			var readOnlyPreviousDocumentCollection = new ReadOnlyPreviousDocumentCollection(entryLineImport);
			readOnlyPreviousDocumentCollection.LoadNew();
			AssertEquals("ReadOnlyPreviousDocumentCollection.Count", 6, readOnlyPreviousDocumentCollection.Count);

			var readOnlyPreviousList = readOnlyPreviousDocumentCollection.Cast<ReadOnlyPreviousDocument>();
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REFD111"), prevDocDec1);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REFD222"), prevDocDec2);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REFI111"), prevDocInv1);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REFL111"), prevDocInvLin1);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222"), prevDocInvLin2);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333"), prevDocEntry);
		}

		public void TestReadOnlyPreviousDocumentCollectionWithDuplicatesForImportStyleT2C()
		{
			var (declarationImport, entryHeaderImport, entryLineImport, invoiceImport, invoiceLineImport, entryInstruction) = SetUpImport();
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.T2C;

			var prevDocDec1 = GetPreviousDoc(1, "REFD111");
			declarationImport.PreviousDocuments.Add(prevDocDec1);
			var prevDocDec2 = GetPreviousDoc(2, "REFD222");
			declarationImport.PreviousDocuments.Add(prevDocDec2);
			var prevDocInv1 = GetPreviousDoc(3, "REFI111");
			invoiceImport.PreviousDocuments.Add(prevDocInv1);
			var prevDocInv2 = GetPreviousDoc(4, "REF222");
			invoiceImport.PreviousDocuments.Add(prevDocInv2);
			var prevDocInvLin1 = GetPreviousDoc(5, "REFL111");
			invoiceLineImport.PreviousDocuments.Add(prevDocInvLin1);
			var prevDocInvLin2 = GetPreviousDoc(4, "REF222");
			invoiceLineImport.PreviousDocuments.Add(prevDocInvLin2);
			var prevDocInvLin3 = GetPreviousDoc(7, "REF333");
			invoiceLineImport.PreviousDocuments.Add(prevDocInvLin3);
			var prevDocEntry = GetPreviousDoc(7, "REF333");
			prevDocEntry.CSI_ParentID = entryLineImport.PK;
			prevDocEntry.CSI_ParentTableCode = entryLineImport.TablePrefix;
			prevDocEntry.CSI_Status = "ACC";

			var readOnlyPreviousDocumentCollection = new ReadOnlyPreviousDocumentCollection(entryLineImport);
			readOnlyPreviousDocumentCollection.LoadNew();
			AssertEquals("ReadOnlyPreviousDocumentCollection.Count", 6, readOnlyPreviousDocumentCollection.Count);

			var readOnlyPreviousList = readOnlyPreviousDocumentCollection.Cast<ReadOnlyPreviousDocument>();
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REFD111"), prevDocDec1);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REFD222"), prevDocDec2);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REFI111"), prevDocInv1);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REFL111"), prevDocInvLin1);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222"), prevDocInvLin2);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333"), prevDocEntry);
		}

		public void TestReadOnlyPreviousDocumentCollectionWithDuplicatesForExport()
		{
			var prevDoc1 = GetPreviousDoc(1, "REF111");
			invoiceLine.PreviousDocuments.Add(prevDoc1);
			var prevDoc2 = GetPreviousDoc(1, "REF111");
			invoiceLine.PreviousDocuments.Add(prevDoc2);
			var prevDoc3 = GetPreviousDoc(3, "REF333");
			invoiceLine.PreviousDocuments.Add(prevDoc3);
			var prevDoc4 = GetPreviousDoc(3, "REF333");
			prevDoc4.CSI_ParentID = entryLine.PK;
			prevDoc4.CSI_ParentTableCode = entryLine.TablePrefix;
			prevDoc4.CSI_Status = "ACC";

			var readOnlyPreviousDocumentCollection = new ReadOnlyPreviousDocumentCollection(entryLine);
			readOnlyPreviousDocumentCollection.LoadNew();
			AssertEquals("ReadOnlyPreviousDocumentCollection.Count", 2, readOnlyPreviousDocumentCollection.Count);

			var readOnlyPreviousList = readOnlyPreviousDocumentCollection.Cast<ReadOnlyPreviousDocument>();
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF111"), prevDoc1);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333"), prevDoc4);
		}

		public void TestReadOnlyPreviousDocumentCollectionInAllLevelsIncludingEntryLines_UCC6_OfficePRE()
		{
			entryHeader.ZG_UCC6Version = 1;
			var customsOffice = declaration.CustomsOffices.AddNew();
			customsOffice.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
			customsOffice.CY_Data = "FR008889";

			var prevDoc1 = GetPreviousDoc(1, "REF111");
			declaration.PreviousDocuments.Add(prevDoc1);
			var prevDoc2 = GetPreviousDoc(2, "RefC651", "C651");
			invoice.PreviousDocuments.Add(prevDoc2);
			var prevDoc3 = GetPreviousDoc(3, "REF333");
			invoiceLine.PreviousDocuments.Add(prevDoc3);
			var prevDoc4 = GetPreviousDoc(4, "REF444");
			prevDoc4.CSI_ParentID = entryLine.PK;
			prevDoc4.CSI_ParentTableCode = entryLine.TablePrefix;

			var readOnlyPreviousDocumentCollection = new ReadOnlyPreviousDocumentCollection(entryLine);
			readOnlyPreviousDocumentCollection.LoadNew();
			AssertEquals("ReadOnlyPreviousDocumentCollection.Count", 4, readOnlyPreviousDocumentCollection.Count);

			var readOnlyPreviousList = readOnlyPreviousDocumentCollection.Cast<ReadOnlyPreviousDocument>();
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF111"), prevDoc1);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "RefC651"), prevDoc2);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333"), prevDoc3);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF444"), prevDoc4);
		}

		public void TestReadOnlyPreviousDocumentCollectionInAllLevelsIncludingEntryLines_UCC6_NoOfficePRE_NoC651()
		{
			entryHeader.ZG_UCC6Version = 1;

			var prevDoc1 = GetPreviousDoc(1, "REF111");
			declaration.PreviousDocuments.Add(prevDoc1);
			var prevDoc2 = GetPreviousDoc(2, "REF222");
			invoice.PreviousDocuments.Add(prevDoc2);
			var prevDoc3 = GetPreviousDoc(3, "REF333");
			invoiceLine.PreviousDocuments.Add(prevDoc3);
			var prevDoc4 = GetPreviousDoc(4, "REF444");
			prevDoc4.CSI_ParentID = entryLine.PK;
			prevDoc4.CSI_ParentTableCode = entryLine.TablePrefix;

			var readOnlyPreviousDocumentCollection = new ReadOnlyPreviousDocumentCollection(entryLine);
			readOnlyPreviousDocumentCollection.LoadNew();
			AssertEquals("ReadOnlyPreviousDocumentCollection.Count", 2, readOnlyPreviousDocumentCollection.Count);

			var readOnlyPreviousList = readOnlyPreviousDocumentCollection.Cast<ReadOnlyPreviousDocument>();
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333"), prevDoc3);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF444"), prevDoc4);
		}

		public void TestReadOnlyPreviousDocumentCollectionInAllLevelsIncludingEntryLines_UCC6_C651()
		{
			entryHeader.ZG_UCC6Version = 1;

			var prevDoc1 = GetPreviousDoc(1, "REF111");
			declaration.PreviousDocuments.Add(prevDoc1);
			var prevDoc2 = GetPreviousDoc(1, "RefC651", "C651");
			declaration.PreviousDocuments.Add(prevDoc2);
			var prevDoc3 = GetPreviousDoc(2, "REF222");
			invoice.PreviousDocuments.Add(prevDoc3);
			var prevDoc4 = GetPreviousDoc(2, "RefC651", "C651");
			invoice.PreviousDocuments.Add(prevDoc4);
			var prevDoc5 = GetPreviousDoc(3, "REF333");
			invoiceLine.PreviousDocuments.Add(prevDoc5);
			var prevDoc6 = GetPreviousDoc(3, "RefC651", "C651");
			invoiceLine.PreviousDocuments.Add(prevDoc6);
			var prevDoc7 = GetPreviousDoc(4, "REF444");
			prevDoc7.CSI_ParentID = entryLine.PK;
			prevDoc7.CSI_ParentTableCode = entryLine.TablePrefix;
			var prevDoc8 = GetPreviousDoc(4, "RefC651", "C651");
			prevDoc8.CSI_ParentID = entryLine.PK;
			prevDoc8.CSI_ParentTableCode = entryLine.TablePrefix;

			var readOnlyPreviousDocumentCollection = new ReadOnlyPreviousDocumentCollection(entryLine);
			readOnlyPreviousDocumentCollection.LoadNew();
			AssertEquals("ReadOnlyPreviousDocumentCollection.Count", 5, readOnlyPreviousDocumentCollection.Count);

			var readOnlyPreviousList = readOnlyPreviousDocumentCollection.Cast<ReadOnlyPreviousDocument>();
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "RefC651" && x.CSI_LineNo == 1), prevDoc2);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "RefC651" && x.CSI_LineNo == 2), prevDoc4);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "RefC651" && x.CSI_LineNo == 3), prevDoc6);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF444"), prevDoc7);
			CheckReadOnlyDocument(readOnlyPreviousList.SingleOrDefault(x => x.CSI_ReferenceNumber == "RefC651" && x.CSI_LineNo == 4), prevDoc8);
		}

		public void TestPreviousDocuments_UOMAndQuantity_UCC6_NoPreviousDocs()
		{
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.CustomsEntryHeaders[0].ZG_UCC6Version = 1;
			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

			var readOnlyPreviousDocumentCollection = new ReadOnlyPreviousDocumentCollection(entryLine);
			readOnlyPreviousDocumentCollection.LoadNew();

			AssertEquals("ReadOnlyPreviousDocumentCollection.Count", 0, readOnlyPreviousDocumentCollection.Count);
		}

		public void TestPreviousDocuments_UOMAndQuantity_UCC6()
		{
			var prevDoc1 = invoiceLine.PreviousDocuments.AddNew();
			prevDoc1.CSI_Code = "9001";
			prevDoc1.CSI_ReferenceNumber = "REF1";
			prevDoc1.CSI_UnitOfQuantity = "KGM";
			prevDoc1.CSI_Quantity = 2m;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var prevDoc2 = invoiceLine2.PreviousDocuments.AddNew();
			prevDoc2.CSI_Code = "9001";
			prevDoc2.CSI_ReferenceNumber = "REF1";
			prevDoc2.CSI_UnitOfQuantity = "KGM";
			prevDoc2.CSI_Quantity = 1m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.CustomsEntryHeaders[0].ZG_UCC6Version = 1;
			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

			var readOnlyPreviousDocumentCollection = new ReadOnlyPreviousDocumentCollection(entryLine);
			readOnlyPreviousDocumentCollection.LoadNew();

			AssertEquals("ReadOnlyPreviousDocumentCollection.Count", 1, readOnlyPreviousDocumentCollection.Count);
			CheckReadOnlyDocument(readOnlyPreviousDocumentCollection.Cast<ReadOnlyPreviousDocument>().FirstOrDefault(), prevDoc1, quantity: 3m);
		}

		public void TestPreviousDocuments_UOMAndQuantity_UCC6_PRECustomOffice()
		{
			var vehicle = invoiceLine.Vehicles.AddNew();
			vehicle.CVH_VehicleIdentificationNumber = "VIN1";
			var prevDoc1 = invoiceLine.PreviousDocuments.AddNew();
			prevDoc1.CSI_Code = "9001";
			prevDoc1.CSI_ReferenceNumber = "REF1";
			prevDoc1.CSI_Quantity = 2m;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var vehicle2 = invoiceLine2.Vehicles.AddNew();
			vehicle2.CVH_VehicleIdentificationNumber = "VIN2";
			var prevDoc2 = invoiceLine2.PreviousDocuments.AddNew();
			prevDoc2.CSI_Code = "9001";
			prevDoc2.CSI_ReferenceNumber = "REF1";
			prevDoc2.CSI_Quantity = 1m;

			var customsOffice = declaration.CustomsOffices.AddNew();
			customsOffice.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfPresentation;
			customsOffice.CY_Data = "FR008889";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.CustomsEntryHeaders[0].ZG_UCC6Version = 1;
			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

			var readOnlyPreviousDocumentCollection = new ReadOnlyPreviousDocumentCollection(entryLine);
			readOnlyPreviousDocumentCollection.LoadNew();

			AssertEquals("ReadOnlyPreviousDocumentCollection.Count", 1, readOnlyPreviousDocumentCollection.Count);
			CheckReadOnlyDocument(readOnlyPreviousDocumentCollection.Cast<ReadOnlyPreviousDocument>().FirstOrDefault(), prevDoc1, Enterprise.Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItems, 2m);
		}

		public void TestPreviousDocuments_UOMAndQuantity_UCC6_C651()
		{
			var prevDoc1 = invoiceLine.PreviousDocuments.AddNew();
			prevDoc1.CSI_Code = "C651";
			prevDoc1.CSI_ReferenceNumber = "REF1";
			prevDoc1.CSI_Quantity = 1m;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var prevDoc2 = invoiceLine2.PreviousDocuments.AddNew();
			prevDoc2.CSI_Code = "C651";
			prevDoc2.CSI_ReferenceNumber = "REF1";
			prevDoc2.CSI_Quantity = 1m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.CustomsEntryHeaders[0].ZG_UCC6Version = 1;
			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

			var readOnlyPreviousDocumentCollection = new ReadOnlyPreviousDocumentCollection(entryLine);
			readOnlyPreviousDocumentCollection.LoadNew();

			AssertEquals("ReadOnlyPreviousDocumentCollection.Count", 1, readOnlyPreviousDocumentCollection.Count);
			CheckReadOnlyDocument(readOnlyPreviousDocumentCollection.Cast<ReadOnlyPreviousDocument>().FirstOrDefault(), prevDoc1, quantity: 1m);
		}

		protected void CheckReadOnlyDocument(ReadOnlyPreviousDocument readOnlyPreviousDocument, PreviousDocument previousDocument, string uom = "", decimal quantity = 0)
		{
			CombineAssertions("Grouped Previous Document" + readOnlyPreviousDocument.CSI_ReferenceNumber, () =>
			{
				AssertEquals("CSI_Code", previousDocument.CSI_Code, readOnlyPreviousDocument.CSI_Code);
				AssertEquals("CSI_SubType", previousDocument.CSI_SubType, readOnlyPreviousDocument.CSI_SubType);
				AssertEquals("CSI_ReferenceNumber", previousDocument.CSI_ReferenceNumber, readOnlyPreviousDocument.CSI_ReferenceNumber);
				AssertEquals("CSI_DateOfIssue", previousDocument.CSI_DateOfIssue, readOnlyPreviousDocument.CSI_DateOfIssue);
				AssertEquals("CSI_LineNo", previousDocument.CSI_LineNo, readOnlyPreviousDocument.CSI_LineNo);
				AssertEquals("CSI_Quantity", quantity == ZDecimal.Zero ? previousDocument.CSI_Quantity : (ZDecimal)quantity, readOnlyPreviousDocument.CSI_Quantity);
				AssertEquals("CSI_UnitOfQuantity", uom.Equals(ZString.Empty) ? previousDocument.CSI_UnitOfQuantity : (ZString)uom, readOnlyPreviousDocument.CSI_UnitOfQuantity);
				AssertEquals("CSI_Status", previousDocument.CSI_Status, readOnlyPreviousDocument.CSI_Status);
			});
		}

		protected override ReadOnlyPreviousDocumentCollection GetCollectionToTest()
		{
			invoiceLine.PreviousDocuments.Add(GetPreviousDoc(1, "REF111"));
			invoiceLine.PreviousDocuments.Add(GetPreviousDoc(2, "REF222"));
			invoiceLine.PreviousDocuments.Add(GetPreviousDoc(3, "REF333"));
			var result = new ReadOnlyPreviousDocumentCollection(entryLine);
			result.LoadNew();

			return result;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var prevDoc = GetPreviousDoc(3, "REF333");
			return new ReadOnlyPreviousDocument(prevDoc);
		}

		(JobDeclaration declarationImport, CusEntryHeader entryHeaderImport, CusEntryLine entryLineImport, JobComInvoiceHeader invoiceImport, JobComInvoiceLine invoiceLineImport, CusEntryInstruction entryInstruction) SetUpImport()
		{
			var declarationImport = Factory.New<JobDeclaration>();
			declarationImport.JE_MessageType = MessageTypeList.Codes.Import;
			declarationImport.JE_ApplicationCode = "BLT";

			var entryHeaderImport = declarationImport.CustomsEntryHeaders.AddNew();
			var entryLineImport = entryHeaderImport.MergedLines.AddNew();
			var invoiceImport = declarationImport.Invoices.AddNew();
			var invoiceLineImport = invoiceImport.InvoiceLines.AddNew();
			invoiceLineImport.JI_CL = entryLineImport.PK;

			var entryInstruction = declarationImport.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.A;
			entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.IM;
			invoiceLineImport.JI_CEI = entryInstruction.PK;

			return (declarationImport, entryHeaderImport, entryLineImport, invoiceImport, invoiceLineImport, entryInstruction);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();

			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { importCodeType, exportCodeType }, "1234", "1234", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { importCodeType, exportCodeType }, "C651", "C651", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = "BLT";
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryLine = entryHeader.MergedLines.AddNew();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Weight = 100m;
			invoiceLine.JI_NetWeight = 200m;
			invoiceLine.JI_CL = entryLine.PK;
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;

		PreviousDocument GetPreviousDoc(int i, ZString refNumber, string code = "1234")
		{
			var prevDoc = Factory.New<PreviousDocument>();
			prevDoc.SuspendValidation();

			prevDoc.CSI_Code = code;
			prevDoc.CSI_SubType = "Y";
			prevDoc.CSI_ReferenceNumber = refNumber;
			prevDoc.CSI_DateOfIssue = new ZDateTime(2020, 01, 01);
			prevDoc.CSI_LineNo = i;
			prevDoc.CSI_Quantity = i * 10;
			prevDoc.CSI_UnitOfQuantity = "BAG";
			prevDoc.CSI_Status = "QWE";

			return prevDoc;
		}
	}
}
