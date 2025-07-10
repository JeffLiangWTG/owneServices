using System;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Design.Testing
{
	sealed class DesignerTypesTest : TestCase
	{
		public void TestTypesExist()
		{
			AssertNotNull(DesignerTypes.TypeValueIntellisenseEditor, Type.GetType(DesignerTypes.TypeValueIntellisenseEditor));
			AssertNotNull(DesignerTypes.BindingMemberEditor, Type.GetType(DesignerTypes.BindingMemberEditor));
		}
	}
}
