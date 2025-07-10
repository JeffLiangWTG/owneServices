using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public static class LicenceHelper
	{
		public static string GenerateServerCode(IEnumerable<LicenceDatabase> databases, string defaultServerCode)
		{
			if (!databases.Any(x => x.LD_ServerCode == defaultServerCode))
			{
				return defaultServerCode;
			}

			var codes = databases.Select(x => x.LD_ServerCode).Distinct().ToHashSet();

			for (var prefix = 'A'; prefix <= 'Z'; prefix++)
			{
				for (var index = 100; index < 200; index++)
				{
					var code = $"{prefix}{index.ToString().Substring(1)}";
					if (!codes.Contains(code))
					{
						return code;
					}
				}
			}

			return null;
		}
	}
}
