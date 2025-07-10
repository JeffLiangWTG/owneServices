using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class AirCTOForm : CMRMessagingForm
	{
		public AirCTOForm(CTOCusMAWB mAWB) : base(mAWB)
		{
			PlugIns.AddJobInvoicing(mAWB.InvoicingSupporter);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
		}

		#region Implementation

		protected override Business.MultiMessageManager GetManager()
		{
			return new CTOCusMAWBMessageManager(() => (CTOCusMAWB)BusinessEntity);
		}

		protected override MenuItem GetMessagingMenu()
		{
			return new AirCTOMenu((CTOCusMAWBMessageManager)Manager);
		}

		public override string FormCaption
		{
			get { return "Air CTO - Import"; }
		}

		public override bool IsResizableByTabPageAllowed => true;

		#endregion
	}
}
