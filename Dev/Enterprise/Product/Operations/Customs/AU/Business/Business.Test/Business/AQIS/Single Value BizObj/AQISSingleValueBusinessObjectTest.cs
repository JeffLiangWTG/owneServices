using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class AQISSingleValueBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCode()
		{
			AssertEquals("Code is empty", true, BusinessObjectToTest.Code.IsEmpty);

			BusinessObjectToTest.Code = "CC";
			AssertEquals("Code is not empty", false, BusinessObjectToTest.Code.IsEmpty);
		}

		public void TestIAQISUniqueCodeForSort()
		{
			BusinessObjectToTest.Code = "CC";
			AssertEquals("IAQISUniqueCodeForSort", 1, ((IAQISUniqueCodeForSort)BusinessObjectToTest).CodesToSortBy.Length);
			AssertEquals("First field to sort by", "CC", ((IAQISUniqueCodeForSort)BusinessObjectToTest).CodesToSortBy[0]);
		}

		public void TestValidation()
		{
			AssertNotNull("Validation", BusinessObjectToTest.Validation);
		}

		public abstract AQISSingleValueBusinessObject BusinessObjectToTest { get; }
	}
}
