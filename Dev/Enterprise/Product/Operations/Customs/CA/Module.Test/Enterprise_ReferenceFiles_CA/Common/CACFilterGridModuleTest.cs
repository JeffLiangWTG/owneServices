using System;
using System.Collections.Generic;
using CargoWise.Schema;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Customs.CA.Module.Testing
{
	abstract class CACFilterGridModuleTest : ZModuleBasherTest
	{
		public void TestGetNewFilterControl()
		{
			using (var testModule = GetNewTestModule())
			{
				var testControl = (ZFilterStripControl)testModule.EmbeddedControl;

				foreach (ZGridColumnInfo expectedColumn in ExpectedColumns)
				{
					var column = testControl.FilteredGrid.GetColumnStyle(expectedColumn.ColumnName);
					AssertNotNull(expectedColumn.ColumnName + " column", column);
					AssertEquals(expectedColumn.ColumnName + " caption", expectedColumn.Caption, column.Caption);
					AssertEquals(expectedColumn.ColumnName + " width", ControlDpiScalingHelper.ScaleToCurrentDpiX(expectedColumn.Width), column.Width);
				}
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var testModule = GetNewTestModule())
			{
				AssertEquals("Collection type", ExpectedGridCollectionType, testModule.GridCollection.GetType());
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var testModule = GetNewTestModule())
			{
				var filterBusinessObject = (CACFilterStripBusinessObject)((ZFilterStripControl)testModule.EmbeddedControl).FilterBusinessObject;

				foreach (KeyValuePair<string, SchemaStringColumn> pair in ExpectedFilters)
				{
					AssertNotNull(pair.Key + " filter", filterBusinessObject[pair.Key]);
				}
			}
		}

		protected override bool HasController() => false;

		protected override string CountryCode => Core.Constants.CountryCodes.Canada;

		protected abstract ZGridColumnInfo[] ExpectedColumns { get; }

		protected abstract Type ExpectedGridCollectionType { get; }

		protected abstract Dictionary<string, SchemaStringColumn> ExpectedFilters { get; }

		protected abstract ZFilterGridModule GetNewTestModule();

		protected override bool ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_View => true;
	}
}
