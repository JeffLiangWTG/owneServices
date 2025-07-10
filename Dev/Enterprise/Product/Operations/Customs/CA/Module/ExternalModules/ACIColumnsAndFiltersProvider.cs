using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	partial class CAShipmentModuleColumnsAndFiltersProvider
	{
		class ACIColumnsAndFiltersProvider : Integration.Customs.CA.IShipmentModuleColumnsAndFiltersProvider
		{
			static class Captions
			{
				internal static MultilingualString ACICargoStatusMultilingualDescription
				{
					get { return ResString.GetMultilingualString("6f3a1256-d15c-40fb-bd1c-f1f0ab26f284", ACICargoStatusFilterId); }
				}

				internal const string ACICargoStatusFilterId = "ACI Cargo Status";

				internal static MultilingualString ACIMessageStatusMultilingualDescription
				{
					get { return ResString.GetMultilingualString("c749278c-d374-4ced-abac-181886a4db0e", ACIMessageStatusFilterId); }
				}

				internal const string ACIMessageStatusFilterId = "ACI Message Status";

				internal static MultilingualString EManifestCargoStatusMultilingualDescription
				{
					get { return ResString.GetMultilingualString("8affa890-0ba3-465c-89b0-0109a8b285fa", EManifestCargoStatusFilterId); }
				}

				internal const string EManifestCargoStatusFilterId = "eManifest Cargo Status";

				internal static MultilingualString EManifestMessageStatusMultilingualDescription
				{
					get { return ResString.GetMultilingualString("963b5158-afdf-4385-845d-2de237a0338a", EManifestMessageStatusFilterId); }
				}

				internal const string EManifestMessageStatusFilterId = "eManifest Message Status";
			}

			public void AddFilters(IModuleFilterCollection filters, BusinessObjectFactory factory)
			{
				var collection = (ModuleFilterCollection)filters;
				collection.AddFilter(new EqualAndBlankModuleStatusFilter(
					Captions.ACICargoStatusFilterId,
					Captions.ACICargoStatusMultilingualDescription,
					(c, v) => GetStatusQuery(c, v, CusSCAHouseSchema.CA_ShipmentStatus),
					GetJobStatusList));

				collection.AddFilter(new EqualAndBlankModuleStatusFilter(
					Captions.ACIMessageStatusFilterId,
					Captions.ACIMessageStatusMultilingualDescription,
					(c, v) => GetStatusQuery(c, v, CusSCAHouseSchema.CA_MessageStatus),
					GetMessageStatusList));

				collection.AddFilter(new EqualAndBlankModuleStatusFilter(
					Captions.EManifestCargoStatusFilterId,
					Captions.EManifestCargoStatusMultilingualDescription,
					(c, v) => GetEManifestStatusQuery(c, v, CusCAeMHHouseSchema.BW_CustomsStatus),
					GetJobStatusList));

				collection.AddFilter(new EqualAndBlankModuleStatusFilter(
					Captions.EManifestMessageStatusFilterId,
					Captions.EManifestMessageStatusMultilingualDescription,
					(c, v) => GetEManifestStatusQuery(c, v, CusCAeMHHouseSchema.BW_MessageStatus),
					GetMessageStatusList));
			}

			static bool ReverseOperatorIfIsBlankOrNotEqual(ref SQLComparisonOperator comparisonOperator)
			{
				if (comparisonOperator == SQLComparisonOperator.IsBlank)
				{
					comparisonOperator = SQLComparisonOperator.IsNotBlank;
					return true;
				}
				else if (comparisonOperator == SQLComparisonOperator.NotEqual)
				{
					comparisonOperator = SQLComparisonOperator.Equal;
					return true;
				}
				return false;
			}

			static ZQuery GetStatusQuery(SQLComparisonOperator comparisonOperator, ZString value, SchemaStringColumn aciStatusColumn)
			{
				var query = new ZDBOnlyQuery(typeof(ForwardingShipment));
				var notIn = ReverseOperatorIfIsBlankOrNotEqual(ref comparisonOperator);
				var aciSubQuery = new ZDBOnlySubQuery(typeof(CusSCAHouse), CusSCAHouseSchema.CA_JS, notIn);
				var oceanBillSubQuery = new ZDBOnlySubQuery(typeof(CusSCAOceanBill), CusSCAOceanBillSchema.PK);
				oceanBillSubQuery.AddToFilter(CusSCAOceanBillSchema.CB_ApplicationCode, CusSCAOceanBill.ApplicationCodes);
				aciSubQuery.AddSubQuery(CusSCAHouseSchema.CA_CB, CusSCAOceanBillSchema.PK, oceanBillSubQuery, JoinCondition.And);
				aciSubQuery.AddToFilter(aciStatusColumn, comparisonOperator, CAExternalColumnsHelper.GetQueryValueForMessageStatusCode(value));
				query.AddSubQuery(aciSubQuery, JoinCondition.Or);
				return query;
			}

			static ZQuery GetEManifestStatusQuery(SQLComparisonOperator comparisonOperator, ZString value, SchemaStringColumn eManifestStatusColumn)
			{
				var query = new ZDBOnlyQuery(typeof(ForwardingShipment));
				var notIn = ReverseOperatorIfIsBlankOrNotEqual(ref comparisonOperator);
				var eManifestSubQuery = new ZDBOnlySubQuery(typeof(CusCAeMHHouse), CusCAeMHHouseSchema.BW_ParentID, notIn);
				eManifestSubQuery.AddToFilter(eManifestStatusColumn, comparisonOperator, CAExternalColumnsHelper.GetQueryValueForMessageStatusCode(value));
				query.AddSubQuery(eManifestSubQuery, JoinCondition.Or);
				return query;
			}

			static IList GetMessageStatusList()
			{
				var result = new MessageStatusList(ResString.GetMultilingualString("89EF9EDC-8F27-464E-A3A0-DB8B89A0382F", "House Bill or Supplementary"));
				result.RemoveCode(MessageStatusList.Codes.NotSent);
				result.RemoveCode(MessageStatusList.Codes.Sent);
				return result;
			}

			static IList GetJobStatusList()
			{
				var result = new EManifestForwarderJobStatusList();
				foreach (CodeDescriptionPair pair in new SupplementaryCargoReportJobStatusList())
				{
					if (!result.ContainsCode(pair.Code))
					{
						result.AddPair(pair.Code, pair.Description);
					}
				}
				return result;
			}

			public void AddColumns(IFilterControl filterControl)
			{
			}
		}
	}
}
