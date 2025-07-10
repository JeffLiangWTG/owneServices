using System.ComponentModel;
using CargoWise.ComponentModel;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.ComponentModel.Testing
{
	sealed class MetadataAccessorTest : TestCase
	{
		public void TestGetListMemberReturnsValueSpecifiedOnDataMemberAttribute()
		{
			AssertEquals("ListMember", MetadataAccessor.GetListMember("", CreateDescriptor(), ""));
		}

		public void TestGetListMemberReturnsValueSpecifiedInDesignTimeIgnoringMetadata()
		{
			AssertEquals("List", MetadataAccessor.GetListMember("List", CreateDescriptor(), ""));
		}

		public void TestGetListMemberReturnsValueWithinFullPath()
		{
			AssertEquals("P1.P2.ListMember", MetadataAccessor.GetListMember("", CreateDescriptor(), "P1.P2.DataMember"));
			AssertEquals("ListMember", MetadataAccessor.GetListMember("", CreateDescriptor(), "P2+DataMember"));
			AssertEquals(".P.ListMember", MetadataAccessor.GetListMember("", CreateDescriptor(), ".P.DataMember"));
			AssertEquals("ListMember", MetadataAccessor.GetListMember("", CreateDescriptor(), "DataMember"));
		}

		#region Test Classes

		class TestObject
		{
			[List("ListMember")]
			public string DataMember
			{
				get { return ""; }
			}

			public TestCollection ListMember
			{
				get { return new TestDescendantCollection(); }
			}
		}

		class TestCollection { }
		class TestDescendantCollection : TestCollection { }

		#endregion

		#region Implementation

		static PropertyDescriptor CreateDescriptor()
		{
			return TypeDescriptor.CreateProperty(typeof(TestObject), "DataMember", typeof(string), null);
		}

		#endregion
	}
}
