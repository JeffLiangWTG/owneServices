using System.Collections.Generic;
using System.Linq;
using Enterprise.DataTransfer.Native.Common.Definitions.Associations;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.DB;
using Enterprise.DataTransfer.Native.Utils.Models;

namespace Enterprise.DataTransfer.Native.Common.Definitions
{
	public static class EntityDefinitionExtension
	{
		public static bool IsParentOf(this IEntityDefinition other, IEntityDefinition self)
		{
			return self.Parents.Contains(other);
		}

		public static bool IsChildrenOf(this IEntityDefinition other, IEntityDefinition self)
		{
			return self.Children.Contains(other);
		}

		public static IEnumerable<IEntityDefinition> HasMany(this IEntityDefinition self)
		{
			return from child in self.Children
						 where self.AssociationCollection[child, self].Cardinality == Cardinality.OneToMany
						 select child;
		}

		public static IEnumerable<IEntityDefinition> HasOne(this IEntityDefinition self)
		{
			return from child in self.Children
						 where self.AssociationCollection[child, self].Cardinality == Cardinality.OneToOne
						 select child;
		}

		public static IEnumerable<IEntityDefinition> BelongsTo(this IEntityDefinition self)
		{
			return self.Parents.Where(p => p != self.Parent);
		}

		public static IEnumerable<ColumnDef> ForeignKeys(this IEntityDefinition definition)
		{
			var table = definition.Table;
			var columns = table.Columns;
			return columns.ForeignKeys;
		}

		public static TreeNode<IEntityDefinition> ConvertToTree(this IEntityDefinition self)
		{
			var visited = new HashSet<IEntityDefinition>();

			if (self == null)
			{
				return null;
			}

			return ConvertToTree(self, visited);
		}

		static TreeNode<IEntityDefinition> ConvertToTree(this IEntityDefinition self, ICollection<IEntityDefinition> visited)
		{
			var treeNode = new TreeNode<IEntityDefinition>(self);
			var childrenList = new List<ITreeNode>();
			visited.Add(self);

			foreach (var parent in self.Parents)
			{
				if (!visited.Contains(parent) && !IsGrandparentThatsAlreadyAdded(self, parent, visited) && !HasParentThatIsAlreadyAdded(self, parent, visited))
				{
					var childNode = ConvertToTree(parent, visited);
					childNode.Parent = treeNode;
					childrenList.Add(childNode);
				}
			}

			foreach (var child in self.Children)
			{
				if (!visited.Contains(child))
				{
					var childNode = ConvertToTree(child, visited);
					childNode.Parent = treeNode;
					childrenList.Add(childNode);
				}
			}

			treeNode.Children = childrenList;

			return treeNode;
		}

		static bool HasParentThatIsAlreadyAdded(IEntityDefinition self, IEntityDefinition parent, ICollection<IEntityDefinition> visited)
		{
			// This part stops the engine from writing out the PK of the OrgSupplierPart inside an OrgPartRelation node when the OU node is a child of a CusClassPartPivot which itself of the child of the OrgSupplierPart in question. 
			var result = false;
			if (self.Parent == null && self.Parents.Any())
			{
				foreach (var p in self.Parents)
				{
					if (p.MainAssociation != null && p.MainAssociation.To != null && p.MainAssociation.To.TableName == parent.TableName
						&& visited.Any() && visited.First().GetHashCode() == parent.GetHashCode())
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		static bool IsGrandparentThatsAlreadyAdded(IEntityDefinition self, IEntityDefinition parent, ICollection<IEntityDefinition> visited)
		{
			// This bit stops the engine from writing-out the PK of an OrgSupplierPart inside a component (child) CusClassPartPivot 
			return self.Parent != null && self.Parent.Parent != null && self.Parent.Parent.EntityName == parent.EntityName && visited.Contains(self.Parent.Parent);
		}
	}
}
