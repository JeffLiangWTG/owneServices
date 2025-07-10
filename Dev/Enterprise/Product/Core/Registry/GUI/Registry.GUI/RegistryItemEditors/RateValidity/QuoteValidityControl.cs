using System;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class QuoteValidityControl : ZUserControl
	{
		public QuoteValidityControl()
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
				BlankValueCheckbox.ReadOnly = value;
			}
		}

		bool fReadOnly;

		public int FieldValue
		{
			get
			{
				var textValue = RateValidityCalcEdit.Text;
				var numericValue = QuoteValidityRegistryItem.BlankValue;

				if (!string.IsNullOrEmpty(textValue))
				{
					numericValue = Math.Abs(int.Parse(textValue));
				}

				return ApplyCheckBox.Checked ? -numericValue : numericValue;
			}
			set
			{
				if (Math.Abs(value) == QuoteValidityRegistryItem.BlankValue)
				{
					RateValidityCalcEdit.Text = "";
				}
				else
				{
					RateValidityCalcEdit.Text = Math.Abs(value).ToString();
				}
				ApplyCheckBox.Checked = value <= 0;
				BlankValueCheckbox.Checked = string.IsNullOrEmpty(RateValidityCalcEdit.Text);
			}
		}

		void UpdateState()
		{
			if (BlankValueCheckbox.Checked)
			{
				FieldValue = QuoteValidityRegistryItem.BlankValue;
				ApplyCheckBox.Checked = false;
			}

			RateValidityCalcEdit.ReadOnly = BlankValueCheckbox.Checked;

			if (ApplyCheckBox.Checked)
			{
				if (string.IsNullOrEmpty(RateValidityCalcEdit.Text))
				{
					RateValidityCalcEdit.Text = "0";
				}
				BlankValueCheckbox.Checked = false;
			}
		}

		void BlankCheckbox_CheckedChanged(object sender, EventArgs e)
		{
			UpdateState();
		}

		void ApplyCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			UpdateState();
		}
	}
}
