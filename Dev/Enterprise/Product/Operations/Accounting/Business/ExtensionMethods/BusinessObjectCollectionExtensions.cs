using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.Accounting.Business
{
	public static class BusinessObjectCollectionExtensions
	{
		public static void LoadByPKInBatches<T>(this BusinessObjectCollection collection, SchemaPKColumn pkColumn, List<ZGuid> pks) where T : BusinessObject
		{
			int batchSize = 1000;
			int pkCount = pks.Count;
			for (int i = 0; i < pkCount; i = i + batchSize)
			{
				ZGuid[] batch = new ZGuid[i <= pkCount - batchSize ? batchSize : pkCount % batchSize];
				pks.CopyTo(i, batch, 0, batch.Length);

				collection.AddRange(collection.Factory.Load<T>(new ZQuery(pkColumn, batch.ToArray())));
			}
		}
	}
}
