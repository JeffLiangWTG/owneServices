using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public static class BufferCreator
	{
		public static void AddProjectBuffer(IJobNetwork network)
		{
			var diagram = network.DiagramShape;

			var name = Res.GetString("b9f628e5-c1d4-435c-a7ae-7932059f0812", "Project Buffer");
			var criticalChainDurationInMinutes = (int)(network.GetCriticalChainDuration().TotalMinutes * CCPMConstants.BufferWidthPercentOfPreceedingCriticalChain);

			var buffer = CreateBuffer(network, diagram, name, criticalChainDurationInMinutes);
			buffer.BufferType = BufferTypeList.Codes.Project;
			var latestShape = network.GetCriticalChain().LastOrDefault();

			if (latestShape != null)
			{
				network.Entities.GetInstance(buffer).LocateAfterShape(latestShape);

				var dependencyAttachment = buffer.Factory.New<BMNCNAttachment>();
				dependencyAttachment.BNA_BNS_FromShape = latestShape.PK;
				dependencyAttachment.BNA_BNS_ToShape = buffer.PK;
				dependencyAttachment.BNA_BNS_Owner = diagram.PK;
				dependencyAttachment.BNA_Type = AttachmentTypeList.Codes.Dependency;

				diagram.RegisterEditableChildObject(dependencyAttachment);
			}
		}

		public static void AddFeedingBuffers(IJobNetwork network)
		{
			var shapes = new HashSet<ShapeNetworkEntity>(network.Entities.ShapeEntities);

			foreach (var shapeEntity in network.GetCriticalChain())
			{
				var nonCCArrows =
					from a in shapeEntity.PreRequisiteLinks
					where !((IProposedNetworkEntity)a.From).IsOnCriticalPath
					where !a.IsResourceDependency
					select a;

				foreach (var nonCCPrereqArrow in nonCCArrows.ToArray())
				{
					var prereqShape = nonCCPrereqArrow.From;

					var buffer = nonCCPrereqArrow.CreateBuffer();
					buffer.Shape.BufferType = BufferTypeList.Codes.Feeding;
					var name = prereqShape.Name.SubstringSafe(0, BMNCNShapeSchema.BNS_Name.MaxLength - FeedingBufferName.Length - 1);

					buffer.Name = string.Format(CultureInfo.InvariantCulture, "{0} {1}", name, FeedingBufferName);
					buffer.Shape.Active = ZBool.False;

					var durationInMinutes = (int)(GetDurationOfNonCCBranch(prereqShape, shapes) * CCPMConstants.BufferWidthPercentOfPreceedingCriticalChain);
					buffer.SetWidthForDuration(durationInMinutes);
					buffer.RoundUpToResolutionIncrement(network.DiagramEntity);

					buffer.X = (int)(shapeEntity.X - buffer.Width);
					buffer.Y = (int)(((prereqShape.Height - buffer.Height) / 2) + prereqShape.Y);

					EnsureNoOverlapInChain(buffer);
				}
			}
		}

		internal static void SetIsBufferedFlag(IJobNetwork network)
		{
			foreach (var entity in network.Shapes.Append(network.DiagramShape))
			{
				if (!entity.IsBuffered)
				{
					entity.IsBuffered = ZBool.True;
				}
			}
		}

		#region Implementation

		static int GetDurationOfNonCCBranch(ShapeNetworkEntity startingShape, HashSet<ShapeNetworkEntity> entities, int runningTotal = 0)
		{
			var floatHours = startingShape.Schedule.FloatHours;
			runningTotal += startingShape.Shape.ExplicitDurationMinutes;

			var prereqShapes =
				from a in startingShape.PreRequisiteLinks
				where !a.IsResourceDependency
				let fromShape = a.From
				where entities.Contains(fromShape)
				where !fromShape.Schedule.IsCriticalPath
				where fromShape.Schedule.FloatHours == floatHours
				select fromShape;

			var nonCCPrereqWithSameFloat = prereqShapes.FirstOrDefault(); // Could be multiple prereq shapes with same float, but this indicates paths with identical lengths, so it's ok to pick the first one.

			if (nonCCPrereqWithSameFloat != null)
			{
				return GetDurationOfNonCCBranch(nonCCPrereqWithSameFloat, entities, runningTotal);
			}
			else
			{
				return runningTotal;
			}
		}

		static void EnsureNoOverlapInChain(ShapeNetworkEntity startingEntity)
		{
			var pushedShapes = new List<ShapeNetworkEntity>();

			foreach (var prereq in startingEntity.PreRequisiteEntities.Where(s => !s.IsOnCriticalPath && !s.IsPinned))
			{
				if (prereq.X + prereq.Width > startingEntity.X)
				{
					pushedShapes.Add(prereq);
					prereq.X = Math.Max((int)(startingEntity.X - prereq.Width), 0);
				}
			}

			foreach (var prereq in pushedShapes)
			{
				EnsureNoOverlapInChain(prereq);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		static BMNCNBufferShape CreateBuffer(IJobNetwork network, BMNCNShape owner, string name, int durationInMinutes)
		{
			var buffer = owner.Factory.New<BMNCNBufferShape>();
			buffer.BNS_Name = name;
			buffer.Active = ZBool.False;

			buffer.MakeChildOf(owner);

			var ownerEntity = network.Entities.GetInstance(owner);
			var bufferEntity = network.Entities.GetInstance(buffer);

			bufferEntity.Height = CCPMConstants.BufferShapeDefaultHeight;
			bufferEntity.SetWidthForDuration(durationInMinutes);
			bufferEntity.RoundUpToResolutionIncrement(ownerEntity);

			owner.RegisterEditableChildObject(buffer);

			return buffer;
		}

		static string FeedingBufferName
		{
			get { return Res.GetString("28a4f023-f5d0-4b7a-b5f1-ad64c16e7349", "Feeding Buffer"); }
		}

		#endregion
	}
}
