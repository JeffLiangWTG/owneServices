using System.Collections.Generic;
using Enterprise.Integration;

namespace Enterprise.Client.EDI
{
	public interface IBootstrapperProvider
	{
		ITokenBootstrapper GetTokenBootstrapper(int version);

		ITfIdfBootstrapper GetTfIdfBootstrapper(int version, IDictionary<string, int> tokens);
	}

	public class BootstrapperProvider : IBootstrapperProvider
	{
		readonly ILogger logger;

		public BootstrapperProvider(ILogger logger)
		{
			this.logger = logger;
		}

		public ITfIdfBootstrapper GetTfIdfBootstrapper(int version, IDictionary<string, int> tokens)
		{
			return new TfIdfBootstrapper(version, tokens, logger);
		}

		public ITokenBootstrapper GetTokenBootstrapper(int version)
		{
			return new TokenBootstrapper(version);
		}
	}
}
