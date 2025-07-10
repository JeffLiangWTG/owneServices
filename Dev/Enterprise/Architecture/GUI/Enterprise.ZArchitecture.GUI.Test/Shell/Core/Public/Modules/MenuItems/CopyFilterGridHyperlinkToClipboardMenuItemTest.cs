using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public class CopyFilterGridHyperlinkToClipboardMenuItemTest : TestCaseWithFactory
	{
		public void TestAdditionalActionsMenuItems()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var notUsedButCausesActionsMenuToLoad = module.EmbeddedControl)
			{
				AssertNotNull(module.ActionsMenuItem.MenuItems.FindByText(CopyFilterGridHyperlinkToClipboardMenuItem.CopyHyperlinksToClipboardText));
			}
		}

		public void TestMultiHyperLink()
		{
			using (var module = new DummyFilterGridModule())
			using (var menuItem = new CopyFilterGridHyperlinkToClipboardMenuItem(module))
			{
				var dummyBizo1 = Factory.NewWithValidTestData<DummyBusinessObject>();
				var dummyBizo2 = Factory.NewWithValidTestData<DummyBusinessObject>();

				Factory.Save();

				menuItem.PerformClick();
				AssertNoHyperLinkInClipboard();
				AssertEquals("No row selected.", UnitTestUserNotification.Instance.LastMessage.Text);

				module.SelectedBusinessObjectsOverride = new[] { dummyBizo1, dummyBizo2 };
				menuItem.PerformClick();
				Application.DoEvents();

				AssertHyperLinksInClipboard(dummyBizo1, dummyBizo2);
			}
		}

		[SnailTest]
		public void TestMultiHyperLink_TakeItToTheLimit()
		{
			using (var module = new DummyFilterGridModule())
			using (var menuItem = new CopyFilterGridHyperlinkToClipboardMenuItem(module))
			{
				var maxNumberOfVisualisableThingsInAGrid = 1800;
				var dummies = Enumerable.Range(0, maxNumberOfVisualisableThingsInAGrid).Select(_ => Factory.NewWithValidTestData<DummyBusinessObject>()).ToArray();

				Factory.Save();

				menuItem.PerformClick();
				AssertNoHyperLinkInClipboard();
				AssertEquals("No row selected.", UnitTestUserNotification.Instance.LastMessage.Text);

				module.SelectedBusinessObjectsOverride = dummies;
				menuItem.PerformClick();
				Application.DoEvents();

				AssertHyperLinksInClipboard(dummies);
			}
		}

		public void TestHyperLink()
		{
			DataRegistry.Instance.WebHyperlinksEnabled = false;

			TestHyperLinkCore();
		}

		public void TestWebEnabledHyperLink()
		{
			DataRegistry.Instance.WebHyperlinksEnabled = true;
			WebDataRegistry.Instance.RootServicesUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://web.services.for.cw1/path/to/services");

			TestHyperLinkCore();
		}

		void TestHyperLinkCore()
		{
			using (var module = new DummyFilterGridModule())
			using (var menuItem = new CopyFilterGridHyperlinkToClipboardMenuItem(module))
			{
				var dummyBizo1 = Factory.NewWithValidTestData<DummyBusinessObject>();
				var dummyBizo2 = Factory.NewWithValidTestData<DummyBusinessObject>();

				Factory.Save();

				menuItem.PerformClick();
				AssertNoHyperLinkInClipboard();
				AssertEquals("No row selected.", UnitTestUserNotification.Instance.LastMessage.Text);

				module.SelectedBusinessObjectsOverride = new[] { dummyBizo2 };
				menuItem.PerformClick();
				AssertHyperLinksInClipboard(dummyBizo2);

				module.SelectedBusinessObjectsOverride = new[] { dummyBizo1 };
				menuItem.PerformClick();
				AssertHyperLinksInClipboard(dummyBizo1);
			}
		}

		#region Implementation

		ClipboardTestHelper clipboardTestHelper;

		protected override void SetUp()
		{
			base.SetUp();

			clipboardTestHelper = new ClipboardTestHelper();
			clipboardTestHelper.MockClipboard();
		}

		void AssertNoHyperLinkInClipboard()
		{
			var hyperlink = clipboardTestHelper.ClipboardData;
			var hyperlinkHtml = hyperlink != null ? (string)hyperlink.GetData(DataFormats.Html) : string.Empty;

			AssertEquals("No parsable hyperlinks should be found", 0, ZMenuStrategyHelper.ShortcutCreator.TryGetUrlsFromHyperlinkBadly(hyperlinkHtml).Count());
		}

		void AssertHyperLinksInClipboard(params BusinessObject[] bizos)
		{
			var hyperlink = clipboardTestHelper.ClipboardData;
			var hyperlinkHtml = hyperlink != null ? (string)hyperlink.GetData(DataFormats.Html) : string.Empty;
			var urls = ZMenuStrategyHelper.ShortcutCreator.TryGetUrlsFromHyperlinkBadly(hyperlinkHtml).ToArray();

			AssertEquals("The hyperlink is parsable", bizos.Length, urls.Length);
			AssertArrayEqualsByElements("This proves the correct hyperlink is in the clipboard.", bizos.Select(bizo => (Guid?)bizo.PK.ToGuid()).ToArray(), urls.Select(url => ZFormUtilities.GetControllerIDsFromURLSafe(url)?.Item2).ToArray());
		}

		public static void CopyBizosToClipboard(ControllerID controllerID, params BusinessObject[] bizos)
		{
			ZMenuStrategyHelper.ShortcutCreator.CopyHyperlinksToClipboard(bizos.Select(bizo => CopyFilterGridHyperlinkToClipboardMenuItem.GetUrlFromBizo(controllerID, bizo)).ToArray());
		}

		#endregion
	}
}
