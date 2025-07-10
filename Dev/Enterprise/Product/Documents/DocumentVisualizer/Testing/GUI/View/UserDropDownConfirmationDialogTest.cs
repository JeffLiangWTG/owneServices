using System.Windows.Forms;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	[TestedType(typeof(UserDropDownConfirmationDialog))]
	sealed class UserDropDownConfirmationDialogTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("aaa", "aaa desc");
			list.AddPair("bbb", "bbb desc");
			list.AddPair("ccc", "ccc desc");

			return new UserDropDownConfirmationDialog(
				"message",
				"caption",
				list);
		}
	}
}
