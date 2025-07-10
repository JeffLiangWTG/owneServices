using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(LegTypeCollection))]
	sealed class LegTypeCollectionTest : RegistryBusinessObjectCollectionTestCase<LegTypeCollection>
	{
		#region Test Lists

		#region LegTypes_List

		public void TestLegTypes_List()
		{
			LegTypeCollection collection = GetCollectionToTest();
			SetUpLegTypeCollection(collection);

			AssertEquals("Should contain 4 codepairs", 4, collection.LegTypes_List.Count);
			Assert("Should contain 'AAA' codepair", collection.LegTypes_List.ContainsCode("AAA"));
			Assert("Should contain 'BBB' codepair", collection.LegTypes_List.ContainsCode("BBB"));
			Assert("Should contain 'CCC' codepair", collection.LegTypes_List.ContainsCode("CCC"));
			Assert("Should contain 'DDD' codepair", collection.LegTypes_List.ContainsCode("DDD"));
		}
		#endregion

		#region LegTypesForImport_List

		public void TestLegTypesForImport_List()
		{
			LegTypeCollection collection = GetCollectionToTest();
			SetUpLegTypeCollection(collection);

			AssertEquals("Should contain 2 codepairs", 2, collection.LegTypesForImport_List.Count);
			Assert("Should contain 'AAA' codepair", collection.LegTypesForImport_List.ContainsCode("AAA"));
			Assert("Should contain 'CCC' codepair", collection.LegTypesForImport_List.ContainsCode("CCC"));
		}
		#endregion

		#region LegTypesForExport_List

		public void TestLegTypesForExport_List()
		{
			LegTypeCollection collection = GetCollectionToTest();
			SetUpLegTypeCollection(collection);

			AssertEquals("Should contain 2 codepairs", 2, collection.LegTypesForExport_List.Count);
			Assert("Should contain 'BBB' codepair", collection.LegTypesForExport_List.ContainsCode("BBB"));
			Assert("Should contain 'DDD' codepair", collection.LegTypesForExport_List.ContainsCode("DDD"));
		}
		#endregion

		#region Setup for Leg Pair Lists

		void SetUpLegTypeCollection(LegTypeCollection collection)
		{
			SetUpLegTypeWithValues(collection.AddNew(), "AAA", "AAADesc", Constants.CartageDirection.Destination);
			SetUpLegTypeWithValues(collection.AddNew(), "BBB", "BBBDesc", Constants.CartageDirection.Origin);
			SetUpLegTypeWithValues(collection.AddNew(), "CCC", "CCCDesc", Constants.CartageDirection.Destination);
			SetUpLegTypeWithValues(collection.AddNew(), "DDD", "DDDDesc", Constants.CartageDirection.Origin);
		}

		void SetUpLegTypeWithValues(LegType legType, ZString code, ZString description, ZString movementType)
		{
			legType.Code = code;
			legType.Description = (NoResString)description;
			legType.MovementType = movementType;
		}

		#endregion

		#endregion

		#region Implementation

		protected override LegTypeCollection GetCollectionToTest()
		{
			return new LegTypeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new LegType(Factory);
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
