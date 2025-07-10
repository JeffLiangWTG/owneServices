using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief.GenericMessagingHarness;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Module;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Module
{
	public class JobDeclarationFilterBusinessObject : EU.Module.JobDeclarationFilterBusinessObject
	{
		public static class FilterConstants
		{
			public const string LocationOfGoods = "Location of Goods";
			public const string Shed = "Shed";
			public const string CSP = "CSP";
			public const string RouteOfEntry = "Route Of Entry";
			public const string IRC = "IRC Inventory Return Code";
			public const string ICS = "ICS";
			public const string Badge = "Badge";
			public const string HouseSplitReference = "House Split Reference";
			public const string VATDeferType = "VAT Defer Type";
			public const string VATDeferNumber = "VAT Defer Number";
			public const string OtherDeferType = "Other Defer Type";
			public const string OtherDeferNumber = "Other Defer Number";
			public const string SuppDecsDeclaredPackages = "SuppDecs' declared #packages";
			public const string SuppDecsOutstanding = "SuppDecs outstanding";
			public const string SuppDecsOutstandingPackages = "SuppDecs' outstanding #packages";
			public const string EIDRType = "EIDR Type";
			public const string SupplementaryDeclarationDueDate = "Supplementary Declaration Due Date";
			public const string TaxPoint = "Tax Point";
			public const string DateOfExit = "Date of Exit";
			public const string NorthernIrelandMode = "Northern Ireland Mode";
			public const string EUSubsidy = "EU Subsidy";
			public const string AreGoodsAtRisk = "Are Goods At Risk?";
			public const string ShortLocationOfGoodsForCDS = "Location (short) of Goods for CDS";
			public const string FullLocationOfGoodsForCDS = "Location (full) of Goods for CDS";
			public const string GVMSEnabledLocations = "GVMS-enabled locations";
			public const string HasInventoryReference = "Has Inventory Reference";
			public const string PrelodgedVersusLodged = "Pre-lodged versus lodged";
			public const string InventoryConsignmentReference = "Inventory Consignment Reference (MUCR)";
			public const string DUCR = "DUCR (Declaration Unique Consignment Reference)";
		}

		protected override ModuleTextFilter AddLocationOfGoodsFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(FilterConstants.LocationOfGoods, GetLocationOfGoodsPortQuery);
			filter.MultilingualDescription = ResString.GetMultilingualString("DCD3510B-36B6-4B64-8A57-5167D7A233FC", FilterConstants.LocationOfGoods);
			filter.Category = FilterCategories.Locations;
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			return filter;
		}

		ZQuery AddCusEntryHeaderAddInfoQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var cusHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
			cusHeaderQuery.AddToFilter(Customs.Business.AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.StartsWith, value, CusEntryHeaderSchema.CH_AddInfo, EUAddInfoSchema.ZG_ExitActualOffice.Name.Substring(3)));
			var jobDecQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			jobDecQuery.AddSubQuery(cusHeaderQuery, JoinCondition.And);
			return jobDecQuery;
		}

		ModuleTextFilter AddCusEntryHeaderAddInfoFilter(ZString description, ModuleFilterCollection filters, CodeDescriptionPairList filterList, SchemaStringColumn filterColumn, string columnDescription)
		{
			var filterDescription = columnDescription + "=";
			var filter = filters.AddTextFilter(description,
				delegate (ZString value)
				{
					var cusHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
					cusHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_AddInfo, SQLComparisonOperator.Contains, filterDescription + value);
					var jobDecQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
					jobDecQuery.AddSubQuery(cusHeaderQuery, JoinCondition.And);
					return jobDecQuery;
				}, filterList).WithMaxLengthOf<ModuleTextFilter>(filterColumn);
			filter.Category = FilterCategories.ModesAndTypes;
			return filter;
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();

			var filter = filters.AddTextFilter(FilterConstants.Shed, GetShedQuery);
			filter.MultilingualDescription = ResString.GetMultilingualString("5D75598A-8D91-462D-BBF6-1E4BB901AACB", FilterConstants.Shed);
			filter.Category = FilterCategories.Locations;
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);

			filter = GetAddInfoTextFilter(FilterConstants.CSP, JobDeclaration.Schema.ZG_Gateway.Substring(3), GatewayList);
			filter.MultilingualDescription = ResString.GetMultilingualString("0C51102D-71CD-40DF-BE87-6E8217E5BDD1", FilterConstants.CSP);
			filter.Category = FilterCategories.ModesAndTypes;
			filters.AddFilter(filter);

			AddCusEntryHeaderAddInfoFilter(FilterConstants.RouteOfEntry, filters, RouteOfEntryList, EUAddInfoSchema.ZG_RouteOfEntry, CusEntryHeader.Schema.RouteOfEntry.Substring(3))
				.MultilingualDescription = ResString.GetMultilingualString("209978F0-E117-47C1-9B36-2DFBB01F80FD", FilterConstants.RouteOfEntry);
			AddCusEntryHeaderAddInfoFilter(FilterConstants.IRC, filters, InventoryReturnCodeList, EUAddInfoSchema.ZG_IrcInventoryReturnCode, CusEntryHeader.Schema.IrcInventoryReturnCode.Substring(3))
				.MultilingualDescription = ResString.GetMultilingualString("576008EF-6F34-4F57-88E8-9D6D44A7D2AB", FilterConstants.IRC);
			AddCusEntryHeaderAddInfoFilter(FilterConstants.ICS, filters, ImportClearanceList, GBJobDeclarationSchema.JE_ImportClearanceStatusICS, CusEntryHeader.Schema.ImportClearanceStatusICS.Substring(3))
				.MultilingualDescription = ResString.GetMultilingualString("86DF7F7F-A311-438A-B629-D717E01F7346", FilterConstants.ICS);

			filter = filters.AddTextFilter(FilterConstants.Badge, JobDeclarationSchema.JE_CustomsProfile);
			filter.MultilingualDescription = ResString.GetMultilingualString("F8E1C5F3-0AA0-4186-80EE-881A5E0DD9D9", FilterConstants.Badge);
			filter.Category = FilterCategories.TextSearch;

			var actualOfficeOfExitFilter = new AddInfoModuleTextFilter(EntryHeaderFilterBusinessObject.FilterConstants.ActualOfficeOfExit, AddCusEntryHeaderAddInfoQuery);
			actualOfficeOfExitFilter.MultilingualDescription = ResString.GetMultilingualString("35CA82AD-5E19-4E2A-9856-FA195D09E6D9", EntryHeaderFilterBusinessObject.FilterConstants.ActualOfficeOfExit);
			actualOfficeOfExitFilter.Category = FilterCategories.TextSearch;
			actualOfficeOfExitFilter.MaxLength = EUAddInfoSchema.ZG_ExitActualOffice.MaxLength;
			filters.AddFilter(actualOfficeOfExitFilter);

			filter = GetAddInfoTextFilter(FilterConstants.HouseSplitReference, JobDeclaration.Schema.ZG_HouseSplitReference.Substring(3));
			filter.MultilingualDescription = ResString.GetMultilingualString("9185031B-AAFC-458B-B1CD-E009BAC9B590", FilterConstants.HouseSplitReference);
			filter.Category = FilterCategories.TextSearch;
			filters.AddFilter(filter);

			filter = GetAddInfoTextFilter(FilterConstants.VATDeferType, JobDeclaration.Schema.ZG_VATDeferType.Substring(3), DefermentMethodList);
			filter.MultilingualDescription = ResString.GetMultilingualString("4AF67A77-DB7B-4B55-B27A-FE2199B00306", FilterConstants.VATDeferType);
			filter.Category = FilterCategories.NumbersAndReferences;
			filters.AddFilter(filter);

			filter = GetAddInfoTextFilter(FilterConstants.VATDeferNumber, JobDeclaration.Schema.ZG_VATDeferNumber.Substring(3));
			filter.MultilingualDescription = ResString.GetMultilingualString("649538CC-C2E8-466E-B0FF-CAB86CF445F2", FilterConstants.VATDeferNumber);
			filter.Category = FilterCategories.NumbersAndReferences;
			filters.AddFilter(filter);

			filters.AddTextFilter(FilterConstants.OtherDeferType, JobDeclarationSchema.JE_PaymentMethod, DefermentMethodList)
				.MultilingualDescription = ResString.GetMultilingualString("5802DCDE-C204-4024-8F16-DF95B28B7D37", FilterConstants.OtherDeferType);

			filter = filters.AddTextFilter(FilterConstants.OtherDeferNumber, JobDeclarationSchema.JE_DefermentAccountNumber)
				.WithMaxLengthOf<ModuleTextFilter>(JobDeclarationSchema.JE_DefermentAccountNumber);
			filter.MultilingualDescription = ResString.GetMultilingualString("0EB0F536-FBA6-4C80-8168-0AD22A0A19F8", FilterConstants.OtherDeferNumber);
			filter.Category = FilterCategories.NumbersAndReferences;

			#region Supplementary dec bits
			filters.AddNumberRangeFilter(FilterConstants.SuppDecsDeclaredPackages, GetSupplementaryCountQueryNumericMatch)
				.MultilingualDescription = ResString.GetMultilingualString("52DFE49F-5F68-4ECA-92C2-DF902532DA7D", FilterConstants.SuppDecsDeclaredPackages);
			filters.AddFlagsFilter(FilterConstants.SuppDecsOutstanding, ["Ticked for exhausted, unticked for outstanding"], new GetFlagsQuery[] { GetSupplementaryCountQuerySimpleFlag })
				.MultilingualDescription = ResString.GetMultilingualString("D2B54CCC-29A0-4B20-B414-BD3427107189", FilterConstants.SuppDecsOutstanding);
			filters.AddNumberRangeFilter(FilterConstants.SuppDecsOutstandingPackages, GetSupplementaryCountQueryNumericMatchOutstanding)
				.MultilingualDescription = ResString.GetMultilingualString("0DEE9FD5-589D-4D98-A1DB-D2AA363EB132", FilterConstants.SuppDecsOutstandingPackages);
			#endregion

			filter = filters.AddTextFilter(FilterConstants.EIDRType, GetEidrTypeQuery, EidrTypeList).WithMaxLengthOf<ModuleTextFilter>(GBJobDeclarationSchema.JE_EidrType);
			filter.Category = FilterCategories.ModesAndTypes;
			filter.MultilingualDescription = ResString.GetMultilingualString("C146600D-45F0-4697-8041-CDC82F1D3428", FilterConstants.EIDRType);
			ModuleFilter dateFilter = filters.AddDateFilter(FilterConstants.SupplementaryDeclarationDueDate, GetSuppDecDueDateQuery);
			dateFilter.MultilingualDescription = ResString.GetMultilingualString("CBB227DE-7326-4CAC-95EB-9AB5A149276F", FilterConstants.SupplementaryDeclarationDueDate);
			dateFilter.Category = FilterCategories.Dates;
			dateFilter = filters.AddDateFilter(FilterConstants.TaxPoint, JobDeclarationSchema.JE_EntryAuthorisationDate);
			dateFilter.MultilingualDescription = ResString.GetMultilingualString("F5A8CB7A-8E20-44E1-A98B-0F3FA865EA0A", FilterConstants.TaxPoint);
			dateFilter.Category = FilterCategories.Dates;
			dateFilter = filters.AddSingleDateFilter(FilterConstants.DateOfExit, GetExitDateQuery);
			dateFilter.MultilingualDescription = ResString.GetMultilingualString("77EB4A8D-CB6E-4652-8770-F41487C2E547", FilterConstants.DateOfExit);
			dateFilter.Category = FilterCategories.Dates;

			#region NI Protocol
			filters.AddTextFilter(FilterConstants.NorthernIrelandMode, GetNorthernIrelandModeQuery, NorthernIrelandModeList)
				.MultilingualDescription = ResString.GetMultilingualString("07419DC1-8AA2-47E4-8588-790606448580", FilterConstants.NorthernIrelandMode);
			filters.AddFlagsFilter(FilterConstants.EUSubsidy, ["EU Subsidy (NI Protocol) claimed"], new GetFlagsQuery[] { GetIsEUSubsidyQuery })
				.MultilingualDescription = ResString.GetMultilingualString("ABD2BF5C-BAE4-41D8-9C4F-CBD187F16540", FilterConstants.EUSubsidy);
			filters.AddFlagsFilter(FilterConstants.AreGoodsAtRisk, ["Goods at risk (NI Protocol)"], new GetFlagsQuery[] { GetIsAtRiskQuery })
				.MultilingualDescription = ResString.GetMultilingualString("65BF1713-E4C7-4964-8AD9-1BE23D8A35F7", FilterConstants.AreGoodsAtRisk);
			#endregion

			filter = filters.AddTextFilter(FilterConstants.ShortLocationOfGoodsForCDS, JobDeclarationSchema.JE_SubLocationOfGoods);
			filter.MultilingualDescription = ResString.GetMultilingualString("65F3D696-4E5B-4D48-B249-C0D6DD04A1B0", FilterConstants.ShortLocationOfGoodsForCDS);
			filter.Category = FilterCategories.Locations;

			filter = filters.AddTextFilter(FilterConstants.FullLocationOfGoodsForCDS, GetLocationOtherInformationQuery);
			filter.MultilingualDescription = ResString.GetMultilingualString("08B38D47-B610-478B-98BF-34A0B6FB1A69", FilterConstants.FullLocationOfGoodsForCDS);
			filter.Category = FilterCategories.Locations;

			filters.AddFlagsFilter(FilterConstants.GVMSEnabledLocations, ["GVMS-enabled"], new GetFlagsQuery[] { GetIsGVMSEnabled })
				.MultilingualDescription = ResString.GetMultilingualString("4F36C973-0CE9-435C-A344-67496E765196", FilterConstants.GVMSEnabledLocations);
			filters.AddFlagsFilter(FilterConstants.HasInventoryReference, ["Has Inventory Ref"], new GetFlagsQuery[] { GetHasInventoryReference })
				.MultilingualDescription = ResString.GetMultilingualString("E41FA1A9-B1CD-4929-8D76-529C5282A14E", FilterConstants.HasInventoryReference);
			filters.AddFlagsFilter(FilterConstants.PrelodgedVersusLodged, ["Arrived/Lodged"], new GetFlagsQuery[] { GetIsLodged })
				.MultilingualDescription = ResString.GetMultilingualString("E13B8EA6-1D35-44A4-8B9E-17EDE2467E66", FilterConstants.PrelodgedVersusLodged);

			var invRefFilter = filters.AddTextFilter(FilterConstants.InventoryConsignmentReference, GetInventoryConsignmentReferenceQuery);
			invRefFilter.MultilingualDescription = ResString.GetMultilingualString("AE23B22D-767B-410D-A779-63DBB510A172", FilterConstants.InventoryConsignmentReference);
			invRefFilter.Category = FilterCategories.NumbersAndReferences;

			var ducrFilter = filters.AddTextFilter(FilterConstants.DUCR, GetDucrQuery);
			ducrFilter.MultilingualDescription = ResString.GetMultilingualString("DUCR_FILTER_ID", FilterConstants.DUCR);
			ducrFilter.Category = FilterCategories.NumbersAndReferences;

			return filters;
		}

		ZQuery GetDucrQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(JobDeclaration));

			if (!value.IsEmpty)
			{
				if (comparisonOperator == SQLComparisonOperator.NotContains)
				{
					var orQuery = new ZQuery();
					orQuery.AddToFilter(JobDeclarationSchema.JE_UCR, SQLComparisonOperator.NotContains, value);
					orQuery.AddToFilter(JobDeclarationSchema.JE_UCR, SQLComparisonOperator.Equal, null);

					query.AddToFilter(orQuery, JoinCondition.And);
				}
				else
				{
					query.AddToFilter(JobDeclarationSchema.JE_UCR, comparisonOperator, value);
				}
			}

			return query;
		}

		ZQuery GetExitDateQuery(ZDateTime value)
		{
			var result = new ZQuery();

			if (!value.IsEmpty)
			{
				var cusHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
				cusHeaderQuery.AddToFilter(Customs.Business.AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.StartsWith, value.ToISO8601ShortDateString(), CusEntryHeaderSchema.CH_AddInfo, EUAddInfoSchema.ZG_ExitDate.Name.Substring(3)));
				var jobDecQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
				jobDecQuery.AddSubQuery(cusHeaderQuery, JoinCondition.And);
				return jobDecQuery;
			}
			return result;
		}

		protected ZQuery GetEidrTypeQuery(ZString value)
		{
			return DeclarationModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, DeclarationModelViewPK, DeclarationModelView, GBJobDeclarationSchema.Constants.JE_EidrType, SQLComparisonOperator.Equal, value);
		}

		protected ZQuery GetSuppDecDueDateQuery(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			return DeclarationModelViewColumnHelper.GetDateFilterQuery(JobDeclaration.Schema.PK, DeclarationModelViewPK, DeclarationModelView, GBJobDeclarationSchema.Constants.JE_SuppDecDueDate, comparisonOperator, startDate, endDate);
		}

		protected ZQuery GetNorthernIrelandModeQuery(ZString mode)
		{
			return DeclarationModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, DeclarationModelViewPK, DeclarationModelView, GBJobDeclarationSchema.Constants.JE_NorthernIrelandMode, SQLComparisonOperator.Equal, mode);
		}

		ZQuery GetIsEUSubsidyQuery(ZBool value)
		{
			return DeclarationModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, DeclarationModelViewPK, DeclarationModelView, GBJobDeclarationSchema.Constants.JE_ClaimEuSubsidy, SQLComparisonOperator.Equal, value);
		}

		ZQuery GetIsAtRiskQuery(ZBool value)
		{
			return DeclarationModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, DeclarationModelViewPK, DeclarationModelView, GBJobDeclarationSchema.Constants.JE_NiGoodsAtRiskOfMovingToROI, SQLComparisonOperator.Equal, value);
		}

		Customs.Business.ModelViewColumnQueryHelper<JobDeclaration> DeclarationModelViewColumnHelper => declarationModelViewColumnHelper ??= new();
		Customs.Business.ModelViewColumnQueryHelper<JobDeclaration> declarationModelViewColumnHelper;
		string DeclarationModelView => GBJobDeclarationSchema.Constants.TableName;
		string DeclarationModelViewPK => GBJobDeclarationSchema.Constants.PK;

		ZQuery GetIsGVMSEnabled(ZBool value)
		{
			var query = new ZQuery();
			if (!value)
			{
				return query;
			}

			var filter = string.Concat(JobDeclaration.Schema.ZG_IsGvmsPort.Substring(3), "=", YesNoList.Codes.Yes);
			query.AddToFilter(JobDeclarationSchema.JE_AddInfo, ContainsComparisonOperator.Contains, filter);
			return query;
		}

		ZQuery GetHasInventoryReference(ZBool value)
		{
			if (!value)
			{
				return new ZQuery();
			}

			var cusEntryNumQ = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			cusEntryNumQ.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.EU.MasterUCR);
			var mainQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			mainQuery.AddSubQuery(cusEntryNumQ, JoinCondition.And);

			return mainQuery;
		}

		ZQuery GetIsLodged(ZBool value)
		{
			if (!value)
			{
				return new ZQuery();
			}
			var ceiQ = new ZDBOnlySubQuery(typeof(CusEntryInstruction), CusEntryInstructionSchema.CEI_JE);
			ceiQ.AddToFilter(CusEntryInstructionSchema.CEI_SubStyle, JobDeclaration.GoodsArrivedSubStyles);
			var mainQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			mainQuery.AddSubQuery(ceiQ, JoinCondition.And);

			return mainQuery;
		}

		ZQuery GetModuleFilterQuery(ZString sqlText, ZString columnName, SQLComparisonOperator filterOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			ZString sqlPrefix = ZString.Empty;
			ZString sqlSuffix = ZString.Empty;
			ZString sqlOperator = "=";

			if (filterOperator == SQLComparisonOperator.StartsWith)
			{
				sqlOperator = "LIKE";
				sqlSuffix = "%";
			}
			else if (filterOperator == SQLComparisonOperator.DoesNotStartWith)
			{
				sqlOperator = "NOT LIKE";
				sqlSuffix = "%";
			}
			else if (filterOperator == SQLComparisonOperator.Contains)
			{
				sqlOperator = "LIKE";
				sqlPrefix = "%";
				sqlSuffix = "%";
			}
			else if (filterOperator == SQLComparisonOperator.NotContains)
			{
				sqlOperator = "NOT LIKE";
				sqlPrefix = "%";
				sqlSuffix = "%";
			}
			else if (filterOperator == SQLComparisonOperator.Equal)
			{
				sqlOperator = "=";
			}
			else if (filterOperator == SQLComparisonOperator.NotEqual)
			{
				sqlOperator = "<>";
			}

			result.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.CurrentCulture, sqlText, columnName, sqlOperator, sqlPrefix + value + sqlSuffix), new ZSqlParameterCollection());
			return result;
		}

		ZQuery GetLocationOfGoodsPortQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetModuleFilterQuery("LTRIM(RTRIM(LEFT({0},5))) {1} '{2}'", JobDeclaration.Schema.JE_LocationOfGoods, filterOperator, value);
		}

		ZQuery GetShedQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetModuleFilterQuery("LTRIM(SUBSTRING({0}+'   ', 4,3)) {1} '{2}'", JobDeclaration.Schema.JE_SubLocationOfGoods, filterOperator, value);
		}

		ZQuery GetLocationOtherInformationQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var cleanedValue = value.Replace(" ", "");
			return GetModuleFilterQuery("REPLACE({0}, ' ', '') {1} '{2}'", JobDeclaration.Schema.JE_LocationOtherInformation, filterOperator, cleanedValue);
		}

		ZQuery GetSupplementaryCountQueryNumericMatch(INumericZType value1, INumericZType value2)
		{
			var query = new ZDBOnlyQuery(typeof(JobDeclaration));
			query.AddFilterAndZSQLParameterCollection(subQueryForSupplementary + " between " + value1 + " and " + value2, null);
			return query;
		}

		ZQuery GetSupplementaryCountQuerySimpleFlag(ZBool value)
		{
			var query = new ZDBOnlyQuery(typeof(JobDeclaration));
			var equality = value ? " = " : " !=";
			query.AddFilterAndZSQLParameterCollection(JobDeclaration.Schema.JE_TotalNoOfPacks + equality + subQueryForSupplementary, null);
			return query;
		}

		ZQuery GetSupplementaryCountQueryNumericMatchOutstanding(INumericZType value1, INumericZType value2)
		{
			var query = new ZDBOnlyQuery(typeof(JobDeclaration));
			query.AddFilterAndZSQLParameterCollection("JobDeclaration.JE_TotalNoOfPacks - " + subQueryForSupplementary + " between " + value1 + " and " + value2, null);
			return query;
		}

		ZQuery GetInventoryConsignmentReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (value.IsEmpty)
			{
				return new ZQuery();
			}

			var cusEntryNumQ = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			cusEntryNumQ.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.EU.MasterUCR);
			cusEntryNumQ.AddToFilter(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);
			var mainQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			mainQuery.AddSubQuery(cusEntryNumQ, JoinCondition.And);

			return mainQuery;
		}

		readonly string subQueryForSupplementary = " isnull ( ( select sum(child.JE_TotalNoOfPacks) from dbo.JobDeclaration child inner join dbo.genpivot on XX_Relation2ID = child.JE_PK and  XX_Relation1ID = JobDeclaration.je_pk and XX_RelationType = 'sup' ) , 0)";

		#region Lookups

		public CodeDescriptionPairList ImportClearanceList
		{
			get { return new DeclarationStatusICSList(); }
		}

		public CodeDescriptionPairList StyleOfEntryList
		{
			get { return new ExportStyleOfEntries(); }
		}

		public CodeDescriptionPairList RouteOfEntryList
		{
			get { return new RouteOfEntryList(); }
		}

		public CodeDescriptionPairList InventoryReturnCodeList
		{
			get { return new InventoryReturnCodesCCS(); }
		}

		public CodeDescriptionPairList GatewayList
		{
			get { return new GatewayList(); }
		}

		public CodeDescriptionPairList DefermentMethodList
		{
			get { return new DefermentMethodList(); }
		}

		public CodeDescriptionPairList EidrTypeList
		{
			get { return new EidrTypeList(); }
		}

		public CodeDescriptionPairList NorthernIrelandModeList
		{
			get
			{
				var list = new NIModeList();
				list.RemoveCode(NIModeList.Codes.NotToOrFromNi);
				return list;
			}
		}

		public new EU.Module.JobDeclarationFilterLookups Lookups
		{
			get { return (JobDeclarationFilterLookups)base.Lookups; }
		}

		protected override Customs.Module.JobDeclarationFilterLookups GetNewLookups()
		{
			return new JobDeclarationFilterLookups(this);
		}

		#endregion

		public override MultilingualString ApplicationCodeFilterCaption => ResString.GetMultilingualString("GBCustoms|DeclarationFilter|SubmitType", "Messaging System");

		GenAddOnColumnHelper GenAddOnColumnHelper
		{
			get { return helper ?? (helper = new GenAddOnColumnHelper()); }
		}
		GenAddOnColumnHelper helper;

		internal GenAddOnColumnQueryHelper SimpleQueryHelper
		{
			get { return GenAddOnColumnHelper.SimpleQueryHelper; }
		}
	}
}
