
using CargoWise.EntityFramework;
using Enterprise.Client.JAS.Business.Cognos;
using Enterprise.Client.JAS.GUI.Cognos;
using Enterprise.MasterFiles.Module;

namespace Enterprise.Client.JAS.Module
{
	public class CognosAccGLAccountDescriptorController : AccGLAccountDescriptorController
	{
		protected override ZArchitecture.GUI.IZForm GetForm(IBusiness businessEntity)
		{
			return new CognosAccGLAccountDescriptorForm(businessEntity as CognosAccGLAccountDescriptor);
		}
	}
}
