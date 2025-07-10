using System;
using System.ComponentModel;
using Enterprise.Client.UPE.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Client.UPE.GUI
{
	public partial class DogHitXRayeDocsForm : ZChildForm
	{
		[Obsolete("Design-time only", true)]
		public DogHitXRayeDocsForm()
		{
		}

		public DogHitXRayeDocsForm(UPECusHAWB uPECusHAWB)
			: base(uPECusHAWB)
		{
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			PlugIns.Add(ControllerIDs.ProcessQueue);
			SetActiveProcessQueue();
			ZFormPostingButtonsStrategy.SetupPosting(this, oPostingButtonsUserControl);
		}

		public UPECusHAWB UPECusHAWB
		{
			get { return (UPECusHAWB)BusinessEntity; }
		}

		public override string FormHeading
		{
			get { return "Dog Hit X-Ray eDocs Form"; }
		}

		void SetActiveProcessQueue()
		{
			if (UPECusHAWB.ActiveProcessQueueForBinding.Count > 0)
			{
				UPECusHAWB.ActiveProcessQueueForBinding[0].QueueType = ProcessQueueType.Enum.Customs;
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		private IContainer components;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
