#if DEBUG

using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class JobTypeSelectionControl
	{
		public ZCheckedListBox JobTypeCheckedListBox_ForTestOnly
		{
			get { return JobTypeCheckedListBox; }
			set { JobTypeCheckedListBox = value; }
		}

		public AllJobTypesForm JobTypesForm_ForTestOnly => JobTypesForm;

		public Business.JobTypePicker JobTypesPicker_ForTestOnly => JobTypesPicker;
	}
}

#endif
