using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Testing
{
	sealed class TotalFunctionExtractorTest : Core.Testing.BaseFunctionExtractorTest
	{
		public void TestGetMethodInfoChainLinkForIBusinessObjectCollectionWithFilter()
		{
			var dummies = new DummyBusinessObjectCollection(Factory);

			var dummy1 = dummies.AddNew();
			dummy1.Z0_VarCharMax = "Yes";
			dummy1.Z0_Number = 10;
			dummies.Add(dummy1);

			var dummy2 = dummies.AddNew();
			dummy2.Z0_VarCharMax = "No";
			dummy2.Z0_Number = 20;
			dummies.Add(dummy2);

			var totalExtractor = TotalFunctionExtractor.ParseAndExtract(@"Total(Z0_Number, 2, ""<Z0_VarCharMax>"" == ""Yes"")");

			var chainLink = totalExtractor.GetMethodInfoChainLink(typeof(MyWrapperActiveCollection));
			AssertEquals("chainLink.ReflectOutObject(collection, collection).ToString())", "10", chainLink.ReflectOutObject(dummies, dummies).ToString());
		}

		public override void TestGetMethodInfoChainLink()
		{
			MyWrapperCollection collection = new MyWrapperCollection();
			collection.Add(new MyWrapper());
			collection.Add(new MyWrapper());
			TotalFunctionExtractor totalExtractor = TotalFunctionExtractor.ParseAndExtract("Total(\"Twelve\")");
			MethodInfoChainLink chainLink = totalExtractor.GetMethodInfoChainLink(typeof(MyWrapperCollection));
			AssertEquals("chainLink.ReflectOutObject(0, collection, collection).ToString())", "24", chainLink.ReflectOutObject(collection, collection).ToString());
		}

		public void TestGetMethodInfoChainLinkForIBusinessObjectCollection()
		{
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			DummyBusinessObject bizObj1 = Factory.New<DummyBusinessObject>();
			bizObj1.Z0_Number = 12;
			collection.Add(bizObj1);
			DummyBusinessObject bizObj2 = Factory.New<DummyBusinessObject>();
			bizObj2.Z0_Number = 13;
			collection.Add(bizObj2);
			TotalFunctionExtractor totalExtractor = TotalFunctionExtractor.ParseAndExtract("Total(\"Z0_Number\")");

			MethodInfoChainLink chainLink = totalExtractor.GetMethodInfoChainLink(typeof(MyWrapperActiveCollection));
			AssertEquals("chainLink.ReflectOutObject(collection, collection).ToString())", "25", chainLink.ReflectOutObject(collection, collection).ToString());
		}

		class MyWrapper : DocumentWrapper
		{
			public ZInt Twelve
			{
				get { return 12; }
			}
		}

		class MyWrapperCollection : DocumentWrapperCollection<MyWrapper>
		{
			public MyWrapperCollection()
				: base(null)
			{
			}
		}

		class MyWrapperActiveCollection : ActiveBusinessObjectCollection<DummyBusinessObject>
		{
			public MyWrapperActiveCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}

		public void TestConstructorWithOneStringAndTwoParameters()
		{
			TotalFunctionExtractor totalExtractor = TotalFunctionExtractor.ParseAndExtract("<Total(\"RotorSize\")>");
			AssertEquals("TotalRotorSize", totalExtractor.PropertyLabel);
			AssertEquals("RotorSize", totalExtractor.FieldToTotal);
			AssertEquals("", totalExtractor.DecimalPlaces);
		}

		public void TestConstructorWithOneString()
		{
			TotalFunctionExtractor totalExtractor = TotalFunctionExtractor.ParseAndExtract("<Total(\"RotorSize\", 1)>");
			AssertEquals("TotalRotorSize", totalExtractor.PropertyLabel);
			AssertEquals("RotorSize", totalExtractor.FieldToTotal);
			AssertEquals("1", totalExtractor.DecimalPlaces);
		}

		public void TestConstructorWithOneEmptyString()
		{
			TotalFunctionExtractor totalExtractor = TotalFunctionExtractor.ParseAndExtract(ZString.Empty);
			AssertNull(totalExtractor);
		}
	}
}
