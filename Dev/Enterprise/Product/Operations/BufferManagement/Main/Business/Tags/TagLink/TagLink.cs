using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[CodeProperty("MagnitudeCode"), DescriptionProperty(TagLink.Schema.TGL_Description)]
	[RowFetchStrategy(FetchStrategyType = typeof(TagLinkRowFetchStrategy))]
	public class TagLink : AutoTagLink, ITagLink
	{
		public TagLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(TGL_Magnitude), ConcurrencyPolicy.Ignore);
		}

		#region TypeDecider

		class TagLinkTypeDecider : TypeDecider
		{
			public override Type GetTypeForBinding()
			{
				return typeof(TagLink);
			}

			public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
			{
				if ((string)row[TagLinkSchema.TGL_ParentTableCode.Name] == ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(TagRule.Schema.TableName))
				{
					return typeof(TagLinkTemplate);
				}
				else if (factory.IsForServiceTask(TagServiceTask.Code))
				{
					return typeof(RuleRunnerTagLink);
				}
				else
				{
					var magnitude = new ZGuid(row[TagLinkSchema.TGL_TGM_Magnitude.Name]);

					if (magnitude.IsValid)
					{
						var queue = factory.Load<TagMagnitude>(magnitude) as WorkQueue;

						if (queue != null)
						{
							return typeof(WorkQueueMembershipLink);
						}
					}

					return typeof(TagLink);
				}
			}

			public override Type GetTypeForNew()
			{
				return typeof(TagLink);
			}
		}

		public static readonly TypeDecider TypeDecider = new TagLinkTypeDecider();

		#endregion

		#region Properties

		#region TGL_TGM_Magnitude

		[RelatedBusinessObject("Magnitude")]
		[List("Lookups.Magnitudes")]
		[ReadOnlyMember(nameof(CheckTagRemoveSecurity))]
		public override ZGuid TGL_TGM_Magnitude
		{
			get { return base.TGL_TGM_Magnitude; }
			set
			{
				var oldValue = TGL_TGM_Magnitude;

				base.TGL_TGM_Magnitude = value;

				if (oldValue != value && value.IsValid)
				{
					if (oldValue.IsValid)
					{
						UpdateWorkflowEditTimes();
					}

					var queue = Magnitude as WorkQueue;

					if (queue != null)
					{
						var highestLink = queue.Members.Where(l => l != this).MaxBySafe(l => l.TGL_Sequence);
						TGL_Sequence = (ZShort)(highestLink != null ? highestLink.TGL_Sequence + 1 : 1);
					}
					else
					{
						TGL_Sequence = ZShort.Zero;
					}
				}
			}
		}

		#endregion

		#region TagDefinitionPk

		[RelatedBusinessObject("Definition")]
		[List("Lookups.Definitions")]
		[ReadOnlyMember(nameof(CheckTagRemoveSecurity))]
		public ZGuid TagDefinitionPk
		{
			get
			{
				if (tagDefinitionPk.IsEmpty && Magnitude != null)
				{
					tagDefinitionPk = Magnitude.TGM_TGD_Tag;
				}
				return tagDefinitionPk;
			}
			set
			{
				SetNonPersistentPropertyValue(TagDefinitionPkInfo, ref tagDefinitionPk, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateTagDefinitionPk();
				}
			}
		}
		ZGuid tagDefinitionPk;

		public ZPropertyInfo TagDefinitionPkInfo
		{
			get { return GetZPropertyInfo(nameof(TagDefinitionPk)); }
		}

		#endregion

		#region TGL_Magnitude

		public override ZDecimal TGL_Magnitude
		{
			get { return base.TGL_Magnitude; }
			set
			{
				base.TGL_Magnitude = value;

				var processHeader = Parent as ProcessHeader;
				if (processHeader != null)
				{
					processHeader.EffectiveNudgeInfo.RefreshBinding();
				}
			}
		}

		#endregion

		#region TGL_ParentTableCode

		public override ZString TGL_ParentTableCode
		{
			get { return base.TGL_ParentTableCode; }
			set
			{
				base.TGL_ParentTableCode = value;
				UpdateWorkflowEditTimes();
			}
		}

		#endregion

		#region TGL_SystemCreateTimeUtc

		[ReadOnly(true)]
		public override ZDateTime TGL_SystemCreateTimeUtc
		{
			get { return base.TGL_SystemCreateTimeUtc; }
			set { base.TGL_SystemCreateTimeUtc = value; }
		}

		#endregion

		#region TGL_SystemCreateUser

		[ReadOnly(true)]
		public override ZString TGL_SystemCreateUser
		{
			get { return base.TGL_SystemCreateUser; }
			set { base.TGL_SystemCreateUser = value; }
		}

		#endregion

		#endregion

		#region New Properties

		[ResourceStringData("TagLink.EffectiveNudge", Caption = "Effective Nudge", ShortCaption = "Nudge", FullDescription = "The nudge value this tag creates.")]
		public ZDecimal EffectiveNudge
		{
			get
			{
				var nudge = Magnitude != null ? Magnitude.TGM_NudgeAmount : ZInt.Zero;
				var multiplier = TGL_Magnitude;

				if (multiplier > 1m && decimal.MaxValue / multiplier < nudge)
				{
					return decimal.MaxValue;
				}
				else if (multiplier < -1m && decimal.MinValue / multiplier < nudge)
				{
					return decimal.MinValue;
				}
				else
				{
					return nudge * multiplier;
				}
			}
		}

		public ZString MagnitudeCode
		{
			get { return Magnitude != null ? Magnitude.TGM_Code : ZString.Empty; }
		}

		public string DisplayText
		{
			get
			{
				var magnitude = Magnitude;
				return magnitude != null ? magnitude.DisplayText : string.Empty;
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get { return ResString.GetMultilingualString("446AF8DB-80B9-4616-B945-83CBDB0B0158", "Tag"); }
		}

		bool IsRuleUsageScopeViolated => !InOperationalScope && HasChanges && !IsRuleUsageScopeValidationSuspended && !Factory.HasContext(BufferManagementBusinessContext.TAGTriggerAction);

		public virtual bool InOperationalScope
		{
			get
			{
				if (Globals.IsUserInteractive)
				{
					return Definition == null || Definition.CanUserUseTags || Definition.CanUserAndRuleUseTags;
				}
				else
				{
					return Definition == null || Definition.CanRuleUseTags || Definition.CanUserAndRuleUseTags;
				}
			}
		}

		public virtual bool MutableInOperationalScope
		{
			get { return InOperationalScope; }
		}

		public virtual bool InParentScope
		{
			get { return Definition == null || Parent == null || Definition.ValidForScope(Parent.GetType()); }
		}

		internal bool IsAddTagValidationSuspended { get; set; }
		internal bool IsRuleUsageScopeValidationSuspended { get; set; }

		public ZInt NextSequenceNumber
		{
			get
			{
				var queue = Magnitude as WorkQueue;

				return queue != null ? queue.GetNextSequenceNumber() : ZInt.Zero;
			}
		}

		#endregion

		#region Related Business Objects

		public TagMagnitude Magnitude
		{
			get { return Factory.Load<TagMagnitude>(TGL_TGM_Magnitude); }
		}

		public TagDefinition Definition
		{
			get { return Factory.Load<TagDefinition>(TagDefinitionPk); }
		}

		public ITagable Parent
		{
			get
			{
				if (TGL_ParentTableCode != TagRuleSchema.Constants.Prefix)
				{
					var businessObjectType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(TGL_ParentTableCode, reportUnknownPrefix: false);
					return businessObjectType == null ? null : (ITagable)Factory.Load(businessObjectType, TGL_ParentId);
				}

				return null;
			}
		}

		public bool IsTemplateTag
		{
			get
			{
				var processHeader = Parent as ProcessHeader;
				return processHeader != null && processHeader.IsTemplate;
			}
		}

		#endregion

		#region BusinessObject Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			TGL_Magnitude = decimal.One;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var bizo = base.CloneInternal(args);

			if (bizo is TagLink clonedTagLink && args is TagLinkCloneArgs tagLinkCloneArgs)
			{
				clonedTagLink.TGL_ParentId = tagLinkCloneArgs.ParentID;
				clonedTagLink.IsAddTagValidationSuspended = tagLinkCloneArgs.SuspendAddTagValidation;
				clonedTagLink.IsRuleUsageScopeValidationSuspended = tagLinkCloneArgs.SuspendRuleUsageScopeValidation;
			}

			return bizo;
		}

		public override bool ReadOnly
		{
			get { return base.ReadOnly || (!HasChanges && !MutableInOperationalScope && InParentScope && !IsTemplateTag); }
			set { base.ReadOnly = value; }
		}

#if DEBUG
		public bool AreOnSavingChecksDisabledForTesting { get; set; }
#endif
		public override void OnSaving()
		{
#if DEBUG
			if (!AreOnSavingChecksDisabledForTesting)
			{
#endif
				if (IsRuleUsageScopeViolated && !IsTemplateTag)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Cannot modify tag with Usage Scope {0} from here", Definition.TGD_UsageScope));
				}
#if DEBUG
			}
#endif

			base.OnSaving();

			if (!IsInDatabase)
			{
				LogTagChange(TagActionType.AddTag);
			}
			else
			{
				LogTagChange(TagActionType.ModifyTag);
			}
		}

		public override void Delete()
		{
			var processHeader = IsDeleted ? null : Parent as ProcessHeader;

			if (IsDeleted || InOperationalScope || (processHeader != null && processHeader.IsDeletingNow) || IsTemplateTag)
			{
				if (IsInDatabase && !IsDeleted)
				{
					UpdateWorkflowEditTimes();
					LogTagChange(TagActionType.RemoveTag);
				}

				base.Delete();
			}
			else
			{
				throw new CannotDeleteException(Res.GetString("b0e8b83b-1f0e-4ce2-9bbd-7f0106de93e7", "Cannot delete tag with scope [{0}] from here.", Definition.TGD_UsageScope));
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				IsAddTagValidationSuspended = false;

				if (Factory.HasContext(BufferManagementBusinessContext.TAGTriggerAction))
				{
					Factory.RemoveContext(BufferManagementBusinessContext.TAGTriggerAction);
				}
			}
		}

		#endregion

		#region Implementation

		void LogTagChange(TagActionType actionType)
		{
			var workflow = Parent as ProcessHeader;
			var tag = Magnitude;

			if (workflow != null && tag != null)
			{
				var parameters = new Dictionary<string, string>
				{
					{ BMConstants.TagEventParameters.Action, actionType.ToCode() },
					{ BMConstants.TagEventParameters.Tag, tag.TGM_Code },
					{ BMConstants.TagEventParameters.TagGroup, tag.Definition.TGD_Code },
				};

				if (!MutatedByTagRulePK.IsEmpty)
				{
					parameters.Add(BMConstants.TagEventParameters.TagRule, MutatedByTagRulePK.ToString());
				}

				var eventValue = new EventValue(Events.TagWasAddedOrRemoved,
					parameters: parameters,
					deferFiringWorkflow: true); // Triggers & milestones on TAG events will fire with Log Walker rather than in the main process. This is to reduce performance impact on BMS/BMG service tasks.

				var existingLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TagWasAddedOrRemovedCode)
				{
					FetchOnlyFromLocalCache = true,
					OrderBy = StmALogSchema.Constants.SL_EventTime + " desc",
				};

				existingLogQuery.AddToFilter(StmALogSchema.SL_Parent, workflow.PK);
				var existingLog = Factory.LoadTop1<StmALog>(existingLogQuery);

				if (existingLog == null || existingLog.IsInDatabase || existingLog.SL_Reference != StmALog.GenerateEventReference(null, parameters))
				{
					workflow.Logs.AddNew(eventValue);
				}
			}
		}

		void UpdateWorkflowEditTimes()
		{
			var processHeader = GetProcessHeader();
			processHeader?.UpdateLastEditTimeIfNotDeleting();
		}

		IProcessHeader GetProcessHeader()
		{
			ProcessHeader processHeader;
			if (TGL_ParentTableCode == ProcessTasksSchema.Constants.Prefix)
			{
				processHeader = (Parent as ProcessTask)?.GetProcessHeader();
			}
			else
			{
				processHeader = Parent as ProcessHeader;
			}

			return processHeader;
		}

		protected virtual bool CheckTagRemoveSecurity
		{
			get
			{
				var magnitude = Magnitude;

				if (magnitude == null || !IsInDatabase)
				{
					return false;
				}

				return magnitude is WorkQueue ?
					!WorkQueueSecurity.CheckRemoveFromQueueSecurity(magnitude, showSecurityDialog: false) :
					!HasChanges && !TagSecurity.CheckTagRemoveSecurity(magnitude, showSecurityDialog: false);
			}
		}

		internal ZGuid MutatedByTagRulePK { get; set; }

		#endregion

		#region ITagLink

		public ITagMagnitude TagMagnitude
		{
			get { return Magnitude; }
		}

		#endregion
	}
}
