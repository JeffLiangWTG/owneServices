using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI
{
	public class LayoutEntryInstructionDetailUserControl : Customs.GUI.LayoutEntryInstructionDetailBasicUserControl
	{
		protected override void UpdateControlsAfterLayoutUpdated(BaseJobDeclaration declaration)
		{
			base.UpdateControlsAfterLayoutUpdated(declaration);
			dynamicDetailsPanel.FindSingle<SeparatorUserControl>(nameof(EntryInstructionDetailsUserControl.RelatedEntrySeparatorUserControl)).AllowOverlap(dynamicDetailsPanel.FindSingle<Control>("ColumnSeparator0"));
		}
	}
}
