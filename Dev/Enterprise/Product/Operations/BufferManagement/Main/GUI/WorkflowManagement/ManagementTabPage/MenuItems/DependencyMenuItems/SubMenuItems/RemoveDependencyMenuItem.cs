using System.Text;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.GUI
{
	class RemoveDependencyMenuItem : LabelledDependencySubMenuItemBase
	{
		internal RemoveDependencyMenuItem(LinkedProcessHeader dependency, ProcessHeader sourceWorkflow, DependencyMenuItemStrategy strategy)
			: base(dependency, sourceWorkflow, strategy)
		{
			this.strategy = strategy;
		}

		readonly DependencyMenuItemStrategy strategy;

		protected override void OnMenuItemClicked()
		{
			var message = new StringBuilder();
			MessageBoxIcon icon;

			if (IsIndirectDependency())
			{
				icon = MessageBoxIcon.Warning;
				message.Append(Res.GetString("72e64936-4fe4-44ab-8253-604431788574", "This is an indirect relationship to a different workflow. Would you like to delete this link anyway?", Dependency.LinkToProcessHeader.DisplayText));
			}
			else
			{
				icon = MessageBoxIcon.Question;
				message.Append(Res.GetString("8c80e29b-5e25-4fed-b3d9-6968c431684c", "Would you like to delete this link?"));
			}

			message.AppendLine();
			message.AppendLine();

			message.Append(Res.GetString("2a94ba05-12fa-423b-8890-ac57600d3108", "From:\t{0}", Dependency.LinkToProcessHeader.HeaderFrom.Description));
			message.AppendLine();
			message.Append(Res.GetString("bed89b76-03a4-4897-a0ca-c3c3ec1b8d80", "To:\t{0}", Dependency.LinkToProcessHeader.HeaderTo.Description));

			var result = Globals.Message.Show(
				message.ToString(),
				Res.GetString("46c69d9c-6c35-4dc5-8c66-b6d36ae2ac52", "Remove link"),
				MessageBoxButtons.YesNo,
				icon,
				DialogResult.Yes
				);

			if (result == DialogResult.Yes)
			{
				var factory = Dependency.LinkToProcessHeader.Factory;
				Dependency.LinkToProcessHeader.Delete();

				if (strategy.SaveAfterActions)
				{
					factory.SaveHandlingZSaveExceptions();
				}
			}
		}

		bool IsIndirectDependency()
		{
			return SourceWorkflow != Dependency.LinkToProcessHeader.HeaderFrom && SourceWorkflow != Dependency.LinkToProcessHeader.HeaderTo;
		}
	}
}
