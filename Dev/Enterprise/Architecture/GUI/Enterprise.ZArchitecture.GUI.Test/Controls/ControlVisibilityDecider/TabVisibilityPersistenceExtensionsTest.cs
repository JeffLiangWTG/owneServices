using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI
{
	sealed class TabVisibilityPersistenceExtensionsTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestStoreAndRetrieveTabsVisibility()
		{
			var dummy = Factory.New<CargoWise.EntityFramework.Testing.DummyBusinessObject>();

			using (var tabControl = new ZTabControl())
			using (var page1 = new ZTabPage { Name = "page1" })
			using (var page2 = new ZTabPage { Name = "page2" })
			{
				tabControl.TabPages.AddRange(new[] { page1, page2 });

				Action<ZTabPage, bool, string> setTabPageVisibilityAndAssertXml = (tab, visible, expectedXml) =>
				{
					tab.TabVisible = visible;
					TabVisibilityPersistenceExtensions.StoreTabVisible(null, tab, dummy, DummyBizoSchema.Z0_VarCharMax);
					AssertEquals(expectedXml, dummy.Z0_VarCharMax);

					Factory.Save();

					var dummyInNewFactory = new BusinessObjectFactory().Load<CargoWise.EntityFramework.Testing.DummyBusinessObject>(dummy.PK);
					var retrievedVisibility = TabVisibilityPersistenceExtensions.RetrieveTabPageVisible(null, tab, dummyInNewFactory, DummyBizoSchema.Z0_VarCharMax);
					AssertEquals(visible, retrievedVisibility);
				};

				setTabPageVisibilityAndAssertXml(page1, true, ZString.Empty);
				setTabPageVisibilityAndAssertXml(page1, false, TabVisibilityPersistenceExtensions.InvisibleTabsOpenTag + ";page1;" + TabVisibilityPersistenceExtensions.InvisibleTabsCloseTag);
				setTabPageVisibilityAndAssertXml(page2, false, TabVisibilityPersistenceExtensions.InvisibleTabsOpenTag + ";page1;page2;" + TabVisibilityPersistenceExtensions.InvisibleTabsCloseTag);
				setTabPageVisibilityAndAssertXml(page2, true, TabVisibilityPersistenceExtensions.InvisibleTabsOpenTag + ";page1;" + TabVisibilityPersistenceExtensions.InvisibleTabsCloseTag);
				setTabPageVisibilityAndAssertXml(page1, true, TabVisibilityPersistenceExtensions.InvisibleTabsOpenTag + TabVisibilityPersistenceExtensions.InvisibleTabsCloseTag);
				setTabPageVisibilityAndAssertXml(page1, false, TabVisibilityPersistenceExtensions.InvisibleTabsOpenTag + ";page1;" + TabVisibilityPersistenceExtensions.InvisibleTabsCloseTag);
			}
		}

		public void TestGetTabPageIdentifier()
		{
			using (var page = new ZTabPage())
			{
				AssertEquals("", TabVisibilityPersistenceExtensions.GetTabPageIdentifier(page));

				page.Text = "abcdef";
				AssertEquals("abcdef", TabVisibilityPersistenceExtensions.GetTabPageIdentifier(page));

				page.Name = "qwerty";
				AssertEquals("qwerty", TabVisibilityPersistenceExtensions.GetTabPageIdentifier(page));
			}
		}
	}
}
