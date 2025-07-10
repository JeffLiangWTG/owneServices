using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.Business
{
	public class ReleaseGateRequest : IWorkflowOrderable
	{
		public ReleaseGateRequest(IProcessHeader workflow, IBMComponentLink componentLink)
		{
			Argument.NotNull(workflow, "workflow");
			Argument.NotNull(componentLink, nameof(componentLink));

			ComponentLink = componentLink;

			if (!FromComponentPK.IsValid)
			{
				throw new ArgumentException("fromComponentPK was invalid");
			}
			if (!BufferPK.IsValid)
			{
				throw new ArgumentException("bufferPK was invalid");
			}

			WorkflowPK = workflow.PK;
			WorkflowParentTableCode = workflow.FH_ParentTableCode;
			ReleaseGroupPK = workflow.FH_GG_ReleaseGroup;

			EffectiveNudge = workflow.EffectiveNudge;
			AgreedDeliveryDate = workflow.ApplicableAgreedDeliveryDateUtc;
			ReleaseSequenceSortDate = workflow.ReleaseSequenceSortDate;
			CreateTime = workflow.FH_SystemCreateTimeUtc;
			ReleaseSequence = workflow.ReleaseSequence;
		}

		public ZGuid WorkflowPK { get; private set; }
		public ZString WorkflowParentTableCode { get; private set; }
		public ZGuid FromComponentPK => ComponentLink.FL_FC_ComponentFrom;
		public ZGuid BufferPK => ComponentLink.FL_FC_ComponentTo;
		public ZGuid ReleaseGroupPK { get; private set; }

		internal ViewProcessHeader Workflow { get; set; }
		internal IBMComponentLink ComponentLink { get; }

		internal Dictionary<BMComponentAcceptabilityBand, AcceptabilityBandResult> GoldenRuleImpact
		{
			get { return goldenRuleImpact ?? (goldenRuleImpact = new Dictionary<BMComponentAcceptabilityBand, AcceptabilityBandResult>()); }
		}

		Dictionary<BMComponentAcceptabilityBand, AcceptabilityBandResult> goldenRuleImpact;

		#region Equals

		public override bool Equals(object obj)
		{
			var other = obj as ReleaseGateRequest;
			return !ReferenceEquals(other, null)
				&& other.WorkflowPK == WorkflowPK
				&& other.BufferPK == BufferPK;
		}

		public override int GetHashCode()
		{
			return WorkflowPK.GetHashCode() ^ BufferPK.GetHashCode();
		}

		public static bool operator ==(ReleaseGateRequest r1, ReleaseGateRequest r2)
		{
			var isNull1 = ReferenceEquals(r1, null);
			var isNull2 = ReferenceEquals(r2, null);

			if (isNull1 || isNull2)
			{
				return isNull1 && isNull2;
			}

			return r1.WorkflowPK == r2.WorkflowPK
				&& r1.FromComponentPK == r2.FromComponentPK
				&& r1.BufferPK == r2.BufferPK;
		}

		public static bool operator !=(ReleaseGateRequest r1, ReleaseGateRequest r2)
		{
			return !(r1 == r2);
		}

		#endregion

		#region IIdentified Members

		ZGuid IIdentified.Identifier
		{
			get { return WorkflowPK; }
		}

		#endregion

		#region Workflow Sorting

		public ZString ReleaseSequence { get; private set; }
		public ZDecimal EffectiveNudge { get; set; }
		public ZDateTime AgreedDeliveryDate { get; private set; }
		public ZDateTime ReleaseSequenceSortDate { get; private set; }
		public ZDateTime CreateTime { get; private set; }

		ZDateTime IWorkflowOrderable.ReleaseDateTime
		{
			get { throw new InvalidOperationException("Why are you ordering an unreleased item by the release time?"); }
		}

		#endregion
	}
}
