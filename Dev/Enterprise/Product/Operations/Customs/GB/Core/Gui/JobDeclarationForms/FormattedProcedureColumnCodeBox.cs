using Enterprise.ZArchitecture.GUI.Internal;
using static Enterprise.ZArchitecture.GUI.Internal.ZFindBoxUserControl;

namespace Enterprise.Customs.GB.GUI
{
	public class FormattedProcedureColumnCodeBox : ZCodeBox
	{
		public FormattedProcedureColumnCodeBox(IFindBoxUserControl parentFindBox) : base(parentFindBox)
		{
		}

		public override string Text { get => base.Text.Replace(" ", ""); set => base.Text = value; }
	}
}
