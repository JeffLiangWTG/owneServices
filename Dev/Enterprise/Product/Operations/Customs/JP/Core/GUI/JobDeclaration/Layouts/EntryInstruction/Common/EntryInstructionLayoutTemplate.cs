using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public partial class EntryInstructionLayoutTemplate : ZUserControl
	{
		public EntryInstructionLayoutTemplate()
		{
			InitializeComponent();
			SetupComponents();
		}

		void SetupComponents()
		{
			CargoQuantityCalcDropEdit.AllowNegative = false;
			WeightzCalcDropEdit.AllowNegative = false;
		}
	}
}
