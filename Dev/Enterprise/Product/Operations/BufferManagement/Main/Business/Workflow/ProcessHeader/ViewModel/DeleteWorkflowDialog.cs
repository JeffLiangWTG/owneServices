using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.Integration.Network;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.BufferManagement.Business
{
	static class DeleteWorkflowDialog
	{
		internal static DeleteWorkflowOption GetDeleteOption(ProcessHeader workflow, IMultiActionButtonDialogWrapper<DeleteWorkflowOption> dialogProvider)
		{
			var linkedShapes = workflow.GetLinkedShapes().Where(s => !s.IsDefaultDiagramChild()).ToArray();
			var useSingular = linkedShapes.Length == 1;

			if (linkedShapes.Any())
			{
				var caption = useSingular ? Res.GetString("0fd01c40-1e1e-428a-b14c-627728e333fb", "Workflow is linked to a shape") : Res.GetString("98f1b1b6-3ee5-4dac-98a7-295f8e26d0bf", "Workflow is linked to shapes");
				var message = GetDialogMessage(workflow, linkedShapes, useSingular);
				var buttons = GetDialogButtons(linkedShapes, useSingular);

				return dialogProvider.ShowDialog(message, caption, buttons);
			}
			else
			{
				return DeleteWorkflowOption.DeleteShapes;
			}
		}

		static string GetDialogMessage(ProcessHeader workflow, ICollection<IBMNCNShape> shapes, bool useSingular)
		{
			var result = new StringBuilder();
			var rootDiagrams = shapes.Select(s => s.RootDiagram).WhereNotNull().OrderBy(s => s.BNS_Name).Distinct().ToArray();

			if (useSingular)
			{
				result.Append(Res.GetString("06e05806-d5db-422a-b161-797fd3072f6a", "The workflow [{0}] is linked to a shape on the diagram listed below. How would you like to handle this?", workflow.FH_CompletionStatement));
			}
			else
			{
				result.Append(Res.GetString("362dce04-1878-4a75-82fc-947a14f40494", "The workflow [{0}] is linked to shapes on the diagrams listed below. How would you like to handle this?", workflow.FH_CompletionStatement));
			}

			result.AppendLine();

			foreach (var diagram in rootDiagrams)
			{
				result.AppendLine();
				result.Append(BMConstants.BulletPointCharacter);
				result.Append(" ");
				result.Append(diagram.BNS_Name);
			}

			return result.ToString();
		}

		static ButtonStripAction<DeleteWorkflowOption>[] GetDialogButtons(ICollection<IBMNCNShape> shapes, bool useSingular)
		{
			return new[]
			{
				new ButtonStripAction<DeleteWorkflowOption>
				{
					Text = useSingular ? Res.GetString("fa4a2c8e-3fbe-4de6-934b-555473f86c6c", "Un-link Shape") : Res.GetString("01ba95fd-1122-41ce-b237-1110d04d5ff4", "Un-link Shapes"),
					ToolTip = useSingular ? ResString.GetMultilingualString("67bb102c-868d-44e2-ae63-273be95f8fdc", "Un-link this workflow from the shape that currently refers to it.") : ResString.GetMultilingualString("7a48d838-f24d-41b4-acc7-e33d138feef5", "Un-link this workflow from the shapes that currently refer to it."),
					Response = DeleteWorkflowOption.UnLinkShapes,
				},
				new ButtonStripAction<DeleteWorkflowOption>
				{
					Text = useSingular ? Res.GetString("6b55264a-b6ea-4726-bc96-68c94c292622", "Delete Shape") : Res.GetString("0447e1b3-9b48-445e-8a1d-aaf430744778", "Delete Shapes"),
					ToolTip = useSingular ? ResString.GetMultilingualString("7de41d29-6654-4a90-bb3a-32c77953c7af", "Delete the shape that is linked to this workflow. Note that this could also remove arrows and nested shapes.") : ResString.GetMultilingualString("48d84c83-6f25-4bf4-a443-ca8a4d5a0899", "Delete all shapes that are linked to this workflow. Note that this could also remove arrows and nested shapes."),
					Response = DeleteWorkflowOption.DeleteShapes,
				},
				new ButtonStripAction<DeleteWorkflowOption>
				{
					Text = useSingular ? Res.GetString("640893e7-91db-4026-bbe8-a05c5088d385", "Open Diagram and Cancel Delete") : Res.GetString("219facd1-fe01-4e50-8658-aaed21b7f093", "Open Diagrams and Cancel Delete"),
					ToolTip = useSingular ? ResString.GetMultilingualString("93a813b8-204d-466a-8199-4de969db87cd", "Don't delete this workflow, and also open the diagram on which there is a shape linked to this workflow.") : ResString.GetMultilingualString("23811559-c832-4c34-9b6e-8a0367f4b64f", "Don't delete this workflow, and also open the diagrams on which there are shapes linked to this workflow."),
					Response = DeleteWorkflowOption.OpenDiagramsAndCancelDelete,
					FireAction = (s, e) => ShowDiagrams(shapes),
				},
				new ButtonStripAction<DeleteWorkflowOption>
				{
					Text = Res.GetString("13dfd8e6-dea6-4c58-82c6-13895ee75e63", "Cancel"),
					ToolTip = ResString.GetMultilingualString("bd130ad0-ec89-4f4c-a72f-987c38abd7df", "Don't delete this workflow."),
					Response = DeleteWorkflowOption.CancelDelete,
				},
			};
		}

		static void ShowDiagrams(ICollection<IBMNCNShape> shapes)
		{
			var formFactory = ObjectFactory.Get<INetworkDiagramFormFactory>();

			foreach (var shape in shapes)
			{
				formFactory.ShowNetworkFormForDiagramContainingShape(shape);
			}
		}
	}
}
