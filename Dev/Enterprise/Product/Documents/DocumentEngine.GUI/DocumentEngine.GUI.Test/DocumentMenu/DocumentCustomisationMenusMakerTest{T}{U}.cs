using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.BuildTools.Testing;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.GUI.SDF;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.Testing
{
	abstract class DocumentCustomisationMenusMakerTest<T, U> : TestCaseWithFactory
			where T : class
			where U : DocumentCustomisationMenusMaker<T>
	{
		public void TestEnableCustomizedDocumentElementsMenuItem()
		{
			var documentSupportable = new MockDocumentSupportable();
			var parent = DocumentMenuCustomisation.New(documentSupportable, null);

			using (var form = new TestForm())
			{
				var maker = GetNewDocumentCustomisationMenusMaker(form, parent);

				maker.Make(GetMenuItems(form));
				AssertContains("Suspend DocBuilder Customizations should not be checked.", "Suspend DocBuilder Customizations (False)", GetMenuItemsString(GetMenuItems(form)));
				AssertContains("Suspend Template Caching should not be checked.", "Suspend Template Caching (False)", GetMenuItemsString(GetMenuItems(form)));
				GetMenuItems(form).Clear();

				using (EnableDebugOnlyMenuItemsForTesting())
				{
					TemplateGenerator.IgnoreUserCustomizedTemplatesAndConfigurations = true;
					maker.Make(GetMenuItems(form));
					AssertContains("Suspend DocBuilder Customizations should be checked.", "Suspend DocBuilder Customizations (True)", GetMenuItemsString(GetMenuItems(form)));

					GetMenuItems(form).Clear();
					TemplateCache.SuspendTemplateCache = true;
					maker.Make(GetMenuItems(form));
					AssertContains("Suspend Template Caching should be checked.", "Suspend Template Caching (True)", GetMenuItemsString(GetMenuItems(form)));
				}
			}
		}

		public void TestEnableCustomizedDocumentElementsMenuItemOutsideDocumentMenuCustomisation()
		{
			var documentSupportable = new MockDocumentSupportable() as IDocumentSupportable;

			using (var form = new TestForm())
			{
				var maker = GetNewDocumentCustomisationMenusMaker(form, documentSupportable);

				maker.Make(GetMenuItems(form));
				AssertNotContains("Suspend DocBuilder Customizations should not be in the menu.", "Suspend DocBuilder Customizations", GetMenuItemsString(GetMenuItems(form)));
				AssertNotContains("Suspend Template Caching should not be in the menu.", "Suspend Template Caching", GetMenuItemsString(GetMenuItems(form)));
				GetMenuItems(form).Clear();

				using (EnableDebugOnlyMenuItemsForTesting())
				{
					TemplateCache.SuspendTemplateCache = true;

					maker.Make(GetMenuItems(form));
					AssertNotContains("Suspend DocBuilder Customizations should not be in the menu.", "Suspend DocBuilder Customizations", GetMenuItemsString(GetMenuItems(form)));
					AssertNotContains("Suspend Template Caching should not be in the menu.", "Suspend Template Caching", GetMenuItemsString(GetMenuItems(form)));
				}
			}
		}

		[RequiresSTA]
		public void TestDebugOnlyMenuItemsAvailable()
		{
			using (var form = new TestForm())
			using (EnableDebugOnlyMenuItemsForTesting())
			{
				var documentSupportable = new MockDocumentSupportable();
				var parent = DocumentMenuCustomisation.New(documentSupportable, null);
				var maker = GetNewDocumentCustomisationMenusMaker(form, parent);

				maker.Make(GetMenuItems(form));

				var expectedMenuItems = new string[]
				{
					"Customize",
					"Mark All Documents Editable",
					"Mark Documents Editable For Client",
					"Regenerate all clients Documents.xml",
					"Save Documents Config Changes",
					"Undo All Changes"
				};

				var currentMenuItems = GetMenuItems(form).OfType<T>().Select(GetText).ToArray();

				Assert("All of the expected menu items should be in the menu", !expectedMenuItems.Except(currentMenuItems).Any());
				Assert("System defined fields should not be in the menu", !currentMenuItems.Contains("System Defined Fields"));
			}
		}

		[RequiresSTA]
		public void TestCustomizeAndDebugItemsAreNotInMenu()
		{
			using (var form = new TestForm())
			using (EnableDebugOnlyMenuItemsForTesting())
			{
				var documentSupportable = new MockDocumentSupportable();
				var parent = DocumentMenuCustomisation.New(documentSupportable, null);
				var superParent = DocumentMenuCustomisation.New(parent, null);
				var maker = GetNewDocumentCustomisationMenusMaker(form, superParent);

				maker.Make(GetMenuItems(form));

				var unexpectedMenuItems = new string[]
				{
					"Customize",
					"Mark All Documents Editable",
					"Mark Documents Editable For Client",
					"Regenerate all clients Documents.xml",
					"Save Documents Config Changes",
					"Undo All Changes",
					"System Defined Fields"
				};

				var currentMenuItems = GetMenuItems(form).OfType<T>().Select(GetText).ToArray();

				AssertEquals("An unexpected menu item was found in the menu", currentMenuItems.Any(), currentMenuItems.Except(unexpectedMenuItems).Any());
			}
		}

		public void TestDocumentAndSystemDefinedFieldMenus()
		{
			using (var form = new TestForm())
			using (EnableDebugOnlyMenuItemsForTesting())
			{
				form.Show();
				var documentSupportable = new MockDocumentSupportable();
				var maker = GetNewDocumentCustomisationMenusMaker(form, documentSupportable);

				maker.Make(GetMenuItems(form));
				AssertMenus(GetMenuItems(form), true, false);

				PerformClick((T)GetMenuItems(form)[2]);
				using (var lastShownForm = (DocumentFieldHelpForm)ZFormModaliser.LastFormShownForTest)
				{
					var fields = ((DocumentFieldDefinitionFormBizo)lastShownForm.BusinessEntity).Fields;
					AssertEquals("fields.Count", 2, fields.Count);
					AssertEquals("fields[0].Name", "Apple", fields[0].FieldName);
					AssertEquals("fields[1].Name", "Orange", fields[1].FieldName);
				}

				PerformClick((T)GetMenuItems(form)[9]);
				using (var lastShownForm = ZFormModaliser.LastFormShownForTest)
				{
					AssertEquals("StmSystemDefinedFieldForm should be shown.", typeof(StmSystemDefinedFieldForm), lastShownForm.GetType());
				}

				documentSupportable.ReturnEmptyWrapperTypes = true;
				GetMenuItems(form).Clear();
				maker.Make(GetMenuItems(form));
				AssertMenus(GetMenuItems(form), false, false);
			}
		}

		public void TestSetupJobSpecificRecipient()
		{
			using (var form = new TestForm())
			using (EnableDebugOnlyMenuItemsForTesting())
			{
				form.Show();
				var documentSupportable = new MocDocumentSupportableWithJobRecipient();
				var maker = GetNewDocumentCustomisationMenusMaker(form, documentSupportable);

				maker.Make(GetMenuItems(form));
				AssertMenus(GetMenuItems(form), true, true);
			}
		}
		#region Implementation

		protected abstract IList GetMenuItems(Form form);
		protected abstract U GetNewDocumentCustomisationMenusMaker(Form form, IDocumentSupportable documentSupportable);
		protected abstract void PerformClick(T menuItem);

		string GetMenuItemsString(IList menuItems)
		{
			var result = new StringBuilder();

			foreach (T menuItem in menuItems)
			{
				result.AppendLine(string.Format("{0} ({1})", GetMenuAsString(menuItem)));
			}

			return result.ToString();
		}

		protected abstract string[] GetMenuAsString(T menuItem);
		protected abstract string GetText(T menuItem);

		void AssertMenus(IList menus, bool hasDocumentFieldMenu, bool hasSetupJobSpecificRecipientsMenu)
		{
			var expectedMenus = new List<string>();

			expectedMenus.Add(" ");
			expectedMenus.Add("Customize");
			if (hasDocumentFieldMenu)
			{
				expectedMenus.Add("Document Field Definitions");
			}
			expectedMenus.Add(" ");
			if (hasSetupJobSpecificRecipientsMenu)
			{
				expectedMenus.Add("Setup Job Specific Recipients");
				expectedMenus.Add(" ");
			}
			expectedMenus.Add("Mark All Documents Editable");
			expectedMenus.Add("Mark Documents Editable For Client");
			expectedMenus.Add("Save Documents Config Changes");
			expectedMenus.Add("Undo All Changes");
			expectedMenus.Add("Regenerate all clients Documents.xml");
			expectedMenus.Add("System Defined Fields");

			AssertEquals("MenuItems.Count", expectedMenus.Count, menus.Count);
			for (int i = 0; i < expectedMenus.Count; i++)
			{
				AssertEquals("MenuItems[" + i + "].Text", expectedMenus[i], GetText((T)menus[i]));
			}
		}

		protected class TestForm : ZForm
		{
			public TestForm()
			{
				Menu = new MainMenu();
				MainMenuStrip = new MenuStrip();
				Controls.Add(MainMenuStrip);
			}

			public TestForm(BusinessObject bizObj)
				: base(bizObj)
			{
				Menu = new MainMenu();
				MainMenuStrip = new MenuStrip();
				Controls.Add(MainMenuStrip);
			}
		}

		#region MockDocumentSupportable

		protected class MockDocumentSupportable : NonPersistentBusinessObject, IDocumentSupportable
		{
			bool returnEmptyWrapperTypes;

			public MockDocumentSupportable()
			{
			}

			public bool ReturnEmptyWrapperTypes
			{
				get { return returnEmptyWrapperTypes; }
				set { returnEmptyWrapperTypes = value; }
			}

			#region MockDocumentSupporter

			class MockDocumentSupporter : DocumentSupporter
			{
				public MockDocumentSupporter(MockDocumentSupportable parentBusinessObject)
					: base(parentBusinessObject)
				{
				}

				public override BusinessContext BusinessContext
				{
					get { return new BusinessContext(); }
				}

				protected new MockDocumentSupportable BusinessObject
				{
					get { return (MockDocumentSupportable)base.BusinessObject; }
				}

				public override ISecurityCheckpoint CustomisationSecurityCheckpoint
				{
					get { return null; }
				}

				protected override DocumentWrapper[] GetDocumentWrappersInternal(Enterprise.Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
				{
					return null;
				}

				protected override Core.Constants.DataContext[] GetSupportedDataContexts()
				{
					return null;
				}

				public override Type[] GetWrapperTypesForDocumentFieldMenu
				{
					get { return BusinessObject.ReturnEmptyWrapperTypes ? null : new Type[] { typeof(MockDocumentWrapper) }; }
				}
			}

			#endregion

			#region MockDocumentWrapper

			class MockDocumentWrapper
			{
				public MockDocumentWrapper()
				{
				}

				[DocumentFieldAttribute("All About Apples")]
				public ZString Apple
				{
					get { return "I like apples."; }
				}

				[DocumentFieldAttribute("Yum Oranges")]
				public ZString Orange
				{
					get { return "Orange"; }
				}
			}

			#endregion

			#region IDocumentSupportable Members

			DocumentSupporter IDocumentSupportable.DocumentSupporter
			{
				get { return new MockDocumentSupporter(this); }
			}

			#endregion
		}

		class MocDocumentSupportableWithJobRecipient : MockDocumentSupportable, ISupportJobDocumentRecipient
		{
			public IEnumerable<(MultilingualString organisationType, IOrgHeader orgHeader)> SuggestedOrganisations => new List<(MultilingualString organisationType, IOrgHeader orgHeader)>();
		}

		#endregion

		protected IDisposable EnableDebugOnlyMenuItemsForTesting()
		{
			DocumentCustomisationMenusMaker<T>.ShouldAddDebugOnlyMenuItemsForTesting = true;
			return new DisposableAction(() => DocumentCustomisationMenusMaker<T>.ShouldAddDebugOnlyMenuItemsForTesting = false);
		}

		protected override void SetUp()
		{
			base.SetUp();

			MockSourceControl.Setup();
		}

		protected override void TearDown()
		{
			base.TearDown();

			MockSourceControl.TearDown();
			TemplateGenerator.IgnoreUserCustomizedTemplatesAndConfigurations = false;
		}

		#endregion
	}
}
