#if DEBUG
using System;
using System.Collections;
using CargoWise.ComponentModel.Design;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class ListValueMemberCompileTimeCheckProviderTests : TestCase
	{
		public void TestGetBindingMembersForCompileTimeCheck_WithListAsMember()
		{ TestGetBindingMembersForCompileTimeCheck(typeof(TestCompileTimeDataSource_WithListAsMember), false); }

		public void TestGetBindingMembersForCompileTimeCheck_WithListAsConstant()
		{ TestGetBindingMembersForCompileTimeCheck(typeof(TestCompileTimeDataSource_WithListAsConstant), true); }

		void TestGetBindingMembersForCompileTimeCheck(Type dataSourceType, bool expectBindingMemberNull)
		{
			ListValueMemberCompileTimeCheckProvider provider = new ListValueMemberCompileTimeCheckProvider();

			CompileTimeCheckBindingMemberCollection members = provider.GetBindingMembersForCompileTimeCheck(
				dataSourceType, "BoundValue");
			AssertEquals(dataSourceType, members[0].DataSourceType);
			AssertEquals(typeof(string), members[0].ControlPropertyType);
			if (expectBindingMemberNull)
			{
				AssertNull(members[0].BindingMember);
			}
			else
			{
				AssertEquals("BoundValue", members[0].BindingMember);
			}
		}

		public void TestGetBindingMembersForCompileTimeCheck_WithoutTypes()
		{
			ListValueMemberCompileTimeCheckProvider provider = new ListValueMemberCompileTimeCheckProvider();

			CompileTimeCheckBindingMemberCollection members;
			members = provider.GetBindingMembersForCompileTimeCheck(
				typeof(TestCompileTimeDataSource_WithListAsMember), "");
			AssertEquals(
				"Should have no length as there is no property to get the value member meta-data on",
				0, members.Count);
			members = provider.GetBindingMembersForCompileTimeCheck(
				typeof(TestCompileTimeDataSource_WithListAsMember), "splaty");
			AssertEquals(
				"Should have no length as there is no valid bound property",
				0, members.Count);
		}

		class TestCompileTimeDataSource_WithListAsMember
		{
			[MetaDataMember(MetaDataTypes.ListDataSource, "List")]
			public int BoundValue
			{ get { return 0; } }

			public TestItemCollection List
			{ get { return null; } }
		}

		class TestCompileTimeDataSource_WithListAsConstant
		{
			[MetaDataValue(MetaDataTypes.ListDataSource, typeof(TestCompileTimeDataSource_WithListAsConstant), "List")]
			public int BoundValue
			{ get { return 0; } }

			public static TestItemCollection List
			{ get { return new TestItemCollection(); } }
		}

		class TestItemCollection : ArrayList
		{
			public new TestItem this[int i]
			{ get { return null; } }
		}

		[ValueDisplayMembers("Value", "Display")]
		class TestItem
		{
			public TestItem(object display, string value)
			{
				this.display = display;
				this.value = value;
			}

			readonly object display;
			public object Display
			{ get { return display; } }

			readonly string value;
			public string Value
			{ get { return value; } }

			public override bool Equals(object obj)
			{ return Display == ((TestItem)obj).Display && Value == ((TestItem)obj).Value; }

			public override int GetHashCode()
			{ return Value.GetHashCode(); }
		}
	}
}
#endif
