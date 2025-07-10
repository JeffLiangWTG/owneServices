using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Module;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public class LVXFilterStripBusinessObject : CommonJobDeclarationFilterBusinessObject
	{
		public static class Constants
		{
			public const string IsReadyForConsolidation = "Ready for Consolidation?";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			AddNumberFilters(result);
			AddDateFilters(result);
			AddOrganisationFilters(result);
			AddLocationFilters(result);
			AddBillingFilters(result);
			AddJobManagementFilters(result, Env.Security.CALVXJobsView);
			AddStatusAndFlagsFilters(result);
			return result;
		}

		public override ZQuery Filter
		{
			get
			{
				var result = base.Filter;
				result.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.LVSForConsolidation);
				return result;
			}
		}

		protected override bool IsActiveStatusFilterAlwaysApplied()
		{
			return false;
		}

		#region Number Filters

		protected override void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddCustomFilter(new PeriodFilter(DeclarationFilterConstants.NumberFilterTypes.Period, JobDeclarationSchema.JE_EntryAuthorisationDate) { MultilingualDescription = ResString.GetMultilingualString("CA|LVXFilterStripBusinessObject|Period", DeclarationFilterConstants.NumberFilterTypes.Period) });

			var invoiceNumberFilter = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.LVSID, JobComInvoiceHeaderSchema.JZ_InvoiceNumber);
			invoiceNumberFilter.SubGroup = new InvoiceHeaderSubGroup();
			invoiceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("CA|LVXFilterStripBusinessObject|LVSID", DeclarationFilterConstants.NumberFilterTypes.LVSID);

			filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.InvoiceLineProductCode, GetInvoiceLineProductCodeQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobComInvoiceLineSchema.JI_PartNo)
				.MultilingualDescription = ResString.GetMultilingualString("CA|LVXFilterStripBusinessObject|InvoiceLineProductCode", DeclarationFilterConstants.NumberFilterTypes.InvoiceLineProductCode);

		filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.TransactionNumber, GetTransactionNumberQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(CusEntryNumSchema.CE_EntryNum)
				.MultilingualDescription = ResString.GetMultilingualString("CA|LVXFilterStripBusinessObject|TransactionNumber", DeclarationFilterConstants.NumberFilterTypes.TransactionNumber);

			var invLineSubGroup = new InvoiceLineSpecificFieldSubGroup();
			var tariffInvoiceLineFilter = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.TariffInvLine, JobComInvoiceLineSchema.JI_Tariff);
			tariffInvoiceLineFilter.SubGroup = invLineSubGroup;
			tariffInvoiceLineFilter.MultilingualDescription = ResString.GetMultilingualString("CA|LVXFilterStripBusinessObject|TariffInvLine", DeclarationFilterConstants.NumberFilterTypes.TariffInvLine);

			var descriptionFilter = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.DescriptionInvLine, JobComInvoiceLineSchema.JI_Description);
			descriptionFilter.SubGroup = invLineSubGroup;
			descriptionFilter.MultilingualDescription = ResString.GetMultilingualString("CA|LVXFilterStripBusinessObject|DescriptionInvLine", DeclarationFilterConstants.NumberFilterTypes.DescriptionInvLine);
		}

		ZQuery GetTransactionNumberQuery(SQLComparisonOperator @operator, ZString value)
		{
			var entryNumberQuery = GetEntryNumberQuery(@operator, value);
			var declQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), JobDeclarationSchema.PK);
			declQuery.AddToFilter(entryNumberQuery);
			var pivotQuery1 = GetGenPivotQuery(false);
			pivotQuery1.AddSubQuery(GenPivotSchema.XX_Relation2ID, declQuery, JoinCondition.And);
			var invoiceQuery1 = new ZDBOnlySubQuery(typeof(JobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
			invoiceQuery1.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, false);
			invoiceQuery1.AddSubQuery(pivotQuery1, JoinCondition.And);
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddSubQuery(invoiceQuery1, JoinCondition.Or);

			if (@operator.IsNegativeSQLOperator() || @operator == SpecialComparisonOperator.IsBlank)
			{
				var pivotQuery2 = GetGenPivotQuery(true);
				var invoiceQuery2 = new ZDBOnlySubQuery(typeof(JobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
				invoiceQuery2.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, false);
				invoiceQuery2.AddSubQuery(pivotQuery2, JoinCondition.And);
				result.AddSubQuery(invoiceQuery2, JoinCondition.Or);
			}

			return result;
		}

		ZDBOnlySubQuery GetGenPivotQuery(bool notIn)
		{
			var pivotQuery = new ZDBOnlySubQuery(typeof(GenPivot), GenPivotSchema.XX_Relation1ID, notIn);
			pivotQuery.AddToFilter(GenPivotSchema.XX_Relation1TableCode, JobComInvoiceHeaderSchema.Constants.Prefix);
			pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2TableCode, JobDeclarationSchema.Constants.Prefix);
			pivotQuery.AddToFilter(GenPivotSchema.XX_RelationType, GenPivotTypeDecider.Types.InvoiceRelatedDeclarationGenPivot);

			return pivotQuery;
		}

		#endregion

		#region Organisation Filters

		protected override void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			AddBranchAndBrokerFilters(filters);

			var importerVendorFilter = filters.AddGuidFilter(DeclarationFilterConstants.OrgFilterTypes.ImporterVendor, ModuleIDs.Organisation, GetImporterVendorQuery, Lookups.Consignees, Lookups.OrganisationList);
			importerVendorFilter.SetItemDescriptions(new ResourceStringData("", DeclarationFilterConstants.OrgFilterTypes.Importer), new ResourceStringData("", DeclarationFilterConstants.OrgFilterTypes.Vendor));
			importerVendorFilter.MultilingualDescription = ResString.GetMultilingualString("CA|LVXFilterStripBusinessObject|ImporterVendor", DeclarationFilterConstants.OrgFilterTypes.ImporterVendor);
		}

		#region GetImporterVendorQuery

		protected ZQuery GetImporterVendorQuery(ZGuid importer, ZGuid vendor)
		{
			ZQuery result = GetImporterQuery(importer);

			if (!vendor.IsEmpty)
			{
				result.AddToFilter(GetVendorQuery(vendor));
			}

			return result;
		}

		protected ZQuery GetImporterQuery(ZGuid importer)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			if (!importer.IsEmpty)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
				subQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_OH_Buyer, importer);
				subQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, false);

				result.AddToFilter(JobDeclarationSchema.JE_OH_Importer, importer);
				result.AddSubQuery(subQuery, JoinCondition.Or);
			}
			return result;
		}

		protected ZQuery GetVendorQuery(ZGuid vendor)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			if (!vendor.IsEmpty)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
				subQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_OH_Supplier, vendor);
				subQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, false);

				result.AddToFilter(JobDeclarationSchema.JE_OH_Supplier, vendor);
				result.AddSubQuery(subQuery, JoinCondition.Or);
			}
			return result;
		}

		#endregion

		#endregion

		#region Date Filters

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.DirectShipmentDate, GetDirectShipmentDateQuery)
				.MultilingualDescription = ResString.GetMultilingualString("CA|LVXFilterStripBusinessObject|DirectShipmentDate", DeclarationFilterConstants.DateFilterTypes.DirectShipmentDate);
			filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.ReleaseDate, JobDeclarationSchema.JE_EntryAuthorisationDate)
				.MultilingualDescription = ResString.GetMultilingualString("CA|LVXFilterStripBusinessObject|ReleaseDate", DeclarationFilterConstants.DateFilterTypes.ReleaseDate);
		}

		protected ZQuery GetDirectShipmentDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
			subQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, false);
			AddDateRange(subQuery, comparisonOperator, JoinCondition.And, JobComInvoiceHeaderSchema.JZ_ValuationDateOverride, value1.Date, value2.Date);

			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddSubQuery(subQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region Location Filters

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			var portOfClearance = filters.AddNkFilter(DeclarationFilterConstants.PortFilterTypes.PortOfClearance, GetPortOfClearanceQuery, ModuleIDs.Customs.Universal.ZZRefCusCodeList, Lookups.CBSAOffices).WithMaxLengthOf<ModuleNkFilter>(JobDeclarationSchema.JE_CustomsOffice);
			portOfClearance.Category = FilterCategories.Locations;
			portOfClearance.MultilingualDescription = ResString.GetMultilingualString("CA|LVXFilterStripBusinessObject|PortOfClearance", DeclarationFilterConstants.PortFilterTypes.PortOfClearance);

			var countryofOrigin = filters.AddNkFilter(DeclarationFilterConstants.CountryofOrigin, GetCountryofOriginQuery, ModuleIDs.RefCountry, Lookups.Countries).WithMaxLengthOf<ModuleNkFilter>(JobComInvoiceHeaderSchema.JZ_RN_NKDefaultOrigin);
			countryofOrigin.Category = FilterCategories.Locations;
			countryofOrigin.MultilingualDescription = ResString.GetMultilingualString("CA|LVXFilterStripBusinessObject|CountryofOrigin", DeclarationFilterConstants.CountryofOrigin);
		}

		protected ZQuery GetCountryofOriginQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
			subQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, false);
			subQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_RN_NKDefaultOrigin, comparisonOperator, value);

			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddSubQuery(subQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region Status and Flags Filters

		void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			filters.AddFlagsFilter(Constants.IsReadyForConsolidation,
				new string[] { Res.GetString("6639A45B-46FC-4E37-9D61-8C00121D501A", "Ready for Consolidation?") },
				new GetFlagsQuery[] { GetIsReadyForConsolidation }).
				MultilingualDescription = ResString.GetMultilingualString("CA|LVXFilterStripBusinessObject|IsReadyForConsolidation", Constants.IsReadyForConsolidation);
		}

		ZQuery GetIsReadyForConsolidation(ZBool ready)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			var invoiceQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE, !ready);
			invoiceQuery.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.CurrentCulture, "{0} like '%ReadyForConsolidation=Y%'", JobComInvoiceHeaderSchema.JZ_AddInfo.Name), new ZSqlParameterCollection());
			result.AddSubQuery(invoiceQuery, JoinCondition.And);
			return result;
		}

		#endregion

		#region Billing Filters

		protected override IAccountingFilterStrip CreateAccountingFilterStrip()
		{
			var accountingFilterStrip = base.CreateAccountingFilterStrip();
			accountingFilterStrip.Initialize(addSupplierCostReferenceFilters: false, addOrganisationFilters: false, addDateFilters: false, addAmountFilters: false, addNumbersAndReferencesFilters: false);
			return accountingFilterStrip;
		}

		#endregion
	}
}
