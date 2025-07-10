using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Module
{
	public sealed class EntryHeaderTotalCustomsQtyFilterGenerator : ModuleFilterGenerator
	{
		protected override ModuleFilter GenerateInternal(ZString description)
		{
			var filter = new ModuleNumberRangeFilter(description, GetTotalCusQuantityRangeQuery)
			{
				Decimals = JobComInvoiceLineSchema.JI_CustomsQuantity.Scale
			};
			return filter;
		}

		ZQuery GetTotalCusQuantityRangeQuery(INumericZType value1, INumericZType value2)
		{
			var entryHeaderMainQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));
			var totalCusQuantityFilterQuery = FormattableString.Invariant($@"{CusEntryHeaderSchema.Constants.PK} IN (
					SELECT {CusEntryLineSchema.Constants.CL_CH}
					FROM {CusEntryLineSchema.Constants.SqlSchemaName}.{CusEntryLineSchema.Constants.TableName}
					INNER JOIN {JobComInvoiceLineSchema.Constants.SqlSchemaName}.{JobComInvoiceLineSchema.Constants.TableName} ON {JobComInvoiceLineSchema.Constants.JI_CL} = {CusEntryLineSchema.Constants.PK}
					GROUP BY {CusEntryLineSchema.Constants.CL_CH}
					HAVING SUM({JobComInvoiceLineSchema.Constants.JI_CustomsQuantity}) BETWEEN @ValueOne AND @ValueTwo)");
			var valueOneParam = ZSqlParameter.New("@ValueOne", value1, Schema.GenericDecimalColumn);
			var valueTwoParam = ZSqlParameter.New("@ValueTwo", value2, Schema.GenericDecimalColumn);
			entryHeaderMainQuery.AddFilterAndZSQLParameterCollection(totalCusQuantityFilterQuery,
				new ZSqlParameterCollection(valueOneParam, valueTwoParam));
			return entryHeaderMainQuery;
		}
	}
}
