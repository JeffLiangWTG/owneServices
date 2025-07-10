using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.CA.DIF.GUI;

public sealed class DISPreFormActionRegistrar : IDISPreFormActionRegistrar
{
	public IDISPreFormActionRunner GetDISPreFormActionRunner(IDISHost host)
	{
		IDISPreFormActionRunner runner = null;

		if (host is Integration.Customs.CA.IJobDeclaration)
		{
			runner = new JobDeclarationDISPreFormActionRunner(host);
		}

		return runner;
	}
}
