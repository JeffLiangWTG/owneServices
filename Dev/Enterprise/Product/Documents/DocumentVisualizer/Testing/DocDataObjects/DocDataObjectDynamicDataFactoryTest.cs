using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.DocDataObjects.Testing
{
	sealed class DocDataObjectDynamicDataFactoryTest : TestCaseWithFactory
	{
		public void TestWrapBusinessObject()
		{
			var factory = new DocDataObjectDynamicDataFactory();
			var dummy = Factory.New<DummyBusinessObject>();

			var dynamicData = factory.Create(dummy, dummy.GetType(), new DynamicDataManager());

			AssertEquals("Created DynamicData", typeof(DocDataObjectDynamicData), dynamicData.GetType());
			AssertEquals("DynamicData Value", dummy, dynamicData.Value);
			AssertEquals("DynamicData Type", typeof(DummyBusinessObject), dynamicData.Type);
		}

		public void TestWrapBusinessObjectCollection()
		{
			var factory = new DocDataObjectDynamicDataFactory();
			var dummyCollection = new DummyBusinessObjectCollection(Factory);

			var dynamicData = factory.Create(dummyCollection, dummyCollection.GetType(), new DynamicDataManager());

			AssertEquals("Created DynamicDataCollection", typeof(DynamicDataCollection), dynamicData.GetType());
			AssertEquals("DynamicDataCollection Value", dummyCollection, dynamicData.Value);
			AssertEquals("DynamicDataCollection Type", typeof(DummyBusinessObjectCollection), dynamicData.Type);
		}

		public void TestWrapReadOnlyCollectionOfStrings()
		{
			var factory = new DocDataObjectDynamicDataFactory();
			IReadOnlyCollection<string> dummyCollection = new[] { "aaa" };

			var dynamicDataCollection = (IDynamicDataCollection)factory.Create(dummyCollection, typeof(IReadOnlyCollection<string>), new DynamicDataManager());

			AssertEquals("DynamicDataCollection Value", dummyCollection, dynamicDataCollection.Value);
			AssertEquals("DynamicDataCollection Type", typeof(IReadOnlyCollection<string>), dynamicDataCollection.Type);
			AssertEquals("DynamicDataCollection ElementType", typeof(string), dynamicDataCollection.ElementType);

			var element = dynamicDataCollection.Single();

			Assert("DynamicDataCollection element is IDynamicData", element is IDynamicData);
			Assert("DynamicDataCollection element is not IDynamicDataCollection", !(element is IDynamicDataCollection));
		}

		public void TestWrapReadOnlyCollectionOfZStrings()
		{
			var factory = new DocDataObjectDynamicDataFactory();
			IReadOnlyCollection<ZString> dummyCollection = new[] { new ZString("aaa") };

			var dynamicDataCollection = (IDynamicDataCollection)factory.Create(dummyCollection, typeof(IReadOnlyCollection<ZString>), new DynamicDataManager());

			AssertEquals("DynamicDataCollection Value", dummyCollection, dynamicDataCollection.Value);
			AssertEquals("DynamicDataCollection Type", typeof(IReadOnlyCollection<ZString>), dynamicDataCollection.Type);
			AssertEquals("DynamicDataCollection ElementType", typeof(ZString), dynamicDataCollection.ElementType);

			var element = dynamicDataCollection.Single();

			Assert("DynamicDataCollection element is IDynamicData", element is IDynamicData);
			Assert("DynamicDataCollection element is not IDynamicDataCollection", !(element is IDynamicDataCollection));
		}

		public void TestWrapReadOnlyCollectionOfInterfaces()
		{
			var factory = new DocDataObjectDynamicDataFactory();
			IReadOnlyCollection<IBusiness> dummyCollection = new[] { new DummyNonPersistentBusinessObject() };

			var dynamicDataCollection = (IDynamicDataCollection)factory.Create(dummyCollection, typeof(IReadOnlyCollection<IBusiness>), new DynamicDataManager());

			AssertEquals("DynamicDataCollection Value", dummyCollection, dynamicDataCollection.Value);
			AssertEquals("DynamicDataCollection Type", typeof(IReadOnlyCollection<IBusiness>), dynamicDataCollection.Type);
			AssertEquals("DynamicDataCollection ElementType", typeof(IBusiness), dynamicDataCollection.ElementType);

			var element = dynamicDataCollection.Single();

			Assert("DynamicDataCollection element is IDynamicData", element is IDynamicData);
			Assert("DynamicDataCollection element is not IDynamicDataCollection", !(element is IDynamicDataCollection));
		}

		public void TestGetFromCache()
		{
			var factory = new DocDataObjectDynamicDataFactory();
			var dummyDocDataObject1 = new DummyDocDataObject("1");

			var dynamicDummy1 = factory.Create(dummyDocDataObject1, typeof(IBusiness), new DynamicDataManager());
			var dynamicDummy1_2 = factory.Create(dummyDocDataObject1, typeof(IBusiness), new DynamicDataManager());
			Assert("same instance of IDynamicData has been returned", object.ReferenceEquals(dynamicDummy1, dynamicDummy1_2));

			var dynamicDummy1_3 = factory.Create(dummyDocDataObject1, typeof(object), new DynamicDataManager());
			Assert("same instance of IDynamicData has been returned", object.ReferenceEquals(dynamicDummy1, dynamicDummy1_3));

			var dummyDocDataObject2 = new DummyDocDataObject("2");

			var dynamicDummy2 = factory.Create(dummyDocDataObject2, typeof(IBusiness), dynamicDummy1);
			var dynamicDummy2_2 = factory.Create(dummyDocDataObject2, typeof(IBusiness), dynamicDummy1);
			Assert("same instance of IDynamicData has been returned", object.ReferenceEquals(dynamicDummy2, dynamicDummy2_2));

			var dynamicDummy2_3 = factory.Create(dummyDocDataObject2, typeof(object), dynamicDummy1);
			Assert("same instance of IDynamicData has been returned", object.ReferenceEquals(dynamicDummy2, dynamicDummy2_3));
		}

		public void TestContainerType()
		{
			var expr = "Code".CreateExpression();

			var data = new ContainerTypeForTest
			{
				Code = "zzz"
			};

			var result = expr.Evaluate(data);

			AssertMultilineASCIIEquals("no errors",
				"",
				expr.ToFormatString());

			AssertEquals("evaluation result", "zzz", result);
		}

		sealed class ContainerTypeForTest : IContainerType
		{
			public ZString Code { get; set; }
			public ZString Description { get; set; }
			public object Codes { get; set; }
			public ZString ISOCode { get; set; }
			public ICodeDescription Type { get; }
		}
	}
}
