using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Module.Testing
{
	class TempStorageRegisterLinesFilterStripControlTest : TestCaseWithFactory
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
					foreach (var (columnName, columnCaption) in ColumnDetails)
					{
						AssertEquals(columnName, columnCaption, grid.GetColumnCaption(columnName));
					}
				});
			}
		}

		TempStorageRegLinesFilterStripControl GetFilterControl() => new TempStorageRegLinesFilterStripControl(new CusTempStorageRegLineSelCollection(Factory, new ZDBOnlyQuery(typeof(CusTempStorageRegLine))), new TempStorageRegisterLinesFilterBusinessObject());

		(string ColumnName, string ColumnCaption)[] ColumnDetails => new[]
		{
			(CusTempStorageRegLine.Schema.SRL_GoodsOwnerIdentifier, "Owner ID"),
			(CusTempStorageRegLine.Schema.SRL_LineNumber, "Line Number"),
			(CusTempStorageRegLine.Schema.SRL_GoodsDescription, "Goods Description"),
			(CusTempStorageRegLine.Schema.SRL_PackageType, "Package Type"),
			(CusTempStorageRegLine.Schema.SRL_CustomsStatus, "Customs Status"),
		};
	}
}
