using System.Text;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI
{
	abstract class LabelledDependencySubMenuItemBase : MaybeMutatingDependencySubMenuItemBase
	{
		protected LabelledDependencySubMenuItemBase(LinkedProcessHeader dependency, ProcessHeader sourceWorkflow, DependencyMenuItemStrategy strategy)
			: base(sourceWorkflow, strategy, GetText(dependency, sourceWorkflow, strategy))
		{
			Dependency = dependency;
		}

		protected LinkedProcessHeader Dependency { get; }

		static string GetText(LinkedProcessHeader prerequisite, ProcessHeader sourceWorkflow, DependencyMenuItemStrategy strategy)
		{
			var text = new StringBuilder(prerequisite.ProcessHeader.Description);
			var isClosed = prerequisite.ProcessHeader.IsClosed;
			var isIndirectRelationship = prerequisite.LinkToProcessHeader.FP_FH_HeaderFrom != sourceWorkflow.PK && prerequisite.LinkToProcessHeader.FP_FH_HeaderTo != sourceWorkflow.PK;

			if (isClosed || isIndirectRelationship)
			{
				text.Append(" [");

				if (isClosed)
				{
					text.Append(Res.GetString("b2a48438-cf25-46cd-aa2e-a2a343c5abe4", "complete"));

					if (isIndirectRelationship)
					{
						text.Append(",");
					}
				}

				if (isIndirectRelationship)
				{
					text.Append(strategy.IndirectRelationshipMenuItemText);
				}

				text.Append("]");
			}

			return text.ToString();
		}
	}
}
