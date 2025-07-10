using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	/// <summary>
	/// This class tracks the right-most point child shapes of a diagram.
	/// Most wierd business rules in this class are purely optimisations in order to do this faster.
	/// EXCEPT the exclusion of Annotation Shapes which do not contribute to project duration.
	/// </summary>
	class NetworkDiagramWidthTracker
	{
		#region Constructors

		public NetworkDiagramWidthTracker(ShapeNetworkEntity entity, NetworkEntityCollection collection)
		{
			if (!entity.IsRoot)
			{
				throw new ArgumentException("Only the root can have a dynamic width.", nameof(entity));
			}

			this.entity = entity;

			lastWidth = SetAndGetLastWidth(0);

			collection.ItemsRemoved += Collection_ItemsRemoved;
			collection.ItemsAdded += Collection_ItemsAdded;
			sortedEntities = entity.Children.Cast<ShapeNetworkEntity>().Where(AddsToWidth).ToList();
			sortedEntities.Sort(new NetworkDiagramSizeComparer());
			SubscribeToPropertyChanged(sortedEntities);
		}

		readonly ShapeNetworkEntity entity;
		readonly List<ShapeNetworkEntity> sortedEntities;

		#endregion

		#region Event Handlers

		void Collection_ItemsAdded(object sender, CollectionItemsChangedEventArgs e)
		{
			var newItems = e.Items.Cast<ShapeNetworkEntity>().Where(AddsToWidth).ToArray();

			if (newItems.Length > 0)
			{
				SubscribeToPropertyChanged(newItems);
				sortedEntities.AddRange(newItems);
				SortAndRefresh();
			}
		}

		void Collection_ItemsRemoved(object sender, CollectionItemsChangedEventArgs e)
		{
			var itemsToRemove = new HashSet<INetworkEntity>(e.Items.Cast<INetworkEntity>());
			RemoveItems(itemsToRemove);
		}

		void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var senderEntity = sender as INetworkEntity;
			if (senderEntity != null && !senderEntity.IsCalculationSuspended && IsHorizontalCoordinateProperty(e.PropertyName))
			{
				SortAndRefresh();
			}
		}

		#endregion

		#region API

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		public int ExplicitDurationMinutes
		{
			get
			{
				var minutes = entity.Shape.ExplicitDurationMinutes;
				return minutes > 0 ? minutes : (ZInt)entity.ConvertCoordinateOffsetToStoredHorizontalOffset(Width);
			}
		}

		public double Width
		{
			get
			{
				if (entity.IsCalculationSuspended)
				{
					return lastWidth;
				}

				var last = sortedEntities.LastOrDefault(l => !l.IsDeleted);
				if (last == null)
				{
					return lastWidth = SetAndGetLastWidth(0);
				}
				else if (!last.IsCalculationSuspended)
				{
					return lastWidth = SetAndGetLastWidth(last.Width + last.X);
				}
				else
				{
					return lastWidth;
				}
			}
		}
		double lastWidth;

		double SetAndGetLastWidth(double lastShapeWidth)
		{
			if (entity.IsDiagramSurfaceFixed)
			{
				lastShapeWidth = entity.ConvertStoredHorizontalOffsetToCoordinateOffset((ZDecimal)entity.EntityDurationMinutes);
			}
			return lastShapeWidth;
		}

		#endregion

		#region Logic

		bool IsHorizontalCoordinateProperty(string property)
		{
			switch (property)
			{
				case nameof(entity.Width):
				case nameof(entity.X):
				case "":
					return true;

				default:
					return false;
			}
		}

		bool AddsToWidth(INetworkEntity arg)
		{
			switch (arg.ShapeType)
			{
				case ShapeTypeList.Codes.Annotation:
					return false;
				default:
					return ((INetworkEntity)arg.Parent)?.EntityPK == entity.PK;
			}
		}

		void SubscribeToPropertyChanged(IEnumerable<INetworkEntity> items)
		{
			foreach (var item in items)
			{
				item.PropertyChanged -= Item_PropertyChanged;
				item.PropertyChanged += Item_PropertyChanged;
			}
		}
		void RemoveItems(HashSet<INetworkEntity> itemsToRemove)
		{
			foreach (var item in itemsToRemove)
			{
				item.PropertyChanged -= Item_PropertyChanged;
			}

			sortedEntities.RemoveAll(e => e.IsDeleted || itemsToRemove.Contains(e));
		}

		void SortAndRefresh()
		{
			sortedEntities.Sort(new NetworkDiagramSizeComparer());
		}

		#endregion

		#region Comparer

		class NetworkDiagramSizeComparer : IComparer<ShapeNetworkEntity>
		{
			public int Compare(ShapeNetworkEntity x, ShapeNetworkEntity y)
			{
				return (x.X + x.Width).CompareTo(y.X + y.Width);
			}
		}

		#endregion
	}
}
