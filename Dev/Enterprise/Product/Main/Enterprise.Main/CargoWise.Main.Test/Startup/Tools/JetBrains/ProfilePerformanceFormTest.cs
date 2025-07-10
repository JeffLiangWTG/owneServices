using System.Linq;
using System.Windows.Forms;
using CargoWise.Main.Startup.Tools;
using CargoWise.Main.Startup.Tools.JetBrains;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Startup.Tools.JetBrains.Testing
{
	[TestedType(typeof(ProfilePerformanceForm))]
	sealed class ProfilePerformanceFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var form = new ProfilePerformanceForm(new ProfilePerformanceModel(new Mock<IDialogService>().Object, ProfilingType.SAMPLING));

			var txtLogs = form.Controls.Find("txtLogs", true).Single();
			MissingResourceStringChecker.ExcludeFromTest(txtLogs);

			return form;
		}
	}
}
