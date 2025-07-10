using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MarketingManager.Business;
using Enterprise.Client.EDI.MarketingManager.Module;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Licensing;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Res;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.MarketingManager.GUI
{
	public class EDIGlbCompanyCampaignContactFilterBusinessObject : GlbCompanyCampaignContactFilterBusinessObject
	{
		EDIGlbCompanyCampaignContactFilterBusinessObject()
		{
			// For filter rule support
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
			=> new EDIGlbCompanyCampaignContactFilterBusinessObject(Campaign);

		public EDIGlbCompanyCampaignContactFilterBusinessObject(EDIGlbCompanyCampaign campaign)
			: base(campaign)
		{
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.DripMarketingFilterRuleEDI;
		}

		new EDIGlbCompanyCampaign Campaign
		{
			get { return base.Campaign as EDIGlbCompanyCampaign; }
			set { base.Campaign = value; }
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();

			AddWARPFilters(filters);
			AddLicenceUsageFilters(filters);
			AddLicenceStatusAndFlagsFilters(filters);
			CustomisedLabels(filters);
			AddSalesFilters(filters);
			AddRelatedIncidentsFilters(filters);
			AddRankingFilter(filters);

			return filters;
		}

		#region Related Incidents Filters

		void AddRelatedIncidentsFilters(ModuleFilterCollection filters)
		{
			var contactsRelatedIncidentsFilter = new ContactsIncidentsFilter("Incidents (by Contact)", ViewCampaignContactSchema.PK, IncidentMainSchema.IM_OC_Contact, () => new SupportIncidentCollection(Factory), typeof(CampaignContact));
			contactsRelatedIncidentsFilter.Category = RelatedSupportIncidentsCategory;
			filters.AddFilter(contactsRelatedIncidentsFilter);

			var orgsRelatedIncidentsFilter = new OrganizationIncidentsFilter("Incidents (by Organization)", ViewCampaignContactSchema.VCC_OH, IncidentMainSchema.IM_OH_Client, () => new SupportIncidentCollection(Factory), typeof(CampaignContact));
			orgsRelatedIncidentsFilter.Category = RelatedSupportIncidentsCategory;
			filters.AddFilter(orgsRelatedIncidentsFilter);
		}

		internal static FilterCategory RelatedSupportIncidentsCategory => FilterCategories.GetOrCreateFilterCategory((NoResString)"Related Support Incidents");

		#endregion

		#region WARP Filters

		void AddWARPFilters(ModuleFilterCollection filters)
		{
			var warpCategory = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("63e9e984-8ebb-484f-88b4-f315b8b77ad0", "WARP"));

			var warpRelatsionshipFilter = filters.AddFlagsFilter("WARP Relationships",
				new string[] { Res.GetString("A529CA16-B583-4A3D-BC51-6C9F89FB4ACF", "Is WARP Nominated Agent"), Res.GetString("33C384F6-C22E-4657-9D17-6D93CA9FAE3C", "Is WARP Referring Customer") },
				new GetFlagsQuery[] { GetWARPNominatingRelationshipQuery, GetWARPReferringRelationshipQuery },
				JoinCondition.Or);
			warpRelatsionshipFilter.Category = warpCategory;

			var warpDateFilter = filters.AddDateFilter("WARP Date", GetWARPDateQuery, true, true);
			warpDateFilter.Category = warpCategory;

			var nominatedAgentsCountFilter = new CampaignContactNumberFilter("Nominated Agents with Referring Customer Count", GetNominatedAgentsCount);
			nominatedAgentsCountFilter.MultilingualDescription = ResString.GetMultilingualString("2126BA1A-5F08-448C-AD39-0C89FFB3CA52", "Nominated Agents with Referring Customer Count");
			nominatedAgentsCountFilter.DefaultComparisonOperator = CampaignContactNumberFilter.ComparisonConstants.GreaterThanOrEqualTo;
			nominatedAgentsCountFilter.Category = warpCategory;
			filters.AddCustomFilter(nominatedAgentsCountFilter);
		}

		ZQuery GetWARPReferringRelationshipQuery(ZBool value)
		{
			return GetWARPRelationshipQuery(value, OrgRelatedPartySchema.PR_OH_RelatedParty);
		}

		ZQuery GetWARPNominatingRelationshipQuery(ZBool value)
		{
			return GetWARPRelationshipQuery(value, OrgRelatedPartySchema.PR_OH_Parent);
		}

		ZQuery GetWARPRelationshipQuery(ZBool value, SchemaGuidColumn foreignColumn)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			if (value)
			{
				var innerQuery = new ZDBOnlySubQuery(typeof(OrgRelatedParty), foreignColumn);
				innerQuery.AddToFilter(OrgRelatedPartySchema.PR_PartyType, EDIOrgRelatedPartyLookups.WARPConstant);
				innerQuery.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, SQLComparisonOperator.NotEqual, OrgRelatedPartySchema.PR_OH_RelatedParty);

				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
				subQuery.AddSubQuery(innerQuery, JoinCondition.And);
				query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);
				query.AddSubQuery(ViewCampaignContactSchema.VCC_OH, subQuery, JoinCondition.And);
			}
			return query;
		}

		ZQuery GetWARPDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var dateQuery = new ZQuery();
			var notIn = false;
			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				notIn = true;
			}
			else
			{
				if (date1.IsValid)
				{
					dateQuery.AddToFilter(OrgRelatedPartySchema.PR_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, date1);
				}

				if (date2.IsValid)
				{
					dateQuery.AddToFilter(OrgRelatedPartySchema.PR_SystemCreateTimeUtc, SQLComparisonOperator.LessThan, date2);
				}
			}
			var addOnQueryRelatedParty = new ZDBOnlySubQuery(typeof(OrgRelatedParty), OrgRelatedPartySchema.PR_OH_RelatedParty, notIn);
			addOnQueryRelatedParty.AddToFilter(OrgRelatedPartySchema.PR_PartyType, EDIOrgRelatedPartyLookups.WARPConstant);
			addOnQueryRelatedParty.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, SQLComparisonOperator.NotEqual, OrgRelatedPartySchema.PR_OH_RelatedParty);
			var addOnQueryParent = new ZDBOnlySubQuery(typeof(OrgRelatedParty), OrgRelatedPartySchema.PR_OH_Parent, notIn);
			addOnQueryParent.AddToFilter(OrgRelatedPartySchema.PR_PartyType, EDIOrgRelatedPartyLookups.WARPConstant);
			addOnQueryParent.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, SQLComparisonOperator.NotEqual, OrgRelatedPartySchema.PR_OH_RelatedParty);
			if (!notIn)
			{
				addOnQueryRelatedParty.AddToFilter(dateQuery, JoinCondition.And);
				addOnQueryParent.AddToFilter(dateQuery, JoinCondition.And);
			}

			addOnQueryRelatedParty.AddAsUnionQuery(addOnQueryParent);
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);
			query.AddSubQuery(ViewCampaignContactSchema.VCC_OH, addOnQueryRelatedParty, JoinCondition.And);
			return query;
		}

		ZQuery GetNominatedAgentsCount(SQLComparisonOperator comparisonOperator, ZInt value)
		{
			var sqlFilter = string.Format(@"
				{0} IN
				(
					SELECT {1}
					FROM {2}
					WHERE {3} = @PR_PartyType AND {4} <> {1}
					GROUP BY {1}
					HAVING COUNT({1}) {5} {6}
				)", ViewCampaignContactSchema.VCC_OH.Name,
					OrgRelatedPartySchema.PR_OH_Parent.Name,
					OrgRelatedPartySchema.Constants.TableName,
					OrgRelatedPartySchema.PR_PartyType.Name,
					OrgRelatedPartySchema.PR_OH_RelatedParty.Name,
					comparisonOperator.ComparisonText(value),
					comparisonOperator.EscapedSqlValue(value));

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@PR_PartyType", EDIOrgRelatedPartyLookups.WARPConstant, OrgRelatedPartySchema.PR_PartyType);

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);
			query.AddFilterAndZSQLParameterCollection(sqlFilter, sqlParams);
			return query;
		}

		#endregion

		#region Licence Usage Filters

		void AddLicenceUsageFilters(ModuleFilterCollection filters)
		{
			LicenceUsageFilter.AddLicenceUsageFilters(filters, ViewCampaignContactSchema.Constants.VCC_OH, isBilledFilter: true);
			LicenceUsageFilter.AddLicenceUsageFilters(filters, ViewCampaignContactSchema.Constants.VCC_OH, isBilledFilter: false);
		}

		#endregion

		#region Licence Filters

		void AddLicenceStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			var licenceCategory = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("3ce5ec67-368f-48e1-ad12-a1513dfb35ee", "License"));

			var licenceDatabaseTypeFilter = filters.AddTextFilter("Licence Database Type", GetLicenceQuery, DatabaseTypesList);
			licenceDatabaseTypeFilter.MultilingualDescription = ResString.GetMultilingualString("054685c3-0571-48c7-9eb4-f0847aa7ceab", "License Database Type");
			licenceDatabaseTypeFilter.Category = licenceCategory;

			var licenceDatabaseHostedLocationFilter = filters.AddTextFilter("Licence Database Hosted Location", GetLicenceQuery, DatabaseHostedLocations);
			licenceDatabaseHostedLocationFilter.MultilingualDescription = ResString.GetMultilingualString("f851ab4c-d1a2-4e05-ae16-923fbd7edbcd", "License Database Hosted Location");
			licenceDatabaseHostedLocationFilter.Category = licenceCategory;

			var licenceReleaseRingFilter = filters.AddTextFilter("Licence Release Ring", GetLicenceQuery, ReleaseRings);
			licenceReleaseRingFilter.MultilingualDescription = ResString.GetMultilingualString("918f118e-564a-4693-b1a8-5d884ef4eaef", "License Release Ring");
			licenceReleaseRingFilter.Category = licenceCategory;

			var licencePurchaseTypeFilter = filters.AddTextFilter("Licence Purchase Type", GetLicenceQuery, LicenceTypesList);
			licencePurchaseTypeFilter.MultilingualDescription = ResString.GetMultilingualString("17304fb3-22c7-42ed-beed-0fe64923ba5d", "License Purchase Type");
			licencePurchaseTypeFilter.Category = licenceCategory;

			var licenceProductTypeFilter = filters.AddTextFilter("Licence Product Type", GetLicenceProductTypeQuery, ProductTypeList);
			licenceProductTypeFilter.MultilingualDescription = ResString.GetMultilingualString("4c106793-5352-40cd-9eda-3394d50e9276", "License Product Type");
			licenceProductTypeFilter.Category = licenceCategory;
			licenceProductTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			licenceProductTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			licenceProductTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			licenceProductTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			licenceProductTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			licenceProductTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);

			var licenceEditionTypeFilter = filters.AddTextFilter("Licence Edition Type", GetLicenceQuery, LicenceEditionTypes);
			licenceEditionTypeFilter.MultilingualDescription = ResString.GetMultilingualString("c84de5b6-bebc-4298-89e3-0bf11c83ddd6", "License Edition Type");
			licenceEditionTypeFilter.Category = licenceCategory;
			licenceEditionTypeFilter.MaxLength = LicenceHeaderSchema.LA_LicenceAdvStdOth.MaxLength;

			var licenceModuleFilter = filters.AddTextFilter("Licence Module", GetLicenceQuery, ModuleList);
			licenceModuleFilter.MultilingualDescription = ResString.GetMultilingualString("db8045e9-2e33-441c-97c9-d4b9288c33db", "License Module");
			licenceModuleFilter.Category = licenceCategory;

			var isMasterOrganisationContactFilter = filters.AddTextFilter("Is Master Organization Contact", GetIsMasterOrganisationContactQuery, IsMasterOrganisationContactList);
			isMasterOrganisationContactFilter.MultilingualDescription = ResString.GetMultilingualString("b805e313-af3e-47d0-a52a-501f5688fc64", "Is Master Organization Contact");
			isMasterOrganisationContactFilter.Category = licenceCategory;

			var sqlServerEditionFilter = filters.AddTextFilter("SQL Server Edition", GetLicenceQuery, SqlEditionList);
			sqlServerEditionFilter.MultilingualDescription = ResString.GetMultilingualString("08fe5a49-da90-4ffa-bf7a-f0a9acdbe762", "SQL Server Edition");
			sqlServerEditionFilter.Category = licenceCategory;

			var sqlVersionListFilter = filters.AddTextFilter("SQL Server Version", GetLicenceQuery, SqlVersionList);
			sqlVersionListFilter.MultilingualDescription = ResString.GetMultilingualString("980ebe52-7d2d-4a4f-85b4-b2f9cf3a33a9", "SQL Server Version");
			sqlVersionListFilter.Category = licenceCategory;
			sqlVersionListFilter.MaxLength = LicenceDatabaseSchema.LD_SQLVersion.MaxLength;

			var osNameFilter = filters.AddTextFilter("OS Name", GetLicenceQuery, LicenceFilters.OSNames);
			osNameFilter.MultilingualDescription = ResString.GetMultilingualString("2742a838-9b1f-4eb6-878e-d65131d79545", "OS Name");
			osNameFilter.Category = licenceCategory;

			var osVersionsFilter = filters.AddTextFilter("OS Version", GetLicenceQuery, LicenceFilters.OSVersions);
			osVersionsFilter.MultilingualDescription = ResString.GetMultilingualString("bece6775-e2e2-4c5d-9449-4666817709c2", "OS Version");
			osVersionsFilter.Category = licenceCategory;

			var systemManufacturerFilter = filters.AddTextFilter("System Manufacturer", GetLicenceQuery, LicenceFilters.SystemManufacturers);
			systemManufacturerFilter.MultilingualDescription = ResString.GetMultilingualString("b345b820-1819-4a8f-a0c2-86c30a4b6ff6", "System Manufacturer");
			systemManufacturerFilter.Category = licenceCategory;

			var processorTypeFilter = filters.AddTextFilter("Processor Type", GetLicenceQuery, LicenceFilters.ProcessorTypes);
			processorTypeFilter.MultilingualDescription = ResString.GetMultilingualString("28b0a692-268c-4167-bd30-3e2fc911022b", "Processor Type");
			processorTypeFilter.Category = licenceCategory;

			var osNameTextFilter = filters.AddTextFilter("OS Name Free Text", GetLicenceQuery);
			osNameTextFilter.MultilingualDescription = ResString.GetMultilingualString("d4290aac-d97c-452c-8d9b-db758d6eaece", "OS Name Free Text");
			osNameTextFilter.Category = licenceCategory;

			var osVersionFilter = filters.AddTextFilter("OS Version Free Text", GetLicenceQuery);
			osVersionFilter.MultilingualDescription = ResString.GetMultilingualString("0f518266-7a2d-4f6e-aa9e-4401c2020b11", "OS Version Free Text");
			osVersionFilter.Category = licenceCategory;

			var systemManufactorerTextFilter = filters.AddTextFilter("System Manufacturer Free Text", GetLicenceQuery);
			systemManufactorerTextFilter.MultilingualDescription = ResString.GetMultilingualString("b50bcc47-f1c9-4438-855d-19cbfa366d06", "System Manufacturer Free Text");
			systemManufactorerTextFilter.Category = licenceCategory;

			var processorTypeTextFilter = filters.AddTextFilter("Processor Type Free Text", GetLicenceQuery);
			processorTypeTextFilter.MultilingualDescription = ResString.GetMultilingualString("404c564d-5e7e-4d66-bd63-fdc3b9cd0c84", "Processor Type Free Text");
			processorTypeTextFilter.Category = licenceCategory;

			var biosDateFilter = filters.AddSingleDateFilter("BIOS Date", GetDateLicenceQuery);
			biosDateFilter.MultilingualDescription = ResString.GetMultilingualString("34f7c1c0-b425-4400-83c3-58def23d8b0e", "BIOS Date");
			biosDateFilter.Category = licenceCategory;

			var isVirtualMachineFilter = filters.AddTextFilter("Is Virtual Machine", GetLicenceQuery, IsVirtualMachineList);
			isVirtualMachineFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			isVirtualMachineFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			isVirtualMachineFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			isVirtualMachineFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			isVirtualMachineFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			isVirtualMachineFilter.MultilingualDescription = ResString.GetMultilingualString("a68489e9-b997-40b8-a5a2-045d446b8284", "Is Virtual Machine");
			isVirtualMachineFilter.Category = licenceCategory;

			ModuleNumberRangeFilter licenceModuleUserCountFilter = new ModuleNumberRangeFilter("Licence Module User Count", GetNumberRangeLicenceQuery);
			licenceModuleUserCountFilter.PropertyType = ZCalcEditPropertyType.Short;
			licenceModuleUserCountFilter.MultilingualDescription = ResString.GetMultilingualString("d48b7784-9bfa-4609-b72b-8e106e828dd5", "License Module User Count");
			licenceModuleUserCountFilter.Category = licenceCategory;
			filters.AddCustomFilter(licenceModuleUserCountFilter);

			ModuleNumberRangeFilter totalMemoryFilter = new ModuleNumberRangeFilter("Total Memory", GetNumberRangeLicenceQuery);
			totalMemoryFilter.PropertyType = ZCalcEditPropertyType.Int;
			totalMemoryFilter.MultilingualDescription = ResString.GetMultilingualString("d940ddce-d10b-4c36-aceb-e95337c5efa7", "Total Memory");
			totalMemoryFilter.Category = licenceCategory;
			filters.AddCustomFilter(totalMemoryFilter);

			ModuleNumberRangeFilter noOfProcessorsFilter = new ModuleNumberRangeFilter("No of Processors", GetNumberRangeLicenceQuery);
			noOfProcessorsFilter.PropertyType = ZCalcEditPropertyType.Int;
			noOfProcessorsFilter.MultilingualDescription = ResString.GetMultilingualString("bdba05d5-405f-4f6b-ac6f-10aa1ca6ee85", "No of Processors");
			noOfProcessorsFilter.Category = licenceCategory;
			filters.AddCustomFilter(noOfProcessorsFilter);

			ModuleNumberRangeFilter processorSpeedFilter = new ModuleNumberRangeFilter("Processor Speed", GetNumberRangeLicenceQuery);
			processorSpeedFilter.PropertyType = ZCalcEditPropertyType.Int;
			processorSpeedFilter.MultilingualDescription = ResString.GetMultilingualString("f17b371e-9f0e-4934-9d24-2850d72ac567", "Processor Speed");
			processorSpeedFilter.Category = licenceCategory;
			filters.AddCustomFilter(processorSpeedFilter);

			var contactIsCW1ProductionVerifiedFilter = filters.AddTextFilter("Is a CW1 Production Verified Contact", GetContactIsCW1ProductionVerifiedQuery, new ContactIsCW1ProductionVerified());
			contactIsCW1ProductionVerifiedFilter.MultilingualDescription = ResString.GetMultilingualString("ec8016de-433a-4f60-89ff-68c6321e8098", "Is a CW1 Production Verified Contact");
			contactIsCW1ProductionVerifiedFilter.Category = licenceCategory;

			var organisationFilter = new OrganisationModuleFilter(ModuleIDs.Organisation, ViewCampaignContactSchema.PK, ViewCampaignContactSchema.VCC_OH, Factory, typeof(CampaignContact));
			organisationFilter.MultilingualDescription = ResString.GetMultilingualString("28e02716-7b62-4fe2-8eaf-f95bd8900e8f", "Organizations (Multiple)");
			organisationFilter.Category = licenceCategory;
			filters.AddFilter(organisationFilter);
		}

		ZQuery GetLicenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetLicenceQuery();
		}

		ZQuery GetLicenceProductTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			bool notIn = comparisonOperator == SQLComparisonOperator.NotEqual;
			ZDBOnlySubQuery licenceCompanySubQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceCompanySchema.LC_OH, notIn);
			ZDBOnlySubQuery licenceHeaderSubQuery = new ZDBOnlySubQuery(typeof(LicenceHeader), LicenceHeaderSchema.LA_LC);
			ZDBOnlySubQuery licenceDatabaseSubQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);
			licenceDatabaseSubQuery.AddToFilter(LicenceDatabaseSchema.LD_Product, value);
			licenceHeaderSubQuery.AddSubQuery(LicenceHeaderSchema.LA_LD, licenceDatabaseSubQuery, JoinCondition.And);
			licenceCompanySubQuery.AddSubQuery(licenceHeaderSubQuery, JoinCondition.And);

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			subQuery.AddSubQuery(licenceCompanySubQuery, JoinCondition.And);

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);
			query.AddSubQuery(ViewCampaignContactSchema.VCC_OH, subQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetContactIsCW1ProductionVerifiedQuery(ZString contactSelectValue)
		{
			var query = new ZDBOnlyQuery(typeof(OrgContact));

			contactSelectValue = contactSelectValue.Trim();
			if (contactSelectValue.EqualsIgnoringCase(ContactIsCW1ProductionVerified.Codes.IsCw1ProductionVerifiedContactOnly))
			{
				var relationshipQuery = new ZDBOnlySubQuery(typeof(EdiCustomerUserAccount), EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact);
				relationshipQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_IsActive, true);

				query.AddSubQuery(ViewCampaignContactSchema.PK, relationshipQuery, JoinCondition.And);
			}
			else if (contactSelectValue.EqualsIgnoringCase(ContactIsCW1ProductionVerified.Codes.NonCw1ProductionVerifiedContact))
			{
				var nonRelationshipQuery = new ZDBOnlySubQuery(typeof(EdiCustomerUserAccount), EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, true);
				nonRelationshipQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_IsActive, true);

				query.AddSubQuery(ViewCampaignContactSchema.PK, nonRelationshipQuery, JoinCondition.And);
			}

			return query;
		}

		ZQuery GetNumberRangeLicenceQuery(INumericZType value1, INumericZType value2)
		{
			return GetLicenceQuery();
		}

		ZQuery GetDateLicenceQuery(ZDateTime value)
		{
			return GetLicenceQuery();
		}

		ZQuery GetLicenceQuery()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);

			List<ModuleFilter> moduleItems = GetFiltersFromActiveModuleFiltersByName(Res.GetString("db8045e9-2e33-441c-97c9-d4b9288c33db", "License Module"));
			List<ModuleFilter> licenceTypeItems = GetFiltersFromActiveModuleFiltersByName(Res.GetString("17304fb3-22c7-42ed-beed-0fe64923ba5d", "License Purchase Type"));
			List<ModuleFilter> licenceDatabaseTypeItems = GetFiltersFromActiveModuleFiltersByName(Res.GetString("054685c3-0571-48c7-9eb4-f0847aa7ceab", "License Database Type"));

			if (licenceTypeItems.Count == 1 &&
				((ModuleTextFilter)licenceTypeItems[0]).Property == LicenceTypes.Codes.NON &&
				((ModuleTextFilter)licenceTypeItems[0]).SqlComparisonOperator == SQLComparisonOperator.Equal &&
				moduleItems.Count == 0)
			{
				string filter =
@"(
	VCC_OH IN
		(
			SELECT OH_PK FROM dbo.OrgHeader
			WHERE OH_PK NOT IN
				(
					SELECT LC_OH FROM dbo.LicenceCompany
				)
		)
	OR
	VCC_OH IN
		(
			SELECT OH_PK FROM dbo.OrgHeader
			WHERE OH_PK IN
			(
				SELECT LC_OH FROM dbo.LicenceCompany
					WHERE LC_PK IN
						(
							SELECT LA_LC FROM dbo.LicenceHeader
							WHERE LA_PK IN
								(
									SELECT LM_LA FROM dbo.LicenceModules
									WHERE LM_LicenceType = 'NON'
								)
						)
			)
		)
	)";

				query.AddFilterAndZSQLParameterCollection(filter, new ZSqlParameterCollection());
			}
			else if (licenceDatabaseTypeItems.Count == 1 &&
				((ModuleTextFilter)licenceDatabaseTypeItems[0]).Property == "NON" &&
				((ModuleTextFilter)licenceDatabaseTypeItems[0]).SqlComparisonOperator == SQLComparisonOperator.Equal)
			{
				string filter =
@"(
	VCC_OH IN
	(
		SELECT OH_PK FROM dbo.OrgHeader WHERE OH_PK NOT IN
		(
			SELECT LC_OH FROM dbo.LicenceCompany
		)
	)
	OR
	VCC_OH IN
	(
		SELECT OH_PK FROM dbo.OrgHeader WHERE OH_PK IN
		(
			SELECT LC_OH FROM dbo.LicenceCompany WHERE LC_PK NOT IN
			(
				SELECT LA_LC FROM dbo.LicenceHeader
			)
		)
	)
)";
				query.AddFilterAndZSQLParameterCollection(filter, new ZSqlParameterCollection());
			}
			else
			{
				ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
				ZDBOnlySubQuery licenceHeaderQuery = new ZDBOnlySubQuery(typeof(LicenceHeader), LicenceHeaderSchema.LA_LC);
				ZDBOnlySubQuery licenceModuleQuery = new ZDBOnlySubQuery(typeof(LicenceModules), LicenceModulesSchema.LM_LA);
				ZDBOnlySubQuery licenceCompanyQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceCompanySchema.LC_OH);

				AddLicenceModuleAndTypeQuery(licenceModuleQuery);
				AddUserCountQuery(licenceModuleQuery);
				AddDatabaseTypeQuery(licenceHeaderQuery);
				AddDatabaseHostedLocationQuery(licenceHeaderQuery);
				AddReleaseRingQuery(licenceHeaderQuery);
				AddEditionTypeQuery(licenceHeaderQuery);
				AddSQLServerEditionQuery(licenceHeaderQuery);
				AddSQLServerVersionQuery(licenceHeaderQuery);
				AddOsNameQuery(licenceHeaderQuery);
				AddOsVersionQuery(licenceHeaderQuery);
				AddSystemManufacturerQuery(licenceHeaderQuery);
				AddBIOSDateQuery(licenceHeaderQuery);
				AddTotalMemoryQuery(licenceHeaderQuery);
				AddNoOfProcessorsQuery(licenceHeaderQuery);
				AddProcessorTypeQuery(licenceHeaderQuery);
				AddProcessorSpeedQuery(licenceHeaderQuery);
				AddIsVirtualMachine(licenceHeaderQuery);

				if (!licenceModuleQuery.IsEmpty)
				{
					licenceHeaderQuery.AddSubQuery(LicenceHeaderSchema.PK, licenceModuleQuery, JoinCondition.And);
				}

				if (!licenceHeaderQuery.IsEmpty)
				{
					licenceCompanyQuery.AddSubQuery(LicenceCompanySchema.PK, licenceHeaderQuery, JoinCondition.And);
				}

				if (!licenceCompanyQuery.IsEmpty)
				{
					orgSubQuery.AddSubQuery(OrgHeaderSchema.PK, licenceCompanyQuery, JoinCondition.And);
				}

				if (!orgSubQuery.IsEmpty)
				{
					query.AddSubQuery(ViewCampaignContactSchema.VCC_OH, orgSubQuery, JoinCondition.And);
				}
			}

			return query;
		}

		ZQuery GetIsMasterOrganisationContactQuery(ZString contactSelectValue)
		{
			var query = new ZDBOnlyQuery(typeof(CampaignContact));

			if (contactSelectValue == IsMasterOrganisationContact.Codes.AllContacts)
			{
				return query;
			}

			contactSelectValue = contactSelectValue.Trim().ToUpper();
			var notIn = (contactSelectValue != IsMasterOrganisationContact.Codes.MasterOrganizationContactsOnly);
			var databaseSubQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.LD_OH_WebAccessOrg, notIn);
			query.AddSubQuery(ViewCampaignContactSchema.VCC_OH, databaseSubQuery, JoinCondition.And);

			return query;
		}

		#region Licence Module and Type

		void AddLicenceModuleAndTypeQuery(ZQuery parentQuery)
		{
			List<ModuleFilter> moduleItems = GetFiltersFromActiveModuleFiltersByName(Res.GetString("db8045e9-2e33-441c-97c9-d4b9288c33db", "License Module"));
			List<ModuleFilter> licenceTypeItems = GetFiltersFromActiveModuleFiltersByName(Res.GetString("17304fb3-22c7-42ed-beed-0fe64923ba5d", "License Purchase Type"));

			if (moduleItems.Count == 0 && licenceTypeItems.Count > 0)
			{
				AddLicenceTypeQuery(parentQuery, licenceTypeItems, false);
			}
			else
			{
				foreach (ModuleTextFilter moduleItem in moduleItems)
				{
					if (!moduleItem.Property.IsEmpty)
					{
						ZQuery moduleQuery = new ZQuery();
						moduleQuery.AddToFilter(GetFilterJoinCondition(moduleItem), LicenceModulesSchema.LM_GroupModuleCode, SQLComparisonOperator.Equal, moduleItem.Property);
						AddLicenceTypeQuery(moduleQuery, licenceTypeItems, (moduleItem.SqlComparisonOperator == SQLComparisonOperator.NotEqual));
						parentQuery.AddToFilter(moduleQuery, GetFilterJoinCondition(moduleItem));
					}
				}
			}
		}

		void AddLicenceTypeQuery(ZQuery parentQuery, List<ModuleFilter> licenceTypeItems, bool shouldFindNone)
		{
			if (licenceTypeItems.Count > 0)
			{
				foreach (ModuleTextFilter licenceTypeItem in licenceTypeItems)
				{
					if (!licenceTypeItem.Property.IsEmpty)
					{
						parentQuery.AddToFilter(GetFilterJoinCondition(licenceTypeItem), LicenceModulesSchema.LM_LicenceType, licenceTypeItem.SqlComparisonOperator, licenceTypeItem.Property);
						licenceTypeItem.MaxLength = LicenceModulesSchema.LM_LicenceType.MaxLength;
					}
					else
					{
						AddNoneLicenceTypeQuery(parentQuery, shouldFindNone);
					}
				}
			}
			else
			{
				AddNoneLicenceTypeQuery(parentQuery, shouldFindNone);
			}
		}

		void AddNoneLicenceTypeQuery(ZQuery parentQuery, bool shouldFindNone)
		{
			parentQuery.AddToFilter(JoinCondition.And, LicenceModulesSchema.LM_LicenceType, (shouldFindNone ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual), LicenceTypes.Codes.NON);
		}

		#endregion

		#region User Count

		void AddUserCountQuery(ZQuery parentQuery)
		{
			List<ModuleFilter> userCountItems = GetFiltersFromActiveModuleFiltersByName(Res.GetString("d48b7784-9bfa-4609-b72b-8e106e828dd5", "License Module User Count"));

			foreach (ModuleNumberRangeFilter userCountItem in userCountItems)
			{
				ZShort from = 0;
				ZShort.TryParse(userCountItem.Property1.ToString(), out from);
				ZShort to = 0;
				ZShort.TryParse(userCountItem.Property2.ToString(), out to);
				if (from > 0 || to > 0)
				{
					parentQuery.AddToFilter(GetFilterJoinCondition(userCountItem), LicenceModulesSchema.LM_UserCount, SQLComparisonOperator.GreaterThanOrEqualTo, from);
					parentQuery.AddToFilter(GetFilterJoinCondition(userCountItem), LicenceModulesSchema.LM_UserCount, SQLComparisonOperator.LessThanOrEqualTo, to);
				}
			}
		}

		#endregion

		#region Database Type

		void AddDatabaseTypeQuery(ZDBOnlyQuery licenceHeaderQuery)
		{
			List<ModuleFilter> databaseTypeItems = GetFiltersFromActiveModuleFiltersByName(Res.GetString("054685c3-0571-48c7-9eb4-f0847aa7ceab", "License Database Type"));

			foreach (ModuleTextFilter databaseTypeItem in databaseTypeItems)
			{
				if (!databaseTypeItem.Property.IsEmpty)
				{
					ZDBOnlySubQuery licenceDatabaseQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);
					licenceDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_LicenceType, databaseTypeItem.SqlComparisonOperator, databaseTypeItem.Property);
					licenceHeaderQuery.AddSubQuery(LicenceHeaderSchema.LA_LD, licenceDatabaseQuery, GetFilterJoinCondition(databaseTypeItem));
				}
			}
		}

		#endregion

		#region Database Hosted Location

		void AddDatabaseHostedLocationQuery(ZDBOnlyQuery licenceHeaderQuery)
		{
			List<ModuleFilter> databaseHostedLocationItems = GetFiltersFromActiveModuleFiltersByName(Res.GetString("f851ab4c-d1a2-4e05-ae16-923fbd7edbcd", "License Database Hosted Location"));

			foreach (ModuleTextFilter hostedLocationItem in databaseHostedLocationItems)
			{
				if (!hostedLocationItem.Property.IsEmpty)
				{
					ZDBOnlySubQuery licenceDatabaseQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);

					if (hostedLocationItem.Property == "ALL")
					{
						licenceDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_HostedLocation, SQLComparisonOperator.NotEqual, Enterprise.Core.Constants.LicenceConstants.NotHostedWithCargoWise);
					}
					else
					{
						licenceDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_HostedLocation, hostedLocationItem.SqlComparisonOperator, hostedLocationItem.Property);
					}

					licenceHeaderQuery.AddSubQuery(LicenceHeaderSchema.LA_LD, licenceDatabaseQuery, GetFilterJoinCondition(hostedLocationItem));
				}
			}
		}

		#endregion

		#region Release Ring

		void AddReleaseRingQuery(ZDBOnlyQuery licenceHeaderQuery)
		{
			List<ModuleFilter> releaseRingItems = GetFiltersFromActiveModuleFiltersByName(Res.GetString("918f118e-564a-4693-b1a8-5d884ef4eaef", "License Release Ring"));

			foreach (ModuleTextFilter releaseRingItem in releaseRingItems)
			{
				if (!releaseRingItem.Property.IsEmpty)
				{
					ZDBOnlySubQuery licenceDatabaseQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);
					licenceDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_ReleaseRing, releaseRingItem.SqlComparisonOperator, releaseRingItem.Property);
					releaseRingItem.MaxLength = LicenceDatabaseSchema.LD_ReleaseRing.MaxLength;
					licenceHeaderQuery.AddSubQuery(LicenceHeaderSchema.LA_LD, licenceDatabaseQuery, GetFilterJoinCondition(releaseRingItem));
				}
			}
		}

		#endregion

		#region Edition Type

		void AddEditionTypeQuery(ZDBOnlyQuery licenceHeaderQuery)
		{
			List<ModuleFilter> editionTypes = GetFiltersFromActiveModuleFiltersByName(Res.GetString("c84de5b6-bebc-4298-89e3-0bf11c83ddd6", "License Edition Type"));

			foreach (ModuleTextFilter editionTypeItem in editionTypes)
			{
				if (!editionTypeItem.Property.IsEmpty)
				{
					licenceHeaderQuery.AddToFilter(GetFilterJoinCondition(editionTypeItem), LicenceHeaderSchema.LA_LicenceAdvStdOth, editionTypeItem.SqlComparisonOperator, editionTypeItem.Property);
				}
			}
		}

		#endregion

		#region SQL Server Infomation Query

		void AddSQLServerEditionQuery(ZDBOnlyQuery licenceHeaderQuery)
		{
			List<ModuleFilter> sqlServerEditionItems = GetFiltersFromActiveModuleFiltersByName(Res.GetString("08fe5a49-da90-4ffa-bf7a-f0a9acdbe762", "SQL Server Edition"));

			foreach (ModuleTextFilter item in sqlServerEditionItems)
			{
				if (!item.Property.IsEmpty)
				{
					ZString value = item.Property;
					if (value == SqlServerVersionDetailsFilterHelper.Blank)
					{
						value = ZString.Empty;
					}

					ZDBOnlySubQuery licenceDatabaseQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);
					licenceDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_SQLEdition, item.SqlComparisonOperator, value);
					licenceHeaderQuery.AddSubQuery(LicenceHeaderSchema.LA_LD, licenceDatabaseQuery, GetFilterJoinCondition(item));
				}
			}
		}

		void AddSQLServerVersionQuery(ZDBOnlyQuery licenceHeaderQuery)
		{
			List<ModuleFilter> sqlServerVersionItems = GetFiltersFromActiveModuleFiltersByName(Res.GetString("980ebe52-7d2d-4a4f-85b4-b2f9cf3a33a9", "SQL Server Version"));

			foreach (ModuleTextFilter item in sqlServerVersionItems)
			{
				if (!item.Property.IsEmpty)
				{
					ZString value = item.Property;
					if (value == SqlServerVersionDetailsFilterHelper.Blank)
					{
						value = ZString.Empty;
					}

					ZDBOnlySubQuery licenceDatabaseQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);
					licenceDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_SQLVersion, item.SqlComparisonOperator, value);
					licenceHeaderQuery.AddSubQuery(LicenceHeaderSchema.LA_LD, licenceDatabaseQuery, GetFilterJoinCondition(item));
				}
			}
		}

		void AddOsNameQuery(ZDBOnlyQuery licenceHeaderQuery)
		{
			List<ModuleFilter> osNameItems = GetFiltersFromActiveModuleFiltersByName(Res.GetString("2742a838-9b1f-4eb6-878e-d65131d79545", "OS Name"))
											.Concat(GetFiltersFromActiveModuleFiltersByName(Res.GetString("d4290aac-d97c-452c-8d9b-db758d6eaece", "OS Name Free Text"))).ToList();

			foreach (ModuleTextFilter item in osNameItems)
			{
				if (!item.Property.IsEmpty)
				{
					ZDBOnlySubQuery licenceDatabaseQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);
					licenceDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_OSName, item.SqlComparisonOperator, item.Property);
					item.MaxLength = LicenceDatabaseSchema.LD_OSName.MaxLength;
					licenceHeaderQuery.AddSubQuery(LicenceHeaderSchema.LA_LD, licenceDatabaseQuery, GetFilterJoinCondition(item));
				}
			}
		}

		void AddOsVersionQuery(ZDBOnlyQuery licenceHeaderQuery)
		{
			List<ModuleFilter> osVersionItems = GetFiltersFromActiveModuleFiltersByName(Res.GetString("bece6775-e2e2-4c5d-9449-4666817709c2", "OS Version"))
												.Concat(GetFiltersFromActiveModuleFiltersByName(Res.GetString("0f518266-7a2d-4f6e-aa9e-4401c2020b11", "OS Version Free Text"))).ToList();

			foreach (ModuleTextFilter item in osVersionItems)
			{
				if (!item.Property.IsEmpty)
				{
					ZDBOnlySubQuery licenceDatabaseQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);
					licenceDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_OSVersion, item.SqlComparisonOperator, item.Property);
					item.MaxLength = LicenceDatabaseSchema.LD_OSVersion.MaxLength;
					licenceHeaderQuery.AddSubQuery(LicenceHeaderSchema.LA_LD, licenceDatabaseQuery, GetFilterJoinCondition(item));
				}
			}
		}

		void AddSystemManufacturerQuery(ZDBOnlyQuery licenceHeaderQuery)
		{
			List<ModuleFilter> manufacturerItems = GetFiltersFromActiveModuleFiltersByName(Res.GetString("b345b820-1819-4a8f-a0c2-86c30a4b6ff6", "System Manufacturer"))
													.Concat(GetFiltersFromActiveModuleFiltersByName(Res.GetString("b50bcc47-f1c9-4438-855d-19cbfa366d06", "System Manufacturer Free Text"))).ToList();

			foreach (ModuleTextFilter item in manufacturerItems)
			{
				if (!item.Property.IsEmpty)
				{
					ZDBOnlySubQuery licenceDatabaseQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);
					licenceDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_SystemManufacturer, item.SqlComparisonOperator, item.Property);
					item.MaxLength = LicenceDatabaseSchema.LD_SystemManufacturer.MaxLength;
					licenceHeaderQuery.AddSubQuery(LicenceHeaderSchema.LA_LD, licenceDatabaseQuery, GetFilterJoinCondition(item));
				}
			}
		}

		void AddBIOSDateQuery(ZDBOnlyQuery licenceHeaderQuery)
		{
			List<ModuleFilter> biosDateItems = GetFiltersFromActiveModuleFiltersByName(Res.GetString("34f7c1c0-b425-4400-83c3-58def23d8b0e", "BIOS Date"));

			foreach (ModuleSingleDateFilter item in biosDateItems)
			{
				ZDateTime biosDate = item.Property1;

				if (!biosDate.IsEmpty && biosDate.IsValid)
				{
					ZDBOnlySubQuery licenceDatabaseQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);
					licenceDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_BIOSDate, SQLComparisonOperator.Equal, biosDate);
					licenceHeaderQuery.AddSubQuery(LicenceHeaderSchema.LA_LD, licenceDatabaseQuery, GetFilterJoinCondition(item));
				}
			}
		}

		void AddTotalMemoryQuery(ZDBOnlyQuery licenceHeaderQuery)
		{
			List<ModuleFilter> memoryItems = GetFiltersFromActiveModuleFiltersByName(Res.GetString("d940ddce-d10b-4c36-aceb-e95337c5efa7", "Total Memory"));

			foreach (ModuleNumberRangeFilter item in memoryItems)
			{
				if (item.Property1 > 0)
				{
					ZDBOnlySubQuery licenceDatabaseQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);
					licenceDatabaseQuery.AddToFilter(JoinCondition.And, LicenceDatabaseSchema.LD_TotalPhysicalMemoryMB, SQLComparisonOperator.GreaterThanOrEqualTo, item.Property1.ToZInt());
					licenceDatabaseQuery.AddToFilter(JoinCondition.And, LicenceDatabaseSchema.LD_TotalPhysicalMemoryMB, SQLComparisonOperator.LessThanOrEqualTo, item.Property2.ToZInt());
					licenceHeaderQuery.AddSubQuery(LicenceHeaderSchema.LA_LD, licenceDatabaseQuery, GetFilterJoinCondition(item));
				}
			}
		}

		void AddNoOfProcessorsQuery(ZDBOnlyQuery licenceHeaderQuery)
		{
			List<ModuleFilter> processorsItems = GetFiltersFromActiveModuleFiltersByName(Res.GetString("bdba05d5-405f-4f6b-ac6f-10aa1ca6ee85", "No of Processors"));

			foreach (ModuleNumberRangeFilter item in processorsItems)
			{
				if (item.Property1 > 0)
				{
					ZDBOnlySubQuery licenceDatabaseQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);
					licenceDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_NoOfProcessorCores, SQLComparisonOperator.GreaterThanOrEqualTo, item.Property1.ToZInt());
					licenceDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_NoOfProcessorCores, SQLComparisonOperator.LessThanOrEqualTo, item.Property2.ToZInt());
					licenceHeaderQuery.AddSubQuery(LicenceHeaderSchema.LA_LD, licenceDatabaseQuery, GetFilterJoinCondition(item));
				}
			}
		}

		void AddProcessorTypeQuery(ZDBOnlyQuery licenceHeaderQuery)
		{
			List<ModuleFilter> processorTypeItems = GetFiltersFromActiveModuleFiltersByName(Res.GetString("28b0a692-268c-4167-bd30-3e2fc911022b", "Processor Type"))
													.Concat(GetFiltersFromActiveModuleFiltersByName(Res.GetString("404c564d-5e7e-4d66-bd63-fdc3b9cd0c84", "Processor Type Free Text"))).ToList();

			foreach (ModuleTextFilter item in processorTypeItems)
			{
				if (!item.Property.IsEmpty)
				{
					ZDBOnlySubQuery licenceDatabaseQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);
					licenceDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_ProcessorType, item.SqlComparisonOperator, item.Property);
					licenceHeaderQuery.AddSubQuery(LicenceHeaderSchema.LA_LD, licenceDatabaseQuery, GetFilterJoinCondition(item));
				}
			}
		}

		void AddProcessorSpeedQuery(ZDBOnlyQuery licenceHeaderQuery)
		{
			List<ModuleFilter> processorSpeedItems = GetFiltersFromActiveModuleFiltersByName(Res.GetString("f17b371e-9f0e-4934-9d24-2850d72ac567", "Processor Speed"));

			foreach (ModuleNumberRangeFilter item in processorSpeedItems)
			{
				if (item.Property1 > 0)
				{
					ZDBOnlySubQuery licenceDatabaseQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);
					licenceDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_ProcessorSpeedMHz, SQLComparisonOperator.GreaterThanOrEqualTo, item.Property1.ToZInt());
					licenceDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_ProcessorSpeedMHz, SQLComparisonOperator.LessThanOrEqualTo, item.Property2.ToZInt());
					licenceHeaderQuery.AddSubQuery(LicenceHeaderSchema.LA_LD, licenceDatabaseQuery, GetFilterJoinCondition(item));
				}
			}
		}

		void AddIsVirtualMachine(ZDBOnlyQuery licenceHeaderQuery)
		{
			List<ModuleFilter> isVirtualMachineItems = GetFiltersFromActiveModuleFiltersByName(Res.GetString("a68489e9-b997-40b8-a5a2-045d446b8284", "Is Virtual Machine"));

			foreach (ModuleTextFilter item in isVirtualMachineItems)
			{
				if (!item.Property.IsEmpty)
				{
					ZBool isVirtualMachine = false;
					if (string.Compare(item.Property, "Yes", true) == 0)
					{
						isVirtualMachine = true;
					}

					ZDBOnlySubQuery licenceDatabaseQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);
					licenceDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_VirtualMachineDetected, item.SqlComparisonOperator, isVirtualMachine);
					licenceHeaderQuery.AddSubQuery(LicenceHeaderSchema.LA_LD, licenceDatabaseQuery, GetFilterJoinCondition(item));
				}
			}
		}

		#endregion

		#endregion

		#region Lookups

		public DatabaseTypes DatabaseTypesList
		{
			get
			{
				DatabaseTypes databaseTypesList = new DatabaseTypes();
				databaseTypesList.AddPair("NON", "No Licenced Databases");
				return databaseTypesList;
			}
		}

		public CodeDescriptionPairList DatabaseHostedLocations
		{
			get
			{
				CodeDescriptionPairList databaseHostedLocations = new CodeDescriptionPairList();
				databaseHostedLocations.AddPair("ALL", "Hosted With Any Data Center");
				databaseHostedLocations.AddRange(EDIDataRegistry.Instance.DatabaseHostedLocations.Value);
				return databaseHostedLocations;
			}
		}

		public CodeDescriptionPairList ReleaseRings
		{
			get
			{
				ReleaseRingsList ringList = new ReleaseRingsList();
				CodeDescriptionPairList releaseRings = new CodeDescriptionPairList();
				foreach (string ring in Enterprise.Client.EDI.ReleaseBuilds.Business.ReleaseRingsLookup.CheckInRingCodes)
				{
					releaseRings.AddPair(ring, ringList.GetDescriptionFromCode(ring));
				}
				return releaseRings;
			}
		}

		public LicenceTypes LicenceTypesList
		{
			get { return new LicenceTypes(); }
		}

		public CodeDescriptionPairList ProductTypeList
		{
			get { return new LicenceHeaderLookups(null).ProductType; }
		}

		public LicenceAdvStdOthList LicenceEditionTypes
		{
			get { return new LicenceAdvStdOthList(); }
		}

		public CodeDescriptionPairList ModuleList
		{
			get
			{
				var result = LicenceModuleList.Instance.Names;
				result.SortByDescription();
				return result;
			}
		}

		public CodeDescriptionPairList SqlEditionList
		{
			get { return SqlServerVersionDetailsFilterHelper.GetSqlEditionList(); }
		}

		public CodeDescriptionPairList SqlVersionList
		{
			get { return SqlServerVersionDetailsFilterHelper.GetSqlVersionList(); }
		}

		public CodeDescriptionPairList IsMasterOrganisationContactList
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(IsMasterOrganisationContact.Codes.AllContacts, Res.GetString("ZClientEDI|EDIGlbCompanyCampaignContactFilterBusinessObject|IsMasterOrganisationContact|AllContacts", "All contacts"));
				list.AddPair(IsMasterOrganisationContact.Codes.MasterOrganizationContactsOnly, Res.GetString("ZClientEDI|EDIGlbCompanyCampaignContactFilterBusinessObject|IsMasterOrganisationContact|MasterOrganizationContactsOnly", "Master Organization Contacts Only"));
				list.AddPair(IsMasterOrganisationContact.Codes.NonMasterOrganizationContactsOnly, Res.GetString("ZClientEDI|EDIGlbCompanyCampaignContactFilterBusinessObject|IsMasterOrganisationContact|NonMasterOrganizationContactsOnly", "Non Master Organization Contacts Only"));

				return list;
			}
		}

		[SuppressWeaklyTypedCollectionMessage]
		public IList IsVirtualMachineList
		{
			get
			{
				if (valueList == null)
				{
					valueList = new CodeDescriptionPairList();
					valueList.AddPair("Yes", "Yes");
					valueList.AddPair("No", "No");
				}
				return valueList;
			}
		}
		CodeDescriptionPairList valueList;

		#endregion

		#region Helper Methods

		List<ModuleFilter> GetFiltersFromActiveModuleFiltersByName(string name)
		{
			List<ModuleFilter> filters = new List<ModuleFilter>();
			foreach (ModuleFilter filter in this.ActiveModuleFilters)
			{
				if (filter.LocalizedDescription == name)
				{
					filters.Add(filter);
				}
			}
			return filters;
		}

		JoinCondition GetFilterJoinCondition(ModuleFilter filter)
		{
			if (filter.OrCategory == FilterOrCategory.None)
			{
				return JoinCondition.And;
			}

			return JoinCondition.Or;
		}

		#endregion

		#region Sales Filters

		void AddSalesFilters(ModuleFilterCollection filters)
		{
			CampaignContactNumberFilter noOfEmployees = new CampaignContactNumberFilter("Sales - Number Of Employees", GetNoOfEmployees);
			noOfEmployees.MultilingualDescription = EDIDataRegistry.Instance.NumberOfEmployeesLabel.Value;
			noOfEmployees.Category = FilterCategories.NumbersAndReferences;
			filters.AddCustomFilter(noOfEmployees);

			CampaignContactNumberFilter paidUpCapital = new CampaignContactNumberFilter("Sales - Paid Up Capital", GetPaidUpCapital);
			paidUpCapital.MultilingualDescription = EDIDataRegistry.Instance.PaidUpCapitalLabel.Value;
			paidUpCapital.Category = FilterCategories.NumbersAndReferences;
			filters.AddCustomFilter(paidUpCapital);
		}

		ZQuery GetNoOfEmployees(SQLComparisonOperator comparisonOperator, ZInt value)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(OrgMiscServ(comparisonOperator, value, OrgMiscServSchema.OM_CMNoOfEmployees), JoinCondition.And);
			return query;
		}

		ZQuery GetPaidUpCapital(SQLComparisonOperator comparisonOperator, ZInt value)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(OrgMiscServ(comparisonOperator, value, OrgMiscServSchema.OM_CMPaidUpCapital), JoinCondition.And);
			return query;
		}

		#endregion

		#region Contact Ranking Filter

		void AddRankingFilter(ModuleFilterCollection filters)
		{
			var flagFilter = filters.AddFlagsFilter("Include Rankings", new string[] { Res.GetString("46d7cc76-e513-4dfb-ab20-4d6e30d1a5b3", "Show Highest Ranked Contact Only") }, new GetFlagsQuery[] { GetShowHighestRankedContactQuery });
			flagFilter.MultilingualDescription = ResString.GetMultilingualString("8074a489-534f-4b67-8613-419d267fc97a", "Include Rankings");
			flagFilter.Property0 = ZBool.False;
			flagFilter.Category = FilterCategories.Other;
		}

		ZQuery GetShowHighestRankedContactQuery(ZBool value)
		{
			var query = new ZDBOnlyQuery(typeof(CampaignContact));
			if (!value)
			{
				return query;
			}

			ZString sQL = @"
			VCC_PK IN (SELECT OC_PK FROM (
							SELECT
								RANK() OVER (PARTITION BY OH_PK ORDER BY

									 CASE WHEN C.OD_DocumentGroup = @CustomerService THEN '1'
						WHEN C.OD_DocumentGroup = @Marketing THEN '2'
						WHEN C.OD_DocumentGroup = @Receivables THEN '3'
						WHEN C.OD_DocumentGroup = @All THEN '4'
						WHEN C.OD_DocumentGroup IS NULL THEN '5'
						ELSE '6' END ASC, COALESCE(C.OD_DefaultContact, 0) DESC, OC_SystemLastEditTimeUtc DESC, OC_ContactName) Rank,
					OC_PK,
					OC_SystemLastEditTimeUtc
				FROM dbo.OrgHeader as A
				INNER JOIN dbo.OrgContact ON OC_OH = OH_PK
				LEFT JOIN dbo.OrgDocument C ON OD_OC = OC_PK
				WHERE OC_IsActive = 1
					AND OC_PK NOT IN(SELECT OD_OC FROM dbo.OrgDocument WHERE OD_DeliverBy = @DoNotDeliver)
				) X
				WHERE X.Rank = 1)";

			var @params = new ZSqlParameterCollection();
			@params.Add("@DoNotDeliver", Core.Constants.ContactNotifyModes.DoNotDeliver, OrgDocumentSchema.OD_DeliverBy);
			@params.Add("@CustomerService", ContactType.CustomerService.Code, OrgDocumentSchema.OD_DocumentGroup);
			@params.Add("@Marketing", ContactType.Marketing.Code, OrgDocumentSchema.OD_DocumentGroup);
			@params.Add("@Receivables", ContactType.Receivables.Code, OrgDocumentSchema.OD_DocumentGroup);
			@params.Add("@All", ContactType.All.Code, OrgDocumentSchema.OD_DocumentGroup);
			query.AddFilterAndZSQLParameterCollection(sQL, @params);
			return query;
		}

		#endregion

		#region Customised Labels

		void CustomisedLabels(ModuleFilterCollection filters)
		{
			foreach (var item in filters)
			{
				var moduleNumberRangeFilter = item as CampaignContactNumberFilter;
				if (moduleNumberRangeFilter != null)
				{
					if (moduleNumberRangeFilter.Description == AchievableBusinessDescription)
					{
						moduleNumberRangeFilter.MultilingualDescription = EDIDataRegistry.Instance.AchievableBusinessLabel.Value;
					}
					else if (moduleNumberRangeFilter.Description == PercentageWonDescription)
					{
						moduleNumberRangeFilter.MultilingualDescription = EDIDataRegistry.Instance.AmountOfBusinessWonLabel.Value;
					}
					else if (moduleNumberRangeFilter.Description == TotalRevenueDescription)
					{
						moduleNumberRangeFilter.MultilingualDescription = EDIDataRegistry.Instance.TotalClientRevenueLabel.Value;
					}
					else if (moduleNumberRangeFilter.Description == WarehouseRevenueDescription)
					{
						moduleNumberRangeFilter.MultilingualDescription = EDIDataRegistry.Instance.WarehouseRevenueLabel.Value;
					}
					else if (moduleNumberRangeFilter.Description == ConsultingRevenueDescription)
					{
						moduleNumberRangeFilter.MultilingualDescription = EDIDataRegistry.Instance.ConsultingRevenueLabel.Value;
					}
				}
			}
		}

		#endregion
	}
}
