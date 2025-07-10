using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class TriageAssistSearchBarUserControl : ZUserControl
	{
		new TriageAssistBusinessObject CurrentDataItem => (TriageAssistBusinessObject)base.CurrentDataItem;

		public TriageAssistSearchBarUserControl()
			: base()
		{
			InitializeComponent();

			ShowSearchOptionsCheckBox.AllowOverlap(SearchOptionsGroupBox);
			ShouldSearchSuggestedListCheckBox.AllowOverlap(ShouldSearchDescriptionCheckBox);
		}

		void ShowSearchOptionsCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			using (((ISingleElementListInternal)CurrentDataItem).SuspendListChanged())
			using (CurrentDataItem.SuspendSettingHasChanges())
			using (CurrentDataItem.GetValidationSuspender())
			{
				SearchOptionsGroupBox.Visible = ShowSearchOptionsCheckBox.Checked;
				var multiplier = SearchOptionsGroupBox.Visible ? 1 : -1;
				var heightOffset = multiplier * SearchOptionsGroupBox.Height;
				Height += heightOffset;
				Parent.Height += heightOffset;
			}
		}
	}
}
