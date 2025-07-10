using System;
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	internal partial class ColumnConfigurationFieldUserControl : RuntimeOptionUserControl
	{
		public ColumnConfigurationFieldUserControl()
		{
			InitializeComponent();
		}

		public override void SetFilter(FilterField filter)
		{
			base.SetFilter(filter);

			ClearColumnConfigField();

			ColumnConfigField = (ColumnConfigurationField)filter;
			ColumnConfigField.ValueChanged += ColumnConfigField_ValueChange;
			ColumnConfigField.ConfigurationsSavedORDeleted += ColumnConfigField_ConfigurationsSavedORDeleted;
			comboSettings.SelectedIndexChanged += comboSettings_SelectedIndexChanged;
			LoadDropDown();

			foreach (ColumnConfigurationManager item in comboSettings.Items)
			{
				if (ColumnConfigField.Value != null &&
					item.UniqueDescription == ColumnConfigField.Value.UniqueDescription)
				{
					comboSettings.SelectedItem = item;
					break;
				}
			}

			if (comboSettings.SelectedIndex < 0)
			{
				comboSettings.SelectedIndex = 0;
			}
		}

		void ColumnConfigField_ConfigurationsSavedORDeleted(object sender, EventArgs e)
		{
			LoadDropDown();
			comboSettings.SelectedItem = ColumnConfigField.Value;
		}

		void ColumnConfigField_ValueChange(object sender, EventArgs e)
		{
			if ((ColumnConfigurationManager)comboSettings.SelectedItem != ColumnConfigField.Value && ColumnConfigField.Value != null)
			{
				comboSettings.SelectedItem = ColumnConfigField.Value;
			}
		}

		void LoadDropDown()
		{
			comboSettings.Items.Clear();
			comboSettings.Items.AddRange([.. ColumnConfigField.SavedConfigurations]);
			comboSettings.UpdateDropDownWidth();
		}

		void comboSettings_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (ColumnConfigField.Value != (ColumnConfigurationManager)comboSettings.SelectedItem)
			{
				ColumnConfigField.Value = (ColumnConfigurationManager)comboSettings.SelectedItem;
			}
		}

		public override Type ExpectedFilterType()
		{
			return typeof(ColumnConfigurationField);
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					ClearColumnConfigField();
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		void ClearColumnConfigField()
		{
			if (ColumnConfigField != null)
			{
				ColumnConfigField.ValueChanged -= ColumnConfigField_ValueChange;
				ColumnConfigField.ConfigurationsSavedORDeleted -= ColumnConfigField_ConfigurationsSavedORDeleted;
				ColumnConfigField = null;
			}

			if (comboSettings != null)
			{
				comboSettings.SelectedIndexChanged -= comboSettings_SelectedIndexChanged;
			}
		}

		ColumnConfigurationField ColumnConfigField;
	}
}
