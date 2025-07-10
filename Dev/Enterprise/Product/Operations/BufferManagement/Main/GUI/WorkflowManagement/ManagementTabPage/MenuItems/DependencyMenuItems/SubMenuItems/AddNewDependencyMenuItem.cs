using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI
{
	class AddNewDependencyMenuItem : MaybeMutatingDependencySubMenuItemBase
	{
		internal AddNewDependencyMenuItem(ProcessHeader workflow, DependencyMenuItemStrategy strategy)
			: base(workflow, strategy, Res.GetString("40c8975f-fdaf-44ff-a70d-f79659bdb617", "Add new..."))
		{
		}

		protected override void OnMenuItemClicked()
		{
			var factory = SourceWorkflow.Factory;
			var collection = new ProcessHeaderCollection(factory);

			ProcessHeaderLookups.AddFilterDefaults(SourceWorkflow.JobHeader, collection);

			var processHeaders = BusinessObjectModulePicker.PickFromModuleScreen<ProcessHeader>(collection, ModuleIDs.ProcessHeader);
			var processHeadersInCorrectFactory = factory.Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.PK, processHeaders.Select(x => x.PK)));

			foreach (var processHeader in processHeadersInCorrectFactory)
			{
				var link = CreateLink(processHeader);

				if (!link.HasErrors && Strategy.SaveAfterActions)
				{
					link.RunPreSaveValidation();
				}

				if (link.HasErrors)
				{
					var message = new StringBuilder(Res.GetString("9883ca41-4eb7-4c25-9ef0-0ed6df913038", "This link is invalid so cannot be added."));
					var caption = Res.GetString("d9c60a5e-afec-4e50-a6d2-e5f5f88ac30b", "Invalid Dependency");

					message.AppendLine();

					foreach (var error in link.Notifications.Where(n => n.Type == CargoWise.ComponentModel.NotificationType.Error))
					{
						message.AppendLine();
						message.Append(error.Message);
					}

					Globals.Message.Show(message.ToString(), caption, MessageBoxButtons.OK, DialogResult.OK);

					link.Delete();
				}
				else if (Strategy.SaveAfterActions)
				{
					factory.SaveHandlingZSaveExceptions();
				}
			}
		}

		ProcessHeaderLink CreateLink(ProcessHeader processHeader)
		{
			var fromWorkflow = Strategy.OtherWorkflowLinkPosition == RelationshipDirection.From ? processHeader : SourceWorkflow;
			var toWorkflow = Strategy.OtherWorkflowLinkPosition == RelationshipDirection.From ? SourceWorkflow : processHeader;

			if (Strategy.LinkType == ProcessHeaderLinkTypeList.Codes.Dependency)
			{
				return WorkflowRelationshipCreator.CreateDependencyRelationship(SourceWorkflow.Factory, fromWorkflow, toWorkflow, RelationshipOptions.None).Link;
			}
			else
			{
				return fromWorkflow.GetOrCreateLinkToParent(toWorkflow);
			}
		}
	}
}
