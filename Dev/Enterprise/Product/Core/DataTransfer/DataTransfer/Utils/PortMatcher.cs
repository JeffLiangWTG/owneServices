using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Business
{
	public class PortMatcher : Matcher<ZString>
	{
		#region Constructor

		public PortMatcher(BusinessObjectFactory factory, ZString unmappedPortName, INotifications notifications, ZGuid mappingOrgPK)
		{
			this.Factory = factory;
			this.UnmappedPortName = unmappedPortName;
			this.Notifications = notifications;
			this.MappingOrgPK = mappingOrgPK;
		}

		#endregion

		#region By Org Pattern

		protected MatchResult ByOrgPatternMatch()
		{
			MatchResult result = new MatchResult();

			// try finding the port by the org pattern match
			ZQuery filter = new ZQuery();
			filter.AddToFilter(JoinCondition.And, OrgPatternMatchOverrideSchema.OO_OH, SQLComparisonOperator.Equal, this.MappingOrgPK);
			filter.AddToFilter(JoinCondition.And, OrgPatternMatchOverrideSchema.OO_Relationship, SQLComparisonOperator.Equal, Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.Port);
			filter.AddToFilter(JoinCondition.And, OrgPatternMatchOverrideSchema.OO_ForeignCode, SQLComparisonOperator.Equal, UnmappedPortName);
			OrgPatternMatchOverride match = (OrgPatternMatchOverride)Factory.LoadTop1(typeof(OrgPatternMatchOverride), filter);

			if (match != null)
			{
				ZGuid portPK = match.OO_LocalGuid;
				RefUNLOCO port = (RefUNLOCO)Factory.Load(typeof(RefUNLOCO), portPK);
				if (port != null)
				{
					result.Value = port.RL_Code;
				}
			}

			return result;
		}

		#endregion

		#region Empty

		protected MatchResult ByEmpty()
		{
			MatchResult result = new MatchResult();

			if (UnmappedPortName.Trim().IsEmpty)
			{
				result.HasMatched = true;
			}

			return result;
		}

		#endregion

		#region ByUSCode

		protected MatchResult ByUSCode()
		{
			MatchResult result = new MatchResult();
			ZString matchedUnloco = ZString.Empty;
			ZString trimUnmappedPortName = UnmappedPortName.Trim();
			if (IsScheduleD(trimUnmappedPortName))
			{
				matchedUnloco = GetUnlocoMatchingDSchedule(trimUnmappedPortName);
			}
			else if (IsScheduleK(trimUnmappedPortName))
			{
				matchedUnloco = GetUnlocoCodeIfScheduleKIsMappedToUnloco(trimUnmappedPortName);
			}
			if (!matchedUnloco.IsEmpty)
			{
				result.Value = matchedUnloco;
			}
			return result;
		}

		ZString GetUnlocoMatchingDSchedule(ZString scheduleDCode)
		{
			RefLocoMap locoMap = Factory.LoadTop1<RefLocoMap>(GetScheduleDFilter(scheduleDCode));
			return (locoMap == null) ? ZString.Empty : locoMap.RY_RL_NKLocoPort;
		}

		ZQuery GetScheduleDFilter(ZString scheduleDCode)
		{
			ZQuery scheduleDFilter = new ZQuery(RefLocoMapSchema.RY_SystemUsage, USLocoMapSystemUsageList.Codes.All);
			scheduleDFilter.AddToFilter(JoinCondition.Or, RefLocoMapSchema.RY_SystemUsage, USLocoMapSystemUsageList.Codes.Air);
			scheduleDFilter.AddToFilter(JoinCondition.Or, RefLocoMapSchema.RY_SystemUsage, USLocoMapSystemUsageList.Codes.Sea);
			scheduleDFilter.AddToFilter(JoinCondition.Or, RefLocoMapSchema.RY_SystemUsage, USLocoMapSystemUsageList.Codes.SCD);
			ZQuery query = new ZQuery(RefLocoMapSchema.RY_RN, Core.Constants.CountryGuids.UnitedStates);
			query.AddToFilter(RefLocoMapSchema.RY_LocalPortCode, scheduleDCode);
			query.AddToFilter(scheduleDFilter);
			return query;
		}

		ZString GetUnlocoCodeIfScheduleKIsMappedToUnloco(ZString scheduleKCode)
		{
			RefLocoMap locoMap = Factory.LoadTop1<RefLocoMap>(GetScheduleKFilter(scheduleKCode));
			return (locoMap != null) ? locoMap.RY_RL_NKLocoPort : ZString.Empty;
		}

		ZQuery GetScheduleKFilter(ZString scheduleKCode)
		{
			ZQuery query = new ZQuery(RefLocoMapSchema.RY_RN, Core.Constants.CountryGuids.UnitedStates);
			query.AddToFilter(RefLocoMapSchema.RY_SystemUsage, USLocoMapSystemUsageList.Codes.SCK);
			query.AddToFilter(RefLocoMapSchema.RY_LocalPortCode, scheduleKCode);
			return query;
		}

		bool IsScheduleD(ZString trimUnmappedPortName)
		{
			ZString numberOnly = trimUnmappedPortName.KeepNumericCharacters();
			return numberOnly.Length == 4 && numberOnly == trimUnmappedPortName;
		}

		bool IsScheduleK(ZString trimUnmappedPortName)
		{
			ZString numberOnly = trimUnmappedPortName.KeepNumericCharacters();
			return numberOnly.Length == 5 && numberOnly == trimUnmappedPortName;
		}

		#endregion

		#region By Port Name

		protected MatchResult ByPortName()
		{
			MatchResult result = new MatchResult();

			// try finding by port name
			RefUNLOCO[] portByNameMatches = (RefUNLOCO[])Factory.Load(
				typeof(RefUNLOCO),
				new ZQuery(RefUNLOCOSchema.RL_PortName, SQLComparisonOperator.Equal, UnmappedPortName));

			if (portByNameMatches.Length == 1)
			{
				result.Value = portByNameMatches[0].RL_Code;
			}

			return result;
		}

		#endregion

		#region By UNLOCO

		protected MatchResult ByUNLOCO()
		{
			MatchResult result = new MatchResult();

			RefUNLOCO[] portByCodeMatches = (RefUNLOCO[])Factory.Load(
				typeof(RefUNLOCO),
				new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.Equal, UnmappedPortName));

			if (portByCodeMatches.Length > 0)
			{
				result.Value = portByCodeMatches[0].RL_Code;
			}

			return result;
		}

		#endregion

		#region By Country

		protected MatchResult ByCountryCode()
		{
			MatchResult result = new MatchResult();

			RefCountry[] countryMatches = (RefCountry[])Factory.Load(
				typeof(RefCountry),
				new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.Equal, UnmappedPortName));

			if (countryMatches.Length > 0)
			{
				result.Value = countryMatches[0].RN_Code;
			}

			return result;
		}

		#endregion

		#region Else

		protected MatchResult Else()
		{
			MatchResult result = new MatchResult();

			// if couldn't find, just store the first 5 chars of whatever we have
			ZString shortenedLoco = UnmappedPortName.Replace(" ", "");
			if (shortenedLoco.Length > 5)
			{
				shortenedLoco = new ZString(shortenedLoco.Substring(0, 5));
			}
			result.Value = shortenedLoco.ToUpper();

			Notifications.Notify(new WarningNotification(Res.GetString("97f17441-2a04-41ad-a9b5-06bc72028b91", "Could not find Port ({0})", UnmappedPortName.ToString())));

			return result;
		}

		#endregion

		#region Match Delegates

		protected override MatchDelegate[] matchDelegates
		{
			get
			{
				return new MatchDelegate[]
				{
					ByOrgPatternMatch,
					ByEmpty,
					ByUSCode,
					ByCountryCode,
					ByUNLOCO,
					ByPortName,
					Else
				};
			}
		}

		#endregion

		#region Implementation

		readonly BusinessObjectFactory Factory;
		readonly ZString UnmappedPortName;
		readonly INotifications Notifications;
		readonly ZGuid MappingOrgPK;

		#endregion
	}
}
