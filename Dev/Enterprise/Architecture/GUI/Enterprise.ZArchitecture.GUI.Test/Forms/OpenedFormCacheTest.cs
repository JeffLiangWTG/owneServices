using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI
{
	sealed class OpenedFormCacheTest : TestCase
	{
		public void TestSwitchToCachedFormShouldRestoreLastWindowStateIfFormIsMinimized()
		{
			var testCache = new OpenedFormCache();
			var pK = Guid.NewGuid();
			const string TestModule = "TestModule";

			using (var testForm = new TestDummyForm())
			{
				testCache.Add(pK, testForm, TestModule);
				testForm.Show();

				testForm.WindowState = FormWindowState.Maximized;
				testForm.WindowState = FormWindowState.Minimized;
				testCache.SwitchToCachedForm(pK, TestModule);
				AssertEquals("WindowState should restore to Maximized", FormWindowState.Maximized, testForm.WindowState);

				testForm.WindowState = FormWindowState.Normal;
				testForm.WindowState = FormWindowState.Minimized;
				testCache.SwitchToCachedForm(pK, TestModule);
				AssertEquals("WindowState should restore to Normal", FormWindowState.Normal, testForm.WindowState);
			}
		}

		public void TestOpenedFormCache()
		{
			OpenedFormCache testCache = new OpenedFormCache();

			Guid pK = Guid.NewGuid();
			const string TestModule = "TestModule";

			using (Form testForm = new Form())
			{
				AssertEquals("Contains", false, testCache.Contains(pK, TestModule));
				testCache.Add(pK, testForm, TestModule);
				AssertEquals("Contains", true, testCache.Contains(pK, TestModule));
				AssertEquals("Count", 1, testCache.Count);

				AssertNull("GetForm with PK that isn't cached", testCache.GetForm(Guid.NewGuid(), TestModule));
				AssertEquals("GetForm", testForm, testCache.GetForm(pK, TestModule));

				testForm.Show();
				testForm.Close();
				AssertEquals("Contains", false, testCache.Contains(pK, TestModule));
			}
		}

		public void TestOpeningFormsFromDifferentModulesWithSamePK()
		{
			OpenedFormCache testCache = new OpenedFormCache();

			Guid pK = Guid.NewGuid();

			using (Form testForm1 = new Form())
			{
				using (Form testForm2 = new Form())
				{
					testCache.Add(pK, testForm1, "Module1");
					testCache.Add(pK, testForm2, "Module2");

					AssertEquals("Contains for Module1", true, testCache.Contains(pK, "Module1"));
					AssertEquals("Contains for Module2", true, testCache.Contains(pK, "Module2"));

					AssertEquals("Count", 2, testCache.Count);

					AssertEquals("GetForm for Module1", testForm1, testCache.GetForm(pK, "Module1"));
					AssertEquals("GetForm for Module2", testForm2, testCache.GetForm(pK, "Module2"));

					testForm1.Show();
					testForm1.Close();
					AssertEquals("Contains", false, testCache.Contains(pK, "Module1"));
					AssertEquals("Contains", true, testCache.Contains(pK, "Module2"));
				}
			}
		}

		public void TestCloseAllCachedForm()
		{
			var testCache = new OpenedFormCache();

			const string TestModule = "TestModule";

			using (var testForm1 = new TestForm())
			using (var testForm2 = new TestForm())
			using (var testForm3 = new TestForm())
			using (var testForm4 = new TestForm())
			{
				var testForm1Closed = false;
				var testForm2Closed = false;
				var testForm2CanClose = false;
				var testForm3Closed = false;
				var testForm3CanClose = false;
				var testForm4Closed = false;

				testForm1.Closed += (s, e) => testForm1Closed = true;
				testForm2.Closing += (s, e) => e.Cancel = !testForm2CanClose;
				testForm2.Closed += (s, e) => testForm2Closed = true;
				testForm3.Closing += (s, e) => e.Cancel = !testForm3CanClose;
				testForm3.Closed += (s, e) => testForm3Closed = true;
				testForm4.Closed += (s, e) => testForm4Closed = true;

				testForm1.DisplayMode = ODisplayMode.Browse;
				testForm1.Show();

				testForm2.DisplayMode = ODisplayMode.Edit;
				testForm2.Show();

				testForm3.DisplayMode = ODisplayMode.New;
				testForm3.Show();

				testForm4.Show();

				testCache.Add(new Guid("106AF779-863D-409A-B6D6-554E81C866DA"), testForm1, TestModule);
				testCache.Add(new Guid("206AF779-863D-409A-B6D6-554E81C866DA"), testForm2, TestModule);
				testCache.Add(new Guid("306AF779-863D-409A-B6D6-554E81C866DA"), testForm3, TestModule);
				testCache.Add(new Guid("406AF779-863D-409A-B6D6-554E81C866DA"), testForm4, TestModule);
				AssertEquals("Count", 4, testCache.Count);

				AssertEquals(false, testCache.CloseAllCachedForms());
				Application.DoEvents();
				AssertEquals("Count", 4, testCache.Count);
				AssertEquals("TestForm1Closed", false, testForm1Closed);
				AssertEquals("TestForm2Closed", false, testForm2Closed);
				AssertEquals("TestForm3Closed", false, testForm3Closed);
				AssertEquals("TestForm4Closed", false, testForm4Closed);

				/*
				 * The order of form2 and form3 may vary when tested locally or on the server. 
				 * To reach the effect of this unit test, do this loop to check which form is the first
				 * and flag that form can be closed.
				 */
				foreach (var cachedForm in testCache.FormCache)
				{
					if (cachedForm.Key.Equals("206af779-863d-409a-b6d6-554e81c866daTestModule"))
					{
						testForm2CanClose = true;
						break;
					}
					else if (cachedForm.Key.Equals("306af779-863d-409a-b6d6-554e81c866daTestModule"))
					{
						testForm3CanClose = true;
						break;
					}
				}

				AssertEquals(false, testCache.CloseAllCachedForms());
				Application.DoEvents();

				AssertEquals("Count", 3, testCache.Count);
				AssertEquals("TestForm1Closed", false, testForm1Closed);
				AssertEquals("TestForm2Closed", testForm2CanClose, testForm2Closed);
				AssertEquals("TestForm3Closed", testForm3CanClose, testForm3Closed);
				AssertEquals("TestForm4Closed", false, testForm4Closed);

				testForm3CanClose = true;
				testForm2CanClose = true;

				AssertEquals(true, testCache.CloseAllCachedForms());
				Application.DoEvents();

				AssertEquals("Count", 0, testCache.Count);
				AssertEquals("TestForm1Closed", true, testForm1Closed);
				AssertEquals("TestForm2Closed", true, testForm2Closed);
				AssertEquals("TestForm3Closed", true, testForm3Closed);
				AssertEquals("TestForm4Closed", true, testForm4Closed);
			}
		}

		public void TestGetAllOpenForms()
		{
			OpenedFormCache testCache = new OpenedFormCache();

			AssertEquals("No forms should be in list", 0, testCache.GetAllOpenForms().Count);

			using (Form testForm = new Form())
			{
				testCache.Add(Guid.NewGuid(), testForm, "TestModule");
				AssertEquals("There should be one form in the list", 1, testCache.GetAllOpenForms().Count);
			}
		}

		public void TestSwitchingToExistingFormWillRestoreFormSize()
		{
			OpenedFormCache testCache = new OpenedFormCache();
			using (Form testForm = new Form())
			{
				using (Form testForm2 = new Form())
				{
					testForm.Show();
					testForm2.Show();

					Guid testPK = Guid.NewGuid();
					testCache.Add(testPK, testForm, "TestModule");

					testForm.WindowState = FormWindowState.Minimized;
					testCache.SwitchToCachedForm(testPK, "TestModule");
					AssertEquals("TestForm.Focus", true, testForm.Focus());
					AssertEquals("TestForm.WindowState", FormWindowState.Normal, testForm.WindowState);
				}
			}
		}

		class TestForm : Form, IDisplayModeAware
		{
			public ODisplayMode DisplayMode { get; set; }
		}

		class TestDummyForm : ZForm
		{
		}
	}
}
