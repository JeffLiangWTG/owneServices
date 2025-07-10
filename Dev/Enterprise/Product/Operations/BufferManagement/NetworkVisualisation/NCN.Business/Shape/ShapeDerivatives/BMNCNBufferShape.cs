using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class BMNCNBufferShape : BMNCNShape, IBuffer
	{
		public BMNCNBufferShape(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region BusinessObject Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			BNS_ShapeType = ShapeTypeList.Codes.Buffer;
		}

		#endregion

		#region BMNCNShape Overrides

		protected override WorkStatus GetStatusCore()
		{
			if (GetRelevantPrereqShapesUpTheTree(this).Select(s => ((IProposedNetworkEntity)s).Status).All(s => s == WorkStatus.Complete || s == WorkStatus.Cancelled))
			{
				return WorkStatus.Complete;
			}
			else
			{
				return base.GetStatusCore();
			}
		}

		protected override BufferPenetrationResult GetBufferPenetration(WorkingTimeContext context)
		{
			return BufferPenetrationCalculator.CalculatePenetrationPercentage((IBuffer)this, context, Factory);
		}

		#endregion

		#region IApprovable Members

		protected override void ApproveCore(string approverCode)
		{
			var wasApproved = IsApproved;
			base.ApproveCore(approverCode);

			if (wasApproved != IsApproved)
			{
				if (IsApproved)
				{
					EnsureRelatedBuffersSet();
				}
				else
				{
					RemoveFromRelatedBuffers();
				}
			}
		}

		internal void EnsureRelatedBuffersSet()
		{
			EnsureRelatedBuffersSetCore(this, new HashSet<BMNCNShape>());
		}

		void EnsureRelatedBuffersSetCore(BMNCNShape startingShape, HashSet<BMNCNShape> visitedShapes)
		{
			if (visitedShapes.Add(startingShape))
			{
				foreach (var shape in GetRelevantPrereqShapes(startingShape))
				{
					if (!shape.GetRelatedBuffers().Any(b => b == this))
					{
						CreateRelatedBufferAttachment(shape);
					}

					EnsureRelatedBuffersSetCore(shape, visitedShapes);
				}
			}
		}

		void CreateRelatedBufferAttachment(BMNCNShape shape)
		{
			var attachment = Factory.New<BMNCNAttachment>();

			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				attachment.BNA_Type = AttachmentTypeList.Codes.RelatedBuffer;
				attachment.BNA_BNS_Owner = PK;
				attachment.BNA_BNS_ToShape = shape.PK;
			}

			RegisterEditableChildObject(attachment);
		}

		void RemoveFromRelatedBuffers()
		{
			var attachments = AllAttachments.Where(a => a.BNA_Type == AttachmentTypeList.Codes.RelatedBuffer && a.BNA_BNS_Owner == PK).ToArray();

			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				foreach (var attachment in attachments)
				{
					attachment.Delete();
				}
			}
		}

		IEnumerable<BMNCNShape> GetRelevantPrereqShapesUpTheTree(BMNCNShape startingShape, HashSet<BMNCNShape> visitedShapes = null)
		{
			if (visitedShapes == null)
			{
				visitedShapes = new HashSet<BMNCNShape>();
			}

			foreach (var shape in GetRelevantPrereqShapes(startingShape))
			{
				if (!visitedShapes.Contains(shape))
				{
					visitedShapes.Add(shape);

					yield return shape;

					foreach (var prereq in GetRelevantPrereqShapesUpTheTree(shape, visitedShapes))
					{
						yield return prereq;
					}
				}
			}
		}

		IEnumerable<BMNCNShape> GetRelevantPrereqShapes(BMNCNShape startingShape)
		{
			var relevantShapes = new List<BMNCNShape>();

			foreach (BMNCNShape shape in startingShape.Prerequisites(new ProcessHeaderDescendantsStrategy()).ToArray())
			{
				if (IsRelevantForBuffering(shape))
				{
					relevantShapes.Add(shape);
					relevantShapes.AddRange(GetRelevantChildren(shape));
				}
			}

			return relevantShapes;
		}

		IEnumerable<BMNCNShape> GetRelevantChildren(BMNCNShape startingShape)
		{
			foreach (var shape in startingShape.ChildShapes)
			{
				if (IsRelevantForBuffering(shape))
				{
					yield return shape;

					foreach (var child in GetRelevantChildren(shape))
					{
						yield return child;
					}
				}
			}
		}

		bool IsRelevantForBuffering(BMNCNShape shape)
		{
			return !shape.IsBufferShape;
		}

		#endregion

		#region IBuffer Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		int IBuffer.SizeInMinutes
		{
			get { return ExplicitDurationMinutes; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		int IBuffer.OffsetInMinutes
		{
			get { return 0; }
		}

		public BufferType Type
		{
			get
			{
				switch (BufferType)
				{
					case BufferTypeList.Codes.Project:
						return CargoWise.PAVE.Common.Interfaces.BufferType.Project;

					case BufferTypeList.Codes.Feeding:
						return CargoWise.PAVE.Common.Interfaces.BufferType.Feeding;

					default:
						return CargoWise.PAVE.Common.Interfaces.BufferType.Unknown;
				}
			}
		}

		IReadOnlyCollection<IBufferedItem> IBuffer.RelatedBufferedItems
		{
			get
			{
				return AllAttachments
					.Where(a => a.BNA_Type == AttachmentTypeList.Codes.RelatedBuffer)
					.Select(attachment => attachment.ToShape)
					.Where(shape => shape != null)
					.ToList();
			}
		}

		#endregion

		#region For Test

#if DEBUG

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2} minutes", Name, BufferType, ExplicitDurationMinutes);
		}

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			if (BNS_BNS_ParentShape.IsEmpty)
			{
				MakeChildOf(Factory.NewWithValidTestData<BMNCNShape>());
			}
		}

#endif

		#endregion
	}
}
