using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	class ObjectPrintTest : TestCase
	{
		public void TestRecurse()
		{
			var recurse = new Recursive();
			var result = recurse.Print();
			AssertContains("A Property", result);
		}

		public void TestNull()
		{
			string a = null;
			var result = a.Print();
			AssertContains("null", result);
			var hasNull = new HasNull();
			result = hasNull.Print();
			AssertContains("null", result);
		}

		public void TestArray()
		{
			var test = new ATest();
			var result = test.Array.Print();
			AssertEquals("{ ( B: 41, A: Bs A ), ( B: 41, A: Bs A ), ( B: 41, A: Bs A ) }", result);
		}

		public void TestTypedEnumeration()
		{
			var test = new ATest();
			var result = test.TypedEnumerable.Print();
			AssertEquals("{ ( B: 41, A: Bs A ), ( B: 41, A: Bs A ), ( B: 41, A: Bs A ) }", result);
		}

		public void TestUntypedEnumeration()
		{
			var test = new ATest();
			var result = test.UntypedEnumerable.Print();
			AssertEquals("{ ( 1 ), ( 2 ), ( 3 ) }", result);
		}

		public void TestCustomEnumeration()
		{
			var test = new CustomEnumeration();
			AssertEquals("{ ( a ), ( b ), ( c ), ( d ) }", test.Print());
		}

		public void TestCustomEnumerationWithProperty()
		{
			var test = new CustomEnumerationWithProperty();
			AssertEquals("AProperty: My Enum Prop", test.Print());
			AssertEquals("{ ( a ), ( b ), ( c ), ( d ) }", ((IEnumerable<string>)test).Print());
		}

		public void TestArrayOnly()
		{
			var array = new[] { 1, 2, 3 };
			AssertEquals("{ ( 1 ), ( 2 ), ( 3 ) }", array.Print());
		}

		public void TestDictionary()
		{
			var test = new ATest();
			var result = test.Dictionary.Print();
			AssertEquals("{ ( Key: A, Value: 1 ), ( Key: B, Value: 2 ), ( Key: C, Value: 3 ) }", result);
		}

		public void TestNestedEnumeration()
		{
			var test = new ATest();
			var result = test.NestNest.Print();
			AssertEquals("{ ( { ( Key: A, Value: 1 ), ( Key: B, Value: 2 ) } ), ( { ( Key: C, Value: 3 ), ( Key: D, Value: 4 ) } ) }", result);
		}

		public void TestMix()
		{
			var test = new ATest();
			var result = test.Print();
			AssertEquals("A: Some A, B: 42, C: 123.123456, CB ( B: 41, A: Bs A ), Array { ( B: 41, A: Bs A ), ( B: 41, A: Bs A ), ( B: 41, A: Bs A ) }, TypedEnumerable { ( B: 41, A: Bs A ), ( B: 41, A: Bs A ), ( B: 41, A: Bs A ) }, UntypedEnumerable { ( 1 ), ( 2 ), ( 3 ) }, Dictionary { ( Key: A, Value: 1 ), ( Key: B, Value: 2 ), ( Key: C, Value: 3 ) }, NestNest { ( { ( Key: A, Value: 1 ), ( Key: B, Value: 2 ) } ), ( { ( Key: C, Value: 3 ), ( Key: D, Value: 4 ) } ) }, MyEnumerable { ( a ), ( b ), ( c ), ( d ) }, List { ( 1 ), ( 2 ), ( 3 ) }, UntypedList { ( 1 ), ( 2 ), ( 3 ) }", result);
		}

		public void TestOnlyPrintSpecifiedTypeAndInherited()
		{
			var test = (IB)new ATest();
			var result = test.Print();
			AssertEquals("B: 42, A: Some A", result);
		}

		public void TestFilterNothingAllowed()
		{
			var test = new ATest();
			var result = test.Print((type, propertyName) => true);
			AssertEquals(string.Empty, result);
		}

		public void TestFilterExcludeType()
		{
			var test = new ATest();
			var result = test.Print((type, propertyName) => type == typeof(string));
			AssertEquals("B: 42, C: 123.123456, CB ( B: 41 ), Array { ( B: 41 ), ( B: 41 ), ( B: 41 ) }, TypedEnumerable { ( B: 41 ), ( B: 41 ), ( B: 41 ) }, UntypedEnumerable { ( 1 ), ( 2 ), ( 3 ) }, Dictionary { ( Value: 1 ), ( Value: 2 ), ( Value: 3 ) }, NestNest { ( { ( Value: 1 ), ( Value: 2 ) } ), ( { ( Value: 3 ), ( Value: 4 ) } ) }, MyEnumerable { (  ), (  ), (  ), (  ) }, List { ( 1 ), ( 2 ), ( 3 ) }, UntypedList { ( 1 ), ( 2 ), ( 3 ) }", result);
		}

		public void TestFilterExcludeProperty()
		{
			var test = new ATest();
			var result = test.Print((type, propertyName) => (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string)) || propertyName == "CB");
			AssertEquals("A: Some A, B: 42, C: 123.123456", result);
		}

		public void TestIgnoreCurrentCulture()
		{
			var oldCulture = Thread.CurrentThread.CurrentCulture;
			try
			{
				double val = 5.2345;
				Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
				AssertEquals("5,2345", val.ToString());
				AssertEquals("5.2345", val.Print());
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = oldCulture;
			}
		}

		public void TestTuple()
		{
			AssertEquals("(55, 5, 25.5)", (Thing1: "55", Thing2: 5, Thing3: 25.5).Print());
		}

		public void TestHiding()
		{
			AssertEquals("A: 6, B: 5", new DerivedA().Print());
		}

		public void TestNestedPropertySameName()
		{
			var nameClash = new NameClash();
			AssertEquals("CargoWise.Common.Testing.NameClashA.INameClash.NameClash: NameClash A, CargoWise.Common.Testing.NameClashB.INameClash.NameClash: NameClash B, CargoWise.Common.Testing.NameClashC.INameClash.NameClash: NameClash C, CargoWise.Common.Testing.A.B.C.D.INameClash.NameClash: NameClash A.B.C.D, CargoWise.Common.Testing.B.C.D.INameClash.NameClash: NameClash B.C.D, CargoWise.Common.Testing.C.D.INameClash.NameClash: NameClash C.D", nameClash.Print());
		}

		public void TestCriticalException()
		{
			var test = new ATest();
			AssertNoExceptionThrown(() => test.Visit(new ExceptionThrowingVisitor(new Exception())));
			AssertExceptionThrown<OutOfMemoryException>(() => test.Visit(new ExceptionThrowingVisitor(new OutOfMemoryException())));
		}

		class ExceptionThrowingVisitor : IPropertyVisitor
		{
			readonly Exception exceptionToThrow;
			public ExceptionThrowingVisitor(Exception exceptionToThrow)
			{
				this.exceptionToThrow = exceptionToThrow;
			}

			public bool ShouldVisit(PropertyVisitType propertyVisitType, Type type, string typeFullName, string propertyFullName, string propertyName)
			{
				return true;
			}

			public bool VisitEnumerable<T>(string propertyName, IEnumerable<T> enumerable, PropertyVisitState state)
			{
				return true;
			}

			public bool VisitEnumerable(string propertyName, IEnumerable enumerable, PropertyVisitState state)
			{
				return true;
			}

			public void VisitLeaf<T>(string propertyName, T item)
			{
				if (exceptionToThrow != null)
				{
					throw exceptionToThrow;
				}
			}

			public bool VisitPropertyBag<T>(string propertyName, T item, PropertyVisitState state)
			{
				return true;
			}
		}
	}
}