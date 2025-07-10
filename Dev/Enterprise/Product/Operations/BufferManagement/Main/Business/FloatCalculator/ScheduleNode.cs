using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	[DebuggerDisplay("Name: {Entity.DisplayName}")]
	public class ScheduleNode : IIdentified
	{
#if DEBUG
		public
#else
		protected internal
#endif
		ScheduleNode(ScheduleGraph network, ILinkEntity entity)
		{
			this.network = network;
			Entity = entity;
		}

		readonly ScheduleGraph network;

		public void AddPrerequisites(params ScheduleNode[] prereqRange)
		{
			foreach (var prereq in prereqRange)
			{
				if (!network.Graph.ContainsEdge(prereq, this))
				{
					network.Graph.AddEdge(new EdgeWithOffset(prereq, this));
				}
			}
		}

		public void AddPostrequisites(params ScheduleNode[] postreqRange)
		{
			foreach (var postreq in postreqRange)
			{
				if (!network.Graph.ContainsEdge(this, postreq))
				{
					network.Graph.AddEdge(new EdgeWithOffset(this, postreq));
				}
			}
		}

		public IEnumerable<ScheduleNode> GetPrerequisites(PassDirection direction = PassDirection.All)
		{
			return GetPrerequisiteEdges(direction).Select(s => s.Source);
		}

		public IEnumerable<ScheduleNode> GetPostrequisites(PassDirection direction = PassDirection.All)
		{
			return GetPostrequisiteEdges(direction).Select(s => s.Target);
		}

		public IEnumerable<EdgeWithOffset> GetPrerequisiteEdges(PassDirection direction)
		{
			IEnumerable<EdgeWithOffset> edges;
			if (network?.Graph != null && network.Graph.TryGetInEdges(this, out edges))
			{
				return FilterPassDirection(edges, direction);
			}
			else
			{
				return Enumerable.Empty<EdgeWithOffset>();
			}
		}

		public IEnumerable<EdgeWithOffset> GetPostrequisiteEdges(PassDirection direction)
		{
			IEnumerable<EdgeWithOffset> edges;
			if (network?.Graph != null && network.Graph.TryGetOutEdges(this, out edges))
			{
				return FilterPassDirection(edges, direction);
			}
			else
			{
				return Enumerable.Empty<EdgeWithOffset>();
			}
		}

		static IEnumerable<EdgeWithOffset> FilterPassDirection(IEnumerable<EdgeWithOffset> edges, PassDirection direction)
		{
			switch (direction)
			{
				case PassDirection.All:
					return edges;
				case PassDirection.Early:
					return edges.Where(e => e.PassDirection != PassDirection.Late);
				case PassDirection.Late:
					return edges.Where(e => e.PassDirection != PassDirection.Early);
				case PassDirection.None:
					return edges.Where(e => e.PassDirection != PassDirection.Early & e.PassDirection != PassDirection.Late);
				default:
					throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Invalid Direction: {0}", direction));
			}
		}

		public IEnumerable<ScheduleNode> Ancestors
		{
			get
			{
				var currentNode = this;
				while (currentNode.Parent != null)
				{
					yield return currentNode.Parent;
					currentNode = currentNode.Parent;
				}
			}
		}

		public ScheduleNode Parent
		{
			get { return parent; }
			set
			{
				if (parent != null)
				{
					throw new InvalidOperationException("Schedules cannot have more than one parent.");
				}
				parent = value;
				parent.AddChildren(this);
			}
		}

		ScheduleNode parent;

		void AddChildren(params ScheduleNode[] childrenRange)
		{
			children.AddRange(childrenRange);
		}

		public IEnumerable<ScheduleNode> Children
		{
			get { return children; }
		}
		readonly List<ScheduleNode> children = new List<ScheduleNode>();

		public IEnumerable<ScheduleNode> Descendants
		{
			get
			{
				foreach (var child in children)
				{
					yield return child;
					foreach (var descendant in child.Descendants)
					{
						yield return descendant;
					}
				}
			}
		}

		public Dictionary<ScheduleNode, decimal> DescendantEarlyStartOffsets
		{
			get { return descendantEarlyStartOffsets ?? (descendantEarlyStartOffsets = new Dictionary<ScheduleNode, decimal>()); }
		}
		Dictionary<ScheduleNode, decimal> descendantEarlyStartOffsets;

		public Dictionary<ScheduleNode, decimal> DescendantLateFinishOffsets
		{
			get { return descendantLateFinishOffsets ?? (descendantLateFinishOffsets = new Dictionary<ScheduleNode, decimal>()); }
		}
		Dictionary<ScheduleNode, decimal> descendantLateFinishOffsets;

		public ILinkEntity Entity { get; }
		public DeliveryDateThreat DeliveryDateThreat { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public decimal EarliestStartHours { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public decimal LatestStartHours { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public decimal EarliestFinishHours
		{
			get { return EarliestStartHours + EstimatedDurationHoursIncludingChildren; }
			set { EarliestStartHours = value - EstimatedDurationHoursIncludingChildren; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public decimal LatestFinishHours
		{
			get { return LatestStartHours + EstimatedDurationHoursIncludingChildren; }
			set { LatestStartHours = value - EstimatedDurationHoursIncludingChildren; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public decimal FloatHours
		{
			get { return LatestStartHours - EarliestStartHours; }
		}

		public ZDateTime EarliestStartTimeUtc { get; set; }
		public ZDateTime EarliestFinishTimeUtc { get; set; }
		public ZDateTime LatestStartTimeUtc { get; set; }
		public ZDateTime LatestFinishTimeUtc { get; set; }

		public ZDateTime AgreedDeliveryDateInUtc { get; set; }

		public bool IsCriticalPath { get; set; }
		public bool IsCyclic { get; set; }
		public bool IsScheduleApplicableToCriticalChainDuration { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public decimal EstimatedDurationHoursIncludingChildren { get; set; }

		public int Depth { get; set; }

		public ZGuid Identifier
		{
			get { return Entity != null ? Entity.PK : ZGuid.Empty; }
		}

		public bool IsLeaf
		{
			get { return !Children.Any(); }
		}

		public ScheduleNodePin Pin
		{
			get { return pin; }
			set
			{
				if (pin != null)
				{
					throw new InvalidOperationException("A pin already exists for this entity.");
				}
				pin = value;

				var ancestor = Parent;
				while (pin != null && ancestor != null && ancestor != pin.Owner)
				{
					ancestor.DescendantPins.Add(pin);
					ancestor = ancestor.Parent;
				}
			}
		}

		ScheduleNodePin pin;

		internal readonly List<ScheduleNodePin> DescendantPins = new List<ScheduleNodePin>();

		internal readonly List<decimal> CandidateEarlyStarts = new List<decimal>();
		internal readonly List<decimal> CandidateLateFinishes = new List<decimal>();

		internal int NumberOfRequisiteLateFinishes
		{
			get { return GetPostrequisiteEdges(PassDirection.Late).Count() + Ancestors.Count(); }
		}

		internal int NumberOfRequisiteEarlyStarts
		{
			get { return GetPrerequisiteEdges(PassDirection.Early).Count() + Ancestors.Count(); }
		}

		public bool HasAllCandidateEarlyStarts
		{
			get { return NumberOfRequisiteEarlyStarts <= CandidateEarlyStarts.Count; }
		}

		public bool HasAllCandidateLateFinishes
		{
			get { return NumberOfRequisiteLateFinishes <= CandidateLateFinishes.Count; }
		}

		public bool HasTooManyCandidateEarlyStarts
		{
			get { return NumberOfRequisiteEarlyStarts < CandidateEarlyStarts.Count; }
		}

		public bool HasTooManyCandidateLateFinishes
		{
			get { return NumberOfRequisiteLateFinishes < CandidateLateFinishes.Count; }
		}

		public bool SchedulingInfoIsEqual(ScheduleNode other)
		{
			return other != null
				&& other.Entity == Entity
				&& other.DeliveryDateThreat == DeliveryDateThreat
				&& other.EarliestStartHours == EarliestStartHours
				&& other.LatestStartHours == LatestStartHours
				&& other.EarliestStartTimeUtc == EarliestStartTimeUtc
				&& other.EarliestFinishTimeUtc == EarliestFinishTimeUtc
				&& other.LatestStartTimeUtc == LatestStartTimeUtc
				&& other.LatestFinishTimeUtc == LatestFinishTimeUtc
				&& other.AgreedDeliveryDateInUtc == AgreedDeliveryDateInUtc
				&& other.IsCriticalPath == IsCriticalPath
				&& other.IsCyclic == IsCyclic
				&& other.IsScheduleApplicableToCriticalChainDuration == IsScheduleApplicableToCriticalChainDuration
				&& other.EstimatedDurationHoursIncludingChildren == EstimatedDurationHoursIncludingChildren
				&& other.Depth == Depth;
		}
	}
}
