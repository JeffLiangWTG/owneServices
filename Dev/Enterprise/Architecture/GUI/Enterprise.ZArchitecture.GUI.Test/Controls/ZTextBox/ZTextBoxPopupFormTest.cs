using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZTextBoxPopupFormTest : TestCaseWithFactory
	{
		public void TestCtorSetsTextToParentText()
		{
			using (var parentTextBox = new ZTextBox())
			{
				parentTextBox.Text = "TEST VALUE";
				parentTextBox.SelectionStart = 1;
				parentTextBox.SelectionLength = 2;

				using (var popupForm = new TestZTextBoxPopupForm(parentTextBox))
				{
					AssertEquals(parentTextBox.Text, popupForm.TestTextBox.Text);
					AssertEquals(parentTextBox.SelectionStart, popupForm.TestTextBox.SelectionStart);
					AssertEquals(parentTextBox.SelectionLength, popupForm.TestTextBox.SelectionLength);
				}
			}
		}

		public void TestCtorSetsCharacterCasing()
		{
			foreach (CharacterCasing casing in Enum.GetValues(typeof(CharacterCasing)))
			{
				using (var parentTextBox = new ZTextBox())
				{
					parentTextBox.CharacterCasing = casing;
					parentTextBox.Text = "Test Value";

					using (var popupForm = new TestZTextBoxPopupForm(parentTextBox))
					{
						AssertEquals(parentTextBox.Text, popupForm.TestTextBox.Text);
						AssertEquals("CharacterCasing", casing, popupForm.TestTextBox.CharacterCasing);
					}
				}
			}
		}

		public void TestOkSetsParentText()
		{
			using (var parentTextBox = new ZTextBox())
			using (var popupForm = new TestZTextBoxPopupForm(parentTextBox))
			{
				parentTextBox.Text = "TEST";
				popupForm.Show();
				TextBox testTextBox = popupForm.TestTextBox;
				testTextBox.Text = "";
				popupForm.TestOkButton.PerformClick();

				AssertEquals("", parentTextBox.Text);
			}

			using (var parentTextBox = new ZTextBox())
			using (var popupForm = new TestZTextBoxPopupForm(parentTextBox))
			{
				parentTextBox.Text = "";
				popupForm.Show();
				TextBox testTextBox = popupForm.TestTextBox;
				testTextBox.Text = "TEST";
				popupForm.TestOkButton.PerformClick();

				AssertEquals("TEST", parentTextBox.Text);
			}
		}

		[DeveloperOnlyTest]
		public void TestZTextBoxPopupFormTruncateTooLongString()
		{
			var tooLongString = "MERCI DE PREVOIR LA TRACTION DES CONTENEURS 20'FLAT POUR DECHARGEMENT CAMION ET EMPOTAGE VOS SOINS TRACTION PORTUAIRE A/R TC / RÉCEPTION CAISSES / EMPOTAGE /CALAGE-SAISISSAGE SELON TON MAIL DU 19/2 DIMENSION 1 CAISSE :L 3.10M X L 2.70M X H 1.70M POIDS 1 CAISSE 5000KGS ENVIRON MAD A SUIVRE DES QUE POSSIBLE LIVRAISON LE 16/03 CHEZ TOI TRACTION DE RETOUR IMPERATIF LE 17 AU PLUS TARD MATIN NE PAS HESITER A SIGNALER TOUT PROBLEME .";
			using (var parentTextBox = new ZTextBox())
			{
				parentTextBox.MaxLength = 256;
				Assert("The text must be longer than parentTextBox MaxLength", tooLongString.Length > parentTextBox.MaxLength);

				using (var popupForm = new TestZTextBoxPopupForm(parentTextBox))
				{
					popupForm.Show();

					SafeClipboard.SetText(tooLongString);
					popupForm.TestTextBox.Paste();
					AssertEquals("Text should be truncated down to MaxLength", parentTextBox.MaxLength, popupForm.TestTextBox.Text.Length);

					popupForm.TestTextBox.Text = string.Empty;
					Application.DoEvents();

					foreach (var c in tooLongString)
					{
						KeySender.SendKeyPress(popupForm.TestTextBox, c);
					}

					AssertEquals("Text should be truncated down to MaxLength", parentTextBox.MaxLength, popupForm.TestTextBox.Text.Length);
				}
			}
		}

		class TestZTextBoxPopupForm : ZTextBoxPopupForm
		{
			public TestZTextBoxPopupForm(ZTextBox parentTextBox)
				: base(parentTextBox)
			{
			}

			public ZTextBox TestTextBox
			{
				get { return (ZTextBox)GetControl("textBox"); }
			}

			public ZButton TestOkButton
			{
				get { return (ZButton)GetControl("okButton"); }
			}

			public ZButton TestCancelButton
			{
				get { return (ZButton)GetControl("cancelButton"); }
			}

			Control GetControl(string name)
			{
				return GetControl(name, this, true);
			}

			Control GetControl(string name, Control parentControl, bool assertNotNull)
			{
				Control result = null;
				foreach (Control control in parentControl.Controls)
				{
					if (control.Name == name)
					{
						result = control;
						break;
					}

					result = GetControl(name, control, false);
					if (result != null)
					{
						break;
					}
				}

				if (assertNotNull)
				{
					AssertNotNull(result);
				}

				return result;
			}
		}
	}
}
