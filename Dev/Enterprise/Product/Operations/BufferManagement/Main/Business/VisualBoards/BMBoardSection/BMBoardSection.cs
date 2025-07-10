using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[XmlSerializerAssembly("Enterprise.BufferManagement.Business.XmlSerializers")]
	public class BMBoardSection : AutoBMBoardSection,
		IBMBoardSection,
		ICustomisedLayoutSupportable,
		IFilterPreviewable,
		IBranchDepartmentProvider,
		IRelatedModuleFilterSupportable,
		IAuditParent
	{
		public static string DefaultForegroundColor => Color.Black.Name;

		public BMBoardSection(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region BusinessObject overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			RowSpan = 1;
			ColSpan = 1;
			ForegroundColor = DefaultForegroundColor;
		}

		public override void Delete()
		{
			Configuration?.Delete();

			var board = Board;

			if (board != null)
			{
				board.MB_SystemLastEditTimeUtc = ZDateTime.UtcNow;
			}

			base.Delete();
		}

		internal void DeleteFilters()
		{
			WorkflowFilterProvider.DeleteFilter();
			TaskFilterProvider.DeleteFilter();
		}

		public override void OnSaving()
		{
			base.OnSaving();

			EnsureRelatedEntitiesAreUpdated();
		}

		void EnsureRelatedEntitiesAreUpdated()
		{
			((BusinessObject)Configuration)?.OnSaving();

			if (Board != null)
			{
				Board.MB_SystemLastEditTimeUtc = ZDateTime.UtcNow;
			}
		}

		internal void UpdateConfigurationForFilters()
		{
			UpdateConfigurationForFilters(WorkflowFilterProvider);
			UpdateConfigurationForFilters(TaskFilterProvider);
		}

		void UpdateConfigurationForFilters(FilterRuleProvider provider)
		{
			if (provider.IsFilterCached)
			{
				var filter = provider.GetOrCreateAndCacheFilter();

				if (filter.HasChanges || provider.HasFilterStrips)
				{
					HasChanges = true;
				}
			}
		}

		protected override IEnumerable<XmlColumnSpecification> XmlSerialisedColumns
		{
			get { yield return new XmlColumnSpecification(BMBoardSectionSchema.MS_LayoutData); }
		}

		protected override ZString HumanReadableNameCore => Res.GetString("fa77e997-6696-4ed9-8b2a-c26219d1500b", "Visual Board: {0}, {1}", Board.MB_Name, SectionName);

		#endregion

		#region Properties

		#region MS_FC_Component

		[RelatedBusinessObject("Component")]
		[List("Lookups.Components")]
		public override ZGuid MS_FC_Component
		{
			get { return base.MS_FC_Component; }
			set
			{
				var previousComponent = Component;
				base.MS_FC_Component = value;
				var newComponent = Component;

				var isBuffer = newComponent != null && newComponent.IsBuffer;
				var didComponentTypeChange = previousComponent == null || newComponent == null || previousComponent.FC_Type != newComponent.FC_Type;

				if (SectionConfiguration != null)
				{
					if (didComponentTypeChange)
					{
						if (isBuffer)
						{
							SectionConfiguration.SetDefaultValues_ForBuffer();
						}
						else
						{
							SectionConfiguration.SetDefaultValues_ForBucket();
						}
					}

					if (isBuffer && !IsValidationSuspended)
					{
						SectionConfiguration.Validation.ValidateCardType();
						SectionConfiguration.Validation.ValidateIsReleaseScheduler();
					}

					SectionConfiguration.DefaultSectionNameInfo.RefreshBinding();
				}
			}
		}

		public BMComponent Component
		{
			get
			{
				if (component == null || component.IsDeleted || component.PK != MS_FC_Component)
				{
					component = Factory.Load<BMComponent>(MS_FC_Component);
				}
				return component;
			}
		}
		BMComponent component;

		#endregion

		#region MS_MB_Board

		[RelatedBusinessObject("Board")]
		[List("Lookups.Boards")]
		public override ZGuid MS_MB_Board
		{
			get { return base.MS_MB_Board; }
			set { base.MS_MB_Board = value; }
		}

		public virtual BMBoard Board
		{
			get { return Factory.Load<BMBoard>(MS_MB_Board); }
		}

		#endregion

		#region MS_SectionType

		[List("Lookups.SectionTypes")]
		public override ZString MS_SectionType
		{
			get { return base.MS_SectionType; }
			set
			{
				if (base.MS_SectionType != value)
				{
					if (configuration != null)
					{
						Configuration.Delete();
					}
					configuration = null;
					base.MS_SectionType = value;
				}
			}
		}

		#endregion

		#region WorkflowFilter

		public StmModuleFilter WorkflowFilter => WorkflowFilterProvider.GetOrCreateAndCacheFilter();

		FilterRuleProvider WorkflowFilterProvider => workflowFilterProvider ?? (workflowFilterProvider = new BMFilterRuleProvider(this, "WFL") { ReloadExistingRowsOnFilterLoad = true });
		FilterRuleProvider workflowFilterProvider;

		public ZQuery WorkflowSectionFilter => SectionConfiguration.GetQuery(ModuleIDs.ProcessHeader, SectionConfiguration.WorkflowFilter);

		public bool AreWorkflowFiltersSpecified
		{
			get
			{
				using (SectionConfiguration.SetTempContext(BufferManagementBusinessContext.IgnoreDefaultFilters))
				{
					var filterBusinessObject = RelatedModuleFiltersHelper.GetNewFilterBusinessObject(ModuleIDs.ProcessHeader);
					var stripCount = filterBusinessObject.GetFilterStripsCount(WorkflowFilter);

					return stripCount > 0;
				}
			}
		}

		#endregion

		#region TaskFilter

		public StmModuleFilter TaskFilter => TaskFilterProvider.GetOrCreateAndCacheFilter();

		FilterRuleProvider TaskFilterProvider => taskFilterProvider ?? (taskFilterProvider = new BMFilterRuleProvider(this, filterName: "TSK", moduleID: ModuleIDs.ProcessTasks) { ReloadExistingRowsOnFilterLoad = true });
		FilterRuleProvider taskFilterProvider;

		public ZQuery TaskSectionFilter => SectionConfiguration.GetQuery(ModuleIDs.ProcessTasks, SectionConfiguration.TaskFilter);

		#endregion

		#region DisplaySequence

		[ResourceStringData("BMBoardSection.DisplaySequence", Caption = "Display Sequence", ShortCaption = "Sequence")]
		public ZInt DisplaySequence
		{
			get
			{
				if (displaySequence == null && Board != null)
				{
					var sections = Factory.Load<BMBoardSection>(new ZQuery(BMBoardSectionSchema.MS_MB_Board, MS_MB_Board));
					var sequence = sections.OrderBy(s => s.Row).ThenBy(s => s.Column).ToList();
					displaySequence = sequence.IndexOf(this) + 1;
				}
				return displaySequence ?? 0;
			}
		}
		ZInt? displaySequence;

		void ResetDisplaySequence()
		{
			displaySequence = null;
			DisplaySequenceInfo.RefreshBinding();
		}

		public ZPropertyInfo DisplaySequenceInfo
		{
			get { return GetZPropertyInfo(nameof(DisplaySequence)); }
		}

		#endregion

		#region AllShownComponentPKs

		public ICollection<ZGuid> AllShownComponentPKs => ApplicableComponents.Select(c => c.PK).ToArray();

		#endregion

		#endregion

		#region XML Properties

		#region Configuration

		[XmlColumnProperty]
		public IBoardSectionConfigurationBizo Configuration
		{
			get
			{
				if (configuration != null)
				{
					return configuration;
				}

				var descriptor = SectionDescriptorProvider.Get(this.MS_SectionType);

				if (descriptor != null)
				{
					Factory.AddFetchHint(BMBoardSectionAdditionalComponentSchema.Instance, new ZQuery(BMBoardSectionAdditionalComponentSchema.BSA_MS_Section, PK));

					configuration = descriptor.GetSectionConfigurationBizo(this);
					configuration.SectionNameInfo.ValueChanged += (object sender, EventArgs e) =>
					{
						SectionNameInfo.RefreshBinding();
					};
				}
				else
				{
					configuration = new UnknownBoardSectionConfiguration(Factory);
				}

				RegisterEditableChildObject(configuration);
				return configuration;
			}
		}
		IBoardSectionConfigurationBizo configuration;

		public BMComponentSectionConfiguration SectionConfiguration
		{
			get { return Configuration as BMComponentSectionConfiguration; }
		}

		public IEnumerable<ZGuid> CapabilityChannelEntityPKs => SectionConfiguration.Channels
					.Where(channel => channel.MSC_ChannelType == ChannelTypeList.Codes.Capability)
					.Select(channel => channel.EntityPK);

		#endregion

		#region Row

		[XmlColumnProperty]
		[ResourceStringData("BMBoardSection.Row", Caption = "Row")]
		public ZInt Row
		{
			get { return GetXmlColumnPropertyValue<ZInt>(RowInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(Row, value))
				{
					SetXmlColumnPropertyValue(RowInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateRow();
					}
					if (Board != null)
					{
						foreach (var section in Board.Sections)
						{
							section.ResetDisplaySequence();
						}
					}
				}
			}
		}

		public ZPropertyInfo RowInfo
		{
			get { return GetZPropertyInfo(nameof(Row)); }
		}

		#endregion

		#region Column

		[XmlColumnProperty]
		[ResourceStringData("BMBoardSection.Column", Caption = "Column")]
		public ZInt Column
		{
			get { return GetXmlColumnPropertyValue<ZInt>(ColumnInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(Column, value))
				{
					SetXmlColumnPropertyValue(ColumnInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateColumn();
					}
					if (Board != null)
					{
						foreach (var section in Board.Sections)
						{
							section.ResetDisplaySequence();
						}
					}
				}
			}
		}

		public ZPropertyInfo ColumnInfo
		{
			get { return GetZPropertyInfo(nameof(Column)); }
		}

		#endregion

		#region RowSpan

		[XmlColumnProperty]
		[ResourceStringData("BMBoardSection.RowSpan", Caption = "Row Span")]
		public ZInt RowSpan
		{
			get { return GetXmlColumnPropertyValue<ZInt>(RowSpanInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(RowSpan, value))
				{
					SetXmlColumnPropertyValue(RowSpanInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateRowSpan();
					}
				}
			}
		}

		public ZPropertyInfo RowSpanInfo
		{
			get { return GetZPropertyInfo(nameof(RowSpan)); }
		}

		#endregion

		#region ColSpan

		[XmlColumnProperty]
		[ResourceStringData("BMBoardSection.ColSpan", Caption = "Column Span")]
		public ZInt ColSpan
		{
			get { return GetXmlColumnPropertyValue<ZInt>(ColSpanInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(ColSpan, value))
				{
					SetXmlColumnPropertyValue(ColSpanInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateColSpan();
					}
				}
			}
		}

		public ZPropertyInfo ColSpanInfo
		{
			get { return GetZPropertyInfo(nameof(ColSpan)); }
		}

		#endregion

		#region RowHeightPercent

		[XmlColumnProperty(DefaultValue = 100)]
		[ResourceStringData("BMBoardSection.RowHeightPercent", Caption = "Row Height Percent", ShortCaption = "Row Height %")]
		public ZInt RowHeightPercent
		{
			get { return GetXmlColumnPropertyValue<ZInt>(RowHeightPercentInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(RowHeightPercent, value))
				{
					SetXmlColumnPropertyValue(RowHeightPercentInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateRowHeightPercent();
					}
				}
			}
		}

		public ZPropertyInfo RowHeightPercentInfo
		{
			get { return GetZPropertyInfo(nameof(RowHeightPercent)); }
		}

		#endregion

		#region ColWidthPercent

		[XmlColumnProperty(DefaultValue = 100)]
		[ResourceStringData("BMBoardSection.ColWidthPercent", Caption = "Column Width Percent", ShortCaption = "Column Width %")]
		public ZInt ColWidthPercent
		{
			get { return GetXmlColumnPropertyValue<ZInt>(ColWidthPercentInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(ColWidthPercent, value))
				{
					SetXmlColumnPropertyValue(ColWidthPercentInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateColWidthPercent();
					}
				}
			}
		}

		public ZPropertyInfo ColWidthPercentInfo
		{
			get { return GetZPropertyInfo(nameof(ColWidthPercent)); }
		}

		#endregion

		#region Colours

		#region BackgroundColor

		[XmlColumnProperty]
		[List("Lookups.ColorList")]
		[ResourceStringData("BMBoardSection.BackgroundColor", Caption = "Background Color", ShortCaption = "Background")]
		[MaxLength(50)]
		public ZString BackgroundColor
		{
			get { return GetXmlColumnPropertyValue<ZString>(BackgroundColorInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(BackgroundColor, value))
				{
					SetXmlColumnPropertyValue(BackgroundColorInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateBackgroundColor();
					}
				}
			}
		}

		public ZPropertyInfo BackgroundColorInfo
		{
			get { return GetZPropertyInfo(nameof(BackgroundColor)); }
		}

		public Color BackgroundColorValue
		{
			get
			{
				return !BackgroundColor.IsEmpty ? ColorList.ColorFromName(BackgroundColor) : BMConstants.BackgroundDefaultColor;
			}
		}

		#endregion

		#region ForegroundColor

		[XmlColumnProperty]
		[List("Lookups.ColorList")]
		[ResourceStringData("BMBoardSection.ForegroundColor", Caption = "Foreground Color", ShortCaption = "Foreground")]
		[MaxLength(50)]
		public ZString ForegroundColor
		{
			get { return GetXmlColumnPropertyValue<ZString>(ForegroundColorInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(ForegroundColor, value))
				{
					SetXmlColumnPropertyValue(ForegroundColorInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateForegroundColor();
					}
				}
			}
		}

		public ZPropertyInfo ForegroundColorInfo
		{
			get { return GetZPropertyInfo(nameof(ForegroundColor)); }
		}

		[BusinessObjectTestExclude] // We want to return null if no color is specified
		public Color? ForegroundColorValue
		{
			get { return string.IsNullOrEmpty(ForegroundColor) ? null : new Color?(ColorList.ColorFromName(ForegroundColor)); }
		}

		#endregion

		#endregion

		#endregion

		#region Related Business Objects

		public override GlbGroup ReleaseGroup
		{
			get { return Factory.Load<GlbGroup>(SectionConfiguration.ApplicableReleaseGroupPK); }
		}

		public IEnumerable<BMComponent> ApplicableComponents
		{
			get
			{
				return SectionConfiguration != null && SectionConfiguration.IsReleaseScheduler
						? ReleaseSchedulerComponents
						: AllComponents;
			}
		}

		public IEnumerable<BMComponent> AllComponents
		{
			get
			{
				Factory.AddFetchHint(BMComponentSchema.Instance, new ZQuery(BMComponentSchema.PK, MS_FC_Component));
				var additionalComponents = new List<BMComponent>(AdditionalActiveComponents);

				return Component.WrapWithEnumerable().Concat(additionalComponents).WhereNotNull();
			}
		}

		public IEnumerable<BMComponent> AdditionalActiveComponents
		{
			get
			{
				if (SectionConfiguration == null)
				{
					return Enumerable.Empty<BMComponent>();
				}

				Factory.AddFetchHint(BMBoardSectionChannelSchema.Instance, new ZQuery(BMBoardSectionChannelSchema.MSC_MS_Section, PK));

				var activeRelationships = new List<ComponentRelationship>();
				var activeComponents = new HashSet<BMComponent>();
				var additionalComponents = SectionConfiguration.AdditionalComponents;

				foreach (var additionalComponent in additionalComponents)
				{
					Factory.AddFetchHint(BMComponentSchema.Instance, new ZQuery(BMComponentSchema.PK, additionalComponent.BSA_FC_Component));
				}

				foreach (var component in additionalComponents
					.Select(x => x.Component)
					.WhereNotNull()
					.Where(c => c.FC_IsActive))
				{
					if (component is ComponentRelationship relationship)
					{
						activeRelationships.Add(relationship);
					}
					else
					{
						activeComponents.Add(component);
					}
				}

				ComponentRelationship.AddComponentFetchHints(activeRelationships);

				var componentsFromRelationships = activeRelationships
					.SelectMany(relationship => relationship.RelatedComponentLinks)
					.Where(link => link.FL_FC_ComponentTo != MS_FC_Component)
					.Select(link => link.ComponentTo)
					.WhereNotNull();

				activeComponents.AddRange(componentsFromRelationships);

				return activeComponents.OrderBy(component => component.FC_Name);
			}
		}

		public IEnumerable<BMComponent> ReleaseSchedulerComponents
		{
			get { return Component?.GetFeedingComponents().Append(Component) ?? Enumerable.Empty<BMComponent>(); }
		}

		public IEnumerable<GlbStaff> AllResources
		{
			get
			{
				if (SectionConfiguration == null)
				{
					return null;
				}

				var resourcePKs = SectionConfiguration.Channels
					.Where(channel => channel.MSC_ParentID.IsValid && channel.MSC_ChannelType == ChannelTypeList.Codes.Resource)
					.Select(channel => channel.MSC_ParentID);

				return Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.PK, resourcePKs));
			}
		}

		#endregion

		#region LayoutConfigUpdated

		public event EventHandler LayoutConfigUpdated;

		IDisposable FireLayoutConfigUpdatedOnDisposeIfValueChanged<T>(T oldValue, T newValue)
		{
			if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
			{
				return DisposableAction.NoAction;
			}
			else
			{
				return new DisposableAction(() =>
				{
					OnLayoutConfigUpdated();
				});
			}
		}

		public void OnLayoutConfigUpdated()
		{
			if (LayoutConfigUpdated != null)
			{
				LayoutConfigUpdated(this, EventArgs.Empty);
			}
		}

		#endregion

		#region Customised Layouts

		public Dictionary<string, BMControlCustomisation> GetSummaryCardCustomisedLayouts()
		{
			var links = BMControlCustomisation.GetAllPossibleCustomisedSummaryCardLinks(SectionConfiguration);
			var controlType = SectionConfiguration.ShowWorkflowOrJobWorkflowCards ? CustomisedControlTypeList.Codes.WorkflowSummaryCard : CustomisedControlTypeList.Codes.TaskCard;

			return GetCustomisedLayouts(links, controlType, Factory);
		}

		public Dictionary<string, BMControlCustomisation> GetDetailedCardCustomisedLayouts()
		{
			var links = BMControlCustomisation.GetAllPossibleCustomisedDetailedCardLinks(SectionConfiguration);
			var controlType = SectionConfiguration.ShowWorkflowOrJobWorkflowCards ? CustomisedControlTypeList.Codes.WorkflowDetailedCard : CustomisedControlTypeList.Codes.DetailedCard;

			return GetCustomisedLayouts(links, controlType, Factory);
		}

		static Dictionary<string, BMControlCustomisation> GetCustomisedLayouts(IEnumerable<BMControlCustomisationLink> links, string controlType, BusinessObjectFactory factory)
		{
			var result = (
				from link in links
				let x = new { Key = link.FML_JobType.ToString(), Layout = link.CustomisedLayout }
				where x.Layout != null
				select x
				).ToDictionary(x => x.Key, x => x.Layout);

			if (!result.ContainsKey(string.Empty))
			{
				var layout = factory.GetCachedValue(string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", nameof(BMBoardSectionViewModel), nameof(GetCustomisedLayouts), controlType), () => BMControlCustomisation.GetNewDefaultCardLayout(factory, controlType, allowSavingByFactory: false));

				result.Add(string.Empty, layout);
			}

			return result;
		}

		#endregion

		#region IBMBoardSection Members

		IBMBoard IBMBoardSection.Board
		{
			get { return Board; }
		}

		IBMComponent IBMBoardSection.Component
		{
			get { return Component; }
		}

		IEnumerable<IBMComponent> IBMBoardSection.AllComponents
		{
			get { return AllComponents; }
		}

		[ResourceStringData("BMBoardSectionConfiguration.SectionName", Caption = "Section Name", ShortCaption = "Name", FullDescription = "This name will appear on section headings. It can be overridden on Component and Module sections.")]
		public ZString SectionName
		{
			get { return Configuration != null ? Configuration.SectionName : ZString.Empty; }
		}

		public ZPropertyInfo SectionNameInfo
		{
			get { return GetZPropertyInfo(nameof(SectionName)); }
		}

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clone = (BMBoardSection)base.CloneInternal(args);

			this.CloneCustomisedLayoutLayoutLinks(clone);

			if (Configuration != null)
			{
				Configuration.CopyConfigurationPropertiesToNewSection(clone);
			}

			return clone;
		}

		internal void CopyFilters(BMBoardSection newSection)
		{
			WorkflowFilterProvider.CopyFilterStrips(() => newSection.WorkflowFilter);
			TaskFilterProvider.CopyFilterStrips(() => newSection.TaskFilter);
		}

		#endregion

		#region Constraint

		public bool IsInConstrainedMode
		{
			get
			{
				bool IsInConstrainedMode() => Component != null && ConstrainedModeHelper.IsInConstrainedMode(Component, SectionConfiguration.ApplicableReleaseGroupPK);
				if (!IsInDatabase || HasChanges)
				{
					return IsInConstrainedMode();
				}
				else
				{
					return Factory.GetCachedValue((PK, nameof(IsInConstrainedMode)), IsInConstrainedMode, CacheStalenessPolicy.StaleOnFactorySave);
				}
			}
		}

		#endregion

		#region IBMControlCustomisationParent Members

		IBMControlCustomisationLinkCollection ICustomisedLayoutSupportable.CustomisedLayoutLinks
		{
			get
			{
				var links = !IsDeleted && MS_SectionType == BMConstants.ComponentSectionType
					? SectionConfiguration.CustomisedLayoutLinks
					: new BMControlCustomisationLinkCollection(Factory, ZQuery.NoResultQuery);

				var layoutSupportable = (ICustomisedLayoutSupportable)this;

				if (!layoutSupportable.AreCustomisedLayoutFetchHintsAdded)
				{
					BMControlCustomisationLink.AddBMControlCustomisationFetchHints(links, Factory);
					layoutSupportable.AreCustomisedLayoutFetchHintsAdded = true;
				}

				return links;
			}
		}

		bool ICustomisedLayoutSupportable.AreCustomisedLayoutFetchHintsAdded { get; set; }

		#endregion

		#region IFilterPreviewable Members

		public ZQuery GetAdditionalPreviewFilter(string moduleId, string dropDownCode)
		{
			return moduleId == ModuleIDs.ProcessTasks.Name ? GetAdditionalTaskPreviewFilter() : GetAdditionalWorkflowPreviewFilter();
		}

		ZQuery GetAdditionalWorkflowPreviewFilter()
		{
			var result = new ZQuery(ProcessHeaderSchema.FH_FC_CurrentComponent, ApplicableComponents.Select(c => c.PK));

			AddReleaseGroupFilterIfRequired(result);

			return result;
		}

		ZQuery GetAdditionalTaskPreviewFilter()
		{
			var taskQuery = new ZDBOnlyQuery(typeof(ProcessTasks));
			taskQuery.AddToFilter(new ZQuery(ProcessTasksSchema.P9_FC_CurrentComponent, ApplicableComponents.Select(c => c.PK)));

			var subQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessTasksSchema.P9_FH_ProcessHeader);
			var workflowFilterBizo = RelatedModuleFiltersHelper.GetNewFilterBusinessObject(ModuleIDs.BMFilterRule);
			var workflowFilter = workflowFilterBizo.GetFilter(SectionConfiguration.WorkflowFilter);
			subQuery.AddToFilter(workflowFilter);
			AddReleaseGroupFilterIfRequired(subQuery);

			taskQuery.AddSubQuery(subQuery, JoinCondition.And);
			return taskQuery;
		}

		void AddReleaseGroupFilterIfRequired(ZQuery query)
		{
			if (SectionConfiguration.ShowWorkInReleaseGroupOnly && !SectionConfiguration.ReleaseGroupPK.IsEmpty)
			{
				query.AddToFilter(ProcessHeaderSchema.FH_GG_ReleaseGroup, SQLComparisonOperator.Equal, SectionConfiguration.ReleaseGroupPK);
			}
		}

		#endregion

		#region IBranchDepartmentProvider Members

		IGlbBranch IBranchDepartmentProvider.GetBranch(BusinessObjectFactory factory)
		{
			return Board?.AgingBranch ?? ((IBranchDepartmentProvider)Component)?.GetBranch(factory);
		}

		IGlbDepartment IBranchDepartmentProvider.GetDepartment(BusinessObjectFactory factory)
		{
			return Board?.AgingDepartment ?? ((IBranchDepartmentProvider)Component)?.GetDepartment(factory);
		}

		#endregion

		#region IAuditParent Mambers

		public IEnumerable<AuditChildInfo> RelatedAuditChildren
		{
			get
			{
				yield return new AuditChildInfo(BMBoardSectionAdditionalComponentSchema.BSA_MS_Section, null);
				yield return new AuditChildInfo(BMBoardSectionChannelSchema.MSC_MS_Section, null);
			}
		}

		#endregion

		#region Load Utils

		public static BMBoardSection Load(BusinessObjectFactory factory, ZGuid pk)
		{
			var sectionQuery = new ZQuery(BMBoardSectionSchema.PK, pk);
			sectionQuery.IncludeBlob(BMBoardSectionSchema.MS_LayoutData);
			return factory.LoadTop1<BMBoardSection>(sectionQuery);
		}

		#endregion
	}
}
