using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OrgCodeElementCollection))]
	sealed class OrgCodeElementCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<OrgCodeElementCollection>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		public static void AssertElements(OrgCodeElementCollection collection)
		{
			AssertEquals("Count", 8, collection.Count);

			AssertEquals("[0].Description", "First Name", collection[0].Description);
			AssertEquals("[1].Description", "Second Name", collection[1].Description);
			AssertEquals("[2].Description", "Last Name", collection[2].Description);
			AssertEquals("[3].Description", "Country Code", collection[3].Description);
			AssertEquals("[4].Description", "UNLOCO Code", collection[4].Description);
			AssertEquals("[5].Description", "IATA Code", collection[5].Description);
			AssertEquals("[6].Description", "Globally Unique Number", collection[6].Description);
			AssertEquals("[7].Description", "Code Specific Unique Number", collection[7].Description);

			AssertEquals("[3].Length", (ZByte)2, collection[3].Length);
			AssertEquals("[4].Length", (ZByte)5, collection[4].Length);
			AssertEquals("[5].Length", (ZByte)3, collection[5].Length);
		}

		protected override OrgCodeElementCollection GetCollectionToTest()
		{
			return new OrgCodeElementCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OrgCodeElement("ZZZ");
		}

		public void TestAllowNew()
		{
			AssertEquals("AllowNew", false, Collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			AssertEquals("AllowRemove", false, Collection.AllowRemove);
		}

		public void TestCopyElementValuesFrom()
		{
			OrgCodeElementCollection sourceCollection = new OrgCodeElementCollection();
			sourceCollection.Load();
			Collection.Load();

			sourceCollection[0].Length = 1;
			sourceCollection[1].Order = 2;
			sourceCollection[2].Length = 3;
			sourceCollection[2].Order = 4;

			Collection.CopyElementValuesFrom(sourceCollection);

			AssertEquals("[0].Length", (ZByte)1, Collection[0].Length);
			AssertEquals("[1].Length", ZByte.Zero, Collection[1].Length);
			AssertEquals("[2].Length", (ZByte)3, Collection[2].Length);
			AssertEquals("[3].Length", (ZByte)2, Collection[3].Length);
			AssertEquals("[4].Length", (ZByte)5, Collection[4].Length);
			AssertEquals("[5].Length", (ZByte)3, Collection[5].Length);
			AssertEquals("[6].Length", ZByte.Zero, Collection[6].Length);
			AssertEquals("[7].Length", ZByte.Zero, Collection[7].Length);

			AssertEquals("[0].Order", ZByte.Zero, Collection[0].Order);
			AssertEquals("[1].Order", (ZByte)2, Collection[1].Order);
			AssertEquals("[2].Order", (ZByte)4, Collection[2].Order);
			AssertEquals("[3].Order", ZByte.Zero, Collection[3].Order);
			AssertEquals("[4].Order", ZByte.Zero, Collection[4].Order);
			AssertEquals("[5].Order", ZByte.Zero, Collection[5].Order);
			AssertEquals("[6].Order", ZByte.Zero, Collection[6].Order);
			AssertEquals("[7].Order", ZByte.Zero, Collection[7].Order);
		}

		public void TestIndexer()
		{
			Collection.Load();
			AssertNull("[\"X\"]", Collection["X"]);
			AssertEquals("[\"First Name\"]", Collection[0], Collection["First Name"]);
		}

		public void TestLoad()
		{
			Collection.Load();
			AssertElements(Collection);
		}

		public void TestParent()
		{
			OrgCodeAlgorithm parent = new OrgCodeAlgorithm();
			OrgCodeElementCollection collection = new OrgCodeElementCollection(parent);
			AssertEquals("Parent", parent, collection.Parent);
		}

		public void TestSupportsSorting()
		{
			AssertEquals("SupportsSorting", false, ((IBindingList)Collection).SupportsSorting);
		}
	}
}
