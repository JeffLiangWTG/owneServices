using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class UserEventTrackerTest : TestCase
	{
		#region Activity Logging

		public void TestActivityLogging()
		{
			using (Form someForm = new Form())
			using (TextBox textBox = new TextBox())
			using (TextBox textBox2 = new TextBox())
			{
				someForm.Controls.Add(textBox);
				someForm.Controls.Add(textBox2);
				UserEventTracker.Instance.AddUserEventToControl(someForm);
				UserEventTracker.Instance.AddUserEventToControl(textBox);
				UserEventTracker.Instance.AddUserEventToControl(textBox2);

				someForm.Show();

				KeySender.SendKeyPress(textBox, textBox.Handle, Keys.A);
				KeySender.SendKeyPress(textBox2, textBox2.Handle, Keys.B);
				KeySender.SendKeyPress(textBox, textBox.Handle, Keys.C);
				KeySender.SendKeyPress(someForm, someForm.Handle, Keys.D);

				textBox2.Focus();
				textBox.Focus();
				textBox2.Focus();

				typeof(Control).InvokeMember("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, textBox, new object[] { new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0) });
				typeof(Control).InvokeMember("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, textBox, new object[] { new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0) });

				FormUserStatistics stats = UserEventTracker.Instance.GetFormStatsForForm(someForm);
				AssertNotNull(stats);
				AssertEquals("All KeyPresses should be recorded", 4, stats.KeyPresses);
				AssertEquals(2, stats.MouseClicks);
				Assert("stats.ControlFocusChanges should be 1 or 4 but is: " + stats.ControlFocusChanges, stats.ControlFocusChanges == 1 || stats.ControlFocusChanges == 4);
			}

			using (Form someForm2 = new Form())
			{
				UserEventTracker.Instance.AddUserEventToControl(someForm2);
				someForm2.Show();

				FormUserStatistics stats = UserEventTracker.Instance.GetFormStatsForForm(someForm2);
				AssertNotNull(stats);
				AssertEquals(0, stats.KeyPresses);
				AssertEquals(1, stats.ControlFocusChanges);
			}
		}

		#endregion

		public void TestGetInstance()
		{
			AssertNotNull("Valid Instance should be returned", UserEventTracker.Instance);
			AssertEquals("Is enabled by default", true, UserEventTracker.Instance.IsEnabled);
		}

		[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "testing SqlCommand")]
		public void TestClearAll()
		{
			UserEventTracker.UserEventList.Clear();
			SqlEventTracker.Instance.Clear();
			DatabaseConnectionEventTracker.Instance.Clear();

			UserEventTracker.Instance.AddUserEvent(new TestControl(), "Test", "Test");
			SqlEventTracker.Instance.AddSqlEvent(new SqlCommand(), new Exception("woolloomooloo"), TimeSpan.FromSeconds(1)); // Internally the function takes an SQLCommand. It must take an SQL Command here in the test as well.
			DatabaseConnectionEventTracker.Instance.AddConnectionEvent(new SqlConnection(@"Server=(localdb)\V11.0"));

			AssertEquals("PreCondition: Event Added", 1, UserEventTracker.UserEventList.Count);
			AssertEquals("PreCondition: SQL Event Added", true, SqlEventTracker.Instance.HasQueries);
			AssertEquals("PreCondition: Connection Event Added", true, DatabaseConnectionEventTracker.Instance.HasConnections);
			AssertNotEquals("PreCondition: SQL Event Added", "<SqlFailedEvents><Count>0</Count></SqlFailedEvents>", SqlEventTracker.Instance.SqlFailedEventDescription);

			UserEventTracker.Instance.ClearAll();
			AssertEquals("Events Cleared", 0, UserEventTracker.UserEventList.Count);
			AssertEquals("SQL Events Cleared", false, SqlEventTracker.Instance.HasQueries);
			AssertEquals("Connection Events Cleared", false, DatabaseConnectionEventTracker.Instance.HasConnections);
			AssertEquals("SQL Events Cleared", "<SqlFailedEvents><Count>0</Count></SqlFailedEvents>", SqlEventTracker.Instance.SqlFailedEventDescription);
		}

		public void TestUserEventTrapping()
		{
			TestControl testControl = new TestControl();
			testControl.Text = "BOB";

			UserEventTracker.UserEventList.Clear();
			AssertEquals("There should be no items in the list", 0, UserEventTracker.UserEventList.Count);
			UserEventTracker.Instance.AddUserEventToControl(testControl);
			AssertEquals("There should be no items in the list", 0, UserEventTracker.UserEventList.Count);
			testControl.SimulateKeyPress('A');
			AssertEquals("There should be items in the list", 1, UserEventTracker.UserEventList.Count);
			testControl.SimulateMouseDown();
			AssertEquals("There should be items in the list", 2, UserEventTracker.UserEventList.Count);

			const string expected =
				"<UserEvents>\r\n" +
				"<Count>2</Count>\r\n" +
				"<Event>KeyPress<Occurrence>####-##-## ##:##:##</Occurrence>\r\n" +
				"<Data>A</Data>\r\n" +
				"<Control />\r\n" +
				"<Reference>Text = BOB</Reference>\r\n" +
				"</Event>\r\n" +
				"<Event>MouseDown<Occurrence>####-##-## ##:##:##</Occurrence>\r\n" +
				"<Data>Left</Data>\r\n" +
				"<Control />\r\n" +
				"<Reference>Text = BOB</Reference>\r\n" +
				"</Event>\r\n" +
				"</UserEvents>" +
				"";

			Regex regex = new Regex("[0-9]{4}-[0-9]{2}-[0-9]{2} [0-9]{2}:[0-9]{2}:[0-9]{2}");
			string actual = regex.Replace(UserEventTracker.Instance.UserEventDescription, "####-##-## ##:##:##").Replace("><", ">\r\n<");
			AssertMultilineASCIIEquals("A string should be returned", expected, actual);

			UserEventTracker.UserEventList.Clear();
			AssertEquals("There should be no items in the list", 0, UserEventTracker.UserEventList.Count);
		}

		public void TestUserEventTrapping_ShouldMaskLogOfMaskedTestBoxes()
		{
			var textBox = new TestControl { PasswordChar = '^' };
			AssertEnteringTextDoesNotRecordKeysInEvents(textBox, '^');
		}

		public void TestUserEventTrapping_ShouldMaskLogOfMaskedTestBoxes_UsingSystemPasswordChar()
		{
			var textBox = new TestControl { UseSystemPasswordChar = true };
			AssertEnteringTextDoesNotRecordKeysInEvents(textBox, '●');
		}

		void AssertEnteringTextDoesNotRecordKeysInEvents(TestControl textBox, char expectedPasswordChar)
		{
			UserEventTracker.UserEventList.Clear();
			try
			{
				using (textBox)
				{
					UserEventTracker.Instance.AddUserEventToControl(textBox);
					var maiPasswrod = "Mai Passwrod";

					foreach (var @char in maiPasswrod)
					{
						textBox.SimulateKeyPress(@char);
					}

					var expectedResult = Regex.Escape(string.Format("<Data>{0}</Data>", expectedPasswordChar));
					var results = new Regex(expectedResult).Matches(UserEventTracker.Instance.UserEventDescription).Count;

					AssertEquals(maiPasswrod.Length, results);
				}
			}
			finally
			{
				UserEventTracker.UserEventList.Clear();
			}
		}

		[ExpectNoExceptions]
		public void TestUserEventDescription()
		{
			TestControl testControl = new TestControl();
			UserEventTracker.UserEventList.Clear();
			UserEventTracker.Instance.AddUserEventToControl(testControl);
			testControl.SimulateKeyPress('A');
			testControl.SimulateMouseDown();
			string userEvents = UserEventTracker.Instance.UserEventDescription;
			//this simply validates well formed XML is produced by method
			System.Xml.XmlDocument testDoc = new System.Xml.XmlDocument();
			testDoc.LoadXml(userEvents);
		}

		public void TestIsEnabledSetsEventTrackerIsEnabled()
		{
			AssertEquals("[PRE-CONDITION] UserEventTracker.IsEnabled default", true, UserEventTracker.Instance.IsEnabled);
			AssertEquals("[PRE-CONDITION] SqlEventTracker.IsEnabled default", true, SqlEventTracker.Instance.IsEnabled);
			AssertEquals("[PRE-CONDITION] DatabaseConnectionEventTracker.IsEnabled default", true, DatabaseConnectionEventTracker.Instance.IsEnabled);

			using (new UserEventTracker.TemporaryEnableStateChanger(false))
			{
				AssertEquals("UserEventTracker.IsEnabled after change", false, UserEventTracker.Instance.IsEnabled);
				AssertEquals("SqlEventTracker.IsEnabled after change", false, SqlEventTracker.Instance.IsEnabled);
				AssertEquals("DatabaseConnectionEventTracker.IsEnabled after change", false, DatabaseConnectionEventTracker.Instance.IsEnabled);
			}

			AssertEquals("UserEventTracker.IsEnabled", true, UserEventTracker.Instance.IsEnabled);
			AssertEquals("SqlEventTracker.IsEnabled", true, SqlEventTracker.Instance.IsEnabled);
			AssertEquals("DatabaseConnectionEventTracker.IsEnabled", true, DatabaseConnectionEventTracker.Instance.IsEnabled);
		}

		[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "testing SqlCommand")]
		public void TestSQLEventTrapping()
		{
			SqlCommand sQL = new SqlCommand("Whatever happens this is not its fault."); // Internally the function takes an SQLCommand. It must take an SQL Command here in the test as well.
			SqlEventTracker.Instance.Clear();
			AssertEquals("There should be no items in the list:" + UserEventTracker.Instance.SqlEventDescription, false, SqlEventTracker.Instance.HasQueries);
			SqlEventTracker.Instance.AddSqlEvent(sQL, null, TimeSpan.FromSeconds(1));
			AssertEquals("There should be 1 item in the list:" + UserEventTracker.Instance.SqlEventDescription, true, SqlEventTracker.Instance.HasQueries);
			Assert("A string should be returned", SqlEventTracker.Instance.SqlEventDescription.Length > 30);
			Assert("A string should be returned", SqlEventTracker.Instance.SqlFailedEventDescription.Length > 30);
			SqlEventTracker.Instance.Clear();
			AssertEquals("There should be no items in the list:" + UserEventTracker.Instance.SqlEventDescription, false, SqlEventTracker.Instance.HasQueries);
		}

		[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "testing SqlCommand")]
		[ExpectNoExceptions]
		public void TestSQLEventDescription()
		{
			SqlCommand sQL = new SqlCommand("Whatever happens this is not its fault."); // Internally the function takes an SQLCommand. It must take an SQL Command here in the test as well.
			SqlEventTracker.Instance.Clear();
			SqlEventTracker.Instance.AddSqlEvent(sQL, new Exception(), TimeSpan.FromSeconds(1));
			string sqlEvents = SqlEventTracker.Instance.SqlEventDescription;
			// This simply validates well formed XML is produced by method
			System.Xml.XmlDocument testDoc = new System.Xml.XmlDocument();
			testDoc.LoadXml(sqlEvents);

			sqlEvents = SqlEventTracker.Instance.SqlFailedEventDescription;
			// This simply validates well formed XML is produced by method
			testDoc = new System.Xml.XmlDocument();
			testDoc.LoadXml(sqlEvents);
		}

		[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "testing SqlConnection")]
		[ExpectNoExceptions]
		public void TestConnectionEventDescription()
		{
			using (var connection = new SqlConnection(@"Server=(localdb)\V11.0"))
			{
				DatabaseConnectionEventTracker.Instance.Clear();
				DatabaseConnectionEventTracker.Instance.AddConnectionEvent(connection);
				var connectionEvents = DatabaseConnectionEventTracker.Instance.ConnectionEventDescription;
				// This simply validates well formed XML is produced by method
				var testDoc = new System.Xml.XmlDocument();
				testDoc.LoadXml(connectionEvents);

				connectionEvents = DatabaseConnectionEventTracker.Instance.ConnectionEventDescription;
				// This simply validates well formed XML is produced by method
				testDoc = new System.Xml.XmlDocument();
				testDoc.LoadXml(connectionEvents);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "testing SqlCommand")]
		[ExpectNoExceptions]
		public void TestAddSqlEventDoesntThrowExceptionOnNullParamValue()
		{
			SqlCommand sQL = new SqlCommand("Whatever happens this is not its fault."); // Internally the function takes an SQLCommand. It must take an SQL Command here in the test as well.
			sQL.Parameters.AddWithValue("LALALA", null);
			SqlEventTracker.Instance.AddSqlEvent(sQL, null, TimeSpan.FromSeconds(1));
		}

		#region Multithreading

		[ExpectNoExceptions]
		public void TestMultithreading()
		{
			Thread sqlAdder = new Thread(new ThreadStart(AddSqlThings));
			Thread sqlGetter = new Thread(new ThreadStart(GetSqlThings));

			var testControl = new Control();
			Thread userAdder = new Thread(new ThreadStart(() => AddUserThings(testControl)));
			Thread userGetter = new Thread(new ThreadStart(GetUserThings));

			try
			{
				sqlGetter.Start();
				sqlAdder.Start();

				userGetter.Start();
				userAdder.Start();

				sqlGetter.Join();
				sqlAdder.Join();

				userGetter.Join();
				userAdder.Join();

				Assert("Everything is fine", true);
			}
			finally
			{
				sqlGetter.Join();
				sqlAdder.Join();

				userGetter.Join();
				userAdder.Join();
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "testing SqlCommand")]
		void AddSqlThings()
		{
			for (int i = 0; i < 1000; i++)
			{
				SqlEventTracker.Instance.AddSqlEvent(new SqlCommand(), new Exception(string.Format("test message{0}", i)), TimeSpan.FromSeconds(1)); // This is used for testing only. We need to provide a not null value for this parameter
			}
		}

		void GetSqlThings()
		{
			string description;
			for (int i = 0; i < 1000; i++)
			{
				description = SqlEventTracker.Instance.SqlEventDescription;
			}
		}

		void AddUserThings(Control testControl)
		{
			for (int i = 0; i < 1000; i++)
			{
				UserEventTracker.Instance.AddUserEvent(testControl, string.Format("test event {0}", i), "test event data");
			}
		}

		void GetUserThings()
		{
			string description;
			for (int i = 0; i < 1000; i++)
			{
				description = UserEventTracker.Instance.UserEventDescription;
			}
		}

		#endregion

		[UserEventDiagnosticReference("Text = {Text}")]
		class TestControl : TextBox
		{
			public void SimulateKeyPress(char character)
			{
				OnKeyPress(new KeyPressEventArgs(character));
			}

			public void SimulateMouseDown()
			{
				OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, 1, 1, 1));
			}
		}
	}
}
