using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.Module
{
	public partial class ModuleDurationFilterControl : ZUserControl
	{
		const int DefaultSpacing = 20;

		public ModuleDurationFilterControl()
		{
			InitializeComponent();

			SetupScopeChangeHandler();
			UpdateControlVisibility(true);
		}

		void SetupScopeChangeHandler()
		{
			scopeDropEdit.SelectedIndexChanged += ScopeDropEdit_SelectedValueChanged;
		}

		void ScopeDropEdit_SelectedValueChanged(object sender, EventArgs e)
		{
			UpdateControlVisibility();
		}

		void UpdateControlVisibility(bool isInitializing = false)
		{
			string currentScope = scopeDropEdit.Text;

			if (string.IsNullOrEmpty(currentScope))
			{
				if (isInitializing)
				{
					currentScope = ModuleDurationFilter.SearchTexts.Between;
				}
				else
				{
					return;
				}
			}

			if (currentScope == ModuleDurationFilter.SearchTexts.Between)
			{
				minTimeEdit.Visible = true;
				maxTimeEdit.Visible = true;
				andLabel.Visible = true;

				minTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(
					scopeDropEdit.Right + CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(DefaultSpacing),
					0, false);

				andLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(
					minTimeEdit.Right + CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(5),
					minTimeEdit.Top + (minTimeEdit.Height - andLabel.Height) / 2, false);

				maxTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(
					andLabel.Right + CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(5),
					0, false);

				Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(
					maxTimeEdit.Right + CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(5), 25, true);
			}
			else
			{
				minTimeEdit.Visible = true;
				maxTimeEdit.Visible = false;
				andLabel.Visible = false;

				minTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(
					scopeDropEdit.Right + CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(DefaultSpacing),
					0, false);

				Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(
					minTimeEdit.Right + CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(5), 25, true);
			}

			this.Refresh();
		}

		public void ConfigureLayout(int spacing)
		{
			minTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(
				scopeDropEdit.Right + CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(spacing),
				0, false);

			if (maxTimeEdit.Visible)
			{
				andLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(
					minTimeEdit.Right + CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(5),
					minTimeEdit.Top + (minTimeEdit.Height - andLabel.Height) / 2, false);

				maxTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(
					andLabel.Right + CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(5),
					0, false);

				Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(
					maxTimeEdit.Right + CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(5), 25, true);
			}
			else
			{
				Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(
					minTimeEdit.Right + CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(5), 25, true);
			}

			this.Refresh();
		}

		protected override void OnBindingContextChanged(EventArgs e)
		{
			base.OnBindingContextChanged(e);

			if (BindingSource.Current is ModuleDurationFilter)
			{
				UpdateControlVisibility();
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UpdateControlVisibility();
		}
	}
}
