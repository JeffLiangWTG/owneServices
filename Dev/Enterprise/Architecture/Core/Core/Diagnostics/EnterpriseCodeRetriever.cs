using Enterprise.Integration.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Core.Diagnostics
{
	public class EnterpriseCodeRetriever : IEnterpriseCodeRetriever
	{
		#region IEnterpriseCodeRetriever Members

		public string EnterpriseCodeFromRegistry
		{
			get
			{
				var expectedDllName = EnvProxy.Instance.Registry.ExpectedClientDLL;
				if (string.IsNullOrWhiteSpace(expectedDllName))
				{
					return null;
				}

				return expectedDllName.Substring(expectedDllName.Length - 3);
			}
		}

		#endregion //IEnterpriseCodeRetriever Members
	}
}
