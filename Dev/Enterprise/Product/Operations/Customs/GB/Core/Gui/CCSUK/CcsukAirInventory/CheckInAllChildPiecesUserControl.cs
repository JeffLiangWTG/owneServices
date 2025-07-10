using CargoWise.Common;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class CheckInAllChildPiecesUserControl : ZUserControl
	{
		public CheckInAllChildPiecesUserControl(NonPersistentCheckInAllChildPiecesOrchestrator orchestrator, VoidMethodToClose closeMethod)
		{
			Argument.NotNull(orchestrator, "orchestrator");
			this.orchestrator = orchestrator;
			closeFormMethod = closeMethod;
			base.SetDataBinding(this.orchestrator.CheckInAllChildPiecesData, "");
			InitializeComponent();
		}

		void ButtonOk_Click(object sender, System.EventArgs e)
		{
			var result = orchestrator.Mawb.CheckInAllChildPieces(orchestrator.CheckInAllChildPiecesData);
			if (!result.IsNullOrEmpty())
			{
				Globals.Message.Show(result);
			}
			closeFormMethod();
		}

		void ButtonCancel_Click(object sender, System.EventArgs e)
		{
			closeFormMethod();
		}

		void CheckInAllChildPiecesUserControl_Load(object sender, System.EventArgs e)
		{
		}

		public delegate void VoidMethodToClose();
		readonly NonPersistentCheckInAllChildPiecesOrchestrator orchestrator;
		readonly VoidMethodToClose closeFormMethod;
	}
}
