using CargoWise.Types;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.AU.GUI
{
	public partial class AUCusOutturnUserControl : CusOutturnUserControl
	{
		public AUCusOutturnUserControl()
		{
			InitializeComponent();
			RemoveColumnFromGrid(Customs.Business.CusOutturn.Schema.UnderbondResponsiblePartyID);
		}

		public override void SetBindPrepend(ZString bindToPrepend)
		{
			base.SetBindPrepend(bindToPrepend);
			ResponsiblePartyIDTextBox.BindTo = bindToPrepend + ResponsiblePartyIDTextBox.BindTo;
		}
	}
}
