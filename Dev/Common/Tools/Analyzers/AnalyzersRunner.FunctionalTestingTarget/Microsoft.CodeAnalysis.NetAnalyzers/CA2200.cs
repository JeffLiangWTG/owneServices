using System;

namespace AnalyzersRunner.FunctionalTestingTarget.Microsoft.CodeAnalysis.NetAnalyzers
{
	/// <summary>
	/// Rule: Rethrow to preserve stack details
	/// </summary>
	class CA2200
	{
		public CA2200()
		{
			try
			{
				throw new Exception();
			}
			catch (Exception e)
			{
				// CA2200: Rethrow to preserve stack details
				throw e;
			}
		}
	}
}
