using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.SpellCheck.GUI;
using CargoWise.Tools.SpellCheck.TestFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.SpellCheck;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Tools.SpellCheck;
using Enterprise.ZArchitecture.GUI.Tools.SpellCheck.Testing;
using NUnit.Framework;
using WTG.SpellCheck;
using static CargoWise.Windows.UI.ControlDpiScalingHelper;

namespace Enterprise.ZArchitecture.GUI.Tools.Testing
{
	internal abstract class SpellCheckerTest<TSpellcheck, TControl> : TestCaseWithFactory where TSpellcheck : SpellChecker where TControl : Control
	{
		protected abstract TextBoxBase GetTextbox(TControl control);
		protected abstract TControl GetControlToSpellCheck(bool enableScroll = true);
		protected abstract TSpellcheck InitialiseSpellcheck(TControl c, ISpellChecker checker = null, string name = "DummyTextBox");
		protected ContextMenuStrip GetContextMenu(TControl c) => GetTextbox(c).ContextMenuStrip;
		protected static ISpellChecker GetDefaultSpellChecker() => SpellChecker.GetSpellChecker();
		public void TestCalculatingLineLocationDoesntChangeScrollPosition()
		{
			var topCorner = new Point(1, 1);
			using (var control = GetControlToSpellCheck(enableScroll: true))
			using (var form = new ZChildForm())
			{
				var textbox = GetTextbox(control);
				textbox.Text = string.Join("\r\n", Enumerable.Repeat("wrongg", 500));
				InitialiseSpellcheck(control);
				control.Dock = DockStyle.Fill;
				form.Controls.Add(control);
				form.Show();
				PumpMessageQueue(TimeSpan.FromSeconds(2));
				var firstVisibleChar = textbox.GetCharIndexFromPosition(topCorner);
				textbox.Invalidate();
				PumpMessageQueue(TimeSpan.FromSeconds(2));
				AssertEquals("The scroll position should not have been changed by rendering the squiggles", firstVisibleChar, textbox.GetCharIndexFromPosition(topCorner));
			}
		}

		static void PumpMessageQueue(TimeSpan duration)
		{
			for (var s = Stopwatch.StartNew(); s.Elapsed < duration;)
			{
				Application.DoEvents();
			}
		}

		public void TestSaveEffectsGlobalDictionary()
		{
			using (CreateUserWithDictionary(out var staff, out var dictionary))
			{
				dictionary.Words.ToList().ForEach(w => dictionary.RemoveWord(w));
				dictionary.AddWord("ziiis");
				dictionary.AddWord("summm");
				Factory.Save();
				using (var control = GetControlToSpellCheck())
				using (var dummyForm = new ZChildForm())
				{
					dummyForm.Controls.Add(control);
					dummyForm.Show();
					var checker = InitialiseSpellcheck(control);
					checker.SendRightClick_ForTest(0);
					var menuItems = GetContextMenu(control).Items;
					menuItems[SpellChecker.MenuItemKeys.ShowDictionary].PerformClick();
					Application.DoEvents();
					var form = Application.OpenForms.OfType<PersonalDictionaryForm>().Single();
					form.Dictionary.RemoveWord("ziiis");
					form.CommandButtonPost.PerformClick();
					while (form.Visible)
					{
						Application.DoEvents();
					}

					AssertContainsExactElementsInAnyOrder("If we save the form, the dictionary should be updated", new[] { "summm" }, WordDictionaryManager.Instance.CurrentDictionary);
				}
			}
		}

		public void TestCancelDoesntEffectGlobalDictionary()
		{
			using (CreateUserWithDictionary(out var staff, out var dictionary))
			{
				dictionary.Words.ToList().ForEach(w => dictionary.RemoveWord(w));
				dictionary.AddWord("ziiis");
				dictionary.AddWord("summm");
				Factory.Save();
				using (var control = GetControlToSpellCheck())
				using (var dummyForm = new ZChildForm())
				{
					dummyForm.Controls.Add(control);
					dummyForm.Show();
					var checker = InitialiseSpellcheck(control);
					checker.SendRightClick_ForTest(0);
					var items = GetContextMenu(control).Items;
					items[SpellChecker.MenuItemKeys.ShowDictionary].PerformClick();
					Application.DoEvents();
					var form = Application.OpenForms.OfType<PersonalDictionaryForm>().Single();
					form.Dictionary.RemoveWord("ziiis");
					form.CommandButtonCancel.PerformClick();
					while (form.Visible)
					{
						Application.DoEvents();
					}

					AssertContainsExactElementsInAnyOrder("If we cancel the form, the dictionary should be unchanged", new[] { "ziiis", "summm" }, WordDictionaryManager.Instance.CurrentDictionary);
				}
			}
		}

		public void TestSpellCheck()
		{
			using (var control = GetControlToSpellCheck())
			{
				var textbox = GetTextbox(control);
				control.Show();
				textbox.Text = "wrongg";
				using (var spellCheckTester = new SpellCheckFormTestHelper(Change))
				{
					SpellChecker.CheckSpelling(control);
				}

				AssertEquals("wrong", textbox.Text);
			}
		}

		public void TestCustomDictionary_DbHitCount()
		{
			var hitsBefore = Db.Connection.ExecutedCommandCount;
			for (var i = 0; i < 20; i++)
			{
				SpellChecker.GetSpellChecker();
			}

			AssertLessThan("We shouldn't need a db hit for every time we need a spellchecker", Db.Connection.ExecutedCommandCount, hitsBefore + 20);
		}

		public void TestCustomDictionary_NotLoggedIn()
		{
			using (Env.SetTemporaryUserContext(null))
			{
				var spellChecker = SpellChecker.GetSpellChecker();
				var badWords = spellChecker.CheckSpelling("Heer is sume words").Select(er => er.Word);
				AssertContainsExactElementsInAnyOrder(new[] { "Heer", "sume" }, badWords);
			}
		}

		public void TestCustomDictionary_LoggedInWithEmptyDictionary()
		{
			using (CreateUserWithDictionary(out var user, out var dictionary))
			{
				dictionary.Words.ToList().ForEach(w => dictionary.RemoveWord(w));
				Factory.Save();
				var spellChecker = SpellChecker.GetSpellChecker();
				var badWords = spellChecker.CheckSpelling("Heer is sume words").Select(er => er.Word);
				AssertContainsExactElementsInAnyOrder(new[] { "Heer", "sume" }, badWords);
			}
		}

		public void TestCustomDictionary_IgnoresWordsInDictionary()
		{
			using (CreateUserWithDictionary(out var user, out var dictionary))
			{
				WordDictionaryManager.Instance.AddWord("sume");
				var spellChecker = SpellChecker.GetSpellChecker();
				var badWords = spellChecker.CheckSpelling("Heer is sume words").Select(er => er.Word);
				AssertContainsExactElementsInAnyOrder(new[] { "Heer" }, badWords);
			}
		}

		public void TestUpdateWordsToIgnore()
		{
			using (var control = GetControlToSpellCheck())
			{
				var textbox = GetTextbox(control);
				var checker = InitialiseSpellcheck(control);
				checker.UpdateWordsToIgnore(new string[] { "wrongg wronggg" });
				textbox.Show();
				textbox.Text = "wronggg, wrongg.";
				using (var spellCheckTester = new SpellCheckFormTestHelper(Change))
				{
					var checkSpellingMenuItem = textbox.ContextMenuStrip.Items.Find("checkSpelling", false)[0];
					UnitTestUserNotification.Instance.ClearMessages();
					checkSpellingMenuItem.PerformClick();
					AssertEquals("No errors found.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestIgnoreSpellCheck()
		{
			using (var control = GetControlToSpellCheck())
			{
				var textbox = GetTextbox(control);
				control.Show();
				textbox.Text = "wrongg";
				using (var spellCheckTester = new SpellCheckFormTestHelper(Ignore))
				{
					SpellChecker.CheckSpelling(control);
				}

				AssertEquals("wrongg", textbox.Text);
			}
		}

		public void TestCancelSpellCheck()
		{
			using (var control = GetControlToSpellCheck())
			{
				var textbox = GetTextbox(control);
				control.Show();
				textbox.Text = "wrongg";
				using (var spellCheckTester = new SpellCheckFormTestHelper(Cancel))
				{
					SpellChecker.CheckSpelling(control);
				}

				AssertEquals("wrongg", textbox.Text);
			}
		}

		public void TestContextMenu()
		{
			using (var control = GetControlToSpellCheck())
			{
				var textbox = GetTextbox(control);
				var checker = InitialiseSpellcheck(control);
				control.Show();
				textbox.Text = "wrongg";
				checker.SendRightClick_ForTest(1);
				var menuItem = GetContextMenu(control).Items.Cast<ToolStripItem>().SingleOrDefault(item => item.Text == "Check Spelling");
				AssertNotNull("Should give the user a chance to use the Check Spelling form", menuItem);
				using (var spellCheckTester = new SpellCheckFormTestHelper(Change))
				{
					menuItem.PerformClick();
				}

				AssertEquals("wrong", textbox.Text);
			}
		}

		public void TestExcludeString()
		{
			WordDictionaryManager.Instance.AddWord("Wrongg");
			using (var control = GetControlToSpellCheck())
			{
				var textbox = GetTextbox(control);
				textbox.Text = "Wrongg alsowrong";
				using (var spellCheckTester = new SpellCheckFormTestHelper(Change))
				{
					SpellChecker.CheckSpelling(control);
				}

				AssertEquals("Wrongg also wrong", textbox.Text);
			}
		}

		[GuiTest]
		public void TestSuggestionsDisplayedForWordUnderMouse()
		{
			const string badWord = "ghghghg";
			using (var form = new ZChildForm())
			using (var control = GetControlToSpellCheck())
			{
				form.Controls.Add(control);
				var textbox = GetTextbox(control);
				textbox.Text = $"Here is some {badWord} text with a spelling error";
				var spellChecker = SpellCheckerTestHelpers.CreateSpellChecker(SpellCheckerTestHelpers.CreateSpellingError(textbox.Text, badWord, "google"));
				var checker = InitialiseSpellcheck(control, spellChecker);
				form.Show();
				checker.SendRightClick_ForTest(textbox.Text.IndexOf(badWord) + 1);
				var items = GetContextMenu(control).Items;
				AssertEquals("google", items[items.Count - 1].Text);
			}
		}

		public void TestNoSuggestionsMenuItemWhenNoneAvailable()
		{
			const string badWord = "bing";
			using (var form = new ZChildForm())
			using (var control = GetControlToSpellCheck())
			{
				form.Controls.Add(control);
				var textbox = GetTextbox(control);
				textbox.Text = $"Here is some {badWord} text with a spelling error";
				var spellChecker = SpellCheckerTestHelpers.CreateSpellChecker(SpellCheckerTestHelpers.CreateSpellingError(textbox.Text, badWord));
				var checker = InitialiseSpellcheck(control, spellChecker);
				form.Show();
				checker.SendRightClick_ForTest(textbox.Text.IndexOf(badWord) + 1);
				var menuItem = GetContextMenu(control).Items[SpellChecker.MenuItemKeys.NoSuggestionsAvailable];
				AssertNotNull("When there are no suggestions available, we should display so", menuItem);
				Assert("The 'no suggestions available' menu item should be disabled to discourage clicking it", !menuItem.Enabled);
			}
		}

		public void TestAddToDictionary()
		{
			using (CreateUserWithDictionary(out var user, out var dictionary))
			using (var control = GetControlToSpellCheck())
			using (var form = new ZChildForm())
			{
				form.Controls.Add(control);
				var checker = InitialiseSpellcheck(control);
				form.Show();
				var textbox = GetTextbox(control);
				textbox.Text = "here izz izz text";
				Application.DoEvents();
				AssertEquals("PRE: The spellchecker should highlight both instances of the misspelled word", 2, checker.Squiggles_Exposed.Count);
				checker.SendRightClick_ForTest(textbox.Text.IndexOf("izz") + 1);
				var items = GetContextMenu(control).Items;
				items[SpellChecker.MenuItemKeys.AddToDictionary].PerformClick();
				Application.DoEvents();
				dictionary.Reload();
				Assert("Word should be added to dictionary", dictionary.Words.Contains("izz"));
				checker.SendRightClick_ForTest(textbox.Text.IndexOf("izz") + 1);
				AssertEquals("The spellchecker should no longer highlight the added word", 0, checker.Squiggles_Exposed.Count);
			}
		}

		public void TestAddToDictionary_DisabledWhenNoSpellingError()
		{
			using (CreateUserWithDictionary(out var user, out var dictionary))
			using (var control = GetControlToSpellCheck())
			using (var form = new ZChildForm())
			{
				form.Controls.Add(control);
				var checker = InitialiseSpellcheck(control);
				form.Show();
				var textbox = GetTextbox(control);
				textbox.Text = "heer izz some texxt";
				Application.DoEvents();
				CombineAssertions(() =>
				{
					checker.SendRightClick_ForTest(textbox.Text.IndexOf("izz") + 1);
					Assert("When over a spelling error, the Add To Dictionary button should be enabled", GetContextMenu(control).Items[SpellChecker.MenuItemKeys.AddToDictionary].Enabled);
					checker.SendRightClick_ForTest(textbox.Text.IndexOf("some") + 1);
					Assert("When there is no spelling error, the Add To Dictionary button should be disabled", !GetContextMenu(control).Items[SpellChecker.MenuItemKeys.AddToDictionary].Enabled);
				});
			}
		}

		public void TestShowPersonalDictionary()
		{
			using (CreateUserWithDictionary(out var user, out var dictionary))
			using (var control = GetControlToSpellCheck())
			using (var dummyForm = new ZChildForm())
			{
				dummyForm.Controls.Add(control);
				dummyForm.Show();
				var textbox = GetTextbox(control);
				textbox.Text = "heer iz some texxt";
				var checker = InitialiseSpellcheck(control);
				checker.SendRightClick_ForTest(textbox.Text.IndexOf("iz") + 1);
				var items = GetContextMenu(control).Items;
				items[SpellChecker.MenuItemKeys.ShowDictionary].PerformClick();
				using (var form = Application.OpenForms.OfType<PersonalDictionaryForm>().Single())
				{
					AssertEquals("Should show this users dictionary", dictionary.PK, form.Dictionary.PK);
				}
			}
		}

		public void TestShowPersonalDictionary_WhenUserHasNoDictionary()
		{
			var user = (IGlbStaff)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			user.GS_Code = "JCD";
			Factory.Save();
			using (Env.SetTemporaryUserContext(new UserContext(user, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var control = GetControlToSpellCheck())
			using (var dummyForm = new ZChildForm())
			{
				dummyForm.Controls.Add(control);
				dummyForm.Show();
				var textbox = GetTextbox(control);
				textbox.Text = "heer iz some texxt";
				var checker = InitialiseSpellcheck(control);
				checker.SendRightClick_ForTest(textbox.Text.IndexOf("iz") + 1);
				var items = GetContextMenu(control).Items;
				items[SpellChecker.MenuItemKeys.ShowDictionary].PerformClick();
				using (var form = Application.OpenForms.OfType<PersonalDictionaryForm>().Single())
				{
					Assert("Save button should be disabled for new items", !form.BusinessEntityForHasChanges.HasChanges);
				}
			}
		}

		public void TestDoNotPumpMessageQueue()
		{
			using (var form = new ZChildForm())
			using (var control = GetControlToSpellCheck())
			{
				var textbox = GetTextbox(control);
				form.Show();
				var x = 0;
				form.BeginInvoke(new Action(() =>
				{
					x++;
				}));
				const string badWord = "nangs";
				textbox.Text = $"Here is some {badWord} text with a spelling error";
				var spellChecker = SpellCheckerTestHelpers.CreateSpellChecker(SpellCheckerTestHelpers.CreateSpellingError(textbox.Text, badWord, "NANGZZZZ!!!"));
				var checker = InitialiseSpellcheck(control, spellChecker);
				checker.SendRightClick_ForTest(textbox.Text.IndexOf(badWord) + 1);
				control.Invalidate();
				AssertEquals(0, x);
				Application.DoEvents();
				AssertEquals(1, x);
			}
		}

		public void TestDrawSquigglesUnderSpellingMistake()
		{
			using (var form = new ZChildForm()
			{ Dock = DockStyle.Fill, Size = NewScaledSize(1717, 709), FormBorderStyle = FormBorderStyle.FixedSingle })
			using (var control = GetControlToSpellCheck())
			{
				var textbox = GetTextbox(control);
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				const string badWord1 = "badWordOne", badWord2 = "badWordTwo";
				var text = $"Here is some {badWord1} text with {badWord2} a spelling error";
				var spellChecker = SpellCheckerTestHelpers.CreateSpellChecker(SpellCheckerTestHelpers.CreateSpellingError(text, badWord1, "google"), SpellCheckerTestHelpers.CreateSpellingError(text, badWord2, "google"));
				var checker = InitialiseSpellcheck(control, spellChecker);
				textbox.Text = text;
				Application.DoEvents();
				UserIdleWorker.Flush();
				Application.DoEvents();
				var spellingErrors = spellChecker.CheckSpelling(textbox.Text);
				var offset = NewScaledSize(0, 10);
				var badWord1Index = textbox.Text.IndexOf(badWord1);
				var expectedbadWordPoint1 = new Tuple<Point, Point>(textbox.GetPositionFromCharIndex(badWord1Index) + offset, textbox.GetPositionFromCharIndex(badWord1Index + badWord1.Length) + offset);
				var badWord2Index = textbox.Text.IndexOf(badWord2);
				var expectedbadWordPoint2 = new Tuple<Point, Point>(textbox.GetPositionFromCharIndex(badWord2Index) + offset, textbox.GetPositionFromCharIndex(badWord2Index + badWord2.Length) + offset);
				AssertClose(expectedbadWordPoint1, checker.Squiggles_Exposed.First(), 10);
				AssertClose(expectedbadWordPoint2, checker.Squiggles_Exposed.ElementAt(1), 10);
			}
		}

		void AssertClose(Tuple<Point, Point> expected, Tuple<Point, Point> actual, int closeness)
		{
			CombineAssertions(string.Format(CultureInfo.InvariantCulture, "{0}, {1} should be close to {2}, {3}", expected.Item1, expected.Item2, actual.Item1, actual.Item2), () =>
			{
				void AssertClose(Point ep, Point ap)
				{
					AssertCloseEnough(ep.X, ap.X, closeness);
					AssertCloseEnough(ep.Y, ap.Y, closeness);
				}

				AssertClose(expected.Item1, actual.Item1);
				AssertClose(expected.Item2, actual.Item2);
			});
		}

		public void TestIgnoredWordsWithoutSquiggles()
		{
			using (var form = new ZChildForm { Dock = DockStyle.Fill })
			using (var control = GetControlToSpellCheck())
			{
				const string ignoredWord = "wrongg";

				var textBox = GetTextbox(control);
				textBox.Text = $"Here is some ignored words, like {ignoredWord}, which should not be drawn squiggles.";

				var checker = InitialiseSpellcheck(control);
				checker.UpdateWordsToIgnore(new string[] { ignoredWord });

				form.Controls.Add(control);
				form.Show();

				AssertEquals(0, checker.Squiggles_Exposed.Count);
			}
		}

		public void TestLimitingSpellingSuggestions()
		{
			using (var control = GetControlToSpellCheck())
			{
				const string badWord = "ghghghg";
				const string text = "Here is some " + badWord + " text";
				string[] suggestions = { "google", "google1", "google2", "google3", "google4", "google5", "google6" };
				var spellChecker = SpellCheckerTestHelpers.CreateSpellChecker(SpellCheckerTestHelpers.CreateSpellingError(text, badWord, suggestions));
				var checker = InitialiseSpellcheck(control, spellChecker);
				var textbox = GetTextbox(control);
				textbox.Text = text;
				checker.SendRightClick_ForTest(textbox.Text.IndexOf(badWord) + 1);
				var items = GetContextMenu(control).Items;
				AssertEquals(5, items.Count - (items.IndexOfKey(SpellChecker.MenuItemKeys.SuggestionsSeperator) + 1));
			}
		}

		public void TestContextMenuItems_WhenSpellCheckerIsDisabled()
		{
			using (var control = GetControlToSpellCheck())
			{
				var checker = InitialiseSpellcheck(control);
				var textbox = GetTextbox(control);
				textbox.Text = "wrongg";
				checker.DisableSpellCheck_ForTest();
				control.Show();
				var items = GetContextMenu(control).Items;
				AssertNull(items[SpellChecker.MenuItemKeys.AddToDictionary]);
				AssertNotNull(items[SpellChecker.MenuItemKeys.EnableSpellCheck]);
				AssertNotNull(items[SpellChecker.MenuItemKeys.CheckSpelling]);
				AssertNotNull(items[SpellChecker.MenuItemKeys.ShowDictionary]);
				AssertEquals("No suggestions in the context menu", items.Count, items.IndexOfKey(SpellChecker.MenuItemKeys.EnableSpellCheck) + 1);
				checker.EnableSpellCheck_ForTest();
			}
		}

		public void TestContextMenuItems_WhenSpellCheckerIsEnabled()
		{
			using (var control = GetControlToSpellCheck())
			{
				var checker = InitialiseSpellcheck(control);
				var textbox = GetTextbox(control);
				control.Show();
				textbox.Text = "wrongg";
				checker.SendRightClick_ForTest(1);
				var items = GetContextMenu(control).Items;
				AssertNotNull(items[SpellChecker.MenuItemKeys.AddToDictionary]);
				AssertNotNull(items[SpellChecker.MenuItemKeys.DisableSpellCheck]);
				AssertNotNull(items[SpellChecker.MenuItemKeys.CheckSpelling]);
				AssertNotNull(items[SpellChecker.MenuItemKeys.ShowDictionary]);
				AssertGreaterThan("There are some suggestions for the wrong word", items.Count, items.IndexOfKey(SpellChecker.MenuItemKeys.DisableSpellCheck) + 1);
			}
		}

		public void TestRegistryToggles_WhenSpellCheckerIsEnabledandDisabled()
		{
			using (var control = GetControlToSpellCheck())
			{
				var textbox = GetTextbox(control);
				textbox.Name = "Whatever";
				var checker = InitialiseSpellcheck(control, null, "Whatever");
				control.Show();
				var item = SpellCheckerStatusSingleton.Instance.IsSpellCheckerEnabled("SpellCheckerKey|Whatever");
				AssertEquals(true, item);
				checker.DisableSpellCheck_ForTest();
				item = SpellCheckerStatusSingleton.Instance.IsSpellCheckerEnabled("SpellCheckerKey|Whatever");
				AssertEquals(false, item);
				checker.EnableSpellCheck_ForTest();
				item = SpellCheckerStatusSingleton.Instance.IsSpellCheckerEnabled("SpellCheckerKey|Whatever");
				AssertEquals(true, item);
			}
		}

		IDisposable CreateUserWithDictionary(out IGlbStaff user, out WordDictionary dictionary, string code = "COD")
		{
			user = (IGlbStaff)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			user.GS_Code = code;
			dictionary = Factory.NewWithValidTestData<WordDictionary>();
			dictionary.DIC_GS_NKStaff = code;
			Factory.Save();
			return Env.SetTemporaryUserContext(new UserContext(user, Env.CurrentBranchPK, Env.CurrentDepartmentPK));
		}

		SpellCheckFormAction Change(ISpellCheckerForm form)
		{
			return new SpellCheckFormAction(SpellCheckerFormResult.Change, null);
		}

		SpellCheckFormAction Ignore(ISpellCheckerForm form)
		{
			return new SpellCheckFormAction(SpellCheckerFormResult.Ignore, null);
		}

		SpellCheckFormAction Cancel(ISpellCheckerForm form)
		{
			return new SpellCheckFormAction(SpellCheckerFormResult.Cancel, null);
		}
	}
}
