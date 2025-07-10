using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class AirCargoOutturnBillsForm : CMRMessagingForm
	{
		public AirCargoOutturnBillsForm(CusUnderbond underbond)
			: base(underbond)
		{
			InitializeComponent();
			underbondControl.CurrentUnderbond = underbond;
			PlugIns.AddJobInvoicing(underbond.InvoicingSupporter);
			workflowTabPage.Initialize(underbond);
		}

		CusUnderbond Underbond
		{
			get { return (CusUnderbond)base.BusinessEntity; }
		}

		public override string FormCaption
		{
			get { return "Air Cargo Depot Outturn"; }
		}

		public override bool IsResizableByTabPageAllowed => true;

		#region Messaging

		protected override MenuItem GetMessagingMenu()
		{
			return AirCargoOutturnBillsMenu.New((CusUnderbondMessageManager)Manager);
		}

		protected override Business.MultiMessageManager GetManager()
		{
			return new CusUnderbondMessageManager(Underbond);
		}

		#endregion
	}
}
