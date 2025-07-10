namespace Enterprise.Client.EDI.ServiceTask
{
	public interface IReleaseBuildTester
	{
		bool RunDeployment();
		bool RunTest();
	}
}
