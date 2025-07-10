using System;
using System.Collections;
using System.ComponentModel;
using NUnit.Framework;

namespace CargoWise.Common.Collections
{
	internal class ListUtilTest : TestCase
	{
		#region GetListElementType
		public void TestGetListElementType_ForArray()
		{
			AssertEquals(typeof(int), ListUtil.GetListElementType(typeof(int[])));
		}

		public void TestGetListElementType_ForListWithItem()
		{
			ArrayList list = new ArrayList();
			list.Add(0);
			AssertEquals(typeof(int), ListUtil.GetListElementType(list));
		}

		public void TestGetListElementType_ForITypedList()
		{
			TypedListImpl list = new TypedListImpl();
			AssertEquals(typeof(TypedListImpl.MyListElement), ListUtil.GetListElementType(list));
		}

		public void TestGetListElementType_ForIndexedList()
		{
			AssertEquals(typeof(String), ListUtil.GetListElementType(typeof(IndexedList)));
		}

		public void TestGetListElementType_ForInterfaces()
		{
			AssertEquals(typeof(TestBo), ListUtil.GetListElementType(typeof(IInterface1<TestBo>)));
			AssertEquals(typeof(string), ListUtil.GetListElementType(typeof(IInterface3<TestBo>)));
			AssertNull(ListUtil.GetListElementType(typeof(IInterface4<TestBo>)));
		}

		public void TestGetListElementType_ForInstanceOfIndexedList()
		{
			IndexedList list = new IndexedList();
			AssertEquals(typeof(String), ListUtil.GetListElementType(list));
		}

		// This should return the ITypedList version of the element types.
		public void TestGetListElementType_WithITypedListAndIndexerWithElements()
		{
			TypedListWithIndexerAndElements list = new TypedListWithIndexerAndElements();
			AssertEquals(typeof(TypedListImpl.MyListElement), ListUtil.GetListElementType(list));
		}

		#endregion
		#region Test Classes
		class TypedListImpl : CollectionBase, ITypedList
		{
			#region ITypedList Members
			public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
			{
				return TypeDescriptor.GetProperties(typeof(MyListElement));
			}

			public string GetListName(PropertyDescriptor[] listAccessors)
			{
				return "";
			}

			#endregion
			[Serializable]
			public class MyListElement : Exception
			{
				public MyListElement(string message) : base(message)
				{
				}

#if NETFRAMEWORK
				protected MyListElement(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
				{
				}
#endif

				public int Prop
				{
					get
					{
						return 0;
					}
				}
			}
		}

		class TestBo
		{
		}

		interface IInterface4<T> : IInterface1<T>, IInterface2<T>
			where T : TestBo
		{
		}

		interface IInterface3<T> : IInterface2<T>, IList
			where T : TestBo
		{
		}

		interface IInterface2<T>
			where T : TestBo
		{
			string this[int i] { get; }
		}

		interface IInterface1<T>
			where T : TestBo
		{
			T this[int i] { get; }
		}

		class TypedListWithIndexerAndElements : TypedListImpl
		{
			public TypedListWithIndexerAndElements()
			{
				List.Add(new ArrayList());
			}

			public String this[int i]
			{
				get
				{
					return "";
				}
			}
		}

		class IndexedListBase : CollectionBase
		{
			public DateTime this[int i]
			{
				get
				{
					return DateTime.MinValue;
				}
			}
		}

		class IndexedList : IndexedListBase
		{
			public new String this[int i]
			{
				get
				{
					return "";
				}
			}
		}
		#endregion
	}
}
