using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.TrustedMessaging.Intergration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	sealed class StartupUserControlTest : TestCaseWithFactory
	{
		public void TestRefreshNowContextMenuItem()
		{
			using var control = new StartupUserControlForTest();

			var mainPanel = (TableLayoutPanel)control.Controls.Find("StartupLayoutPanel", true)[0];
			var note1 = CreateClientNote(NewsSectionTypeList.Codes.ClientNews, "Hello: Zay", "http://www.google.com/", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2008, 12, 3));
			var note2 = CreateClientNote(NewsSectionTypeList.Codes.ClientNews, "Cya: Ry", "http://www.altavista.com/", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, new ZDateTime(2010, 12, 16));

			Factory.Save();

			var menuItem = control.ContextMenu.MenuItems.FindByText("Refresh Now");
			AssertNotNull("Refresh Now Context Menu Item exists", menuItem);

			menuItem.PerformClick();
			var topLeftControl = mainPanel.GetControlFromPosition(0, 1);

			AssertSection(control, topLeftControl, NewsSectionTypeList.Descriptions.ClientNews, true, true, note1, note2);
		}

		public void TestRefreshPanelLabel()
		{
			var note1 = CreateClientNote(NewsSectionTypeList.Codes.ClientAnnouncements, "Hello: Zay", "http://www.google.com/", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2008, 12, 3), true);
			var note2 = CreateClientNote("DEM", "Cya: Ry", "http://www.altavista.com/", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, new ZDateTime(2010, 12, 16));
			Factory.Save();

			var sectionTypes = new NewsAnnouncementSectionTypeCollection();
			var sectionType = sectionTypes.AddNew();
			sectionType.Code = "DEM";
			sectionType.Description = (NoResString)"Demo Section";
			sectionType.OrderItemsBy = NewsSectionSortTypeList.Codes.PublishedTime;
			sectionType.SystemDefined = false;

			var sectionType2 = sectionTypes.AddNew();
			sectionType2.Code = NewsSectionTypeList.Codes.ClientAnnouncements;
			sectionType2.Description = NewsSectionTypeList.Descriptions.ClientAnnouncements;
			sectionType2.OrderItemsBy = NewsSectionSortTypeList.Codes.PublishedTime;
			sectionType2.SystemDefined = true;

			SystemDataRegistry.Instance.NewsSectionTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, sectionTypes);

			var layoutConfig = new NewsSectionCollection();

			var topLeft = layoutConfig.AddNew();
			topLeft.SectionID = NewsSectionTypeList.Codes.ClientAnnouncements;
			topLeft.LayoutPanelID = "Top-Left";
			topLeft.HideReadItems = false;
			topLeft.MandatoryToRead = true;

			SystemDataRegistry.Instance.NewsSectionLayouts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, layoutConfig);
			using var control = new StartupUserControlForTest();
			var mainPanel = (TableLayoutPanel)control.Controls.Find("StartupLayoutPanel", true)[0];

			var topLeftControl = mainPanel.GetControlFromPosition(0, 1);
			AssertSection(control, topLeftControl, NewsSectionTypeList.Descriptions.ClientAnnouncements, true, true, note1);

			topLeft.SectionID = "DEM";
			SystemDataRegistry.Instance.NewsSectionLayouts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, layoutConfig);

			var menuItem = control.ContextMenu.MenuItems.FindByText("Refresh Now");
			AssertNotNull("Refresh Now Context Menu Item exists", menuItem);
			menuItem.PerformClick();

			topLeftControl = mainPanel.GetControlFromPosition(0, 1);
			AssertSection(control, topLeftControl, "Demo Section", true, true, note2);
		}

		public void TestMouseOverGridDoesNotStealFocus()
		{
			using var form = new ZChildForm();
			using var control = new StartupUserControlForTest();
			using var textbox = new ZTextBox();
			_ = CreateClientNote(NewsSectionTypeList.Codes.ClientNews, "Hello: Zay", "http://www.google.com/", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2008, 12, 3));
			_ = CreateClientNote(NewsSectionTypeList.Codes.ClientNews, "Cya: Ry", "http://www.altavista.com/", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, new ZDateTime(2010, 12, 16));

			Factory.Save();

			textbox.Dock = DockStyle.Left;
			control.Dock = DockStyle.Right;

			form.Size = ControlDpiScalingHelper.NewScaledSize(200, 0) + control.Size;

			form.Controls.Add(textbox);
			form.Controls.Add(control);
			form.Show();

			var smallOffsetToEnsureInsideControl = ControlDpiScalingHelper.NewScaledPoint(10, 10);

			Application.DoEvents();

			Cursor.Position = textbox.PointToScreen(smallOffsetToEnsureInsideControl);
			textbox.Focus();

			Application.DoEvents();

			Assert("PRE: Textbox has focus", textbox.Focused);

			Cursor.Position = control.Controls.Find("SectionFlowPanel", true)[0].PointToScreen(smallOffsetToEnsureInsideControl);

			Application.DoEvents();

			Assert("Textbox should still have focus", textbox.Focused);
		}

		public void TestNewsPanels()
		{
			var note1 = CreateClientNote(NewsSectionTypeList.Codes.ClientAnnouncements, "Hello: Zay", "http://www.google.com/", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2008, 12, 3), true);
			var note2 = CreateClientNote(NewsSectionTypeList.Codes.ClientAnnouncements, "Cya: Ry", "http://www.altavista.com/", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, new ZDateTime(2010, 12, 16));
			_ = CreateClientNote(NewsSectionTypeList.Codes.ClientAnnouncements, "Dude: Lola", "http://www.dogsearch.com/", Core.Constants.CountryCodes.India, new ZDateTime(2008, 12, 3));
			Factory.Save();

			var wiseTechNotes = new DummyRefGlbReleaseNote[]
			{
				new(NewsSectionTypeList.Codes.WiseNews, "Arivadeci: Lola", "http://www.metacrawler.com/", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2008, 12, 3)),
				new(NewsSectionTypeList.Codes.WiseLearningUpdates, "Konichiwa Baby 3", "http://www.yahoo.com/", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2008, 12, 3)),
				new(NewsSectionTypeList.Codes.WiseLearningUpdates, "Konichiwa Baby 4", "http://www.yahoo.com/", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2008, 12, 3)) { IsRead = true }
			};

			RefGlbReleaseNoteTestDataHelper.InsertRefGlbReleaseNotes(TestConnection, wiseTechNotes);

			var note3 = Factory.Load<GlbReleaseNoteCombined>(wiseTechNotes[0].PK);
			var note4 = Factory.Load<GlbReleaseNoteCombined>(wiseTechNotes[1].PK);

			var layoutConfig = new NewsSectionCollection();

			var topLeft = layoutConfig.AddNew();
			topLeft.SectionID = NewsSectionTypeList.Codes.ClientAnnouncements;
			topLeft.LayoutPanelID = "Top-Left";
			topLeft.HideReadItems = false;
			topLeft.MandatoryToRead = true;

			var bottomLeft = layoutConfig.AddNew();
			bottomLeft.SectionID = NewsSectionTypeList.Codes.WiseLearningUpdates;
			bottomLeft.LayoutPanelID = "Bottom-Left";
			bottomLeft.HideReadItems = true;
			bottomLeft.MandatoryToRead = true;

			var bottomMiddle = layoutConfig.AddNew();
			bottomMiddle.SectionID = NewsSectionTypeList.Codes.WiseNews;
			bottomMiddle.LayoutPanelID = "Bottom-Middle";
			bottomMiddle.HideReadItems = true;
			bottomMiddle.MandatoryToRead = false;

			var bottomRight = layoutConfig.AddNew();
			bottomRight.SectionID = NewsSectionTypeList.Codes.ProductUpdates;
			bottomRight.LayoutPanelID = "Bottom-Right";
			bottomRight.HideReadItems = true;
			bottomRight.MandatoryToRead = false;

			SystemDataRegistry.Instance.NewsSectionLayouts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, layoutConfig);
			using var control = new StartupUserControlForTest();
			var mainPanel = (TableLayoutPanel)control.Controls.Find("StartupLayoutPanel", true)[0];
			var unReadItemsBeforeClick = control.GetUnReadItems();

			AssertEquals("three UnReadItems exist", 3, unReadItemsBeforeClick.Count);
			Assert("note2 is UnReadItem", unReadItemsBeforeClick.ContainsKey(note2.PK.ToString()));
			Assert("note4 is UnReadItem", unReadItemsBeforeClick.ContainsKey(note3.PK.ToString()));
			Assert("note5 is UnReadItem", unReadItemsBeforeClick.ContainsKey(note4.PK.ToString()));

			var topLeftControl = mainPanel.GetControlFromPosition(0, 1);
			AssertSection(control, topLeftControl, NewsSectionTypeList.Descriptions.ClientAnnouncements, true, true, note2, note1);
			AssertEquals(1, mainPanel.GetRowSpan(topLeftControl));

			var topMiddleControl = mainPanel.GetControlFromPosition(1, 1);
			AssertSection(control, topMiddleControl, NewsSectionTypeList.Descriptions.WiseNews, true, true, note3);
			AssertEquals(2, mainPanel.GetRowSpan(topMiddleControl));

			var topRightControl = mainPanel.GetControlFromPosition(2, 1);
			AssertSection(control, topRightControl, NewsSectionTypeList.Descriptions.ProductUpdates, true, true);
			AssertEquals(2, mainPanel.GetRowSpan(topRightControl));

			var bottomLeftControl = mainPanel.GetControlFromPosition(0, 2);
			AssertSection(control, bottomLeftControl, NewsSectionTypeList.Descriptions.WiseLearningUpdates, true, true, note4);
			AssertEquals(1, mainPanel.GetRowSpan(bottomLeftControl));

			var bottomMiddleControl = mainPanel.GetControlFromPosition(1, 2);
			AssertEquals(topMiddleControl, bottomMiddleControl);

			var bottomRightControl = mainPanel.GetControlFromPosition(2, 2);
			AssertEquals(topRightControl, bottomRightControl);

			var unReadItemsAfterClick = control.GetUnReadItems();
			AssertEquals("no UnReadItems exist", 0, unReadItemsAfterClick.Count);
		}

		public void TestGetNews_OrderByRegistryNewsSectionTypesSetting()
		{
			var sectionTypes = new NewsAnnouncementSectionTypeCollection();
			var sectionType = sectionTypes.AddNew();
			sectionType.Code = NewsSectionTypeList.Codes.ClientAnnouncements;
			sectionType.Description = NewsSectionTypeList.Descriptions.ClientAnnouncements;
			sectionType.OrderItemsBy = NewsSectionSortTypeList.Codes.PublishedTime;
			sectionType.SystemDefined = true;

			var sectionType2 = sectionTypes.AddNew();
			sectionType2.Code = NewsSectionTypeList.Codes.WiseLearningUpdates;
			sectionType2.Description = NewsSectionTypeList.Descriptions.WiseLearningUpdates;
			sectionType2.OrderItemsBy = NewsSectionSortTypeList.Codes.Alphabetically;
			sectionType.SystemDefined = true;

			SystemDataRegistry.Instance.NewsSectionTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, sectionTypes);

			var note1 = CreateClientNote(NewsSectionTypeList.Codes.ClientAnnouncements, "Hello: Zay", "http://www.google.com/", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2018, 12, 3));
			var note2 = CreateClientNote(NewsSectionTypeList.Codes.ClientAnnouncements, "Dude: Lola", "http://www.dogsearch.com/", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2016, 12, 3));
			var note3 = CreateClientNote(NewsSectionTypeList.Codes.ClientAnnouncements, "Cya: Ry", "http://www.altavista.com/", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2017, 12, 3));
			Factory.Save();

			var wiseTechNotes = new DummyRefGlbReleaseNote[]
			{
				new (NewsSectionTypeList.Codes.WiseLearningUpdates, "Konichiwa Baby 4", "http://www.metacrawler.com/", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2018, 12, 3)),
				new (NewsSectionTypeList.Codes.WiseLearningUpdates, "Konichiwa Baby 3", "http://www.yahoo.com/", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2017, 12, 3)),
				new (NewsSectionTypeList.Codes.WiseLearningUpdates, "Konichiwa Baby 2", "http://www.yahoo.com/", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2016, 12, 3))
			};

			RefGlbReleaseNoteTestDataHelper.InsertRefGlbReleaseNotes(TestConnection, wiseTechNotes);

			var note4 = Factory.Load<GlbReleaseNoteCombined>(wiseTechNotes[0].PK);
			var note5 = Factory.Load<GlbReleaseNoteCombined>(wiseTechNotes[1].PK);
			var note6 = Factory.Load<GlbReleaseNoteCombined>(wiseTechNotes[2].PK);

			var layoutConfig = new NewsSectionCollection();

			var topLeft = layoutConfig.AddNew();
			topLeft.SectionID = NewsSectionTypeList.Codes.ClientAnnouncements;
			topLeft.LayoutPanelID = "Top-Left";
			topLeft.HideReadItems = false;
			topLeft.MandatoryToRead = true;

			var bottomLeft = layoutConfig.AddNew();
			bottomLeft.SectionID = NewsSectionTypeList.Codes.WiseLearningUpdates;
			bottomLeft.LayoutPanelID = "Bottom-Left";
			bottomLeft.HideReadItems = true;
			bottomLeft.MandatoryToRead = true;

			SystemDataRegistry.Instance.NewsSectionLayouts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, layoutConfig);
			using var control = new StartupUserControlForTest();
			var mainPanel = (TableLayoutPanel)control.Controls.Find("StartupLayoutPanel", true)[0];
			var unReadItemsBeforeClick = control.GetUnReadItems();
			AssertEquals("6 UnReadItems exist", 6, unReadItemsBeforeClick.Count);

			var topLeftControl = mainPanel.GetControlFromPosition(0, 1);
			var flowPanel = (FlowLayoutPanel)topLeftControl.Controls.Find("SectionFlowPanel", true)[0];
			var labels = flowPanel.Controls.Find("LinkLabel", true).OfType<ZLinkLabel>();
			AssertContainsExactElementsInExactOrder("ClientAnnouncements is ordered by PublishedTime", new string[] { note1.Title, note3.Title, note2.Title }, labels.Select(x => x.Text).ToArray());

			var bottomLeftControl = mainPanel.GetControlFromPosition(0, 2);
			var flowPanel2 = (FlowLayoutPanel)bottomLeftControl.Controls.Find("SectionFlowPanel", true)[0];
			var labels2 = flowPanel2.Controls.Find("LinkLabel", true).OfType<ZLinkLabel>();
			AssertContainsExactElementsInExactOrder("WiseLearningUpdates is ordered by Alphabetically", new string[] { note6.Title, note5.Title, note4.Title }, labels2.Select(x => x.Text).ToArray());
		}

		public void TestProductUpdatesPanelRead()
		{
			var wiseTechNotes = new DummyRefGlbReleaseNote[]
			{
				new (NewsSectionTypeList.Codes.BorderWise, "1bor read", "http://www.altavista.com/", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2010, 12, 16)) { IsRead = true },
				new (NewsSectionTypeList.Codes.ProductUpdates, "2c1u read", "http://www.google.com/", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2008, 12, 3)) { IsRead = true },
				new (NewsSectionTypeList.Codes.WiseLearningUpdates, "3wlu read", "http://www.dogsearch.com/", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2008, 12, 3)) { IsRead = true },
				new (NewsSectionTypeList.Codes.ProductUpdates, "4c1u unread", "http://www.metacrawler.com/", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2008, 12, 3)),
				new (NewsSectionTypeList.Codes.BorderWise, "5bor unread", "http://www.yahoo.com/", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2008, 12, 3)),
				new (NewsSectionTypeList.Codes.WiseNews, "6wns unread", "http://www.yahoo.com/", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2008, 12, 3)),
				new (NewsSectionTypeList.Codes.WiseLearningUpdates, "7wlu unread2", "http://www.yahoo.com/", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2008, 12, 3))
			};
			RefGlbReleaseNoteTestDataHelper.InsertRefGlbReleaseNotes(TestConnection, wiseTechNotes);
			var notes = Factory.Load<GlbReleaseNoteCombined>(new ZQuery(GlbReleaseNoteCombinedSchema.GF_Section, new[] { NewsSectionTypeList.Codes.BorderWise, NewsSectionTypeList.Codes.ProductUpdates, NewsSectionTypeList.Codes.WiseLearningUpdates, NewsSectionTypeList.Codes.WiseNews }) { OrderBy = GlbReleaseNoteCombinedSchema.Constants.GF_Summary });

			var layoutConfig = new NewsSectionCollection();

			var bottomLeft = layoutConfig.AddNew();
			bottomLeft.SectionID = NewsSectionTypeList.Codes.ProductUpdates;
			bottomLeft.LayoutPanelID = "Bottom-Left";
			bottomLeft.HideReadItems = true;
			bottomLeft.MandatoryToRead = false;

			SystemDataRegistry.Instance.NewsSectionLayouts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, layoutConfig);
			using var control = new StartupUserControlForTest();
			var mainPanel = (TableLayoutPanel)control.Controls.Find("StartupLayoutPanel", true)[0];
			var unReadItemsBeforeClick = control.GetUnReadItems();

			AssertEquals("Two UnReadItems exist", 2, unReadItemsBeforeClick.Count);
			Assert("note4 is UnReadItem", unReadItemsBeforeClick.ContainsKey(notes[3].PK.ToString()));
			Assert("note5 is UnReadItem", unReadItemsBeforeClick.ContainsKey(notes[4].PK.ToString()));

			var bottomLeftControl = mainPanel.GetControlFromPosition(0, 2);
			AssertSection(control, bottomLeftControl, NewsSectionTypeList.Descriptions.ProductUpdates, true, true, notes[3], notes[4]);
			AssertEquals(2, mainPanel.GetRowSpan(bottomLeftControl));

			var unReadItemsAfterClick = control.GetUnReadItems();
			AssertEquals("no UnReadItems exist", 0, unReadItemsAfterClick.Count);
		}

		public void TestReadBOR_NotesAreHidden()
		{
			var wiseTechNotes = new DummyRefGlbReleaseNote[]
			{
				new (NewsSectionTypeList.Codes.ProductUpdates, "1c1u read", "http://www.google.com/", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2008, 12, 3)) { IsRead = true },
				new (NewsSectionTypeList.Codes.BorderWise, "2bor read", "http://www.altavista.com/", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2010, 12, 16)) { IsRead = true },
				new (NewsSectionTypeList.Codes.ProductUpdates, "3c1u unread", "http://www.metacrawler.com/", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2008, 12, 3)),
				new (NewsSectionTypeList.Codes.BorderWise, "4bor unread", "http://www.yahoo.com/", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2008, 12, 3)),
			};

			RefGlbReleaseNoteTestDataHelper.InsertRefGlbReleaseNotes(TestConnection, wiseTechNotes);

			var notes = Factory.Load<GlbReleaseNoteCombined>(new ZQuery(GlbReleaseNoteCombinedSchema.GF_Section, new[] { NewsSectionTypeList.Codes.BorderWise, NewsSectionTypeList.Codes.ProductUpdates }) { OrderBy = GlbReleaseNoteCombinedSchema.Constants.GF_Summary });

			var layoutConfig = new NewsSectionCollection();

			var topLeft = layoutConfig.AddNew();
			topLeft.SectionID = NewsSectionTypeList.Codes.ProductUpdates;
			topLeft.LayoutPanelID = "Top-Left";
			topLeft.HideReadItems = true;
			topLeft.MandatoryToRead = false;

			var topRight = layoutConfig.AddNew();
			topRight.SectionID = NewsSectionTypeList.Codes.BorderWise;
			topRight.LayoutPanelID = "Top-Right";
			topRight.HideReadItems = true;
			topRight.MandatoryToRead = false;

			SystemDataRegistry.Instance.NewsSectionLayouts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, layoutConfig);
			using var control = new StartupUserControlForTest();

			var mainPanel = (TableLayoutPanel)control.Controls.Find("StartupLayoutPanel", true)[0];
			var topLeftControl = mainPanel.GetControlFromPosition(0, 1);
			AssertSection(control, topLeftControl, NewsSectionTypeList.Descriptions.ProductUpdates, false, true, notes[2], notes[3]);

			var topRightControl = mainPanel.GetControlFromPosition(2, 1);
			AssertSection(control, topRightControl, NewsSectionTypeList.Descriptions.BorderWise, false, false, notes[3]);

			AssertSection(control, topLeftControl, NewsSectionTypeList.Descriptions.ProductUpdates, true, false, notes[2], notes[3]);
			AssertSection(control, topRightControl, NewsSectionTypeList.Descriptions.BorderWise, false, false);
		}

		public void TestProductUpdatesShowsSections_BOR_C1U()
		{
			var wiseTechNotes = new DummyRefGlbReleaseNote[]
			{
				new(NewsSectionTypeList.Codes.ProductUpdates, "A", "http://www.google.com/", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2008, 12, 5)),
				new(NewsSectionTypeList.Codes.BorderWise, "B", "http://www.yahoo.com/", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2008, 12, 4))
			};

			RefGlbReleaseNoteTestDataHelper.InsertRefGlbReleaseNotes(TestConnection, wiseTechNotes);

			var layoutConfig = new NewsSectionCollection();

			var bottomLeft = layoutConfig.AddNew();
			bottomLeft.SectionID = NewsSectionTypeList.Codes.ProductUpdates;
			bottomLeft.LayoutPanelID = "Bottom-Left";

			SystemDataRegistry.Instance.NewsSectionLayouts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, layoutConfig);
			using var control = new StartupUserControlForTest();

			var mainPanel = (TableLayoutPanel)control.Controls.Find("StartupLayoutPanel", true)[0];
			var bottomLeftControl = mainPanel.GetControlFromPosition(0, 2);

			var flowPanel = (FlowLayoutPanel)bottomLeftControl.Controls.Find("SectionFlowPanel", true)[0];
			var labels = flowPanel.Controls.Find("LinkLabel", true).OfType<ZLinkLabel>();

			AssertEquals("Expected BOR and C1U news types to be displayed in Product Updates News Section", 2, labels.Count());
		}

		public void TestNewsPanels_BeforeLogin()
		{
			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			using (var control = new StartupUserControlForTest())
			{
				var mainPanel = (TableLayoutPanel)control.Controls.Find("StartupLayoutPanel", true)[0];

				var topLeftControl = mainPanel.GetControlFromPosition(0, 1);
				AssertSection(control, topLeftControl, NewsSectionTypeList.Descriptions.ProductUpdates, true, true);
				AssertEquals(2, mainPanel.GetRowSpan(topLeftControl));

				var topMiddleControl = mainPanel.GetControlFromPosition(1, 1);
				AssertSection(control, topMiddleControl, NewsSectionTypeList.Descriptions.WiseLearningUpdates, true, true);
				AssertEquals(2, mainPanel.GetRowSpan(topMiddleControl));

				var topRightControl = mainPanel.GetControlFromPosition(2, 1);
				AssertSection(control, topRightControl, NewsSectionTypeList.Descriptions.WiseNews, true, true);
				AssertEquals(2, mainPanel.GetRowSpan(topRightControl));

				var bottomLeftControl = mainPanel.GetControlFromPosition(0, 2);
				AssertEquals(topLeftControl, bottomLeftControl);

				var bottomMiddleControl = mainPanel.GetControlFromPosition(1, 2);
				AssertEquals(topMiddleControl, bottomMiddleControl);

				var bottomRightControl = mainPanel.GetControlFromPosition(2, 2);
				AssertEquals(topRightControl, bottomRightControl);
			}
		}

		public void TestShowItem()
		{
			var note = CreateClientNote(NewsSectionTypeList.Codes.ClientAnnouncements, "Hello: Zay", "http://www.google.com/", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2008, 12, 3));
			note.Delete();

			using (var control = new StartupUserControlForTest())
			{
				control.ShowItem(note, null);
			}

			AssertEquals("Error Message of deleted-news-item should be displayed", UnitTestUserNotification.Instance.LastMessage.Text, "This news item has been deleted.");
		}

		public void TestShowItem_ConcurrencyException()
		{
			Factory.RefreshEnabled = false;
			var note = Factory.NewWithValidTestData<GlbReleaseNoteCombinedForTest>();
			note.GF_Section = NewsSectionTypeList.Codes.ClientNews;
			note.GF_Summary = "Hello: Zay";
			note.GF_URL = "http://www.google.com/";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsSystemAccount = false;
			Factory.Save();

			note.GF_ReleaseNoteDate = ZDateTime.Now.AddMinutes(1); //dummy change so following invocation runs

			note.Saving = () =>
			{
				var noteInFactory2 = new BusinessObjectFactory { RefreshEnabled = false }.Load<GlbReleaseNoteCombined>(note.PK);
				noteInFactory2.IsCurrentlyRead = true;
				noteInFactory2.Factory.Save();
			};

			using var control = new StartupUserControl();
			using var control2 = new StartupUserControl();
			using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				control.ShowItem(note, new ZLinkLabel());
				AssertEquals(true, note.IsCurrentlyRead);
				AssertEquals(true, new BusinessObjectFactory().Load<GlbReleaseNoteCombinedForTest>(note.PK).IsCurrentlyRead);
			}
		}

		public void TestShowItem_IsWiseTechGlobalItemViaTrustedMessaging()
		{
			try
			{
				WebDataRegistry.Instance.EnableTrustedMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var dummyRefGlbReleaseNote = new DummyRefGlbReleaseNote(NewsSectionTypeList.Codes.ProductUpdates, "A", "http://www.google.com/", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2008, 12, 5));
				RefGlbReleaseNoteTestDataHelper.InsertRefGlbReleaseNotes(TestConnection, dummyRefGlbReleaseNote);

				var note = Factory.LoadTop1<GlbReleaseNoteCombinedForTest>(new ZQuery(GlbReleaseNoteCombinedSchema.GF_Section, new[] { NewsSectionTypeList.Codes.ProductUpdates }));

				AssertNotNull(note);
				AssertEquals(true, note.IsWiseTechGlobalItemViaTrustedMessaging);
				WebUrlLauncher.ClearLastUrlLaunched();

				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_IsSystemAccount = false;
				Factory.Save();

				var checker = new SystemUserAccountCollectionTermCheckerForTest();
				using (ObjectFactory.Substitute<ISystemUserAccountCollectionTermChecker>(checker))
				using (var control = new StartupUserControl())
				{
					using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
					{
						AssertEquals(false, checker.TermAcknowledged);
						control.ShowItem(note, new ZLinkLabel());
						AssertEquals("", WebUrlLauncher.LastUrlLaunched);

						checker.TermAcknowledged = true;
						AssertEquals(true, checker.TermAcknowledged);
						control.ShowItem(note, new ZLinkLabel());
						AssertEquals("http://www.cw1.com/token?k=123", WebUrlLauncher.LastUrlLaunched);
					}
				}
			}
			finally
			{
				WebUrlLauncher.ClearLastUrlLaunched();
			}
		}

		public void TestShowItem_IsWiseTechGlobalItemViaTrustedMessaging_CurrentUserSystemAdmin()
		{
			try
			{
				WebDataRegistry.Instance.EnableTrustedMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var dummyRefGlbReleaseNote = new DummyRefGlbReleaseNote(NewsSectionTypeList.Codes.ProductUpdates, "A", "http://www.cw1.com/123.pdf", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2008, 12, 5));
				RefGlbReleaseNoteTestDataHelper.InsertRefGlbReleaseNotes(TestConnection, dummyRefGlbReleaseNote);

				var note = Factory.LoadTop1<GlbReleaseNoteCombinedForTest>(new ZQuery(GlbReleaseNoteCombinedSchema.GF_Section, new[] { NewsSectionTypeList.Codes.ProductUpdates }));

				AssertNotNull(note);
				AssertEquals(true, note.IsWiseTechGlobalItemViaTrustedMessaging);
				WebUrlLauncher.ClearLastUrlLaunched();

				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_IsSystemAccount = false;

				var adminStaff = Factory.NewWithValidTestData<GlbStaff>();
				adminStaff.GS_IsSystemAccount = true;
				Factory.Save();

				var checker = new SystemUserAccountCollectionTermCheckerForTest();
				using (ObjectFactory.Substitute<ISystemUserAccountCollectionTermChecker>(checker))
				using (var control = new StartupUserControl())
				{
					using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
					{
						checker.TermAcknowledged = true;
						AssertEquals(true, checker.TermAcknowledged);
						control.ShowItem(note, new ZLinkLabel());
						AssertEquals("http://www.cw1.com/token?k=123", WebUrlLauncher.LastUrlLaunched);
					}

					using (Env.SetTemporaryUserContext(new UserContext(adminStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
					{
						WebUrlLauncher.ClearLastUrlLaunched();
						control.ShowItem(note, new ZLinkLabel());
						AssertEquals("http://www.cw1.com/123.pdf", WebUrlLauncher.LastUrlLaunched);
					}
				}
			}
			finally
			{
				WebUrlLauncher.ClearLastUrlLaunched();
			}
		}

		public void TestAllowAutoLogin()
		{
			try
			{
				WebDataRegistry.Instance.EnableTrustedMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var dummyRefGlbReleaseNote = new DummyRefGlbReleaseNote(NewsSectionTypeList.Codes.ProductUpdates, "A", "http://www.cw1.com/123.pdf", GlbReleaseNoteLookups.AllCountriesCode, new ZDateTime(2008, 12, 5));
				RefGlbReleaseNoteTestDataHelper.InsertRefGlbReleaseNotes(TestConnection, dummyRefGlbReleaseNote);

				var note = Factory.LoadTop1<GlbReleaseNoteCombinedForTest>(new ZQuery(GlbReleaseNoteCombinedSchema.GF_Section, new[] { NewsSectionTypeList.Codes.ProductUpdates }));

				AssertNotNull(note);
				AssertEquals(true, note.IsWiseTechGlobalItemViaTrustedMessaging);
				WebUrlLauncher.ClearLastUrlLaunched();

				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_IsSystemAccount = false;
				Factory.Save();

				var checker = new SystemUserAccountCollectionTermCheckerForTest();
				using (ObjectFactory.Substitute<ISystemUserAccountCollectionTermChecker>(checker))
				using (var control = new StartupUserControl())
				using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					checker.TermAcknowledged = true;
					AssertEquals(true, checker.TermAcknowledged);
					AssertEquals(true, control.AllowAutoLogin);

					control.ShowItem(note, new ZLinkLabel());
					AssertEquals("http://www.cw1.com/token?k=123", WebUrlLauncher.LastUrlLaunched);

					control.AllowAutoLogin = false;
					AssertEquals(false, control.AllowAutoLogin);

					control.ShowItem(note, new ZLinkLabel());
					AssertEquals("http://www.cw1.com/123.pdf", WebUrlLauncher.LastUrlLaunched);
				}
			}
			finally
			{
				WebUrlLauncher.ClearLastUrlLaunched();
			}
		}

		public void TestReloadNewsIfThrowsExceptionInNewSections()
		{
			using var control = new StartupUserControlForTest();
			control.ThrowExceptionInNewsSections = true;
			AssertNoExceptionThrown(control.ReloadNews);

			AssertEquals("The news can not be loaded. Error: SHUTDOWN is in progress.\nLogin failed for user 'xxx'. Only administrators may connect at this time.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		class SystemUserAccountCollectionTermCheckerForTest : ISystemUserAccountCollectionTermChecker
		{
			public bool TermAcknowledged { get; set; }

			public Task<bool> CheckTermAcknowledged() => Task.FromResult(TermAcknowledged);
		}

		class GlbReleaseNoteCombinedForTest : GlbReleaseNoteCombined
		{
			public GlbReleaseNoteCombinedForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override void OnSaving()
			{
				Saving?.Invoke();
				base.OnSaving();
			}

			public Action Saving;

			public override ZString GetDownloadURL() => "http://www.cw1.com/token?k=123";
		}

		class StartupUserControlForTest : StartupUserControl
		{
			protected override void ShowItemCore(GlbReleaseNoteCombined item, ZLinkLabel linkLabel)
			{
				ShownItemPK = item.PK.ToGuid();
			}
			public Guid ShownItemPK;
		}

		void AssertSection(StartupUserControlForTest startupUserControl, Control control, string title, bool doClick, bool checkBackColor, params GlbReleaseNoteCombined[] notes)
		{
			var titleLabel = (Label)control.Controls.Find("TitleLabel", true)[0];
			AssertEquals(title, titleLabel.Text);
			AssertEquals(new Font("Segoe UI", 13), titleLabel.Font);
			AssertEquals(new Padding(0), titleLabel.Padding);
			AssertEquals(new Padding(0, 0, 0, 3), titleLabel.Margin);

			var splitContainer = (SplitContainer)control;
			AssertEquals(splitContainer.Panel1MinSize, 28);

			var flowPanel = (FlowLayoutPanel)control.Controls.Find("SectionFlowPanel", true)[0];
			AssertEquals(true, flowPanel.WrapContents);
			AssertEquals(true, flowPanel.AutoScroll);
			AssertEquals(DockStyle.Fill, flowPanel.Dock);
			AssertEquals(new Padding(0), flowPanel.Padding);
			AssertEquals(new Padding(3), flowPanel.Margin);

			var labels = flowPanel.Controls.Find("LinkLabel", true).OfType<ZLinkLabel>();
			AssertContainsExactElementsInAnyOrder(notes.Select(x => x.Title.ToString()).ToArray(), labels.Select(x => x.Text).ToArray());

			if (doClick)
			{
				var backColor = Color.White;
				int disposedLabelCount = 0;
				foreach (var label in labels)
				{
					var currentNote = notes.Where(x => x.Title == label.Text).ToArray().First();

					backColor = backColor == Color.White ? SystemDataRegistry.Instance.ColorTheme.GridAlternatingRowColor : Color.White;

					AssertEquals(currentNote.IsCurrentlyRead ? new Font("Arial", 8) : new Font("Arial", 8, FontStyle.Bold), label.Font);
					AssertEquals(new Padding(0), label.Padding);
					AssertEquals(new Padding(0, 3, 0, 3), label.Margin);
					AssertEquals(DockStyle.Fill, label.Dock);
					AssertEquals(true, label.AutoSize);
					AssertEquals(LinkBehavior.HoverUnderline, label.LinkBehavior);
					if (checkBackColor)
					{
						AssertEquals(backColor, label.BackColor);
					}

					PerformLinkClicked(label);
					var reloadedLabels = flowPanel.Controls.Find("LinkLabel", true).OfType<ZLinkLabel>();
					AssertEquals("Label is binded to note", startupUserControl.ShownItemPK, currentNote.PK);
					var factory = new BusinessObjectFactory { NameForDebugging = "StartupUserControl.News" };
					var nodeForVerifyIsCurrentlyRead = factory.Load<GlbReleaseNoteCombined>(currentNote.PK);
					AssertEquals(true, nodeForVerifyIsCurrentlyRead.IsCurrentlyRead);
					AssertEquals(false, currentNote.HasChanges);
					if (!startupUserControl.HideReadItems[notes.First().GF_Section]) //edge case reached when each note in notes has a different GF_Section
					{
						AssertEquals(new Font("Arial", 8), label.Font); //expected label.font.OriginalFontName is null in edgecase
						AssertEquals("Label not removed, as previously read notes are still showing", labels.Count(), reloadedLabels.Count() + disposedLabelCount);
					}
					else
					{
						disposedLabelCount++;
						reloadedLabels = flowPanel.Controls.Find("LinkLabel", true).OfType<ZLinkLabel>();
						AssertEquals(labels.Count() - disposedLabelCount, reloadedLabels.Count());
					}
				}
			}
		}

		void PerformLinkClicked(Control linkLabel)
		{
			linkLabel.GetType().InvokeMember("OnLinkClicked", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, linkLabel, [new LinkLabelLinkClickedEventArgs(null)]);
		}

		GlbReleaseNoteCombined CreateClientNote(string section, string summary, string url, string countryCode, ZDateTime publishedDate, bool read = false)
		{
			var note = Factory.New<GlbReleaseNoteCombined>();
			note.GF_Section = section;
			note.GF_Summary = summary;
			note.GF_URL = url;
			note.GF_RN_NKCountryForReleaseNote = countryCode;
			note.GF_ReleaseNoteDate = publishedDate;

			if (read)
			{
				note.IsCurrentlyRead = true;
			}

			return note;
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(GlbReleaseNoteReadSchema.Constants.TableName);
			TestCaseHelper.ClearTable(GlbReleaseNoteSchema.Constants.TableName);
		}
	}
}
