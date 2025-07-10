using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.DocWrappers.Testing
{
	[TestedType(typeof(DocumentWrapper))]
	public class DocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBusinessObjectForPrintJob()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var testWrapper = BODocDataProvider.Get(new DocumentWrapperForTesting(dummy, Factory));

			Assert(testWrapper is IBODocDataProviderWithBOForPrintJob);
			AssertEquals(testWrapper.BusinessObjectToLogAgainst, ((IBODocDataProviderWithBOForPrintJob)testWrapper).BusinessObjectForPrintJob);
		}

		public void TestToString()
		{
			TestWrapper wrapperWithDefaultValue = new TestWrapper();
			wrapperWithDefaultValue.Name = "Default Value";
			AssertEquals("wrapperWithDefaultValue.ToString()", "Default Value", wrapperWithDefaultValue.ToString());
			TestWrapperWithoutDefaultField defaultlessWrapper = new TestWrapperWithoutDefaultField();
			AssertEquals("defaultlessWrapper.ToString()", "(No Default Field Value Available on ShoesAndSocks)", defaultlessWrapper.ToString());
		}

		public void TestHumanReadableName()
		{
			DocumentWrapper wrapper = new TestWrapper();
			AssertEquals("Test", wrapper.HumanReadableName);
			wrapper = new TestWrapperWithoutDefaultField();
			AssertEquals("ShoesAndSocks", wrapper.HumanReadableName);
		}

		public void TestImageNamesToRemove()
		{
			IBODocDataProvider testWrapper = BODocDataProvider.Get(new DocumentWrapperForTesting());
			AssertEquals("ImageNamesToRemove should be an empty array by default", 0, testWrapper.ImageNamesToRemove.Length);
		}

		public void TestAdditionalCopyInfo()
		{
			IBODocDataProvider docDataProvider = new DocumentWrapperForTesting();
			AssertNull("AdditionalCopyInfo should be null by default", docDataProvider.AdditionalCopyInfo);

			DocWrapperCopyInfo info = new DocWrapperCopyInfoForTesting();
			((DocumentWrapperForTesting)docDataProvider).SetAdditionalCopyInfo(info);
			AssertEquals("CopyInfo should be same instance", info, docDataProvider.AdditionalCopyInfo);

			((DocumentWrapperForTesting)docDataProvider).SetAdditionalCopyInfo(null);
			AssertEquals("AdditionalCopyInfo should be null", null, docDataProvider.AdditionalCopyInfo);
		}

		public void TestBusinessObjectToLogAgainst()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var testWrapper = BODocDataProvider.Get(new DocumentWrapperForTesting(dummy, Factory));
			AssertEquals("BusinessObjectToLogAgainst should be the same instance as ParentBusinessObject", testWrapper.ParentBusinessObject, testWrapper.BusinessObjectToLogAgainst);

			testWrapper = BODocDataProvider.Get(new DocumentWrapperForTesting("hello", Factory));
			AssertNull("BusinessObjectToLogAgainst should be null if the wrapped object is not a BusinessObject", testWrapper.BusinessObjectToLogAgainst);
		}

		public void TestAddToFactoryCache()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var testWrapper = new DocumentWrapperForTesting(dummy, Factory);
			AssertEquals("DocumentWrapper must not be cached in a Factory. It cause memory leaks.", 0, Factory.GetBizOsForPK(testWrapper.PK.ToGuid()).Length);
		}

		#region Implementation
		[WrapperTypeName("ShoesAndSocks")]
		class TestWrapperWithoutDefaultField : DocumentWrapper
		{
		}

		[DefaultField("Name")]
		class TestWrapper : DocumentWrapper
		{
			public ZString Name
			{
				get { return fName; }
				set { fName = value; }
			}
			ZString fName;

			public ZString Phone
			{
				get { return fPhone; }
				set { fPhone = value; }
			}
			ZString fPhone;

			public ZInt IntField
			{
				get { return fIntField; }
				set { fIntField = value; }
			}
			ZInt fIntField;

			public ZDecimal DecimalField
			{
				get { return fDecimalField; }
				set { fDecimalField = value; }
			}
			ZDecimal fDecimalField;

			public ZDateTime DateTimeField
			{
				get { return fDateTimeField; }
				set { fDateTimeField = value; }
			}
			ZDateTime fDateTimeField;

			public ZBool BoolTrueField
			{
				get { return true; }
			}

			public ZBool BoolFalseField
			{
				get { return false; }
			}

			public TestWrapperChild Child
			{
				get { return new TestWrapperChild(); }
			}
		}

		[DefaultField("Frangelico")]
		class TestWrapperChild : DocumentWrapper
		{
			public ZString Frangelico
			{
				get { return "Frangelico"; }
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocumentWrapperForTesting();
		}
		#endregion
	}
}
