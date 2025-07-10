using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.ZArchitecture.Business.StmModuleFilter;

namespace Enterprise.ZArchitecture.GUI.Controls.Grid.GridColour
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	sealed class GridColourSchemeTranslatableDataFieldAttribute : TranslatableDataFieldAttribute, ICustomizableDataCaptionSource
	{
		public GridColourSchemeTranslatableDataFieldAttribute(string tableName, string columnName, string dataXmlFilePaths, string contextColumnName)
			: base(tableName, columnName, dataXmlFilePaths, contextColumnName)
		{
			Type = typeof(GridColourScheme);
		}

		public override ZQuery Filter
		{
			get
			{
				var filter = new ZQuery(StmModuleFilterSchema.S9_IsPublished, true);
				filter.AddToFilter(StmModuleFilterSchema.S9_ModuleID, SQLComparisonOperator.EndsWith, ModuleIdSuffix.GridColorScheme);
				return filter;
			}
		}

		protected override string KeyPrefixContextColumnName
		{
			get { return base.KeyPrefixContextColumnName + ModuleIdSuffix.GridColorScheme; }
		}
	}
}
