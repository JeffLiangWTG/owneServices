using System;
using System.Collections.Generic;
using System.Drawing;
using CargoWise.Common;

namespace CargoWise.NetworkVisualisation.Integration
{
	public interface INetworkEntity : IProposedNetworkEntity, INetworkActionResult, IEquatable<INetworkEntity>
	{
		Guid EntityPK { get; }
		string ShapeType { get; }
		EntityState EntityState { get; }

		double X { get; set; }
		double Y { get; set; }
		double Width { get; set; }
		double Height { get; set; }
		int ZIndex { get; set; }

		int CornerRadius { get; set; }
		Color BackColor { get; set; }
		Color ForeColor { get; set; }

		bool IsLeafEntity { get; }
		bool HasLinkedEntity { get; }
		bool IsLinkedToWorkflow { get; }

		/// <summary>
		/// Returns True if this INetworkEntity has had its position
		/// explicitely set by the user, or whether it has had it set in the
		/// past and had it persistent and loaded.
		/// 
		/// Returns False if this INetworkEntity has never had its
		/// position set by the user.
		/// </summary>
		bool IsPositioned { get; }
		bool IsNonScheduled { get; set; }
		bool IsDiagramWithRibbon { get; }

		bool IsDeleted { get; }

		string AdditionalDetail { get; }
		string AdditionalDetailTooltip { get; }
		string Notes { get; set; }

		IObservableReloadableCollection<IAffinity> AvailableAffinities { get; }
		IObservableReloadableCollection<IAffinity> AppliedAffinities { get; }

		bool HasNotifications { get; }
		IEnumerable<IEntityNotification> EntityNotifications { get; }

		NetworkActions SupportedActions { get; }

		bool CanCreateRelationship(INetworkEntity other);
		bool IsCalculationSuspended { get; set; }

		IEnumerable<INetworkEntity> Children { get; }
		INetworkPin Pin { get; }

		void SwapNonScheduledState();

		IDisposable SuspendSettingHasChanges();
	}

	public static class NetworkEntityExtensions
	{
		public static void SuspendCalculation(this INetworkEntity layout)
		{
			Argument.NotNull(layout, nameof(layout)); // Suggested By ReviewBot 
			layout.IsCalculationSuspended = true;
		}

		public static void ResumeCalculation(this INetworkEntity layout)
		{
			Argument.NotNull(layout, nameof(layout)); // Suggested By ReviewBot 
			layout.IsCalculationSuspended = false;
		}
	}
}
