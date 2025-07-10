using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	partial class BMNCNShape
	{
		#region INetworkEntity

		#region Affinities

		IObservableReloadableCollection<IAffinity> INetworkEntity.AvailableAffinities
		{
			get { return null; }
		}

		IObservableReloadableCollection<IAffinity> INetworkEntity.AppliedAffinities
		{
			get { return null; }
		}

		#endregion

		string INetworkEntity.AdditionalDetail
		{
			get { throw new InvalidOperationException(); }
		}

		string INetworkEntity.AdditionalDetailTooltip
		{
			get { throw new InvalidOperationException(); }
		}

		Color INetworkEntity.BackColor
		{
			get => ColorFromNameConverter.ColorFromName(BackColor);
			set => BackColor = value.Name;
		}

		bool INetworkEntity.CanCreateRelationship(INetworkEntity other)
		{
			return CanCreateRelationship(other);
		}

		IEnumerable<INetworkEntity> INetworkEntity.Children
		{
			get { return ChildShapes; }
		}

		INetworkPin INetworkEntity.Pin
		{
			get { throw new InvalidOperationException(); }
		}

		int INetworkEntity.CornerRadius
		{
			get { return CornerRadius; }
			set { CornerRadius = value; }
		}

		IEnumerable<IEntityNotification> INetworkEntity.EntityNotifications
		{
			get
			{
				foreach (var error in this.GetErrors())
				{
					yield return new EntityNotification(EntityNotifcationType.Error, error.Message, Name);
				}
				foreach (var warning in this.GetWarnings())
				{
					yield return new EntityNotification(EntityNotifcationType.Warning, warning.Message, Name);
				}
				foreach (var message in this.GetMessageErrors())
				{
					yield return new EntityNotification(EntityNotifcationType.Message, message.Message, Name);
				}

				if (ProcessHeader != null)
				{
					foreach (var error in ProcessHeader.GetErrors())
					{
						yield return new EntityNotification(EntityNotifcationType.Error, error.Message, ProcessHeader.FH_CompletionStatement);
					}
					foreach (var warning in ProcessHeader.GetWarnings())
					{
						yield return new EntityNotification(EntityNotifcationType.Warning, warning.Message, ProcessHeader.FH_CompletionStatement);
					}
					foreach (var message in ProcessHeader.GetMessageErrors())
					{
						yield return new EntityNotification(EntityNotifcationType.Message, message.Message, Name);
					}
				}
			}
		}

		Guid INetworkEntity.EntityPK
		{
			get { return PK.ToGuid(); }
		}

		EntityState INetworkEntity.EntityState
		{
			get { throw new InvalidOperationException(); }
		}

		Color INetworkEntity.ForeColor
		{
			get => Color.FromName(ForeColor);
			set => ForeColor = value.Name;
		}

		bool INetworkEntity.HasNotifications
		{
			get { return HasNotifications; }
		}

		double INetworkEntity.Height
		{
			get { throw new InvalidOperationException(); }
			set { throw new InvalidOperationException(); }
		}

		bool INetworkEntity.IsCalculationSuspended
		{
			get { throw new InvalidOperationException(); }
			set { throw new InvalidOperationException(); }
		}

		bool INetworkEntity.IsLeafEntity
		{
			get { return ((ILinkEntity)this).IsLeaf; }
		}

		bool INetworkEntity.HasLinkedEntity
		{
			get { return LinkedEntity != null; }
		}

		bool INetworkEntity.IsLinkedToWorkflow
		{
			get { return ProcessHeader != null; }
		}

		bool INetworkEntity.IsDiagramWithRibbon
		{
			get { return BNS_ShapeType == ShapeTypeList.Codes.Diagram && BMSRegistry.Instance.NCNRibbonEnabled.Value; }
		}

		bool INetworkEntity.IsPositioned
		{
			get { return IsPositioned; }
		}

		string INetworkEntity.Notes
		{
			get { return ShapeNotes; }
			set { ShapeNotes = value; }
		}

		string INetworkEntity.ShapeType
		{
			get { return IsDeleted ? ShapeTypes.Shape : ShapeType; }
		}

		NetworkActions INetworkEntity.SupportedActions
		{
			get { return SupportedActions; }
		}

		double INetworkEntity.Width
		{
			get { throw new InvalidOperationException(); }
			set { throw new InvalidOperationException(); }
		}

		double INetworkEntity.X
		{
			get { throw new InvalidOperationException(); }
			set { throw new InvalidOperationException(); }
		}

		double INetworkEntity.Y
		{
			get { throw new InvalidOperationException(); }
			set { throw new InvalidOperationException(); }
		}

		int INetworkEntity.ZIndex
		{
			get { throw new InvalidOperationException(); }
			set { throw new InvalidOperationException(); }
		}

		bool IEquatable<INetworkEntity>.Equals(INetworkEntity other)
		{
			var shape = other as BMNCNShape;
			return shape != null && shape == this;
		}

		#endregion

		#region IProposedNetworkEntity

		bool IProposedNetworkEntity.CanDeleteUnderlyingEntity
		{
			get { return CanDeleteUnderlyingEntity; }
		}

		bool IProposedNetworkEntity.CanUnlinkEntity
		{
			get { return CanUnlinkEntity; }
		}

		string IProposedNetworkEntity.CompletionCriteria
		{
			get
			{
				var entity = IsLinkedToRealEntity ? (IProposedNetworkEntity)ProcessHeader : new BMNCNShapeNetworkEntity(this);
				return entity.CompletionCriteria;
			}
			set
			{
				var entity = IsLinkedToRealEntity ? (IProposedNetworkEntity)ProcessHeader : new BMNCNShapeNetworkEntity(this);
				entity.CompletionCriteria = value;
			}
		}

		string IProposedNetworkEntity.Description
		{
			get { return NetworkEntity.Description; }
		}

		string IProposedNetworkEntity.EstimateSummary
		{
			get { return NetworkEntity.EstimateSummary; }
		}

		bool IProposedNetworkEntity.IsOnCriticalPath
		{
			get { return IsOnCriticalPath; }
		}

		bool IProposedNetworkEntity.IsSameEntity(IProposedNetworkEntity other)
		{
			return IsSameEntity(other);
		}

		bool IProposedNetworkEntity.IsStartable
		{
			get { return NetworkEntity.IsStartable; }
		}

		string IProposedNetworkEntity.JobName
		{
			get
			{
				if (LinkedEntity is IProposedNetworkEntity linkedEntity)
				{
					return linkedEntity.Name;
				}

				return ShapeNetworkEntity.JobName;
			}
			set
			{
				if (LinkedEntity is IProposedNetworkEntity linkedEntity)
				{
					linkedEntity.Name = value;
				}

				ShapeNetworkEntity.JobName = value;
			}
		}

		bool IProposedNetworkEntity.JobName_ReadOnly
		{
			get { return (LinkedEntity as IProposedNetworkEntity ?? ShapeNetworkEntity).JobName_ReadOnly; }
		}

		string IProposedNetworkEntity.JobNumber
		{
			get { return JobNumber; }
		}

		IEnumerable<IEntityRelationship> IProposedNetworkEntity.Links
		{
			get { return ShapeNetworkEntity.Links; }
		}

		string IProposedNetworkEntity.Name
		{
			get { return Name; }
			set { Name = value; }
		}

		IProposedNetworkEntity IProposedNetworkEntity.Parent
		{
			get { throw new InvalidOperationException(); }
		}

		public IEnumerable<IEntityRelationship> PostRequisiteLinks
		{
			get { return ShapeNetworkEntity.PostRequisiteLinks; }
		}

		public int PreRequisiteDepth
		{
			get { return ShapeNetworkEntity.PreRequisiteDepth; }
		}

		public IEnumerable<IEntityRelationship> PreRequisiteLinks
		{
			get { return ShapeNetworkEntity.PreRequisiteLinks; }
		}

		WorkStatus IProposedNetworkEntity.Status
		{
			get { return GetStatusCore(); }
		}

		protected virtual WorkStatus GetStatusCore()
		{
			return NetworkEntity.Status;
		}

		string IProposedNetworkEntity.StatusDescription
		{
			get { return NetworkEntity.StatusDescription; }
		}

		string IProposedNetworkEntity.StatusName
		{
			get { return NetworkEntity.StatusName; }
		}

		#endregion

		#region INotifyPropertyChanged

		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add { PropertyChanged += value; }
			remove { PropertyChanged -= value; }
		}

		#endregion

		#region INetworkActionResult

		bool INetworkActionResult.IsHandledByVisualiser
		{
			get { return true; }
		}

		#endregion
	}
}
