using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.TemporaryStorage.Module;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using CusTempStorageRegPremises = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegPremises;

namespace Enterprise.Customs.EU.Module.TemporaryStorage.Testing
{
	[TestedType(typeof(TempStoragePremisesFilterStripControl))]
	sealed class TempStoragePremisesFilterStripControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestGridColumnNames()
		{
			using (var form = new ZForm())
			using (var filterControl = GetFilterControl())
			{
				form.Controls.Add(filterControl);
				form.Show();
				var grid = filterControl.Grid;

				CombineAssertions(() =>
				{
					foreach (var (columnName, columnCaption, _) in OrderedColumnDetails)
					{
						AssertEquals(columnName, columnCaption, grid.GetColumnCaption(columnName));
					}
				});
			}
		}

		[RequiresSTA]
		public void TestGridDefaultColumnOrder()
		{
			using (var form = new ZForm())
			using (var filterControl = GetFilterControl())
			{
				form.Controls.Add(filterControl);
				form.Show();
				var gridColumns = filterControl.Grid.DefaultColumns;

				AssertSequencesEqual(OrderedColumnDetails.Select(x => x.ColumnName), gridColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));
			}
		}

		[RequiresSTA]
		public void TestGridColumnWidths()
		{
			using (var filterControl = GetFilterControl())
			{
				var grid = filterControl.Grid;
				CombineAssertions(() =>
				{
					foreach (var (columnName, _, columnWidth) in OrderedColumnDetails)
					{
						AssertEquals(columnName, columnWidth, grid.GetColumnStyle(columnName).Width);
					}
				});
			}
		}

		TempStoragePremisesFilterStripControl GetFilterControl()
		{
			return new TempStoragePremisesFilterStripControl(new CusTempStorageRegPremisesCollection(Factory), new TempStoragePremisesFilterBusinessObject());
		}

		(string ColumnName, string ColumnCaption, int ColumnWidth)[] OrderedColumnDetails => new[]
		{
			(CusTempStorageRegPremises.Schema.SRP_Code, "Code", 150),
			(CusTempStorageRegPremises.Schema.SRP_Type, "Type", 150),
			(CusTempStorageRegPremises.Schema.TypeDescription, "Type Description", 250),
			(CusTempStorageRegPremises.Schema.SRP_CustomsLocation, "Location", 200)
		};
	}
}
