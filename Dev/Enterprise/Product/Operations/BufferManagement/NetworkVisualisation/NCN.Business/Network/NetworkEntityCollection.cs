using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	/// <summary>
	/// Deleted shapes and attachments will remain in their dictionaries. (And that's ok.)
	/// </summary>
	public class NetworkEntityCollection : ImpObservableSet<INetworkEntity>
	{
		public NetworkEntityCollection(BMNCNShapeCollection shapes, JobNetwork network)
		{
			this.shapes = shapes;
			this.network = network;
			entityProviderFunction = CreateNetworkEntities;
			Update();
		}

		readonly BMNCNShapeCollection shapes;
		readonly JobNetwork network;
		readonly Dictionary<ZGuid, ShapeNetworkEntity> entityInstances = new Dictionary<ZGuid, ShapeNetworkEntity>();
		readonly Dictionary<ZGuid, NetworkAttachment> attachmentInstances = new Dictionary<ZGuid, NetworkAttachment>();

		IEnumerable<ShapeNetworkEntity> CreateNetworkEntities()
		{
			foreach (var shape in GetShapesInDepthOrder())
			{
				var shapeNetworkEntity = GetInstance(shape);
				if (shapeNetworkEntity != null && !shapeNetworkEntity.IsDeleted)
				{
					yield return shapeNetworkEntity;
				}
			}
		}

		public IEnumerable<BMNCNShape> GetShapesInDepthOrder()
		{
			var queue = new Queue<BMNCNShape>(shapes);  // Not the most efficient way of doing this with worst case O(n^2), but at least it doesn't require going to the database again
			var hash = new HashSet<ZGuid>();
			hash.Add(network.DiagramShape.PK);
			var panicCounter = 0;
			while (queue.Any())
			{
				var next = queue.Dequeue();
				var parentShape = next.BNS_BNS_ParentShape;
				if (hash.Contains(parentShape))
				{
					panicCounter = 0;
					hash.Add(next.PK);
					yield return next;
				}
				else
				{
					queue.Enqueue(next);
					if (panicCounter > queue.Count)
					{
						ErrorReporter.ReportOnce("OrphanShapeAlert", FormattableString.Invariant($"There is a shape in this collection that has no parent: {next}"));
						break;
					}
					panicCounter++;
				}
			}
		}

		public IEnumerable<ShapeNetworkEntity> ShapeEntities
		{
			get { return this.Cast<ShapeNetworkEntity>(); }
		}

		public ShapeNetworkEntity GetInstance(BMNCNShape shape, bool addIfNotPresent = true)
		{
			return GetInstanceCore(shape.PK, shape, addIfNotPresent);
		}

		public ShapeNetworkEntity GetInstance(IProposedNetworkEntity entity, bool addIfNotPresent = true)
		{
			return GetInstanceCore(((IBusiness)entity).Identifier, entity.AsShape(), addIfNotPresent);
		}

		ShapeNetworkEntity GetInstanceCore(ZGuid entityPK, BMNCNShape shape, bool addIfNotPresent)
		{
			if (!entityInstances.TryGetValue(entityPK, out var entity) && addIfNotPresent)
			{
				entity = new ShapeNetworkEntity(shape, network, this);

				entityInstances.Add(entityPK, entity);
			}

			return entity;
		}

		public NetworkAttachment GetInstance(BMNCNAttachment attachment)
		{
			return attachmentInstances.GetOrAdd(attachment.PK, () => new NetworkAttachment(attachment, this));
		}

		public NetworkAttachment GetInstance(IEntityRelationship layout)
		{
			return attachmentInstances.GetOrAdd(((IBusiness)layout).Identifier, () => new NetworkAttachment((BMNCNAttachment)layout, this));
		}

		public NetworkAttachment GetInstance(ILink layout)
		{
			return attachmentInstances.GetOrAdd(((IBusiness)layout).Identifier, () => new NetworkAttachment((BMNCNAttachment)layout, this));
		}

		public ShapeNetworkEntity GetInstance(ILinkEntity entity)
		{
			return GetInstance((IProposedNetworkEntity)entity);
		}

		public IEnumerable<ShapeNetworkEntity> GetInstances(IEnumerable<INetworkEntity> entities)
		{
			return entities.Select(e => GetInstance(e));
		}

		public IEnumerable<ShapeNetworkEntity> GetInstances(IEnumerable<BMNCNShape> entities)
		{
			return entities.Select(GetInstance);
		}

		public IEnumerable<ShapeNetworkEntity> GetInstances(IEnumerable<ILinkEntity> entities)
		{
			return entities.Select(GetInstance);
		}
		public IEnumerable<ShapeNetworkEntity> GetInstances(IEnumerable<IProposedNetworkEntity> entities)
		{
			return entities.Select(e => GetInstance(e));
		}

		public IEnumerable<NetworkAttachment> GetInstances(IEnumerable<IEntityRelationship> links)
		{
			return links.Select(GetInstance);
		}

		public IEnumerable<NetworkAttachment> GetInstances(IEnumerable<ILink> links)
		{
			return links.Select(GetInstance);
		}
	}
}
