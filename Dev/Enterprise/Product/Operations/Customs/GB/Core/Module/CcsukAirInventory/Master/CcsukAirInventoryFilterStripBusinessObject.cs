using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Biz = Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Module
{
	class CcsukAirInventoryFilterStripBusinessObject : FilterStripBusinessObject
	{
		public static class FilterConstants
		{
			public const string MAWB = "MAWB";
			public const string Shed = "Shed";
			public const string ShedAirport = "Shed airport";
			public const string Agent = "Agent";
			public const string ArrivalDate = "Arrival date";
			public const string DepartureDate = "Departure date";
			public const string DischargeAirport = "Discharge airport";
			public const string ArrivalAirport = "Arrival airport";
			public const string OriginAirport = "Origin airport";
			public const string GoodsDescription = "Goods description";
			public const string Profile = "Profile";
			public const string IsAgent = "Is Agent?";
			public const string IsShed = "Is Shed?";
			public const string HasSplits = "Has splits?";
			public const string Basic = "Basic?";
			public const string CustomsActionCode = "Customs Action Code";
			public const string PresenceOnNetwork = "Presence on Network";
			public const string Status2 = "Status 2";
			public const string UFO = "UFO";
			public const string P5AutoCreated = "P5 (auto-created)";
			public const string ConsolidationOfAllClosedWholeHouses = "Consol of closed whole houses?";
		}

		public CcsukAirInventoryFilterStripBusinessObject()
			: this(true)
		{
		}

		public CcsukAirInventoryFilterStripBusinessObject(bool isCreateDefaultTextFilterStrips)
		{
			this.QueryObjectType = typeof(CusMAWB);
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
			SetActiveStatusFilter(null, false);

			var result = new ModuleFilterCollection();
			result.AddNumberFilter(FilterConstants.MAWB, CusMAWBSchema.CM_MAWB)
				.MultilingualDescription = ResString.GetMultilingualString("9AA91BA6-D7D2-410E-A0D5-33DC6CDEBFF0", FilterConstants.MAWB);
			result.AddTextFilter(FilterConstants.Shed, GetShedQuery)
				.MultilingualDescription = ResString.GetMultilingualString("C2A5423C-88E9-4EAC-B1A3-06C8D52E4F65", FilterConstants.Shed);
			result.AddTextFilter(FilterConstants.ShedAirport, GetShedQuery)
				.MultilingualDescription = ResString.GetMultilingualString("C595D451-2944-4325-A3A9-86C84B681926", FilterConstants.ShedAirport);
			result.AddTextFilter(FilterConstants.Agent, GetAgentQuery)
				.MultilingualDescription = ResString.GetMultilingualString("201959F8-34B6-4C61-816B-95B22CA349F4", FilterConstants.Agent);
			result.AddDateFilter(FilterConstants.ArrivalDate, CusMAWBSchema.CM_ArrivalDate)
				.MultilingualDescription = ResString.GetMultilingualString("2C5D7FC7-96B8-4474-852F-37DFA38D7267", FilterConstants.ArrivalDate);
			result.AddDateFilter(FilterConstants.DepartureDate, CusMAWBSchema.CM_DepartureDate)
				.MultilingualDescription = ResString.GetMultilingualString("1F1C02BB-3F68-4D8F-80EC-D7B8475F7E7B", FilterConstants.DepartureDate);
			result.AddTextFilter(FilterConstants.DischargeAirport, CusMAWBSchema.CM_RL_NKDischargePort)
				.MultilingualDescription = ResString.GetMultilingualString("27DCD799-4A59-429D-BBF6-AC171AFD739C", FilterConstants.DischargeAirport);
			result.AddTextFilter(FilterConstants.ArrivalAirport, CusMAWBSchema.CM_RL_NKFirstArrivalPort)
				.MultilingualDescription = ResString.GetMultilingualString("E6D4D200-95EE-4618-91CA-F4B635F5F414", FilterConstants.ArrivalAirport);
			result.AddTextFilter(FilterConstants.OriginAirport, CusMAWBSchema.CM_RL_NKLoadPort)
				.MultilingualDescription = ResString.GetMultilingualString("91B44A35-04F4-442C-BD9A-E0B3F4A90E07", FilterConstants.OriginAirport);
			result.AddTextFilter(FilterConstants.GoodsDescription, GetGoodDescQuery)
				.MultilingualDescription = ResString.GetMultilingualString("78DFCFA8-5D4D-4456-8D33-CB93C7206610", FilterConstants.GoodsDescription);
			result.AddTextFilter(FilterConstants.Profile, GetPimaQuery)
				.MultilingualDescription = ResString.GetMultilingualString("DED3B676-BFC0-476B-972E-020198B8A536", FilterConstants.Profile);
			result.AddFlagsFilter(FilterConstants.IsAgent, ["Ticked for yes, unticked for no"], new GetFlagsQuery[] { GetIsAgent })
				.MultilingualDescription = ResString.GetMultilingualString("CEA1E9DF-47CC-4B66-A709-3BFD5BBB062B", FilterConstants.IsAgent);
			result.AddFlagsFilter(FilterConstants.IsShed, ["Ticked for yes, unticked for no"], new GetFlagsQuery[] { GetIsShed })
				.MultilingualDescription = ResString.GetMultilingualString("567056C1-7267-4105-81EB-3ADBAE6E8B23", FilterConstants.IsShed);
			result.AddFlagsFilter(FilterConstants.HasSplits, ["Ticked for split, unticked for whole"], new GetFlagsQuery[] { GetHasSplitsQuery })
				.MultilingualDescription = ResString.GetMultilingualString("AC91162D-8AF6-43E7-B9B9-125C161DDF11", FilterConstants.HasSplits);
			result.AddFlagsFilter(FilterConstants.Basic, ["Ticked for basic, unticked for consol"], new GetFlagsQuery[] { GetIsBasicQuery })
				.MultilingualDescription = ResString.GetMultilingualString("3BCDAA0A-916B-47DC-8758-53D21E1F4519", FilterConstants.Basic);
			result.AddTextFilter(FilterConstants.CustomsActionCode, GetCustomsActionCodeFilter, () => new CustomsStatusCodes())
				.MultilingualDescription = ResString.GetMultilingualString("AEAEB4C3-B91C-4A3F-8C62-9677F0F212A5", FilterConstants.CustomsActionCode);
			result.AddTextFilter(FilterConstants.PresenceOnNetwork, GetPresenceOnNetworkFilter, () => new PresenceOnNetworkList())
				.MultilingualDescription = ResString.GetMultilingualString("7925DB10-5CF0-498B-83AE-7E749F4A7BC5", FilterConstants.PresenceOnNetwork);
			result.AddFlagsFilter(FilterConstants.Status2, ["Ticked for granted, unticked for revoked"], new GetFlagsQuery[] { GetStatus2Query })
				.MultilingualDescription = ResString.GetMultilingualString("987598B1-45C4-465F-8444-4500CDCB78F1", FilterConstants.Status2);
			result.AddFlagsFilter(FilterConstants.UFO, ["Is Unidentified Freight Object record?"], new GetFlagsQuery[] { GetIsUfo })
				.MultilingualDescription = ResString.GetMultilingualString("4AC2F873-8B2C-473A-99AD-7A9C85FEB024", FilterConstants.UFO);
			result.AddFlagsFilter(FilterConstants.P5AutoCreated, ["Is auto-created (from P5) record awaiting FRC?"], new GetFlagsQuery[] { GetIsISRAutoCreatedQuery })
				.MultilingualDescription = ResString.GetMultilingualString("35171B79-3FD0-441E-9DE2-4B5F6DDE0594", FilterConstants.P5AutoCreated);
			result.AddFlagsFilter(FilterConstants.ConsolidationOfAllClosedWholeHouses, ["Ticked for yes, unticked for no"], new GetFlagsQuery[] { GetCompletedConsolQuery })
				.MultilingualDescription = ResString.GetMultilingualString("ED4DCBDB-029A-4185-B9B9-765198CE1256", FilterConstants.ConsolidationOfAllClosedWholeHouses);

			return result;
		}

		public override ZQuery Filter
		{
			get
			{
				var query = base.Filter;
				query.AddToFilter(CusMAWBSchema.CM_ApplicationCode, ApplicationCodeList.Codes.GbCcsuk);
				var branchPks = (from GlbBranch b in GlbCompany.CurrentCompany.Branches select b.PK).ToArray();
				query.AddToFilter(CusMAWBSchema.CM_GB, branchPks);
				return query;
			}
		}

		ZQuery GetStatus2Query(ZBool value)
		{
			return MakeWorkerHawbSubQuery(CusHAWBSchema.CS_IsSurplus, SQLComparisonOperator.Equal, value);
		}

		ZQuery GetIsUfo(ZBool value)
		{
			return new ZQuery(CusMAWBSchema.CM_HasProhibitedPackaging, value);
		}

		ZQuery GetIsISRAutoCreatedQuery(ZBool value)
		{
			return MakeWorkerHawbSubQuery(CusHAWBSchema.CS_MsgStatus, value ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, (ZString)PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc);
		}

		ZQuery GetIsAgent(ZBool value)
		{
			return MakeWorkerHawbSubQuery(CusHAWBSchema.CS_FolioReference, value ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.DoesNotStartWith, (ZString)LicenceAndPimaHelper.AgentProfilePrefix);
		}

		ZQuery GetIsShed(ZBool value)
		{
			return MakeWorkerHawbSubQuery(CusHAWBSchema.CS_FolioReference, value ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.DoesNotStartWith, (ZString)LicenceAndPimaHelper.ShedProfilePrefix);
		}

		ZQuery GetIsBasicQuery(ZBool value)
		{
			var result = new ZDBOnlyQuery(typeof(CusMAWB));
			var sub = new ZDBOnlySubQuery(typeof(CusHAWB), CusHAWBSchema.CS_CM, value);
			sub.AddToFilter(CusHAWBSchema.CS_IsMasterHouse, false);
			sub.AddToFilter(CusHAWBSchema.CS_CM, SQLComparisonOperator.NotEqual, DBNull.Value);
			result.AddSubQuery(sub, JoinCondition.And);
			return result;
		}

		ZQuery GetHasSplitsQuery(ZBool value)
		{
			var query = new ZDBOnlyQuery(typeof(CusMAWB));
			var sub = new ZDBOnlySubQuery(typeof(Biz.CusPartShip), CusPartShipSchema.CG_CM_LinkToPartMaster, !value);
			sub.AddToFilter(CusPartShipSchema.CG_CS, SQLComparisonOperator.Equal, DBNull.Value);
			sub.AddToFilter(CusPartShipSchema.CG_CM_LinkToPartMaster, SQLComparisonOperator.NotEqual, DBNull.Value);
			query.AddSubQuery(CusMAWBSchema.PK, sub, JoinCondition.And);
			if (value)
			{
				query.AddToFilter(GetIsBasicQuery(true));
			}
			return query;
		}

		ZQuery GetPresenceOnNetworkFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return MakeWorkerHawbSubQuery(CusHAWBSchema.CS_MsgStatus, comparisonOperator, value);
		}

		ZQuery GetCustomsActionCodeFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return MakeWorkerHawbSubQuery(CusHAWBSchema.CS_CustomsStatus, comparisonOperator, value);
		}

		ZQuery GetPimaQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return MakeWorkerHawbSubQuery(CusHAWBSchema.CS_FolioReference, comparisonOperator, value);
		}

		ZQuery GetShedQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return MakeWorkerHawbSubQuery(CusHAWBSchema.CS_WarehouseLocation, SQLComparisonOperator.Contains, value);
		}

		ZQuery GetAgentQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return MakeWorkerHawbSubQuery(CusHAWBSchema.CS_ResponsiblePartyID, comparisonOperator, value);
		}

		ZQuery GetGoodDescQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return MakeWorkerHawbSubQuery(CusHAWBSchema.CS_GoodsDescription, comparisonOperator, value);
		}

		static ZQuery MakeWorkerHawbSubQuery(SchemaColumn hawbSchemaColumn, SQLComparisonOperator comparisonOperator, IZType value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CusMAWB));
			ZDBOnlySubQuery workerHawbQuery = new ZDBOnlySubQuery(typeof(CusHAWB), CusHAWBSchema.CS_CM);
			workerHawbQuery.AddToFilter(CusHAWBSchema.CS_IsMasterHouse, true);
			workerHawbQuery.AddToFilter(hawbSchemaColumn, comparisonOperator, value);
			result.AddSubQuery(workerHawbQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetCompletedConsolQuery(ZBool value)
		{
			var hasNoHousesWithOpenCacsQueryText = string.Format(@"
					{0} exists
					(
						select * from {1} where {2} = {3} and {4} = 0 and {5} in ('', 'cr', 'ca' , 'cx', 'cq', '--')
					)
					",
					value ? "not" : "", //0
					CusHAWB.Schema.TableName,// 1
					CusHAWB.Schema.CS_CM, //2
					CusMAWB.Schema.PK, // 3
					CusHAWB.Schema.CS_IsMasterHouse, //4 
					CusHAWB.Schema.CS_CustomsStatus // 5
					);

			// Finalised codes are CC, CW, CT, CU, CB, CS, EC
			// Therefore unfinal codes are blank CR, CA, CX, CQ.
			// Code -- indicates splits and we are unsure of their status
			var result = new ZQuery();
			result.AddToFilter(GetIsBasicQuery(false));  // consols only
			result.AddFilterAndZSQLParameterCollection(hasNoHousesWithOpenCacsQueryText, null);
			return result;
		}
	}
}
