using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class WriteToLogMenuItemTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var dummy1 = new DummyBusinessObject();
			var security1 = new DummyCheckPointWithSecuritySet(true);

			using (var menuItem1 = new WriteToLogMenuItem(dummy1, security1, "caption1"))
			{
				AssertEquals("menuItem1.topLevelBusinessObject", dummy1, menuItem1.topLevelBusinessObject);
				AssertEquals("menuItem1.LoggedBusinessObject", dummy1, menuItem1.LoggedBusinessObject);
				AssertEquals("menuItem1.security", security1, menuItem1.security);
				AssertEquals("menuItem1.caption", "caption1", menuItem1.caption);
			}

			var dummy2 = new DummyBusinessObject();
			var security2 = new DummyCheckPointWithSecuritySet(false);

			using (var menuItem2 = new WriteToLogMenuItem(dummy1, delegate { return dummy2; }, security2, "caption2"))
			{
				AssertEquals("menuItem2.topLevelBusinessObject", dummy1, menuItem2.topLevelBusinessObject);
				AssertEquals("menuItem2.LoggedBusinessObject", dummy2, menuItem2.LoggedBusinessObject);
				AssertEquals("menuItem2.security", security2, menuItem2.security);
				AssertEquals("menuItem2.caption", "caption2", menuItem2.caption);
			}
		}

		public void TestAddTo()
		{
			using (MenuItem parentMenuItem = new ZMenuItem())
			{
				var menuItem1 = GetNewMenuItem();
				menuItem1.AddTo(parentMenuItem);

				AssertEquals("parentMenuItem.MenuItems.Count", 1, parentMenuItem.MenuItems.Count);
				AssertEquals("parentMenuItem.MenuItems[0]", menuItem1, parentMenuItem.MenuItems[0]);

				var menuItem2 = GetNewMenuItem();
				menuItem2.AddTo(parentMenuItem);

				AssertEquals("parentMenuItem.MenuItems.Count", 3, parentMenuItem.MenuItems.Count);
				AssertEquals("parentMenuItem.MenuItems[0]", menuItem1, parentMenuItem.MenuItems[0]);
				AssertEquals("parentMenuItem.MenuItems[1].Text", "-", parentMenuItem.MenuItems[1].Text);
				AssertEquals("parentMenuItem.MenuItems[2]", menuItem2, parentMenuItem.MenuItems[2]);

				parentMenuItem.MenuItems.Add("-");
				var menuItem3 = GetNewMenuItem();
				menuItem3.AddTo(parentMenuItem);

				AssertEquals("parentMenuItem.MenuItems.Count", 5, parentMenuItem.MenuItems.Count);
				AssertEquals("parentMenuItem.MenuItems[0]", menuItem1, parentMenuItem.MenuItems[0]);
				AssertEquals("parentMenuItem.MenuItems[1].Text", "-", parentMenuItem.MenuItems[1].Text);
				AssertEquals("parentMenuItem.MenuItems[2]", menuItem2, parentMenuItem.MenuItems[2]);
				AssertEquals("parentMenuItem.MenuItems[3].Text", "-", parentMenuItem.MenuItems[3].Text);
				AssertEquals("parentMenuItem.MenuItems[4]", menuItem3, parentMenuItem.MenuItems[4]);
			}
		}

		[ExpectExceptionMessage(typeof(InvalidOperationException), "This WriteToLogMenuItem has already been added to a parentMenuItem.")]
		public void TestCannotBeAddedToMultipleTimes()
		{
			using (MenuItem parentMenuItem = new ZMenuItem())
			{
				var menuItem = GetNewMenuItem();
				menuItem.AddTo(parentMenuItem);
				menuItem.AddTo(parentMenuItem);
			}
		}

		public void TestSetLoggedBusinessObject()
		{
			var dummy1 = new DummyBusinessObject();
			var dummy2 = new DummyBusinessObject();

			using (var menuItem = new WriteToLogMenuItem(dummy1, DummyCheckPoint.Instance, "Dummy"))
			{
				AssertEquals("topLevelBusinessObject", dummy1, menuItem.topLevelBusinessObject);
				AssertEquals("LoggedBusinessObject", dummy1, menuItem.LoggedBusinessObject);

				menuItem.SetLoggedBusinessObject(dummy2);
				AssertEquals("topLevelBusinessObject", dummy2, menuItem.topLevelBusinessObject);
				AssertEquals("LoggedBusinessObject", dummy2, menuItem.LoggedBusinessObject);
			}

			using (var menuItem = new WriteToLogMenuItem(null, DummyCheckPoint.Instance, "Dummy"))
			{
				AssertNull("topLevelBusinessObject", menuItem.topLevelBusinessObject);
				AssertNull("LoggedBusinessObject", menuItem.LoggedBusinessObject);

				menuItem.SetLoggedBusinessObject(dummy1);
				AssertEquals("topLevelBusinessObject", dummy1, menuItem.topLevelBusinessObject);
				AssertEquals("LoggedBusinessObject", dummy1, menuItem.LoggedBusinessObject);
			}

			var dummy3 = new DummyBusinessObject();

			using (var menuItem = new WriteToLogMenuItem(dummy1, delegate { return dummy2; }, DummyCheckPoint.Instance, "Dummy"))
			{
				AssertEquals("topLevelBusinessObject", dummy1, menuItem.topLevelBusinessObject);
				AssertEquals("LoggedBusinessObject", dummy2, menuItem.LoggedBusinessObject);

				menuItem.SetLoggedBusinessObject(dummy3);
				AssertEquals("topLevelBusinessObject", dummy1, menuItem.topLevelBusinessObject);
				AssertEquals("LoggedBusinessObject", dummy3, menuItem.LoggedBusinessObject);
			}
		}

		public void TestDefaultReference()
		{
			using (var menuItem = GetNewMenuItem())
			{
				AssertNull("DefaultReference", menuItem.DefaultReference);
				((BusinessObject)menuItem.topLevelBusinessObject).HasChanges = false;
				((DummyBusinessObject)menuItem.topLevelBusinessObject).isInDatabase = true;

				menuItem.PerformClick();
				using (var lastShownForm = (WriteToLogForm)ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertEquals("logger.Reference", "", ((BusinessObjectLogger)lastShownForm.LastDataSourceForTest).Reference);
				}

				menuItem.DefaultReference = "Default";
				menuItem.PerformClick();
				using (var lastShownForm = (WriteToLogForm)ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertEquals("logger.Reference", "Default", ((BusinessObjectLogger)lastShownForm.LastDataSourceForTest).Reference);
				}
			}
		}

		public void TestShowLogForm_LoggedBusinessObjectIsNull()
		{
			using (var menuItem = new WriteToLogMenuItem(null, DummyCheckPoint.Instance, "Orange"))
			{
				AssertNull("No messages should be shown yet.", UnitTestUserNotification.Instance.LastMessage.Text);

				menuItem.PerformClick();
				AssertEquals("A message should be shown.", "The Orange record has not been created yet.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var dummy = new DummyBusinessObject();
				dummy.IsTopLevel = true;
				menuItem.SetLoggedBusinessObject(dummy);
				TestShowLogForm(menuItem);
			}
		}

		public void TestShowLogForm_SameTopLevel()
		{
			using (var menuItem = GetNewMenuItem())
			{
				TestShowLogForm(menuItem);
			}
		}

		public void TestShowLogForm_DifferentTopLevel()
		{
			var dummy1 = new DummyBusinessObject();
			var dummy2 = new DummyBusinessObject();
			dummy1.IsTopLevel = true;

			using (var menuItem = new WriteToLogMenuItem(dummy1, delegate { return dummy2; }, DummyCheckPoint.Instance, "Potato"))
			{
				dummy2.isInDatabase = true;
				TestShowLogForm(menuItem);

				dummy2.HasChanges = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertEquals("A message should be shown.", "The Potato record needs to be saved. Please save the form first before write to log.", UnitTestUserNotification.Instance.LastMessage.Text);

				dummy2.HasChanges = false;
				dummy2.isInDatabase = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertEquals("A message should be shown.", "The Potato record needs to be saved. Please save the form first before write to log.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOnLogged()
		{
			var dummy = new DummyBusinessObject();
			dummy.IsTopLevel = true;
			dummy.isInDatabase = true;
			dummy.HasChanges = false;

			using (var menuItem = new DummyWriteToLogMenuItem(dummy, DummyCheckPoint.Instance, "Dummy"))
			{
				typeof(MenuItem).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, menuItem, new object[] { EventArgs.Empty });
				menuItem.PerformClick();
				AssertNull("lastLoggedEventArgs should be null.", lastLoggedEventArgs);

				try
				{
					menuItem.Logged += new EventHandler<LoggedEventArgs>(Logged);
					menuItem.writeToLogSucceeded = true;
					menuItem.PerformClick();
					AssertEquals("lastLoggedEventArgs.Succeeded", true, lastLoggedEventArgs.Succeeded);

					menuItem.writeToLogSucceeded = false;
					menuItem.PerformClick();
					AssertEquals("lastLoggedEventArgs.Succeeded", false, lastLoggedEventArgs.Succeeded);
				}
				finally
				{
					menuItem.Logged -= new EventHandler<LoggedEventArgs>(Logged);
				}
			}
		}

		void TestShowLogForm(WriteToLogMenuItem menuItem)
		{
			var topLevel = (DummyBusinessObject)menuItem.topLevelBusinessObject;
			var logged = (DummyBusinessObject)menuItem.LoggedBusinessObject;

			DummyCheckPoint.SecurityAllowed = false;
			AssertNull("No messages should be shown yet.", UnitTestUserNotification.Instance.LastMessage.Text);

			menuItem.PerformClick();
			AssertEquals("A message should be shown.", "You do not have the appropriate security rights to run this function.\r\n\r\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:\r\n\r\nTESTING 1 2 3", UnitTestUserNotification.Instance.LastMessage.Text);

			DummyCheckPoint.SecurityAllowed = true;
			topLevel.HasChanges = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			menuItem.PerformClick();
			AssertEquals("A message should be shown.", "This record needs to be saved. Please save the form first before write to log.", UnitTestUserNotification.Instance.LastMessage.Text);

			topLevel.HasChanges = false;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			menuItem.PerformClick();
			AssertEquals("A message should be shown.", "This record needs to be saved. Please save the form first before write to log.", UnitTestUserNotification.Instance.LastMessage.Text);

			topLevel.isInDatabase = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			menuItem.PerformClick();
			AssertNull("No messages should be shown.", UnitTestUserNotification.Instance.LastMessage.Text);

			using (var lastShownForm = (WriteToLogForm)ZFormModaliser.LastFormShownDialogForTest)
			{
				var flags = BindingFlags.NonPublic | BindingFlags.Instance;
				AssertEquals("logger.topLevelBusinessObject", topLevel, typeof(BusinessObjectLogger).GetField("topLevelBusinessObject", flags).GetValue(lastShownForm.LastDataSourceForTest));
				AssertEquals("logger.LoggedBusinessObject", logged, typeof(BusinessObjectLogger).GetField("revisedBusinessObject", flags).GetValue(lastShownForm.LastDataSourceForTest));
				AssertEquals("lastShownForm.FormCaption", menuItem.caption, lastShownForm.FormCaption);
			}
		}

		WriteToLogMenuItem GetNewMenuItem()
		{
			var dummy = new DummyBusinessObject();
			dummy.IsTopLevel = true;
			return new WriteToLogMenuItem(dummy, DummyCheckPoint.Instance, "caption");
		}

		void Logged(object sender, LoggedEventArgs e)
		{
			lastLoggedEventArgs = e;
		}

		LoggedEventArgs lastLoggedEventArgs;

		#region class DummyWriteToLogMenuItem

		class DummyWriteToLogMenuItem : WriteToLogMenuItem
		{
			public DummyWriteToLogMenuItem(IStmALogParent loggedBusinessObject, SecurityCheckpoint security, string caption)
				: base(loggedBusinessObject, security, caption)
			{
			}

			protected override void OnLogged(bool writeToLogSucceeded)
			{
				base.OnLogged(this.writeToLogSucceeded);
			}

			public bool writeToLogSucceeded;
		}

		#endregion

		#region class DummyBusinessObject

		class DummyBusinessObject : NonPersistentBusinessObject, IStmALogParent
		{
			public override bool HasChanges
			{
				get { return hasChanges; }
				set { hasChanges = value; }
			}

			public override bool IsInDatabase
			{
				get { return isInDatabase; }
			}

			#region IStmALogParent

			public Logs Logs
			{
				get { return logs ?? (logs = new Logs(this)); }
			}
			Logs logs;

			ZGuid IStmALogParent.LogsParentPK
			{
				get { return PK; }
			}

			string IStmALogParent.LogsParentTableName
			{
				get { return TableName; }
			}

			public BusinessObjectFactory LogsFactory
			{
				get { return Factory; }
			}

			BusinessObject[] IStmALogParent.BusinessObjectsWithRelatedEvents
			{
				get { return Array.Empty<BusinessObject>(); }
			}

			void IStmALogParent.ProcessLog(IStmALog log)
			{
			}

			bool IStmALogParent.DeferFiringWorkflow
			{
				get { return false; }
			}

			#endregion

			public bool isInDatabase;
			bool hasChanges;
		}

		#endregion
	}

	/// <summary>
	/// This class provides static methods to test your WriteToLogMenuItem.
	/// </summary>
	public abstract class Tester
	{
		public static void Test(WriteToLogMenuItem logMenuItem, BusinessObject expectedRevisedBusinessObject)
		{
			NUnit.Framework.TestCase.AssertEquals("revisedBusinessObject", expectedRevisedBusinessObject, logMenuItem.LoggedBusinessObject);
		}

		public static void Test(WriteToLogMenuItem logMenuItem, BusinessObject expectedLoggedBusinessObject, SecurityCheckpoint expectedSecurity, string expectedCaption)
		{
			Test(logMenuItem, expectedLoggedBusinessObject, expectedLoggedBusinessObject, expectedSecurity, expectedCaption);
		}

		public static void Test(WriteToLogMenuItem logMenuItem, BusinessObject expectedTopLevelBusinessObject, BusinessObject expectedLoggedBusinessObject, SecurityCheckpoint expectedSecurity, string expectedCaption)
		{
			NUnit.Framework.TestCase.AssertEquals("topLevelBusinessObject", expectedTopLevelBusinessObject, logMenuItem.topLevelBusinessObject);
			NUnit.Framework.TestCase.AssertEquals("loggedBusinessObject", expectedLoggedBusinessObject, logMenuItem.LoggedBusinessObject);
			NUnit.Framework.TestCase.AssertEquals("security", expectedSecurity, logMenuItem.security);
			NUnit.Framework.TestCase.AssertEquals("caption", expectedCaption, logMenuItem.caption);
			logMenuItem.PerformClick(); // Test that it doesn't crash.
		}

		public static void FireOnLogged(WriteToLogMenuItem menuItem, bool logSucceeded)
		{
			menuItem.OnLoggedInternal(logSucceeded);
		}
	}
}
