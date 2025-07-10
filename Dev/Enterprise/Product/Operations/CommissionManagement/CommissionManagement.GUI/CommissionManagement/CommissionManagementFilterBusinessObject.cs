using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.CommissionManagement.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.GUI
{
	public class CommissionManagementFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filter

		public override ZQuery Filter
		{
			get
			{
				var result = base.Filter;
				if (OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.Value)
				{
					result.AddToFilter(ViewCommissionLineSchema.VCL_GC_Company, GlbCompany.CurrentCompany.PK);
				}

				if (!Env.Security.CommissionManagerViewForAnyEntity.IsAllowed)
				{
					var orgProxyPk = OrgCommissionAgreementRecipient.GetStaffOrgProxyPk(GlbStaff.CurrentUser);
					if (!Env.Security.CommissionManagerViewForAnyEntityInOrganisation.IsAllowed || orgProxyPk.IsEmpty)
					{
						result.AddToFilter(ViewCommissionLineSchema.VCL_GS_NKStaff, GlbStaff.CurrentUser.GS_Code);
					}
					else
					{
						result.AddToFilter(ViewCommissionLineSchema.VCL_OH_Party, orgProxyPk);
					}
				}

				var isChargeCodeCommissionableOrLineIsApprovedOrPaidQuery = new ZQuery(ViewCommissionLineSchema.VCL_IsChargeCodeCommissionable, 1);
				isChargeCodeCommissionableOrLineIsApprovedOrPaidQuery.AddToFilter(JoinCondition.Or, ViewCommissionLineSchema.VCL_ApprovedDateTimeUtc, SQLComparisonOperator.NotEqual, null);
				isChargeCodeCommissionableOrLineIsApprovedOrPaidQuery.AddToFilter(JoinCondition.Or, ViewCommissionLineSchema.VCL_PaidDateTimeUtc, SQLComparisonOperator.NotEqual, null);

				result.AddToFilter(isChargeCodeCommissionableOrLineIsApprovedOrPaidQuery);

				return result;
			}
		}

		#endregion

		#region Module Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddCommissionHeaderFilters(filters);
			AddCommissionAgreementFilters(filters);
			AddCommissionLineFilters(filters);

			return filters;
		}

		#region CommissionHeader Filters

		public void AddCommissionHeaderFilters(ModuleFilterCollection filters)
		{
			var customerFilter = filters.AddGuidFilter(FilterDescription.Customer, ModuleIDs.Organisation, AccCommissionHeaderSchema.CH0_OH_Customer, Headers);
			customerFilter.SubGroup = HeaderSubGroup;
			customerFilter.MultilingualDescription = ResString.GetMultilingualString("74f1da47-a150-4ce7-97d2-598ab9cabf6a", "Customer");

			var commissionStreamFilter = filters.AddTextFilter(FilterDescription.CommissionStream, AccCommissionHeaderSchema.CH0_CommissionStream, CommissionStreams);
			commissionStreamFilter.SubGroup = HeaderSubGroup;
			commissionStreamFilter.MultilingualDescription = ResString.GetMultilingualString("6877602c-b51d-4d3d-8166-c41563e55410", "Commission Stream");

			var productFilter = filters.AddTextFilter(FilterDescription.Product, AccCommissionHeaderSchema.CH0_Product, Products);
			productFilter.SubGroup = HeaderSubGroup;
			productFilter.MultilingualDescription = ResString.GetMultilingualString("0b5f8988-f5c2-4b25-aa94-0618639ba4ea", "Product");

			if (CommissionLookups.ShouldShowServicesAndSubModules)
			{
				var serviceFilter = filters.AddTextFilter(FilterDescription.Service, AccCommissionHeaderSchema.CH0_Service, Services);
				serviceFilter.SubGroup = HeaderSubGroup;
				serviceFilter.MultilingualDescription = ResString.GetMultilingualString("2727fd9b-d9dc-4842-908d-90edae41c831", "Service");

				var subModuleFilter = filters.AddTextFilter(FilterDescription.SubModule, AccCommissionHeaderSchema.CH0_SubModule, SubModules);
				subModuleFilter.SubGroup = HeaderSubGroup;
				subModuleFilter.MultilingualDescription = ResString.GetMultilingualString("36b3865d-1790-4fcd-a852-cb242eb9b522", "Sub-Module");
			}

			var transactionNumberFilter = filters.AddTextFilter(FilterDescription.TransactionNumber, GetTransactionNumberQuery);
			transactionNumberFilter.MaxLength = AccTransactionHeaderSchema.AH_TransactionNum.MaxLength;
			transactionNumberFilter.SubGroup = HeaderSubGroup;
			transactionNumberFilter.MultilingualDescription = ResString.GetMultilingualString("6be849e3-8243-4eb8-92e3-1de09799f886", "Transaction Number");

			var snapshotDateFilter = filters.AddDateFilter(FilterDescription.SnapshotDate, AccCommissionHeaderSchema.CH0_SnapshotDateTime);
			snapshotDateFilter.SubGroup = HeaderSubGroup;
			snapshotDateFilter.MultilingualDescription = ResString.GetMultilingualString("7f400771-c9d1-4a73-aa50-0fb8a0fdec9a", "Snapshot Date");
		}

		ZQuery GetTransactionNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var transactionHeaderSubquery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccCommissionHeaderSchema.CH0_GroupingSourceID);
			transactionHeaderSubquery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, comparisonOperator, value);

			var commissionHeaderSubquery = new ZDBOnlyQuery(typeof(AccCommissionHeader));
			commissionHeaderSubquery.AddToFilter(AccCommissionHeaderSchema.CH0_GroupingSourceTableCode, AccTransactionHeaderSchema.Constants.Prefix);
			commissionHeaderSubquery.AddSubQuery(transactionHeaderSubquery, JoinCondition.And);

			return commissionHeaderSubquery;
		}

		CommissionHeaderSubGroup HeaderSubGroup
		{
			get { return headerSubGroup ?? (headerSubGroup = new CommissionHeaderSubGroup()); }
		}
		CommissionHeaderSubGroup headerSubGroup;

		class CommissionHeaderSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(ViewCommissionLine));
				var headerSubquery = new ZDBOnlySubQuery(typeof(AccCommissionHeader), ViewCommissionLineSchema.VCL_CH0);
				headerSubquery.AddToFilter(filter);
				result.AddSubQuery(headerSubquery, JoinCondition.And);

				return result;
			}
		}

		#endregion

		#region CommissionAgreement Filters

		public void AddCommissionAgreementFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(FilterDescription.AgreementId, GetAgreementIdQuery).MultilingualDescription = ResString.GetMultilingualString("ca1cbbe9-13f0-4f0e-bec1-0325bf028043", "Agreement ID");
		}

		ZQuery GetAgreementIdQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var sql = string.Format(@"
VCL_CAT IN
(
	SELECT CAT_PK
	FROM dbo.OrgCommissionAgreementRecipientRate
		JOIN dbo.OrgCommissionAgreementRecipient ON CAT_CAR = CAR_PK
		JOIN dbo.OrgCommissionAgreement ON CAR_CA0 = CA0_PK
		JOIN dbo.OrgOpportunity ON CA0_P8 = P8_PK
	WHERE
		(P8_OpportunityID + CA0_Name) {0} @AgreementID
)
", comparisonOperator.ComparisonText(value));

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@AgreementID", comparisonOperator.EscapedSqlValue(value), OrgCommissionAgreementSchema.CA0_Name);

			var commissionHeaderQuery = new ZDBOnlyQuery(typeof(AccCommissionHeader));
			commissionHeaderQuery.AddFilterAndZSQLParameterCollection(sql, sqlParams);

			return commissionHeaderQuery;
		}

		#endregion

		#region CommissionLine Filters

		public void AddCommissionLineFilters(ModuleFilterCollection filters)
		{
			if (!OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.Value)
			{
				var companyFilter = filters.AddGuidFilter(FilterDescription.Company, ModuleIDs.GlbCompany, ViewCommissionLineSchema.VCL_GC_Company, Companies);
				companyFilter.MultilingualDescription = ResString.GetMultilingualString("47118ed6-4855-467b-839a-e4259ff23b74", "Company");
			}

			var entityStaffFilter = new EntityStaffModuleFilter(FilterDescription.EntityStaff, Staff);
			entityStaffFilter.MultilingualDescription = ResString.GetMultilingualString("71b635bc-b239-45d9-98b9-1496cb06ce5f", "Entity Staff");
			entityStaffFilter.Category = FilterCategories.Organisations;
			filters.AddFilter(entityStaffFilter);

			var entityOrgFilter = new EntityOrganisationModuleFilter(FilterDescription.EntityOrganisation, Headers);
			entityOrgFilter.MultilingualDescription = ResString.GetMultilingualString("c6cc00af-f484-431f-87b7-c192b707aa83", "Entity Organization");
			filters.AddFilter(entityOrgFilter);

			filters.AddTextFilter(FilterDescription.CommissionType, ViewCommissionLineSchema.VCL_CommissionType, CommissionTypesIncludingCustom)
				.MultilingualDescription = ResString.GetMultilingualString("1df35883-d879-4422-9f37-8129c57e3243", "Commission Type");

			filters.AddGuidFilter(FilterDescription.ApprovalRequest, ModuleIDs.CommissionApprovalRequest, GetApprovalRequestQuery, CommissionApprovalRequests)
				.MultilingualDescription = ResString.GetMultilingualString("b15e8ef9-fb19-4990-9d5d-34af8b9fccf7", "Approval Request");

			var paymentStatusFilter = filters.AddTextFilter(FilterDescription.PaymentStatus, GetPaymentStatusQuery, PaymentStatuses);
			paymentStatusFilter.MultilingualDescription = ResString.GetMultilingualString("a0304e0f-4b39-4a84-b9fc-6651d93e3570", "Payment Status");
			paymentStatusFilter.Category = FilterCategories.StatusAndFlags;

			var commissionStatusFilter = filters.AddTextFilter(FilterDescription.CommissionStatus, GetCommissionStatusQuery, CommissionStatuses);
			commissionStatusFilter.MultilingualDescription = ResString.GetMultilingualString("60ed2382-7b42-40b1-bddf-e2eb04d53e96", "Commission Status");
			commissionStatusFilter.Category = FilterCategories.StatusAndFlags;
			RemoveAllComparisonOperatorsExceptExactAndNotEquals(commissionStatusFilter);

			var excludeCanceledFilter = filters.AddFlagsFilter(FilterDescription.ExcludeCanceled,
				new[] { Res.GetString("9d618817-e3d6-4569-9cc9-158cb13eeee7", "Yes") },
				new GetFlagsQuery[] { GetExcludeCanceledQuery });
			excludeCanceledFilter.MultilingualDescription = ResString.GetMultilingualString("f71b4f4b-3623-4041-8fa1-d3562821c7ff", "Exclude Canceled");
			excludeCanceledFilter.Property0 = ZBool.True;
			excludeCanceledFilter.Visibility = FilterVisibility.AlwaysApplied;

			var excludeWithheldFilter = filters.AddFlagsFilter(FilterDescription.ExcludeWithheld,
				new[] { Res.GetString("9d618817-e3d6-4569-9cc9-158cb13eeee7", "Yes") },
				new GetFlagsQuery[] { GetExcludeWithheldQuery });
			excludeWithheldFilter.MultilingualDescription = ResString.GetMultilingualString("7294698d-064c-4ffb-96a1-c651d29812ad", "Exclude Withheld");
			excludeWithheldFilter.Property0 = ZBool.True;
			excludeWithheldFilter.Visibility = FilterVisibility.AlwaysApplied;

			var recognitionDateFilter = filters.AddDateFilter(FilterDescription.RecognitionDate, ViewCommissionLineSchema.VCL_CommissionDate, false);
			recognitionDateFilter.MultilingualDescription = ResString.GetMultilingualString("61d9af4f-a6dc-4238-91eb-19faf26c1440", "Recognition Date");

			var jobNumberFilter = filters.AddTextFilter(FilterDescription.JobNumber, ViewCommissionLineSchema.VCL_JobNumber);
			jobNumberFilter.MaxLength = JobHeaderSchema.JH_JobNum.MaxLength;
			jobNumberFilter.MultilingualDescription = ResString.GetMultilingualString("4d66c36b-e214-4c16-b84e-82209158214d", "Job Number");
		}

		void RemoveAllComparisonOperatorsExceptExactAndNotEquals(ModuleTextFilter textFilter)
		{
			foreach (ICodeDescription comparisonOperator in textFilter.ComparisonOperator_List.ToArray())
			{
				if (comparisonOperator.Code != ModuleTextFilter.ComparisonConstants.Exact && comparisonOperator.Code != ModuleTextFilter.ComparisonConstants.NotEqual)
				{
					textFilter.ComparisonOperator_List.Remove(comparisonOperator);
				}
			}
		}

		ZQuery GetApprovalRequestQuery(SQLComparisonOperator comparisonOperator, object value)
		{
			var query = new ZDBOnlyQuery(typeof(ViewCommissionLine));
			var notIn = comparisonOperator.IsNegativeSQLOperator() || (comparisonOperator == SpecialComparisonOperator.IsBlank);
			var approvalRequestItemSubQuery = new ZDBOnlySubQuery(typeof(AccCommissionApprovalRequestItem), AccCommissionApprovalRequestItemSchema.CRI_CL0, notIn);
			if (comparisonOperator != SpecialComparisonOperator.IsBlank && comparisonOperator != SpecialComparisonOperator.IsNotBlank)
			{
				approvalRequestItemSubQuery.AddToFilter(AccCommissionApprovalRequestItemSchema.CRI_CRQ, comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery(), value);
			}
			query.AddSubQuery(approvalRequestItemSubQuery, JoinCondition.And);

			return query;
		}

		ZQuery GetPaymentStatusQuery(ZString value)
		{
			if (value == AccCommissionLinePaymentStatusList.Codes.Paid)
			{
				return new ZQuery(ViewCommissionLineSchema.VCL_PaidDateTimeUtc, SQLComparisonOperator.NotEqual, null);
			}
			else if (value == AccCommissionLinePaymentStatusList.Codes.Unpaid)
			{
				return new ZQuery(ViewCommissionLineSchema.VCL_PaidDateTimeUtc, null);
			}
			else
			{
				return new ZQuery();
			}
		}

		ZQuery GetCommissionStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();

			if (!comparisonOperator.IsNegativeSQLOperator())
			{
				if (value == AccCommissionLineCommissionStatusList.Codes.Paid)
				{
					query.AddToFilter(ViewCommissionLineSchema.VCL_PaidDateTimeUtc, SQLComparisonOperator.NotEqual, null);
				}
				else if (value == AccCommissionLineCommissionStatusList.Codes.Approved)
				{
					query.AddToFilter(ViewCommissionLineSchema.VCL_PaidDateTimeUtc, SQLComparisonOperator.Equal, null);
					query.AddToFilter(ViewCommissionLineSchema.VCL_ApprovedDateTimeUtc, SQLComparisonOperator.NotEqual, null);
				}
				else if (value == AccCommissionLineCommissionStatusList.Codes.Pending)
				{
					query.AddToFilter(ViewCommissionLineSchema.VCL_PaidDateTimeUtc, SQLComparisonOperator.Equal, null);
					query.AddToFilter(ViewCommissionLineSchema.VCL_ApprovedDateTimeUtc, SQLComparisonOperator.Equal, null);
				}
			}
			else
			{
				if (value == AccCommissionLineCommissionStatusList.Codes.Paid)
				{
					query.AddToFilter(JoinCondition.Or, ViewCommissionLineSchema.VCL_PaidDateTimeUtc, SQLComparisonOperator.Equal, null);
				}
				else if (value == AccCommissionLineCommissionStatusList.Codes.Approved)
				{
					query.AddToFilter(JoinCondition.Or, ViewCommissionLineSchema.VCL_PaidDateTimeUtc, SQLComparisonOperator.NotEqual, null);
					query.AddToFilter(JoinCondition.Or, ViewCommissionLineSchema.VCL_ApprovedDateTimeUtc, SQLComparisonOperator.Equal, null);
				}
				else if (value == AccCommissionLineCommissionStatusList.Codes.Pending)
				{
					query.AddToFilter(JoinCondition.Or, ViewCommissionLineSchema.VCL_PaidDateTimeUtc, SQLComparisonOperator.NotEqual, null);
					query.AddToFilter(JoinCondition.Or, ViewCommissionLineSchema.VCL_ApprovedDateTimeUtc, SQLComparisonOperator.NotEqual, null);
				}
			}

			return query;
		}

		ZQuery GetExcludeCanceledQuery(ZBool value)
		{
			if (value)
			{
				return new ZQuery(ViewCommissionLineSchema.VCL_CancelledDateTimeUtc, null);
			}
			else
			{
				return new ZQuery();
			}
		}

		ZQuery GetExcludeWithheldQuery(ZBool value)
		{
			if (value)
			{
				var result = new ZDBOnlyQuery(typeof(ViewCommissionLine));

				var isPaidQuery = new ZQuery(ViewCommissionLineSchema.VCL_PaidDateTimeUtc, SQLComparisonOperator.NotEqual, null);
				result.AddToFilter(isPaidQuery, JoinCondition.Or);

				var hasNoAgreementQuery = new ZQuery(ViewCommissionLineSchema.VCL_CA0, null);
				result.AddToFilter(hasNoAgreementQuery, JoinCondition.Or);

				var notAnAgreementWithDraftSubquery = new ZDBOnlySubQuery(typeof(OrgCommissionAgreement), ViewCommissionLineSchema.VCL_CA0, true);
				var agreementWithDraftSubquery = new ZDBOnlySubQuery(typeof(OrgCommissionAgreement), OrgCommissionAgreementSchema.CA0_CA0_ParentVersion);
				var agreementInQueueSubQuery = new ZDBOnlySubQuery(typeof(OrgCommissionCalculationQueue), OrgCommissionCalculationQueueSchema.CAQ_CA0);
				agreementWithDraftSubquery.AddAsUnionQuery(agreementInQueueSubQuery, true);
				notAnAgreementWithDraftSubquery.AddSubQuery(agreementWithDraftSubquery, JoinCondition.And);

				result.AddSubQuery(notAnAgreementWithDraftSubquery, JoinCondition.Or);

				return result;
			}

			return new ZQuery();
		}

#if DEBUG
		public
#endif
		class EntityStaffModuleFilter : ModuleNkFilter
		{
			public EntityStaffModuleFilter(ZString description, IBusinessObjectCollection list)
				: base(description, ViewCommissionLineSchema.VCL_GS_NKStaff, ModuleIDs.GlbStaff, list)
			{
				if (!Env.Security.CommissionManagerViewForAnyEntity.IsAllowed && !Env.Security.CommissionManagerViewForAnyEntityInOrganisation.IsAllowed)
				{
					PropertyValidation = CurrentLoginUserOnlyPropertyValidation;
					Visibility = FilterVisibility.AlwaysVisible;
					DefaultProperty = GlbStaff.CurrentUser.GS_Code;
				}
			}

			public override bool HasComparisonOperator
			{
				get { return (Env.Security.CommissionManagerViewForAnyEntity.IsAllowed || Env.Security.CommissionManagerViewForAnyEntityInOrganisation.IsAllowed) && base.HasComparisonOperator; }
			}

			void CurrentLoginUserOnlyPropertyValidation(ZPropertyInfo info)
			{
				var value = (ZString)info.Value;
				if (!value.EqualsIgnoringCase(GlbStaff.CurrentUser.GS_Code))
				{
					info.AddError(GetViewAnyCommissionErrorMessage(false));
				}
				else if (OrCategory != FilterOrCategory.None || GroupOrCategory != FilterOrCategory.None)
				{
					info.AddError(GetViewAnyCommissionErrorMessage(true));
				}
			}

			static string GetViewAnyCommissionErrorMessage(bool isOrFilterError)
			{
				return Res.GetString("9bda2aab-d013-4d85-8aab-2131d92002f7", @"You do not have the appropriate security rights to view commissions for entities other than your current login ({0}).
{1}
If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{2}",
		GlbStaff.CurrentUser.GS_Code,
		isOrFilterError ? Res.GetString("b2d028f5-6195-4266-ad75-a2977d6a1eb7", "As a result, you are not allowed to specify an 'Or' filter category for this filter.") + System.Environment.NewLine : "",
		Env.Security.CommissionManagerViewForAnyEntity.DisplayTextPathToSecurityRight);
			}
		}

#if DEBUG
		public
#endif
		class EntityOrganisationModuleFilter : ModuleGuidFilter
		{
			public EntityOrganisationModuleFilter(ZString description, IBusinessObjectCollection list)
				: base(description, ModuleIDs.Organisation, ViewCommissionLineSchema.VCL_OH_Party, list)
			{
				if (!Env.Security.CommissionManagerViewForAnyEntity.IsAllowed)
				{
					PropertyValidation = CurrentLoginUserOrgProxyOnlyPropertyValidation;
					Visibility = FilterVisibility.AlwaysVisible;
					DefaultProperty = CurrentUserOrgProxyPk;
				}
			}

			public override bool HasComparisonOperator
			{
				get { return Env.Security.CommissionManagerViewForAnyEntity.IsAllowed && base.HasComparisonOperator; }
			}

			void CurrentLoginUserOrgProxyOnlyPropertyValidation(ZPropertyInfo info)
			{
				var value = (ZGuid)info.Value;
				if (value != CurrentUserOrgProxyPk)
				{
					info.AddError(GetViewAnyCommissionErrorMessage(false));
				}
				else if (OrCategory != FilterOrCategory.None || GroupOrCategory != FilterOrCategory.None)
				{
					info.AddError(GetViewAnyCommissionErrorMessage(true));
				}
			}

			static ZGuid CurrentUserOrgProxyPk
			{
				get { return OrgCommissionAgreementRecipient.GetStaffOrgProxyPk(GlbStaff.CurrentUser); }
			}

			static string GetViewAnyCommissionErrorMessage(bool isOrFilterError)
			{
				var factory = GlbStaff.CurrentUser.Factory;
				var orgProxy = factory.Load<OrgHeader>(CurrentUserOrgProxyPk);

				return Res.GetString("bd9afe18-0248-45a8-8b91-18e432fcb832", @"You do not have the appropriate security rights to view commissions for entities outside your current login proxy organization ({0}).
{1}
If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{2}",
		orgProxy != null ? (string)orgProxy.OH_Code : Res.GetString("ed427825-a17e-4f40-8e89-0f5ffc50264c", "None"),
		isOrFilterError ? Res.GetString("a42a4106-3999-46ac-b661-3337def407dd", "As a result, you are not allowed to specify an 'Or' filter category for this filter.") + System.Environment.NewLine : "",
		Env.Security.CommissionManagerViewForAnyEntity.DisplayTextPathToSecurityRight);
			}
		}

		#endregion

		#region Module Filter Descriptions

		public static class FilterDescription
		{
			#region SuppressResourceStringsCheckRegion

			public const string Company = "Company";
			public const string RecognitionDate = "RecognitionDate";
			public const string Customer = "Customer";
			public const string CommissionStream = "CommissionStream";
			public const string AgreementId = "AgreementId";
			public const string Product = "Product";
			public const string Service = "Service";
			public const string SubModule = "SubModule";
			public const string TransactionNumber = "TransactionNumber";
			public const string JobNumber = "JobNumber";
			public const string SnapshotDate = "SnapshotDate";

			public const string EntityStaff = "EntityStaff";
			public const string EntityOrganisation = "EntityOrganisation";
			public const string CommissionType = "CommissionType";
			public const string CommissionStatus = "CommissionStatus";
			public const string PaymentStatus = "PaymentStatus";
			public const string ApprovalRequest = "ApprovalRequest";
			public const string ExcludeCanceled = "ExcludeCanceled";
			public const string ExcludeWithheld = "ExcludeWithheld";

			#endregion
		}

		#endregion

		#endregion

		#region Lookups

		#region Companies

		public GlbCompanyCollection Companies
		{
			get { return companies ?? (companies = new GlbCompanyCollection(Factory)); }
		}
		GlbCompanyCollection companies;

		#endregion

		#region Headers

		OrgHeaderCollection Headers
		{
			get { return headers ?? (headers = new OrgHeaderCollection(Factory)); }
		}
		OrgHeaderCollection headers;

		#endregion

		#region Staff

		public GlbStaffCollection Staff
		{
			get { return staff ?? (staff = new GlbStaffCollection(Factory)); }
		}
		GlbStaffCollection staff;

		#endregion

		#region Products

		public ReadOnlyCodeDescriptionPairList Products
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.Add(OrgCommissionAgreementItemLookups.AllProductsItem);
				result.AddRange(CommissionLookups.New(Factory).AllProducts);
				return result;
			}
		}

		#endregion

		#region Services

		public ReadOnlyCodeDescriptionPairList Services
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.Add(OrgCommissionAgreementItemLookups.AllServicesItem);
				result.AddRange(CommissionLookups.New(Factory).AllServices);
				return result;
			}
		}

		#endregion

		#region SubModules

		public ReadOnlyCodeDescriptionPairList SubModules
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.Add(OrgCommissionAgreementItemLookups.AllSubModulesItem);
				result.AddRange(CommissionLookups.New(Factory).AllSubModules);
				return result;
			}
		}

		#endregion

		#region Commission Approval Requests

		public AccCommissionApprovalRequestCollection CommissionApprovalRequests
		{
			get { return commissionApprovalRequests ?? (commissionApprovalRequests = new AccCommissionApprovalRequestCollection(Factory)); }
		}
		AccCommissionApprovalRequestCollection commissionApprovalRequests;

		#endregion

		#region Commission Streams

		ICodeDescriptionPairList CommissionStreams
		{
			get { return commissionStreams ?? (commissionStreams = CommissionLookups.New(Factory).CommissionStreams_Active); }
		}
		ICodeDescriptionPairList commissionStreams;

		#endregion

		#region Commission Types

		public ReadOnlyCodeDescriptionPairList CommissionTypesIncludingCustom
		{
			get { return new CommissionTypesIncludingCustom(); }
		}

		#endregion

		#region Statuses

		public ReadOnlyCodeDescriptionPairList PaymentStatuses
		{
			get { return new AccCommissionLinePaymentStatusList(); }
		}

		public virtual ReadOnlyCodeDescriptionPairList CommissionStatuses
		{
			get { return new AccCommissionLineCommissionStatusList(); }
		}

		#endregion

		#endregion
	}
}
