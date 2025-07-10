using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class ProposedProcessHeaderLink : NonPersistentBusinessObject<ProposedProcessHeaderLinkValidation>, IProcessHeaderLinkRow
	{
		public ProposedProcessHeaderLink(ProcessJobHeader fromJobHeader, ProcessJobHeader toJobHeader)
			: base(fromJobHeader.Factory)
		{
			this.fromJobHeader = fromJobHeader;
			this.toJobHeader = toJobHeader;
		}

		readonly ProcessJobHeader fromJobHeader;
		readonly ProcessJobHeader toJobHeader;

		#region IProcessHeaderLinkRow Members

		#region FP_LinkType

		[List("LinkTypes")]
		public ZString FP_LinkType
		{
			get { return linkType; }
			set
			{
				SetNonPersistentPropertyValue(FP_LinkTypeInfo, ref linkType, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateFP_LinkType();
				}
			}
		}

		ZString linkType;

		public ZPropertyInfo FP_LinkTypeInfo
		{
			get { return GetZPropertyInfo(nameof(FP_LinkType)); }
		}

		public ICodeDescriptionPairList LinkTypes
		{
			get { return Factory.GetCachedValue<ProcessHeaderLinkTypeList>(); }
		}

		#endregion

		#region FP_FH_HeaderFrom

		[List(nameof(AllWorkflows))]
		[RelatedBusinessObject(nameof(FromWorkflow))]
		public ZGuid FP_FH_HeaderFrom
		{
			get { return headerFromPK; }
			set
			{
				SetNonPersistentPropertyValue(FP_FH_HeaderFromInfo, ref headerFromPK, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateFP_FH_HeaderFrom();
				}
			}
		}

		ZGuid headerFromPK;

		public ZPropertyInfo FP_FH_HeaderFromInfo
		{
			get { return GetZPropertyInfo(nameof(FP_FH_HeaderFrom)); }
		}

		#endregion

		#region FP_FH_HeaderTo

		[List("AllWorkflows")]
		[RelatedBusinessObject("ToWorkflow")]
		public ZGuid FP_FH_HeaderTo
		{
			get { return headerToPK; }
			set
			{
				SetNonPersistentPropertyValue(FP_FH_HeaderToInfo, ref headerToPK, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateFP_FH_HeaderTo();
				}
			}
		}

		ZGuid headerToPK;

		public ZPropertyInfo FP_FH_HeaderToInfo
		{
			get { return GetZPropertyInfo(nameof(FP_FH_HeaderTo)); }
		}

		#endregion

		#region FP_TimeDelayFactor

		public ZDecimal FP_TimeDelayFactor
		{
			get { return timeDelayFactor; }
			set
			{
				SetNonPersistentPropertyValue(FP_TimeDelayFactorInfo, ref timeDelayFactor, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateFP_TimeDelayFactor();
				}
			}
		}

		ZDecimal timeDelayFactor;

		public ZPropertyInfo FP_TimeDelayFactorInfo
		{
			get { return GetZPropertyInfo(nameof(FP_TimeDelayFactor)); }
		}

		#endregion

		#region FP_TimeDelayMinutes

		public ZInt FP_TimeDelayMinutes
		{
			get { return timeDelayMinutes; }
			set
			{
				SetNonPersistentPropertyValue(FP_TimeDelayMinutesInfo, ref timeDelayMinutes, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateFP_TimeDelayMinutes();
				}
			}
		}

		ZInt timeDelayMinutes;

		public ZPropertyInfo FP_TimeDelayMinutesInfo
		{
			get { return GetZPropertyInfo(nameof(FP_TimeDelayMinutes)); }
		}

		#endregion

		#region FP_SynchroniseBufferPenetration

		public ZBool FP_SynchroniseBufferPenetration
		{
			get { return synchroniseBufferPenetration; }
			set { SetNonPersistentPropertyValue(FP_SynchroniseBufferPenetrationInfo, ref synchroniseBufferPenetration, value); }
		}

		ZBool synchroniseBufferPenetration;

		public ZPropertyInfo FP_SynchroniseBufferPenetrationInfo
		{
			get { return GetZPropertyInfo(nameof(FP_SynchroniseBufferPenetration)); }
		}

		#endregion

		#region FP_IsActive

		public ZBool FP_IsActive { get; set; }

		#endregion

		#region FP_SystemCreateTimeUtc

		public ZDateTime FP_SystemCreateTimeUtc { get; set; }

		#endregion

		#region FP_SystemCreateUser

		public ZString FP_SystemCreateUser { get; set; }

		#endregion

		#region FP_SystemLastEditTimeUtc

		public ZDateTime FP_SystemLastEditTimeUtc { get; set; }

		#endregion

		#region FP_SystemLastEditUser

		public ZString FP_SystemLastEditUser { get; set; }

		#endregion

		#endregion

		#region Properties

		[ResourceStringData("ProposedProcessHeaderLink.ActionDescription", Caption = "Action Description", FullDescription = "If this row is committed, this action will be performed.")]
		public ZString ActionDescription
		{
			get
			{
				if (!HasErrors)
				{
					var from = FromWorkflow;
					var to = ToWorkflow;

					if (from != null && to != null)
					{
						return Res.GetString("e1c1f267-3b37-4bfb-8ed1-d107a9ff2e69", "[{0}] will be made a {1} of [{2}]",
							from.FH_CompletionStatement.ToString().ShortenLineLengths(30),
							LinkTypeDescription,
							to.FH_CompletionStatement.ToString().ShortenLineLengths(30)
							);
					}
				}

				return string.Empty;
			}
		}

		string LinkTypeDescription
		{
			get
			{
				switch (FP_LinkType)
				{
					case ProcessHeaderLinkTypeList.Codes.Dependency:
						return Res.GetString("6d2e0bdd-b118-4bd2-99ad-e9fd7e5759b8", "pre-requisite");

					case ProcessHeaderLinkTypeList.Codes.ParentChild:
						return Res.GetString("15bf2e52-c862-4ff0-b9fc-18e8c2ce70ac", "child");

					default:
						throw new InvalidOperationException("Invalid value for FP_LinkType: " + FP_LinkType);
				}
			}
		}

		#endregion

		#region Related Business Objects

		public ProcessHeader FromWorkflow
		{
			get { return Factory.Load<ProcessHeader>(FP_FH_HeaderFrom); }
		}

		public ProcessHeader ToWorkflow
		{
			get { return Factory.Load<ProcessHeader>(FP_FH_HeaderTo); }
		}

		public ProcessHeaderCollection AllWorkflows
		{
			get
			{
				if (allWorkflows == null)
				{
					var query = fromJobHeader.GetAllProcessHeadersInJobQuery().AddToFilter(toJobHeader.GetAllProcessHeadersInJobQuery(), JoinCondition.Or);
					allWorkflows = new ProcessHeaderCollection(Factory, query);
				}

				return allWorkflows;
			}
		}

		ProcessHeaderCollection allWorkflows;

		#endregion

		#region NonPersistentBusinessObject Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			FP_IsActive = ZBool.True;
			FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;
		}

		public override ProposedProcessHeaderLinkValidation GetNewValidation()
		{
			return new ProposedProcessHeaderLinkValidation(this);
		}

		#endregion

		#region Implementation

		internal void EnsureRealLinkExists()
		{
			if (!Factory.Exists(typeof(ProcessHeaderLink), GetExistingLinkQuery(), mergeDbAndCacheResult: false))
			{
				var link = Factory.New<ProcessHeaderLink>();

				link.FP_LinkType = FP_LinkType;
				link.FP_FH_HeaderFrom = FP_FH_HeaderFrom;
				link.FP_FH_HeaderTo = FP_FH_HeaderTo;
				link.FP_TimeDelayFactor = FP_TimeDelayFactor;
				link.FP_TimeDelayMinutes = FP_TimeDelayMinutes;
				link.FP_SynchroniseBufferPenetration = FP_SynchroniseBufferPenetration;
				link.FP_IsActive = ZBool.True;

				var regStrings = BMSRegistry.Instance.DebuggingStringsForProcessHeaderLinks.Value;
				if (regStrings != BMSRegistry.Instance.DebuggingStringsForProcessHeaderLinks.DefaultValue && regStrings.All(ActionDescription.ToString().Contains))
				{
					link.DefectFixLog = Res.GetString("970180D2-1B50-43F5-B677-B901F05EB9CC", "Thread {0}, {1}, Header From: {2}, Header To: {3}, Link Type: {4}, Description: {5}, Stack Trace: \r\n{6}",
						System.Threading.Thread.CurrentThread.ManagedThreadId.ToString(),
						ZDateTime.Now.ToString(),
						FP_FH_HeaderFrom,
						FP_FH_HeaderTo,
						FP_LinkType,
						ActionDescription,
						System.Environment.StackTrace + "\r\n");
				}
			}
		}

		ZQuery GetExistingLinkQuery()
		{
			return new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderFrom, FP_FH_HeaderFrom)
				.AddToFilter(ProcessHeaderLinkSchema.FP_FH_HeaderTo, FP_FH_HeaderTo)
				.AddToFilter(ProcessHeaderLinkSchema.FP_LinkType, FP_LinkType);
		}

		internal static bool TryCreateFromTemplateLink(ProcessHeaderLink templateLink, ProcessJobHeader fromJobHeader, ProcessJobHeader toJobHeader, out ProposedProcessHeaderLink proposedLink)
		{
			proposedLink = null;

			var fromWorkflow = GetRealWorkflow(templateLink.HeaderFrom, fromJobHeader);
			var toWorkflow = GetRealWorkflow(templateLink.HeaderTo, toJobHeader);

			if (fromWorkflow != null && toWorkflow != null)
			{
				var linkAlreadyExistsInFactory = fromWorkflow.LinksFromMeToOthers.Any(l => l.FP_FH_HeaderTo == toWorkflow.PK);

				if (!linkAlreadyExistsInFactory)
				{
					proposedLink = new ProposedProcessHeaderLink(fromJobHeader, toJobHeader);

					proposedLink.FP_FH_HeaderFrom = fromWorkflow.PK;
					proposedLink.FP_FH_HeaderTo = toWorkflow.PK;

					proposedLink.FP_LinkType = templateLink.FP_LinkType;
					proposedLink.FP_SynchroniseBufferPenetration = templateLink.FP_SynchroniseBufferPenetration;
					proposedLink.FP_TimeDelayFactor = templateLink.FP_TimeDelayFactor;
					proposedLink.FP_TimeDelayMinutes = templateLink.FP_TimeDelayMinutes;
				}
			}

			return proposedLink != null;
		}

		static ProcessHeader GetRealWorkflow(ProcessHeader templateWorkflow, ProcessJobHeader jobHeader)
		{
			if (templateWorkflow.IsWorkflow)
			{
				return jobHeader.ProcessHeaders.FirstOrDefault(w => DoesTemplateWorkflowMatchJobWorkflow(w, templateWorkflow));
			}
			else
			{
				return jobHeader;
			}
		}

		static bool DoesTemplateWorkflowMatchJobWorkflow(ProcessHeader jobWorkflow, ProcessHeader templateWorkflow)
		{
			if (jobWorkflow.FH_CompletionStatement == templateWorkflow.FH_CompletionStatement)
			{
				return true;
			}
			else
			{
				// This is here because many support incident workflows have an event code prefix in their FH_CompletionStatement (workflows that respond to criticality changes etc).
				// This code should be removed if and when the 'incident event' hack- I mean feature, is replaced by using core Workflow functionality.

				return templateWorkflow.FH_CompletionStatement.Contains(jobWorkflow.FH_CompletionStatement, StringComparison.Ordinal) && templateWorkflow.PK == jobWorkflow.FH_ParentTemplateId;
			}
		}

		#endregion
	}
}
