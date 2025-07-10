using System;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class LVSLineDetailsUserControl : ZUserControl
	{
		public LVSLineDetailsUserControl()
		{
			InitializeComponent();
			Resize += LVSLineDetailsUserControl_Resize;

			ClassificationTariffUserControlHelper.UpdateTariffFindBoxToGetTariffFromSRDb(
				ClassificationNumberFindBox,
				"ClassificationNumberFrimSRDbFindBox",
				() => { return Line?.EffectiveDateForDutyRate ?? ZDateTime.Today; },
				(x) =>
				{
					this.BindingSource.SetBindingMember(x, JobComInvoiceLine.Schema.JI_FormattedTariff);
					CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(null)).JI_FormattedTariff);
				}
				);
		}

		JobComInvoiceLine Line
		{
			get { return (JobComInvoiceLine)CurrentDataItem; }
		}

		#region SplitContainer Layout Bug Fix

		//TODO: Replace this code with new ZSplitContainer when ready

		void LVSLineDetailsUserControl_Resize(object sender, EventArgs e)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				if (workItem != null)
				{
					workItem.Dispose();
				}

				workItem = UserIdleWorker.QueueWorkItem(this, 0, new Action(RefreshSplitter));
			}
		}

		void RefreshSplitter()
		{
			SplitContainer.Panel1.SuspendLayout();
			SplitContainer.Panel2.SuspendLayout();
			if (shouldMoveLeft)
			{
				SplitContainer.SplitterDistance--;
				shouldMoveLeft = false;
			}
			else
			{
				SplitContainer.SplitterDistance++;
				shouldMoveLeft = true;
			}
			SplitContainer.Panel1.ResumeLayout();
			SplitContainer.Panel2.ResumeLayout();
		}

		IDisposable workItem;
		bool shouldMoveLeft;

		#endregion

		#region Visibility

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
