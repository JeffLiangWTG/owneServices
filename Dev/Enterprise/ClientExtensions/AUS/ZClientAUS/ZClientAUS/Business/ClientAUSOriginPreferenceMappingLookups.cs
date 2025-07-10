//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientAUSOriginPreferenceMappingLookups
//
//    This class should be used for overriding collections in AutoClientAUSOriginPreferenceMappingLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.AUS.Business
{
	public class ClientAUSOriginPreferenceMappingLookups : AutoClientAUSOriginPreferenceMappingLookups
	{
		public ClientAUSOriginPreferenceMappingLookups(AutoClientAUSOriginPreferenceMapping parent) : base(parent)
		{
		}

		public OrgHeaderCollection Organisations
		{
			get
			{
				if (fOrganisations == null)
				{
					fOrganisations = new OrgHeaderCollection(Factory);
				}

				return fOrganisations;
			}
		}

		OrgHeaderCollection fOrganisations;

		#region T7_ORG_List

		public RefCountryCollection T7_ORG_List
		{
			get
			{
				if (fT7_ORG_List == null)
				{
					fT7_ORG_List = new RefCountryCollection(Factory);
				}

				return fT7_ORG_List;
			}
		}

		RefCountryCollection fT7_ORG_List;

		#endregion

		#region T7_PRT_List

		public CodeDescriptionPairList T7_PRT_List
		{
			get
			{
				CodeDescriptionPairList result = null;
				if (ClientAUSOriginPreferenceMapping.T7_PreferenceSchemeType.IsEmpty)
				{
					result =  RulesListWithoutScheme;
				}
				else
				{
					result = RulesWithScheme;
				}

				return result;
			}
		}

		CodeDescriptionPairList RulesWithScheme
		{
			get
			{
				ZQuery filter = new ZQuery(CMRPreferenceSchemeRuleSchema.PR_PreferenceSchemePeriodSnapshotSchemeType, ClientAUSOriginPreferenceMapping.T7_PreferenceSchemeType);
				filter.OrderBy = CMRPreferenceSchemeRuleSchema.Constants.PR_RuleType;

				if (fRulesWithScheme == null || fRulesWithSchemeCollection == null || !fRulesWithSchemeCollection.LoadedFilterMatches(filter))
				{
					fRulesWithSchemeCollection = new CMRPreferenceSchemeRuleCollection(Factory, filter);
					fRulesWithSchemeCollection.Load();

					fRulesWithScheme = new CodeDescriptionPairList();
					fRulesWithScheme.AddRange(fRulesWithSchemeCollection);
				}
				return fRulesWithScheme;
			}
		}
		CodeDescriptionPairList fRulesWithScheme;
		CMRPreferenceSchemeRuleCollection fRulesWithSchemeCollection;

		CodeDescriptionPairList RulesListWithoutScheme
		{
			get
			{
				ZQuery filter = FilterForStartAndEndDate(CMRPreferenceRulePeriodSnapshotSchema.PU_StartDate, CMRPreferenceRulePeriodSnapshotSchema.PU_EndDate);
				filter.OrderBy = CMRPreferenceRulePeriodSnapshotSchema.Constants.PU_RuleType;

				if (fRuleListWithoutScheme == null || fRulesListWithoutSchemeCollection == null || !fRulesListWithoutSchemeCollection.LoadedFilterMatches(filter))
				{
					fRulesListWithoutSchemeCollection = new CMRPreferenceRulePeriodSnapshotCollection(Factory, filter);
					fRulesListWithoutSchemeCollection.Load();

					fRuleListWithoutScheme = new CodeDescriptionPairList();
					fRuleListWithoutScheme.AddRange(fRulesListWithoutSchemeCollection);
				}

				return fRuleListWithoutScheme;
			}
		}
		CodeDescriptionPairList fRuleListWithoutScheme;
		CMRPreferenceRulePeriodSnapshotCollection fRulesListWithoutSchemeCollection;

		#endregion

		#region T7_PST_List

		public CodeDescriptionPairList T7_PST_List
		{
			get
			{
				CodeDescriptionPairList result = null;

				if (!ClientAUSOriginPreferenceMapping.T7_RN_NKPreferenceOrigin.IsEmpty)
				{
					result = PreferenceSchemeWithOrigin;
				}
				else
				{
					result = FlatPreferenceScheme;
				}
				return result;
			}
		}

		CodeDescriptionPairList PreferenceSchemeWithOrigin
		{
			get
			{
				ZQuery filter = new ZQuery(CMRPreferenceSchemePeriodCountrySchema.PC_CountryCode, SQLComparisonOperator.Equal, ClientAUSOriginPreferenceMapping.T7_RN_NKPreferenceOrigin);

				if (fPreferenceSchemeWithOrigin == null || fPreferenceSchemeWithOriginCollection == null || !fPreferenceSchemeWithOriginCollection.LoadedFilterMatches(filter))
				{
					fPreferenceSchemeWithOriginCollection = new CMRPreferenceSchemePeriodCountryCollection(ClientAUSOriginPreferenceMapping.Factory, filter);
					fPreferenceSchemeWithOriginCollection.Load();

					fPreferenceSchemeWithOrigin = new CodeDescriptionPairList();
					fPreferenceSchemeWithOrigin.AddRange(fPreferenceSchemeWithOriginCollection);
				}
				return fPreferenceSchemeWithOrigin;
			}
		}
		CodeDescriptionPairList fPreferenceSchemeWithOrigin;
		CMRPreferenceSchemePeriodCountryCollection fPreferenceSchemeWithOriginCollection;

		CodeDescriptionPairList FlatPreferenceScheme
		{
			get
			{
				ZQuery filter = FilterForStartAndEndDate(CMRPreferenceSchemePeriodSnapshotSchema.PF_StartDate, CMRPreferenceSchemePeriodSnapshotSchema.PF_EndDate);

				if (fFlatPrefrenceScheme == null || fFlatPrefrenceSchemeCollection == null || !fFlatPrefrenceSchemeCollection.LoadedFilterMatches(filter))
				{
					fFlatPrefrenceSchemeCollection = new CMRPreferenceSchemePeriodSnapshotCollection(Factory, filter);
					fFlatPrefrenceSchemeCollection.Load();

					fFlatPrefrenceScheme = new CodeDescriptionPairList();
					fFlatPrefrenceScheme.AddRange(fFlatPrefrenceSchemeCollection);
				}

				return fFlatPrefrenceScheme;
			}
		}
		CodeDescriptionPairList fFlatPrefrenceScheme;
		CMRPreferenceSchemePeriodSnapshotCollection fFlatPrefrenceSchemeCollection;

		#endregion

		#region Implementation

		ZQuery FilterForStartAndEndDate(SchemaColumn startDate, SchemaColumn endDate)
		{
			ZQuery endDateFilter = new ZQuery(endDate, null);
			ZQuery filter = new ZQuery(startDate, null);
			filter.AddToFilter(endDateFilter);

			return filter;
		}

		protected ClientAUSOriginPreferenceMapping ClientAUSOriginPreferenceMapping
		{
			get { return (ClientAUSOriginPreferenceMapping)Parent; }
		}

		#endregion
	}
}
