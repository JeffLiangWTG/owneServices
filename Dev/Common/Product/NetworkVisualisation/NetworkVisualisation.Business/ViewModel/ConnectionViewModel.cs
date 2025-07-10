using System;
using System.Collections.Generic;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	/// <summary>
	/// Defines a connection between two connectors (aka connection points) of two nodes.
	/// </summary>
	public sealed class ConnectionViewModel : ViewModelBase
	{
		#region Internal Data Members

		/// <summary>
		/// The source connector the connection is attached to.
		/// </summary>
		ConnectorViewModel sourceConnector;

		/// <summary>
		/// The destination connector the connection is attached to.
		/// </summary>
		ConnectorViewModel destConnector;

		/// <summary>
		/// The source and dest hotspots used for generating connection points.
		/// </summary>
		Location sourceConnectorHotspot;
		Location destConnectorHotspot;

		/// <summary>
		/// Points that make up the connection.
		/// </summary>
		IEnumerable<Location> points;

		#endregion Internal Data Members

		public string DeleteImageSource => "..\\Resources\\scissors.png";

		public string DeleteTooltip
		{
			get { return Res.GetString("b0d5b727-f51c-48a0-9ef2-e80ff395a922", "Remove arrow from diagram and delete any underlying relationship."); }
		}

		public IEntityRelationship Relationship
		{
			get { return relationship; }
			set
			{
				relationship = value;
				relationshipLayout = value as IEntityRelationshipLayout;
			}
		}

		IEntityRelationship relationship;
		IEntityRelationshipLayout relationshipLayout;

		/// <summary>
		/// The source connector the connection is attached to.
		/// </summary>
		public ConnectorViewModel SourceConnector
		{
			get { return sourceConnector; }
			set
			{
				if (sourceConnector == value)
				{
					return;
				}

				if (sourceConnector != null)
				{
					sourceConnector.AttachedConnections.Remove(this);
					sourceConnector.HotspotUpdated -= new EventHandler<EventArgs>(sourceConnector_HotspotUpdated);
				}

				sourceConnector = value;

				if (sourceConnector != null)
				{
					sourceConnector.AttachedConnections.Add(this);
					sourceConnector.HotspotUpdated += new EventHandler<EventArgs>(sourceConnector_HotspotUpdated);
					this.SourceConnectorHotspot = sourceConnector.Hotspot;
				}

				OnPropertyChanged(nameof(SourceConnector));
				OnConnectionChanged();
			}
		}

		/// <summary>
		/// The destination connector the connection is attached to.
		/// </summary>
		public ConnectorViewModel DestConnector
		{
			get { return destConnector; }
			set
			{
				if (destConnector == value)
				{
					return;
				}

				if (destConnector != null)
				{
					destConnector.AttachedConnections.Remove(this);
					destConnector.HotspotUpdated -= new EventHandler<EventArgs>(destConnector_HotspotUpdated);
				}

				destConnector = value;

				if (destConnector != null)
				{
					destConnector.AttachedConnections.Add(this);
					destConnector.HotspotUpdated += new EventHandler<EventArgs>(destConnector_HotspotUpdated);
					this.DestConnectorHotspot = destConnector.Hotspot;
				}

				OnPropertyChanged(nameof(DestConnector));
				OnConnectionChanged();
			}
		}

		/// <summary>
		/// The source and dest hotspots used for generating connection points.
		/// </summary>
		public Location SourceConnectorHotspot
		{
			get { return sourceConnectorHotspot; }
			set
			{
				sourceConnectorHotspot = value;

				ComputeConnectionPoints();

				OnPropertyChanged(nameof(SourceConnectorHotspot));
			}
		}

		public Location DestConnectorHotspot
		{
			get { return destConnectorHotspot; }
			set
			{
				destConnectorHotspot = value;

				ComputeConnectionPoints();

				OnPropertyChanged(nameof(DestConnectorHotspot));
			}
		}

		/// <summary>
		/// Points that make up the connection.
		/// </summary>
		public IEnumerable<Location> Points
		{
			get { return points; }
			set
			{
				points = value;

				OnPropertyChanged(nameof(Points));
			}
		}

		/// <summary>
		/// Event fired when the connection has changed.
		/// </summary>
		public event EventHandler<EventArgs> ConnectionChanged;

		public bool CanHide
		{
			get
			{
				var connector = SourceConnector ?? DestConnector;
				var parentNode = connector != null ? connector.ParentNode : null;
				var entity = parentNode != null ? parentNode.Entity : null;

				return entity != null && entity.Supports(NetworkActions.Hide);
			}
		}

		public bool IsVisible
		{
			get { return relationshipLayout == null || relationshipLayout.IsVisible; }
		}

		#region Private Methods

		/// <summary>
		/// Raises the 'ConnectionChanged' event.
		/// </summary>
		void OnConnectionChanged()
		{
			if (ConnectionChanged != null)
			{
				ConnectionChanged(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Event raised when the hotspot of the source connector has been updated.
		/// </summary>
		void sourceConnector_HotspotUpdated(object sender, EventArgs e)
		{
			this.SourceConnectorHotspot = this.SourceConnector.Hotspot;
		}

		/// <summary>
		/// Event raised when the hotspot of the dest connector has been updated.
		/// </summary>
		void destConnector_HotspotUpdated(object sender, EventArgs e)
		{
			this.DestConnectorHotspot = this.DestConnector.Hotspot;
		}

		/// <summary>
		/// Rebuild connection points.
		/// </summary>
		void ComputeConnectionPoints()
		{
			var computedPoints = new List<Location>();
			computedPoints.Add(this.SourceConnectorHotspot);

			double deltaX = Math.Abs(this.DestConnectorHotspot.X - this.SourceConnectorHotspot.X);
			double deltaY = Math.Abs(this.DestConnectorHotspot.Y - this.SourceConnectorHotspot.Y);
			if (deltaX > deltaY)
			{
				double midPointX = this.SourceConnectorHotspot.X + ((this.DestConnectorHotspot.X - this.SourceConnectorHotspot.X) / 2);
				computedPoints.Add(new Location(midPointX, this.SourceConnectorHotspot.Y));
				computedPoints.Add(new Location(midPointX, this.DestConnectorHotspot.Y));
			}
			else
			{
				double midPointY = this.SourceConnectorHotspot.Y + ((this.DestConnectorHotspot.Y - this.SourceConnectorHotspot.Y) / 2);
				computedPoints.Add(new Location(this.SourceConnectorHotspot.X, midPointY));
				computedPoints.Add(new Location(this.DestConnectorHotspot.X, midPointY));
			}

			computedPoints.Add(this.DestConnectorHotspot);

			this.Points = computedPoints;
		}

		#endregion Private Methods
	}
}
