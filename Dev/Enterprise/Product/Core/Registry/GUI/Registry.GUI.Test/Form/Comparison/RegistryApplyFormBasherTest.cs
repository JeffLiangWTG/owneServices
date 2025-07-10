using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.OverrideLevels;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Registry.GUI.RegistryItemEditorFactory;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(RegistryApplyForm))]
	sealed class RegistryApplyFormBasherTest : ZFormBasherTest
	{
		public void TestDisplayIsUpdatedWhenValuesAreApplied()
		{
			const int overrideValue = 1;

			var item = (IRegistryItem)new IntRegistryItem("ICAN", (NoResString)"Catagory", (NoResString)"Item", (NoResString)"", RegistryStorageFlags.All);

			var baseLevel = new DummyOverrideLevel();
			var overrideLevel = new DummyOverrideLevel();

			overrideLevel.SetValueOf(item, overrideValue);

			var diffBizo = new RegistryComparisonBusinessObject(new[] { item }, baseLevel, overrideLevel);
			using (var form = new RegistryApplyForm(diffBizo))
			{
				form.Show();

				var nodeOfItem = form.treeView.Nodes[0].Nodes[0];
				AssertEquals("PRE: Should be node for item", item, nodeOfItem.Tag);

				using (var editorFactory = new EditorFactoryThatCounts())
				{
					form.treeView.PerformSelect(nodeOfItem);

					var initialUpdateCount = editorFactory.newEditorCount;
					form.btnApplyOverrides.PerformClick();

					AssertEquals("Item should now have the override value", overrideValue, baseLevel.GetValueOf(item));
					Assert("Both panels should have been updated", editorFactory.newEditorCount >= (initialUpdateCount + 2));
				}
			}
		}

		public void TestApplyButton()
		{
			var item = (IRegistryItem)new IntRegistryItem("MAHCODE", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.All);

			var baseLevel = new SystemOverrideLevel();
			var overrideLevel = new DummyOverrideLevel(baseLevel);

			overrideLevel.SetValueOf(item, 23);

			using (var form = new RegistryApplyForm(new RegistryComparisonBusinessObject(new[] { item }, baseLevel, overrideLevel)))
			{
				form.Show();

				AssertNotEquals("We have not yet clicked apply, the items should have different values", baseLevel.GetValueOf(item), overrideLevel.GetValueOf(item));

				form.btnApplyOverrides.PerformClick();

				AssertEquals("We have clicked apply, so the base should have the overrides value", 23, baseLevel.GetValueOf(item));

				var logs = Factory.Load<StmData>(baseLevel.GetPkOfEntry(item)).Logs;
				AssertEquals("Should have logged that the value was changed", Events.EditedARecordCode, logs.MostRecentLog.Event.SE_Code);
			}
		}

		[RequiresSTA]
		public void TestApplyButton_MultilingualStringRegistryItem()
		{
			var item = (IRegistryItem)new MultilingualStringRegistryItem("MAHCODE", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.All);

			var baseLevel = new SystemOverrideLevel();

			var glbCompany = Factory.New<GlbCompany>();
			var overrideLevel = new CompanyOverrideLevel(glbCompany);
			overrideLevel.SetValueOf(item, "Text");

			using (var form = new RegistryApplyForm(new RegistryComparisonBusinessObject(new[] { item }, baseLevel, overrideLevel)))
			{
				form.Show();
				AssertNotEquals("We have not yet clicked apply, the items should have different values", baseLevel.GetValueOf(item), overrideLevel.GetValueOf(item));

				form.btnApplyOverrides.PerformClick();
				AssertEquals("We have clicked apply, so the base should have the overrides value", "Text", baseLevel.GetValueOf(item));
			}
		}

		public void TestApplyButtonWhenYouCantSetTheValueDoesntThrowException()
		{
			var item = new StringRegistryItem("MAHCODE", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.All, "DefaultValue");

			var baseLevel = new DummyOverrideLevel(new[] { item });
			var overrideLevel = new DummyOverrideLevel(baseLevel);

			using (var form = new RegistryApplyForm(new RegistryComparisonBusinessObject(new[] { item }, baseLevel, overrideLevel)))
			{
				form.Show();

				AssertNoExceptionThrown(form.btnApplyOverrides.PerformClick);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new RegistryApplyForm(new RegistryComparisonBusinessObject(Array.Empty<IRegistryItem>(), new DefaultOverrideLevel(), new SystemOverrideLevel()));
		}

		#endregion

		class EditorFactoryThatCounts : IDisposable
		{
			public int newEditorCount;

			RegistryItemEditor NewEditor_ForTest(IRegistryItem item, FallbackLevel fallback, IRegistryDataType dataType, IRegistryEditorInfo editorInfo, BusinessObjectFactory factory)
			{
				newEditorCount++;

				return GetNewEditor(item, fallback, dataType, editorInfo, factory);
			}

			public EditorFactoryThatCounts()
			{
				OverridenNewEditorDelegate = NewEditor_ForTest;
			}

			public void Dispose()
			{
				OverridenNewEditorDelegate = null;
			}
		}

		public void TestColors()
		{
			var itemThatCanBeSet = (IRegistryItem)new IntRegistryItem("ICAN", (NoResString)"My/Catagory", (NoResString)"", (NoResString)"", RegistryStorageFlags.All);
			var itemThatCantBeSet = (IRegistryItem)new IntRegistryItem("ICANT", (NoResString)"My/Other", (NoResString)"", (NoResString)"", RegistryStorageFlags.All);

			var baseLevel = new DummyOverrideLevel();

			var overrideLevel = new DummyOverrideLevel(baseLevel);
			overrideLevel.SetValueOf(itemThatCanBeSet, 1);
			overrideLevel.SetValueOf(itemThatCantBeSet, 1);

			baseLevel.MarkAsCanBeSet(itemThatCantBeSet, false);

			var diffBizo = new RegistryComparisonBusinessObject(new[] { itemThatCanBeSet, itemThatCantBeSet }, baseLevel, overrideLevel);
			using (var form = new RegistryApplyForm(diffBizo))
			{
				var root = form.treeView.Nodes["My"];

				var canBeSetParent = root.Nodes["Catagory"];
				var canBeSet = canBeSetParent.FirstNode;

				var cantBeSetParent = root.Nodes["Other"];
				var cantBeSet = cantBeSetParent.FirstNode;

				var colorOfItemsThatCanBeSet = Color.Empty;
				var colorOfItemsThatCantBeSet = Color.Orange;

				CombineAssertions(() =>
				{
					AssertEquals("canBeSet", colorOfItemsThatCanBeSet, canBeSet.ForeColor);
					AssertEquals("canBeSetParent", colorOfItemsThatCanBeSet, canBeSetParent.ForeColor);
					AssertEquals("root", colorOfItemsThatCanBeSet, root.ForeColor);

					AssertEquals("cantBeSet", colorOfItemsThatCantBeSet, cantBeSet.ForeColor);
					AssertEquals("cantBeSetParent", colorOfItemsThatCantBeSet, cantBeSetParent.ForeColor);
				});
			}
		}

		[RequiresSTA]
		public void TestTellUserWhySomeItemsAreColored()
		{
			var cantBeSet = (IRegistryItem)new IntRegistryItem("ICANT", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.All);
			var canBeSet = (IRegistryItem)new IntRegistryItem("ICANT", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.All);

			var baseLevel = new DummyOverrideLevel();
			var overrideLevel = new DummyOverrideLevel();

			overrideLevel.SetValueOf(canBeSet, 1);
			overrideLevel.SetValueOf(cantBeSet, 1);

			baseLevel.MarkAsCanBeSet(cantBeSet, false);

			var bizo = new RegistryComparisonBusinessObject(new[] { canBeSet, cantBeSet }, baseLevel, overrideLevel);
			using (var form = new RegistryApplyForm(bizo))
			{
				form.Show();

				var message = (UnitTestUserNotification)Globals.Message;
				AssertEquals("Why are some items colored?", message.LastMessage.Caption);

				var color = Color.Orange;
				AssertContains(color.Name.ToLowerInvariant(), message.LastMessage.Text.ToLowerInvariant());
			}
		}
	}
}
