using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class RateValidityControl : ZUserControl
	{
		public RateValidityControl()
		{
			InitializeComponent();
		}

		public bool ReadOnly
		{
			get { return fReadOnly; }
			set
			{
				fReadOnly = value;
				RateValidityCalcEdit.ReadOnly = value;
				ApplyCheckBox.ReadOnly = value;
				BlankCheckbox.ReadOnly = value;
			}
		}

		bool fReadOnly;

		public int FieldValue
		{
			get
			{
				string textValue = RateValidityCalcEdit.Text;
				if (string.IsNullOrEmpty(textValue))
				{
					textValue = "0";
				}

				int value = Math.Abs(int.Parse(textValue));
				return ApplyCheckBox.Checked ? -value : value;
			}
			set
			{
				RateValidityCalcEdit.Text = Math.Abs(value).ToString();
				ApplyCheckBox.Checked = value < 0;
				BlankCheckbox.Checked = value == 0;
			}
		}

		void BlankCheckbox_CheckedChanged(object sender, EventArgs e)
		{
			if (BlankCheckbox.Checked)
			{
				FieldValue = 0;
				ApplyCheckBox.Checked = false;
			}

			RateValidityCalcEdit.ReadOnly = BlankCheckbox.Checked;
		}

		void ApplyCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (ApplyCheckBox.Checked)
			{
				BlankCheckbox.Checked = false;
			}
		}
	}
}
