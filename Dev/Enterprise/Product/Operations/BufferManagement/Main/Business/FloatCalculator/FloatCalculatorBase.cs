using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using QuikGraph;

namespace Enterprise.BufferManagement.Business
{
	public abstract class FloatCalculatorBase
	{
		protected FloatCalculatorBase(ScheduleNode root, ScheduleGraph network, WorkingTimeContext context, BusinessObjectFactory factory)
		{
			this.root = root;
			this.network = network;
			this.context = context ?? WorkingTimeContext.Create(factory);
			this.factory = factory;
		}

		readonly ScheduleNode root;
		readonly ScheduleGraph network;
		readonly WorkingTimeContext context;
		readonly BusinessObjectFactory factory;

		bool hasBeenCalculated;

		protected Dictionary<ZGuid, ScheduleNode> SchedulesByEntity
		{
			get { return network.SchedulesByEntity; }
		}

		protected WorkingTimeContext Context
		{
			get { return context; }
		}

		protected BusinessObjectFactory Factory
		{
			get { return factory; }
		}

		public ScheduleNode GetSchedule(ILinkEntity entity)
		{
			if (!hasBeenCalculated)
			{
				Calculate();
			}

			return SchedulesByEntity.ContainsKey(entity.PK) ? SchedulesByEntity[entity.PK] : null;
		}

		public void Calculate()
		{
			var cyclicVertexes = QuickGraphUtils.GetCyclicVertexes(network.Graph);

			if (!cyclicVertexes.Any())
			{
				PopulateScheduleTimes();
			}
			else
			{
				MarkCyclicVertexes(cyclicVertexes);
			}

			hasBeenCalculated = true;
		}

		void PopulateScheduleTimes()
		{
			var crossDependencyEdges = DoCrossDependecyPass();

			SetClosestHours(PassDirection.Early, root, 0m);
			var startingSchedules = SchedulesByEntity.Values.Where(n => n.HasAllCandidateEarlyStarts);
			DoPass(PassDirection.Early, startingSchedules.Where(s => s.Parent != null), SchedulesByEntity.Values.Where(s => s.Parent != null), crossDependencyEdges);

			EstimateRootDuration(SchedulesByEntity.Values);
			SetClosestHours(PassDirection.Late, root, root.EstimatedDurationHoursIncludingChildren);
			var endpointSchedules = SchedulesByEntity.Values.Where(n => n.HasAllCandidateLateFinishes).ToArray();

			var thingsRemoved = DoPass(PassDirection.Late, endpointSchedules.Where(s => s.Parent != null), SchedulesByEntity.Values.Where(s => s.Parent != null), crossDependencyEdges);

			if (thingsRemoved)
			{
				endpointSchedules = SchedulesByEntity.Values.Where(n => n.NumberOfRequisiteLateFinishes == 1).ToArray();
			}

			DoCriticalPathPass(endpointSchedules.ToArray());
			DoAgreedDeliveryDatePass(factory, context);
			DoStartAndEndTimesPass();
		}

		#region Cross Dependency Pass

		List<EdgeWithOffset> DoCrossDependecyPass()
		{
			var crossDependencies = new List<EdgeWithOffset>();

			foreach (var schedule in network.SchedulesByEntity.Values.Where(s => !s.IsLeaf).OrderByDescending(s => s.Depth))
			{
				CalculateSubDiagramOffsets(schedule);
			}

			foreach (var edge in network.CrossBoundaryDependencies)
			{
				var commonAncestors = FindPathToCommonAncestor(edge.Source, edge.Target);

				foreach (var direction in new[] { PassDirection.Early, PassDirection.Late })
				{
					foreach (var newEdge in FindBackPropagationEdges(direction, edge, commonAncestors))
					{
						if (!network.Graph.Edges.Contains(newEdge))
						{
							network.Graph.AddEdge(newEdge);
							crossDependencies.Add(newEdge);
						}
					}
				}
			}

			return crossDependencies;
		}

		static Tuple<List<ScheduleNode>, List<ScheduleNode>, ScheduleNode> FindPathToCommonAncestor(ScheduleNode from, ScheduleNode to)
		{
			var fromAncestors = new List<ScheduleNode>();
			var toAncestors = new List<ScheduleNode>();
			ScheduleNode commonAncestor = null;

			foreach (var zippedPair in from.Ancestors.ZipWithNull(to.Ancestors, (fromAncestor, toAncestor) => new { FromAncestor = fromAncestor, ToAncestor = toAncestor }))
			{
				fromAncestors.Add(zippedPair.FromAncestor);
				toAncestors.Add(zippedPair.ToAncestor);

				var index = toAncestors.IndexOf(zippedPair.FromAncestor);

				if (index >= 0)
				{
					commonAncestor = toAncestors[index];
					toAncestors.RemoveAfterIndex(index);
					break;
				}
				else
				{
					index = fromAncestors.IndexOf(zippedPair.ToAncestor);

					if (index >= 0)
					{
						fromAncestors.RemoveAfterIndex(index);
						commonAncestor = fromAncestors[index];
						break;
					}
				}
			}

			fromAncestors.RemoveAll(n => n == null);
			toAncestors.RemoveAll(n => n == null);

			if (fromAncestors.Count > 0)
			{
				fromAncestors.RemoveAt(fromAncestors.Count - 1);
			}

			if (toAncestors.Count > 0)
			{
				toAncestors.RemoveAt(toAncestors.Count - 1);
			}

			return Tuple.Create(fromAncestors, toAncestors, commonAncestor);
		}

		/// <summary>
		/// This is the most annoying function in this entire algorithm. It creates back-in-time edges for Cross Over dependencies.
		/// </summary>
		static IEnumerable<EdgeWithOffset> FindBackPropagationEdges(PassDirection direction, EdgeWithOffset edge, Tuple<List<ScheduleNode>, List<ScheduleNode>, ScheduleNode> commonAncestors)
		{
			var edges = new List<EdgeWithOffset>();

			var prereq = GetPrereq(direction, edge);
			var postreq = GetPostreq(direction, edge);

			var furthestAncestors = GetFurthestAncestors(direction, commonAncestors);
			var furthestAncestorSet = new HashSet<ScheduleNode>(furthestAncestors);
			var chains = prereq.FilteredTraversalWithDepth(t => GetPrerequisites(direction, t), (node, chain) => true, (node, chain) => EntityHasAncestor(furthestAncestorSet, node), fullChainsOnly: true);

			var intermediaryShapes = new HashSet<ScheduleNode>(chains.Where(c => EntityHasAncestor(furthestAncestorSet, c[c.Length - 1])).SelectMany(c => c));

			foreach (var chain in chains.Where(c => !EntityHasAncestor(furthestAncestorSet, c[c.Length - 1])))
			{
				var offset = 0m;
				var lastNode = postreq;
				foreach (var node in furthestAncestors)
				{
					offset += GetOffsetFromAncestor(direction, node, lastNode);
					lastNode = node;
				}

				if (offset >= 0)
				{
					var propagator = GetPropagator(intermediaryShapes, chain);

					switch (direction)
					{
						case PassDirection.Early:
							if (furthestAncestors.Any())
							{
								edges.Add(new EdgeWithOffset(propagator.Item1, furthestAncestors.Last()) { Offset = offset - propagator.Item2, PassDirection = direction });
							}
							break;

						case PassDirection.Late:
							if (furthestAncestors.Any())
							{
								edges.Add(new EdgeWithOffset(furthestAncestors.Last(), propagator.Item1) { Offset = offset - propagator.Item2, PassDirection = direction });
							}
							break;

						default:
							throw direction.CreateNotSupportedException();
					}
				}
			}

			return edges;
		}

		static Tuple<ScheduleNode, decimal> GetPropagator(HashSet<ScheduleNode> intermediaryShapes, ScheduleNode[] chain)
		{
			var chainDuration = 0m;
			int i = 0;
			while (intermediaryShapes.Contains(chain[i]))
			{
				chainDuration += chain[i].EstimatedDurationHoursIncludingChildren;
				i++;
			}

			return Tuple.Create(chain[i], chainDuration);
		}

		static IEnumerable<ScheduleNode> GetPrerequisites(PassDirection direction, ScheduleNode node)
		{
			switch (direction)
			{
				case PassDirection.Early:
					return node.GetPrerequisites(PassDirection.None);

				case PassDirection.Late:
					return node.GetPostrequisites(PassDirection.None);

				default:
					throw direction.CreateNotSupportedException();
			}
		}

		static bool EntityHasAncestor(HashSet<ScheduleNode> ancestors, ScheduleNode node)
		{
			return node.Ancestors.Any(n => ancestors.Contains(n));
		}

		static IEnumerable<ScheduleNode> GetFurthestAncestors(PassDirection direction, Tuple<List<ScheduleNode>, List<ScheduleNode>, ScheduleNode> commonAncestors)
		{
			switch (direction)
			{
				case PassDirection.Early:
					return commonAncestors.Item2;

				case PassDirection.Late:
					return commonAncestors.Item1;

				default:
					throw direction.CreateNotSupportedException();
			}
		}

		void CalculateSubDiagramOffsets(ScheduleNode schedule)
		{
			var children = new HashSet<ScheduleNode>(schedule.Children);
			var subNetwork = MakeSubNetwork(children);

			var startNodes = children.Where(c => subNetwork.IsInEdgesEmpty(c));
			var endNodes = children.Where(c => subNetwork.IsOutEdgesEmpty(c));

			foreach (var startNode in startNodes)
			{
				DoEarlyStartOffsetPass(schedule, subNetwork, startNode);
			}

			foreach (var endNode in endNodes)
			{
				DoLateFinishOffsetPass(schedule, subNetwork, endNode);
			}
		}

		void DoLateFinishOffsetPass(ScheduleNode localRoot, BidirectionalGraph<ScheduleNode, EdgeWithOffset> subNetwork, ScheduleNode schedule, decimal offset = 0)
		{
			decimal currentLateFinish;
			localRoot.DescendantLateFinishOffsets.TryGetValue(schedule, out currentLateFinish);
			currentLateFinish = Math.Max(offset, currentLateFinish);
			localRoot.DescendantLateFinishOffsets[schedule] = currentLateFinish;

			IEnumerable<EdgeWithOffset> prereqEdges;

			if (subNetwork.TryGetInEdges(schedule, out prereqEdges))
			{
				foreach (var edge in prereqEdges)
				{
					var postreq = edge.Source;
					DoLateFinishOffsetPass(localRoot, subNetwork, postreq, currentLateFinish + schedule.EstimatedDurationHoursIncludingChildren - edge.Offset);
				}
			}
		}

		void DoEarlyStartOffsetPass(ScheduleNode localRoot, BidirectionalGraph<ScheduleNode, EdgeWithOffset> subNetwork, ScheduleNode schedule, decimal offset = 0)
		{
			decimal currentEarlyStart;
			localRoot.DescendantEarlyStartOffsets.TryGetValue(schedule, out currentEarlyStart);
			currentEarlyStart = Math.Max(offset, currentEarlyStart);
			localRoot.DescendantEarlyStartOffsets[schedule] = currentEarlyStart;

			IEnumerable<EdgeWithOffset> postreqEdges;

			if (subNetwork.TryGetOutEdges(schedule, out postreqEdges))
			{
				foreach (var edge in postreqEdges)
				{
					var postreq = edge.Target;
					DoEarlyStartOffsetPass(localRoot, subNetwork, postreq, currentEarlyStart + schedule.EstimatedDurationHoursIncludingChildren - edge.Offset);
				}
			}
		}

		static BidirectionalGraph<ScheduleNode, EdgeWithOffset> MakeSubNetwork(HashSet<ScheduleNode> children)
		{
			var graph = new BidirectionalGraph<ScheduleNode, EdgeWithOffset>();
			graph.AddVertexRange(children);

			var descendants = new HashSet<ScheduleNode>(children.SelectMany(s => s.SelectRecursive(n => n.Children)));

			foreach (var schedule in children)
			{
				AddVertexesThatPointToOtherChildren(graph, schedule, children, descendants);
			}

			return graph;
		}

		static void AddVertexesThatPointToOtherChildren(BidirectionalGraph<ScheduleNode, EdgeWithOffset> subGraph, ScheduleNode schedule, HashSet<ScheduleNode> children, HashSet<ScheduleNode> descendants)
		{
			var chains = schedule.FilteredTraversalWithDepth(t => t.GetPostrequisites(), (node, chain) => children.Contains(node) || descendants.Contains(node), (node, chain) => false);
			foreach (var chain in chains)
			{
				for (int i = 1; i < chain.Length; i++)
				{
					var node1 = chain[i - 1];
					var node2 = chain[i];

					if (!subGraph.ContainsVertex(node2))
					{
						subGraph.AddVertex(node2);
					}

					EdgeWithOffset existingEdge;
					if (!subGraph.TryGetEdge(node1, node2, out existingEdge))
					{
						existingEdge = new EdgeWithOffset(node1, node2);
						if (descendants.Contains(node2))
						{
							existingEdge.Offset = GetEarlyStartOffsetModifier(node2, children);
						}
						subGraph.AddEdge(existingEdge);
					}
				}
			}
		}

		static decimal GetEarlyStartOffsetModifier(ScheduleNode node, HashSet<ScheduleNode> children)
		{
			var offsetSum = 0m;
			var currentNode = node;
			var parent = currentNode.Parent;

			while (!children.Contains(currentNode))
			{
				offsetSum += parent.DescendantEarlyStartOffsets[currentNode];

				currentNode = parent;
				parent = parent.Parent;
			}

			return offsetSum;
		}

		void EstimateRootDuration(IEnumerable<ScheduleNode> endpointSchedules)
		{
			if (root.EstimatedDurationHoursIncludingChildren == 0)
			{
				var latestChild = endpointSchedules.MaxBySafe(e => e.EarliestFinishHours);

				if (latestChild != null)
				{
					root.EstimatedDurationHoursIncludingChildren = latestChild.EarliestFinishHours;

					root.EarliestStartHours = 0;
					root.LatestStartHours = 0;
					root.EarliestFinishHours = root.EstimatedDurationHoursIncludingChildren;
					root.LatestFinishHours = root.EarliestFinishHours;
				}
			}
		}

		#endregion

		#region Early Start/LatestFinish Pass

		bool DoPass(PassDirection direction, IEnumerable<ScheduleNode> startingSchedules, IEnumerable<ScheduleNode> allSchedules, List<EdgeWithOffset> crossDependencyEdges)
		{
			var unScheduled = new HashSet<ScheduleNode>(allSchedules);
			var passQueue = new Queue<ScheduleNode>(startingSchedules);
			var thingsWereRemoved = false;

			while (unScheduled.Count > 0)
			{
				while (passQueue.Count > 0)
				{
					var node = passQueue.Dequeue();
					var modifiedCandidates = new List<ScheduleNode>();

					var closestHours = GetBestCandidateClosestHours(direction, node);
					modifiedCandidates.AddRange(SetClosestHours(direction, node, closestHours));

					foreach (var edge in GetFurtherEdges(direction, node))
					{
						modifiedCandidates.Add(AddCandidateClosestHours(direction, edge));
					}

					EnqueueModifiedCandidate(direction, passQueue, modifiedCandidates, unScheduled);

					unScheduled.Remove(node);
				}

				if (unScheduled.Count == 0)
				{
					continue;
				}

				if (crossDependencyEdges.Count == 0)
				{
					return thingsWereRemoved;
				}

				// This happens if two Cross Heirarchy Dependencies are creating an inescapable loop.
				// All we have to do here is break the loop by finding a one of these dependencies and delete it.
				// Everything ought to just magically work out because by this point the parent of the cross heirarchical dependency
				// Should already have offset from any external postrequisites.

				// We have to find the *LEAST USEFUL* backpropagation edge. This is the edge that will not cause the float to change.
				var breakingDependency = direction == PassDirection.Early
					? GetBreakingBackPropagationEdge(direction, unScheduled, crossDependencyEdges).MaxBySafe(edge => edge.Target.CandidateEarlyStarts.Max() + edge.Offset)
					: GetBreakingBackPropagationEdge(direction, unScheduled, crossDependencyEdges).MaxBySafe(edge => Math.Abs(-edge.Source.CandidateLateFinishes.Min() - edge.Offset));

				if (breakingDependency == null)
				{
					return thingsWereRemoved;
				}

				var postreq = GetPostreq(direction, breakingDependency);
				network.Graph.RemoveEdge(breakingDependency);
				crossDependencyEdges.Remove(breakingDependency);

				EnqueueModifiedCandidate(direction, passQueue, new[] { postreq }, unScheduled);
				thingsWereRemoved = true;
			}

			return thingsWereRemoved;
		}

		IEnumerable<EdgeWithOffset> GetBreakingBackPropagationEdge(PassDirection direction, HashSet<ScheduleNode> unScheduled, IEnumerable<EdgeWithOffset> edges)
		{
			switch (direction)
			{
				case PassDirection.Early:
					foreach (var edge in edges.Where(e => e.PassDirection == direction))
					{
						if (edge.Target.CandidateEarlyStarts.Count < edge.Target.NumberOfRequisiteEarlyStarts)
						{
							yield return edge;
						}
					}
					break;

				case PassDirection.Late:
					foreach (var edge in edges.Where(e => e.PassDirection == direction))
					{
						if (edge.Source.CandidateLateFinishes.Count < edge.Source.NumberOfRequisiteLateFinishes)
						{
							yield return edge;
						}
					}
					break;

				default:
					throw direction.CreateNotSupportedException();
			}
		}

		static void EnqueueModifiedCandidate(PassDirection direction, Queue<ScheduleNode> passQueue, IEnumerable<ScheduleNode> modifiedCandidates, HashSet<ScheduleNode> unScheduled)
		{
			foreach (var modifiedCandidate in modifiedCandidates.Distinct())
			{
				if (modifiedCandidate.Parent != null && HasAllCandidateClosestHours(direction, modifiedCandidate))
				{
					if (unScheduled.Contains(modifiedCandidate))
					{
						passQueue.Enqueue(modifiedCandidate);
					}
				}
			}
		}

		#region Check if dates can be chosen for nodes yet.

		static bool HasAllCandidateClosestHours(PassDirection direction, ScheduleNode node)
		{
			switch (direction)
			{
				case PassDirection.Early:
					return node.HasAllCandidateEarlyStarts;

				case PassDirection.Late:
					return node.HasAllCandidateLateFinishes;

				default:
					throw direction.CreateNotSupportedException();
			}
		}

		#endregion

		#region Modify State on Schedule Nodes

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		static IEnumerable<ScheduleNode> SetClosestHours(PassDirection direction, ScheduleNode node, decimal currentClosestHours)
		{
			var modifiedEntities = new List<ScheduleNode>();
			switch (direction)
			{
				case PassDirection.Early:
					node.EarliestStartHours = currentClosestHours;
					foreach (var child in node.Descendants)
					{
						child.CandidateEarlyStarts.Add(currentClosestHours);
						modifiedEntities.Add(child);
					}
					break;

				case PassDirection.Late:
					node.LatestFinishHours = currentClosestHours;
					foreach (var child in node.Descendants)
					{
						child.CandidateLateFinishes.Add(currentClosestHours);
						modifiedEntities.Add(child);
					}
					break;

				default:
					throw direction.CreateNotSupportedException();
			}

			return modifiedEntities;
		}

		static ScheduleNode AddCandidateClosestHours(PassDirection direction, EdgeWithOffset edge)
		{
			var prereq = GetPrereq(direction, edge);
			var postreq = GetPostreq(direction, edge);

			AddCandidateClosestHoursToNode(direction, prereq, postreq, edge.Offset);

			return postreq;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		static void AddCandidateClosestHoursToNode(PassDirection direction, ScheduleNode prereq, ScheduleNode postreq, decimal offset)
		{
			switch (direction)
			{
				case PassDirection.Early:
					var earlyStart = prereq.EarliestFinishHours - offset;
					postreq.CandidateEarlyStarts.Add(earlyStart);
					break;

				case PassDirection.Late:
					var lateFinish = prereq.LatestStartHours + offset;
					postreq.CandidateLateFinishes.Add(lateFinish);
					break;

				default:
					throw direction.CreateNotSupportedException();
			}
		}

		#endregion

		#region Get Pass Oriented State

		static ScheduleNode GetPrereq(PassDirection direction, EdgeWithOffset edge)
		{
			switch (direction)
			{
				case PassDirection.Early:
					return edge.Source;

				case PassDirection.Late:
					return edge.Target;

				default:
					throw direction.CreateNotSupportedException();
			}
		}

		static ScheduleNode GetPostreq(PassDirection direction, EdgeWithOffset edge)
		{
			switch (direction)
			{
				case PassDirection.Early:
					return edge.Target;

				case PassDirection.Late:
					return edge.Source;

				default:
					throw direction.CreateNotSupportedException();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		decimal GetBestCandidateClosestHours(PassDirection direction, ScheduleNode node)
		{
			var pin = node.Pin;

			switch (direction)
			{
				case PassDirection.Early:
					if (pin != null)
					{
						return pin.Owner.EarliestStartHours + pin.Offset;
					}
					else
					{
						return node.DescendantPins.Select(p => p.Owner.EarliestStartHours + p.Offset - GetOffsetFromAncestor(direction, node, p.Descendant))
							.Concat(node.CandidateEarlyStarts)
							.Append(node.Parent.EarliestStartHours).Max();
					}

				case PassDirection.Late:
					if (pin != null)
					{
						return pin.Owner.LatestStartHours + pin.Offset + node.EstimatedDurationHoursIncludingChildren;
					}
					else
					{
						return node.DescendantPins.Select(p => p.Owner.LatestFinishHours - p.Offset + GetOffsetFromAncestor(direction, node, p.Descendant))
							.Concat(node.CandidateLateFinishes)
							.Append(node.Parent.LatestFinishHours, root.EstimatedDurationHoursIncludingChildren).Min();
					}

				default:
					throw direction.CreateNotSupportedException();
			}
		}

		static IEnumerable<EdgeWithOffset> GetFurtherEdges(PassDirection direction, ScheduleNode node)
		{
			switch (direction)
			{
				case PassDirection.Early:
					return node.GetPostrequisiteEdges(PassDirection.Early);

				case PassDirection.Late:
					return node.GetPrerequisiteEdges(PassDirection.Late);

				default:
					throw direction.CreateNotSupportedException();
			}
		}

		static decimal GetOffsetFromAncestor(PassDirection direction, ScheduleNode ancestor, ScheduleNode node)
		{
			var currentNode = node;
			var offset = 0m;

			while (currentNode != ancestor)
			{
				var parent = currentNode.Parent;
				offset += GetOffsetFromNode(direction, parent, currentNode);
				currentNode = parent;
			}

			return offset;
		}

		static decimal GetOffsetFromNode(PassDirection direction, ScheduleNode parent, ScheduleNode currentNode)
		{
			switch (direction)
			{
				case PassDirection.Early:
					parent.DescendantLateFinishOffsets.TryGetValue(currentNode, out var lateFinishOffset);
					return parent.EstimatedDurationHoursIncludingChildren - currentNode.EstimatedDurationHoursIncludingChildren - lateFinishOffset;

				case PassDirection.Late:
					parent.DescendantEarlyStartOffsets.TryGetValue(currentNode, out var earlyStartOffset);
					return parent.EstimatedDurationHoursIncludingChildren - earlyStartOffset - currentNode.EstimatedDurationHoursIncludingChildren;

				default:
					throw direction.CreateNotSupportedException();
			}
		}

		#endregion

		#endregion

		#region Critical Path Pass

		void DoCriticalPathPass(ICollection<ScheduleNode> endpointSchedules)
		{
			var walker = new DependencyPathWalker(endpointSchedules, includeClosedTaskHours: true);

			foreach (var node in SchedulesByEntity.Values)
			{
				node.IsCriticalPath = walker.IsOnCriticalPath(node);
			}

			network.CriticalPathDurationHours = walker.CriticalPathStandardEstimateHours;
		}

		#endregion

		#region AgreedDeliveryDatePass

		void DoAgreedDeliveryDatePass(BusinessObjectFactory dateFactory, WorkingTimeContext workingTimeContext)
		{
			var now = workingTimeContext.GetCurrentLocalTime(dateFactory).ToDateTime();
			var workTimeArithmetic = workingTimeContext.GetWorkTimeArithmetic(dateFactory);

			foreach (var schedule in SchedulesByEntity.Values)
			{
				var agreedDeliveryDate = schedule.AgreedDeliveryDateInUtc;
				if (agreedDeliveryDate.IsValid)
				{
					var localAgreedDeliveryDate = agreedDeliveryDate.ToLocalBranchTime(workingTimeContext.Branch);
					var latestFinishHoursFromNow = GetLatestFinishHoursFromNow(schedule);
					var projectedFinishTime = workTimeArithmetic.GetDateTimeInWorkingHoursFutureOrPast(now, latestFinishHoursFromNow);

					if (projectedFinishTime > localAgreedDeliveryDate)
					{
						var timeDifference = (decimal)workTimeArithmetic.TimeDifference(localAgreedDeliveryDate.ToDateTime(), projectedFinishTime).TotalHours;
						var threat = new DeliveryDateThreat(schedule.Entity, timeDifference);

						PropagateThreatBackward(schedule, threat);
					}
				}
			}
		}

		void PropagateThreatBackward(ScheduleNode schedule, DeliveryDateThreat threat)
		{
			schedule.DeliveryDateThreat = threat;

			foreach (var previousSchedule in schedule.GetPrerequisites())
			{
				if (previousSchedule.LatestFinishHours > schedule.LatestStartHours - threat.FloatConsumption)
				{
					PropagateThreatBackward(previousSchedule, threat);
				}
			}
		}

		protected virtual double GetLatestFinishHoursFromNow(ScheduleNode schedule)
		{
			return (double)schedule.LatestFinishHours;
		}

		#endregion

		#region StartAndEndTimes

		void DoStartAndEndTimesPass()
		{
			var timeCache = new Dictionary<decimal, ZDateTime>();

			foreach (var node in SchedulesByEntity.Values.OrderBy(n => n.EarliestStartHours))
			{
				node.EarliestStartTimeUtc = GetTimeInUtcForOffset(node.EarliestStartHours, timeCache);
				node.EarliestFinishTimeUtc = GetTimeInUtcForOffset(node.EarliestFinishHours, timeCache);
				node.LatestStartTimeUtc = GetTimeInUtcForOffset(node.LatestStartHours, timeCache);
				node.LatestFinishTimeUtc = GetTimeInUtcForOffset(node.LatestFinishHours, timeCache);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		ZDateTime GetTimeInUtcForOffset(decimal hoursOffset, Dictionary<decimal, ZDateTime> timeCache)
		{
			if (timeCache.ContainsKey(hoursOffset))
			{
				return timeCache[hoursOffset];
			}
			else
			{
				return timeCache[hoursOffset] = OffsetConverter.GetTimeInUtcForTimeOffset(hoursOffset);
			}
		}

		OffsetToDateConverter OffsetConverter
		{
			get { return offsetConverter ?? (offsetConverter = CreateOffsetConverter()); }
		}

		OffsetToDateConverter offsetConverter;

		protected virtual OffsetToDateConverter CreateOffsetConverter()
		{
			return new OffsetToDateConverter(context, factory);
		}

		#endregion

		#region Mark Cyclic Vertexes

		static void MarkCyclicVertexes(IEnumerable<ScheduleNode> cyclicVertexes)
		{
			foreach (var schedule in cyclicVertexes)
			{
				schedule.IsCyclic = true;
			}
		}

		#endregion
	}
}
