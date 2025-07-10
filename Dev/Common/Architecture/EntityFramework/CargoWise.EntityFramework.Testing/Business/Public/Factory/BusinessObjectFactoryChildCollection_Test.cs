using System.Collections;
using CargoWise.Integration;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectFactoryChildCollection_Test : TestCase
	{
		public void TestAdd()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			Collection.Add(factory1);
			AssertEquals("Count", 1, Collection.Count);
			AssertEquals("Contains(factory1)", true, Collection.Contains(factory1));
			AssertEquals("Contains(factory2)", false, Collection.Contains(factory2));
			AssertEquals("Factories.Count", 1, Collection.Factories.Count);
			AssertEquals("Factories.Contains(factory1)", true, Collection.Factories.Contains(factory1));
			AssertEquals("Factories.Contains(factory2)", false, Collection.Factories.Contains(factory2));

			Collection.Add(factory2);
			AssertEquals("Count", 2, Collection.Count);
			AssertEquals("Contains(factory1)", true, Collection.Contains(factory1));
			AssertEquals("Contains(factory2)", true, Collection.Contains(factory2));
			AssertEquals("Factories.Count", 2, Collection.Factories.Count);
			AssertEquals("Factories.Contains(factory1)", true, Collection.Factories.Contains(factory1));
			AssertEquals("Factories.Contains(factory2)", true, Collection.Factories.Contains(factory2));

			Collection.Add(factory2);
			AssertEquals("Count", 2, Collection.Count);
			AssertEquals("Contains(factory1)", true, Collection.Contains(factory1));
			AssertEquals("Contains(factory2)", true, Collection.Contains(factory2));
			AssertEquals("Factories.Count", 2, Collection.Factories.Count);
			AssertEquals("Factories.Contains(factory1)", true, Collection.Factories.Contains(factory1));
			AssertEquals("Factories.Contains(factory2)", true, Collection.Factories.Contains(factory2));
		}

		public void TestRemove()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			Collection.Add(factory1);
			Collection.Add(factory2);

			Collection.Remove(factory2);
			AssertEquals("Count", 1, Collection.Count);
			AssertEquals("Contains(factory1)", true, Collection.Contains(factory1));
			AssertEquals("Contains(factory2)", false, Collection.Contains(factory2));
			AssertEquals("Factories.Count", 1, Collection.Factories.Count);
			AssertEquals("Factories.Contains(factory1)", true, Collection.Factories.Contains(factory1));
			AssertEquals("Factories.Contains(factory2)", false, Collection.Factories.Contains(factory2));

			Collection.Remove(factory1);
			AssertEquals("Count", 0, Collection.Count);
			AssertEquals("Contains(factory1)", false, Collection.Contains(factory1));
			AssertEquals("Contains(factory2)", false, Collection.Contains(factory2));
			AssertEquals("Factories.Count", 0, Collection.Factories.Count);
			AssertEquals("Factories.Contains(factory1)", false, Collection.Factories.Contains(factory1));
			AssertEquals("Factories.Contains(factory2)", false, Collection.Factories.Contains(factory2));

			Collection.Remove(factory1);
			AssertEquals("Count", 0, Collection.Count);
			AssertEquals("Contains(factory1)", false, Collection.Contains(factory1));
			AssertEquals("Contains(factory2)", false, Collection.Contains(factory2));
			AssertEquals("Factories.Count", 0, Collection.Factories.Count);
			AssertEquals("Factories.Contains(factory1)", false, Collection.Factories.Contains(factory1));
			AssertEquals("Factories.Contains(factory2)", false, Collection.Factories.Contains(factory2));
		}

		public void TestClear()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			Collection.Add(factory1);
			Collection.Add(factory2);

			Collection.Clear();
			AssertEquals("Count", 0, Collection.Count);
			AssertEquals("Contains(factory1)", false, Collection.Contains(factory1));
			AssertEquals("Contains(factory2)", false, Collection.Contains(factory2));
			AssertEquals("Factories.Count", 0, Collection.Factories.Count);
			AssertEquals("Factories.Contains(factory1)", false, Collection.Factories.Contains(factory1));
			AssertEquals("Factories.Contains(factory2)", false, Collection.Factories.Contains(factory2));
		}

		public void TestIndexer()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			Collection.Add(factory1);
			Collection.Add(factory2);

			AssertEquals("Collection[0]", factory1, Collection[0]);
			AssertEquals("Collection[1]", factory2, Collection[1]);
		}

		public void TestToArray()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			Collection.Add(factory1);
			Collection.Add(factory2);

			ITransactionParticipant[] participants = Collection.ToArray();
			AssertEquals("participants.Length", 2, participants.Length);
			AssertEquals("participants[0]", factory1, participants[0]);
			AssertEquals("participants[1]", factory2, participants[1]);
		}

		public void TestInterfaceMembers()
		{
			ICollection iCollection = Collection;

			AssertEquals("IsSynchronized", Collection.Factories.IsSynchronized, iCollection.IsSynchronized);
			AssertEquals("SyncRoot", Collection.Factories.SyncRoot, iCollection.SyncRoot);

			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			Collection.Add(factory1);
			Collection.Add(factory2);

			BusinessObjectFactory[] factories = new BusinessObjectFactory[2];
			iCollection.CopyTo(factories, 0);
			AssertEquals("factories[0]", factory1, factories[0]);
			AssertEquals("factories[1]", factory2, factories[1]);

			int i = 0;
			foreach (BusinessObjectFactory factory in Collection)
			{
				BusinessObjectFactory expectedFactory = (i == 0) ? factory1 : factory2;
				AssertEquals("factory", expectedFactory, factory);
				i++;
			}
		}

		#region Collection

		BusinessObjectFactoryChildCollection Collection
		{
			get
			{
				if (collection == null)
				{
					collection = new BusinessObjectFactoryChildCollection(parent);
				}
				return collection;
			}
		}

		BusinessObjectFactoryChildCollection collection;
		readonly BusinessObjectFactory parent = new BusinessObjectFactory();

		#endregion
	}
}
