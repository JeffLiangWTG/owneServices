using CargoWise.EntityFramework.Testing;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Module
{
	[TestedType(typeof(GeneralActionMethodProvider))]
	sealed class GeneralActionMethodProviderTest : OperationalActionMethodProviderTest
	{
		public void TestNewMethods_BizoSupportsStmNote()
		{
			OperationalActionMethod[] methods = Provider.NewMethods(new DummyModuleWithActionsSupport<DummyEnterpriseBusinessObject>.Supporter());
			AssertEquals(3, methods.Length);
			AssertEquals(typeof(ShowEditNoteActionMethod), methods[0].GetType());
			AssertEquals(typeof(OpenURLActionMethod), methods[1].GetType());
			AssertEquals(typeof(RunProgramActionMethod), methods[2].GetType());
		}

		public void TestNewMethods_BizoDoesNotSupportStmNote()
		{
			OperationalActionMethod[] methods = Provider.NewMethods(new DummyModuleWithActionsSupport<DummyBusinessObject>.Supporter());
			AssertEquals(2, methods.Length);
			AssertEquals(typeof(OpenURLActionMethod), methods[0].GetType());
			AssertEquals(typeof(RunProgramActionMethod), methods[1].GetType());
		}

		#region Implementation

		protected override ActionMethodProviderID ID
		{
			get { return ActionMethodProviderIDs.General; }
		}

		#endregion
	}
}
