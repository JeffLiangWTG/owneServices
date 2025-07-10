using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.GUI;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.Module
{
	public class UPEOrganisationModule : OrganisationModule
	{
		protected override IFilterControl GetNewFilterControl()
		{
			return new UPEOrganisationFilterControl(GridCollection, (UPEOrganisationFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new UPEOrganisationFilterBusinessObject();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewActionMenuItems());
			result.Add(new ZMenuItem("&Update Classifier Role", new EventHandler(UpdateClassifierRole_Click)));
			return result.ToArray();
		}

		void UpdateClassifierRole_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(
				new ClassifierAssignmentForm(
					new UPEStaffAssignmentUpdater(Factory)));
		}
	}
}
