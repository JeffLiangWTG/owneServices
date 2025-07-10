using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.BarcodeParsing.Business
{
	public static class ZQueryExtensions
	{
		public static void AddGuidFilterOrIsNullIfGuidIsEmpty(this ZQuery query, SchemaGuidColumn column, ZGuid guid)
		{
			if (guid.IsValid)
			{
				query.AddToFilter(column, guid);
			}
			else
			{
				query.AddToFilter(column, null);
			}
		}
	}
}
