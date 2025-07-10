using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Testing.Core
{
	class UniversalObjectFactoryTest : TestCaseWithUniversalObjectFactory
	{
		public void TestNameForDebuggingIsSet()
		{
			AssertEquals("Factory.BOFactory.NameForDebugging", "Universal Data Buss Import", Factory.BOFactory.NameForDebugging);
		}

		public void TestDataRefreshBussDisabled()
		{
			AssertEquals("Factory.BOFactory.RefreshEnabled", false, Factory.BOFactory.RefreshEnabled);
		}

		public void TestValidationIsDisabled()
		{
			AssertEquals("Factory.BOFactory.IsValidationSuspended", true, Factory.BOFactory.IsValidationSuspended);
		}

		public void TestCannotCallSaveOnBOFactoryDirectly()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			AssertEquals("Precondition: dummyBO.IsInDatabase", false, dummyBO.IsInDatabase);

			AssertExceptionThrown(typeof(InvalidOperationException), "This factory should only ever be saved via the Universal Object Factory, not directly.", () => Factory.BOFactory.Save());
			AssertEquals("dummyBO.IsInDatabase", false, dummyBO.IsInDatabase);

			var logger = new DummyLogger();
			AssertNoExceptionThrown(() => Factory.SaveAtEndOfImport(logger));
			AssertEquals("dummyBO.IsInDatabase", true, dummyBO.IsInDatabase);
		}
	}
}
