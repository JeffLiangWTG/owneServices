using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public class ProgressFormWrapper : IDisposable
	{
		public ProgressFormWrapper(Form parentForm, ResourceStringData progressFormCaption, bool allowCancel, bool showFullLog, params IProgressFormSupportable[] progressFormSupportableBizO)
		{
			Argument.NotNull(parentForm, "parentForm");
			Argument.NotNull(progressFormSupportableBizO, "progressFormSupportableBizO");

			this.parentForm = parentForm;
			this.progressFormSupportableBizO = progressFormSupportableBizO;
			this.progressFormCaption = progressFormCaption;
			this.allowCancel = allowCancel;
			this.showFullLog = showFullLog;
			this.logs = new ZStringBuilder(Res.GetString("6ccf28d3-d0fa-414a-bb17-36277aa00ec3", "============================LOG============================"));
			this.logs.AppendLine();
		}

		readonly protected IProgressFormSupportable[] progressFormSupportableBizO;
		readonly protected Form parentForm;
		readonly protected ZStringBuilder logs;
		readonly ResourceStringData progressFormCaption;
		readonly bool allowCancel;
		readonly bool showFullLog;

		protected ProgressForm progressForm;

		public virtual void ShowProgressForm()
		{
			if (progressFormSupportableBizO.Length > 1)
			{
				progressForm = new MultistepProgressForm(StepNumber);
			}
			else
			{
				progressForm = new ProgressForm();
			}

			progressForm.ShowCancelButton = allowCancel;
			progressForm.CaptionResourceString = progressFormCaption;
			progressFormSupportableBizO.ForEach(x => x.RaiseProgressUpdateEvent += RaiseProgressUpdateEventHandler);

			ZFormModaliser.Show(progressForm, parentForm);
		}

		public void Dispose()
		{
			if (progressForm != null)
			{
				progressForm.Dispose();
			}
			if (progressFormSupportableBizO != null)
			{
				progressFormSupportableBizO.ForEach(x => x.RaiseProgressUpdateEvent -= RaiseProgressUpdateEventHandler);
			}
		}

		protected virtual void RaiseProgressUpdateEventHandler(IProgressFormSupportable sender, bool isProcessCompleted)
		{
			if (sender != null)
			{
				progressForm.SetStatusAndPercentComplete(sender.CurrentStatusText, PercentageComplete(sender));

				if (isProcessCompleted)
				{
					if (showFullLog)
					{
						Globals.Message.ShowInformation(sender.Log, progressFormCaption.FullDescription);
					}
				}
			}
		}

		protected virtual int StepNumber
		{
			get { return progressFormSupportableBizO.Length; }
		}

		protected virtual int PercentageComplete(IProgressFormSupportable bizO)
		{
			return Convert.ToInt32(bizO.CompletedItems / (float)bizO.TotaItemsToComplete * 100);
		}
	}
}
