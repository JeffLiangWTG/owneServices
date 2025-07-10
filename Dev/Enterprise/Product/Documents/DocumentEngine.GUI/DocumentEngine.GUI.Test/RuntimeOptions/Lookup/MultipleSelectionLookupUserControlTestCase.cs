using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	sealed class MultipleSelectionLookupUserControlTestCase : TestCaseWithFactory
	{
		[GuiTest]
		public void TestBindingWithOrgs()
		{
			OrgHeaderCollection bindToCollection = new OrgHeaderCollection(Factory);
			CollectionProvider provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, "organisation");

			MultipleSelectionLookup lookup = new MultipleSelectionLookup(Factory);
			lookup.SetCollectionProvider(provider);

			using (ZForm testForm = new ZForm())
			using (MultipleSelectionLookupUserControl userControl = new MultipleSelectionLookupUserControl())
			{
				testForm.Controls.Add(userControl);
				testForm.Show();
				userControl.SetFilter(lookup);
				userControl.Show();
				userControl.Grid.InnerGrid.SetDataBinding(lookup, userControl.Grid.BindToGridList);

				AssertEquals("Collection should be readonly", true, userControl.Grid.InnerGrid.ReadOnly);
				AssertEquals("UserControl's grid should have 2 columns", 2, userControl.Grid.ColumnStyles.Count);
				AssertEquals("User control's first column should be bound to OH_Code", "OH_Code", ((ZTextBoxColumnStyleInfo)userControl.Grid.ColumnStyles[0]).ColumnName);
				AssertEquals("User control's second column should be bound to OH_FullName", "OH_FullName", ((ZTextBoxColumnStyleInfo)userControl.Grid.ColumnStyles[1]).ColumnName);
				AssertEquals("Grid's element count should be 0", 0, userControl.Grid.InnerGrid.ListManager.Count);

				provider.Collection.AddNew();
				AssertEquals("Grid's element count should now be 1", 1, userControl.Grid.InnerGrid.ListManager.Count);
			}
		}

		[GuiTest]
		public void TestBindingWithOrganisations()
		{
			CollectionProvider provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, "organisation");

			MultipleSelectionLookup lookup = new MultipleSelectionLookup(Factory);
			lookup.SetCollectionProvider(provider);

			using (ZForm testForm = new ZForm())
			using (MultipleSelectionLookupUserControl userControl = new MultipleSelectionLookupUserControl())
			{
				testForm.Controls.Add(userControl);
				testForm.Show();
				userControl.SetFilter(lookup);
				userControl.Show();
				userControl.Grid.InnerGrid.SetDataBinding(lookup, userControl.Grid.BindToGridList);

				AssertEquals("Collection should be readonly", true, userControl.Grid.InnerGrid.ReadOnly);
				AssertEquals("UserControl's grid should have 2 columns", 2, userControl.Grid.ColumnStyles.Count);
				AssertEquals("User control's first column shoudl be bound to OH_Code", "OH_Code", ((ZTextBoxColumnStyleInfo)userControl.Grid.ColumnStyles[0]).ColumnName);
				AssertEquals("User control's second column should be bound to OH_FullName", "OH_FullName", ((ZTextBoxColumnStyleInfo)userControl.Grid.ColumnStyles[1]).ColumnName);
				AssertEquals("Grid's element count should be 0", 0, userControl.Grid.InnerGrid.ListManager.Count);

				provider.Collection.AddNew();
				AssertEquals("Grid's element count should now be 1", 1, userControl.Grid.InnerGrid.ListManager.Count);
			}
		}

		public void TestExpectedFilterType()
		{
			using (MultipleSelectionLookupUserControl userControl = new MultipleSelectionLookupUserControl())
			{
				AssertEquals("Expected filter type should be a LookupField", typeof(MultipleSelectionLookup), userControl.ExpectedFilterType());
			}
		}

		public void TestSetFilterWithoutOptionalColumns()
		{
			using (var form = new ZForm())
			using (MultipleSelectionLookupUserControl userControl = new MultipleSelectionLookupUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory);
				OrgHeaderCollection orgFindboxCollection = new OrgHeaderCollection(Factory);

				MultipleSelectionLookup lookupField = new MultipleSelectionLookup(Factory);
				lookupField.DisplayName = "&ABC";

				CollectionProvider provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, "organisation");
				lookupField.SetCollectionProvider(provider);
				userControl.SetFilter(lookupField);

				AssertEquals("Label text should now set to '&&ABC' - ampersands escaped", "&&ABC", userControl.FieldLabel.Text);
				AssertEquals("BindToFindBoxList should be set", "BindToFindBoxList", userControl.Grid.BindToFindBoxList);
				AssertEquals("BindToGridList should be set", "BindToList", userControl.Grid.BindToGridList);
				Assert("BindToFindBoxList collection and BindToGridList collection should be different instances", userControl.Grid.BindToFindBoxList != userControl.Grid.BindToGridList);
				AssertEquals("ModuleID should be set to Orgs", ModuleIDs.Organisation, userControl.Grid.ModuleID);

				AssertEquals("Grid must be read only", true, userControl.Grid.InnerGrid.ReadOnly);
				AssertEquals("Attach button must be enabled", true, GetButton(userControl.Grid, "AttachButton").Enabled);
				AssertEquals("Detach button must be enabled", true, GetButton(userControl.Grid, "DetachButton").Enabled);
			}
		}

		public void TestSetFilterWithOptionalColumns()
		{
			using (var form = new ZForm())
			using (MultipleSelectionLookupUserControl userControl = new MultipleSelectionLookupUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				ColumnInfo columnInfo = new ColumnInfo("Caption");
				columnInfo.Properties.Add("ColumnName", "Voyage.JV_VoyageFlight");
				MultipleSelectionLookup lookupField = new MultipleSelectionLookup(Factory);
				lookupField.DisplayName = "&ABC";
				lookupField.Columns.Add(columnInfo);

				CollectionProvider provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, "sea voyage");

				lookupField.SetCollectionProvider(provider);
				userControl.SetFilter(lookupField);

				AssertEquals("Label text should now set to '&&ABC' - ampersands escaped", "&&ABC", userControl.FieldLabel.Text);
				AssertEquals("BindToFindBoxList should be set", "BindToFindBoxList", userControl.Grid.BindToFindBoxList);
				AssertEquals("BindToGridList should be set", "BindToList", userControl.Grid.BindToGridList);
				Assert("BindToFindBoxList collection and BindToGridList collection should be different instances", userControl.Grid.BindToFindBoxList != userControl.Grid.BindToGridList);
				AssertEquals("ModuleID should be set to Orgs", ModuleIDs.JobSeaVoyage, userControl.Grid.ModuleID);

				AssertEquals("Grid must be read only", true, userControl.Grid.InnerGrid.ReadOnly);
				AssertEquals("Attach button must be enabled", true, GetButton(userControl.Grid, "AttachButton").Enabled);
				AssertEquals("Detach button must be enabled", true, GetButton(userControl.Grid, "DetachButton").Enabled);
			}
		}

		[GuiTest]
		public void TestListChanged_SafeCopyValuesFrom()
		{
			OrgHeaderCollection bindToCollection = new OrgHeaderCollection(Factory);
			CollectionProvider provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, "organisation");

			MultipleSelectionLookup lookup = new MultipleSelectionLookup(Factory);
			lookup.SetCollectionProvider(provider);

			using (ZForm testForm = new ZForm())
			using (var userControl = new MultipleSelectionLookupUserControl())
			{
				testForm.Controls.Add(userControl);
				testForm.Show();

				var fieldInfo = typeof(BusinessObjectCollection).GetField("ListChanged", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
				var eventDelegate = fieldInfo.GetValue(lookup.BindToList) as Delegate;

				AssertNull("BindToList.ListChanged event is null.", eventDelegate);

				userControl.SetFilter(lookup);
				userControl.Show();

				eventDelegate = fieldInfo.GetValue(lookup.BindToList) as Delegate;
				AssertNotNull("BindToList.ListChanged event is not null.", eventDelegate);

				CollectionProvider provider1 = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, "organisation");
				var newLookup = new MultipleSelectionLookup(Factory);
				newLookup.SetCollectionProvider(provider1);

				lookup.SafeCopyValuesFrom(newLookup);
				eventDelegate = fieldInfo.GetValue(lookup.BindToList) as Delegate;
				AssertNotNull("BindToList.ListChanged event is not null after SafeCopyValuesFrom method.", eventDelegate);
			}
		}

		ZToolStripButton GetButton(Control control, string buttonName)
		{
			var toolStrip = (ZToolStrip)control.Controls.Find("toolStrip", true)[0];
			return toolStrip.Items.Cast<ZToolStripButton>().FirstOrDefault(x => x.Name == buttonName);
		}
	}
}
