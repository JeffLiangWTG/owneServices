using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(UserDefinedFilterEmbeddedModulePopup))]
	sealed class UserDefinedFilterEmbeddedModulePopupBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var filter = FilterStripsTestHelper.CreateModuleUserDefinedFilterAndSavedFilterLayout();

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				return new UserDefinedFilterEmbeddedModulePopup(module, filter);
			}
		}

		public override bool AllowUntranslatableFormTitle()
		{
			return true;
		}
	}
}
