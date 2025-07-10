using CargoWise.Types;

namespace Enterprise.BufferManagement.Integration
{
	public interface IProcessHeaderUniqueCompletionStatementGenerator
	{
		ZString GetUniqueCompletionStatement(IProcessHeaderCollection processHeaders, ZString completionStatement);
	}
}
