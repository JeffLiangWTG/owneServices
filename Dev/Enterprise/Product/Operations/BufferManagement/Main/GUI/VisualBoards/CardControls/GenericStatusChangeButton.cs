using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public class GenericStatusChangeButton : ZUserControl
	{
		public GenericStatusChangeButton(ITaskCardComponentParent parent, ZString statusChangeValue, Func<Bitmap> selectedPictureGetter, Func<Bitmap> unSelectedPictureGetter, Func<ProcessTask, bool> selectedStatusFunc, string clickBehaviour)
		{
			button = new ZButton();
			button.BackColor = SystemColors.Control; // Overriding the system theming which gets set in the constructor.
			button.Dock = DockStyle.Fill;
			button.UseVisualStyleBackColor = false;
			button.BackgroundImageLayout = ImageLayout.Stretch;
			Controls.Add(button);

			this.parent = parent;
			this.StatusChangeValue = statusChangeValue;

			this.selectedPictureGetter = selectedPictureGetter;
			this.unSelectedPictureGetter = unSelectedPictureGetter;
			this.selectedStatusFunc = selectedStatusFunc;
			this.clickBehaviour = clickBehaviour;

			parent.Saved += OnParentSaved;

			button.Click += Button_Click;
			UpdateButtonGraphics();
		}

		public readonly ZString StatusChangeValue;
		readonly ZButton button;
		readonly ITaskCardComponentParent parent;
		readonly Func<Bitmap> selectedPictureGetter;
		readonly Func<Bitmap> unSelectedPictureGetter;
		readonly Func<ProcessTask, bool> selectedStatusFunc;
		readonly string clickBehaviour;

		void Button_Click(object sender, EventArgs e)
		{
			parent.StatusUpdated -= OnStatusUpdated;
			parent.StatusUpdated += OnStatusUpdated;
			UpdateStatus(StatusChangeValue);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			UpdateButtonGraphics();
		}

		void UpdateStatus(string newStatus)
		{
			parent?.UpdateStatus(newStatus);
			UpdateButtonGraphics();
		}

		internal void UpdateButtonGraphics()
		{
			if (!IsDisposed && !Disposing)
			{
				if (parent.Task != null && !parent.Task.IsDeleted && selectedStatusFunc(parent.Task))
				{
					button.BackgroundImage = selectedPictureGetter();
				}
				else
				{
					button.BackgroundImage = unSelectedPictureGetter();
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				parent.StatusUpdated -= OnStatusUpdated;
				parent.Saved -= OnParentSaved;
			}

			base.Dispose(disposing);
		}

		#region Click Behaviour

		void OnStatusUpdated(object sender, StatusUpdatedEventArgs e)
		{
			BeginInvoke(new Action(UpdateButtonGraphics));

			if (e.NewStatus == StatusChangeValue && clickBehaviour.In(StatusButtonBehaviorOptionsList.Codes.SaveAndClose, StatusButtonBehaviorOptionsList.Codes.OpenJob)
				&& !(parent.Task.P9_ActualDurationInfo.HasError(ProcessTaskValidation.ActualDurationErrorMessage)))
			{
				BeginInvoke(new Action(() => parent.Save(this)));
			}
		}

		void OnParentSaved(object sender, TasksSavedArgs e)
		{
			if (clickBehaviour == StatusButtonBehaviorOptionsList.Codes.OpenJob && sender == this)
			{
				parent.ShowParent();
			}
		}

		#endregion

		#region For Test

		public void PerformClick_ForTest()
		{
			button.PerformClick();
		}

		#endregion
	}
}
