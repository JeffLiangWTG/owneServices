using System;
using System.Collections.Generic;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public interface IShapeNetworkEntity : IDiagramEntity,
		IProposedNetworkEntity,
		IScheduledNetworkEntity,
		ILinkEntity,
		IApprovable,
		IBranchDepartmentProvider,
		IEquatable<BMNCNShape>
	{
		ZGuid RelatedEntityPK { get; set; }
		ProcessHeader ProcessHeader { get; }
		BMNCNShape Shape { get; }

		IShapeNetworkEntity Owner { get; }
		IShapeNetworkEntity Root { get; }
		new IEnumerable<IShapeNetworkEntity> Children { get; }

		IEnumerable<NetworkAttachment> DependencyAttachments { get; }

		bool IsDiagram { get; }
		bool IsScaled { get; }
		bool IsBufferShape { get; }
	}

	public static class IShapeNetworkEntity_Extensions
	{
		/// <summary>
		/// Lazy Depth first descendant traversal.
		/// </summary>
		public static IEnumerable<IShapeNetworkEntity> Descendants(this IShapeNetworkEntity entity)
		{
			foreach (var child in entity.Children)
			{
				yield return child;

				foreach (var grandChild in Descendants(child))
				{
					yield return grandChild;
				}
			}
		}
	}
}
