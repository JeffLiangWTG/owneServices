using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class TypeDescriptorProviderAttributeCheckerTest : TestCase
	{
		public void TestAttributeChecking()
		{
			AssertEquals(true, TypeDescriptionProviderAttributeChecker.IsAppliedTo(typeof(ZTextBox)));
			AssertEquals(false, TypeDescriptionProviderAttributeChecker.IsAppliedTo(typeof(TextBox)));
		}
	}
}
