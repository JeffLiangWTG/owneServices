using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TimeEngineScheduler.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.BufferManagement.Business
{
	[CodeProperty(BMSystemSchema.Constants.FS_Name), DescriptionProperty(BMSystemSchema.Constants.FS_Description)]
	public class BMSystem : AutoBMSystem,
		IDocManagerSupport,
		IBMSystem,
		IPAVESystem,
		ITemplateCopyable,
		ICustomisedLayoutSupportable,
		IAuditParent
	{
		public BMSystem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Load

		public static IEnumerable<BMSystem> GetAllApplicableBMSystems(string workflowType, BusinessObjectFactory factory)
		{
			if (string.IsNullOrEmpty(workflowType))
			{
				return factory.Load<BMSystem>(new ZQuery()).OrderBy(s => s.FS_Name);
			}
			else
			{
				var system = GetSystemForWorkflowType(workflowType, factory);
				return system != null ? new[] { system } : Enumerable.Empty<BMSystem>();
			}
		}

		public static BMSystem GetSystemForWorkflowProvider(IWorkflowProviderCore workflowProvider, BusinessObjectFactory factory)
		{
			Argument.NotNull(workflowProvider, nameof(workflowProvider));
			return GetSystemForWorkflowType(workflowProvider.WorkflowType, factory);
		}

		public static BMSystem GetSystemForWorkflowType(string workflowType, BusinessObjectFactory factory)
		{
			return GetSystemsForWorkflowType(workflowType, factory).FirstOrDefault();
		}

		internal static BMSystem[] GetSystemsForWorkflowType(string workflowType, BusinessObjectFactory factory)
		{
			var allSystems = factory.Load<BMSystem>(new ZQuery());

			return allSystems.Where(x => x.IsForWorkflowType(workflowType)).ToArray();
		}

		public static BMSystem GetForTemplate(ProcessTaskTemplate template)
		{
			return (!template.P0_FS_BufferManagementSystem.IsEmpty) ?
				template.Factory.Load<BMSystem>(template.P0_FS_BufferManagementSystem) :
				GetSystemForWorkflowType(template.P0_ProcessType, template.Factory);
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new BMSystemFetchStrategy(this);
		}

		class BMSystemFetchStrategy : EnterpriseBusinessObjectFetchStrategy
		{
			internal BMSystemFetchStrategy(BMSystem bmSystem)
				: base(bmSystem)
			{
				this.bmSystem = bmSystem;
			}

			readonly BMSystem bmSystem;

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();

				Factory.AddFetchHint(BMSystemWorkflowDeterminerSchema.FSW_FS_System, bmSystem.PK);
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				foreach (BMSystemReleaseGroup releaseGroup in bmSystem.ReleaseGroups)
				{
					var query = new ZQuery(BMControlCustomisationLinkSchema.FML_ParentId, releaseGroup.FSG_GG_Group);
					Factory.AddFetchHint(BMControlCustomisationLinkSchema.Instance, query);
					Factory.AddFetchHint(BMControlCustomisationLinkSchema.FML_ParentId, releaseGroup.FSG_GG_Group);
				}

				foreach (var component in bmSystem.Components)
				{
					Factory.AddFetchHint(BMComponentSchema.FC_FC_ParentComponent, component.PK);
					Factory.AddFetchHint(BMComponentLinkSchema.FL_FC_ComponentFrom, component.PK);
					Factory.AddFetchHint(BMComponentReleaseGroupLinkSchema.FO_FC_Component, component.PK);
					Factory.AddFetchHint(BMZoneCapacityMultiplierSchema.BZC_FC_Component, component.PK);
				}
			}
		}

		#endregion

		#region New Properties

		#region CountdownTime

		[ZDateTimeDurationValue]
		public override ZDateTime FS_ResourceCountdownHours
		{
			get => base.FS_ResourceCountdownHours;
			set => base.FS_ResourceCountdownHours = value.ConvertToDurationBasedDate(FS_ResourceCountdownHoursInfo);
		}

		public TimeSpan CountdownTime
		{
			get { return FS_ResourceCountdownHours.IsValid ? TimeSpan.FromMinutes(FS_ResourceCountdownHours.GetMinutesFromDateTimeSpan()) : TimeSpan.Zero; }
		}

		#endregion

		#region TextSchematic

		public ZString TextSchematic
		{
			get { return new SystemSchematic(this).SchematicText; }
		}

		public ZPropertyInfo TextSchematicInfo
		{
			get { return GetZPropertyInfo(nameof(TextSchematic)); }
		}

		#endregion

		#region Get First Component

		internal BMComponent GetFirstBMComponent()
		{
			return Components.FirstOrDefault(x => x.FC_IsActive && !x.GetFromOthersToMeLinksWithinSystem(this).Any());
		}

		#endregion

		#region IsLive

		static string IsLiveText { get { return Res.GetString("5E25499D-5BA8-423D-A577-04F6D9579DF6", "Live"); } }
		static string IsNotLiveText { get { return Res.GetString("E85037EF-BAA5-4946-9D09-66CCE367942D", "Not Live"); } }

		public ZString LivelinessDescriptionText
		{
			get
			{
				return FS_IsLive
					? IsLiveText
					: IsNotLiveText;
			}
		}

		#endregion

		#endregion

		#region Related Business Objects

		[ChildEditable]
		public BMComponentCollection Components
		{
			get
			{
				if (components == null)
				{
					components = new BMComponentCollection(this);
					components.ApplySort(BMComponent.Schema.FC_DisplaySequence, ListSortDirection.Ascending);
					RegisterEditableChildObject(components);
				}

				return components;
			}
		}

		BMComponentCollection components;

		[ChildEditable]
		public BMSystemWorkflowDeterminerCollection RelatedWorkflowTypes
		{
			get
			{
				if (relatedWorkflowTypes == null)
				{
					relatedWorkflowTypes = new BMSystemWorkflowDeterminerCollection(this);
					RegisterEditableChildObject(relatedWorkflowTypes);
				}

				return relatedWorkflowTypes;
			}
		}

		BMSystemWorkflowDeterminerCollection relatedWorkflowTypes;

		public bool IsForWorkflowType(string workflowType)
		{
			return RelatedWorkflowTypes.Any(w => w.FSW_WorkflowType == workflowType);
		}

		public BMBoardCollection Boards
		{
			get
			{
				if (boards == null)
				{
					// Not child editable because boards are never edited on a BMSystem anymore.
					boards = new BMBoardCollection(this, new ZQuery());
				}

				return boards;
			}
		}

		BMBoardCollection boards;

		[ChildEditable]
		public BMSystemReleaseGroupCollection ReleaseGroups
		{
			get
			{
				if (releaseGroups == null)
				{
					releaseGroups = new BMSystemReleaseGroupCollection(this);
					RegisterEditableChildObject(releaseGroups);
				}

				return releaseGroups;
			}
		}

		BMSystemReleaseGroupCollection releaseGroups;

		public ActiveBusinessObjectCollection<GlbGroup> ReleaseGroupLookups
		{
			get
			{
				return Factory.GetCachedValue("BMSystem.ReleaseGroupLookups." + PK, () =>
				{
					var collection = new ReleaseGroupGlbGroupCollection(this);
					collection.ApplySort(GlbGroupSchema.Constants.GG_Code, ListSortDirection.Ascending);

					return collection;
				});
			}
		}

		[ChildEditable]
		public BMControlCustomisationLinkCollection CustomisedLayoutLinks
		{
			get
			{
				if (customisedLayoutLinks == null)
				{
					customisedLayoutLinks = new BMControlCustomisationLinkCollection(this);
					RegisterEditableChildObject(customisedLayoutLinks);
				}

				return customisedLayoutLinks;
			}
		}

		BMControlCustomisationLinkCollection customisedLayoutLinks;

		#endregion

		#region BusinessObject Overrides

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return Res.GetString("75cf83ae-7d8a-4074-a804-e9068badc58e", "Buffer Management System - {0}", ((ICodeDescription)this).Code);
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			FS_IsLive = false;
		}

		public override void Delete()
		{
			Components.DeleteAll();
			RelatedWorkflowTypes.DeleteAll();
			Boards.DeleteAll();
			ReleaseGroups.DeleteAll();
			CustomisedLayoutLinks.DeleteAll();

			base.Delete();
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clone = (BMSystem)base.CloneInternal(args);
			clone.EnsureNameIsUnique();

			foreach (var workflowType in RelatedWorkflowTypes)
			{
				var cloneWorkflowType = (BMSystemWorkflowDeterminer)workflowType.Clone(new BusinessObjectCloneArgs(new[] { BMSystemWorkflowDeterminerSchema.FSW_FS_System.Name }));
				cloneWorkflowType.FSW_FS_System = clone.PK;
			}

			AddCloneFetchHints();

			var componentPKMap = new Dictionary<ZGuid, ZGuid>();
			var cloneArgs = new BusinessObjectCloneArgs(columnNamesToExcludeFromCopy: new[] { BMComponentSchema.FC_FS_System.Name });
			var clonedComponentsAndOriginals = Components.Select(c => new { Clone = (BMComponent)c.Clone(cloneArgs), Original = c });

			foreach (var cloneOriginalPair in clonedComponentsAndOriginals)
			{
				componentPKMap[cloneOriginalPair.Original.PK] = cloneOriginalPair.Clone.PK;
				cloneOriginalPair.Clone.FC_FS_System = clone.PK;

				foreach (var child in cloneOriginalPair.Clone.ChildComponents)
				{
					child.FC_FS_System = clone.PK;
				}
			}

			var clonedLinksAndOriginals = Components.SelectMany(component => component.FromMeToOthersLinks.Select(l => new
			{
				Clone = (BMComponentLink)l.Clone(new BusinessObjectCloneArgs(columnNamesToExcludeFromCopy: new[]
				{
					BMComponentLinkSchema.FL_FC_ComponentFrom.Name, BMComponentLinkSchema.FL_FC_ComponentTo.Name
				})),
				Original = l
			}));

			foreach (var cloneOriginalPair in clonedLinksAndOriginals)
			{
				cloneOriginalPair.Clone.FL_FC_ComponentFrom = componentPKMap[cloneOriginalPair.Original.FL_FC_ComponentFrom];
				cloneOriginalPair.Clone.FL_FC_ComponentTo = componentPKMap[cloneOriginalPair.Original.FL_FC_ComponentTo];
				cloneOriginalPair.Clone.FL_IsReleaseGateRuleApplied = cloneOriginalPair.Original.FL_IsReleaseGateRuleApplied;
			}

			return clone;
		}

		void AddCloneFetchHints()
		{
			foreach (var component in Components)
			{
				Factory.AddFetchHint(BMComponentSchema.FC_FC_ParentComponent, component.PK);
				Factory.AddFetchHint(BMComponentLinkSchema.FL_FC_ComponentFrom, component.PK);
				Factory.AddFetchHint(BMComponentReleaseGroupLinkSchema.FO_FC_Component, component.PK);
				Factory.AddFetchHint(BMZoneCapacityMultiplierSchema.BZC_FC_Component, component.PK);
			}
		}

		public void EnsureNameIsUnique()
		{
			var oldIgnoreValidationSuspended = IgnoreValidationSuspended;

			using (new DisposableAction(() => IgnoreValidationSuspended = true, () => IgnoreValidationSuspended = oldIgnoreValidationSuspended))
			{
				if (!FS_NameInfo.IsBusinessObjectValidationSuspended())
				{
					Validation.ValidateFS_Name();

					while (FS_NameInfo.HasErrors())
					{
						FS_Name = FS_Name.ToString().AppendNextBracketedNumber(BMSystemSchema.FS_Name.MaxLength);
					}
				}
			}
		}

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.BMSystem)); }
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IBMSystem members

		IBusinessObjectCollection IBMSystem.Components
		{
			get { return Components; }
		}

		#endregion

		#region IPAVESystem Members

		Guid IPAVESystem.PK => PK.ToGuid();

		string IPAVESystem.Name => FS_Name;

		bool IPAVESystem.IsLive => FS_IsLive;

		#endregion

		#region ITemplateCopyable Members

		IBusiness ITemplateCopyable.TemplateCopy()
		{
			return Clone();
		}

		#endregion

		#region CustomisedControls

		public BMControlCustomisationCollection CustomisedControls
		{
			get
			{
				if (customisedControls == null)
				{
					customisedControls = new BMControlCustomisationCollection(Factory);
				}

				return customisedControls;
			}
		}

		BMControlCustomisationCollection customisedControls;

		#endregion

		#region ICustomisedLayoutSupportable Members

		IBMControlCustomisationLinkCollection ICustomisedLayoutSupportable.CustomisedLayoutLinks
		{
			get { return CustomisedLayoutLinks; }
		}

		bool ICustomisedLayoutSupportable.AreCustomisedLayoutFetchHintsAdded { get; set; }

		#endregion

		#region IAuditParent Members

		public IEnumerable<AuditChildInfo> RelatedAuditChildren
		{
			get
			{
				yield return new AuditChildInfo(BMComponentSchema.FC_FS_System, null);
				yield return new AuditChildInfo(BMSystemWorkflowDeterminerSchema.FSW_FS_System, null);
				yield return new AuditChildInfo(BMControlCustomisationLinkSchema.FML_ParentId, null);
				yield return new AuditChildInfo(BMSystemReleaseGroupSchema.FSG_FS_System, null);
			}
		}

		#endregion

		#region Update Related Workflows

		public void UpdateRelatedWorkflows()
		{
			ZExceptionReporting.ProcessWithConcurrencyHandling(() =>
			{
				var scheduleFactory = new BusinessObjectFactory() { NameForDebugging = $"{nameof(BMSystem)}.{nameof(UpdateRelatedWorkflows)}" };

				foreach (var component in Components.Where(c => c.FromMeToOthersLinks.Any()))
				{
					var schedule = ScheduleProvider.ScheduleOrRescheduleAction(ProcessHeaderResponsiveActionConstants.ProcessHeaderResponsiveUpdateSchedulerActionCode,
						ZDateTime.UtcNow,
						targetPk: component.PK,
						targetTableCode: BMComponentSchema.Constants.Prefix,
						jsonParameter: ProcessHeaderResponsiveActionConstants.UpdateAllExceptDedicatedBuffer,
						factory: scheduleFactory);
				}
				scheduleFactory.Save();
			}, onRetry: () => { });

			BMSRegistry.Instance.ProcessAllTransferRulesLinksOnNextBMSRun.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var nudger = ObjectFactory.Get<IServiceTaskNudger>();
			var delay = BMSRegistry.Instance.DelayForResponsiveWorkflowUpdatesOnRelatedObjectChanges.Value;
			nudger.NudgeServiceTask(TransferRuleRunnerServiceTask.Code, TimeSpan.FromMinutes(delay));
		}

		IActionScheduleProvider ScheduleProvider => scheduleProvider ?? (scheduleProvider = ObjectFactory.Get<IActionScheduleProvider>());
		IActionScheduleProvider scheduleProvider;

		#endregion

		#region Static Helper Methods

		public static bool DoesSystemHaveReleaseGroups(BusinessObjectFactory factory, ZGuid systemPk)
		{
			return factory.GetCachedValue(DoesSystemHaveReleaseGroupsKey + systemPk, () => DoesSystemHaveReleaseGroupsInFactory(factory, systemPk));
		}

		static bool DoesSystemHaveReleaseGroupsInFactory(BusinessObjectFactory factory, ZGuid systemPk)
		{
			var query = new ZQuery(BMSystemReleaseGroupSchema.FSG_FS_System, systemPk);

			return factory.Exists(typeof(BMSystemReleaseGroup), query);
		}

		const string DoesSystemHaveReleaseGroupsKey = "DoesSystemHaveReleaseGroupsKey";

		#endregion
	}
}
