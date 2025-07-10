using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	public class EntityPositionStrategy : IEntityPositionStrategy
	{
		public virtual double ConvertXToPixels(double value)
		{
			return value;
		}

		public virtual double ConvertYToPixels(double value)
		{
			return value;
		}

		public virtual double ConvertPixelsToX(double value)
		{
			return value;
		}

		public virtual double ConvertPixelsToY(double value)
		{
			return value;
		}

		public virtual double DefaultWidth
		{
			get { return 270; }
		}

		public virtual double DefaultHeight
		{
			get { return 75; }
		}

		#region Margin

		public virtual double LeftMargin
		{
			get { return 0d; }
		}

		public virtual double TopMargin
		{
			get { return 40d; }
		}

		public virtual double RightMargin
		{
			get { return 0d; }
		}

		public virtual double BottomMargin
		{
			get { return 5d; }
		}
		#endregion

		public Location DefaultPosition(INetworkEntity entity)
		{
			var x = Math.Max(NodeAutoPositioningLeftOffset, entity.X);
			var y = Math.Max(NodeAutoPositioningTopOffset, entity.Y);
			return new Location(ConvertXToPixels(x), ConvertYToPixels(y));
		}

		public virtual IEnumerable<INetworkEntity> SetPositionsForNewEntities(IEnumerable<INetworkEntity> newEntities, IEnumerable<INetworkEntity> allEntities, Location minimumOffset)
		{
			CheckThatNoEntitiesArePinned(newEntities);

			var modifiedEntityHash = new HashSet<INetworkEntity>(newEntities);
			var oldEntities = allEntities.Where(e => !modifiedEntityHash.Contains(e));
			var groups = GetEntityGroups(newEntities, oldEntities);

			/*
			 * By the two factors, 'Setting Minimum Offset of all new entities before we begin' and 'Laying out children before parents'
			 * The layout algorithm can use exact coordinates rather than needing to use relative offsets from parents.
			 * 
			 * This algorithm will perform poorly when laying out networks with cross heirarchical dependencies. :(
			 */
			foreach (var entity in newEntities)
			{
				if (entity.X < minimumOffset.X)
				{
					entity.X = minimumOffset.X;
				}

				if (entity.Y < minimumOffset.Y)
				{
					entity.Y = minimumOffset.Y;
				}
			}

			foreach (var group in groups.OrderByDescending(g => g.Depth))
			{
				var parent = group.Parent;
				var groupEntities = group.NewEntities;
				var minParentWidth = 0d;
				var minParentHeight = 0d;

				var hitBoxes = new HashSet<Tuple<int, int>>();

				foreach (var entity in group.OldEntities)
				{
					PopulateHitBoxesForEntity(hitBoxes, entity);
				}

				var depthCache = new Dictionary<IProposedNetworkEntity, int>();
				foreach (var newEntity in groupEntities.OrderBy(e => e.GetPreRequisiteDepth(depthCache)))
				{
					var righmostPrereq = GetRightmostPrereq(newEntity);
					var startX = GetBestX(ref minimumOffset, group, parent, righmostPrereq);
					var startY = GetBestY(ref minimumOffset, group, parent, righmostPrereq);

					if (newEntity.X != startX)
					{
						newEntity.X = startX;
					}

					if (newEntity.Y != startY)
					{
						newEntity.Y = startY;
					}

					if (DefaultWidth > newEntity.Width)
					{
						newEntity.Width = DefaultWidth;
					}

					if (DefaultHeight > newEntity.Height)
					{
						newEntity.Height = DefaultHeight;
					}

					while (GetHitBoxKeys(newEntity).Any(key => hitBoxes.Contains(key)))
					{
						var y = newEntity.Y;
						newEntity.Y += NodeYSpacer + DefaultHeight;
						if (y == newEntity.Y)
						{
							break;
						}
					}

					PopulateHitBoxesForEntity(hitBoxes, newEntity);

					minParentHeight = Math.Max(newEntity.Y - parent.Y + newEntity.Height, minParentHeight);
					minParentWidth = Math.Max(newEntity.X - parent.X + newEntity.Width, minParentWidth);
				}

				TryGrowParentToFit(modifiedEntityHash, parent, minParentWidth, minParentHeight);
			}

			return modifiedEntityHash;
		}

		static void CheckThatNoEntitiesArePinned(IEnumerable<INetworkEntity> newEntities)
		{
			if (newEntities.Any(n => n.Pin != null))
			{
				// How can you pin an entity before it has been laid out? It makes no sense, and thus is forbidden.
				throw new InvalidOperationException("Pinned enties should never be considered 'New Entities'.");
			}
		}

		void TryGrowParentToFit(HashSet<INetworkEntity> modifiedEntityHash, INetworkEntity parent, double minParentWidth, double minParentHeight)
		{
			if (parent.Pin == null && !parent.IsRoot())
			{
				minParentWidth += RightMargin;

				if (parent.Width < minParentWidth)
				{
					parent.Width = minParentWidth;
					modifiedEntityHash.Add(parent);
				}

				minParentHeight += BottomMargin;

				if (parent.Height < minParentHeight)
				{
					parent.Height = minParentHeight;
					modifiedEntityHash.Add(parent);
				}
			}
		}

		static INetworkEntity GetRightmostPrereq(INetworkEntity newEntity)
		{
			return newEntity.PreRequisiteLinks.Select(e => e.From).Cast<INetworkEntity>().OrderByDescending(entity => entity.X + entity.Width).FirstOrDefault();
		}

		double GetBestX(ref Location minimumOffset, EntityGroup group, INetworkEntity parent, INetworkEntity rightmostPrereq)
		{
			if (rightmostPrereq != null)
			{
				return rightmostPrereq.X + rightmostPrereq.Width + NodeXSpacer;
			}
			else
			{
				var leftOffsetFromParent = group.Depth > 1 ? NodeAutoPositioningLeftOffset + parent.X + LeftMargin : 0d;
				return Math.Max(leftOffsetFromParent, minimumOffset.X);
			}
		}

		double GetBestY(ref Location minimumOffset, EntityGroup group, INetworkEntity parent, INetworkEntity righmostPrereq)
		{
			if (righmostPrereq != null)
			{
				return righmostPrereq.Y;
			}
			else
			{
				var yOffsetFromParent = group.Depth > 1 ? NodeAutoPositioningTopOffset + parent.Y + TopMargin : 0d;
				return Math.Max(yOffsetFromParent, minimumOffset.Y);
			}
		}

		void PopulateHitBoxesForEntity(HashSet<Tuple<int, int>> hitBoxes, INetworkEntity entity)
		{
			foreach (var key in GetHitBoxKeys(entity))
			{
				if (!hitBoxes.Contains(key))
				{
					hitBoxes.Add(key);
				}
			}
		}

		IEnumerable<EntityGroup> GetEntityGroups(IEnumerable<INetworkEntity> newEntities, IEnumerable<INetworkEntity> oldEntities)
		{
			var dictionary = new Dictionary<INetworkEntity, EntityGroup>();

			foreach (var entity in newEntities)
			{
				EntityGroup group;
				var parent = entity.Parent as INetworkEntity;
				if (parent != null) // Only the root diagram has a null parent, and we don't need to position it.
				{
					if (!dictionary.TryGetValue(parent, out group))
					{
						group = new EntityGroup(parent);
						dictionary.Add(parent, group);
					}

					group.NewEntities.Add(entity);
				}
			}

			foreach (var entity in oldEntities)
			{
				EntityGroup group;
				var parent = entity.Parent as INetworkEntity;
				if (parent != null && dictionary.TryGetValue(parent, out group))
				{
					group.OldEntities.Add(entity);
				}
			}

			return dictionary.Values;
		}

		IEnumerable<Tuple<int, int>> GetHitBoxKeys(INetworkEntity entity)
		{
			var hitWidth = (int)(entity.Width * 4 / DefaultWidth);
			var hitHeight = (int)(entity.Height * 4 / (DefaultHeight + NodeYSpacer));
			var xStart = (int)(entity.X * 4 / DefaultWidth);
			var yStart = (int)(entity.Y * 4 / (DefaultHeight + NodeYSpacer));

			for (var x = xStart; x <= xStart + hitWidth; x++)
			{
				for (var y = yStart; y <= yStart + hitHeight; y++)
				{
					yield return Tuple.Create(x, y);
				}
			}
		}

		const int NodeXSpacer = 75;
		const int NodeYSpacer = 15;
		const int NodeAutoPositioningTopOffset = 0;
		const int NodeAutoPositioningLeftOffset = 0;

		class EntityGroup : Tuple<int, INetworkEntity, List<INetworkEntity>, List<INetworkEntity>>
		{
			public EntityGroup(INetworkEntity entity)
				: base(CalculateDepth(entity), entity, new List<INetworkEntity>(), new List<INetworkEntity>())
			{
			}

			public int Depth
			{
				get { return Item1; }
			}

			public INetworkEntity Parent
			{
				get { return Item2; }
			}

			public List<INetworkEntity> NewEntities
			{
				get { return Item3; }
			}

			public List<INetworkEntity> OldEntities
			{
				get { return Item4; }
			}
		}

		internal static int CalculateDepth(INetworkEntity entity)
		{
			var depth = 0;
			var parent = entity;
			while (parent != null)
			{
				++depth;
				parent = parent.Parent as INetworkEntity;
			}

			return depth;
		}

		double GetStaggeringXOffset(INetworkViewModel networkViewModel) => networkViewModel.Network.DiagramEntity.IsDiagramScaled ? networkViewModel.Network.DiagramEntity.ScaleUnitPixelSize : 30;
		double GetStaggeringYOffset() => 25;

		public Location GetPositionForNewEntities(Location lastCreatedNodeLocationRelativeToViewport, ViewportRect viewport, INetworkViewModel networkViewModel)
		{
			var newNodeLocationRelativeToViewport = new Location(lastCreatedNodeLocationRelativeToViewport.X + GetStaggeringXOffset(networkViewModel), lastCreatedNodeLocationRelativeToViewport.Y + GetStaggeringYOffset());

			if (newNodeLocationRelativeToViewport.X + DefaultWidth > viewport.Width)
			{
				newNodeLocationRelativeToViewport.X = LeftMargin;
			}

			if (newNodeLocationRelativeToViewport.Y + DefaultHeight > viewport.Height)
			{
				newNodeLocationRelativeToViewport.Y = TopMargin;
			}

			var newNodeAbsLocation = new Location(viewport.X + newNodeLocationRelativeToViewport.X, viewport.Y + newNodeLocationRelativeToViewport.Y);
			return newNodeAbsLocation;
		}
	}
}
