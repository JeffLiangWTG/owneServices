using System.Linq;
using System.Windows.Forms;
using CargoWise.Main.Startup.Tools;
using CargoWise.Main.Startup.Tools.JetBrains;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Startup.Tools.JetBrains.Testing
{
	[TestedType(typeof(ProfileMemoryForm))]
	sealed class ProfileMemoryFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var form = new ProfileMemoryForm(new ProfileMemoryModel(new Mock<IDialogService>().Object));

			var txtLogs = form.Controls.Find("txtLogs", true).Single();
			MissingResourceStringChecker.ExcludeFromTest(txtLogs);

			return form;
		}
	}
}
