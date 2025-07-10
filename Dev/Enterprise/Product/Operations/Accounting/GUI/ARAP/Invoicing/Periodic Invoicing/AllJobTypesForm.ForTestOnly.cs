#if DEBUG

using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class AllJobTypesForm
	{
		public Business.JobTypePicker JobTypePicker_ForTestOnly
		{
			get { return JobTypePicker; }
			set { JobTypePicker = value; }
		}

		public ZCheckedListBox JobTypeCheckedListBox_ForTestOnly
		{
			get { return JobTypeCheckedListBox; }
			set { JobTypeCheckedListBox = value; }
		}

		public ZButton BtnSelectAll_ForTestOnly
		{
			get { return btnSelectAll; }
			set { btnSelectAll = value; }
		}

		public ZButton BtnSelect_ForTestOnly
		{
			get { return btnSelect; }
			set { btnSelect = value; }
		}

		public ZButton BtnCancel_ForTestOnly
		{
			get { return btnCancel; }
			set { btnCancel = value; }
		}

		public static AllJobTypesForm CreateAllJobTypesForm_ForTestOnly()
		{
			return new AllJobTypesForm();
		}
	}
}

#endif
