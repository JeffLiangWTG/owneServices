using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Licensing;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public class EDIOrganisationFilterBusinessObjectCore : OrganisationFilterBusinessObject
	{
		#region Description Constants

		public static class EDIDescriptions
		{
			#region SuppressResourceStringsCheckRegion

			public const string HasMainCompetitorOnRatesManagement = "Has Main Competitor On Rates Management";

			#endregion
		}

		#endregion

		LicenceFilters LicenceFilters
		{
			get
			{
				if (licenceFilters == null)
				{
					licenceFilters = new LicenceFilters(this, StatusActive, StatusInactive, StatusAll);
				}

				return licenceFilters;
			}
		}
		LicenceFilters licenceFilters;

		#region Filters

		protected override IReadOnlyList<FilterCategory> CategorySortOrderCore
		{
			get
			{
				return new FilterCategory[]
				{
					FilterCategories.NumbersAndReferences,
					FilterCategories.StatusAndFlags,
					FilterCategories.Dates,
					FilterCategories.Locations,
					FilterCategories.TextSearch,
					LicenceFilters.VersionAndLicence,
					invoicingSettingAndPricing,
					invoicingFees,
					invoicingDiscounts,
					FilterCategories.AuditInformation,
					FilterCategories.Other
				};
			}
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = base.GetModuleFiltersCore();

			AddTextFilters(filters);
			AddLicenceAndPriceListFilters(filters);
			AddInvoicingSettingAndPricingFilters(filters);
			AddGuidFilters(filters);
			AddWARPFilters(filters);
			CustomisedLabels(filters);
			AddNumberFilters(filters);
			AddLicenceUsageFilters(filters);
			AddMembershipFilters(filters);
			AddMasterOrgFilters(filters);
			AddBillingOrgForDatabaseFilter(filters);
			AddStaffAssignmentsFilters(filters);

			return filters;
		}

		#endregion

		#region Version and Licence Filters

		void AddLicenceAndPriceListFilters(ModuleFilterCollection filters)
		{
			LicenceHeaderFilterSubGroup headerSubGroup = new LicenceHeaderFilterSubGroup();
			LicenceDatabaseFilterSubGroup databaseSubGroup = new LicenceDatabaseFilterSubGroup(headerSubGroup);
			LicenceModuleFilterSubGroup moduleSubGroup = new LicenceModuleFilterSubGroup(headerSubGroup);
			var clientCompanySubGroup = new ClientCompanyFilterSubGroup(headerSubGroup);

			ModuleFilter filter = filters.AddTextFilter("Licence Company Code", GetLicenceCompanyCodeQuery);
			filter.Category = LicenceFilters.VersionAndLicence;
			filter.MaxLength = LicenceCompanySchema.LC_CompanyCode.MaxLength;

			filter = filters.AddTextFilterForExactComparison("Licence Company ID", GetLicenceCompanyIdQuery);
			filter.Category = LicenceFilters.VersionAndLicence;

			LicenceFilters.AddLicenceHeaderFilters(filters, headerSubGroup);
			LicenceFilters.AddLicenceDatabaseFilters(filters, databaseSubGroup);
			LicenceFilters.AddLicenceModuleFilters(filters, moduleSubGroup);
			LicenceFilters.AddCoreFilters(filters, moduleSubGroup);
			LicenceFilters.AddClientCompanyFilters(filters, clientCompanySubGroup);
			LicenceFilters.AddPremiumServiceFilters(filters, new LicenceDatabaseFilterBusinessObject.ClientPremiumServiceFilterSubGroup(databaseSubGroup));

			new StlPriceListFilters().AddPriceListFilters(filters, new LicenceDatabaseFilterBusinessObject.EdiPriceHeaderLinkFilterSubGroup(databaseSubGroup),
				new LicenceDatabaseFilterBusinessObject.EdiLicenceSettingFilterSubGroup(databaseSubGroup));
		}

		#region Filters Sub Groups

		class LicenceHeaderFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZQuery result = new ZQuery();

				ZDBOnlyQuery organisationQuery = new ZDBOnlyQuery(typeof(OrgHeader));
				ZDBOnlySubQuery companySubQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceCompanySchema.LC_OH);
				ZDBOnlySubQuery headerSubQuery = new ZDBOnlySubQuery(typeof(LicenceHeader), LicenceHeaderSchema.LA_LC);
				headerSubQuery.AddToFilter(filter);
				companySubQuery.AddSubQuery(headerSubQuery, JoinCondition.And);
				organisationQuery.AddSubQuery(companySubQuery, JoinCondition.And);
				result.AddToFilter(organisationQuery);

				return result;
			}
		}

		class LicenceDatabaseFilterSubGroup : ModuleFilterSubGroup
		{
			public LicenceDatabaseFilterSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery licenceQuery = new ZDBOnlyQuery(typeof(LicenceHeader));
				ZDBOnlySubQuery databaseSubQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceHeaderSchema.LA_LD);
				databaseSubQuery.AddToFilter(filter);
				licenceQuery.AddSubQuery(databaseSubQuery, JoinCondition.And);
				return licenceQuery;
			}
		}

		class ClientCompanyFilterSubGroup : ModuleFilterSubGroup
		{
			public ClientCompanyFilterSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery licenceQuery = new ZDBOnlyQuery(typeof(LicenceHeader));
				ZDBOnlySubQuery databaseSubQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceHeaderSchema.LA_LD);
				ZDBOnlySubQuery clientCompanySubQuery = new ZDBOnlySubQuery(typeof(ClientCompany), ClientCompanySchema.LCC_LD);
				clientCompanySubQuery.AddToFilter(filter);
				databaseSubQuery.AddSubQuery(clientCompanySubQuery, JoinCondition.And);
				licenceQuery.AddSubQuery(databaseSubQuery, JoinCondition.And);
				return licenceQuery;
			}
		}

		class LicenceModuleFilterSubGroup : ModuleFilterSubGroup
		{
			public LicenceModuleFilterSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery licenceQuery = new ZDBOnlyQuery(typeof(LicenceHeader));
				ZDBOnlySubQuery moduleSubQuery = new ZDBOnlySubQuery(typeof(LicenceModules), LicenceModulesSchema.LM_LA);
				moduleSubQuery.AddToFilter(filter);
				licenceQuery.AddSubQuery(moduleSubQuery, JoinCondition.And);
				return licenceQuery;
			}
		}

		#endregion

		#region Is Master Organisation

		#region Master Organisations Only

		public static class IsMasterOrganisation
		{
			public static class Code
			{
				public const string AllOrganisations = "ALL";
				public const string MasterOrganisationsOnly = "MST";
				public const string NonMasterOrganisationsOnly = "NON";
			}
		}

		#endregion

		#region Is Master Organisation List

		public CodeDescriptionPairList IsMasterOrganisationList
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(IsMasterOrganisation.Code.AllOrganisations, Res.GetString("ZClientEDI|EDIOrganisationFilterBusinessObject|IsMasterOrganisation|AllContacts", "All Organizations"));
				list.AddPair(IsMasterOrganisation.Code.MasterOrganisationsOnly, Res.GetString("ZClientEDI|EDIOrganisationFilterBusinessObject|IsMasterOrganisation|MasterOrganizationsOnly", "Master Organizations Only"));
				list.AddPair(IsMasterOrganisation.Code.NonMasterOrganisationsOnly, Res.GetString("ZClientEDI|EDIOrganisationFilterBusinessObject|IsMasterOrganisation|NonMasterOrganizationsOnly", "Non Master Organizations Only"));

				return list;
			}
		}
		#endregion

		ZQuery GetMasterOrganisationQuery(ZString masterOrgSelectValue)
		{
			var query = new ZDBOnlyQuery(typeof(OrgHeader));
			masterOrgSelectValue = masterOrgSelectValue.Trim().ToUpper();

			if (masterOrgSelectValue == IsMasterOrganisation.Code.AllOrganisations)
			{
				return query;
			}

			var notIn = (masterOrgSelectValue != IsMasterOrganisation.Code.MasterOrganisationsOnly);
			var databaseSubQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.LD_OH_WebAccessOrg, notIn);
			query.AddSubQuery(databaseSubQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#endregion

		#region Billing Filters

		readonly FilterCategory invoicingSettingAndPricing = new FilterCategory((NoResString)"Invoicing Setting and Pricing");
		readonly FilterCategory invoicingFees = new FilterCategory((NoResString)"Invoice Fees");
		readonly FilterCategory invoicingDiscounts = new FilterCategory((NoResString)"Invoice Discounts");

		void AddInvoicingSettingAndPricingFilters(ModuleFilterCollection filters)
		{
			InvoicingSettingFilterSubGroup invoicingSubGroup = new InvoicingSettingFilterSubGroup();
			InvoiceDeliveryFilterSubGroup invoiceDeliverySubGroup = new InvoiceDeliveryFilterSubGroup();
			PricingFilterSubGroup pricingSubGroup = new PricingFilterSubGroup();
			PricingItemFilterSubGroup priceItemSubGroup = new PricingItemFilterSubGroup(pricingSubGroup);
			FeeSubGroup feeSubGroup = new FeeSubGroup();
			DiscountSubGroup discountSubGroup = new DiscountSubGroup(invoicingSubGroup);

			ModuleFilter filter = filters.AddGuidFilter("Invoice From Branch", ModuleIDs.GlbBranch, GetInvoiceFromBranchQuery, new GlbBranchCollection(Factory));
			filter.Category = invoicingSettingAndPricing;
			filter.SubGroup = invoiceDeliverySubGroup;

			filter = filters.AddGuidFilter("Default Tax Rate", ModuleIDs.AccTaxRate, GetTaxRateQuery, new AccTaxRateCollection(Factory));
			filter.Category = invoicingSettingAndPricing;
			filter.SubGroup = invoiceDeliverySubGroup;

			filter = filters.AddNkFilter("Invoicing Currency", GetInvoicingCurrencyQuery, ModuleIDs.RefCurrency, new RefCurrencyCollection(Factory));
			filter.Category = invoicingSettingAndPricing;
			filter.SubGroup = invoiceDeliverySubGroup;

			filter = filters.AddTextFilter("Processing Fee", GetProcessingFeeQuery, EDIDataRegistry.Instance.InvoicingProcessingFeeLookup.Value);
			filter.Category = invoicingSettingAndPricing;
			filter.SubGroup = invoicingSubGroup;

			filter = new ModuleNumberRangeFilter("Processing Fee %", ClientLicenceBillingSchema.L4_ProcessingFeePercent)
			{
				SubGroup = invoicingSubGroup,
				Category = invoicingSettingAndPricing
			};
			filters.AddFilter(filter);

			filter = filters.AddGuidFilter("Invoice To Organisation", ModuleIDs.Organisation, GetInvoiceToOrganisationQuery, Organisations);
			filter.Category = invoicingSettingAndPricing;
			filter.SubGroup = invoiceDeliverySubGroup;

			filter = filters.AddNkFilter("Price Currency", GetPricingCurrencyQuery, ModuleIDs.RefCurrency, new RefCurrencyCollection(Factory));
			filter.Category = invoicingSettingAndPricing;
			filter.SubGroup = pricingSubGroup;

			filter = filters.AddTextFilter("Price List Version", ClientLicencePriceHeaderSchema.L6_PricelistVersion);
			filter.Category = invoicingSettingAndPricing;
			filter.SubGroup = pricingSubGroup;
			filter.MaxLength = ClientLicencePriceHeaderSchema.L6_PricelistVersion.MaxLength;

			filter = filters.AddTextFilter("Price List Discount Version", ClientLicencePriceHeaderSchema.L6_DiscountCode);
			filter.Category = invoicingSettingAndPricing;
			filter.SubGroup = pricingSubGroup;

			filter = filters.AddTextFilter("Price List Module Code", GetPriceListModuleCodeQuery, ModuleList);
			filter.Category = invoicingSettingAndPricing;
			filter.SubGroup = priceItemSubGroup;

			filter = filters.AddTextFilter("Module Fee Type", GetModuleFeeTypeQuery, BillingConstants.GetFeeTypeList());
			filter.Category = invoicingSettingAndPricing;
			filter.SubGroup = priceItemSubGroup;

			filter = filters.AddNumberRangeFilter("Module Price", GetModulePriceQuery);
			filter.Category = invoicingSettingAndPricing;
			filter.SubGroup = priceItemSubGroup;

			// Fees
			filter = filters.AddNkFilter("Invoice Fee Currency", GetPerFeeCurrencyQuery, ModuleIDs.RefCurrency, new RefCurrencyCollection(Factory));
			filter.Category = invoicingFees;
			filter.SubGroup = feeSubGroup;
			filter.MaxLength = ClientLicenceFeeSchema.L8_RX_NKCurrency.MaxLength;

			filter = filters.AddDateFilter("Invoice Fee Start Date", GetFeeStartDateQuery);
			filter.Category = invoicingFees;
			filter.SubGroup = feeSubGroup;

			filter = filters.AddDateFilter("Invoice Fee End Date", GetFeeEndDateQuery);
			filter.Category = invoicingFees;
			filter.SubGroup = feeSubGroup;

			filter = filters.AddTextFilter("Invoice Fee Description", GetFeeDescriptionQuery);
			filter.Category = invoicingFees;
			filter.SubGroup = feeSubGroup;

			filter = filters.AddTextFilter("Invoice Fee Comment", GetFeeCommentQuery);
			filter.Category = invoicingFees;
			filter.SubGroup = feeSubGroup;

			filter = filters.AddTextFilter("Invoice Fee Charge Code", GetFeeChargeCodeQuery);
			filter.Category = invoicingFees;
			filter.SubGroup = feeSubGroup;
			filter.MaxLength = ClientLicenceFeeSchema.L8_ChargeCode.MaxLength;

			filter = filters.AddTextFilter("Invoice Fee Type", GetFeeTypeQuery, EDIDataRegistry.Instance.LicenceFeeTypes.Value);
			filter.Category = invoicingFees;
			filter.SubGroup = feeSubGroup;
			filter.MaxLength = ClientLicenceFeeSchema.L8_Type.MaxLength;

			// Discounts
			filter = filters.AddTextFilter("Invoice Discount Type", GetDiscountTypeQuery, BillingConstants.GetDiscountTypeList());
			filter.Category = invoicingDiscounts;
			filter.SubGroup = discountSubGroup;
			filter.MaxLength = ClientLicenceBillingDiscountSchema.L5_Type.MaxLength;

			filter = filters.AddTextFilter("Invoice Discount System", GetDiscountSystemQuery, ClientLicenceBillingDiscountLookups.GetSystemCodes());
			filter.Category = invoicingDiscounts;
			filter.SubGroup = discountSubGroup;
			filter.MaxLength = ClientLicenceBillingDiscountSchema.L5_SystemCode.MaxLength;

			filter = filters.AddDateFilter("Invoice Discount Start Date", GetDiscountStartDateQuery);
			filter.Category = invoicingDiscounts;
			filter.SubGroup = discountSubGroup;

			filter = filters.AddDateFilter("Invoice Discount End Date", GetDiscountEndDateQuery);
			filter.Category = invoicingDiscounts;
			filter.SubGroup = discountSubGroup;

			filter = filters.AddTextFilter("Invoice Discount Description", GetDiscountDescriptionQuery);
			filter.Category = invoicingDiscounts;
			filter.SubGroup = discountSubGroup;
			filter.MaxLength = ClientLicenceBillingDiscountSchema.L5_Description.MaxLength;

			filter = filters.AddTextFilter("Invoice Discount Module Code", GetDiscountModuleQuery, Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceModuleList.Instance.Names);
			filter.Category = invoicingDiscounts;
			filter.SubGroup = discountSubGroup;
			filter.MaxLength = ClientLicenceBillingDiscountSchema.L5_ModuleCode.MaxLength;
		}

		ZQuery GetInvoiceFromBranchQuery(ZGuid value)
		{
			return new ZQuery(ClientInvoiceDeliverySchema.L9_GB_InvoicingBranch, value);
		}

		ZQuery GetTaxRateQuery(ZGuid value)
		{
			return new ZQuery(ClientInvoiceDeliverySchema.L9_AT_TaxId, value);
		}

		ZQuery GetInvoicingCurrencyQuery(ZString value)
		{
			return new ZQuery(ClientInvoiceDeliverySchema.L9_RX_NKInvoiceCurrency, value);
		}

		ZQuery GetProcessingFeeQuery(ZString value)
		{
			return new ZQuery(ClientLicenceBillingSchema.L4_ProcessingFee, value);
		}

		ZQuery GetInvoiceToOrganisationQuery(ZGuid value)
		{
			return new ZQuery(ClientInvoiceDeliverySchema.L9_OH_InvoiceTo, value);
		}

		ZQuery GetPricingCurrencyQuery(ZString value)
		{
			return new ZQuery(ClientLicencePriceHeaderSchema.L6_RX_NKCurrency, value);
		}

		ZQuery GetPriceListModuleCodeQuery(ZString value)
		{
			return new ZQuery(ClientLicencePriceItemSchema.L7_Code, value);
		}

		ZQuery GetModuleFeeTypeQuery(ZString value)
		{
			return new ZQuery(ClientLicencePriceItemSchema.L7_FeeType, value);
		}

		ZQuery GetModulePriceQuery(INumericZType value1, INumericZType value2)
		{
			return ModuleNumberRangeFilter.AddToFilters(new ZQuery(), ClientLicencePriceItemSchema.L7_Price, value1, value2);
		}

		#region Fee queries

		ZQuery GetPerFeeCurrencyQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(ClientLicenceFeeSchema.L8_RX_NKCurrency, comparisonOperator, value);
		}

		ZQuery GetFeeDescriptionQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(ClientLicenceFeeSchema.L8_Description, comparisonOperator, value);
		}

		ZQuery GetFeeCommentQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(ClientLicenceFeeSchema.L8_Comment, comparisonOperator, value);
		}

		ZQuery GetFeeStartDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			ZQuery result = new ZQuery();
			AddDateRange(result, comparisonOperator, JoinCondition.And, ClientLicenceFeeSchema.L8_StartDate, date1.Date, date2.Date);
			return result;
		}

		ZQuery GetFeeEndDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			ZQuery result = new ZQuery();
			AddDateRange(result, comparisonOperator, JoinCondition.And, ClientLicenceFeeSchema.L8_EndDate, date1.Date, date2.Date);
			return result;
		}

		ZQuery GetFeeChargeCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(ClientLicenceFeeSchema.L8_ChargeCode, comparisonOperator, value);
		}

		ZQuery GetFeeTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(ClientLicenceFeeSchema.L8_Type, comparisonOperator, value);
		}

		#endregion

		#region Discount queries

		ZQuery GetDiscountSystemQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(ClientLicenceBillingDiscountSchema.L5_SystemCode, comparisonOperator, value);
		}

		ZQuery GetDiscountTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(ClientLicenceBillingDiscountSchema.L5_Type, comparisonOperator, value);
		}

		ZQuery GetDiscountStartDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			ZQuery result = new ZQuery();
			AddDateRange(result, comparisonOperator, JoinCondition.And, ClientLicenceBillingDiscountSchema.L5_StartDate, date1.Date, date2.Date);
			return result;
		}

		ZQuery GetDiscountEndDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			ZQuery result = new ZQuery();
			AddDateRange(result, comparisonOperator, JoinCondition.And, ClientLicenceBillingDiscountSchema.L5_EndDate, date1.Date, date2.Date);
			return result;
		}

		ZQuery GetDiscountDescriptionQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(ClientLicenceBillingDiscountSchema.L5_Description, comparisonOperator, value);
		}

		ZQuery GetDiscountModuleQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(ClientLicenceBillingDiscountSchema.L5_ModuleCode, comparisonOperator, value);
		}

		#endregion

		class InvoicingSettingFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZQuery result = new ZQuery();

				ZDBOnlyQuery organisationQuery = new ZDBOnlyQuery(typeof(OrgHeader));
				ZDBOnlySubQuery companySubQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceCompanySchema.LC_OH);
				ZDBOnlySubQuery invoicingSubQuery = new ZDBOnlySubQuery(typeof(ClientLicenceBilling), ClientLicenceBillingSchema.L4_LC);
				invoicingSubQuery.AddToFilter(filter);
				companySubQuery.AddSubQuery(invoicingSubQuery, JoinCondition.And);
				organisationQuery.AddSubQuery(companySubQuery, JoinCondition.And);
				result.AddToFilter(organisationQuery);

				return result;
			}
		}

		class InvoiceDeliveryFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZQuery result = new ZQuery();

				ZDBOnlyQuery organisationQuery = new ZDBOnlyQuery(typeof(OrgHeader));
				ZDBOnlySubQuery companySubQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceCompanySchema.LC_OH);
				ZDBOnlySubQuery invoicingSubQuery = new ZDBOnlySubQuery(typeof(ClientInvoiceDelivery), ClientInvoiceDeliverySchema.L9_LC);
				invoicingSubQuery.AddToFilter(filter);
				companySubQuery.AddSubQuery(invoicingSubQuery, JoinCondition.And);
				organisationQuery.AddSubQuery(companySubQuery, JoinCondition.And);
				result.AddToFilter(organisationQuery);

				return result;
			}
		}

		class FeeSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZQuery result = new ZQuery();
				ZDBOnlyQuery organisationQuery = new ZDBOnlyQuery(typeof(OrgHeader));
				ZDBOnlySubQuery companySubQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceCompanySchema.LC_OH);
				ZDBOnlySubQuery feeSubQuery = new ZDBOnlySubQuery(typeof(ClientLicenceFee), ClientLicenceFeeSchema.L8_LC);
				feeSubQuery.AddToFilter(filter);
				companySubQuery.AddSubQuery(feeSubQuery, JoinCondition.And);
				organisationQuery.AddSubQuery(companySubQuery, JoinCondition.And);
				result.AddToFilter(organisationQuery);
				return result;
			}
		}

		class DiscountSubGroup : ModuleFilterSubGroup
		{
			public DiscountSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery parentQuery = new ZDBOnlyQuery(typeof(ClientLicenceBilling));
				ZDBOnlySubQuery discountSubQuery = new ZDBOnlySubQuery(typeof(ClientLicenceBillingDiscount), ClientLicenceBillingDiscountSchema.L5_L4);
				discountSubQuery.AddToFilter(filter);
				parentQuery.AddSubQuery(discountSubQuery, JoinCondition.And);
				return parentQuery;
			}
		}

		class PricingFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZQuery result = new ZQuery();

				ZDBOnlyQuery organisationQuery = new ZDBOnlyQuery(typeof(OrgHeader));
				ZDBOnlySubQuery companySubQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceCompanySchema.LC_OH);
				ZDBOnlySubQuery pricingSubQuery = new ZDBOnlySubQuery(typeof(ClientLicencePriceHeader), ClientLicencePriceHeaderSchema.L6_LC);

				string additionalSql = string.Format(
@"{0} IN 
(
	SELECT 
		ClientLicencePriceHeader.{0} 
	FROM 
		dbo.ClientLicencePriceHeader JOIN 
		(
			SELECT 
				InnerQuery.{1}, MAX({2}) LatestValidFrom
			FROM 
				dbo.ClientLicencePriceHeader InnerQuery
			GROUP BY {1}
		) LatestValidFromDate ON LatestValidFromDate.{1} = ClientLicencePriceHeader.{1} AND {2} = LatestValidFrom
)",
					ClientLicencePriceHeaderSchema.Constants.PK,            //0
					ClientLicencePriceHeaderSchema.Constants.L6_LC,         //1
					ClientLicencePriceHeaderSchema.Constants.L6_ValidFrom,  //2
					ClientLicencePriceHeaderSchema.Constants.TableName      //3
				);

				pricingSubQuery.AddFilterAndZSQLParameterCollection(additionalSql, null);
				pricingSubQuery.AddToFilter(filter);

				companySubQuery.AddSubQuery(pricingSubQuery, JoinCondition.And);
				organisationQuery.AddSubQuery(companySubQuery, JoinCondition.And);
				result.AddToFilter(organisationQuery);

				return result;
			}
		}

		class PricingItemFilterSubGroup : ModuleFilterSubGroup
		{
			public PricingItemFilterSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery pricingQuery = new ZDBOnlyQuery(typeof(ClientLicencePriceHeader));
				ZDBOnlySubQuery priceItemSubQuery = new ZDBOnlySubQuery(typeof(ClientLicencePriceItem), ClientLicencePriceItemSchema.L7_L6);
				priceItemSubQuery.AddToFilter(filter);
				pricingQuery.AddSubQuery(priceItemSubQuery, JoinCondition.And);
				return pricingQuery;
			}
		}

		#endregion

		#region Code Filters

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("Original Name/Code", GetOriginalOrgNameAndCodeQuery);
			filter.MaxLength = OrgBrandOrRelatedNameSchema.P1_RelatedName.MaxLength;
		}

		void AddGuidFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddGuidFilter("Key Account Manager - Primary",
				ModuleIDs.GlbStaff,
				GetRelManagerPrimaryFilter,
				Staff);
			filter.Category = RelationshipOrgStaff;

			ModuleGuidFilter filter2 = filters.AddGuidFilter("Key Account Manager - Secondary",
				ModuleIDs.GlbStaff,
				GetRelManagerSecondaryFilter,
				Staff);
			filter2.Category = RelationshipOrgStaff;

			filters.AddGuidFilter("Enterprise Code", ClientModuleRegistration.LicenceEnterprise, GetLicenceEnterpriseQuery, new LicenceEnterpriseCollectionForEntCodeFilter(Factory));
			filters.AddGuidFilter("Enterprise ID", ClientModuleRegistration.LicenceEnterprise, GetLicenceEnterpriseQuery, new LicenceEnterpriseCollection(Factory));
		}

		public static ZQuery GetLicenceEnterpriseQuery(SQLComparisonOperator comparisonOperator, object pkValue)
		{
			var enterprisePK = ZGuid.Empty;
			ZGuid.TryParse(pkValue, out enterprisePK);

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgHeader));

			bool notIn = false;
			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				notIn = true;
			}

			ZDBOnlySubQuery lcQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceCompanySchema.LC_OH, notIn);
			ZDBOnlySubQuery leQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceCompanySchema.LC_LE, notIn);
			leQuery.AddToFilter(LicenceEnterpriseSchema.PK, comparisonOperator, enterprisePK);
			lcQuery.AddSubQuery(leQuery, JoinCondition.And);
			result.AddSubQuery(OrgHeaderSchema.PK, lcQuery, JoinCondition.And);
			return result;
		}

		protected override void AddRelatedPartiesModuleFilters(ModuleFilterCollection filters)
		{
			var eDIOrgRelatedPartiesModuleFilter = new EDIOrgRelatedPartiesModuleFilter("Related Parties");
			eDIOrgRelatedPartiesModuleFilter.Category = RelationshipOrgStaff;
			eDIOrgRelatedPartiesModuleFilter.MultilingualDescription = ResString.GetMultilingualString("EDIOrganisationFilter|RelatedParties", "Related Parties");
			filters.AddCustomFilter(eDIOrgRelatedPartiesModuleFilter);
		}

		void AddWARPFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddFlagsFilter("WARP Relationships", new string[] { Res.GetString("EDIOrganisationFilter|NominatingAgent", "Is WARP Nominated Agent"), Res.GetString("EDIOrganisationFilter|ReferringCustomer", "Is WARP Referring Customer") }, new GetFlagsQuery[] { GetWARPNominatingRelationshipQuery, GetWARPReferringRelationshipQuery }, JoinCondition.Or);
			filter.Category = FilterCategories.StatusAndFlags;

			var dateFilter = filters.AddDateFilter("WARP Date", GetWARPDateQuery, true, true);
			dateFilter.Category = FilterCategories.Dates;
		}

		protected virtual void AddLicenceUsageFilters(ModuleFilterCollection filters)
		{
		}

		protected virtual void AddMembershipFilters(ModuleFilterCollection filters)
		{
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
			var outerQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			if (value)
			{
				var innerQuery = new ZDBOnlySubQuery(typeof(OrgRelatedParty), foreignColumn);
				innerQuery.AddToFilter(OrgRelatedPartySchema.PR_PartyType, EDIOrgRelatedPartyLookups.WARPConstant);
				innerQuery.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, SQLComparisonOperator.NotEqual, OrgRelatedPartySchema.PR_OH_RelatedParty);

				outerQuery.AddSubQuery(innerQuery, JoinCondition.And);
			}
			return outerQuery;
		}

		ZQuery GetWARPDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var outerQuery = new ZDBOnlyQuery(typeof(OrgHeader));

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
			outerQuery.AddSubQuery(addOnQueryRelatedParty, JoinCondition.And);
			return outerQuery;
		}

		ZQuery GetRelManagerPrimaryFilter(ZGuid value)
		{
			ZQuery query = new ZQuery();
			AddForRelManagerPrimary(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		ZQuery GetRelManagerSecondaryFilter(ZGuid value)
		{
			ZQuery query = new ZQuery();
			AddForRelManagerSecondary(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		void AddForRelManagerPrimary(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetStaffAssignmentsQuery(OrgHeaderSchema.OH_IsSalesLead, "ALL", "RM1", value));
		}

		void AddForRelManagerSecondary(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetStaffAssignmentsQuery(OrgHeaderSchema.OH_IsSalesLead, "ALL", "RM2", value));
		}

		ZQuery GetLicenceCompanyCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			ZDBOnlySubQuery lCQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceCompanySchema.LC_OH);
			lCQuery.AddToFilter(LicenceCompanySchema.LC_CompanyCode, comparisonOperator, value);
			query.AddSubQuery(lCQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetLicenceCompanyIdQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			int number;
			if (!Base27Encoding.TryDecode(value, out number))
			{
				number = -1;
			}

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			ZDBOnlySubQuery companyQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceCompanySchema.LC_OH);
			companyQuery.AddToFilter(LicenceCompanySchema.LC_CompanyNumber, comparisonOperator, number);
			query.AddSubQuery(companyQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetOriginalOrgNameAndCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgBrandOrRelatedName), OrgBrandOrRelatedNameSchema.P1_OH);
			subQuery.AddToFilter(OrgBrandOrRelatedNameSchema.P1_RelatedName, comparisonOperator, value);
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region Lookups

		#region VersionFilterTypes List

		public CodeDescriptionPairList VersionFilterTypes
		{
			get
			{
				if (fVersionFilterTypes == null)
				{
					fVersionFilterTypes = new CodeDescriptionPairList();
					fVersionFilterTypes.AddPair("None", "None");
					fVersionFilterTypes.AddPair(CurrentRunningVersion, "Current Running Version");
					fVersionFilterTypes.AddPair(LastDeliveredVersion, "Last Successfully Delivered Version");
				}
				return fVersionFilterTypes;
			}
		}

		public const string CurrentRunningVersion = "Current";
		public const string LastDeliveredVersion = "Delivered";

		CodeDescriptionPairList fVersionFilterTypes;

		#endregion

		#region Module List

		public virtual CodeDescriptionPairList ModuleList
		{
			get { return Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceModuleList.Instance.Names; }
		}

		#endregion

		#region LicenceType List

		public virtual CodeDescriptionPairList LicenceTypeList
		{
			get
			{
				if (fLicenceTypeList == null)
				{
					fLicenceTypeList = new LicenceTypes();
					fLicenceTypeList.Insert(0, new CodeDescriptionPair("ANY", "PUR, TRI or REN"));
				}

				return fLicenceTypeList;
			}
		}
		CodeDescriptionPairList fLicenceTypeList;

		#endregion

		#region Org List

		OrgHeaderCollection Organisations
		{
			get { return organisations ?? (organisations = new OrgHeaderCollection(Factory)); }
		}
		OrgHeaderCollection organisations;

		#endregion

		#endregion

		#region Number Filters

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			var countSubGroup = new CountSubGroup();

			ModuleNumberRangeFilter amountOfBusinessWonFilter = new ModuleNumberRangeFilter("Amount Of Business Won", GetAmountOfBusinessWonQuery);
			amountOfBusinessWonFilter.PropertyType = ZCalcEditPropertyType.Int;
			amountOfBusinessWonFilter.MultilingualDescription = EDIDataRegistry.Instance.AmountOfBusinessWonLabel.Value;
			amountOfBusinessWonFilter.SubGroup = countSubGroup;
			filters.AddCustomFilter(amountOfBusinessWonFilter);

			ModuleNumberRangeFilter totalClientRevenueFilter = new ModuleNumberRangeFilter("Total Client Revenue", GetTotalClientRevenueQuery);
			totalClientRevenueFilter.PropertyType = ZCalcEditPropertyType.Int;
			totalClientRevenueFilter.MultilingualDescription = EDIDataRegistry.Instance.TotalClientRevenueLabel.Value;
			totalClientRevenueFilter.SubGroup = countSubGroup;
			filters.AddCustomFilter(totalClientRevenueFilter);

			ModuleNumberRangeFilter warehouseRevenueFilter = new ModuleNumberRangeFilter("Warehouse Revenue", GetWarehouseRevenueQuery);
			warehouseRevenueFilter.PropertyType = ZCalcEditPropertyType.Int;
			warehouseRevenueFilter.MultilingualDescription = EDIDataRegistry.Instance.WarehouseRevenueLabel.Value;
			warehouseRevenueFilter.SubGroup = countSubGroup;
			filters.AddCustomFilter(warehouseRevenueFilter);

			ModuleNumberRangeFilter consultingRevenueFilter = new ModuleNumberRangeFilter("Consulting Revenue", GetConsultingRevenueQuery);
			consultingRevenueFilter.PropertyType = ZCalcEditPropertyType.Int;
			consultingRevenueFilter.MultilingualDescription = EDIDataRegistry.Instance.ConsultingRevenueLabel.Value;
			consultingRevenueFilter.SubGroup = countSubGroup;
			filters.AddCustomFilter(consultingRevenueFilter);

			ModuleNumberRangeFilter paidUpCapitalFilter = new ModuleNumberRangeFilter("Paid Up Capital", GetPaidUpCapitalQuery);
			paidUpCapitalFilter.PropertyType = ZCalcEditPropertyType.Int;
			paidUpCapitalFilter.MultilingualDescription = EDIDataRegistry.Instance.PaidUpCapitalLabel.Value;
			paidUpCapitalFilter.SubGroup = countSubGroup;
			filters.AddCustomFilter(paidUpCapitalFilter);
		}

		ZQuery GetAmountOfBusinessWonQuery(INumericZType value1, INumericZType value2)
		{
			return GetQueryWithModuleNumberRangerFilter(OrgMiscServSchema.OM_CMAmountOfBusinessWon, value1, value2, ZCalcEditPropertyType.Byte);
		}

		ZQuery GetTotalClientRevenueQuery(INumericZType value1, INumericZType value2)
		{
			return GetQueryWithModuleNumberRangerFilter(OrgMiscServSchema.OM_CMTotalClientRevenue, value1, value2, ZCalcEditPropertyType.Decimal);
		}

		ZQuery GetWarehouseRevenueQuery(INumericZType value1, INumericZType value2)
		{
			return GetQueryWithModuleNumberRangerFilter(OrgMiscServSchema.OM_CMWarehouseRevenue, value1, value2, ZCalcEditPropertyType.Decimal);
		}

		ZQuery GetConsultingRevenueQuery(INumericZType value1, INumericZType value2)
		{
			return GetQueryWithModuleNumberRangerFilter(OrgMiscServSchema.OM_CMConsultingRevenue, value1, value2, ZCalcEditPropertyType.Decimal);
		}

		ZQuery GetPaidUpCapitalQuery(INumericZType value1, INumericZType value2)
		{
			return GetQueryWithModuleNumberRangerFilter(OrgMiscServSchema.OM_CMPaidUpCapital, value1, value2, ZCalcEditPropertyType.Decimal);
		}

		#endregion

		#region Master Org Filters

		void AddMasterOrgFilters(ModuleFilterCollection filters)
		{
			var isMasterOrgFilter = filters.AddTextFilter("Is Master Organization", GetMasterOrganisationQuery, IsMasterOrganisationList);
			isMasterOrgFilter.MultilingualDescription = ResString.GetMultilingualString("2241b73f-8faf-4a50-854c-114cb6850098", "Is Master Organization");
			isMasterOrgFilter.Category = OrgTypeCategory;

			var licDatabaseFilter = new ModuleGuidForeignCollectionFilter("License Database (for Master Organization)", ClientModuleRegistration.LicenceDatabase, OrgHeaderSchema.PK, LicenceDatabaseSchema.LD_OH_WebAccessOrg, new LicenceDatabaseCollection(Factory), typeof(OrgHeader));
			licDatabaseFilter.MultilingualDescription = ResString.GetMultilingualString("380b7a51-8bc6-499c-85b8-30a26b4aa931", "License Database (for Master Organization)");
			licDatabaseFilter.Category = OrgTypeCategory;
			licDatabaseFilter.ComparisonOperator_List.Clear();
			licDatabaseFilter.ComparisonOperator_List.AddPair(ModuleFilterWithListAndComparisonOperators<ZGuid>.ComparisonConstants.AnyMatch);
			licDatabaseFilter.ComparisonOperator_List.AddPair(ModuleFilterWithListAndComparisonOperators<ZGuid>.ComparisonConstants.NoneMatch);
			filters.AddFilter(licDatabaseFilter);
		}

		#endregion

		#region Billing Org for Database Filter

		void AddBillingOrgForDatabaseFilter(ModuleFilterCollection filters)
		{
			var billingDatabaseFilter = new BillingOrgForDatabaseModuleFilter("Billing Org For Database", new LicenceDatabaseNonDependentCollection(Factory))
			{
				MultilingualDescription = ResString.GetMultilingualString("C4842079-1106-47AC-A286-1C85CCD4C42E", "License Database (for Billing Organization)")
			};
			filters.AddFilter(billingDatabaseFilter);
		}

		#endregion

		#region Staff Assignments Filter

		void AddStaffAssignmentsFilters(ModuleFilterCollection filters)
		{
			var staffAssignmentsSubGroup = new StaffAssignmentsSubGroup();

			var filter = filters.AddTextFilter("Staff Assignments - Product", GetStaffAssignmentProductFilter, IncidentDetailsLookupsHelper.ProductList);
			filter.Category = staffAssignments;
			filter.MultilingualDescription = ResString.GetMultilingualString("09944164-fc7f-4db6-96b0-6e3cf642bcb3", "Staff Assignments - Product");
			filter.SubGroup = staffAssignmentsSubGroup;
		}

		ZQuery GetStaffAssignmentProductFilter(ZString value)
		{
			var query = new ZQuery();
			query.AddToFilter(new ZQuery(OrgStaffAssignmentsSchema.O8_Product, value));
			return query;
		}

		#endregion

		void CustomisedLabels(ModuleFilterCollection filters)
		{
			foreach (var item in filters)
			{
				var moduleNumberRangeFilter = item as ModuleNumberRangeFilter;
				if (moduleNumberRangeFilter != null)
				{
					if (moduleNumberRangeFilter.Description == AchievableBusinessDescription)
					{
						moduleNumberRangeFilter.MultilingualDescription = EDIDataRegistry.Instance.AchievableBusinessLabel.Value;
					}
					else if (moduleNumberRangeFilter.Description == NumberOfEmployeesDescription)
					{
						moduleNumberRangeFilter.MultilingualDescription = EDIDataRegistry.Instance.NumberOfEmployeesLabel.Value;
					}
				}
			}
		}
	}
}
