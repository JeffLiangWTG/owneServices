using System.Collections.Generic;
using System.Text;

namespace Enterprise.ZArchitecture.Business.Testing
{
	class ActionTreeNodeCollection
	{
		public List<ActionTreeNode> Nodes { get; private set; }

		public ActionTreeNodeCollection(StmUsage usage)
			: this(usage.UsageActionsSettings)
		{
		}

		public ActionTreeNodeCollection(ActionTreeNode[] nodes)
		{
			Nodes = nodes != null ? new List<ActionTreeNode>(nodes) : new List<ActionTreeNode>();
		}

		public ActionTreeNodeCollection(Statistics.Xml.IUsages settings)
		{
			Nodes = new List<ActionTreeNode>();
			foreach (var childSettings in settings.Children)
			{
				if ((!childSettings.Name.Contains("BusinessObject")) &&
					(!childSettings.Name.Contains("ZSqlConnectionInfo")) &&
					(!childSettings.Name.Contains("ZAccessor")))
				{
					Nodes.Add(new ActionTreeNode(childSettings));
				}
			}
		}

		public static bool operator ==(ActionTreeNodeCollection node1, ActionTreeNodeCollection node2) => node1.Equals(node2);
		public static bool operator !=(ActionTreeNodeCollection node1, ActionTreeNodeCollection node2) => !node1.Equals(node2);

		public override bool Equals(object obj)
		{
			if (obj is ActionTreeNodeCollection other)
			{
				for (int i = 0; i < Nodes.Count; i++)
				{
					if (Nodes[i] != other.Nodes[i])
					{
						return false;
					}
				}

				return true;
			}

			return false;
		}

		public override int GetHashCode()
		{
			int hashCode = 0;
			foreach (var node in Nodes)
			{
				hashCode ^= node.GetHashCode();
			}
			return hashCode;
		}

		public override string ToString()
		{
			return ToString(-1);
		}

		public virtual string ToString(int depth)
		{
			var builder = new StringBuilder();
			foreach (var node in Nodes)
			{
				builder.Append(node.ToString(depth + 1));
			}

			return builder.ToString();
		}
	}
}
