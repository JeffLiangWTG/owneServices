using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public interface IEMCSTopMenuProvider
	{
		EMCSMenu TopLevelMenu(EMCSJobDeclaration declaration);
	}
}
