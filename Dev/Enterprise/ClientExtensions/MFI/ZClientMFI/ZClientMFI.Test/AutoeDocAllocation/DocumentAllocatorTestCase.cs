using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.MFI.AutoeDocAllocation.Testing
{
	public class DocumentAllocatorTestCase : TestCaseWithFactory
	{
		public void TestValidateDocumentNames()
		{
			TestDocAllocator allocator = new TestDocAllocator();
			AssertEquals("Should not be a valid document for importing", false, allocator.IsAValidDocument("ABCD.DOC"));
			AssertEquals("Is a valid document type", false, allocator.IsAValidDocument("C.C000049586"));
			AssertEquals("Is a valid document type", true, allocator.IsAValidDocument("C.C000049586.PDF"));
			AssertEquals("Is a valid document type", true, allocator.IsAValidDocument("C.C000049586.txt"));
			AssertEquals("Is a valid document type", true, allocator.IsAValidDocument("BL.CSCLON1234.freighted.PDF"));
			AssertEquals("Is a valid document type", true, allocator.IsAValidDocument("BL.CSCLON1234.non-freighted.PDF"));
			AssertEquals("Is a valid document type", true, allocator.IsAValidDocument("BL.CSCLON1234.original.PDF"));
			AssertEquals("Is a valid document type", true, allocator.IsAValidDocument("OBL.CSCLON1234.PDF"));
			AssertEquals("Is a valid document type", true, allocator.IsAValidDocument("AGI.CSCLON1234.doc"));
			AssertEquals("Is a valid document type", true, allocator.IsAValidDocument("O.CSCLON1234.PDF"));
			AssertEquals("Is a valid document type", true, allocator.IsAValidDocument("ORG.CSCLON1234.PDF"));
			AssertEquals("Is a valid document type", true, allocator.IsAValidDocument("BOO.CSCLON1234.PDF"));
			AssertEquals("Is a valid document type", true, allocator.IsAValidDocument("COM.CSCLON1234.PDF"));
			AssertEquals("Is a valid document type", true, allocator.IsAValidDocument("COMOBL.CSCLON1234.PDF"));
			AssertEquals("Is a valid document type", true, allocator.IsAValidDocument("BO.CSCLON1234.PDF"));
			AssertEquals("Is a valid document type", true, allocator.IsAValidDocument("B.B00001234.PDF"));
			AssertEquals("Is a valid document type", true, allocator.IsAValidDocument("S.S00001234.PDF"));
			AssertEquals("Is a valid document type", false, allocator.IsAValidDocument("ORD.CSCLON1234.PDF"));
			AssertEquals("Is a valid document type", true, allocator.IsAValidDocument("ORD.JAYSCH.CSCLON1234.PDF"));
			AssertEquals("Is a valid document type", true, allocator.IsAValidDocument("TSP.ConNoteNo.POD.msg"));
			AssertEquals("Is a valid document type", true, allocator.IsAValidDocument("TLX.HKGSYDH08706H01.TIF"));
		}

		class TestDocAllocator : DocumentAllocator
		{
			public new bool IsAValidDocument(string fileName)
			{
				return base.IsAValidDocument(fileName);
			}

			#region Implementation
			protected override string DirectoryToProcess
			{
				get
				{
					return "";
				}
			}
			#endregion
		}
	}
}
