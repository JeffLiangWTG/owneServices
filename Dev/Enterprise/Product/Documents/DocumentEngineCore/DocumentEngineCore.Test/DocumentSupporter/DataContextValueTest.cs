using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.DocumentEngineCore.DocumentSupport.Testing
{
	sealed class DataContextValueTest : TestCaseWithFactory
	{
		public void TestBOTypeConstructor()
		{
			DataContextValue businessObjectContextValue = new DataContextValue(".DummyBusinessObject", typeof(DummyBusinessObject));
			AssertEquals("businessObjectContextValue.DataContext", DataContext.BusinessObject, businessObjectContextValue.DataContext);
			AssertEquals("businessObjectContextValue.BusinessObjectDataContext", ".DummyBusinessObject", businessObjectContextValue.BusinessObjectDataContext);
			AssertEquals("businessObjectContextValue.BusinessObjectType", ".DummyBusinessObject", businessObjectContextValue.BusinessObjectDataContext);
			AssertEquals("businessObjectContextValue.FullDataContext", ".DummyBusinessObject", businessObjectContextValue.FullDataContext);
			AssertEquals("businessObjectContextValue.ToString()", ".DummyBusinessObject", businessObjectContextValue.ToString());
			AssertEquals("businessObjectContextValue.IsBusinessObjectType", true, businessObjectContextValue.IsBusinessObjectType);
		}

		public void TestNormalConstructor()
		{
			string storedDataContextForDocWrapper = "UnitTest";
			string storedDataContextForBusinessObject = ".JobDeclaration";

			DataContextValue docWrapperContextValue = new DataContextValue(storedDataContextForDocWrapper);
			AssertEquals("docWrapperContextValue.DataContext", DataContext.UnitTest, docWrapperContextValue.DataContext);
			AssertEquals("docWrapperContextValue.BusinessObjectDataContext", "", docWrapperContextValue.BusinessObjectDataContext);
			AssertEquals("docWrapperContextValue.FullDataContext", storedDataContextForDocWrapper, docWrapperContextValue.FullDataContext);
			AssertEquals("docWrapperContextValue.ToString()", storedDataContextForDocWrapper, docWrapperContextValue.ToString());
			AssertEquals("docWrapperContextValue.IsBusinessObjectType", false, docWrapperContextValue.IsBusinessObjectType);

			DataContextValue businessObjectContextValue = new DataContextValue(storedDataContextForBusinessObject);
			AssertEquals("businessObjectContextValue.DataContext", DataContext.BusinessObject, businessObjectContextValue.DataContext);
			AssertEquals("businessObjectContextValue.BusinessObjectDataContext", storedDataContextForBusinessObject, businessObjectContextValue.BusinessObjectDataContext);
			AssertEquals("businessObjectContextValue.FullDataContext", storedDataContextForBusinessObject, businessObjectContextValue.FullDataContext);
			AssertEquals("businessObjectContextValue.ToString()", storedDataContextForBusinessObject, businessObjectContextValue.ToString());
			AssertEquals("businessObjectContextValue.IsBusinessObjectType", true, businessObjectContextValue.IsBusinessObjectType);

			AssertExceptionThrown("Cannot have an empty string", typeof(DataContextIsInvalidException), delegate
			{ new DataContextValue(""); });
			AssertExceptionThrown("Could not find invalid datacontext 'FlapJacksFromMartianCrap'", typeof(DataContextIsInvalidException), delegate
			{ new DataContextValue("FlapJacksFromMartianCrap"); });
			AssertExceptionThrown("BusinessObjectDataContext is too long to be stored in SO_DataContext", typeof(DataContextIsInvalidException), delegate
			{ new DataContextValue(".Business.Testing.DummyBusinessObject"); });
		}

		public void TestIsValidFullDataContext()
		{
			AssertEquals("DataContextValue.IsValidFullDataContext(DataContext.APPayment.ToString())", true, DataContextValue.IsValidFullDataContext(nameof(DataContext.APPayment)));
			AssertEquals("DataContextValue.IsValidFullDataContext(\"\")", false, DataContextValue.IsValidFullDataContext(""));
			AssertEquals("DataContextValue.IsValidFullDataContext(\"FlapJacksFromMartianCrap\")", false, DataContextValue.IsValidFullDataContext("FlapJacksFromMartianCrap"));
			AssertEquals("DataContextValue.IsValidFullDataContext(\".Business.Testing.DummyBusinessObject\")", false, DataContextValue.IsValidFullDataContext(".Business.Testing.DummyBusinessObject"));
		}

		public void TestConstructorForTesting()
		{
			DataContextValue docWrapperContextValue = new DataContextValueForTesting(DataContext.UnitTest);
			AssertEquals("docWrapperContextValue.DataContext", DataContext.UnitTest, docWrapperContextValue.DataContext);
			AssertEquals("docWrapperContextValue.BusinessObjectType", "", docWrapperContextValue.BusinessObjectDataContext);
			AssertEquals("docWrapperContextValue.FullDataContext", nameof(DataContext.UnitTest), docWrapperContextValue.FullDataContext);
			AssertEquals("docWrapperContextValue.IsBusinessObjectType", false, docWrapperContextValue.IsBusinessObjectType);
		}

		public void TestStaticNone()
		{
			DataContextValue noneValue = DataContextValue.None;
			AssertEquals("noneValue.DataContext", DataContext.None, noneValue.DataContext);
			AssertEquals("noneValue.BusinessObjectType", "", noneValue.BusinessObjectDataContext);
			AssertEquals("noneValue.FullDataContext", nameof(DataContext.None), noneValue.FullDataContext);
			AssertEquals("noneValue.IsBusinessObjectType", false, noneValue.IsBusinessObjectType);

			DataContextValue anotherNoneValue = DataContextValue.None;
			AssertEquals("anotherNoneValue", noneValue, anotherNoneValue);
		}

		public void TestWantsBusinessObjectOfType()
		{
			BusinessObject dummyBO = Factory.New<DummyBusinessObject>();
			BusinessObject otherBO = Factory.New<DummyBaseBusinessObject>();

			DataContextValue docWrapperContextValue = new DataContextValue(nameof(DataContext.UnitTest));
			AssertEquals("docWrapperContextValue.WantsBusinessObjectOfType(dummyBO)", false, docWrapperContextValue.WantsBusinessObjectOfType(dummyBO.GetType()));
			AssertEquals("docWrapperContextValue.WantsBusinessObjectOfType(otherBO)", false, docWrapperContextValue.WantsBusinessObjectOfType(otherBO.GetType()));

			DataContextValue businessObjectContextValue = new DataContextValue("." + nameof(DummyBusinessObject));
			AssertEquals("businessObjectContextValue.WantsBusinessObjectOfType(dummyBO)", true, businessObjectContextValue.WantsBusinessObjectOfType(dummyBO.GetType()));
			AssertEquals("businessObjectContextValue.WantsBusinessObjectOfType(otherBO)", false, businessObjectContextValue.WantsBusinessObjectOfType(otherBO.GetType()));
		}

		public void TestEquals()
		{
			DataContextValue value1 = new DataContextValueForTesting(DataContext.APPayment);
			DataContextValue value2 = new DataContextValueForTesting(DataContext.APPayment);
			AssertEquals(value1, value2);
		}
	}
}
