using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public class FormattedProcedureCodeFindBox : ZCodeFindBox
	{
		protected override string Code { get => base.Code.Replace(" ", ""); set => base.Code = value; }
	}
}
