#if DEBUG
using CargoWise.ComponentModel.Design;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class DesignerTypesTests : TestCase
	{
		public void TestTypesExist()
		{
			AssertNotNull(DesignerTypes.UITypeEditor);
			AssertNotNull(DesignerTypes.BindingMemberEditor);
		}
	}
}
#endif
