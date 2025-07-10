using Enterprise.Client.JAS.GUI.Cognos;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Module.Testing
{
	[TestedType(typeof(AccGLAccountDescriptorController))]
	class CognosAccGLAccountDescriptorControllerTest : AccGLAccountDescriptorControllerTest
	{
		public void TestGetForm()
		{
			AssertEquals(typeof(CognosAccGLAccountDescriptorForm), Controller.ShowNewForm().GetType());
		}
	}
}
