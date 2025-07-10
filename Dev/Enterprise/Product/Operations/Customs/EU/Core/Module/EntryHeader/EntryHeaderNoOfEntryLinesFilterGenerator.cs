using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Module
{
	public sealed class EntryHeaderNoOfEntryLinesFilterGenerator : ModuleFilterGenerator
	{
		protected override ModuleFilter GenerateInternal(ZString description)
		{
			var noOfEntryLinesFilter = new ModuleNumberRangeFilter(description, GetNoOfEntriesRangeQuery)
			{
				Decimals = 0
			};
			return noOfEntryLinesFilter;
		}

		ZQuery GetNoOfEntriesRangeQuery(INumericZType value1, INumericZType value2)
		{
			var entryHeaderMainQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));
			var numberOfLinesQuery = FormattableString
				.Invariant($@"{CusEntryHeaderSchema.Constants.PK} IN (
							SELECT {CusEntryLineSchema.Constants.CL_CH}
							FROM {CusEntryLineSchema.Constants.SqlSchemaName}.{CusEntryLineSchema.Constants.TableName}
							GROUP BY {CusEntryLineSchema.Constants.CL_CH}
							HAVING COUNT({CusEntryLineSchema.Constants.CL_CH}) BETWEEN @ValueOne AND @ValueTwo)");
			var valueOneParam = ZSqlParameter.New("@ValueOne", value1, Schema.GenericDecimalColumn);
			var valueTwoParam = ZSqlParameter.New("@ValueTwo", value2, Schema.GenericDecimalColumn);
			entryHeaderMainQuery.AddFilterAndZSQLParameterCollection(numberOfLinesQuery,
				new ZSqlParameterCollection(valueOneParam, valueTwoParam));
			return entryHeaderMainQuery;
		}
	}
}
