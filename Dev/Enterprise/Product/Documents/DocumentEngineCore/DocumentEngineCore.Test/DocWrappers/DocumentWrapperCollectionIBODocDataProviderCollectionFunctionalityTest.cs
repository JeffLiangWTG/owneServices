using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.DocumentEngineCore.DocWrappers.Testing
{
	sealed class DocumentWrapperCollectionIBODocDataProviderCollectionFunctionalityTest : TestCaseWithFactory
	{
		public void TestIntBasedIndexer()
		{
			var boCollection = new TestBOCollection(Factory);
			TestBO bo1 = boCollection.AddNew();
			TestBO bo2 = boCollection.AddNew();
			TestBO bo3 = boCollection.AddNew();

			IBODocDataProviderCollection docCollection = boCollection;
			AssertExceptionThrown(typeof(ArgumentOutOfRangeException), delegate
			{ IBODocDataProvider dataProvider = docCollection[-1]; });
			AssertEquals(bo1, docCollection[0]);
			AssertEquals(bo2, docCollection[1]);
			AssertEquals(bo3, docCollection[2]);
			AssertExceptionThrown(typeof(ArgumentOutOfRangeException), delegate
			{ IBODocDataProvider dataProvider = docCollection[3]; });
		}

		public void TestStringBasedIndexer()
		{
			var boCollection = new TestBOCollection(Factory);
			TestBO bo1 = boCollection.AddNew();
			TestBO bo2 = boCollection.AddNew();
			TestBO bo3 = boCollection.AddNew();

			IBODocDataProviderCollection docCollection = boCollection;
			AssertEquals(null, docCollection["0"]);
			AssertEquals(bo1, docCollection["1"]);
			AssertEquals(bo2, docCollection["2"]);
			AssertEquals(bo3, docCollection["3"]);
			AssertEquals(null, docCollection["4"]);

			AssertEquals("docCollection[1]", bo1, docCollection["1"]);
			AssertEquals("docCollection[2]", bo2, docCollection["2"]);
			AssertEquals("docCollection[3]", bo3, docCollection["3"]);
			AssertEquals("docCollection[4]", null, docCollection["4"]);

			AssertEquals("docCollection[count]", bo3, docCollection["count"]);
			AssertEquals("docCollection[count-1]", bo2, docCollection["count-1"]);
			AssertEquals("docCollection[count-2]", bo1, docCollection["count-2"]);
			AssertEquals("docCollection[count-3]", null, docCollection["count-3"]);

			AssertEquals("docCollection[count - 1]  (spaces should be ignored) ", bo2, docCollection["count - 1"]);
			AssertEquals("docCollection[COUNT]      (case should be ignored)   ", bo3, docCollection["COUNT"]);

			AssertEquals("docCollection[first]", bo1, docCollection["first"]);
			AssertEquals("docCollection[last]", bo3, docCollection["last"]);

			AssertEquals("docCollection[AnyThingIDon'tRecognise]", null, docCollection["AnyWordIDon'tRecognise"]);
		}

		public void TestTotalForZInt()
		{
			var boCollection = new TestBOCollection(Factory);
			boCollection.AddNew().IntField = 1;
			boCollection.AddNew().IntField = 2;
			boCollection.AddNew().IntField = 3;

			IBODocDataProviderCollection docCollection = boCollection;
			AssertEquals(6, docCollection.Total("IntField", null, null));
			AssertEquals(6, docCollection.Total("IntField", "", null));
			AssertEquals(6, docCollection.Total("IntField", "1", null));
			AssertEquals(6, docCollection.Total("IntField", "A", null));
		}

		public void TestTotalForZDecimal()
		{
			var boCollection = new TestBOCollection(Factory);
			boCollection.AddNew().DecimalField = 1.11m;
			boCollection.AddNew().DecimalField = 2.02m;
			boCollection.AddNew().DecimalField = 3.01m;

			IBODocDataProviderCollection docCollection = boCollection;
			AssertEquals(6.14m, docCollection.Total("DecimalField", null, null));
			AssertEquals(6.14m, docCollection.Total("DecimalField", "", null));
			AssertEquals(6m, docCollection.Total("DecimalField", "0", null));
			AssertEquals(6.1m, docCollection.Total("DecimalField", "1", null));
			AssertEquals(6.14m, docCollection.Total("DecimalField", "2", null));
			AssertEquals(6.140m, docCollection.Total("DecimalField", "3", null));
			AssertEquals(6.14m, docCollection.Total("DecimalField", "A", null));
		}

		public void TestTotalForITotalValueAndUnits()
		{
			var boCollection = new TestBOCollection(Factory);
			TestBO bO1 = boCollection.AddNew();
			bO1.Weight.Value = 1.11m;
			bO1.Weight.Unit = "KGS";
			TestBO bO2 = boCollection.AddNew();
			bO2.Weight.Value = 2.02m;
			bO2.Weight.Unit = "KGS";
			TestBO bO3 = boCollection.AddNew();
			bO3.Weight.Value = 3.01m;
			bO3.Weight.Unit = "KGS";

			IBODocDataProviderCollection docCollection = boCollection;
			AssertEquals("6.14 KGS", docCollection.Total("Weight", "2", null));
		}

		public void TestTotalWithUnrecognisedType()
		{
			var boCollection = new TestBOCollection(Factory);
			boCollection.AddNew().TextField = "";
			boCollection.AddNew().TextField = "ONE";
			boCollection.AddNew().TextField = "TWO";

			IBODocDataProviderCollection docCollection = boCollection;
			AssertEquals("", docCollection.Total("TextField", null, null));
			AssertEquals("", docCollection.Total("TextField", "", null));
			AssertEquals("", docCollection.Total("TextField", "1", null));
			AssertEquals("", docCollection.Total("TextField", "A", null));
		}

		public void TestTotalWithFilter()
		{
			var boCollection = new TestBOCollection(Factory);
			boCollection.AddNew().IntField = 1;
			boCollection.AddNew().IntField = 2;
			boCollection.AddNew().IntField = 3;

			IBODocDataProviderCollection docCollection = boCollection;
			AssertEquals(1, docCollection.Total("IntField", null, "\"{IntField}\" == 1"));
			AssertEquals(4, docCollection.Total("IntField", "", "\"{IntField}\" != 2"));
			AssertEquals(6, docCollection.Total("IntField", "A", "\"{IntField}\" != 4"));
		}

		public void TestFormatMethodWithTwoParameters()
		{
			var boCollection = new TestBOCollection(Factory);
			boCollection.AddNew().TextField = "";
			boCollection.AddNew().TextField = "ONE";
			boCollection.AddNew().TextField = "TWO";

			IBODocDataProviderCollection docCollection = boCollection;
			AssertEquals("ONE : TWO", docCollection.Format("{TextField}", "Colon", ZString.Empty, ZString.Empty, 0));
			AssertEquals("ONE, TWO", docCollection.Format("{TextField}", "Comma", ZString.Empty, ZString.Empty, 0));
			AssertEquals("ONE - TWO", docCollection.Format("{TextField}", "Dash", ZString.Empty, ZString.Empty, 0));
			AssertEquals("ONE\r\nTWO", docCollection.Format("{TextField}", "NewLine", ZString.Empty, ZString.Empty, 0));
			AssertEquals("ONE TWO", docCollection.Format("{TextField}", "Space", ZString.Empty, ZString.Empty, 0));
		}

		public void TestFormatMethodWithThreeParameters()
		{
			var boCollection = new TestBOCollection(Factory);
			boCollection.AddNew().TextField = "";
			boCollection.AddNew().TextField = "ONE";
			boCollection.AddNew().TextField = "ONE";
			boCollection.AddNew().TextField = "TWO";

			IBODocDataProviderCollectionHelper docCollection = (IBODocDataProviderCollectionHelper)Activator.CreateInstance(ObjectFactory.GetType<IBODocDataProviderCollectionHelper>(), new object[] { boCollection });
			AssertEquals("1 x  : 2 x ONE : 1 x TWO", docCollection.Format("{Count} x {TextField}", "Colon", ZString.Empty, "\"{TextField}\"", 0));
			AssertEquals("1 x , 2 x ONE, 1 x TWO", docCollection.Format("{Count} x {TextField}", "Comma", ZString.Empty, "\"{TextField}\"", 0));
			AssertEquals("1 x  - 2 x ONE - 1 x TWO", docCollection.Format("{Count} x {TextField}", "Dash", ZString.Empty, "\"{TextField}\"", 0));
			AssertEquals("1 x \r\n2 x ONE\r\n1 x TWO", docCollection.Format("{Count} x {TextField}", "NewLine", ZString.Empty, "\"{TextField}\"", 0));
			AssertEquals("1 x  2 x ONE 1 x TWO", docCollection.Format("{Count} x {TextField}", "Space", ZString.Empty, "\"{TextField}\"", 0));
		}

		public void TestFormatMethodWithFourParameters()
		{
			var boCollection = new TestBOCollection(Factory);
			boCollection.AddNew().TextField = "";
			boCollection.AddNew().TextField = "ONE";
			boCollection.AddNew().TextField = "ONE";
			boCollection.AddNew().TextField = "TWO";

			var docCollection = (IBODocDataProviderCollectionHelper)Activator.CreateInstance(ObjectFactory.GetType<IBODocDataProviderCollectionHelper>(), new object[] { boCollection });
			AssertEquals("1 x , 2 x ONE, 1 x TWO", docCollection.Format("{Count} x {TextField}", "Comma", ZString.Empty, "\"{TextField}\"", -1));
			AssertEquals("1 x , 2 x ONE, 1 x TWO", docCollection.Format("{Count} x {TextField}", "Comma", ZString.Empty, "\"{TextField}\"", 0));
			AssertEquals("1 x ", docCollection.Format("{Count} x {TextField}", "Comma", ZString.Empty, "\"{TextField}\"", 1));
			AssertEquals("1 x , 2 x ONE", docCollection.Format("{Count} x {TextField}", "Comma", ZString.Empty, "\"{TextField}\"", 2));
			AssertEquals("1 x , 2 x ONE, 1 x TWO", docCollection.Format("{Count} x {TextField}", "Comma", ZString.Empty, "\"{TextField}\"", 3));
			AssertEquals("1 x , 2 x ONE, 1 x TWO", docCollection.Format("{Count} x {TextField}", "Comma", ZString.Empty, "\"{TextField}\"", 10));

			AssertEquals("ONE, ONE, TWO", docCollection.Format("{TextField}", "Comma", ZString.Empty, ZString.Empty, -1));
			AssertEquals("ONE, ONE, TWO", docCollection.Format("{TextField}", "Comma", ZString.Empty, ZString.Empty, 0));
			AssertEquals("ONE", docCollection.Format("{TextField}", "Comma", ZString.Empty, ZString.Empty, 1));
			AssertEquals("ONE, ONE", docCollection.Format("{TextField}", "Comma", ZString.Empty, ZString.Empty, 2));
			AssertEquals("ONE, ONE, TWO", docCollection.Format("{TextField}", "Comma", ZString.Empty, ZString.Empty, 3));
			AssertEquals("ONE, ONE, TWO", docCollection.Format("{TextField}", "Comma", ZString.Empty, ZString.Empty, 10));
		}

		public void TestEnumerationModificationMessage()
		{
			AssertExceptionThrown<InvalidOperationException>("Modifying while enumerating should throw an exception", "Collection was modified; enumeration operation may not execute.", () =>
			{
				var boCollection = new TestBOCollection(Factory);
				boCollection.AddNew();
				foreach (BusinessObject bizO in boCollection)
				{
					boCollection.AddNew();
				}
			});

			AssertStartsWith("The only reported key should be:", "EnumerationCockUp_TestBOCollection", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		#region Test classes

		class WeightBusinessObject : ITotalValueAndUnits
		{
			public ZDecimal Value
			{
				get { return value; }
				set { this.value = value; }
			}
			ZDecimal value;

			public ZString Unit
			{
				get { return unit; }
				set { unit = value; }
			}
			ZString unit;

			#region ITotalValueAndUnits Members

			public ValueAndUnitSelfTotaller GetNewForTotalling()
			{
				return new ValueAndUnitSelfTotaller(Value, Unit);
			}

			public void AddSelfToResult(ValueAndUnitSelfTotaller result)
			{
				result.Value += Value;
			}

			#endregion
		}

		class TestBO : DocumentWrapper
		{
			public ZString TextField
			{
				get { return textField; }
				set { textField = value; }
			}
			ZString textField;

			public ZInt IntField
			{
				get { return intField; }
				set { intField = value; }
			}
			ZInt intField;

			public ZDecimal DecimalField
			{
				get { return decimalField; }
				set { decimalField = value; }
			}
			ZDecimal decimalField;

			public WeightBusinessObject Weight
			{
				get { return weight ?? (weight = new WeightBusinessObject()); }
			}
			WeightBusinessObject weight;
		}

		class TestBOCollection : DocumentWrapperCollection<TestBO>
		{
			public TestBOCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				return new TestBO();
			}
		}
		#endregion
	}
}
