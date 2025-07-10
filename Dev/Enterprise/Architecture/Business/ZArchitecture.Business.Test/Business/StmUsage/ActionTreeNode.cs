using System.Text;

namespace Enterprise.ZArchitecture.Business.Testing
{
	class ActionTreeNode : ActionTreeNodeCollection
	{
		public string Name { get; private set; }
		public int ActionCount { get; private set; }

		public ActionTreeNode(string name) : this(name, 1) { }
		public ActionTreeNode(string name, int actionCount) : this(name, actionCount, null) { }
		public ActionTreeNode(string name, ActionTreeNode[] nodes) : this(name, 1, nodes) { }

		public ActionTreeNode(string name, int actionCount, ActionTreeNode[] nodes)
			: base(nodes)
		{
			Name = name;
			ActionCount = actionCount;
		}

		public ActionTreeNode(Statistics.Xml.IUsage settings)
			: base(settings)
		{
			Name = settings.Name;
			ActionCount = settings.ActionCount;
		}

		public static bool operator ==(ActionTreeNode node1, ActionTreeNode node2) => node1.Equals(node2);
		public static bool operator !=(ActionTreeNode node1, ActionTreeNode node2) => !node1.Equals(node2);

		public override bool Equals(object obj)
		{
			if (obj is ActionTreeNode other)
			{
				if (!Name.Equals(other.Name) || ActionCount != other.ActionCount || Nodes.Count != other.Nodes.Count)
				{
					return false;
				}

				return base.Equals(obj);
			}

			return false;
		}

		public override int GetHashCode() => Name.GetHashCode() ^ ActionCount.GetHashCode() ^ base.GetHashCode();

		public override string ToString(int depth)
		{
			var builder = new StringBuilder();
			for (int i = 0; i < depth; ++i)
			{
				builder.Append("> ");
			}
			builder.AppendLine(string.Format("{0}({1})", Name, ActionCount));
			builder.Append(base.ToString(depth));
			return builder.ToString();
		}
	}
}
