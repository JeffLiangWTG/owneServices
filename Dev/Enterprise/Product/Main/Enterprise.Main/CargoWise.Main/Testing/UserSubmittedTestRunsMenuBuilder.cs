#if DEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Startup
{
	sealed class UserSubmittedTestRunsMenuBuilder
	{
		public UserSubmittedTestRunsMenuBuilder(bool showOnlyMyTests = false)
		{
			showOnlyMine = showOnlyMyTests;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public void BuildMenu(ZToolStripMenuItem userSubmittedTestFailuresMenuItem, ZToolStripMenuItem errorGettingTestRunListMenuItem, ref bool hasCalculatedSubmittedTestRuns)
		{
			if (!hasCalculatedSubmittedTestRuns)
			{
				try
				{
					var userName = WindowsIdentity.GetCurrent().Name;
					var userNameWithoutDomain = userName.Split('\\').Last();
					var fetcher = showOnlyMine ? new UserSubmittedTestRunDATFetcher(userName) : new UserSubmittedTestRunDATFetcher();
					var submittedTestRuns = fetcher.GetSubmittedTestRuns();
					if (!showOnlyMine)
					{
						submittedTestRuns.Sort(new Comparison<UserSubmittedTestRun>((y, x) => x.DateSubmitted.CompareTo(y.DateSubmitted)));
					}

					userSubmittedTestFailuresMenuItem.DropDownItems.Clear();
					ZToolStripMenuItem currentUserMenuItem = null;
					var submittedUsers = new SortedDictionary<string, IDictionary<string, ZToolStripMenuItem>>(StringComparer.OrdinalIgnoreCase);

					foreach (var testRun in submittedTestRuns)
					{
						var testRunUserName = testRun.UserName;
						var displayNameWithoutUserName = testRun.DisplayName.Replace(testRunUserName + " - ", "");
						var newTestRun = new ZToolStripMenuItem(displayNameWithoutUserName, UserSubmittedTestRunHandler);
						newTestRun.Tag = testRun.UserTestPK;

						if (userNameWithoutDomain.Equals(testRunUserName, StringComparison.OrdinalIgnoreCase))
						{
							if (showOnlyMine)
							{
								currentUserMenuItem = userSubmittedTestFailuresMenuItem;
							}
							else
							{
								currentUserMenuItem ??= new ZToolStripMenuItem(testRunUserName);
							}
							currentUserMenuItem.DropDownItems.Add(newTestRun);
						}
						else
						{
							IDictionary<string, ZToolStripMenuItem> userList;
							var key = testRunUserName.Substring(0, 1).ToUpper();
							if (!submittedUsers.TryGetValue(key, out userList))
							{
								userList = new SortedDictionary<string, ZToolStripMenuItem>(StringComparer.OrdinalIgnoreCase);
								submittedUsers.Add(key, userList);
							}

							ZToolStripMenuItem userMenuItem;
							if (!userList.TryGetValue(testRunUserName, out userMenuItem))
							{
								userMenuItem = new ZToolStripMenuItem(testRunUserName);
								userList.Add(testRunUserName, userMenuItem);
							}
							userMenuItem.DropDownItems.Add(newTestRun);
						}
					}

					if (currentUserMenuItem != null && userSubmittedTestFailuresMenuItem != currentUserMenuItem)
					{
						userSubmittedTestFailuresMenuItem.DropDownItems.Add(currentUserMenuItem);
					}

					if (submittedUsers.Count > 0 && currentUserMenuItem != null)
					{
						userSubmittedTestFailuresMenuItem.DropDownItems.Add(new ToolStripSeparator());
					}
					foreach (var submittedAlphabetic in submittedUsers)
					{
						var alphabeticMenuItem = new ZToolStripMenuItem(submittedAlphabetic.Key);

						foreach (var value in submittedAlphabetic.Value)
						{
							alphabeticMenuItem.DropDownItems.Add(value.Value);
						}
						userSubmittedTestFailuresMenuItem.DropDownItems.Add(alphabeticMenuItem);
					}

					errorGettingTestRunListMenuItem.Visible = false;
					hasCalculatedSubmittedTestRuns = true;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					errorGettingTestRunListMenuItem.Visible = true;
				}
			}
		}

		void UserSubmittedTestRunHandler(object sender, EventArgs e)
		{
			var menuItem = sender as ZToolStripMenuItem;
			if (menuItem != null && menuItem.Tag is Guid)
			{
				UnitTestRunner.ShowUserSubmittedFailures((Guid)menuItem.Tag);
			}
			else
			{
				Globals.Message.Show("Unable to identify the test run.");
			}
		}

		readonly bool showOnlyMine;
	}
}

#endif
