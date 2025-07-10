
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.JAS.Module
{
	public class CognosAccGLAccountDescriptorModule : AccGLAccountDescriptorModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CognosAccGLAccountDescriptorFilterBusinessObject();
		}
	}
}
