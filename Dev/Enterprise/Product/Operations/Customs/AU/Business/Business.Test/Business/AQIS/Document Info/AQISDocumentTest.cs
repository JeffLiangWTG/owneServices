using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AQISDocument))]
	sealed class AQISDocumentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestNumber()
		{
			var document = new AQISDocument(Factory);
			AssertEquals("Number is empty", true, document.Number.IsEmpty);

			document.Number = "123";
			AssertEquals("Number is not empty", false, document.Number.IsEmpty);
		}

		public void TestNumberMaxLength()
		{
			var document = new AQISDocument(Factory);
			AssertEquals("Document Type", 35, document.NumberInfo.MaxLength);
		}

		public void TestType()
		{
			var document = new AQISDocument(Factory);
			AssertEquals("Type is empty", true, document.Type.IsEmpty);

			document.Type = "CCCC";
			AssertEquals("Type is not empty", false, document.Type.IsEmpty);
		}

		public void TestTypeMaxLength()
		{
			var document = new AQISDocument(Factory);
			AssertEquals("Document Type", 10, document.TypeInfo.MaxLength);
		}

		public void TestIAQISUniqueCodeForSort()
		{
			var document = new AQISDocument(Factory);
			document.Type = "T";
			document.Number = "N";
			AssertEquals("IAQISUniqueCodeForSort", 2, ((IAQISUniqueCodeForSort)document).CodesToSortBy.Length);
			AssertEquals("First field to sort by", "T", ((IAQISUniqueCodeForSort)document).CodesToSortBy[0]);
			AssertEquals("Second field to sort by", "N", ((IAQISUniqueCodeForSort)document).CodesToSortBy[1]);
		}

		public void TestLookups()
		{
			var document = new AQISDocument(Factory);
			AssertNotNull("Lookups", document.Lookups);
		}

		public void TestValidation()
		{
			var document = new AQISDocument(Factory);
			AssertNotNull("Validation", document.Validation);
		}

		protected override BusinessObject GetNewBusinessObject() => new AQISDocument(Factory);
	}
}
