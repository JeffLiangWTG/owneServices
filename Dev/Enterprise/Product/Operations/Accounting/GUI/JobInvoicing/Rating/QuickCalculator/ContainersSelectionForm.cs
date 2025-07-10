using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class ContainersSelectionForm : ZChildForm
	{
		public ContainersSelectionForm(ContainerSelectionBusinessObjectCollection containerSelectionInfos)
			: base(containerSelectionInfos) { }

		#region Implementations

		public override string FormVerb => string.Empty;

		public override string FormCaption => Res.GetString(ContainerCountMenuItemManager.AssignContainersResourceKey, "Assign Container(s)");

		#endregion
	}
}

