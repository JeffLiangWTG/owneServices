using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public static class ContextExtensions
	{
		public static bool IsMatch(this Context context, ZString key)
		{
			return context.Type.Type.HasValue && context.Type.Type.Value == key;
		}

		public static ZString? GetZStringValue(this IEnumerable<Context> contexts, ZString key)
		{
			return contexts.FirstOrDefault(x => IsMatch(x, key))?.Value;
		}
	}
}
