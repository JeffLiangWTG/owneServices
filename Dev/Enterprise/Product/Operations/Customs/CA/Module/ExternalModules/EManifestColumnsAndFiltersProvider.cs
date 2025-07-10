using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using IFilterControl = Enterprise.Integration.ZArchitecture.IFilterControl;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	partial class CAConsolModuleColumnsAndFiltersProvider
	{
		internal class EManifestColumnsAndFiltersProvider : Integration.Customs.CA.IConsolModuleColumnsAndFiltersProvider
		{
			internal static class Captions
			{
				internal static MultilingualString CloseJobStatusMultilingualDescription
				{
					get { return ResString.GetMultilingualString("E12D3886-4C11-47C4-84C4-4C710B945D39", CloseJobStatusFilterId); }
				}
				internal const string CloseJobStatusFilterId = "CA Close Status";

				internal static MultilingualString CloseMessageStatusMultilingualDescription
				{
					get { return ResString.GetMultilingualString("A62CB496-3670-40A8-BC72-153644DEB0F5", CloseMessageStatusFilterId); }
				}
				internal const string CloseMessageStatusFilterId = "CA Close Msg. Sta";

				internal static MultilingualString HouseJobStatusMultilingualDescription
				{
					get { return ResString.GetMultilingualString("3B0CD807-FF8F-4932-AEE7-BC004EDCDB18", HouseJobStatusFilterId); }
				}
				internal const string HouseJobStatusFilterId = "CA House Status";

				internal static MultilingualString HouseMessageStatusMultilingualDescription
				{
					get { return ResString.GetMultilingualString("594F98B9-2E20-4BAC-A463-62F53B00430B", HouseMessageStatusFilterId); }
				}
				internal const string HouseMessageStatusFilterId = "CA House Msg. Sta";

				internal static MultilingualString CAHouseLatestD4NoticeMultilingualDescription
				{
					get { return ResString.GetMultilingualString("670BE2C8-8153-4068-AD25-921D4C0D4C41", CAHouseLatestD4NoticeFilterId); }
				}
				internal const string CAHouseLatestD4NoticeFilterId = "CA House Latest D4 Notice";

				internal static MultilingualString CAHouseLatestD4NoticeDescriptionMultilingualDescription
				{
					get { return ResString.GetMultilingualString("7B5C6DC7-7A5E-4D9F-A692-6C1565E249B5", CAHouseLatestD4NoticeDescriptionFilterId); }
				}
				internal const string CAHouseLatestD4NoticeDescriptionFilterId = "CA House Latest D4 Notice Description";

				internal static MultilingualString CAMasterLatestD4NoticeMultilingualDescription
				{
					get { return ResString.GetMultilingualString("8D3BC6FE-F206-4465-BA5C-F217149E40CC", CAMasterLatestD4NoticeFilterId); }
				}
				internal const string CAMasterLatestD4NoticeFilterId = "CA Master Latest D4 Notice";

				internal static MultilingualString CAMasterLatestD4NoticeDescriptionMultilingualDescription
				{
					get { return ResString.GetMultilingualString("F2CC91C4-53DA-4C1E-964A-92BA96AAFB8B", CAMasterLatestD4NoticeDescriptionFilterId); }
				}
				internal const string CAMasterLatestD4NoticeDescriptionFilterId = "CA Master Latest D4 Notice Description";
			}

			static class ColumnSchema
			{
				internal const string CACloseStatus = "CACloseStatus";
				internal const string CACloseMsgStatus = "CACloseMsgStatus";
				internal const string CAMasterLatestD4Notice = "CAMasterLatestD4Notice";
				internal const string CAMasterLatestD4NoticeDescription = "CAMasterLatestD4NoticeDescription";
				internal const string CAHouseLatestD4Notice = "CAHouseLatestD4Notice";
				internal const string CAHouseLatestD4NoticeDescription = "CAHouseLatestD4NoticeDescription";
			}

			public void AddFilters(IModuleFilterCollection filters, BusinessObjectFactory factory)
			{
				var collection = (ModuleFilterCollection)filters;
				collection.AddFilter(new EqualModuleStatusFilter(
					Captions.CloseJobStatusFilterId,
					Captions.CloseJobStatusMultilingualDescription,
					(c, v) => GetStatusQuery(c, v, CusCAeMHMasterSchema.BP_CustomsStatus),
					GetJobStatusList));
				collection.AddFilter(new EqualAndBlankModuleStatusFilter(
					Captions.CAMasterLatestD4NoticeFilterId,
					Captions.CAMasterLatestD4NoticeMultilingualDescription,
					(c, v) => GetStatusQuery(c, v, CusCAeMHMasterSchema.BP_D4MessageStatus),
					() => GetNoticeReasonCodeList(factory)));
				collection.AddFilter(new EqualModuleStatusFilter(
					Captions.CloseMessageStatusFilterId,
					Captions.CloseMessageStatusMultilingualDescription,
					(c, v) => GetStatusQuery(c, v, CusCAeMHMasterSchema.BP_MessageStatus),
					GetMessageStatusList));
				collection.AddFilter(new EqualModuleStatusFilter(
					Captions.HouseJobStatusFilterId,
					Captions.HouseJobStatusMultilingualDescription,
					(c, v) => GetHouseStatusQuery(c, v, CusCAeMHHouseSchema.BW_CustomsStatus),
					GetJobStatusList));
				collection.AddFilter(new EqualModuleStatusFilter(
					Captions.HouseMessageStatusFilterId,
					Captions.HouseMessageStatusMultilingualDescription,
					(c, v) => GetHouseStatusQuery(c, v, CusCAeMHHouseSchema.BW_MessageStatus),
					GetMessageStatusList));
				collection.AddFilter(new EqualAndBlankModuleStatusFilter(
					Captions.CAHouseLatestD4NoticeFilterId,
					Captions.CAHouseLatestD4NoticeMultilingualDescription,
					(c, v) => GetHouseStatusQuery(c, v, CusCAeMHHouseSchema.BW_D4MessageStatus),
					() => GetNoticeReasonCodeList(factory)));
			}

			static ZQuery GetStatusQuery(SQLComparisonOperator comparisonoperator, ZString value, SchemaStringColumn statusColumn1)
			{
				var comparisonOperatorDictionary = new Dictionary<SQLComparisonOperator, SQLComparisonOperator> {
					{ SQLComparisonOperator.NotEqual, SQLComparisonOperator.Equal },
					{ SQLComparisonOperator.NotContains, SQLComparisonOperator.Contains },
					{ SQLComparisonOperator.DoesNotStartWith, SQLComparisonOperator.StartsWith },
					{ SQLComparisonOperator.DoesNotEndWith, SQLComparisonOperator.EndsWith },
					{ SQLComparisonOperator.IsBlank, SQLComparisonOperator.IsNotBlank },
				};
				var isNotEqual = comparisonOperatorDictionary.ContainsKey(comparisonoperator);

				var query = new ZDBOnlyQuery(typeof(ForwardingConsol));
				var subQuery = new ZDBOnlySubQuery(typeof(CusCAeMHMaster), CusCAeMHMasterSchema.BP_ParentID, isNotEqual);
				subQuery.AddToFilter(statusColumn1, isNotEqual ? comparisonOperatorDictionary[comparisonoperator] : comparisonoperator, CAExternalColumnsHelper.GetQueryValueForMessageStatusCode(value));
				query.AddSubQuery(subQuery, JoinCondition.And);
				return query;
			}

			static ZQuery GetHouseStatusQuery(SQLComparisonOperator comparisonoperator, ZString value, SchemaStringColumn statusColumn1)
			{
				var comparisonOperatorDictionary = new Dictionary<SQLComparisonOperator, SQLComparisonOperator> {
					{ SQLComparisonOperator.NotEqual, SQLComparisonOperator.Equal },
					{ SQLComparisonOperator.NotContains, SQLComparisonOperator.Contains },
					{ SQLComparisonOperator.DoesNotStartWith, SQLComparisonOperator.StartsWith },
					{ SQLComparisonOperator.DoesNotEndWith, SQLComparisonOperator.EndsWith },
					{ SQLComparisonOperator.IsBlank, SQLComparisonOperator.IsNotBlank },
				};
				var isNotEqual = comparisonOperatorDictionary.ContainsKey(comparisonoperator);

				var query = new ZDBOnlyQuery(typeof(ForwardingConsol));
				var masterSubQuery = new ZDBOnlySubQuery(typeof(CusCAeMHMaster), CusCAeMHMasterSchema.BP_ParentID, isNotEqual);
				var houseSubQuery = new ZDBOnlySubQuery(typeof(CusCAeMHHouse), CusCAeMHHouseSchema.BW_BP_Master);
				houseSubQuery.AddToFilter(statusColumn1, isNotEqual ? comparisonOperatorDictionary[comparisonoperator] : comparisonoperator, CAExternalColumnsHelper.GetQueryValueForMessageStatusCode(value));
				masterSubQuery.AddSubQuery(houseSubQuery, JoinCondition.And);
				query.AddSubQuery(masterSubQuery, JoinCondition.And);
				return query;
			}

			static IList GetJobStatusList()
			{
				var result = new EManifestForwarderJobStatusList();
				return result;
			}

			static IList GetNoticeReasonCodeList(BusinessObjectFactory factory)
			{
				var result = ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CANoticeReasonCode, ZDateTime.Today);
				if (!result.IsLoaded)
				{
					result.Load();
					result.Sort(RefCusCodeListSchema.Constants.ZZD_Code);
				}
				return result;
			}

			static IList GetMessageStatusList()
			{
				var result = new MessageStatusList();
				result.RemoveCode(MessageStatusList.Codes.NotSent);
				result.RemoveCode(MessageStatusList.Codes.Sent);
				result.AddPair(CAExternalColumnsHelper.Constants.NotSentCode, CAExternalColumnsHelper.Constants.NotSentDescription);
				return result;
			}

			public void AddColumns(IFilterControl filterControl)
			{
				var control = (ZFilterStripControl)filterControl;
				var propertyContainer = new CustomPropertyContainer();
				propertyContainer.AddCustomProperty(ColumnSchema.CACloseStatus, Captions.CloseJobStatusMultilingualDescription, typeof(ZString), GetCloseStatus);
				propertyContainer.AddCustomProperty(ColumnSchema.CACloseMsgStatus, Captions.CloseMessageStatusMultilingualDescription, typeof(ZString), GetCloseMsgStatus);
				propertyContainer.AddCustomProperty(ColumnSchema.CAMasterLatestD4Notice, Captions.CAMasterLatestD4NoticeMultilingualDescription, typeof(ZString), GetMasterLatestD4Notice);
				propertyContainer.AddCustomProperty(ColumnSchema.CAMasterLatestD4NoticeDescription, Captions.CAMasterLatestD4NoticeDescriptionMultilingualDescription, typeof(ZString), GetMasterLatestD4NoticeDescription);
				propertyContainer.AddCustomProperty(ColumnSchema.CAHouseLatestD4Notice, Captions.CAHouseLatestD4NoticeMultilingualDescription, typeof(ZString), GetHouseLatestD4Notice);
				propertyContainer.AddCustomProperty(ColumnSchema.CAHouseLatestD4NoticeDescription, Captions.CAHouseLatestD4NoticeDescriptionMultilingualDescription, typeof(ZString), GetHouseLatestD4NoticeDescription);
				new CAExternalColumnsHelper.CAExternalModuleCustomColumnsInitializer(control.FilteredGrid, control.GridCollection, null, propertyContainer).AddCustomColumns();
			}

			static object GetCloseStatus(BusinessObject consol)
			{
				var master = consol.Factory.LoadTop1<CusCAeMHMaster>(new ZQuery(CusCAeMHMasterSchema.BP_ParentID, consol.PK));
				if (master != null && !master.BP_CustomsStatus.IsEmpty)
				{
					return CAExternalColumnsHelper.GetCodeDescriptionFormatted(master.BP_CustomsStatus, master.BP_CustomsStatusDescription);
				}
				else
				{
					return master != null ? CAExternalColumnsHelper.GetCodeDescriptionFormatted(CAExternalColumnsHelper.Constants.NotSentCode, CAExternalColumnsHelper.Constants.NotSentDescription) : ZString.Empty;
				}
			}

			static object GetCloseMsgStatus(BusinessObject consol)
			{
				var master = consol.Factory.LoadTop1<CusCAeMHMaster>(new ZQuery(CusCAeMHMasterSchema.BP_ParentID, consol.PK));
				if (master != null && !master.BP_MessageStatus.IsEmpty)
				{
					return CAExternalColumnsHelper.GetCodeDescriptionFormatted(master.BP_MessageStatus, master.BP_MessageStatusDescription);
				}
				else
				{
					return master != null ? CAExternalColumnsHelper.GetCodeDescriptionFormatted(CAExternalColumnsHelper.Constants.NotSentCode, CAExternalColumnsHelper.Constants.NotSentDescription) : ZString.Empty;
				}
			}

			static object GetMasterLatestD4Notice(BusinessObject consol)
			{
				var master = consol.Factory.LoadTop1<CusCAeMHMaster>(new ZQuery(CusCAeMHMasterSchema.BP_ParentID, consol.PK));
				if (master != null && !master.BP_D4MessageStatus.IsEmpty)
				{
					return master.BP_D4MessageStatus;
				}
				else
				{
					return ZString.Empty;
				}
			}

			static object GetMasterLatestD4NoticeDescription(BusinessObject consol)
			{
				var master = consol.Factory.LoadTop1<CusCAeMHMaster>(new ZQuery(CusCAeMHMasterSchema.BP_ParentID, consol.PK));
				if (master != null && !master.BP_D4MessageStatusDescription.IsEmpty)
				{
					return master.BP_D4MessageStatusDescription;
				}
				else
				{
					return ZString.Empty;
				}
			}

			static object GetHouseLatestD4Notice(BusinessObject consol)
			{
				var master = consol.Factory.LoadTop1<CusCAeMHMaster>(new ZQuery(CusCAeMHMasterSchema.BP_ParentID, consol.PK));
				if (master != null && !master.HouseBillLatestD4MessageStatus.IsEmpty)
				{
					return master.HouseBillLatestD4MessageStatus;
				}
				else
				{
					return ZString.Empty;
				}
			}

			static object GetHouseLatestD4NoticeDescription(BusinessObject consol)
			{
				var master = consol.Factory.LoadTop1<CusCAeMHMaster>(new ZQuery(CusCAeMHMasterSchema.BP_ParentID, consol.PK));
				if (master != null && !master.HouseBillLatestD4MessageStatusDescription.IsEmpty)
				{
					return master.HouseBillLatestD4MessageStatusDescription;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		[WTG.StaticAnalysis.Annotation.CodeAlive("Is used by CA custom")]
		class CAeMHMasterQueryCreator
		{
			public static ZDBOnlyQuery CreateQueryWithHouseSubQuery(SchemaColumn houseSchemaColumn, SQLComparisonOperator comparisonOperator, object value)
			{
				var result = new ZDBOnlyQuery(typeof(CusCAeMHMaster));
				var houseQuery = new ZDBOnlySubQuery(typeof(CusCAeMHHouse), CusCAeMHHouseSchema.BW_BP_Master);
				houseQuery.AddToFilter(houseSchemaColumn, comparisonOperator, value);
				result.AddSubQuery(houseQuery, JoinCondition.And);
				return result;
			}

			public static ZDBOnlyQuery CreateQueryWithHouseSubQuery(SchemaColumn houseSchemaColumn, object value)
			{
				return CreateQueryWithHouseSubQuery(houseSchemaColumn, SQLComparisonOperator.Equal, value);
			}
		}
	}
}
