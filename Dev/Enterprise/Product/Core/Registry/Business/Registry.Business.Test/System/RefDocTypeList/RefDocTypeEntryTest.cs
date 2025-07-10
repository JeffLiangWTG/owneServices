using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(RefDocTypeEntry))]
	sealed class RefDocTypeEntryTest : RegistryBusinessObjectTestCaseBase
	{
		#region Properties

		public void TestRefDocTypePK()
		{
			var guid1 = new ZGuid();
			var guid2 = new ZGuid();
			var guid3 = new ZGuid();

			var refDocTypeEntryCollection = new RefDocTypeEntryCollection();
			var entry1 = refDocTypeEntryCollection.AddNew();
			var entry2 = refDocTypeEntryCollection.AddNew();
			var entry3 = refDocTypeEntryCollection.AddNew();

			entry1.RefDocTypePK = guid1;
			entry2.RefDocTypePK = guid2;
			entry3.RefDocTypePK = guid3;

			AssertEquals(guid1, entry1.RefDocTypePK);
			AssertEquals(guid2, entry2.RefDocTypePK);
			AssertEquals(guid3, entry3.RefDocTypePK);
		}

		public void TestRefDocTypeName()
		{
			var docTypeACV = Factory.LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "ACV"));
			var docTypeMSC = Factory.LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "MSC"));

			var refDocTypeEntryCollection = new RefDocTypeEntryCollection();
			var entry1 = refDocTypeEntryCollection.AddNew();
			var entry2 = refDocTypeEntryCollection.AddNew();

			entry1.RefDocTypePK = docTypeACV.PK;
			entry2.RefDocTypePK = docTypeMSC.PK;

			AssertEquals(docTypeACV.RT_Desc, entry1.RefDocTypeName);
			AssertEquals(docTypeMSC.RT_Desc, entry2.RefDocTypeName);
		}

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var docTypeACV = Factory.LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "ACV"));

			var result = new RefDocTypeEntry();
			result.RefDocTypePK = docTypeACV.PK;
			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool IsCodeMandatory
		{
			get { return false; }
		}

		#endregion
	}
}
