using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ActiveBusinessObjectCollectionSharedViewTest : TestCaseWithFactory
	{
		DummyBusinessObject CreateDummy(ZGuid name)
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Guid = name;
			dummy.Z0_Description = "Jam";
			return dummy;
		}

		void Delete(List<DummyBusinessObject> list, int index)
		{
			var item = list[index];
			list.RemoveAt(index);
			item.Delete();
		}

		public void TestCollectionWithSharedQuery_Deleting()
		{
			var g1 = new ZGuid("1000e136-4e99-4ab9-901a-72c6e5e33bac");
			var g2 = new ZGuid("920c51d3-3f97-448b-8d27-e8ba5c289c81");
			var g3 = new ZGuid("d515e77d-1164-4bb2-9fac-1419d3bb7ccf");
			var group1 = Enumerable.Range(0, 3).Select(_ => CreateDummy(g1)).ToList();
			var group2 = Enumerable.Range(0, 3).Select(_ => CreateDummy(g2)).ToList();
			var group3 = Enumerable.Range(0, 3).Select(_ => CreateDummy(g3)).ToList();

			var collection1 = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, new ZQuery(DummyBizoSchema.Z0_Guid, g1).AddToFilter(DummyBizoSchema.Z0_Description, "Jam"));
			var collection2 = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, new ZQuery(DummyBizoSchema.Z0_Guid, g2).AddToFilter(DummyBizoSchema.Z0_Description, "Jam"));
			var collection3 = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, new ZQuery(DummyBizoSchema.Z0_Guid, g3).AddToFilter(DummyBizoSchema.Z0_Description, "Jam"));

			void AssertAll()
			{
				AssertContainsExactElementsInAnyOrder(group1, collection1);
				AssertContainsExactElementsInAnyOrder(group2, collection2);
				AssertContainsExactElementsInAnyOrder(group3, collection3);
			}

			AssertAll();

			Delete(group1, 0);
			AssertAll();

			Delete(group2, 0);
			AssertAll();

			Delete(group3, 0);
			AssertAll();

			Delete(group3, 1);
			AssertAll();

			Delete(group2, 1);
			AssertAll();
		}

		public void TestCollectionWithSharedQuery_Moving()
		{
			var g1 = new ZGuid("1000e136-4e99-4ab9-901a-72c6e5e33bac");
			var g2 = new ZGuid("920c51d3-3f97-448b-8d27-e8ba5c289c81");
			var g3 = new ZGuid("d515e77d-1164-4bb2-9fac-1419d3bb7ccf");
			var group1 = Enumerable.Range(0, 3).Select(_ => CreateDummy(g1)).ToList();
			var group2 = Enumerable.Range(0, 3).Select(_ => CreateDummy(g2)).ToList();
			var group3 = Enumerable.Range(0, 3).Select(_ => CreateDummy(g3)).ToList();

			var collection1 = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, new ZQuery(DummyBizoSchema.Z0_Guid, g1).AddToFilter(DummyBizoSchema.Z0_Description, "Jam"));
			var collection2 = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, new ZQuery(DummyBizoSchema.Z0_Guid, g2).AddToFilter(DummyBizoSchema.Z0_Description, "Jam"));
			var collection3 = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, new ZQuery(DummyBizoSchema.Z0_Guid, g3).AddToFilter(DummyBizoSchema.Z0_Description, "Jam"));

			void AssertAll()
			{
				AssertContainsExactElementsInAnyOrder(group1, collection1);
				AssertContainsExactElementsInAnyOrder(group2, collection2);
				AssertContainsExactElementsInAnyOrder(group3, collection3);
			}

			AssertAll();

			void MoveGroup(List<DummyBusinessObject> src, List<DummyBusinessObject> dest, int index)
			{
				var item = src[index];
				src.Remove(item);
				item.Z0_Guid = dest[0].Z0_Guid;
				dest.Add(item);
			}

			MoveGroup(group1, group2, 0);
			AssertAll();

			MoveGroup(group3, group1, 2);
			AssertAll();

			MoveGroup(group3, group1, 1);
			AssertAll();

			MoveGroup(group3, group2, 0);
			AssertAll();
		}

		public void TestCollectionWithSharedQuery_Adding()
		{
			var g1 = new ZGuid("1000e136-4e99-4ab9-901a-72c6e5e33bac");
			var g2 = new ZGuid("920c51d3-3f97-448b-8d27-e8ba5c289c81");
			var g3 = new ZGuid("d515e77d-1164-4bb2-9fac-1419d3bb7ccf");
			var group1 = Enumerable.Range(0, 3).Select(_ => CreateDummy(g1)).ToList();
			var group2 = Enumerable.Range(0, 3).Select(_ => CreateDummy(g2)).ToList();
			var group3 = Enumerable.Range(0, 3).Select(_ => CreateDummy(g3)).ToList();

			var collection1 = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, new ZQuery(DummyBizoSchema.Z0_Guid, g1).AddToFilter(DummyBizoSchema.Z0_Description, "Jam"));
			var collection2 = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, new ZQuery(DummyBizoSchema.Z0_Guid, g2).AddToFilter(DummyBizoSchema.Z0_Description, "Jam"));
			var collection3 = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, new ZQuery(DummyBizoSchema.Z0_Guid, g3).AddToFilter(DummyBizoSchema.Z0_Description, "Jam"));

			void AssertAll()
			{
				AssertContainsExactElementsInAnyOrder(group1, collection1);
				AssertContainsExactElementsInAnyOrder(group2, collection2);
				AssertContainsExactElementsInAnyOrder(group3, collection3);
			}

			AssertAll();

			void AddToGroup(List<DummyBusinessObject> src)
			{
				src.Add(CreateDummy(src[0].Z0_Guid));
			}

			AddToGroup(group1);
			AssertAll();

			AddToGroup(group2);
			AssertAll();

			AddToGroup(group3);
			AssertAll();

			CreateDummy(ZGuid.NewZGuid());
			AssertAll();
		}

		public void TestCollectionWithSharedQuery_ChangingFilter()
		{
			var g1 = new ZGuid("1000e136-4e99-4ab9-901a-72c6e5e33bac");
			var g2 = new ZGuid("920c51d3-3f97-448b-8d27-e8ba5c289c81");
			var g3 = new ZGuid("d515e77d-1164-4bb2-9fac-1419d3bb7ccf");
			var group1 = Enumerable.Range(0, 3).Select(_ => CreateDummy(g1)).ToList();
			var group2 = Enumerable.Range(0, 3).Select(_ => CreateDummy(g2)).ToList();
			var group3 = Enumerable.Range(0, 3).Select(_ => CreateDummy(g3)).ToList();

			var collection1 = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, new ZQuery(DummyBizoSchema.Z0_Guid, g1).AddToFilter(DummyBizoSchema.Z0_Description, "Jam"));
			var collection2 = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, new ZQuery(DummyBizoSchema.Z0_Guid, g2).AddToFilter(DummyBizoSchema.Z0_Description, "Jam"));
			var collection3 = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, new ZQuery(DummyBizoSchema.Z0_Guid, g3).AddToFilter(DummyBizoSchema.Z0_Description, "Jam"));

			void AssertAll()
			{
				AssertContainsExactElementsInAnyOrder(group1, collection1);
				AssertContainsExactElementsInAnyOrder(group2, collection2);
				AssertContainsExactElementsInAnyOrder(group3, collection3);
			}

			void UnFilter(List<DummyBusinessObject> list, int index)
			{
				var item = list[index];
				list.RemoveAt(index);
				item.Z0_Description = "";
			}

			AssertAll();

			UnFilter(group1, 0);
			AssertAll();

			UnFilter(group2, 0);
			AssertAll();

			UnFilter(group3, 0);
			AssertAll();

			UnFilter(group3, 1);
			AssertAll();

			UnFilter(group2, 1);
			AssertAll();
		}
	}
}
