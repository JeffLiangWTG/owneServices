using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.EventManagement.Testing
{
	sealed class ProcessHandlingInfoTest : TestCaseWithFactory
	{
		public void TestConstruction()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ new ProcessHandlingInfoForTesting(null); });
			AssertNoExceptionThrown(delegate
			{ new ProcessHandlingInfoForTesting(Factory.New<DummyIProcessHandlingInfoProvider>()); });
		}

		public void TestIsEventExcludedFromCascadingOrPropagation()
		{
			Factory.LoadFromNaturalKey<StmEvent>(StmEventSchema.SE_Code, Events.CustomisableEvent00Code).SE_PropagateToParent = true;
			Factory.LoadFromNaturalKey<StmEvent>(StmEventSchema.SE_Code, Events.CustomisableEvent01Code).SE_PropagateToParent = false;

			var bizo = Factory.New<DummyIProcessHandlingInfoProvider>();
			var handler = new ProcessHandlingInfoForTesting(bizo);
			AssertEquals(false, handler.IsEventExcludedFromCascadingOrPropagation(Events.CustomisableEvent00Code));
			AssertEquals(true, handler.IsEventExcludedFromCascadingOrPropagation(Events.CustomisableEvent01Code));
		}
	}
}
