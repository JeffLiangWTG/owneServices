
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Module;

namespace Enterprise.Client.Wow
{
	public class WoolworthsJobDeclarationControllerOverride : JobDeclarationController
	{
		protected override Enterprise.ZArchitecture.GUI.IZForm GetFormCore(IBusiness businessEntity)
		{
			return new WowAUCustomsDeclarationForm((WoolworthsJobDeclaration)businessEntity);
		}
	}
}
