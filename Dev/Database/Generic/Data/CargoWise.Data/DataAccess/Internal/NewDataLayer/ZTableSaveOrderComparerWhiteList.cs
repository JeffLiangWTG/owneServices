using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework
{
	public static class ZTableSaveOrderComparerWhiteList
	{
		public static int Compare(string tableName1, string tableName2)
		{
			if (WhiteList.Any(x => x.precedentTableName == tableName1 && x.subsequentTableName == tableName2))
			{
				return -1;
			}
			else if (WhiteList.Any(x => x.precedentTableName == tableName2 && x.subsequentTableName == tableName1))
			{
				return 1;
			}

			return 0;
		}

		static readonly List<(string precedentTableName, string subsequentTableName)> WhiteList = new List<(string, string)>
		{
			(AccExchangeRateConfigurationViewSchema.Constants.TableName, AccJobConfigPivotSchema.Constants.TableName),
			(JobSupplierBookingLineSchema.Constants.TableName, JobPackLinesSchema.Constants.TableName)
		};
	}
}
