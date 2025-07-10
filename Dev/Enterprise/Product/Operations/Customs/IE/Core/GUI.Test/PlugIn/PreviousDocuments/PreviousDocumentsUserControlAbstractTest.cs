using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Customs.IE.GUI
{
	[TestsSubclassesOf(typeof(PreviousDocumentsUserControl))]
	abstract class PreviousDocumentsUserControlAbstractTest : TestCaseWithFactory
	{
		protected abstract IEnumerable<(string ColumnName, Type ExpectedColumnType, CharacterCasing ExpectedCharacterCasing, int ExpectedWidth, string ExpectedCaption)> PreviousDocumentsGridCheckList { get; }

		public void TestPreviousDocumentsGridColumnDetails()
		{
			using (var control = (PreviousDocumentsUserControl)Activator.CreateInstance(TestedTypeHelper.GetTestedType(GetType())))
			{
				var grid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");

				CombineAssertions(() =>
				{
					foreach (var checkItem in PreviousDocumentsGridCheckList)
					{
						var columnName = checkItem.ColumnName;
						var columnStyle = grid.GetColumnStyle(columnName);
						AssertNotNull("Should be able to find a ColumnStyle " + columnName, columnStyle);

						if (columnStyle != null)
						{
							AssertType("Type", checkItem.ExpectedColumnType, columnStyle);
							AssertEquals("CharacterCasing", checkItem.ExpectedCharacterCasing, columnStyle.CharacterCasing);
							AssertEquals("Width", checkItem.ExpectedWidth, columnStyle.Width);
							if (!string.IsNullOrEmpty(checkItem.ExpectedCaption))
							{
								AssertEquals("Caption", checkItem.ExpectedCaption, columnStyle.CaptionResourceString.Caption);
							}
						}
					}

					var expectedColumnCount = PreviousDocumentsGridCheckList.ToList().Count;
					AssertEquals("PreviousDocumentsGrid should have " + expectedColumnCount + " ColumnStyles.", expectedColumnCount, grid.ColumnStyles.Count);
				});
			}
		}
	}
}
