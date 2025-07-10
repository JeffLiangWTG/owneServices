using System.Windows.Forms;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class MessageBoxForRulingTest : TestCase
	{
		public void TestUserEventDiagnosticReference()
		{
			using (var messageBox = new MessageBoxForRuling())
			{
				AssertEquals("Caption = \"Create Remission\", Message = \"The Special Authority Number does not exist in the Remissions Maintenance. Would you like to add it now?\"", UserEventDiagnosticReferenceAttribute.Render(messageBox));
			}
		}

		public void TestIsCreateForAllOrgs()
		{
			using (var messageBox = new MessageBoxForRuling())
			{
				messageBox.Show();
				Application.DoEvents();

				var withImporterRadioButton = messageBox.WithImporterRadioButton;
				var withoutImporterRadioButton = messageBox.WithoutImporterRadioButton;

				withImporterRadioButton.Checked = true;
				Assert("Should be false as the user select 'Add as Organization specific'.", !messageBox.IsCreateForAllOrgs);

				withoutImporterRadioButton.Checked = true;
				Assert("Should be true as the user select 'Add for all Organizations'.", messageBox.IsCreateForAllOrgs);
			}
		}
	}
}
