using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class CustomisedControlDetailsUserControl : ZUserControl, IPreviewReceiver, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public CustomisedControlDetailsUserControl()
		{
			InitializeComponent();
			InitialiseRenderOnTheWebCheckBox();
		}

		void InitialiseRenderOnTheWebCheckBox()
		{
			this.RenderOnTheWebCheckBox.ReadOnly = true;

			if (!BMSRegistry.Instance.PAVEOnTheWeb.Value)
			{
				this.RenderOnTheWebCheckBox.Visible = false;
			}
		}

		public new BMControlCustomisation DataSource
		{
			get { return base.DataSource as BMControlCustomisation ?? BindingSource.Current as BMControlCustomisation; }
		}

		#region UserControl overrides

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			((ZDropEditInternals)BackgroundColorDropEdit).SetControlWidth(ControlDpiScalingHelper.ScaleToCurrentDpiX(150));

			var propertyNameWidth = Math.Min(ControlDpiScalingHelper.ScaleToCurrentDpiX(200), ControlTypeDropEdit.Width);
			((ZDropEditInternals)PropertyNameDropEdit).SetControlWidth(propertyNameWidth);

			if (!DesignMode)
			{
				UpdatePreview();
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (DataSource != null)
			{
				DataSource.PreviewReceiver = this;
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (viewModel != null)
			{
				viewModel.SelectedControlChanged -= ViewModel_SelectedControlChanged;
			}
		}

		#endregion

		#region Preview

		ControlCustomisationViewModel viewModel;

		public void UpdatePreview()
		{
			if (!IsDisposed && IsHandleCreated)
			{
				BeginInvoke(new Action(UpdatePreviewCore)); // BeginInvoking so that clicking in a text box doesn't explode while we are disposing it.
			}
		}

		void UpdatePreviewCore()
		{
			var control = PreviewGroupBox.Controls.OfType<ZUserControl>().FirstOrDefault();
			if (control != null)
			{
				control.SetDataBinding(null, string.Empty);
			}

			PreviewGroupBox.Controls.RemoveAndDisposeAll();

			var dataSource = DataSource;

			if (dataSource != null && !dataSource.HasErrors)
			{
				dataSource.PreviewReceiver = this;
				try
				{
					if (viewModel != null)
					{
						viewModel.SelectedControlChanged -= ViewModel_SelectedControlChanged;
					}

					control = CustomisedControlRenderer.RenderForPreview(dataSource);
					viewModel = control != null ? control.Tag as ControlCustomisationViewModel : null;

					if (viewModel != null)
					{
						viewModel.SelectedControlChanged += ViewModel_SelectedControlChanged;
					}

					if (control != null)
					{
						control.Location = ControlDpiScalingHelper.NewScaledPoint(5, 15);
						PreviewGroupBox.Controls.Add(control);
					}
				}
				catch (KDataBindingException)
				{
					ShowPreviewErrorLabel(Res.GetString("C60E85CE-5B4E-44AD-8FAC-B3A434453183", "A property on this Visual Layout has been removed or renamed. \r\nPlease delete or re-set the value before saving."));
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					var additionalDetail = string.Join(System.Environment.NewLine, ex.FlattenInnerExceptions().Select(e =>
#if DEBUG
 e.ToString()
#else
 e.Message
#endif
));

					ShowPreviewErrorLabel(Res.GetString("0718d470-e0f7-4f54-a183-13ba69db01e5", "There was an error previewing the control. Please check the property values."), additionalDetail);
				}
			}
			else
			{
				ShowPreviewErrorLabel(Res.GetString("9bc6b80c-fc35-41d4-9635-9751d2bc17ae", "Please correct the validation errors."));
			}
		}

		void ShowPreviewErrorLabel(string message, string additionalDetail = null)
		{
			var label = new ZLabel
			{
				Dock = DockStyle.Fill,
				IsFontBold = true,
				TextAlign = ContentAlignment.MiddleCenter,
				Text = message,
			};

			if (additionalDetail != null)
			{
				label.Cursor = Cursors.Hand;
				label.Tag = additionalDetail;
				label.Click += (s, e) => Globals.Message.ShowInformation(additionalDetail, Res.GetString("97527219-07cc-4dd1-8af5-87529938044e", "Error Details"));
			}

			PreviewGroupBox.Controls.Add(label);
		}

		void ViewModel_SelectedControlChanged(object sender, SelectedControlChangedEventArgs e)
		{
			var customisationField = e.SelectedControl;

			if (customisationField is BMControlCustomisationLine)
			{
				SelectLine(customisationField, DataSource.CustomisationLines, PropertiesTabPage, PropertiesGrid);
			}
			else
			{
				SelectLine(customisationField, DataSource.CustomisedControls, ControlsTabPage, ControlsGrid);
			}
		}

		void SelectLine(ControlCustomisationBase customisationField, IBusinessObjectCollection customisationCollection, ZTabPage tabPage, ZGrid grid)
		{
			PropertiesTabControl.SelectedTab = tabPage;

			grid.UnSelectAll();

			for (int i = 0; i < customisationCollection.Count; i++)
			{
				if (customisationCollection[i] == customisationField && i < grid.ListManager.Count)
				{
					grid.Select(i);
					grid.ListManager.Position = i;
					break;
				}
			}
		}

		#endregion

		#region BackgroundImage

		void BackgroundImageButton_Click(object sender, EventArgs e)
		{
			var form = new ImageSelectionForm();
			var result = ZFormModaliser.ShowDialogAndDispose(form);
			if (result == DialogResult.OK)
			{
				var image = form.Logo;
				if (!image.IsEmpty)
				{
					DataSource.BackgroundImage = image;
				}
			}
		}

		void ClearBackgroundImageButton_Click(object sender, EventArgs e)
		{
			DataSource.BackgroundImage = ZBlob.Empty;
		}

		#endregion

		#region IAllowTabBackwardBetweenSomeOfMyChildren

		public bool AllowTabBackward(Control control, Control previousControl)
		{
			return control is ZDropEdit || previousControl is ZDropEdit;
		}

		#endregion
	}
}
