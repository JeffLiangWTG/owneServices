#if NETFRAMEWORK // BindingMemberEditor is derived from UITypeEditor which is not available in .NET Core
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Design
{
	sealed class DesignerTypes_Test : TestCase
	{
		public void TestTypesExist()
		{
			AssertNotNull(DesignerTypes.BindingMemberEditor, System.Type.GetType(DesignerTypes.BindingMemberEditor));
			AssertEquals(
				"Assembly Version and PublicKeyToken should be correct",
				DesignerTypes.BindingMemberEditor, System.Type.GetType(DesignerTypes.BindingMemberEditor).AssemblyQualifiedName);
		}
	}
}
#endif