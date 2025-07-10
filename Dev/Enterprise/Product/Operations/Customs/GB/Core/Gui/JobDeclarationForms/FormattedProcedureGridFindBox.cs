using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.GB.GUI
{
	public class FormattedProcedureGridFindBox : ZGridFindBox
	{
		public override ZCodeBox GetCodeBox()
		{
			return new FormattedProcedureColumnCodeBox(this);
		}
	}
}
