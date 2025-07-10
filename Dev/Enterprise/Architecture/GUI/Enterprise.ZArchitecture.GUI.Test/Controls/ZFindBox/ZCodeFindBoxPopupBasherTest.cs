using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(ZCodeFindBoxPopup))]
	sealed class ZCodeFindBoxPopupBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ZCodeFindBoxPopup("ZCodeFindBoxPopupBasher");
		}
	}
}
