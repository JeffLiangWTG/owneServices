using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public interface ISingleLineEntryProvider
	{
		SingleLineEntry GetSingleLineEntry(JobDeclaration declaration);
		ZString DefaultCPCCode(JobDeclaration declaration);
	}
}
