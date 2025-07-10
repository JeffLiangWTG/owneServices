using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public class MiscOptionsLayoutBuilder<T> : CommonMiscOptionsLayoutBuilder<T> where T : BaseJobDeclaration
	{
		protected override int MaxColumns => 2;
	}
}
