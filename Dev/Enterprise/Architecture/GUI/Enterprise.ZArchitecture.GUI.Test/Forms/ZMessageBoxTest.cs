using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Controls.Internal;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Environment;
using Enterprise.Core.Forms;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZMessageBoxTestForDbUpgrade : TestCase
	{
		[UseSnapshotProtection]
		public void TestZMessageBoxIsConstructedWhenDatabaseUpgrade()
		{
			using (((IDbUpgradeSupport)Db.Instance).SetUpgradeWorkingInProgress())
			using (var adminConnection = Db.NewAdminConnection())
			{
				_ = SystemDataRegistry.Instance.ColorTheme;
				var actualDbEnv = DbEnv.Instance;
				var databaseMajorSchemaVersion = DbRegistry.DatabaseMajorSchemaVersion.LoadValue(adminConnection);
				DbEnv.SetDbEnvironment(new BaseDbEnvironment());
				try
				{
					AssertEquals(DbLockoutState.AquiredLockout, adminConnection.AcquireLockout());
					try
					{
						DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);

						AssertExceptionThrown(typeof(DatabaseUpgradeInProgressException), () => Db.Connection.BeginTransaction());

						AssertNoExceptionThrown(() => { UserNotification.Instance.ShowError("Test Connection when upgrading"); });
					}
					finally
					{
						adminConnection.ResetLockout();
					}
				}
				finally
				{
					DbRegistry.DatabaseMajorSchemaVersion.SaveValue(databaseMajorSchemaVersion, adminConnection);
					Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = false;
					DbEnv.SetDbEnvironment(actualDbEnv);
				}
			}
		}
	}

	public class ZMessageBoxTest : TransactionedTestCase
	{
		public void TestUserEventDiagnosticReference()
		{
			using (var box = new ZMessageBox("message", "caption", MessageBoxButtons.OK, MessageBoxIcon.Information))
			{
				AssertEquals("Caption = \"caption\", Message = \"message\"", UserEventDiagnosticReferenceAttribute.Render(box));
			}
		}

		public void TestWordBreak()
		{
			var text = "Hello this is my new messagebox " + new string('a', 200);
			using (var msgBox = NewMessageBox(text, "Ellie's Test", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk))
			{
				msgBox.Show();
				var msgBoxTextHeight = msgBox.TextBox.Height;
				var measureSize = msgBox.MeasureTextSize();
				var expectedHeight = measureSize.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(15);

				AssertEquals("The height (px) should be tall enough to fit 3 lines of text", (int)expectedHeight, msgBoxTextHeight);
			}
		}

		public void TestMessageBoxHeight()
		{
			var text = "Hello this is my new messagebox " + new string('a', 200);
			using (var msgBox = NewMessageBox(text, "Ellie's Test", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk))
			{
				msgBox.Show();
				var measureSize = msgBox.MeasureTextSize();
				var msgBoxHeight = msgBox.TextBox.Height - measureSize.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(15) + msgBox.Height;
				AssertEquals((int)msgBoxHeight, msgBox.Height);
			}
		}

		public void TestNotificationReporterContainedMessage()
		{
			using (var control = new KForm())
			using (var testControl = new ZMessageBox("Test Sango", "This is the caption", MessageBoxButtons.OK, MessageBoxIcon.Error))
			{
				var testTracker = GCTracker.Track("Test key", control);
				testTracker.NotifyDisposed(false);

				var test2Tracker = GCTracker.Track("Test key", testControl);
				ErrorReporter.ReportOnce("Shutting down", new Exception("Whatever."));
				test2Tracker.NotifyDisposed(false);
				var lastMessage = ErrorReporter.LastMessageReported;

				Assert("ErrorMessage has to contain message error if control is ZMessageBox", lastMessage.Contains("Test Sango"));
				Assert("ErrorMessage has to contain Form Title if control is KForm", lastMessage.Contains("Form title:"));
			}

			GCTracker.ClearListForTest();
			ErrorReporter.Clear();
		}

		public void TestNotificationReporterOnZMessageBoxWillNotThrowNullReferenceExceptionWhenTextBoxIsNull()
		{
			ErrorReporter.Clear();
			using (var control = new ZForm())
			using (var testControl = new ZMessageBox("Test Message", "This is the caption", MessageBoxButtons.OK, MessageBoxIcon.Error))
			{
				var testTracker = GCTracker.Track("Test key", control);
				testTracker.NotifyDisposed(false);

				var test2Tracker = GCTracker.Track("Test key", testControl);
				ErrorReporter.ReportOnce("Shutting down", new Exception("Whatever."));
				testControl.TextBox = null;

				AssertNoExceptionThrown(() => test2Tracker.NotifyDisposed(false));
			}

			GCTracker.ClearListForTest();
			ErrorReporter.Clear();
		}

		public void TestAnotherLargeMessage()
		{
			var text = @"You are about to disable integration with Active Directory. Before proceeding, please be aware of the following:

1. All existing staff and group records will be disconnected from Active Directory and their details will no longer be synced.
2. The next time each staff member logs into CargoWise One they will be required to change their password. The default intermediate password is '{0}'.";
			using (var msgBox = NewMessageBox(text, "Timothy's Test", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk))
			{
				msgBox.Show();
				var msgBoxTextHeight = msgBox.TextBox.Height;

				AssertEquals("The height (px) should be tall enough to fit 5 lines of text. 77 is too short", 80, msgBoxTextHeight);
			}
		}

		public void TestTextBoxSizeShouldMatchTextSize()
		{
			var text = @"You are about to disable integration with Active Directory. Before proceeding, please be aware of the following:

1. All existing staff and group records will be disconnected from Active Directory and their details will no longer be synced.
2. The next time each staff member logs into CargoWise One they will be required to change their password. The default intermediate password is '{0}'.
3. This message needs to be long so I ll add more lines.
4. as I said this message is a bit longer than usually.
5. I think 5 should be enought 
6.actually six here we go.";

			using (ZMessageBox msgBox = NewMessageBox(text, "Sango's Test", MessageBoxButtons.OK, MessageBoxIcon.Error))
			{
				msgBox.Show();
				int msgBoxTextHeight = msgBox.TextBox.Height;

				var textSize = TextRenderer.MeasureText(text, msgBox.TextBox.Font);

				AssertEquals("The height (px) should be tall enough to fit 5 lines of text. 77 is too short", 132, msgBoxTextHeight);
			}
		}

		public void TestLargeMessageWithScrollBars()
		{
			var text = "";
			var message = "This is a very very long message and i wonder if it is going to work. Let's hope it does, now I will repeat this over.";

			for (var i = 0; i < 400; i++)
			{
				text += message;
			}

			using (var msgBox = NewMessageBox(text, "Zubin's Test", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk))
			{
				msgBox.Show();
				AssertEquals("Scrollbars Exist", ScrollBars.Vertical, msgBox.TextBox.ScrollBars);

				var msgBoxTextHeight = msgBox.TextBox.Height;
				var msgBoxTextWidth = msgBox.TextBox.Width;

				var maxHeight = msgBox.CurrentScreenInfo.Height * 2 / 3;
				var maxWidth = Math.Min(ControlDpiScalingHelper.ScaleToCurrentDpiX(600), msgBox.CurrentScreenInfo.Width * 2 / 3);

				AssertEquals("Height", maxHeight, msgBoxTextHeight);
				Assert("Width is " + msgBoxTextWidth + " but should be less than " + (maxWidth + IDynamicSizedDialogExtensions.ScrollBarsWidthForTest), msgBoxTextWidth <= maxWidth + IDynamicSizedDialogExtensions.ScrollBarsWidthForTest);
			}
		}

		public void TestTabEffectsOnWidthAndHeight()
		{
			using (var msgBox = NewMessageBox("", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk))
			{
				//Tested using Microsoft Sans Serif 8.25 point (actually both 8 and 9 point since Word doesn't let me use fractional points...)
				var widths = new List<float>();
				var strings = new List<string>
				{
"WWWWWWWWWW",
"    WWWWWWWWWW",
"        WWWWWWWWWW",
"            WWWWWWWWWW",
"               WWWWWWWWWW",
"\tWWWWWWWWWW",
"\t\tWWWWWWWWWW",
"WWWWWWWWWWWWWWWWWWWWW",
"    WWWWWWWWWWWWWWWWWWWWW",
"        WWWWWWWWWWWWWWWWWWWWW",
"            WWWWWWWWWWWWWWWWWWWWW",
"               WWWWWWWWWWWWWWWWWWWWW",
"\tWWWWWWWWWWWWWWWWWWWWW"
				};

				foreach (var @string in strings)
				{
					msgBox.Message = @string;
					widths.Add(msgBox.MeasureTextSize().Width);
				}

				for (var i = 0; i < widths.Count - 1; ++i)
				{
					Assert(string.Format("{0} (measured {1}) beats {2} (measured {3})", strings[i + 1], widths[i + 1], strings[i], widths[i]), widths[i + 1] >= widths[i]);
				}
			}
		}

		public void TestLargeMessageWithoutScrollBars()
		{
			using (var form = new KForm())
			{
				var textBox = new ZTextBox();
				form.Controls.Add(textBox);
				form.Show();
				form.ActiveControl = textBox;

				var text = FormDebugInfo.GetActiveControlInfoFromFormForDocEngine(form).ControlInfo;
				using (var msgBox = NewMessageBox(text, "Zubin's Test", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk))
				{
					msgBox.Show();

					AssertEquals("Scrollbars", ScrollBars.None, msgBox.TextBox.ScrollBars);

					var msgBoxTextHeight = msgBox.TextBox.Height;
					var msgBoxTextWidth = msgBox.TextBox.Width;

					var maxHeight = msgBox.CurrentScreenInfo.Height * 2 / 3;
					var maxWidth = 600;

					Assert("Height is " + msgBoxTextHeight + " but should be less than " + maxHeight, msgBoxTextHeight <= maxHeight);
					Assert("Width is " + msgBoxTextWidth + " but should be less than " + maxWidth, msgBoxTextWidth <= maxWidth);
				}
			}
		}

		public void TestSmallMessage()
		{
			using (var msgBox = NewMessageBox("You are about to delete this record permanently from the system. Do you want to proceed?", "Zubin's Test", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk))
			{
				msgBox.Show();
				AssertEquals("Scrollbars", ScrollBars.None, msgBox.TextBox.ScrollBars);
			}
		}

		public void TestRegEx()
		{
			using (var msgBox = NewMessageBox("Nothing", "Title", MessageBoxButtons.OK, MessageBoxIcon.None))
			{
				var message = "HouseBill : FIRSTLCLCONTAINER Message has been successfully sent\r\nHouseBill : LCLCONTAINER2 Message has been successfully sent\r\n";
				AssertEquals("RegEx", message, msgBox.CorrectNewLines(message));

				message = "Hello.\n. Test Number 2";
				Assert("RegEx Replaces with NewLine", message != msgBox.CorrectNewLines(message));
				AssertEquals("RegEx Replaced Value is Correct", "Hello.\r\n. Test Number 2", msgBox.CorrectNewLines(message));
			}
		}

		public void TestMessageBoxFont()
		{
			using (var msgBox = NewMessageBox("Test", "Test Form Font", MessageBoxButtons.OK, MessageBoxIcon.Warning))
			{
				msgBox.Show();
				AssertEquals("Should set the font call method 'OFont.GetFont()' for message box, otherwise it will display incorrect on RDP server.", msgBox.Font.Name, OFont.GetFont().Name);
			}
		}

		[ExpectNoExceptions]
		public void TestCustomIconNotDisposed()
		{
			var icon = Icons.GetIcon(IconTypes.Box);
			using (var msgBox = NewMessageBox("Text", "Cinty winty likes stripy panty", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk))
			{
				msgBox.SetCustomIcon(icon);
			}

			// expect no exception as the icon should not be disposed
			icon.ToBitmap();
		}

		public void TestShowDialog_InUnitTest()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			DialogResult actualAnswer;
			using (var msgBox = NewMessageBox("Nothing", "Title", MessageBoxButtons.OK, MessageBoxIcon.None))
			{
				actualAnswer = msgBox.ShowDialogForTest();
			}
			AssertEquals("Should return the DialogResult that was passed into AddAnswer()", DialogResult.No, actualAnswer);
			AssertEquals("UnitTestUserNotification.Instance.LastMessage.Text", "Nothing", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestButton1Text()
		{
			using (var msgBox = NewMessageBox("Nothing", "Title", MessageBoxButtons.OKCancel, MessageBoxIcon.None, "Agree"))
			{
				var button1 = (ZButton)msgBox.Controls.Find("OKButton", true)[0];
				AssertEquals("Agree", button1.Text);
			}
		}

		public void TestButton2Text()
		{
			using (var msgBox = new ZMessageBox("Nothing", "Title", MessageBoxButtons.YesNoCancel, MessageBoxIcon.None, "Departure", "Arrival"))
			{
				var button2 = (ZButton)msgBox.Controls.Find("NoButton", true)[0];
				AssertEquals("Arrival", button2.Text);
			}
		}

		public void TestShowDialog_InUnitTest_WithDefaultButton()
		{
			using (var msgBox = NewMessageBox("Nothing", "Title", MessageBoxButtons.YesNoCancel, MessageBoxIcon.None, MessageBoxDefaultButton.Button1))
			{
				AssertEquals("Button1", DialogResult.Yes, msgBox.ShowDialogForTest());
			}
			using (var msgBox = NewMessageBox("Nothing", "Title", MessageBoxButtons.YesNoCancel, MessageBoxIcon.None, MessageBoxDefaultButton.Button2))
			{
				AssertEquals("Button2", DialogResult.No, msgBox.ShowDialogForTest());
			}
			using (var msgBox = NewMessageBox("Nothing", "Title", MessageBoxButtons.YesNoCancel, MessageBoxIcon.None, MessageBoxDefaultButton.Button3))
			{
				AssertEquals("Button3", DialogResult.Cancel, msgBox.ShowDialogForTest());
			}
		}

		public void TestButtonsAreTranslatable()
		{
			var readOnlyResourceStrings = ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(Res.DefaultLanguage);
			using (var mockResourceStrings = Res.UseMockData())
			{
				mockResourceStrings.SetResourceGetter(key =>
				{
					AssertNotNull(key, readOnlyResourceStrings.Get(key));
					return new ResourceStringData(key, "!@#$%^");
				});
				foreach (MessageBoxButtons buttons in Enum.GetValues(typeof(MessageBoxButtons)))
				{
					using (var msgBox = NewMessageBox("Test", "This is a test", buttons, MessageBoxIcon.Information))
					{
						msgBox.Show();
						foreach (var control in msgBox.Controls)
						{
							var button = control as Button;
							if (button != null && button.Visible)
							{
								AssertEquals("!@#$%^", ((Button)control).Text);
							}
						}
					}
				}
			}
		}

		protected virtual ZMessageBox NewMessageBox(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			return NewMessageBox(message, caption, buttons, icon, MessageBoxDefaultButton.Button1);
		}

		protected virtual ZMessageBox NewMessageBox(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, string button1Text)
		{
			return new ZMessageBox(message, caption, buttons, icon, button1Text); // These are the unit tests for ZMessageBox
		}

		protected virtual ZMessageBox NewMessageBox(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
		{
			return new ZMessageBox(message, caption, buttons, icon, defaultButton); // These are the unit tests for ZMessageBox
		}
	}
}
