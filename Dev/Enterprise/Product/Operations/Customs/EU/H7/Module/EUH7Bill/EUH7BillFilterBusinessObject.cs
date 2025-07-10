using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.H7.Module
{
	public class EUH7BillFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Descriptions
		{
			#region SuppressResourceStringsCheckRegion

			public const string BillNumber = "Bill Number";
			public const string JobReference = "Job Reference";
			public const string ManifestNumber = "Manifest Number";
			public const string MRN = "MRN";
			public const string LRN = "LRN";
			public const string Branch = "Branch";
			public const string CustomsOffice = "Customs Office";
			public const string Declarant = "Declarant";
			public const string Representative = "Representative";
			public const string MessageStatus = "Message Status";
			public const string CustomsStatus = "Customs Status";
			public const string MemberState = "Member State";
			public const string Origin = "Origin";
			public const string FinalDestination = "Final Destination";
			public const string EstimateDateOfArrival = "Estimate Date of Arrival";
			public const string ActualArrivalDate = "Actual Arrival Date";
			public const string EstimatedDateOfDeparture = "Estimated Date of Departure";
			public const string MRNIssuedDate = "MRN issued date";
			public const string ReleaseDate = "Release Date";
			public const string CustomsDocsRequired = "Customs Docs. Req.";
			public const string CustomsDocsRequiredFilterPrompt = "At least one Customs Docs. Req. open or pending";

			#endregion
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var headerQuerySubGroup = new HeaderQuerySubGroup();

			var billNumberFilter = result.AddTextFilter(Descriptions.BillNumber, AsycudaBillSchema.ABL_BillNumber)
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_BillNumber);
			billNumberFilter.Category = FilterCategories.NumbersAndReferences;
			billNumberFilter.MultilingualDescription = ResString.GetMultilingualString("EUH7BillFilterBusinessObject|BillNumberFilter", Descriptions.BillNumber);

			var jobReferenceFilter = result.AddTextFilter(Descriptions.JobReference, AsycudaManifestHeaderSchema.AMA_JobReference)
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaManifestHeaderSchema.AMA_JobReference);
			jobReferenceFilter.Category = FilterCategories.NumbersAndReferences;
			jobReferenceFilter.SubGroup = headerQuerySubGroup;
			jobReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("EUH7BillFilterBusinessObject|JobReferenceFilter", Descriptions.JobReference);

			var manifestNumberFilter = result.AddTextFilter(Descriptions.ManifestNumber, GetMasterBillQuery)
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_BillNumber);
			manifestNumberFilter.Category = FilterCategories.NumbersAndReferences;
			manifestNumberFilter.MultilingualDescription = ResString.GetMultilingualString("EUH7BillFilterBusinessObject|ManifestFilter", Descriptions.ManifestNumber);

			SetLRNAndMRNFilter(result);

			var branchFilter = result.AddGuidFilter(Descriptions.Branch, ModuleIDs.GlbBranch, AsycudaManifestHeaderSchema.AMA_GB, new GlbBranchCollection(Factory));
			branchFilter.Category = FilterCategories.Organisations;
			branchFilter.SubGroup = headerQuerySubGroup;
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("EUH7BillFilterBusinessObject|BranchFilter", Descriptions.Branch);

			var customsOfficeFilter = result.AddTextFilter(Descriptions.CustomsOffice, AsycudaManifestHeaderSchema.AMA_CustomsOffice)
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaManifestHeaderSchema.AMA_CustomsOffice);
			customsOfficeFilter.Category = FilterCategories.Organisations;
			customsOfficeFilter.SubGroup = headerQuerySubGroup;
			customsOfficeFilter.MultilingualDescription = ResString.GetMultilingualString("EUH7BillFilterBusinessObject|CustomsOfficeFilter", Descriptions.CustomsOffice);

			var declarantFilter = result.AddGuidFilter(Descriptions.Declarant, ModuleIDs.OrgAddresses, AsycudaManifestHeaderSchema.AMA_OA_Declarant, new OrgAddressCollection(Factory));
			declarantFilter.Category = FilterCategories.Organisations;
			declarantFilter.SubGroup = headerQuerySubGroup;
			declarantFilter.MultilingualDescription = ResString.GetMultilingualString("EUH7BillFilterBusinessObject|DeclarantFilter", Descriptions.Declarant);

			var representativeFilter = result.AddGuidFilter(Descriptions.Representative, ModuleIDs.OrgAddresses, AsycudaManifestHeaderSchema.AMA_OA_Representative, new OrgAddressCollection(Factory));
			representativeFilter.Category = FilterCategories.Organisations;
			representativeFilter.SubGroup = headerQuerySubGroup;
			representativeFilter.MultilingualDescription = ResString.GetMultilingualString("EUH7BillFilterBusinessObject|RepresentativeFilter", Descriptions.Representative);

			SetMessageStatusFilter(result);

			SetCustomStastusFilter(result);

			var memberStateFilter = result.AddTextFilter(Descriptions.MemberState, AsycudaManifestHeaderSchema.AMA_RN_NKCountry)
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaManifestHeaderSchema.AMA_RN_NKCountry);
			memberStateFilter.Category = FilterCategories.Locations;
			memberStateFilter.SubGroup = headerQuerySubGroup;
			memberStateFilter.MultilingualDescription = ResString.GetMultilingualString("EUH7BillFilterBusinessObject|MemberStateFilter", Descriptions.MemberState);

			var originFilter = result.AddTextFilter(Descriptions.Origin, AsycudaBillSchema.ABL_RL_NKOrigin);
			originFilter.Category = FilterCategories.Locations;
			originFilter.MultilingualDescription = ResString.GetMultilingualString("EUH7BillFilterBusinessObject|OriginFilter", Descriptions.Origin);

			var finalDestinationFilter = result.AddTextFilter(Descriptions.FinalDestination, AsycudaBillSchema.ABL_RL_NKFinalDestination);
			finalDestinationFilter.Category = FilterCategories.Locations;
			finalDestinationFilter.MultilingualDescription = ResString.GetMultilingualString("EUH7BillFilterBusinessObject|FinalDestinationFilter", Descriptions.FinalDestination);

			var estimatedDateOfArrivalFilter = result.AddDateFilter(Descriptions.EstimateDateOfArrival, (comparisonOperator, value1, value2) => GetHeaderDateQuery(AsycudaBillSchema.ABL_E_ARV, comparisonOperator, value1, value2));
			estimatedDateOfArrivalFilter.Category = FilterCategories.Dates;
			estimatedDateOfArrivalFilter.MultilingualDescription = ResString.GetMultilingualString("EUH7BillFilterBusinessObject|EstimateDateOfArrivalFilter", Descriptions.EstimateDateOfArrival);

			var actualArrivalDateFilter = result.AddDateFilter(Descriptions.ActualArrivalDate, (comparisonOperator, value1, value2) => GetHeaderDateQuery(AsycudaBillSchema.ABL_A_ARV, comparisonOperator, value1, value2));
			actualArrivalDateFilter.Category = FilterCategories.Dates;
			actualArrivalDateFilter.MultilingualDescription = ResString.GetMultilingualString("EUH7BillFilterBusinessObject|ActualArrivalDateFilter", Descriptions.ActualArrivalDate);

			var estimatedDateOfDepartureFilter = result.AddDateFilter(Descriptions.EstimatedDateOfDeparture, (comparisonOperator, value1, value2) => GetHeaderDateQuery(AsycudaBillSchema.ABL_E_DEP, comparisonOperator, value1, value2));
			estimatedDateOfDepartureFilter.Category = FilterCategories.Dates;
			estimatedDateOfDepartureFilter.MultilingualDescription = ResString.GetMultilingualString("EUH7BillFilterBusinessObject|EstimatedDateOfDepartureFilter", Descriptions.EstimatedDateOfDeparture);

			var mrnIssuedDateFilter = result.AddDateFilter(Descriptions.MRNIssuedDate, GetMRNIssuedDateQuery);
			mrnIssuedDateFilter.Category = FilterCategories.Dates;
			mrnIssuedDateFilter.MultilingualDescription = ResString.GetMultilingualString("EUH7BillFilterBusinessObject|MRNIssuedDateFilter", Descriptions.MRNIssuedDate);

			var releaseDateFilter = result.AddDateFilter(Descriptions.ReleaseDate, AsycudaBillSchema.ABL_ReleaseDate);
			releaseDateFilter.Category = FilterCategories.Dates;
			releaseDateFilter.MultilingualDescription = ResString.GetMultilingualString("EUH7BillFilterBusinessObject|ReleaseDateFilter", Descriptions.ReleaseDate);

			var customsDocsRequiredFilter = result.AddFlagsFilter(Descriptions.CustomsDocsRequired, new string[] { Descriptions.CustomsDocsRequiredFilterPrompt }, new GetFlagsQuery[] { GetCustomsDocsRequiredQuery });
			customsDocsRequiredFilter.Category = FilterCategories.StatusAndFlags;
			customsDocsRequiredFilter.MultilingualDescription = ResString.GetMultilingualString("EUH7BillFilterBusinessObject|CustomsDocsRequiredFilter", Descriptions.CustomsDocsRequired);

			return result;
		}

		protected virtual void SetLRNAndMRNFilter(ModuleFilterCollection result)
		{
			var mRNFilter = result.AddTextFilter(Descriptions.MRN, CusEntryNumSchema.CE_EntryNum)
							.WithMaxLengthOf<ModuleTextFilter>(CusEntryNumSchema.CE_EntryNum);
			mRNFilter.Category = FilterCategories.NumbersAndReferences;
			mRNFilter.SubGroup = new BillCusEntryNumSubGroup { EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber };
			mRNFilter.MultilingualDescription = ResString.GetMultilingualString("EUH7BillFilterBusinessObject|MovementReferenceNumber", Descriptions.MRN);

			var lRNFilter = result.AddTextFilter(Descriptions.LRN, CusEntryNumSchema.CE_EntryNum)
				.WithMaxLengthOf<ModuleTextFilter>(CusEntryNumSchema.CE_EntryNum);
			lRNFilter.Category = FilterCategories.NumbersAndReferences;
			lRNFilter.SubGroup = new BillCusEntryNumSubGroup { EntryType = CusEntryNumberTypes.EU.LocalReferenceNumber };
			lRNFilter.MultilingualDescription = ResString.GetMultilingualString("EUH7BillFilterBusinessObject|LocalReferenceNumber", Descriptions.LRN);
		}

		protected virtual void SetMessageStatusFilter(ModuleFilterCollection filters)
		{
			var messageStatusFilter = filters.AddTextFilter(Descriptions.MessageStatus, AsycudaBillSchema.ABL_MessageStatus, Factory.GetCachedValue<LogicalStatusList>());
			messageStatusFilter.Category = FilterCategories.StatusAndFlags;
			messageStatusFilter.MultilingualDescription = ResString.GetMultilingualString("EUH7BillFilterBusinessObject|MessageStatusFilter", Descriptions.MessageStatus);
			messageStatusFilter.ComparisonOperator_List.Clear();
			messageStatusFilter.ComparisonOperator_List.AddPair(ModuleNumberFilter.ComparisonConstants.Exact);
			messageStatusFilter.ComparisonOperator_List.AddPair(ModuleNumberFilter.ComparisonConstants.NotEqual);
			messageStatusFilter.ComparisonOperator_List.AddPair(ModuleNumberFilter.ComparisonConstants.IsBlank);
			messageStatusFilter.ComparisonOperator_List.AddPair(ModuleNumberFilter.ComparisonConstants.IsNotBlank);
			messageStatusFilter.ComparisonOperator_List.DefaultCode = ModuleNumberFilter.ComparisonConstants.Exact;
		}

		protected virtual void SetCustomStastusFilter(ModuleFilterCollection filters)
		{
			var customsStatusFilter = filters.AddTextFilter(Descriptions.CustomsStatus, AsycudaBillSchema.ABL_BillStatus, Lookups.CustomsStatusList);
			customsStatusFilter.Category = FilterCategories.StatusAndFlags;
			customsStatusFilter.MultilingualDescription = ResString.GetMultilingualString("EUH7BillFilterBusinessObject|CustomsStatusFilter", Descriptions.CustomsStatus);
			customsStatusFilter.ComparisonOperator_List.Clear();
			customsStatusFilter.ComparisonOperator_List.AddPair(ModuleNumberFilter.ComparisonConstants.Exact);
			customsStatusFilter.ComparisonOperator_List.AddPair(ModuleNumberFilter.ComparisonConstants.NotEqual);
			customsStatusFilter.ComparisonOperator_List.AddPair(ModuleNumberFilter.ComparisonConstants.IsBlank);
			customsStatusFilter.ComparisonOperator_List.AddPair(ModuleNumberFilter.ComparisonConstants.IsNotBlank);
			customsStatusFilter.ComparisonOperator_List.DefaultCode = ModuleNumberFilter.ComparisonConstants.Exact;
		}

		ZQuery GetCustomsDocsRequiredQuery(ZBool value)
		{
			var result = new ZDBOnlyQuery(typeof(AsycudaBill));

			if (value)
			{
				var requestedDocumentSubQuery = new ZDBOnlySubQuery(typeof(EU.Business.RequestedDocument), CusSupportingInfoSchema.CSI_ParentID);
				requestedDocumentSubQuery.AddToFilter(CusSupportingInfoSchema.CSI_Status, SQLComparisonOperator.Equal, EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened);
				requestedDocumentSubQuery.AddToFilter(JoinCondition.Or, CusSupportingInfoSchema.CSI_Status, SQLComparisonOperator.Equal, EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.PhysicallyPresentDocument);

				result.AddSubQuery(requestedDocumentSubQuery, JoinCondition.And);
			}

			return result;
		}

		ZQuery GetBillQuery(ZDBOnlySubQuery headerQuery)
		{
			var result = new ZDBOnlyQuery(typeof(AsycudaBill));
			result.AddSubQuery(AsycudaBillSchema.ABL_AMA, headerQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetHeaderDateQuery(SchemaDateTimeColumn column, DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var headerQuery = new ZDBOnlySubQuery(typeof(AsycudaManifestHeader), AsycudaManifestHeaderSchema.PK);
			var billQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
			switch (comparisonOperator)
			{
				case DateComparisonOperator.HasDateEntered:
					billQuery.AddToFilter(column, value1);
					break;
				case DateComparisonOperator.HasNoDateEntered:
					billQuery.AddToFilter(column, ZDateTime.Empty);
					break;
				default:
					AddDateTimeRange(billQuery, comparisonOperator, JoinCondition.And, column, value1, value2);
					break;
			}
			headerQuery.AddSubQuery(billQuery, JoinCondition.And);
			return GetBillQuery(headerQuery);
		}

		ZQuery GetMRNIssuedDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var result = new ZDBOnlyQuery(typeof(AsycudaBill));
			var subQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_IssueDate);
			subQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			AddDateTimeRange(subQuery, comparisonOperator, JoinCondition.And, CusEntryNumSchema.CE_IssueDate, value1, value2);
			result.AddSubQuery(AsycudaBillSchema.PK, CusEntryNumSchema.CE_ParentID, subQuery, JoinCondition.And);

			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				var cusNoMRNSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, true);
				cusNoMRNSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
				cusNoMRNSubQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Env.CurrentCompany.Country.Code);
				result.AddSubQuery(cusNoMRNSubQuery, JoinCondition.Or);
			}

			return result;
		}

		ZQuery GetMasterBillQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var headerQuery = new ZDBOnlySubQuery(typeof(AsycudaManifestHeader), AsycudaManifestHeaderSchema.PK);
			var subQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
			subQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);
			subQuery.AddToFilter(AsycudaBillSchema.ABL_BillNumber, comparisonOperator, value);
			headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.PK, AsycudaBillSchema.ABL_AMA, subQuery, JoinCondition.And);
			return GetBillQuery(headerQuery);
		}

		public EUH7BillFilterBusinessObjectLookups Lookups => GetNewLookups();

		protected virtual EUH7BillFilterBusinessObjectLookups GetNewLookups() => new EUH7BillFilterBusinessObjectLookups(this);

		#region Sub Group Filters

		protected class HeaderQuerySubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var billQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
				var headerQuery = new ZDBOnlySubQuery(typeof(AsycudaManifestHeader), AsycudaManifestHeaderSchema.PK);
				headerQuery.AddToFilter(filter);
				billQuery.AddSubQuery(AsycudaBillSchema.ABL_AMA, AsycudaManifestHeaderSchema.PK, headerQuery, JoinCondition.And);
				return billQuery;
			}
		}

		protected class BillCusEntryNumSubGroup : HeaderQuerySubGroup
		{
			public string EntryType;
			public string EntryLineReference;

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var billQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
				var cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				cusEntryNumQuery.AddToFilter(filter);
				cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, EntryType);
				if (!string.IsNullOrEmpty(EntryLineReference))
				{
					cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryLineReference, EntryLineReference);
				}
				billQuery.AddSubQuery(AsycudaBillSchema.PK, CusEntryNumSchema.CE_ParentID, cusEntryNumQuery, JoinCondition.And);
				return billQuery;
			}
		}

		#endregion
	}
}
