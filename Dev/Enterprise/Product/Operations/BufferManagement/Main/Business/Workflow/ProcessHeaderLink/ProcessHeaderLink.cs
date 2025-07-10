using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[System.Diagnostics.DebuggerDisplay("From: {HeaderFrom.FH_CompletionStatement} To: {HeaderTo.FH_CompletionStatement}")]
	public class ProcessHeaderLink : AutoProcessHeaderLink,
		IProcessHeaderLink,
		IProcessHeaderLinkRow,
		IEntityRelationship,
		ILink,
		ILoopDetectable,
		IAuditParent
	{
		public ProcessHeaderLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Validation

		public IDisposable ForceLoopValidation()
		{
			IsLoopValidationForced = true;

			return new DisposableAction(() => IsLoopValidationForced = false);
		}

		internal bool IsLoopValidationForced { get; private set; }

#if DEBUG
		public IDisposable EmulatePreSaveValidation_ForTest()
		{
			IsPreSaveValidationEmulated_ForTest = true;

			return new DisposableAction(() => IsPreSaveValidationEmulated_ForTest = false);
		}

		internal bool IsPreSaveValidationEmulated_ForTest { get; private set; }
#endif
		#endregion

		#region BusinessObject Overrides

		public override void OnSaving()
		{
			base.OnSaving();
			UpdateProcessHeaderFromEditTime();
			UpdateProcessHeaderToEditTime();
		}

		public enum DeleteOption
		{
			Invalid = 0,
			DeleteRelatedAttachments,
			DisconnectRelatedAttachments,
		}

		public override void Delete()
		{
			Delete(DeleteOption.DeleteRelatedAttachments);
		}

		public void Delete(DeleteOption deleteOption)
		{
			if (deleteOption != DeleteOption.Invalid)
			{
				var attachmentQuery = new ZQuery(BMNCNAttachmentSchema.BNA_FP_ProcessHeaderLink, PK) { FetchOnlyFromLocalCache = !IsInDatabase };
				attachmentQuery.AddToFilter(BMNCNAttachmentSchema.BNA_GS_NKApprovedBy, ZString.Empty);
				var attachments = Factory.Load<IBMNCNAttachment>(attachmentQuery);

				foreach (var attachment in attachments)
				{
					if (deleteOption == DeleteOption.DisconnectRelatedAttachments)
					{
						attachment.BNA_FP_ProcessHeaderLink = ZGuid.Empty;
					}
					else if (deleteOption == DeleteOption.DeleteRelatedAttachments)
					{
						attachment.Delete();
					}
				}
			}

			var headerTo = HeaderTo;
			var headerFrom = HeaderFrom;

			base.Delete();

			UpdateProcessHeaderEditTime(headerTo);
			UpdateProcessHeaderEditTime(headerFrom);
		}

		public void ProcessNewLink()
		{
			if (FP_LinkType == ProcessHeaderLinkTypeList.Codes.ParentChild
				&& HeaderFrom is ProcessJobHeader child
				&& HeaderTo is ProcessJobHeader parent)
			{
				child.FH_IsApproved = parent.FH_IsApproved;
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				switch (FP_LinkType)
				{
					case ProcessHeaderLinkTypeList.Codes.Dependency:
						return Res.GetString("09d556f4-14cb-4603-955e-4252dc4d83ad", "Dependency Link from [{0}] to [{1}]", HeaderFrom?.FH_CompletionStatement, HeaderTo?.FH_CompletionStatement);

					case ProcessHeaderLinkTypeList.Codes.ParentChild:
						return Res.GetString("7e46f4bf-d28a-458c-a1c5-1a87706aecd2", "Parent-Child Link from child [{0}] to parent [{1}]", HeaderFrom?.FH_CompletionStatement, HeaderTo?.FH_CompletionStatement);

					default:
						return base.HumanReadableNameCore;
				}
			}
		}

		#endregion

		#region Properties

		#region FP_FH_HeaderFrom

		[RelatedBusinessObject("HeaderFrom")]
		[List("Lookups.HeaderFroms")]
		public override ZGuid FP_FH_HeaderFrom
		{
			get { return base.FP_FH_HeaderFrom; }
			set
			{
				UpdateProcessHeaderFromEditTime();
				headerFrom = null;

				base.FP_FH_HeaderFrom = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateFP_FH_HeaderTo();
				}

				if (!FP_FH_HeaderFromInfo.HasErrors())
				{
					UpdateProcessHeaderFromEditTime();
				}
			}
		}

		public ProcessHeader HeaderFrom
		{
			get
			{
				return headerFrom == null ?
					headerFrom = Factory.Load<ProcessHeader>(FP_FH_HeaderFrom)
					: headerFrom.IsDeleted ? Factory.Load<ProcessHeader>(FP_FH_HeaderFrom) : headerFrom;
			}
		}
		ProcessHeader headerFrom;

		#endregion

		#region FP_FH_HeaderTo

		[RelatedBusinessObject("HeaderTo")]
		[List("Lookups.HeaderTos")]
		public override ZGuid FP_FH_HeaderTo
		{
			get { return base.FP_FH_HeaderTo; }
			set
			{
				UpdateProcessHeaderToEditTime();
				headerTo = null;

				base.FP_FH_HeaderTo = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateFP_FH_HeaderFrom();
				}

				if (!FP_FH_HeaderToInfo.HasErrors())
				{
					UpdateProcessHeaderToEditTime();
				}
			}
		}

		public ProcessHeader HeaderTo
		{
			get
			{
				return headerTo == null ?
					headerTo = Factory.Load<ProcessHeader>(FP_FH_HeaderTo)
					: headerTo.IsDeleted ? Factory.Load<ProcessHeader>(FP_FH_HeaderTo) : headerTo;
			}
		}
		ProcessHeader headerTo;

		#endregion

		#region FP_LinkType

		[ResourceStringData("ProcessHeaderLink|LinkTypeDescription", Caption = "Relationship Type", ShortCaption = "Type", FullDescription = "The workflow relationship type.")]
		public ZString LinkTypeDescription
		{
			get { return Lookups.LinkTypeList.GetDescriptionFromCode(FP_LinkType); }
		}

		[List("Lookups.LinkTypeList")]
		public override ZString FP_LinkType
		{
			get { return base.FP_LinkType; }
			set
			{
				base.FP_LinkType = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateFP_FH_HeaderFrom();
				}
			}
		}

		#endregion

		#region FP_SynchroniseBufferPenetration

		[ReadOnlyMember(nameof(FP_SynchroniseBufferPenetration_ReadOnly))]
		public override ZBool FP_SynchroniseBufferPenetration
		{
			get { return base.FP_SynchroniseBufferPenetration; }
			set { base.FP_SynchroniseBufferPenetration = value; }
		}

		protected bool FP_SynchroniseBufferPenetration_ReadOnly
		{
			get
			{
				var from = HeaderFrom;
				var to = HeaderTo;

				return from == null || to == null || !from.IsChildWorkflowWithinJobOf(to);
			}
		}

		#endregion

		#endregion

		#region New Properties

		#region StaggeredReleaseTimeDelay

		[ZDateTimeDurationValueCalculatedFromMinutes]
		[ResourceStringData("ProcessHeaderLink|StaggeredReleaseTimeDelay", ShortCaption = "Delay Time", Caption = "Release Delay Time", FullDescription = "The number of hours/minutes from release of previous workflows that need to be elapsed when determining a workflow’s Staggered Release Delay Expiry.")]
		public ZDateTime StaggeredReleaseTimeDelay
		{
			get { return FP_TimeDelayMinutes.GetDateTimeFromMinutes(); }
			set
			{
				FP_TimeDelayMinutes = (ZInt)value.GetMinutesFromDateTimeSpan();
				StaggeredReleaseTimeDelayInfo.RefreshBinding();
				FP_TimeDelayMinutesInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo StaggeredReleaseTimeDelayInfo
		{
			get { return GetZPropertyInfo(nameof(StaggeredReleaseTimeDelay)); }
		}

		#endregion

		#region Relevant Staggered Delays

		public ZDecimal GetApplicableTimeDelayFactor(ProcessJobHeader jobHeader)
		{
			return !FP_TimeDelayFactor.IsEmpty ? FP_TimeDelayFactor : jobHeader.FH_TimeDelayFactor;
		}

		public ZInt GetApplicableAbsoluteGapMinutes(ProcessJobHeader jobHeader)
		{
			return !FP_TimeDelayMinutes.IsEmpty ? FP_TimeDelayMinutes : jobHeader.FH_TimeDelayMinutes;
		}

		#endregion

		#region External Workflow Template

		#region FromWorkflowExternalTemplatePK

		[List("Lookups.Templates")]
		[RelatedBusinessObject("FromWorkflowExternalTemplate")]
		[ResourceStringData("ProcessHeaderLink.FromWorkflowExternalTemplatePK", Caption = "External 'From' Workflow Template", ShortCaption = "'From' Template", FullDescription = "Another Workflow Template from which to pick the 'From' Workflow.")]
		public ZGuid FromWorkflowExternalTemplatePK
		{
			get
			{
				MaybeSetExternalWorkflowTemplatePK(ref fromWorkflowExternalTemplatePK, Lazy.Create(() => HeaderFrom));

				return fromWorkflowExternalTemplatePK;
			}
			set
			{
				if (value.IsEmpty && !fromWorkflowExternalTemplatePK.IsEmpty)
				{
					FP_FH_HeaderFrom = ZGuid.Empty;
				}

				SetNonPersistentPropertyValue(FromWorkflowExternalTemplatePKInfo, ref fromWorkflowExternalTemplatePK, value);
				Lookups.ClearCachedCollections();
			}
		}

		ZGuid fromWorkflowExternalTemplatePK;

		public ZPropertyInfo FromWorkflowExternalTemplatePKInfo
		{
			get { return GetZPropertyInfo(nameof(FromWorkflowExternalTemplatePK)); }
		}

		#endregion

		#region ToWorkflowExternalTemplatePK

		[List("Lookups.Templates")]
		[RelatedBusinessObject("ToWorkflowExternalTemplate")]
		[ResourceStringData("ProcessHeaderLink.ToWorkflowExternalTemplatePK", Caption = "External 'To' Workflow Template", ShortCaption = "'To' Template", FullDescription = "Another Workflow Template from which to pick the 'To' Workflow.")]
		public ZGuid ToWorkflowExternalTemplatePK
		{
			get
			{
				MaybeSetExternalWorkflowTemplatePK(ref toWorkflowExternalTemplatePK, Lazy.Create(() => HeaderTo));

				return toWorkflowExternalTemplatePK;
			}
			set
			{
				if (value.IsEmpty && !toWorkflowExternalTemplatePK.IsEmpty)
				{
					FP_FH_HeaderTo = ZGuid.Empty;
				}

				SetNonPersistentPropertyValue(ToWorkflowExternalTemplatePKInfo, ref toWorkflowExternalTemplatePK, value);
				Lookups.ClearCachedCollections();
			}
		}

		ZGuid toWorkflowExternalTemplatePK;

		public ZPropertyInfo ToWorkflowExternalTemplatePKInfo
		{
			get { return GetZPropertyInfo(nameof(ToWorkflowExternalTemplatePK)); }
		}

		#endregion

		void MaybeSetExternalWorkflowTemplatePK(ref ZGuid externalTemplatePK, Lazy<ProcessHeader> processHeader)
		{
			if (!IsDeleted && Template != null && processHeader.Value != null && processHeader.Value.FH_P0_Template != Template.PK)
			{
				externalTemplatePK = processHeader.Value.FH_P0_Template;
			}
		}

		internal bool IsExternalTemplateLink
		{
			get
			{
				var from = HeaderFrom;
				var to = HeaderTo;

				return from != null && to != null && from.FH_P0_Template != to.FH_P0_Template;
			}
		}

		#endregion

		#region FromHeaderDescription

		[ResourceStringData("FromHeaderDescription", Caption = "From Workflow")]
		public ZString FromHeaderDescription
		{
			get { return HeaderFrom?.CodeWithParentCode ?? ZString.Empty; }
		}

		public ZPropertyInfo FromHeaderDescriptionInfo => GetWrappedZPropertyInfo(nameof(FromHeaderDescription), _ => FP_FH_HeaderFromInfo);

		#endregion

		#region ToHeaderDescription

		[ResourceStringData("ToHeaderDescription", Caption = "To Workflow")]
		public ZString ToHeaderDescription
		{
			get { return HeaderTo?.CodeWithParentCode ?? ZString.Empty; }
		}

		public ZPropertyInfo ToHeaderDescriptionInfo => GetWrappedZPropertyInfo(nameof(ToHeaderDescription), _ => FP_FH_HeaderToInfo);

		#endregion

		#region StatusOfFromWorkflow

		[ResourceStringData("ProcessHeaderLink.StatusOfFromWorkflow", Caption = "Status of Prerequisite Workflow", ShortCaption = "Prerequisite Status")]
		public ZString StatusOfFromWorkflow => HeaderFrom?.FH_StatusDescription ?? ZString.Empty;

		#endregion

		#endregion

		#region Related Business Objects

		internal ProcessJobHeader ProcessJobHeader
		{
			get { return processJobHeader; }
			set
			{
				processJobHeader = value;

				Lookups.ClearCachedCollections();
			}
		}

		ProcessJobHeader processJobHeader;

		public IBMNCNAttachment ApprovedArrow
		{
			get
			{
				var query = new ZQuery(BMNCNAttachmentSchema.BNA_FP_ProcessHeaderLink, PK).AddToFilter(BMNCNAttachmentSchema.BNA_GS_NKApprovedBy, SQLComparisonOperator.NotEqual, ZString.Empty);
				return Factory.LoadTop1<IBMNCNAttachment>(query);
			}
		}

		#region Process Task Template

		internal ProcessTaskTemplate Template { get; set; }

		public ProcessTaskTemplate FromWorkflowExternalTemplate
		{
			get { return Factory.Load<ProcessTaskTemplate>(FromWorkflowExternalTemplatePK); }
		}

		public ProcessTaskTemplate ToWorkflowExternalTemplate
		{
			get { return Factory.Load<ProcessTaskTemplate>(ToWorkflowExternalTemplatePK); }
		}

		#endregion

		#endregion

		#region Temporary (for defect fix)

		public string DefectFixLog { get; set; }

		#endregion

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new ProcessHeaderLinkUniqueIndexFailureHandler(this); }
		}

		class ProcessHeaderLinkUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			internal ProcessHeaderLinkUniqueIndexFailureHandler(ProcessHeaderLink link)
			{
				this.link = link;
			}

			readonly ProcessHeaderLink link;

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				notifier.ReportError(GetFailureMessage(), Res.GetString("6b31e577-8918-48b7-97f4-6069c09d5107", "Cannot Save"));
			}

			string GetFailureMessage()
			{
				var str = Res.GetString("2ebeeb73-a076-4285-9bc9-6a127dead8aa", "There are duplicate Related Workflow links. The duplicate values are: (From Workflow: {0}, To Workflow: {1}, Link Type: {2}).{3}",
					link.HeaderFrom != null ? link.HeaderFrom.FH_CompletionStatement.ToString() : link.FP_FH_HeaderFrom.ToString(),
					link.HeaderTo != null ? link.HeaderTo.FH_CompletionStatement.ToString() : link.FP_FH_HeaderTo.ToString(),
					link.FP_LinkType,
					string.IsNullOrEmpty(link.DefectFixLog) ? string.Empty : "\r\nDefectFixLog:\r\n" + link.DefectFixLog);

				link.DefectFixLog = string.Empty;
				return str;
			}

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get { yield return ProcessHeaderLinkSchema.Constants.Indexes.FK_UC__FP_FH_HeaderFrom_FP_FH_HeaderTo_FP_LinkType; }
			}
		}

		#endregion

		#region UpdateProcessHeaderEditTime

		void UpdateProcessHeaderFromEditTime()
		{
			UpdateProcessHeaderEditTime(HeaderFrom);
		}

		void UpdateProcessHeaderToEditTime()
		{
			UpdateProcessHeaderEditTime(HeaderTo);
		}

		void UpdateProcessHeaderEditTime(ProcessHeader processHeader)
		{
			processHeader?.UpdateLastEditTimeIfNotDeleting();
		}

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		IProcessHeaderLink IProcessHeaderLink.Clone()
		{
			return (IProcessHeaderLink)Clone();
		}

		#endregion

		#region ModuleFilterConstants

		public static class ModuleFilterConstants
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Module filter 'name'")]
			public const string HeaderFrom = "Header From";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Module filter 'name'")]
			public const string HeaderTo = "Header To";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Module filter 'name'")]
			public const string LinkType = "Link Type";
		}

		#endregion

		#region IProcessHeaderLink Members

		IProcessHeader IProcessHeaderLink.HeaderFrom
		{
			get { return HeaderFrom; }
		}

		IProcessHeader IProcessHeaderLink.HeaderTo
		{
			get { return HeaderTo; }
		}

		#endregion

		#region IEntityRelationship Members

		IProposedNetworkEntity IEntityRelationship.From
		{
			get { return HeaderFrom; }
			set
			{
				if (value == null)
				{
					FP_FH_HeaderFrom = ZGuid.Empty;
				}
				else
				{
					var workflow = (ProcessHeader)value;
					FP_FH_HeaderFrom = workflow.PK;
				}
			}
		}

		IProposedNetworkEntity IEntityRelationship.To
		{
			get { return HeaderTo; }
			set
			{
				if (value == null)
				{
					FP_FH_HeaderTo = ZGuid.Empty;
				}
				else
				{
					var workflow = (ProcessHeader)value;
					FP_FH_HeaderTo = workflow.PK;
				}
			}
		}

		string IEntityRelationship.DisplayText
		{
			get { return DisplayText; }
		}

		public string DisplayText
		{
			get { return this.GetDisplayName(); }
		}

		public string BackColor
		{
			get { return BMConstants.NecessityDependencyArrowColorName; }
		}

		public bool IsVisible
		{
			get { return true; }
		}

		#endregion

		#region ILink Members

		Guid ILink.PK
		{
			get { return PK.ToGuid(); }
		}

		ILinkEntity ILink.Owner
		{
			get { return null; }
		}

		ILinkEntity ILink.From
		{
			get { return HeaderFrom; }
		}

		ILinkEntity ILink.To
		{
			get { return HeaderTo; }
		}

		public bool IsBufferedAttachment => false;

		ZString ILoopDetectable.LinkType
		{
			get => FP_LinkType;
		}

		#endregion

		#region IAuditParent Mambers

		public IEnumerable<AuditChildInfo> RelatedAuditChildren => Enumerable.Empty<AuditChildInfo>();

		#endregion

		#region Static Methods

		public static IEnumerable<ProcessHeaderLink> LoadLinksIntoFactoryOptimisingForUnsavedProcessHeaders(BusinessObjectFactory factory, IEnumerable<ProcessHeader> processHeadersWhichMayContainUnsavedWorkflows, ZGuid[] extraProcessHeaderPksForWhichToFetchLinksFromDatabase = null, bool shouldForceSeek = false, RelationshipDirection? relationshipDirection = null)
		{
			var headersByDatabaseOrMemory = processHeadersWhichMayContainUnsavedWorkflows.GroupBy(x => x.IsInDatabase).ToArray();
			var workflowsInMemory = headersByDatabaseOrMemory.SingleOrDefault(x => !x.Key);
			var workflowsInDatabase = headersByDatabaseOrMemory.SingleOrDefault(x => x.Key);

			var linksForWorkflowsInMemory = LoadLinks(workflowsInMemory, true);
			var linksForWorkflowsInDatabase = LoadLinks(workflowsInDatabase, false);

			IEnumerable<ProcessHeaderLink> LoadLinks(IEnumerable<ProcessHeader> headers, bool shouldFetchFromLocalCacheOnly)
			{
				var pks = headers?.Select(x => x.PK) ?? Enumerable.Empty<ZGuid>();

				if (extraProcessHeaderPksForWhichToFetchLinksFromDatabase != null)
				{
					pks = pks.Union(extraProcessHeaderPksForWhichToFetchLinksFromDatabase);
				}

				return LoadLinksIntoFactory(factory, pks, shouldForceSeek, shouldFetchFromLocalCacheOnly, relationshipDirection);
			}

			return linksForWorkflowsInMemory.Union(linksForWorkflowsInDatabase);
		}

		public static IEnumerable<ProcessHeaderLink> LoadLinksIntoFactoryWithoutOptimisingForUnsavedObjects(BusinessObjectFactory factory, IEnumerable<ZGuid> processHeaderPKs, bool shouldForceSeek = false, RelationshipDirection? relationshipDirection = null)
		{
			return LoadLinksIntoFactory(factory, processHeaderPKs, shouldForceSeek, shouldFetchFromLocalCacheOnly: false, relationshipDirection);
		}

		static IEnumerable<ProcessHeaderLink> LoadLinksIntoFactory(BusinessObjectFactory factory, IEnumerable<ZGuid> processHeaderPKs, bool shouldForceSeek, bool shouldFetchFromLocalCacheOnly, RelationshipDirection? relationshipDirection)
		{
			var pks = processHeaderPKs.WhereNotNull().ToArray();

			if (pks.Any())
			{
				var linksFrom = relationshipDirection == null || relationshipDirection == RelationshipDirection.From ? GetLinks(ProcessHeaderLinkSchema.FP_FH_HeaderFrom) : Enumerable.Empty<ProcessHeaderLink>();
				var linksTo = relationshipDirection == null || relationshipDirection == RelationshipDirection.To ? GetLinks(ProcessHeaderLinkSchema.FP_FH_HeaderTo) : Enumerable.Empty<ProcessHeaderLink>();

				return linksFrom.Union(linksTo);
			}

			return Enumerable.Empty<ProcessHeaderLink>();

			ProcessHeaderLink[] GetLinks(SchemaGuidColumn column)
			{
				return factory.Load<ProcessHeaderLink>(new ZQuery { AllowTableValuedParameters = true, IsForceSeek = shouldForceSeek, FetchOnlyFromLocalCache = shouldFetchFromLocalCacheOnly }.AddToFilter(column, pks));
			}
		}

		#endregion
	}
}
