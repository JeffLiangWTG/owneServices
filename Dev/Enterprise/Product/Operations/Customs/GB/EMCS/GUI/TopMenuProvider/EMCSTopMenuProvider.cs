using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.EU.EMCS.GUI;

namespace Enterprise.Customs.GB.EMCS.GUI
{
	public class EMCSTopMenuProvider : IEMCSTopMenuProvider
	{
		public EU.EMCS.GUI.EMCSMenu TopLevelMenu(EMCSJobDeclaration declaration)
		{
			return new EMCSMenu(declaration);
		}
	}
}
