using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class ScheduleTaskRecurrenceControl : ZUserControl, IReadOnlyToggleControl
	{
		public ScheduleTaskRecurrenceControl()
		{
			InitializeComponent();
		}

		public bool IsCollectionActiveCheckboxVisible
		{
			get { return isActiveCheckBox.Visible; }
			set { isActiveCheckBox.Visible = value; }
		}

		public bool ReadOnly
		{
			get { return readOnly; }
			set
			{
				readOnly = value;
				collectionScheduleGroupBox.SetReadOnly(value);
				recurrenceControl1.SetReadOnly(value);
			}
		}

		bool readOnly;
	}
}
