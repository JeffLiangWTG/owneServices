using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(PopupStatusChangeForm))]
	sealed class PopupStatusChangeFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new PopupStatusChangeForm();
	}
}
