using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Module;

class MostRecentlyModifiedDeclarationSubGroup : ModuleFilterSubGroup
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "filter requires SQL statement")]
	public override ZQuery GetSubQuery(ZQuery filter)
	{
		const string lastEditedDecQuery = @"EXISTS (" +
			@"SELECT 1
				FROM (
					SELECT STH_SJH AS groupSTH_SJH, MAX(STH_SystemLastEditTimeUtc) AS groupLastEditTime
					FROM dbo.CusTempStorageDec
					GROUP BY STH_SJH
				) AS GroupedBySTH_SJH
				WHERE 
					groupSTH_SJH = STH_SJH AND
					groupLastEditTime = STH_SystemLastEditTimeUtc
				)";

		var cusTempStorageDec = new ZDBOnlySubQuery(typeof(Business.CusTempStorage.CusTempStorageDec), CusTempStorageDecSchema.STH_SJH);
		cusTempStorageDec.AddFilterAndZSQLParameterCollection(lastEditedDecQuery, new ZSqlParameterCollection());

		var cusEntryNumQueries = new List<ZQuery>();
		foreach (var filterQuery in filter.GetCompositeParts())
		{
			if (filterQuery.FilterString.Contains(CusEntryNumSchema.Constants.CE_EntryNum))
			{
				cusEntryNumQueries.Add(filterQuery);
			}
			else
			{
				cusTempStorageDec.AddToFilter(filterQuery);
			}
		}

		if (cusEntryNumQueries.Any())
		{
			var cusEntryNumber = new ZDBOnlySubQuery(typeof(Common.CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			cusEntryNumber.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Germany);
			cusEntryNumber.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusTempStorageDecSchema.Constants.TableName);
			foreach (var cusEntryNumQuery in cusEntryNumQueries)
			{
				cusEntryNumber.AddToFilter(cusEntryNumQuery);
			}
			cusTempStorageDec.AddSubQuery(cusEntryNumber, JoinCondition.And);
		}

		var result = new ZDBOnlyQuery(typeof(Business.CusTempStorage.CusTempStorageJobHeader));
		result.AddSubQuery(cusTempStorageDec, JoinCondition.And);
		return result;
	}
}
