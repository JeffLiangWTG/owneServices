using System;
using System.Collections.Generic;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.Design.TypeIntellisense.Testing
{
	[RequiresSoftware(RequiredSoftware.VisualStudio)]
	class IntellisenseDataSourceTest : TestCase
	{
		public void TestPopulate()
		{
			try
			{
				DoTestPopulate();
			}
			catch
			{
				DoTestPopulate();
			}
		}

		void DoTestPopulate()
		{
			List<Type> types = new List<Type>();
			types.Add(new MyFakeType(typeof(MyClass), "ns.type"));
			types.Add(new MyFakeType(typeof(MyInterface), "ns.type2"));
			types.Add(new MyFakeType(typeof(MyStruct), "ns.type3"));
			types.Add(new MyFakeType(typeof(MyDelegate), "ns.type4"));
			types.Add(new MyFakeType(typeof(MyEnum), "ns.type5"));
			types.Add(new MyFakeType(typeof(object), "nsx.type3"));
			types.Add(new MyFakeType(typeof(object), "ns.type+nested"));
			types.Add(new MyFakeType(typeof(object), "using.ns.afterusingns.usingtype"));
			types.Add(new MyFakeType(typeof(object), "using.ns.afterusingns+usingnested"));
			types.Add(new MyFakeType(typeof(object), "afterusingns.notreallyinusing"));
			using (IntellisenseDataSource source = new IntellisenseDataSource(types, null, new string[] { "using.ns" }))
			{
				for (int i = 0; i < 200; i++)
				{
					TestPopulate(source, new Random(i).Next(9));
				}
			}
		}

		class MyClass
		{
		}

		interface MyInterface
		{
		}

		struct MyStruct
		{
		}

		delegate void MyDelegate();
		enum MyEnum
		{
		}

		[Serializable]
		class MyFakeType : TypeNameHolder
		{
#if NETFRAMEWORK
			protected MyFakeType(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
			{
			}
#endif

			public MyFakeType(Type typeForIsSubclassOf, string fullName) : base(fullName)
			{
				this.typeForIsSubclassOf = typeForIsSubclassOf;
			}

			public override bool IsSubclassOf(Type c)
			{
				return typeForIsSubclassOf.IsSubclassOf(c);
			}

			readonly Type typeForIsSubclassOf;
		}

		void TestPopulate(IntellisenseDataSource source, int i)
		{
			if (i == 0)
			{
				source.PartialName = "";
				AssertEquals(4, source.Count);
				AssertEquals(new IntellisenseDataSourceItem("afterusingns", "afterusingns", IntellisenseItemType.Class), source[0]);
				AssertEquals(new IntellisenseDataSourceItem("ns", "ns", IntellisenseItemType.Namespace), source[1]);
				AssertEquals(new IntellisenseDataSourceItem("nsx", "nsx", IntellisenseItemType.Namespace), source[2]);
				AssertEquals(new IntellisenseDataSourceItem("using", "using", IntellisenseItemType.Class), source[3]);
			}

			if (i == 1)
			{
				source.PartialName = ".splaty";
				AssertEquals(4, source.Count);
				AssertEquals(new IntellisenseDataSourceItem("afterusingns", "afterusingns", IntellisenseItemType.Class), source[0]);
				AssertEquals(new IntellisenseDataSourceItem("ns", "ns", IntellisenseItemType.Namespace), source[1]);
				AssertEquals(new IntellisenseDataSourceItem("nsx", "nsx", IntellisenseItemType.Namespace), source[2]);
				AssertEquals(new IntellisenseDataSourceItem("using", "using", IntellisenseItemType.Class), source[3]);
			}

			if (i == 2)
			{
				source.PartialName = "ns.";
				AssertEquals(5, source.Count);
				AssertEquals(new IntellisenseDataSourceItem("type", "type", IntellisenseItemType.Class), source[0]);
				AssertEquals(new IntellisenseDataSourceItem("type2", "type2", IntellisenseItemType.Interface), source[1]);
				AssertEquals(new IntellisenseDataSourceItem("type3", "type3", IntellisenseItemType.Struct), source[2]);
				AssertEquals(new IntellisenseDataSourceItem("type4", "type4", IntellisenseItemType.Enum), source[3]);
				AssertEquals(new IntellisenseDataSourceItem("type5", "type5", IntellisenseItemType.Delegate), source[4]);
			}

			if (i == 3)
			{
				source.PartialName = "N";
				AssertEquals(4, source.Count);
				AssertEquals(new IntellisenseDataSourceItem("afterusingns", "afterusingns", IntellisenseItemType.Class), source[0]);
				AssertEquals(new IntellisenseDataSourceItem("ns", "ns", IntellisenseItemType.Namespace), source[1]);
				AssertEquals(new IntellisenseDataSourceItem("nsx", "nsx", IntellisenseItemType.Namespace), source[2]);
				AssertEquals(new IntellisenseDataSourceItem("using", "using", IntellisenseItemType.Class), source[3]);
			}

			if (i == 4)
			{
				source.PartialName = "ns.tyP";
				AssertEquals(5, source.Count);
				AssertEquals(new IntellisenseDataSourceItem("type", "type", IntellisenseItemType.Class), source[0]);
				AssertEquals(new IntellisenseDataSourceItem("type2", "type2", IntellisenseItemType.Interface), source[1]);
				AssertEquals(new IntellisenseDataSourceItem("type3", "type3", IntellisenseItemType.Struct), source[2]);
				AssertEquals(new IntellisenseDataSourceItem("type4", "type4", IntellisenseItemType.Enum), source[3]);
				AssertEquals(new IntellisenseDataSourceItem("type5", "type5", IntellisenseItemType.Delegate), source[4]);
			}

			if (i == 5)
			{
				source.PartialName = "nS.type.";
				AssertEquals(1, source.Count);
				AssertEquals(new IntellisenseDataSourceItem("nested", "nested", IntellisenseItemType.Class), source[0]);
			}

			if (i == 6)
			{
				source.PartialName = "Ns.tyPe.N";
				AssertEquals(1, source.Count);
				AssertEquals(new IntellisenseDataSourceItem("nested", "nested", IntellisenseItemType.Class), source[0]);
			}

			if (i == 7)
			{
				source.PartialName = "ns.type.splaty";
				AssertEquals(1, source.Count);
				AssertEquals(new IntellisenseDataSourceItem("nested", "nested", IntellisenseItemType.Class), source[0]);
			}

			if (i == 8)
			{
				source.PartialName = "afterusingns.usingt";
				AssertEquals(3, source.Count);
				AssertEquals(new IntellisenseDataSourceItem("notreallyinusing", "notreallyinusing", IntellisenseItemType.Namespace), source[0]);
				AssertEquals(new IntellisenseDataSourceItem("usingnested", "usingnested", IntellisenseItemType.Class), source[1]);
				AssertEquals(new IntellisenseDataSourceItem("usingtype", "usingtype", IntellisenseItemType.Class), source[2]);
			}
		}

		[ExpectNoExceptions]
		public void TestDisposeWhilePopulating()
		{
			IntellisenseDataSource source = new IntellisenseDataSource(GetType().Assembly.GetTypes(), null);
			source.PartialName = "System.";
			source.Dispose();
		}

		public void TestFindNearestMatch()
		{
			string bodgey_name = "type4[]'%%&^*&()_)@#!@";
			List<Type> types = new List<Type>();
			types.Add(new MyFakeType(typeof(object), "type"));
			types.Add(new MyFakeType(typeof(object), "type2"));
			types.Add(new MyFakeType(typeof(object), "type3"));
			types.Add(new MyFakeType(typeof(object), "type4"));
			types.Add(new MyFakeType(typeof(object), "type5"));
			types.Add(new MyFakeType(typeof(object), bodgey_name));
			using (IntellisenseDataSource source = new IntellisenseDataSource(types, null))
			{
				source.PartialName = "t";
				AssertEquals("Should find the selected item", "type", source[source.FindNearestMatch()].DisplayName);
				source.PartialName = "type4";
				AssertEquals("Should find the selected item", "type4", source[source.FindNearestMatch()].DisplayName);
				source.PartialName = bodgey_name;
				AssertEquals("Should find the selected item", bodgey_name, source[source.FindNearestMatch()].DisplayName);
				source.PartialName = "type3z";
				AssertEquals("Should find the selected item", "type3", source[source.FindNearestMatch()].DisplayName);
			}
		}
	}
}
