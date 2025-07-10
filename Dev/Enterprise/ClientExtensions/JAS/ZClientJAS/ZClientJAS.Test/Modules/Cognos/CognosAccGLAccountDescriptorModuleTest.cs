using System.Reflection;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Module.Testing
{
	[TestedType(typeof(CognosAccGLAccountDescriptorModule))]
	class CognosAccGLAccountDescriptorModuleTest : AccGLAccountDescriptorModuleTest
	{
		public void TestFilterBusinessObject()
		{
			using (ZModule module = ZModuleFactory.Instance.Create(ModuleIDs.AccGLAccountDescriptor))
			{
				object filterBizO = typeof(CognosAccGLAccountDescriptorModule).GetMethod("GetNewFilterBusinessObject", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(module, null);
				AssertEquals(typeof(CognosAccGLAccountDescriptorFilterBusinessObject), filterBizO.GetType());
			}
		}
	}
}
