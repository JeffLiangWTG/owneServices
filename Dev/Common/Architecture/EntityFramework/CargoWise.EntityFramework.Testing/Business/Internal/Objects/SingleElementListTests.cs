using System;
using System.Collections;
using System.ComponentModel;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class SingleElementListTests : TestCase
	{
		class NonAbstractSingleElementList : NonPersistentBusinessObject
		{
		}

		public void TestICollection_CopyTo_ShouldNotExplode()
		{
			var bizo = new NonAbstractSingleElementList();
			AssertNoExceptionThrown("CopyTo is called by WPF during binding", () => ((ICollection)bizo).CopyTo(new NonAbstractSingleElementList[1], 0));
		}

		public void TestOnListChangedException()
		{
			NonAbstractSingleElementList elementList = new NonAbstractSingleElementList();
			elementList.ListChangedInternal += new ListChangedEventHandler(ElementList_ListChangedInternal);
			try
			{
				elementList.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, 0));
			}
			catch (Exception e)
			{
				Assert(e.Source.IndexOf(GetType().FullName) > -1);
			}
		}

		public void TestOnListChangedIgnoreNullReferenceException()
		{
			var elementList = new NonAbstractSingleElementList();
			elementList.ListChangedInternal += new ListChangedEventHandler(ElementList_ListChangedInternalThrowNullReferenceException);
			AssertNoExceptionThrown(() => elementList.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, 0)));
		}

		public void TestGetEnumerator()
		{
			NonAbstractSingleElementList elementList = new NonAbstractSingleElementList();
			IEnumerator enumerator = ((IEnumerable)elementList).GetEnumerator();
			Assert(enumerator.MoveNext());
			AssertEquals("Should not throw exception and should return itself", elementList, enumerator.Current);
			Assert(!enumerator.MoveNext());
		}

		void ElementList_ListChangedInternal(object sender, ListChangedEventArgs e)
		{
			throw new Exception("This is a dummy exception");
		}

		void ElementList_ListChangedInternalThrowNullReferenceException(object sender, ListChangedEventArgs e)
		{
			throw new NullReferenceException("This is a dummy null reference exception");
		}
	}
}
