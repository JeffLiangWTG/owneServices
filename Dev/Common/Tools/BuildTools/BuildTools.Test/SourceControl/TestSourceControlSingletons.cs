using Moq;
using NUnit.Framework;

namespace CargoWise.BuildTools.Testing
{
	public class TestSourceControlSingletons : TestCase
	{
		public void TestSourceControlForTesting()
		{
			MockSourceControl.Setup();
			try
			{
				var mockSourceControl = new Mock<ISourceControl>();
				mockSourceControl.CallBase = true;
				ISourceControl oldSourceControlInstance = SourceControl.EnterpriseDatabase;

				SourceControl.SetEnterpriseInstanceForTesting(mockSourceControl.Object);
				Assert(oldSourceControlInstance != SourceControl.EnterpriseDatabase);

				SourceControl.RemoveEnterpriseInstanceForTesting();
				AssertEquals(oldSourceControlInstance, SourceControl.EnterpriseDatabase);
			}
			finally
			{
				MockSourceControl.TearDown();
			}
		}

		public void TestIsMockDuringTest()
		{
			AssertNotNull(SourceControl.EnterpriseDatabase as MockSourceControl);
		}

		public void TestSourceControlFactoryInstanceExists()
		{
			AssertNotNull(SourceControl.SourceControlFactory);
		}
	}
}
