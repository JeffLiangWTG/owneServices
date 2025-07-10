using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestsSubclassesOf(typeof(IPanelLayoutProvider))]
	public abstract class LayoutsAbstractTest : TestCaseWithFactory
	{
		public void TestControlBagCount()
		{
			AssertEquals(ControlBagCount, LayoutForTesting.ControlBags.Count);
		}

		public void TestIncludedControls()
		{
			var layout = LayoutForTesting;
			var expectedColumns = IncludedControlsPerColumn.Count();
			var actualColumns = layout.Columns.Count;
			if (expectedColumns != actualColumns)
			{
				AssertEquals("Columns Count", expectedColumns, actualColumns);
			}
			else
			{
				CombineAssertions(() =>
				{
					AssertionCount++;
					var index = 0;
					foreach (var controlsInColumn in IncludedControlsPerColumn)
					{
						var actualControlsInColumns = layout.Columns[index].Rows.SelectMany(x => x.Parts).OfType<ControlReference>();
						AssertEquals($"Number of controls differ in column #{index + 1}", controlsInColumn.Count(), actualControlsInColumns.Count());

						var controlRows = layout.Columns[index].Rows;
						foreach (var control in controlsInColumn)
						{
							int captionWidth = ((int)CommonLayoutBuilder.CaptionWidth);
							Assert($"Control {control.controlReference.Name} in column #{index + 1}", actualControlsInColumns.Contains(control.controlReference));

							if (control.controlWidth != ControlWidthClass.CustomWidth)
							{
								AssertControlWidth(controlRows.First(obj => obj.Parts.Contains(control.controlReference)).Parts, control.controlWidth, control.controlReference.Name);
							}
						}
						index++;
					}
				});
			}

			void AssertControlWidth(IEnumerable<IPanelLayoutPart> parts, ControlWidthClass widthClass, string controlName)
			{
				int expectedWidth;
				int captionWidth = ((int)CommonLayoutBuilder.CaptionWidth);

				if (widthClass == ControlWidthClass.LongControl)
				{
					expectedWidth = ColumnLayoutBuilder<BusinessObject, IControlBag>.LongControlWidth
						+ captionWidth + ColumnLayoutBuilder<BusinessObject, IControlBag>.LongWidth + ColumnLayoutBuilder<BusinessObject, IControlBag>.ColumnWidthOffset;
				}
				else if (widthClass == ControlWidthClass.LongNoCaption)
				{
					expectedWidth = captionWidth
						+ ColumnLayoutBuilder<BusinessObject, IControlBag>.LongWidth
						+ captionWidth + ColumnLayoutBuilder<BusinessObject, IControlBag>.LongWidth + ColumnLayoutBuilder<BusinessObject, IControlBag>.ColumnWidthOffset;
				}
				else if (widthClass != ControlWidthClass.Auto)
				{
					var widthRuler = widthClass == ControlWidthClass.Long ? (captionWidth + ColumnLayoutBuilder<BusinessObject, IControlBag>.LongWidth) : (captionWidth + ColumnLayoutBuilder<BusinessObject, IControlBag>.MediumWidth);
					var columnWidthRuler = CommonLayoutBuilder.NarrowColumnForMediumControls && widthClass == ControlWidthClass.Medium
						? captionWidth + ColumnLayoutBuilder<BusinessObject, IControlBag>.MediumWidth + ColumnLayoutBuilder<BusinessObject, IControlBag>.ColumnWidthOffset
						: captionWidth + ColumnLayoutBuilder<BusinessObject, IControlBag>.LongWidth + ColumnLayoutBuilder<BusinessObject, IControlBag>.ColumnWidthOffset;
					expectedWidth = captionWidth + widthRuler + columnWidthRuler;
				}
				else
				{
					expectedWidth = captionWidth
						+ captionWidth + ColumnLayoutBuilder<BusinessObject, IControlBag>.LongWidth + ColumnLayoutBuilder<BusinessObject, IControlBag>.ColumnWidthOffset;
				}

				int actucalWidth = 0;
				foreach (var part in parts)
				{
					int partWidth = 0;
					if (part.Name.Contains("Ruler") && part.Name.Contains("-"))
					{
						int.TryParse(part.Name.Split('-')[1], out partWidth);
						actucalWidth += partWidth;
					}
				}

				AssertEquals($"ControlWidthClass of {controlName}", expectedWidth, actucalWidth);
			}
		}

		public void TestIncludedControlsAreSorted()
		{
			if (!ShouldAssertThatIncludedControlsAreSorted)
			{
				AssertNotNull("Skipping test due to condition");
				return;
			}
			var expectedColumns = IncludedControlsPerColumn.ToList();
			var actualColumns = LayoutForTesting.Columns;
			AssertEquals("[PRE-CONDITION] Column count", expectedColumns.Count, actualColumns.Count);
			CombineAssertions(() =>
			{
				var zippedColumns = expectedColumns.Zip(actualColumns, (expectedColumn, actualColumn) => (
					expectedColumn.Select(x => x.controlReference),
					actualColumn.Rows.SelectMany(x => x.Parts).OfType<ControlReference>()
				));
				foreach (var (expectedColumn, actualColumn) in zippedColumns)
				{
					AssertSequencesEqual(expectedColumn, actualColumn);
				}
			});
		}

		protected virtual bool ShouldAssertThatIncludedControlsAreSorted => true;

		public void TestGridUserControlType()
		{
			if (PanelLayoutProvider is IPanelLayoutWithGridProvider panelLayoutWithGridProvider)
			{
				AssertEquals(ExpectedGridUserControlType, panelLayoutWithGridProvider.GridUserControlType);
			}
			else
			{
				Assert(true);
			}
		}

		protected abstract IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn { get; }

		protected virtual Type ExpectedGridUserControlType { get; }

		protected abstract int ControlBagCount { get; }

		protected abstract ICommonLayoutBuilder CommonLayoutBuilder { get; }

		protected virtual IPanelLayoutProvider GetNewPanelLayoutProvider() =>
			(IPanelLayoutProvider)Activator.CreateInstance(TestedTypeHelper.GetTestedType(GetType()));

		IPanelLayoutProvider PanelLayoutProvider => panelLayoutProvider ?? (panelLayoutProvider = GetNewPanelLayoutProvider());

		IPanelLayoutProvider panelLayoutProvider;

		protected PanelLayout LayoutForTesting => layoutForTesting ?? (layoutForTesting = PanelLayoutProvider.Layout);
		PanelLayout layoutForTesting;
	}
}
