using System.Windows.Forms;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class NonPersistentSplitCreatorForm : ZChildForm
	{
		public NonPersistentSplitCreatorForm(ICcsukCusAwb awb)
		{
			var controller = new NonPersistentSplitLineOrchestrator(awb);
			this.controller = controller;
			var grid = new NonPersistentSplitCollectionUserControl(controller, Close);
			grid.Dock = System.Windows.Forms.DockStyle.Fill;
			Controls.Add(grid);
			grid.BringToFront();
			InitializeComponent();
		}

		protected override void OnLoad(System.EventArgs e)
		{
			base.OnLoad(e);
			QueueWorker();
		}

		#region Update totals
		/// <summary>
		/// This is pretty stinkin' ugly.... but if you do it the "right" way then you cannot commit new rows by pressing the down arrow key, as the cursor jumps around all over the place. 
		/// </summary>
		void QueueWorker()
		{
			UserIdleWorker.QueueWorkItem(this, 250, new MethodInvoker(UpdateTotalAndReQueueWorker), null);
		}

		void UpdateTotalAndReQueueWorker()
		{
			controller.SplitsAndFlightData.UpdateTotal();
			if (Globals.IsTest)
			{
			}
			else
			{
				UserIdleWorker.QueueWorkItem(this, 250, new MethodInvoker(UpdateTotalAndReQueueWorker), null);
			}
		}
		#endregion

		internal NonPersistentSplitLineOrchestrator controller;
	}
}
