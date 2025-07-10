using System.Linq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class CustomPropertyContainerTest : TestCase
	{
		static CustomPropertyContainer GetContainer() => new CustomPropertyContainer();

		public void TestNopropertiesOfSameTypeWithDuplicateNames()
		{
			CustomPropertyContainer container = GetContainer();
			AssertEquals(0, container.CustomProperties.Count());

			container.AddCustomProperty(new CustomPropertyForTest { Identifier = "f1" });
			AssertEquals(1, container.CustomProperties.Count());

			container.AddCustomProperty(new CustomPropertyForTest { Identifier = "f2" });
			AssertEquals(2, container.CustomProperties.Count());

			container.AddCustomProperty(new CustomPropertyForTest { Identifier = "f1" });
			AssertEquals(2, container.CustomProperties.Count());

			container.AddCustomProperty("f1", "111");
			AssertEquals(3, container.CustomProperties.Count());

			container.AddCustomProperty("f3", "111");
			AssertEquals(4, container.CustomProperties.Count());
		}

		public void TestAddCustomProperty()
		{
			var container = GetContainer();
			AssertEquals(0, container.CustomProperties.Count());

			CustomPropertyForTest property1 = new CustomPropertyForTest() { Identifier = "Test" };
			container.AddCustomProperty(property1);
			AssertEquals(1, container.CustomProperties.Count());
			AssertEquals(property1, container.CustomProperties.ToList()[0]);

			container.AddCustomProperty("abc", "111");
			ICustomProperty property2 = container.FindPropertyByIdentifier("abc");
			AssertNotNull(property2);
			AssertEquals("abc", property2.Identifier);
			AssertEquals(typeof(string), property2.Info.Type);
			Assert(property2.Info.Visible);
			AssertEquals("111", property2.GetValue(null));

			container.AddCustomProperty("xyz", typeof(int), parent => 111);
			ICustomProperty property3 = container.FindPropertyByIdentifier("xyz");
			AssertNotNull(property3);
			AssertEquals("xyz", property3.Identifier);
			AssertEquals(typeof(int), property3.Info.Type);
			Assert(property3.Info.Visible);
			AssertEquals(111, property3.GetValue(null));
		}

		public void TestAddCustomProperty_SameIdentifierDifferentType()
		{
			var container = GetContainer();
			AssertEquals(0, container.CustomProperties.Count());

			container.AddCustomProperty("abc", "111");
			ICustomProperty property1 = container.FindPropertyByIdentifier("abc");
			AssertNotNull(property1);
			AssertEquals("abc", property1.Identifier);
			AssertEquals(typeof(string), property1.Info.Type);
			Assert(property1.Info.Visible);
			AssertEquals("111", property1.GetValue(null));

			container.AddCustomProperty("abc", 111);
			ICustomProperty property2 = container.FindPropertyByIdentifier("abc");
			AssertNotNull(property2);
			AssertEquals("abc", property2.Identifier);
			AssertEquals(typeof(int), property2.Info.Type);
			Assert(property2.Info.Visible);
			AssertEquals(111, property2.GetValue(null));
		}

		public void TestAddInvisibleProperties()
		{
			var container = GetContainer();
			container.AddCustomProperty("abc", "abc", typeof(string), valueGetter: parent => "A", visible: false);

			var property = container.FindPropertyByIdentifier("abc");
			AssertNotNull(property);
			AssertEquals("abc", property.Identifier);
			AssertEquals(typeof(string), property.Info.Type);
			Assert(!property.Info.Visible);
			AssertEquals("A", property.GetValue(null));
		}

		public void TestAddEditableCustomProperty()
		{
			string value1 = "111";
			int value2 = 111;

			CustomPropertyContainer container = GetContainer();

			container.AddCustomProperty("abc", typeof(string), parent => value1, (parent, value) => { value1 = (string)value; return true; });
			ICustomProperty property1 = container.FindPropertyByIdentifier("abc");
			AssertNotNull(property1);
			AssertEquals("abc", property1.Identifier);
			AssertEquals(typeof(string), property1.Info.Type);
			AssertEquals("111", property1.GetValue(null));

			Assert(property1.TrySetValue(null, "222"));
			AssertEquals("222", value1);
			AssertEquals("222", property1.GetValue(null));
			value1 = "333";
			AssertEquals("333", property1.GetValue(null));

			container.AddCustomProperty("xyz", typeof(int), parent => value2, (parent, value) => { value2 = (int)value; return true; });
			ICustomProperty property2 = container.FindPropertyByIdentifier("xyz");
			AssertNotNull(property2);
			AssertEquals("xyz", property2.Identifier);
			AssertEquals(typeof(int), property2.Info.Type);
			AssertEquals(111, property2.GetValue(null));

			Assert(property2.TrySetValue(null, 222));
			AssertEquals(222, value2);
			AssertEquals(222, property2.GetValue(null));
			value2 = 333;
			AssertEquals(333, property2.GetValue(null));
		}

		public void TestRemoveCustomProperty()
		{
			CustomPropertyContainer container = GetContainer();
			AssertEquals(0, container.CustomProperties.Count());

			container.AddCustomProperty("abc", "111");
			container.AddCustomProperty("xyz", "111");
			container.AddCustomProperty("ttt", "111");
			AssertEquals(3, container.CustomProperties.Count());

			container.RemoveCustomProperty("qqq");
			AssertEquals(3, container.CustomProperties.Count());

			container.RemoveCustomProperty("xyz");
			AssertEquals(2, container.CustomProperties.Count());

			container.RemoveCustomProperty("xyz");
			AssertEquals(2, container.CustomProperties.Count());

			container.RemoveCustomProperty("abc");
			AssertEquals(1, container.CustomProperties.Count());

			container.RemoveCustomProperty("ttt");
			AssertEquals(0, container.CustomProperties.Count());
			Assert(container["ttt"] == null);
		}

		public void TestClear()
		{
			CustomPropertyContainer container = GetContainer();
			AssertEquals(0, container.CustomProperties.Count());

			container.AddCustomProperty("abc", "111");
			container.AddCustomProperty("xyz", "111");
			container.AddCustomProperty("ttt", "111");
			AssertEquals(3, container.CustomProperties.Count());

			container.ClearCustomProperties();
			AssertEquals(0, container.CustomProperties.Count());
		}

		public void TestFindPropertyByIdentifier()
		{
			var container = GetContainer();
			container.AddCustomProperty("abc", "111");
			var property = container.FindPropertyByIdentifier("abc");
			AssertEquals("Indexer", property, container["abc"]);
			AssertNotNull(property);
			AssertEquals("abc", property.Identifier);
			AssertEquals(typeof(string), property.Info.Type);
			AssertEquals("111", property.GetValue(null));
		}

		public void TestpropertysSetChanged()
		{
			int propertysSetChangedCounter = 0;

			CustomPropertyContainer container = GetContainer();
			container.PropertySetChanged += (sender, eventArgs) => propertysSetChangedCounter++;

			AssertEquals(0, propertysSetChangedCounter);

			container.AddCustomProperty("f1", string.Empty);
			container.AddCustomProperty("f2", string.Empty);
			container.AddCustomProperty("f3", string.Empty);
			AssertEquals(3, propertysSetChangedCounter);

			container.AddCustomProperty("f1", string.Empty);
			AssertEquals(3, propertysSetChangedCounter);

			container.RemoveCustomProperty("f1");
			container.RemoveCustomProperty("f2");
			AssertEquals(5, propertysSetChangedCounter);

			container.RemoveCustomProperty("f1");
			AssertEquals(5, propertysSetChangedCounter);

			using (container.SuspendpropertysSetChanged())
			{
				container.AddCustomProperty("f4", string.Empty);
				container.AddCustomProperty("f5", string.Empty);
				container.AddCustomProperty("f6", string.Empty);
				AssertEquals(5, propertysSetChangedCounter);
			}
			AssertEquals(6, propertysSetChangedCounter);

			using (container.SuspendpropertysSetChanged())
			{
				container.RemoveCustomProperty("f4");
				container.RemoveCustomProperty("f5");
				AssertEquals(6, propertysSetChangedCounter);
			}
			AssertEquals(7, propertysSetChangedCounter);

			using (container.SuspendpropertysSetChanged())
			{
				using (container.SuspendpropertysSetChanged())
				{
					container.AddCustomProperty("f7", string.Empty);
					container.AddCustomProperty("f8", string.Empty);
					AssertEquals(7, propertysSetChangedCounter);
				}
				container.AddCustomProperty("f9", string.Empty);
				AssertEquals(7, propertysSetChangedCounter);
			}
			AssertEquals(8, propertysSetChangedCounter);

			container.AddCustomProperty("f10", string.Empty);
			container.AddCustomProperty("f11", string.Empty);
			container.AddCustomProperty("f12", string.Empty);
			AssertEquals(11, propertysSetChangedCounter);

			using (container.SuspendpropertysSetChanged())
			{
				container.ClearCustomProperties();
				AssertEquals(11, propertysSetChangedCounter);
			}
			AssertEquals(12, propertysSetChangedCounter);

			container.ClearCustomProperties();
			AssertEquals(12, propertysSetChangedCounter);
		}
	}
}
