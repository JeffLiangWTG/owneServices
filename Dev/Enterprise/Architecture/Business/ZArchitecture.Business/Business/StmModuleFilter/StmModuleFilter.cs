using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	[CodeProperty(StmModuleFilterSchema.Constants.S9_FilterName), DescriptionProperty(StmModuleFilterSchema.Constants.S9_FilterName)]
	public class StmModuleFilter : AutoStmModuleFilter, IGridLayoutStorage, IStmModuleFilter
	{
		public StmModuleFilter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(S9_FilterData), ConcurrencyPolicy.Protect);
		}

		#region Loader

#pragma warning disable IDE0001 // Simplify Names
		public new class Loader : AutoStmModuleFilter.Loader
#pragma warning restore IDE0001 // Simplify Names
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public StmModuleFilter[] FindByID(string id)
			{
				return Factory.Load<StmModuleFilter>(GetQuery(id));
			}

			public StmModuleFilter FindTop1ByID(string id)
			{
				return Factory.LoadTop1<StmModuleFilter>(GetQuery(id));
			}

			public StmModuleFilter[] FindByIDAndName(string id, string layoutName, bool isPublished)
			{
				return Factory.Load<StmModuleFilter>(GetQuery(id, layoutName, isPublished, shouldIgnoreIsPublished: false));
			}

			public StmModuleFilter FindTop1ByIDAndName(string id, string layoutName, bool isPublished)
			{
				return Factory.LoadTop1<StmModuleFilter>(GetQuery(id, layoutName, isPublished, shouldIgnoreIsPublished: false));
			}

			public StmModuleFilter FindTop1ByIDAndName(string id, string layoutName)
			{
				return Factory.LoadTop1<StmModuleFilter>(GetQuery(id, layoutName, isPublished: false, shouldIgnoreIsPublished: true));
			}

			public ZQuery GetQuery(string id)
			{
				return GetQuery(id, new FilterStripLayoutsHelper());
			}

			public ZQuery GetQuery(string id, FilterStripLayoutsHelper layoutHelper)
			{
				if (layoutHelper == null)
				{
					return new ZQuery();
				}

				var query = new ZQuery(StmModuleFilterSchema.S9_ModuleID, id);
				query.AddToFilter(StmModuleFilterSchema.S9_FilterType, SQLComparisonOperator.NotEqual, StmModuleFilterTypes.Codes.FilterRule);

				if (!Globals.IsWeb)
				{
					AddFiltersForNonWeb(query, id, layoutHelper);
				}
				else
				{
					AddFiltersForWeb(query, layoutHelper);
				}

				return query;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
			void AddFiltersForNonWeb(ZQuery query, string id, FilterStripLayoutsHelper layoutHelper)
			{
				ZQuery relatedEntityIDQuery = new ZQuery(StmModuleFilterSchema.S9_RelatedEntityID, layoutHelper.CurrentUserPk);
				if (!layoutHelper.AdditionalEntityID.IsEmpty)
				{
					relatedEntityIDQuery.AddToFilter(JoinCondition.Or, StmModuleFilterSchema.S9_RelatedEntityID, layoutHelper.AdditionalEntityID);
				}
				relatedEntityIDQuery.AddToFilter(JoinCondition.Or, StmModuleFilterSchema.S9_IsPublished, ZBool.True);

				//check for other rows that have RelatedEntityID as our target, and allow their PKs to also be the target
				if (id != null && (id.EndsWith(ModuleIdSuffix.GridColorStrip, StringComparison.Ordinal) || id.EndsWith(ModuleIdSuffix.GridColorScheme, StringComparison.Ordinal)))
				{
					foreach (StmModuleFilter filter in Factory.Load<StmModuleFilter>(new ZQuery(StmModuleFilterSchema.S9_RelatedEntityID, layoutHelper.CurrentUserPk)))
					{
						relatedEntityIDQuery.AddToFilter(JoinCondition.Or, StmModuleFilterSchema.S9_RelatedEntityID, filter.PK);
					}
				}

				query.AddToFilter(relatedEntityIDQuery);

				if (id != null && !id.EndsWith(ModuleIdSuffix.GridColorStrip, StringComparison.Ordinal))
				{
					//colour strips have the company they were made from even if they're for a global colour theme we're trying to access from a different company
					if (EnvProxy.Instance.CurrentCompany != null)
					{
						ZQuery companyQuery = new ZQuery(StmModuleFilterSchema.S9_GC, EnvProxy.Instance.CurrentCompany.PK);
						companyQuery.AddToFilter(JoinCondition.Or, StmModuleFilterSchema.S9_GC, DBNull.Value);
						query.AddToFilter(companyQuery);
					}
				}
			}

			static void AddFiltersForWeb(ZQuery query, FilterStripLayoutsHelper layoutHelper)
			{
				//System Default: (S9_IsSystem = 1) 
				var systemDefaultQuery = new ZQuery(StmModuleFilterSchema.S9_IsSystem, ZBool.True);

				//Company: (S9_IsPublished = 1 and S9_RelatedEntityID is null and S9_GC = GC_PK)
				var companyQueryInitialized = false;
				var companyQuery = new ZDBOnlyQuery(typeof(StmModuleFilter));
				if (EnvProxy.Instance.CurrentCompany != null)
				{
					companyQuery.AddToFilter(StmModuleFilterSchema.S9_IsPublished, ZBool.True);
					companyQuery.AddToFilter(StmModuleFilterSchema.S9_RelatedEntityID, null);
					companyQuery.AddToFilter(StmModuleFilterSchema.S9_GC, EnvProxy.Instance.CurrentCompany.PK);
					companyQueryInitialized = true;
				}

				//Organisation: (S9_IsPublished = 1 and S9_RelatedEntityID = OH_PK)
				var publishedQuery = new ZDBOnlyQuery(typeof(StmModuleFilter));
				publishedQuery.AddToFilter(StmModuleFilterSchema.S9_IsPublished, ZBool.True);
				publishedQuery.AddToFilter(StmModuleFilterSchema.S9_RelatedEntityID, layoutHelper.CurrentOrganisationPk);

				var nonPublishedQuery = new ZQuery(StmModuleFilterSchema.S9_IsPublished, ZBool.False);
				nonPublishedQuery.AddToFilter(StmModuleFilterSchema.S9_RelatedEntityID, layoutHelper.CurrentUserPk);

				var filterQuery = new ZQuery();
				filterQuery.AddToFilter(systemDefaultQuery);
				filterQuery.AddToFilter(nonPublishedQuery, JoinCondition.Or);
				if (companyQueryInitialized)
				{
					filterQuery.AddToFilter(companyQuery, JoinCondition.Or);
				}
				filterQuery.AddToFilter(publishedQuery, JoinCondition.Or);

				query.AddToFilter(filterQuery);
			}

			ZQuery GetQuery(string id, string layoutName, bool isPublished, bool shouldIgnoreIsPublished)
			{
				ZQuery result = GetQuery(id);
				result.AddToFilter(StmModuleFilterSchema.S9_FilterName, layoutName);

				if (!shouldIgnoreIsPublished)
				{
					result.AddToFilter(StmModuleFilterSchema.S9_IsPublished, isPublished);
				}

				return result;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(StmModuleFilter);
			}
		}

		#endregion

		public static readonly StmModuleFilterTypeDecider TypeDecider = new StmModuleFilterTypeDecider();

		public static class ModuleIdSuffix
		{
			public const string GridColorStrip = "_CT";
			public const string GridColorScheme = "_CS";
			public const string UniversalCopyTemplate = "_UC";
		}

		#region for WebTracker
		//This is used by WebTracker only
		public ZBool IsPublishedForCompanyInWeb
		{
			get
			{
				if (!Globals.IsWeb)
				{
					throw new NotSupportedException("This function is for WebTracker only.");
				}

				return S9_IsPublished && S9_RelatedEntityID.IsEmpty;
			}
		}

		public void SetIsPublishedForCompanyInWeb(ZBool isPublishedForCompany, ZGuid companyPK)
		{
			if (!Globals.IsWeb)
			{
				throw new NotSupportedException("This function is for WebTracker only.");
			}

			S9_IsPublished = isPublishedForCompany;
			if (isPublishedForCompany)
			{
				S9_GC = companyPK;
			}
		}

		//This is used by WebTracker only
		public ZBool IsPublishedForOrganisationInWeb
		{
			get
			{
				if (!Globals.IsWeb)
				{
					throw new NotSupportedException("This function is for WebTracker only.");
				}

				return S9_IsPublished && !S9_RelatedEntityID.IsEmpty;
			}
		}

		public void SetIsPublishedForOrganisationInWeb(ZBool isPublishedForOrganisation, ZGuid organisationPK)
		{
			if (!Globals.IsWeb)
			{
				throw new NotSupportedException("This function is for WebTracker only.");
			}

			S9_IsPublished = isPublishedForOrganisation;
			if (isPublishedForOrganisation)
			{
				S9_RelatedEntityID = organisationPK;
			}
		}

		#endregion

		#region GetOrCreateLayoutUserData

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is an exception text")]
		public StmModuleFilterUserData GetOrCreateLayoutUserData(FilterStripLayoutsHelper layoutsHelper)
		{
			EnsureNotNull(layoutsHelper, "Cannot call GetOrCreateLayoutUserData() with a null layouts helper.");

			StmModuleFilterUserData result = GetLayoutUserData(layoutsHelper);

			if (result == null)
			{
				result = Factory.New<StmModuleFilterUserData>();
				result.Initialise(layoutsHelper);
				result.S0_S9 = PK;
			}

			return result;
		}

		public StmModuleFilterUserData GetLayoutUserData(FilterStripLayoutsHelper layoutsHelper)
		{
			var query = new ZQuery { FetchOnlyFromLocalCache = !IsInDatabase };
			if (!layoutsHelper.AdditionalEntityID.IsEmpty)
			{
				query.ReLoadExistingRows = true;
			}
			query.AddToFilter(StmModuleFilterUserDataSchema.S0_S9, PK);
			if (!S9_IsPublished)
			{
				query.AddToFilter(StmModuleFilterUserDataSchema.S0_RelatedEntityID, layoutsHelper.CurrentUserPk);
				query.AddToFilter(StmModuleFilterUserDataSchema.S0_RelatedEntityTableCode, layoutsHelper.CurrentUserTablePrefix);
			}

			return Factory.LoadTop1<StmModuleFilterUserData>(query);
		}

		#endregion

		#region Bracketed/Unbracketed Layout Name

		public ZString DisplayName => GetDisplayName(S9_FilterNameMultilingual, IsUserDefinedFilter);

		public static ZString GetDisplayName(ZString layoutName, bool isUserDefinedFilter, bool isAction = true)
		{
			if (!layoutName.IsEmpty && isUserDefinedFilter && isAction)
			{
				layoutName = string.Format(CultureInfo.InvariantCulture, "{0} [+]", layoutName);
			}

			return layoutName;
		}

		#endregion

		#region S9_IsPublished

		[BusinessObjectTestExclude] // throws exception when attempting to unpublish the filter layout
		public override ZBool S9_IsPublished
		{
			get { return base.S9_IsPublished; }
			set
			{
				if (value & !S9_ModuleID.EndsWith(ModuleIdSuffix.UniversalCopyTemplate))
				{
					S9_RelatedEntityID = ZGuid.Empty;
				}
				else if (S9_IsPublished && !(S9_ModuleID.EndsWith(ModuleIdSuffix.GridColorStrip) || S9_ModuleID.EndsWith(ModuleIdSuffix.GridColorScheme) || S9_ModuleID.EndsWith(ModuleIdSuffix.UniversalCopyTemplate)))
				{
					throw new NotSupportedException("A published filter layout cannot be made unpublished.");
				}

				base.S9_IsPublished = value;
			}
		}

		#endregion

		#region S9_IsSystem

		public override ZBool S9_IsSystem
		{
			get { return base.S9_IsSystem; }
			set
			{
				base.S9_IsSystem = value;

				if (S9_IsSystem)
				{
					S9_GC = ZGuid.Empty;
					S9_IsPublished = true;
				}
			}
		}

		#endregion

		#region S9_FilterName

		[ResourceStringData("446C4CE0-684B-471C-90DC-0B1C31A943D5", Caption = "Filter Name", FullDescription = "Filter Name of the Find Filter")]
		[StmModuleFilterTranslatableDataField(
			Schema.TableName,
			Schema.S9_FilterName,
			@"Database\Odyssey\Data\Public\StmModuleFilter\StmModuleFilter.xml;Database\Odyssey\Data\Public\TagRule\TagRule.xml",
			Schema.S9_ModuleID,
			MaxLength = Schema.S9_FilterNameMaxLength,
			Type = typeof(StmModuleFilter),
			Asmid = ResString.AssemblyId)]
		public override ZString S9_FilterName
		{
			get { return base.S9_FilterName; }
			set { base.S9_FilterName = value; }
		}

		[ResourceStringData("9C26A92B-25CE-4214-B045-4A961AC93AAF", Caption = "Filter Name", FullDescription = "Multilingual Filter Name of the Find Filter")]
		public MultilingualString S9_FilterNameMultilingual
		{
			get { return GetMultilingual(S9_FilterNameInfo); }
		}
		#endregion

		#region S9_FilterType

		[ResourceStringData("d6c62b9f-b6ca-4883-b712-f4c65a3734ac", Caption = "Filter Type")]
		[List("Lookups.FilterTypes")]
		public override ZString S9_FilterType
		{
			get { return base.S9_FilterType; }
			set { base.S9_FilterType = value; }
		}

		public bool IsUserDefinedFilter => S9_FilterType == StmModuleFilterTypes.Codes.UserDefined;

		#endregion

		#region Overriden methods

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			S9_GC = EnvProxy.Instance.CurrentCompany?.PK ?? ZGuid.Empty;
			S9_RelatedEntityID = EnvProxy.Instance.CurrentUser?.PK ?? ZGuid.Empty;
		}

		public override void OnSaving()
		{
			base.OnSaving();

			wasInDatabaseOnSaving = IsInDatabase;
			oldFilterName = (ZString)S9_FilterNameInfo.OriginalValue;
			oldSaveColumnLayout = (ZBool)S9_SaveColumnLayoutInfo.OriginalValue;
		}

		bool wasInDatabaseOnSaving;
		ZString oldFilterName;
		ZBool oldSaveColumnLayout;

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded && wasInDatabaseOnSaving)
			{
				if (oldFilterName != S9_FilterNameMultilingual.GetUnresolvedString() || oldSaveColumnLayout != S9_SaveColumnLayout)
				{
					GridModuleFilterSavedLayoutUpdatedEvent.OnLayoutUpdated(Factory, this, oldSaveColumnLayout);
				}
			}
		}

		protected override void OnDeletedByDataRefresh()
		{
			GridModuleFilterLayoutDeletedEvent.OnLayoutDeleted(Factory, PK);
			base.OnDeletedByDataRefresh();
		}

		public override void Delete()
		{
			GridModuleFilterLayoutDeletedEvent.OnLayoutDeleted(Factory, PK);
			base.Delete();
		}

		public void DeleteWithUserData()
		{
			var layoutUserData = GetLayoutUserData(new EmptyLayoutsHelper());

			if (layoutUserData != null)
			{
				layoutUserData.Delete();
			}

			Delete();
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clone = (StmModuleFilter)base.CloneInternal(args);
			RelatedModuleFiltersHelper.CopyFilterStrips(this, clone);
			return clone;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var query1 = new ZQuery(StmModuleFilterSchema.S9_RelatedEntityID, PK);
				var filters = Factory.Load<StmModuleFilter>(query1).OfType<BusinessObject>().ToList();

				if (filters.IsNullOrEmpty())
				{
					return Array.Empty<BusinessObject>();
				}

				var query2 = new ZQuery();
				foreach (var filter in filters)
				{
					query2.AddToFilter(new ZQuery(StmModuleFilterUserDataSchema.S0_S9, filter.PK), JoinCondition.Or);
				}

				var filtersUserData = Factory.Load<StmModuleFilterUserData>(query2).OfType<BusinessObject>().ToList();
				filters.RemoveAll(f => filtersUserData.Select(fud => ((StmModuleFilterUserData)fud).S0_S9).Contains(f.PK));
				return filtersUserData.IsNullOrEmpty() ? filters.ToArray() : filters.Concat(filtersUserData).ToArray();
			}
		}

		#region Unique index handling

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new UniqueValueOnParentUniqueIndexFailureHandler(this); }
		}

		class UniqueValueOnParentUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public UniqueValueOnParentUniqueIndexFailureHandler(StmModuleFilter moduleFilter)
			{
				this.moduleFilter = moduleFilter;
			}

			readonly StmModuleFilter moduleFilter;

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				notifier.ReportError(
					Res.GetString("9ecafbc5-53c9-49fa-a4c9-7684303dea08", "A layout or scheme with this name '{0}' already exists in database. Please use a different name.", moduleFilter.S9_FilterName),
					Res.GetString("5685619d-5aa1-4d70-8e6d-62680581f883", "Save Error")
				);
			}

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get { yield return StmModuleFilterSchema.Constants.Indexes.NR_UX__S9_ModuleID_S9_FilterName_S9_GC_S9_RelatedEntityID; }
			}
		}

		#endregion

		#endregion

		public static bool AreFiltersDifferent(StmModuleFilter filter1, StmModuleFilter filter2, FilterStripLayoutsHelper layoutHelper)
		{
			return !filter1.S9_FilterData.Equals(filter2.S9_FilterData) ||
				!filter1.GetLayoutUserData(layoutHelper).S0_FilterDataValues.Equals(filter2.GetLayoutUserData(layoutHelper).S0_FilterDataValues);
		}

		#region Lookups

		public StmModuleFilterLookups Lookups => lookups ?? (lookups = new StmModuleFilterLookups(this));
		StmModuleFilterLookups lookups;

		#endregion

		#region Filter Logs

		public IGlbStaff CreatorOrEarlyestUser
		{
			get
			{
				if (creatorOrEarlyestUser == null)
				{
					if (!IsInDatabase)
					{
						creatorOrEarlyestUser = Logs.AddedLog.User;
					}
					else
					{
						var zQuery = new ZQuery(StmALogSchema.SL_Parent, PK);
						zQuery.AddToFilter(StmALogSchema.SL_Table, StmModuleFilterSchema.Constants.TableName);
						zQuery.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Ascending;
						var log = Factory.LoadTop1<StmALog>(zQuery);
						creatorOrEarlyestUser = log?.User;
					}
				}
				return creatorOrEarlyestUser;
			}
		}
		IGlbStaff creatorOrEarlyestUser;

		protected override EnterpriseBusinessObject.AutologState AutoLoggingState => EnterpriseBusinessObject.AutologState.AutoLogged;

		protected override ZString CustomLogReferenceSuffix => Res.GetString("24F573B7-25E1-4871-8355-8C99FFA1A207", "Layout Name: '{0}'", S9_FilterName);

		#endregion

		void EnsureNotNull(object value, string errorMessage)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value), errorMessage);
			}
		}

		#region IGridLayoutStorage Members

		string IGridLayoutStorage.GridLayoutKey
		{
			get
			{
				string result = S9_ModuleID;

				//for module grids, module id alone is not enough for grid layout
				if (S9_FilterData.Length > 0)
				{
					result += DisplayName;
				}
				return result;
			}
		}

		byte[] IGridLayoutStorage.ColumnLayoutData
		{
			get { return S9_ColumnLayoutData; }
		}

		string IGridLayoutStorage.ColumnLayoutName
		{
			get { return S9_FilterNameMultilingual.GetUnresolvedString(); }
			set { S9_FilterName = value; }
		}

		string IGridLayoutStorage.ColumnLayoutDisplayName => S9_FilterNameMultilingual;

		bool IGridLayoutStorage.IsRenameAllowed
		{
			get { return true; }
		}

		bool IGridLayoutStorage.SaveColumnLayout
		{
			get { return S9_SaveColumnLayout; }
			set { S9_SaveColumnLayout = value; }
		}

		bool IGridLayoutStorage.SaveGridColourLayout
		{
			get { return S9_SaveGridColourLayout; }
			set { S9_SaveGridColourLayout = value; }
		}

		ZGuid IGridLayoutStorage.GridColourLayoutID
		{
			get { return S9_GridColourLayoutID; }
			set { S9_GridColourLayoutID = value; }
		}

		bool IGridLayoutStorage.IsSystemDefined
		{
			get { return S9_IsSystem; }
		}

		bool IGridLayoutStorage.IsPublished
		{
			get { return S9_IsPublished; }
		}

		bool IGridLayoutStorage.IsPublishedAcrossAllCompanies
		{
			get { return ((IGridLayoutStorage)this).IsPublished && S9_GC.IsEmpty; }
		}

		bool IGridLayoutStorage.IsDeleteAllowed
		{
			get { return true; }
		}

		#endregion

		#region Module Filter Comparison

		public ModuleFilterComparisonStrategy ModuleFilterComparisonStrategy { get; set; } = new ModuleFilterComparisonStrategy();

		#endregion
	}
}
