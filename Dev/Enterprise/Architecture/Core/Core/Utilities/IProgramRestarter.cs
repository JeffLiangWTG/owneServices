namespace Enterprise.ZArchitecture.Core
{
	public interface IProgramRestarter
	{
		void ShutdownEnterpriseWithMessage(string exitMessage);
		bool IsAlreadyClosing { get; }
		void Restart(string exeFilePath = null, CommandLineArguments arguments = null);
	}
}
