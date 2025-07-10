using System.Windows.Forms;
using Enterprise.ResourceStrings.Business;
using Enterprise.ResourceStrings.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.GUI.CustomizableData.Testing
{
	[TestedType(typeof(CustomizableDataTranslationForm))]
	sealed class CustomizableDataTranslationFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new CustomizableDataTranslationForm(new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("one"), null));
		}

		public void TestFormBehaviour()
		{
			using (var form = new CustomizableDataTranslationForm(new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("two"), null)))
			{
				form.Show();

				AssertEquals("Translations: Customizable Data Test", form.Text);

				var currentTopGrid = (CustomizableDataTranslationEntry)form.translationsOfCurrentValueGrid.ListManager.GetCurrent();
				AssertEquals("two", currentTopGrid.English);
				foreach (CustomizableDataTranslationEntry entry in form.translationsOfCurrentValueGrid.ListManager.List)
				{
					AssertEquals("two", entry.English);
				}
				var currentBottomGrid = (CustomizableDataTranslationEntry)form.allValuesGrid.ListManager.GetCurrent();
				AssertEquals("two", currentBottomGrid.English);
				AssertNotEquals("Precondition: first language should not be ZH-CN", Core.SharedConstants.Languages.ChineseSimplified, currentBottomGrid.Language);
				AssertEquals(currentTopGrid.Language, currentBottomGrid.Language);
				AssertEquals(currentTopGrid.Language, new CodeDescriptionPairList(OLookUpEditType.Language).GetCodeFromDescription(form.allValuesGrid.Columns[CustomizableDataTranslationEntry.Schema.Translation].ColumnStyle.HeaderText));

				for (int i = 0; i < form.translationsOfCurrentValueGrid.ListManager.List.Count; i++)
				{
					if (((CustomizableDataTranslationEntry)form.translationsOfCurrentValueGrid.ListManager.List[i]).Language == Core.SharedConstants.Languages.ChineseSimplified)
					{
						form.translationsOfCurrentValueGrid.ListManager.Position = i;
						break;
					}
				}

				currentTopGrid = (CustomizableDataTranslationEntry)form.translationsOfCurrentValueGrid.ListManager.GetCurrent();
				AssertEquals("two", currentTopGrid.English);
				AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, currentTopGrid.Language);
				AssertEquals(testHelper.ChineseNumbers[2], currentTopGrid.Translation);
				currentBottomGrid = (CustomizableDataTranslationEntry)form.allValuesGrid.ListManager.GetCurrent();
				AssertEquals("two", currentBottomGrid.English);
				AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, currentBottomGrid.Language);
				AssertEquals(testHelper.ChineseNumbers[2], currentBottomGrid.Translation);
				AssertEquals("Chinese - Simplified", form.allValuesGrid.Columns[CustomizableDataTranslationEntry.Schema.Translation].ColumnStyle.HeaderText);
				foreach (CustomizableDataTranslationEntry entry in form.allValuesGrid.ListManager.List)
				{
					AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, entry.Language);
				}

				for (int i = 0; i < form.allValuesGrid.ListManager.List.Count; i++)
				{
					if (((CustomizableDataTranslationEntry)form.allValuesGrid.ListManager.List[i]).English == "seven")
					{
						form.allValuesGrid.ListManager.Position = i;
						break;
					}
				}

				currentTopGrid = (CustomizableDataTranslationEntry)form.translationsOfCurrentValueGrid.ListManager.GetCurrent();
				AssertEquals("seven", currentTopGrid.English);
				AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, currentTopGrid.Language);
				AssertEquals(testHelper.ChineseNumbers[7], currentTopGrid.Translation);
				currentBottomGrid = (CustomizableDataTranslationEntry)form.allValuesGrid.ListManager.GetCurrent();
				AssertEquals("seven", currentBottomGrid.English);
				AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, currentBottomGrid.Language);
				AssertEquals(testHelper.ChineseNumbers[7], currentBottomGrid.Translation);

				AssertEquals("七", testHelper.CustomizableDataResourceStrings.GetMultilingualString(null, "seven").ToString(Core.SharedConstants.Languages.ChineseSimplified));
				AssertEquals(false, form.zPostingButtonsUserControl.SaveButton.Enabled);
				currentBottomGrid.Translation = "柒";
				AssertEquals(true, form.zPostingButtonsUserControl.SaveButton.Enabled);
				form.zPostingButtonsUserControl.SaveButton.PerformClick();
				AssertEquals("柒", testHelper.CustomizableDataResourceStrings.GetMultilingualString(null, "seven").ToString(Core.SharedConstants.Languages.ChineseSimplified));
				AssertEquals(false, form.zPostingButtonsUserControl.SaveButton.Enabled);
			}
		}

		public void TestDefaultLanguage()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			{
				using (var form = new CustomizableDataTranslationForm(new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("two"), null)))
				{
					form.Show();

					var currentTopGrid = (CustomizableDataTranslationEntry)form.translationsOfCurrentValueGrid.ListManager.GetCurrent();
					AssertEquals("two", currentTopGrid.English);
					AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, currentTopGrid.Language);
					AssertEquals(testHelper.ChineseNumbers[2], currentTopGrid.Translation);
					var currentBottomGrid = (CustomizableDataTranslationEntry)form.allValuesGrid.ListManager.GetCurrent();
					AssertEquals("two", currentBottomGrid.English);
					AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, currentBottomGrid.Language);
					AssertEquals(testHelper.ChineseNumbers[2], currentBottomGrid.Translation);
					AssertEquals("Chinese - Simplified", form.allValuesGrid.Columns[CustomizableDataTranslationEntry.Schema.Translation].ColumnStyle.HeaderText);
				}
			}
		}

		public void TestColumnWidths()
		{
			using (var form = new CustomizableDataTranslationForm(new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("two"), null)))
			{
				form.Show();
				var grid = form.translationsOfCurrentValueGrid;
				for (int w = form.MinimumSize.Width; w <= 1280; w += 5)
				{
					form.Width = w;
					Assert("Grid should stretch with the form", form.Width - grid.Width < 50);
					var hitTest = grid.HitTest(grid.Size.Width - 40, 5);
					AssertEquals("Translation column header should stretch to the right edge of the grid " + w, DataGrid.HitTestType.ColumnHeader, hitTest.Type);
					AssertEquals("Translation column header should stretch to the right edge of the grid " + w, CustomizableDataTranslationEntry.Schema.Translation, grid.Columns[hitTest.Column].ColumnName);
					hitTest = grid.HitTest(grid.TableStyles[0].RowHeaderWidth + 130, 5);
					AssertEquals("Source column header should anchor on the left edge of the grid " + w, DataGrid.HitTestType.ColumnHeader, hitTest.Type);
					AssertEquals("Source column header should anchor on the left edge of the grid " + w, CustomizableDataTranslationEntry.Schema.English, grid.Columns[hitTest.Column].ColumnName);
					AssertEquals("Source and translation columns should be the same width " + w, grid.Columns[CustomizableDataTranslationEntry.Schema.English].ColumnStyle.Width, grid.Columns[CustomizableDataTranslationEntry.Schema.Translation].ColumnStyle.Width);
					AssertEquals("grid.IsHorizontalScrollBarVisible " + w, false, grid.IsHorizontalScrollBarVisible);
				}

				grid = form.allValuesGrid;
				for (int w = form.MinimumSize.Width; w <= 1280; w += 5)
				{
					form.Width = w;
					Assert("Grid should stretch with the form", form.Width - grid.Width < 50);
					var hitTest = grid.HitTest(grid.Size.Width - 40, 5);
					AssertEquals("Translation column header should stretch to the right edge of the grid " + w, DataGrid.HitTestType.ColumnHeader, hitTest.Type);
					AssertEquals("Translation column header should stretch to the right edge of the grid " + w, CustomizableDataTranslationEntry.Schema.Translation, grid.Columns[hitTest.Column].ColumnName);
					hitTest = grid.HitTest(grid.TableStyles[0].RowHeaderWidth + 130, 5);
					AssertEquals("Source column header should anchor on the left edge of the grid " + w, DataGrid.HitTestType.ColumnHeader, hitTest.Type);
					AssertEquals("Source column header should anchor on the left edge of the grid " + w, CustomizableDataTranslationEntry.Schema.English, grid.Columns[hitTest.Column].ColumnName);
					AssertEquals("Source and translation columns should be the same width " + w, grid.Columns[CustomizableDataTranslationEntry.Schema.English].ColumnStyle.Width, grid.Columns[CustomizableDataTranslationEntry.Schema.Translation].ColumnStyle.Width);
					AssertEquals("grid.IsHorizontalScrollBarVisible " + w, false, grid.IsHorizontalScrollBarVisible);
				}
			}
		}

		protected override void SetUp()
		{
			testHelper = new CustomizableDataTestHelper();
			base.SetUp();
		}

		protected override void TearDown()
		{
			testHelper.Dispose();
			base.TearDown();
		}

		CustomizableDataTestHelper testHelper;
	}
}
