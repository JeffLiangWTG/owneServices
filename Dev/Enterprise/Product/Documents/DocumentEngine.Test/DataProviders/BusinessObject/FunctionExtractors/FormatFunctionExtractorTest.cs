using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Testing
{
	sealed class FormatFunctionExtractorTest : Core.Testing.BaseFunctionExtractorTest
	{
		public override void TestGetMethodInfoChainLink()
		{
			var collection = new MyWrapperCollection();
			var wrapper1 = new MyWrapper();
			collection.Add(wrapper1);
			var wrapper2 = new MyWrapper();
			collection.Add(wrapper2);
			FormatFunctionExtractor formatExtractor = FormatFunctionExtractor.ParseAndExtract("Format(\"{Smells}\")");

			MethodInfoChainLink chainLink = formatExtractor.GetMethodInfoChainLink(typeof(MyWrapperCollection));
			AssertEquals("chainLink.ReflectOutObject(0, collection, collection).ToString())", "Bad, Bad", chainLink.ReflectOutObject(collection, collection).ToString());

			chainLink = formatExtractor.GetMethodInfoChainLink(typeof(MyWrapper));
			AssertEquals("chainLink.ReflectOutObject(0, collection, collection).ToString())", "Bad", chainLink.ReflectOutObject(wrapper1, wrapper1).ToString());

			formatExtractor = FormatFunctionExtractor.ParseAndExtract("Format(\"{Smells}\", Comma, \"\", \"{Smells}\")");

			chainLink = formatExtractor.GetMethodInfoChainLink(typeof(MyWrapperCollection));
			AssertEquals("chainLink.ReflectOutObject(0, collection, collection).ToString())", "Bad", chainLink.ReflectOutObject(collection, collection).ToString());

			chainLink = formatExtractor.GetMethodInfoChainLink(typeof(MyWrapper));
			AssertEquals("chainLink.ReflectOutObject(0, collection, collection).ToString())", "Bad", chainLink.ReflectOutObject(wrapper1, wrapper1).ToString());

			formatExtractor = FormatFunctionExtractor.ParseAndExtract("Format(\"{Smells}, {Smells}, {Smells}\", Comma, \"\", \"{Smells}\")");

			chainLink = formatExtractor.GetMethodInfoChainLink(typeof(MyWrapperCollection));
			AssertEquals("chainLink.ReflectOutObject(0, collection, collection).ToString())", "Bad, Bad, Bad", chainLink.ReflectOutObject(collection, collection).ToString());

			formatExtractor = FormatFunctionExtractor.ParseAndExtract("Format(\"{Smells}, {Smells}, {Smells}\", Comma, \"\", \"\", 1)");

			chainLink = formatExtractor.GetMethodInfoChainLink(typeof(MyWrapperCollection));
			AssertEquals("chainLink.ReflectOutObject(0, collection, collection).ToString())", "Bad, Bad, Bad", chainLink.ReflectOutObject(collection, collection).ToString());
		}

		public void TestGetMethodInfoChainLinkForIBusinessObjectCollection()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			var bizObj1 = Factory.New<DummyBusinessObject>();
			bizObj1.Z0_Description = "Bad 1";
			collection.Add(bizObj1);
			var bizObj2 = Factory.New<DummyBusinessObject>();
			bizObj2.Z0_Description = "Bad 2";
			collection.Add(bizObj2);
			FormatFunctionExtractor formatExtractor = FormatFunctionExtractor.ParseAndExtract("Format(\"{Z0_Description}\")");

			MethodInfoChainLink chainLink = formatExtractor.GetMethodInfoChainLink(typeof(DummyBusinessObjectCollection));
			AssertEquals("chainLink.ReflectOutObject(0, collection, collection).ToString())", "Bad 1, Bad 2", chainLink.ReflectOutObject(collection, collection).ToString());
		}

		class MyWrapper : DocumentWrapper
		{
			public ZString Smells
			{
				get { return "Bad"; }
			}
		}

		class MyWrapperCollection : DocumentWrapperCollection<MyWrapper>
		{
			public MyWrapperCollection()
				: base(null)
			{
			}
		}

		public void TestConstructorWithOneStringAndTwoParameters()
		{
			FormatFunctionExtractor formatExtractor = FormatFunctionExtractor.ParseAndExtract("<Format(\"{ROFLCopters} - Rotor Size: {RotorSize:N2}\")>");
			AssertEquals("ROFLCopters - Rotor Size: RotorSize", formatExtractor.PropertyLabel);
			AssertEquals("{ROFLCopters} - Rotor Size: {RotorSize:N2}", formatExtractor.FormatString);
			AssertEquals(string.Empty, formatExtractor.Delimiter);
			AssertEquals("", formatExtractor.FilterString);
			AssertEquals(string.Empty, formatExtractor.GroupByParameters);
		}

		public void TestConstructorWithFilterString()
		{
			FormatFunctionExtractor formatExtractor = FormatFunctionExtractor.ParseAndExtract("<Format(\"{ROFLCOPTERS}\", NewLine, \"{FILTERDATA}\")>");
			AssertEquals("ROFLCOPTERS", formatExtractor.PropertyLabel);
			AssertEquals("{ROFLCOPTERS}", formatExtractor.FormatString);
			AssertEquals("NewLine", formatExtractor.Delimiter);
			AssertEquals("{FILTERDATA}", formatExtractor.FilterString);
			AssertEquals("", formatExtractor.GroupByParameters);

			formatExtractor = FormatFunctionExtractor.ParseAndExtract("<Format(\"{ROFLCOPTERS}\", NewLine, \"{FILTERDATA}\" == \"aaa\")>");
			AssertEquals("ROFLCOPTERS", formatExtractor.PropertyLabel);
			AssertEquals("{ROFLCOPTERS}", formatExtractor.FormatString);
			AssertEquals("NewLine", formatExtractor.Delimiter);
			AssertEquals("\"{FILTERDATA}\" == \"aaa\"", formatExtractor.FilterString);
			AssertEquals("", formatExtractor.GroupByParameters);

			formatExtractor = FormatFunctionExtractor.ParseAndExtract("<Format(\"{ROFLCOPTERS}\", NewLine, \"\")>");
			AssertEquals("ROFLCOPTERS", formatExtractor.PropertyLabel);
			AssertEquals("{ROFLCOPTERS}", formatExtractor.FormatString);
			AssertEquals("NewLine", formatExtractor.Delimiter);
			AssertEquals("", formatExtractor.FilterString);
			AssertEquals("", formatExtractor.GroupByParameters);
		}

		public void TestConstructorWithOneStringAndGroupByClause()
		{
			FormatFunctionExtractor formatExtractor = FormatFunctionExtractor.ParseAndExtract("<Format(\"{ROFLCopters} - Rotor Size: {RotorSize:N2}\", NewLine, \"\", \"x\")>");
			AssertEquals("ROFLCopters - Rotor Size: RotorSize", formatExtractor.PropertyLabel);
			AssertEquals("{ROFLCopters} - Rotor Size: {RotorSize:N2}", formatExtractor.FormatString);
			AssertEquals("NewLine", formatExtractor.Delimiter);
			AssertEquals("", formatExtractor.FilterString);
			AssertEquals("x", formatExtractor.GroupByParameters);
		}

		public void TestConstructorWithOneEmptyString()
		{
			FormatFunctionExtractor formatExtractor = FormatFunctionExtractor.ParseAndExtract(ZString.Empty);
			AssertNull(formatExtractor);
		}

		public void TestFormatWithFilter()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			var bizObj1 = Factory.New<DummyBusinessObject>();
			bizObj1.Z0_Description = "Bad 1";
			bizObj1.Z0_Number = 4;
			collection.Add(bizObj1);
			var bizObj2 = Factory.New<DummyBusinessObject>();
			bizObj2.Z0_Description = "Bad 2";
			bizObj2.Z0_Number = 2;
			collection.Add(bizObj2);
			var bizObj3 = Factory.New<DummyBusinessObject>();
			bizObj3.Z0_Description = "Bad 3";
			bizObj3.Z0_Number = 6;
			collection.Add(bizObj3);
			FormatFunctionExtractor formatExtractor = FormatFunctionExtractor.ParseAndExtract("Format(\"{Z0_Description}\")");
			MethodInfoChainLink chainLink = formatExtractor.GetMethodInfoChainLink(typeof(DummyBusinessObjectCollection));
			AssertEquals("chainLink.ReflectOutObject(0, collection, collection).ToString())", "Bad 1, Bad 2, Bad 3", chainLink.ReflectOutObject(collection, collection).ToString());

			formatExtractor = FormatFunctionExtractor.ParseAndExtract("Format(\"{Z0_Description}\", Comma, {Z0_Number} > 3)");
			chainLink = formatExtractor.GetMethodInfoChainLink(typeof(DummyBusinessObjectCollection));
			AssertEquals("chainLink.ReflectOutObject(0, collection, collection).ToString())", "Bad 1, Bad 3", chainLink.ReflectOutObject(collection, collection).ToString());
		}

		public void TestBraces()
		{
			var collection = new DummyBusinessObjectCollection(Factory);

			var bizObj1 = Factory.New<DummyBusinessObject>();
			bizObj1.Z0_Description = "biz1";
			bizObj1.Z0_Number = 1;
			bizObj1.Z0_AnotherDecimal = 1.1m;
			collection.Add(bizObj1);

			var bizObj2 = Factory.New<DummyBusinessObject>();
			bizObj2.Z0_Description = "biz2";
			bizObj2.Z0_Number = 2;
			bizObj2.Z0_AnotherDecimal = 4.4m;

			collection.Add(bizObj2);
			var bizObj3 = Factory.New<DummyBusinessObject>();
			bizObj3.Z0_Description = "biz3";
			bizObj3.Z0_Number = 3;
			bizObj3.Z0_AnotherDecimal = 6.6m;
			collection.Add(bizObj3);

			FormatFunctionExtractor formatExtractor = FormatFunctionExtractor.ParseAndExtract("Format(\"{Z0_Description} ({Z0_Number}) ({Z0_AnotherDecimal})\", Comma)");
			MethodInfoChainLink chainLink = formatExtractor.GetMethodInfoChainLink(typeof(DummyBusinessObjectCollection));
			AssertEquals("biz1 (1) (1.1), biz2 (2) (4.4), biz3 (3) (6.6)", chainLink.ReflectOutObject(collection, collection).ToString());
		}
	}
}
