using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.GUI.Tags;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI
{
	public partial class TagGrid : ZGrid
	{
		public TagGrid()
		{
			InitializeComponent();
		}

		protected override int HandleDelete(int clickedRow)
		{
			if (List.Count <= clickedRow)
			{
				return 0;
			}
			var link = (TagLink)List[clickedRow];
			if (!link.IsInDatabase)
			{
				return base.HandleDelete(clickedRow);
			}

			bool securityPassed = true;
			if (link.Magnitude != null)
			{
				var workQueue = link as WorkQueueMembershipLink;
				securityPassed = workQueue != null ? WorkQueueSecurity.CheckRemoveFromQueueSecurity(workQueue.Magnitude) : TagSecurity.CheckTagRemoveSecurity(link.Magnitude);
			}

			return securityPassed ? base.HandleDelete(clickedRow) : 0;
		}

		public void TagGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			var hitRow = GetRow(e);

			if (hitRow >= 0)
			{
				var bizEntityForForm = default(BusinessObject);
				var controllerID = ControllerIDs.BMTagDefinition;
				var selectedTagLink = ListManager.GetCurrent() as TagLink;

				if (selectedTagLink != null)
				{
					selectedTagLink.Validation.ValidateAll();

					if (selectedTagLink.HasErrors)
					{
						using (var errorMessage = new ZErrorMessageBox(selectedTagLink))
						{
							ZFormModaliser.ShowMessageBoxWithoutDispose(errorMessage);
							return;
						}
					}

					switch (selectedTagLink.Definition.TGD_Code)
					{
						case BMConstants.WorkQueuesTagGroupCode:
							controllerID = ControllerIDs.WorkQueues;
							bizEntityForForm = selectedTagLink.Magnitude;
							break;

						default:
							bizEntityForForm = selectedTagLink.Definition;
							break;
					}
				}

				var formOpenResult = GridEntityFormOpener.OpenFormForSavedParent((ZForm)FindForm(), this, e, controllerID, () => bizEntityForForm);

				if (formOpenResult.Controller != null)
				{
					var form = formOpenResult.Form;
					if (form != null)
					{
						((INavigableTagForm)form).NavigateToTagMagnitude(selectedTagLink.Magnitude);
						// For some unknown reason ZGrid.SelectSingleElement is diverting focus back to the main form, so I have to call this below.
						// I guess it's just "given you the focus you deserve"
						((ZForm)form).Focus();
					}
				}

				SelectSingleElement(selectedTagLink);
			}
		}
	}
}
