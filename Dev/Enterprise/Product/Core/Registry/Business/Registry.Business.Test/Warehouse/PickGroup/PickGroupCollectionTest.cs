using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PickGroupCollection))]
	sealed class PickGroupCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<PickGroupCollection>
	{
		#region TestOnAddNew_DefaultsNewPickGroupSequenceToNextHighestSequence

		public void TestOnAddNew_DefaultsNewPickGroupSequenceToNextHighestSequence()
		{
			var collection = new PickGroupCollection();
			var pickGroup1 = collection.AddNew();
			AssertEquals(new ZShort(1), pickGroup1.PickSequence);

			var pickGroup2 = collection.AddNew();
			AssertEquals(new ZShort(2), pickGroup2.PickSequence);

			collection.Remove(pickGroup2);
			var pickGroup3 = collection.AddNew();
			AssertEquals(new ZShort(2), pickGroup2.PickSequence);

			var pickGroup4 = new PickGroup();
			pickGroup4.PickSequence = 5;
			collection.Add(pickGroup4);
			AssertEquals(new ZShort(5), pickGroup4.PickSequence);

			var pickGroup5 = collection.AddNew();
			AssertEquals(new ZShort(6), pickGroup5.PickSequence);
		}

		#endregion

		#region TestAllowNew

		public void TestAllowNew()
		{
			AssertEquals("Must allow new rows", true, new PickGroupCollection().AllowNew);
		}

		#endregion

		#region ICodeDescriptionPairList

		#region TestContainsCode

		public void TestContainsCode()
		{
			var collection = new PickGroupCollection();
			ICodeDescriptionPairList codeDescriptionPairList = collection;
			AssertEquals(false, codeDescriptionPairList.ContainsCode(1));
			AssertEquals(false, codeDescriptionPairList.ContainsCode("1"));

			var pickGroup1 = collection.AddNew();
			var pickGroup2 = new PickGroup { PickSequence = 3 };
			collection.Add(pickGroup2);
			AssertEquals(true, codeDescriptionPairList.ContainsCode(1));
			AssertEquals(true, codeDescriptionPairList.ContainsCode("1"));
			AssertEquals(false, codeDescriptionPairList.ContainsCode(2));
			AssertEquals(false, codeDescriptionPairList.ContainsCode("2"));
			AssertEquals(true, codeDescriptionPairList.ContainsCode(3));
			AssertEquals(true, codeDescriptionPairList.ContainsCode("3"));
		}

		#endregion

		#region TestGetDescriptionFromCode

		public void TestGetDescriptionFromCode()
		{
			var collection = new PickGroupCollection();
			ICodeDescriptionPairList codeDescriptionPairList = collection;
			AssertEquals("", codeDescriptionPairList.GetDescriptionFromCode("1"));

			var pickGroup1 = collection.AddNew();
			pickGroup1.Description = (NoResString)"Desc";
			var pickGroup2 = new PickGroup { PickSequence = 3, Description = (NoResString)"Test" };
			collection.Add(pickGroup2);
			AssertEquals("Desc", codeDescriptionPairList.GetDescriptionFromCode("1"));
			AssertEquals("", codeDescriptionPairList.GetDescriptionFromCode("2"));
			AssertEquals("Test", codeDescriptionPairList.GetDescriptionFromCode("3"));
		}

		#endregion

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

		protected override PickGroupCollection GetCollectionToTest()
		{
			return new PickGroupCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PickGroup();
		}

		#endregion
	}
}
