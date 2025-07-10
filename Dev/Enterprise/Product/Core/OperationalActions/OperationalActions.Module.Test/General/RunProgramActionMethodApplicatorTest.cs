using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	[TestedType(typeof(RunProgramActionMethodApplicator))]
	sealed class RunProgramActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestRunProgram()
		{
			var applicator = new RunProgramActionMethodApplicator(Settings, Factory);
			var log = new DummyOperationalActionSectionLog();
			AssertEquals("Precondition", 0, applicator.resolvedArgumentsList.Count);
			applicator.Apply(log, new[] { GetDataBusinessObject(@"dummy1"), GetDataBusinessObject(@"dummy2") });
			AssertEquals(2, applicator.resolvedArgumentsList.Count);
			AssertEquals(@"dummy1.txt", applicator.resolvedArgumentsList[0]);
			AssertEquals(@"dummy2.txt", applicator.resolvedArgumentsList[1]);
		}

		#region Implementation

		public RunProgramActionMethodSettings Settings
		{
			get { return new RunProgramActionMethodSettings { Path = System.Environment.SystemDirectory + "\\notepad.exe", Arguments = @"<Z0_NVarChar>.txt" }; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RunProgramActionMethodApplicator(Settings, Factory);
		}

		BusinessObject GetDataBusinessObject(string name)
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			dummy.Z0_NVarChar = name;
			return dummy;
		}

		#endregion
	}
}
