using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Biz = Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Module
{
	class CcsukAirInventoryHouseFilterStripBusinessObject : FilterStripBusinessObject
	{
		public static class FilterConstants
		{
			public const string HAWB = "HAWB";
			public const string AirportShed = "Airport+Shed";
			public const string Agent = "Agent";
			public const string GoodsDescription = "Goods description";
			public const string MAWB = "MAWB";
			public const string Weight = "Weight";
			public const string NPR = "NPR";
			public const string NPX = "NPX";
			public const string Status1 = "Status 1 (NPR=NPX)";
			public const string HasEntryLinked = "Has entry linked?";
			public const string HasSplits = "Has splits?";
			public const string IsAgent = "Is Agent?";
			public const string IsShed = "Is Shed?";
			public const string MawbArrivalDate = "Mawb arrival date";
			public const string CustomsActionCode = "Customs Action Code";
			public const string PresenceOnNetwork = "Presence on Network";
			public const string AirportOfArrival = "Airport of Arrival";
			public const string AirportOfOrigin = "Airport of Origin";
			public const string AirportOfDestination = "Airport of Destination";
			public const string CustomsActionDate = "Customs Action Date";
			public const string Status2 = "Status 2";
			public const string P5AutoCreated = "P5 (auto-created)";
		}

		public CcsukAirInventoryHouseFilterStripBusinessObject()
			: this(true)
		{
		}

		public CcsukAirInventoryHouseFilterStripBusinessObject(bool isCreateDefaultTextFilterStrips)
		{
			this.QueryObjectType = typeof(CusHAWB);
			if (isCreateDefaultTextFilterStrips)
			{
				LayoutLoaded += CreateDefaultTextFilterStrips;
			}
		}

		void CreateDefaultTextFilterStrips(object sender, EventArgs e)
		{
			var defaultValues = new ZString[]
			{
				PresenceOnNetworkList.Codes.ArchivedOnCcsuk,
				PresenceOnNetworkList.Codes.NotOnCommDbDeleted
			};
			if (!ActiveModuleFilters.Any(filter => filter is ModuleTextFilter f &&
				f.Description.StartsWith("Presence on Network") && f.SqlComparisonOperator == SQLComparisonOperator.NotEqual && defaultValues.Contains(f.Property)))
			{
				foreach (var defaultValue in defaultValues)
				{
					var textFilterStrip = AddTextFilterStrip("Presence on Network", defaultValue);
					textFilterStrip.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
				}
			}
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddNumberFilter(FilterConstants.HAWB, CusHAWBSchema.CS_HAWB)
				.MultilingualDescription = ResString.GetMultilingualString("D24A0CC8-65F4-4F5A-9F8B-A4DE1A7765B5", FilterConstants.HAWB);
			result.AddTextFilter(FilterConstants.AirportShed, CusHAWBSchema.CS_WarehouseLocation)
				.MultilingualDescription = ResString.GetMultilingualString("306C4776-3EEB-47C7-AA37-97C1A481CFF2", FilterConstants.AirportShed);
			result.AddTextFilter(FilterConstants.Agent, CusHAWBSchema.CS_ResponsiblePartyID)
				.MultilingualDescription = ResString.GetMultilingualString("BB66FCD3-9C86-48CD-8F2A-78EA15EEE64A", FilterConstants.Agent);
			result.AddTextFilter(FilterConstants.GoodsDescription, CusHAWBSchema.CS_GoodsDescription)
				.MultilingualDescription = ResString.GetMultilingualString("4C5496D5-9956-469D-8ECB-B248D5444A2C", FilterConstants.GoodsDescription);
			var mawbFilter = result.AddNumberFilter(FilterConstants.MAWB, GetMawbNumberQuery);
			mawbFilter.MaxLength = CusMAWBSchema.CM_MAWB.MaxLength;
			mawbFilter.MultilingualDescription = ResString.GetMultilingualString("58F1BC9E-6326-4151-B640-261137DA7EDA", FilterConstants.MAWB);
			result.AddNumberRangeFilter(FilterConstants.Weight, CusHAWBSchema.CS_Weight)
				.MultilingualDescription = ResString.GetMultilingualString("5A254ADF-59A0-40B9-B995-C2049CD38505", FilterConstants.Weight);
			result.AddNumberRangeFilter(FilterConstants.NPR, CusHAWBSchema.CS_PiecesLanded)
				.MultilingualDescription = ResString.GetMultilingualString("00AE3815-1298-4723-A18E-4A16239F86F6", FilterConstants.NPR);
			result.AddNumberRangeFilter(FilterConstants.NPX, CusHAWBSchema.CS_PiecesManifested)
				.MultilingualDescription = ResString.GetMultilingualString("7CC9F752-BCB9-46EA-B61A-A39D7B75705A", FilterConstants.NPX);
			result.AddFlagsFilter(FilterConstants.Status1, ["Ticked for set, unticked for unset"], new GetFlagsQuery[] { GetStatus1Query })
				.MultilingualDescription = ResString.GetMultilingualString("DE429823-26A6-4CE3-98B9-7DCFF6687460", FilterConstants.Status1);
			result.AddFlagsFilter(FilterConstants.HasEntryLinked, ["Ticked for yes, unticked for no"], new GetFlagsQuery[] { GetHasEntryQuery })
				.MultilingualDescription = ResString.GetMultilingualString("04AFFAD1-0DB3-463E-923C-A64BE711ADA8", FilterConstants.HasEntryLinked);
			result.AddFlagsFilter(FilterConstants.HasSplits, ["Ticked for split, unticked for whole"], new GetFlagsQuery[] { GetHasSplitsQuery })
				.MultilingualDescription = ResString.GetMultilingualString("7D2AEA18-94EA-4607-ACBF-0FC80E148E55", FilterConstants.HasSplits);
			result.AddFlagsFilter(FilterConstants.IsAgent, ["Ticked for yes, unticked for no"], new GetFlagsQuery[] { GetIsAgent })
				.MultilingualDescription = ResString.GetMultilingualString("2B7882D2-9B4B-421E-8B04-136E1FF811F9", FilterConstants.IsAgent);
			result.AddFlagsFilter(FilterConstants.IsShed, ["Ticked for yes, unticked for no"], new GetFlagsQuery[] { GetIsShed })
				.MultilingualDescription = ResString.GetMultilingualString("3A4A3097-3F9A-42E0-BECE-5CA213091418", FilterConstants.IsShed);
			result.AddDateFilter(FilterConstants.MawbArrivalDate, GetMawbDateQuery)
				.MultilingualDescription = ResString.GetMultilingualString("009DB4E8-A3BB-4E00-A39B-FD27E5C2B152", FilterConstants.MawbArrivalDate);
			result.AddTextFilter(FilterConstants.CustomsActionCode, CusHAWBSchema.CS_CustomsStatus, () => new CustomsStatusCodes())
				.MultilingualDescription = ResString.GetMultilingualString("12247562-4AB0-4676-B9F1-1ECD59F8CBAE", FilterConstants.CustomsActionCode);
			result.AddTextFilter(FilterConstants.PresenceOnNetwork, CusHAWBSchema.CS_MsgStatus, () => new PresenceOnNetworkList())
				.MultilingualDescription = ResString.GetMultilingualString("3ECDB003-E45B-44A5-989D-0FB75CA70415", FilterConstants.PresenceOnNetwork);
			result.AddTextFilter(FilterConstants.AirportOfArrival, CusHAWBSchema.CS_RL_NKDestination)
				.MultilingualDescription = ResString.GetMultilingualString("2A4B276D-F710-4917-8D67-6B5E37B592AB", FilterConstants.AirportOfArrival);
			result.AddTextFilter(FilterConstants.AirportOfOrigin, CusHAWBSchema.CS_RL_NKLoadPort)
				.MultilingualDescription = ResString.GetMultilingualString("16992460-4C7D-4BF8-9870-7E5203441041", FilterConstants.AirportOfOrigin);
			result.AddTextFilter(FilterConstants.AirportOfDestination, CusHAWBSchema.CS_RL_NKDischargePort)
				.MultilingualDescription = ResString.GetMultilingualString("2AD1A3AC-C767-46EB-B063-1C0FB7219E4A", FilterConstants.AirportOfDestination);
			result.AddDateFilter(FilterConstants.CustomsActionDate, GetCustomsActionDateQuery)
				.MultilingualDescription = ResString.GetMultilingualString("DED363AC-9E88-476F-A453-CBA9B2CB74B0", FilterConstants.CustomsActionDate);
			result.AddFlagsFilter(FilterConstants.Status2, ["Ticked for granted, unticked for revoked"], new GetFlagsQuery[] { GetStatus2Query })
				.MultilingualDescription = ResString.GetMultilingualString("C434E8AE-6C39-41D3-AB7D-E7F95387CFA3", FilterConstants.Status2);
			result.AddFlagsFilter(FilterConstants.P5AutoCreated, ["Is auto-created (from P5) record awaiting FRC?"], new GetFlagsQuery[] { GetIsISRAutoCreatedQuery })
				.MultilingualDescription = ResString.GetMultilingualString("7EF8DEE1-DCEB-4D55-A63F-658E78A301BE", FilterConstants.P5AutoCreated);

			return result;
		}

		ZQuery GetStatus2Query(ZBool value)
		{
			return new ZQuery(CusHAWBSchema.CS_IsSurplus, value);
		}

		ZQuery GetIsISRAutoCreatedQuery(ZBool value)
		{
			return new ZQuery(CusHAWBSchema.CS_MsgStatus, value ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, (ZString)PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc);
		}

		ZQuery GetIsAgent(ZBool value)
		{
			var query = new ZQuery();
			query.AddToFilter(CusHAWBSchema.CS_FolioReference, value ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.DoesNotStartWith, LicenceAndPimaHelper.AgentProfilePrefix);
			return query;
		}

		ZQuery GetIsShed(ZBool value)
		{
			var query = new ZQuery();
			query.AddToFilter(CusHAWBSchema.CS_FolioReference, value ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.DoesNotStartWith, LicenceAndPimaHelper.ShedProfilePrefix);
			return query;
		}

		ZQuery GetCustomsActionDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var query = new ZDBOnlyQuery(typeof(CusHAWB));
			var sub = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent);
			sub.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code);
			sub.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Equal, CusHAWBSchema.CS_CustomsStatus);
			AddDateTimeRange(sub, comparisonOperator, JoinCondition.And, StmALogSchema.SL_EventTime, value1, value2);
			query.AddSubQuery(sub, JoinCondition.And);
			return query;
		}

		protected virtual ZQuery GetHasSplitsQuery(ZBool value)
		{
			var query = new ZDBOnlyQuery(typeof(CusHAWB));
			var sub = new ZDBOnlySubQuery(typeof(Biz.CusPartShip), CusPartShipSchema.CG_CS, !value);
			sub.AddToFilter(CusPartShipSchema.CG_CS, SQLComparisonOperator.NotEqual, DBNull.Value);
			sub.AddToFilter(CusPartShipSchema.CG_CM_LinkToPartMaster, SQLComparisonOperator.Equal, DBNull.Value);
			query.AddSubQuery(CusHAWBSchema.PK, sub, JoinCondition.And);
			return query;
		}

		ZQuery GetStatus1Query(ZBool value)
		{
			var query = new ZQuery();
			query.AddToFilter(CusHAWBSchema.CS_PiecesLanded, value ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, CusHAWBSchema.CS_PiecesManifested);
			return query;
		}

		ZQuery GetHasEntryQuery(ZBool value)
		{
			var query = new ZQuery();
			query.AddToFilter(CusHAWBSchema.CS_JE_CustomsFormalEntry, value ? SQLComparisonOperator.NotEqual : SQLComparisonOperator.Equal, DBNull.Value);
			return query;
		}

		ZQuery GetMawbNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(CusHAWB));
			EnforceNotMasterHouse(query);
			var sub = new ZDBOnlySubQuery(typeof(CusMAWB), CusHAWBSchema.CS_CM);
			sub.AddToFilter(GbInterchangeSender.BritishBranches(Factory, CusMAWBSchema.CM_GB));
			sub.AddToFilter_PossiblyCommaSeparated(CusMAWBSchema.CM_MAWB, comparisonOperator, value);
			query.AddSubQuery(sub, JoinCondition.And);
			return query;
		}

		ZQuery GetMawbDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var query = new ZDBOnlyQuery(typeof(CusHAWB));
			EnforceNotMasterHouse(query);
			var sub = new ZDBOnlySubQuery(typeof(CusMAWB), CusHAWBSchema.CS_CM);
			sub.AddToFilter(GbInterchangeSender.BritishBranches(Factory, CusMAWBSchema.CM_GB));
			AddDateTimeRange(sub, comparisonOperator, JoinCondition.And, CusMAWBSchema.CM_ArrivalDate, value1, value2);
			query.AddSubQuery(sub, JoinCondition.And);
			return query;
		}

		public override ZQuery Filter
		{
			get
			{
				var baseFilter = base.Filter;
				var query = new ZDBOnlyQuery(typeof(CusHAWB));
				query.AddToFilter(baseFilter);
				EnforceNotMasterHouse(query);
				query.AddToFilter(CusHAWBSchema.CS_ApplicationCode, ApplicationCodeList.Codes.GbCcsuk);
				var sub = new ZDBOnlySubQuery(typeof(CusMAWB), CusHAWBSchema.CS_CM);
				var ukBranchPks = (from GlbBranch b in GlbCompany.CurrentCompany.Branches select b.PK).ToArray();
				sub.AddToFilter(CusMAWBSchema.CM_GB, ukBranchPks);
				query.AddSubQuery(sub, JoinCondition.And);
				return query;
			}
		}

		protected virtual void EnforceNotMasterHouse(ZDBOnlyQuery query)
		{
			query.AddToFilter(CusHAWBSchema.CS_IsMasterHouse, false);
		}
	}
}
