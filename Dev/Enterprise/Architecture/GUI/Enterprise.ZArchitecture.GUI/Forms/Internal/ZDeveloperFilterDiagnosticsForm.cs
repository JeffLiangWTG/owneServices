using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;
using GlowIndexQueryService.Business;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public partial class ZDeveloperFilterDiagnosticsForm : KForm
	{
		public ZDeveloperFilterDiagnosticsForm(ZFilterStripCommonControl control, IBusinessObjectCollection gridCollection) : this(control.FilterBusinessObject, gridCollection)
		{
		}

		public ZDeveloperFilterDiagnosticsForm(FilterStripBusinessObject filterStripBusinessObject, IBusinessObjectCollection gridCollection)
		{
			InitializeComponent();

			this.filterBusinessObject = filterStripBusinessObject;
			this.gridCollection = gridCollection;

			if (filterBusinessObject.HasIndexSearchFields && filterBusinessObject.IsGlowIndexSearchAllowed)
			{
				indexSearchAnalyzer.Visible = true;
				ControlDpiScalingHelper.SetHeight(ref operationButtonsGroupBox, operationButtonsGroupBox.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(24), false);
				ClientSize = ControlDpiScalingHelper.NewScaledSize(186, 202, true);

				UpdateGUIBySearchType();
				this.filterBusinessObject.SearchTypeChanged += _ =>
				{
					UpdateGUIBySearchType();
				};
			}
		}

		void UpdateGUIBySearchType()
		{
			var isIndexSearchType = filterBusinessObject.SearchType == SearchType.Index;
			var isSqlSearchType = filterBusinessObject.SearchType == SearchType.Sql;
			queryAnalyzer.Enabled = isSqlSearchType;
			queryAnalyzer.Checked = isSqlSearchType;
			generatedFilterCheckBox.Enabled = isSqlSearchType;
			additionalFilterCheckBox.Enabled = isSqlSearchType;
			relationshipFilterCheckBox.Enabled = isSqlSearchType;
			indexSearchAnalyzer.Enabled = isIndexSearchType;
			indexSearchAnalyzer.Checked = isIndexSearchType;
		}

		void ShowXml()
		{
			var serializer = ZXmlSerializer.New(typeof(FilterStripCollection));
			var builder = new StringBuilder();

			using (var stream = new StringWriter(builder))
			{
				serializer.Serialize(stream, filterBusinessObject.FilterStrips);
			}

			Globals.Message.Show(builder.ToString());
		}

		void ShowZQueryAnalyzer()
		{
			var tool = new DevTools.QueryAnalyserTool();
			var filter = AssembleFilter();
			var sql = filter != null ? BuildSQL(filter) : string.Empty;
			tool.Show(sql);
		}

		ZQuery AssembleFilter()
		{
			var filter = new ZQuery();
			var filterAdded = false;

			if (generatedFilterCheckBox.Checked)
			{
				filter.AddToFilter(filterBusinessObject.Filter);
				filter.MaximumRows = filterBusinessObject.Filter.MaximumRows;
				filterAdded = true;
			}

			if (relationshipFilterCheckBox.Checked)
			{
				var legacyGridCollection = gridCollection as BusinessObjectCollection;
				if (legacyGridCollection != null)
				{
					filter.AddToFilter(((ILegacyBusinessObjectCollectionInternals)legacyGridCollection).RelationshipFilter);
				}
				var activeCollection = gridCollection as IActiveBusinessObjectCollection;
				if (activeCollection != null)
				{
					filter.AddToFilter(activeCollection.Relationship.RelationshipFilter);
				}
				filterAdded = true;
			}

			if (additionalFilterCheckBox.Checked)
			{
				var activeCollection = gridCollection as IActiveBusinessObjectCollection;
				var legacyCollection = gridCollection as BusinessObjectCollection;
				if (activeCollection != null)
				{
					filter.AddToFilter(activeCollection.AdditionalFilter);
				}
				if (legacyCollection != null)
				{
					filter.AddToFilter(((ILegacyBusinessObjectCollectionInternals)legacyCollection).AdditionalFilter);
				}
				filterAdded = true;
			}

			return filterAdded ? filter : null;
		}

		string BuildSQL(ZQuery filter)
		{
			if (!filter.IgnoreActiveFilter)
			{
				filter.AddToFilter(BusinessObject.GetActiveFilter(gridCollection.TypeOfElements));
			}
			var tableName = GetTableName();

			if (filter.IsUnionQuery)
			{
				var connectionInfo = new ZSqlConnectionInfo(null, "");
				var query = new ZDataQuery(connectionInfo, tableName, filter);
				return query.LiteralTextSql;
			}
			else
			{
				var filterText = filter.LiteralTextSqlFormatted;

				var sqlQuery = "SELECT";

				if (filter.MaximumRows > 0)
				{
					sqlQuery += $" TOP {filter.MaximumRows}";
				}

				sqlQuery += $" *\r\nFROM {tableName}";

				if (!string.IsNullOrEmpty(filterText))
				{
					sqlQuery += $"\r\nWHERE\r\n{filter.LiteralTextSqlFormatted}";
				}

				return sqlQuery;
			}
		}

		string GetTableName()
		{
			try
			{
				return BusinessObjectFactory.GetTableNameFromType(gridCollection.TypeOfElements);
			}
			catch (ZException)
			{
				return "<UnknownTableName>"; // Not Translatable. Placeholder for SQL table name
			}
		}

		void closeButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void showButton_Click(object sender, EventArgs e)
		{
			if (queryAnalyzer.Checked)
			{
				ShowZQueryAnalyzer();
			}
			else if (showXml.Checked)
			{
				ShowXml();
			}
			else if (indexSearchAnalyzer.Checked)
			{
				ShowIndexSearchAnalyzer();
			}
		}

		void ShowIndexSearchAnalyzer()
		{
			var queries = filterBusinessObject.GetIndexSearchQueries().ToList();
			var entityType = filterBusinessObject.IndexSearchFields?.EntityType;
			var maxCount = ObjectFactory.Get<IGlowRegistry>().MaximumNumberOfModuleFiltersSearchResults;
			var tool = new DevTools.IndexSearchAnalyzerTool();
			tool.Show(new GlowIndexQueryParam(queries, entityType, maxCount));
		}

		readonly FilterStripBusinessObject filterBusinessObject;
		readonly IBusinessObjectCollection gridCollection;
	}
}

#region Test
#if DEBUG

#region Extra Test Methods

namespace Enterprise.ZArchitecture.GUI.Internal
{
	partial class ZDeveloperFilterDiagnosticsForm
	{
		public void DoShowXml()
		{
			ShowXml();
		}
	}

	partial class ZDeveloperFilterDiagnosticsForm
	{
		public string DoBuildSQL(ZQuery filter)
		{
			return BuildSQL(filter);
		}
	}
}

#endregion

#endif
#endregion
