using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionWithEnabledAndDefaultCollection))]
	sealed class CodeDescriptionWithEnabledAndDefaultCollectionTest : RegistryBusinessObjectCollectionTestCase<CodeDescriptionWithEnabledAndDefaultCollection>
	{
		#region Properties

		public void TestDefault()
		{
			var collection = new CodeDescriptionWithEnabledAndDefaultCollection();
			var item1 = collection.AddNew();
			var item2 = collection.AddNew();

			item1.IsDefault = false;
			item2.IsDefault = true;
			AssertEquals(item2, collection.Default);

			collection.Remove(item2);
			AssertEquals(null, collection.Default);
		}

		public void TestCodeMaxLength()
		{
			var collection = new CodeDescriptionWithEnabledAndDefaultCollection();
			AssertEquals(0, collection.CodeMaxLength);

			collection = new CodeDescriptionWithEnabledAndDefaultCollection(256);
			AssertEquals(256, collection.CodeMaxLength);
		}

		#endregion

		#region Get Active CodeDescriptionPairList

		public void TestGetActiveCodeDescriptionPairList()
		{
			var collection = new CodeDescriptionWithEnabledAndDefaultCollection();
			var element1 = collection.AddNew();
			element1.Code = "AAA";
			element1.Description = (NoResString)"Desc A";
			element1.IsEnabled = true;
			var element2 = collection.AddNew();
			element2.Code = "BBB";
			element2.Description = (NoResString)"Desc B";
			element2.IsEnabled = false;
			var element3 = collection.AddNew();
			element3.Code = "CCC";
			element3.Description = (NoResString)"Desc C";
			element3.IsEnabled = true;

			var activeList = collection.GetActiveCodeDescriptionPairList();
			AssertEquals(2, activeList.Count);
			AssertEquals("AAA", activeList[0].Code);
			AssertEquals("CCC", activeList[1].Code);
		}

		#endregion

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override CodeDescriptionWithEnabledAndDefaultCollection GetCollectionToTest()
		{
			return new CodeDescriptionWithEnabledAndDefaultCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CodeDescriptionWithEnabledAndDefault();
		}

		#endregion
	}
}
