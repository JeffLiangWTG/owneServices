using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.ExitControl.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.ExitControl.Module
{
	public class ExitControlFilterBusinessObject : EU.ExitControl.Module.ExitControlFilterBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public static class ESFilterConstants
		{
			public const string ClearanceDate = "Clearance Date";
			public const string Circuit = "Circuit";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();
			AddClearanceDateFilter(result);
			AddCircuitFilter(result);
			return result;
		}

		void AddClearanceDateFilter(ModuleFilterCollection moduleFilterCollection)
		{
			var filter = moduleFilterCollection.AddDateFilter
				(
					ESFilterConstants.ClearanceDate,
					(comparisonOperator, fromDate, toDate) =>
					{
						var entryNumSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
						entryNumSubQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, Core.Constants.CountryCodes.Spain);
						entryNumSubQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumberTypes.Spain.ClearanceCSV);
						AddDateTimeRange(entryNumSubQuery, comparisonOperator, JoinCondition.And, CusEntryNumSchema.CE_IssueDate, fromDate, toDate, false, false);

						var exitReportSubQuery = new ZDBOnlySubQuery(typeof(CusExitReport), CusExitReportSchema.CER_ClusterKey, CusExitHeaderSchema.CXH_ClusterKey);
						exitReportSubQuery.AddSubQuery(entryNumSubQuery, JoinCondition.And);

						var exitHeaderQuery = new ZDBOnlyQuery(typeof(CusExitHeader));
						exitHeaderQuery.AddSubQuery(exitReportSubQuery, JoinCondition.And);

						return exitHeaderQuery;
					},
					isNullable: true
				);

			filter.Category = FilterCategories.Dates;
			filter.MultilingualDescription = ResString.GetMultilingualString("E5041F05-BF2F-45B1-AA5B-F4DE9D00C891", "Clearance Date");
		}

		void AddCircuitFilter(ModuleFilterCollection moduleFilterCollection)
		{
			string GetMappedValueForCircuit(string circuitCode)
			{
				switch (circuitCode)
				{
					case MessageFunctionCodeList.Codes.GreenCircuitText:
						return CircuitCodeList.Codes.GREEN;
					case MessageFunctionCodeList.Codes.RedCircuitText:
						return CircuitCodeList.Codes.RED;
					case MessageFunctionCodeList.Codes.OrangeCircuitText:
						return CircuitCodeList.Codes.ORANGE;
					default:
						return circuitCode;
				}
			}

			var filter = moduleFilterCollection.AddTextFilter
				(
					ESFilterConstants.Circuit,
					(comparisonOperator, value) =>
					{
						var entryNumSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
						entryNumSubQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, Core.Constants.CountryCodes.Spain);
						entryNumSubQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumberTypes.Spain.ClearanceCSV);
						var mappedValue = GetMappedValueForCircuit(value);
						entryNumSubQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryStatus, comparisonOperator, mappedValue);

						var exitReportSubQuery = new ZDBOnlySubQuery(typeof(CusExitReport), CusExitReportSchema.CER_ClusterKey, CusExitHeaderSchema.CXH_ClusterKey);
						exitReportSubQuery.AddSubQuery(entryNumSubQuery, JoinCondition.And);

						var exitHeaderQuery = new ZDBOnlyQuery(typeof(CusExitHeader));
						exitHeaderQuery.AddSubQuery(exitReportSubQuery, JoinCondition.And);

						return exitHeaderQuery;
					},
					() => Factory.GetCachedValue("ExitControl_CircuitCodeList", () =>
							{
								var result = new CodeDescriptionPairList();
								result.AddPair(MessageFunctionCodeList.Codes.GreenCircuitText, Res.GetString("B2C2075C-76D6-4763-916C-8B895577D4EE", "GREEN"));
								result.AddPair(MessageFunctionCodeList.Codes.RedCircuitText, Res.GetString("62A8E799-33B1-4FAB-85A2-BC94C82D1723", "RED"));
								result.AddPair(MessageFunctionCodeList.Codes.OrangeCircuitText, Res.GetString("9934A618-56C4-4380-89C2-1A499C06FB0F", "ORANGE"));
								return result;
							})
				).WithMaxLengthOf<ModuleTextFilter>(CusEntryNumSchema.CE_EntryStatus);

			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("C1E5F374-8A1E-4E43-A52F-F675EFE3AD39", "Circuit");
		}
	}
}
