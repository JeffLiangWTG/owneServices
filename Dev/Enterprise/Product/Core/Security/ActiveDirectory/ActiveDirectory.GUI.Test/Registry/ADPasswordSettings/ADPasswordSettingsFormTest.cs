using System.Windows.Forms;
using CargoWise.ActiveDirectory;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.GUI.Test
{
	[TestedType(typeof(ADPasswordSettingsForm))]
	class ADPasswordSettingsFormTest : ZFormBasherTest
	{
		public void TestEnsureFormCaptionNotMissing()
		{
			using (var form = new ADPasswordSettingsForm())
			{
				AssertEquals("Override Domain Password Policy", form.FormCaption);
			}
		}

		#region Implementations

		protected override Form GetFormToBashCore() => new ADPasswordSettingsForm();

		protected override void SetUp()
		{
			base.SetUp();
			var directorySearcherMock = new Mock<IDirectorySearcher>();
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = directorySearcherMock.Object;
		}

		#endregion
	}
}
