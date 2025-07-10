using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions
{
	public class DefinitionFinderTest : TransactionedTestCase
	{
		public void TestHasDefinition_WithEntitySetName()
		{
			var result = finder.HasDefinitionWithEntitySetName("Airline");
			AssertEquals(true, result);

			result = finder.HasDefinitionWithEntitySetName("");
			AssertEquals(false, result);

			result = finder.HasDefinitionWithEntitySetName("Error");
			AssertEquals(false, result);
		}

		public void TestHasDefinition_WithTopTableName()
		{
			var result = finder.HasDefinitionWithTopTableName("RefAirline");
			AssertEquals(true, result);

			result = finder.HasDefinitionWithTopTableName("Error");
			AssertEquals(false, result);

			result = finder.HasDefinitionWithTopTableName("");
			AssertEquals(false, result);

			result = finder.HasDefinitionWithTopTableName(null);
			AssertEquals(false, result);
		}

		public void TestFindByEntitySetName_DefinitionNotFound()
		{
			AssertExceptionThrown(
				"Finder should throw Application Exception",
				typeof(NativeXMLUserVisibleException),
				() => finder.FindByEntitySetName("NotExistedName")
				);
		}

		public void TestFindByTopTableName_DefinitionNotFound()
		{
			AssertExceptionThrown(
				"Finder should throw Application Exception",
				typeof(NativeXMLUserVisibleException),
				() => finder.FindByTopTableName("NotExistedName")
				);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			finder = TestUtil.GetEntitySetDefinitionFinder();
		}
		IDefinitionFinder finder;
		#endregion
	}
}
