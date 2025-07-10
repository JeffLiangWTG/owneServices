using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using CargoWise.Bi.Developement.SchemaSync;
using CargoWise.Bi.Development.Common;
using CargoWise.Common;

namespace CargoWise.Bi.Development.Automation.Manager
{
	#region SuppressResourceStringsCheckRegion
	public partial class MainWindow : Window, IDisposable
	{
		#region Source and Model View Mapping

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void findMapping_Click(object sender, RoutedEventArgs e)
		{
			Invoke(() =>
			{
				try
				{
					var input = mappingInputTextBox.Text.TrimEnd().TrimStart();
					var columnMapper = new ColumnMapper();
					columnMapper.MapCdcColumnsToModelView();
					var mapping = columnMapper.GetViewMappingFromStringOfColumns(input);

					var numberOfMapping = mapping.Where(c => !string.IsNullOrEmpty(c.ModelColumn)).Select(c => c.SourceColumn).Distinct().Count();
					var numberOfUmappedColumns = mapping.Count(c => !string.IsNullOrEmpty(c.SourceTable) && string.IsNullOrEmpty(c.ModelColumn));
					var numberOfUnfoundColumns = mapping.Count(c => string.IsNullOrEmpty(c.SourceTable) && string.IsNullOrEmpty(c.ModelColumn));

					BiLogger.Complete(string.Format(CultureInfo.InvariantCulture, "{0} column(s) with mapping, {1} column(s) without mapping, {2} column(s) not found is source.", numberOfMapping, numberOfUmappedColumns, numberOfUnfoundColumns));

					sourceModelViewMappingGrid.ItemsSource = mapping;
					sourceModelViewMappingGrid.Items.Refresh();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					BiLogger.Fail(ex);
				}
			});
		}

		#endregion

	}
	#endregion
}