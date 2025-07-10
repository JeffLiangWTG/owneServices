using System.Globalization;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public static class PipeEngineHelper
	{
		public static string GetID()
		{
			return ZDateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
		}
	}
}
