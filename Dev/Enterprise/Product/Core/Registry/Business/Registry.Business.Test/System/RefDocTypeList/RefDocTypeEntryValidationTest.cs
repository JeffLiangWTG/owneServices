using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business.Testing
{
	sealed class RefDocTypeEntryValidationTest : BusinessObjectValidationTestCase
	{
		public void TestRefDocTypePKValidation()
		{
			var docTypeACV = Factory.LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "ACV"));
			var docTypeMSC = Factory.LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "MSC"));

			var refDocTypeEntryCollection = new RefDocTypeEntryCollection();
			var entry1 = refDocTypeEntryCollection.AddNew();
			var entry2 = refDocTypeEntryCollection.AddNew();

			entry1.RefDocTypePK = docTypeACV.PK;
			entry2.RefDocTypePK = docTypeMSC.PK;

			AssertNoErrors(entry1.RefDocTypePKInfo);
			AssertNoErrors(entry2.RefDocTypePKInfo);

			entry1.RefDocTypePK = docTypeMSC.PK;
			AssertHasError(entry1.RefDocTypePKInfo, "The Doc Type has been duplicated and must be unique.");
		}
	}
}
