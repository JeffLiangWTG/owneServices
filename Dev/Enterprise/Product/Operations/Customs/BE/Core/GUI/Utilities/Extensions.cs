using System.Collections.Generic;
using System.Linq;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.BE.GUI;

public static class Extensions
{
	public static void RemoveUnneededColumns(this ZGrid zgrid, IEnumerable<string> columnNamesToKeep)
	{
		var columnsToRemove = zgrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => !columnNamesToKeep.Contains(x.ColumnName)).ToList();
		columnsToRemove.ForEach(zgrid.ColumnStyles.Remove);
	}

	public static void RemoveColumn(this ZGrid zgrid, string columnName)
	{
		var columnToRemove = zgrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == columnName);
		if (columnToRemove != null)
		{
			zgrid.ColumnStyles.Remove(columnToRemove);
		}
	}

	public static void SetResourceStringForColumn(this ZGrid zgrid, string columnName, ResourceStringData resourceStringData)
	{
		var column = zgrid.ColumnStyles.OfType<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == columnName);
		if (column != null)
		{
			column.CaptionResourceString = resourceStringData;
		}
	}
}
