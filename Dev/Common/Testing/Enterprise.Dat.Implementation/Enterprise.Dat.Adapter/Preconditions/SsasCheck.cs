using CargoWise.Common;
using Microsoft.AnalysisServices;

namespace Enterprise.Dat.Implementation.Preconditions
{
	static class SsasCheck
	{
		public static bool IsSsasMeetsRequirement()
		{
			using (var server = new Server())
			{
				try
				{
					server.Connect($"Data Source={LocalDBConnection.GetServerName()};Application Name=SSAS Requirement Checker");
					return server.ServerMode == ServerMode.Tabular;
				}
				catch (ConnectionException ex) when (!ex.IsCriticalException())
				{
					return false;
				}
			}
		}
	}
}
