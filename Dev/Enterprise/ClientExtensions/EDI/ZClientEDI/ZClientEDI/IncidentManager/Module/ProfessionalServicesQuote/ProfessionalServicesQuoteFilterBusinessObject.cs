using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class ProfessionalServicesQuoteFilterBusinessObject : IncidentMainFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			filters.AddTextFilter("Quotation Type", IncidentMainSchema.IM_WorkItemType, new ProfessionalServicesQuoteLookups(null).WorkItemTypeList);
			filters.AddTextFilter("Product", IncidentMainSchema.IM_Product, Lookups.ModuleListAllModules);
			filters.AddTextFilter("Program Area", IncidentMainSchema.IM_ProgramArea, new ProfessionalServicesQuoteLookups(null).ListHelper.ProgramAreaList);
			filters.AddTextFilter("Status", GetStatusQuery, QuoteStatusList);

			filters.AddTextFilter("Description", IncidentMainSchema.IM_Description);

			filters.AddGuidFilter("Client", ModuleIDs.Organisation, IncidentMainSchema.IM_OH_Client, new OrganisationsFindBoxCollection(Factory));
			filters.AddGuidFilter("Contact", ModuleIDs.OrgContacts, IncidentMainSchema.IM_OC_Contact, new OrgContactCollection(Factory));

			filters.AddGuidFilter("Assigned Team", ModuleIDs.GlbGroup, IncidentMainSchema.IM_GG_Team, new GlbGroupCollection(Factory));
			filters.AddNkFilter("Assigned To", IncidentMainSchema.IM_GS_NKAssignedToCurrent, ModuleIDs.GlbStaff, Lookups.AssignedToCurrents);
			filters.AddNkFilter("Customer Service Contact", IncidentMainSchema.IM_GS_NKCustServiceContact, ModuleIDs.GlbStaff, Lookups.CustServiceContacts);

			filters.AddDateFilter("Closed Date", IncidentMainSchema.IM_CloseTimeUtc, true);

			filters.AddGuidFilter("Enterprise Code", ClientModuleRegistration.LicenceEnterprise, GetLicenceEnterpriseQuery, new LicenceEnterpriseCollectionForEntCodeFilter(Factory)).Category = FilterCategories.Other;
			filters.AddGuidFilter("Enterprise ID", ClientModuleRegistration.LicenceEnterprise, GetLicenceEnterpriseQuery, new LicenceEnterpriseCollection(Factory)).Category = FilterCategories.Other;
			filters.AddGuidFilter("Country", ModuleIDs.RefCountry, GetLicenceCompanyCountryQuery, new RefCountryCollection(Factory)).Category = FilterCategories.Other;

			return filters;
		}

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			return new ModuleFountainFilter("Quote Number", IncidentMainSchema.IM_IncidentNumber, "PSQ");
		}

		#region Enterprise Licence Code/Country Query

		ZQuery GetLicenceEnterpriseQuery(ZGuid licencePK)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ProfessionalServicesQuote));
			ZDBOnlySubQuery lcQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceCompanySchema.LC_OH);
			ZDBOnlySubQuery leQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceCompanySchema.LC_LE);
			leQuery.AddToFilter(LicenceEnterpriseSchema.PK, licencePK);
			lcQuery.AddSubQuery(leQuery, JoinCondition.And);
			result.AddSubQuery(IncidentMainSchema.IM_OH_Client, lcQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetLicenceCompanyCountryQuery(ZGuid countryPK)
		{
			var country = Factory.Load<RefCountry>(countryPK);
			var countryCode = country != null ? country.RN_Code : ZString.Empty;

			var result = new ZDBOnlyQuery(typeof(ProfessionalServicesQuote));
			var lcQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceCompanySchema.LC_OH);

			lcQuery.AddToFilter(JoinCondition.And, LicenceCompanySchema.LC_CompanyCountry, countryCode);
			result.AddSubQuery(IncidentMainSchema.IM_OH_Client, lcQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region Status Query

		ZQuery GetStatusQuery(ZString status)
		{
			ZQuery result = new ZQuery();

			if (status == IncidentConstants.IncidentStatus.AllOpen)
			{
				ZQuery notCancelledFilter = new ZQuery(IncidentMainSchema.IM_Status, SQLComparisonOperator.NotEqual, IncidentConstants.IncidentStatus.Cancelled);
				ZQuery notClosedFilter = new ZQuery(IncidentMainSchema.IM_Status, SQLComparisonOperator.NotEqual, IncidentConstants.IncidentStatus.Closed);

				result.AddToFilter(notCancelledFilter, JoinCondition.And);
				result.AddToFilter(notClosedFilter, JoinCondition.And);
			}
			else
			{
				result.AddToFilter(IncidentMainSchema.IM_Status, status);
			}

			return result;
		}

		#endregion

		#region Implementation

		protected override string IncidentType
		{
			get { return IncidentConstants.IncidentType.ProfessionalServicesQuote; }
		}

		protected new SupportIncidentLookups Lookups
		{
			get { return (SupportIncidentLookups)base.Lookups; }
		}

		protected override IncidentMainLookups GetNewLookups()
		{
			return new SupportIncidentLookups(this);
		}

		#endregion

		#region Lookups

		CodeDescriptionPairList QuoteStatusList
		{
			get
			{
				if (fStatusList == null)
				{
					fStatusList = new ProfessionalServicesQuoteLookups(null).StatusList;
					fStatusList.Insert(0, new CodeDescriptionPair(IncidentConstants.IncidentStatus.AllOpen, "All Open"));
				}
				return fStatusList;
			}
		}

		CodeDescriptionPairList fStatusList;

		#endregion
	}
}
