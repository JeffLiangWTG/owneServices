using System.Collections.Generic;
using System.Data;
using CargoWise.Bi.Development.Common;

namespace CargoWise.Bi.Deployment.AnalysisServices.Reports.DataSets
{
	public partial class UseCaseConfigDataSet
	{
		partial class ResultSetDataTable
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Dummy literal")]
			protected override void OnColumnChanged(DataColumnChangeEventArgs e)
			{
				base.OnColumnChanged(e);
				if (e.Column.ColumnName == "UseCaseName")
				{
					if (((ResultSetRow)e.Row).IsNull("UseCaseName") && ((ResultSetRow)e.Row)["Name"].ToString().Contains("dummy"))
					{
						((ResultSetRow)e.Row).Delete();
						((ResultSetRow)e.Row).AcceptChanges();
					}
				}
			}
		}

		public void SaveConfiguration(string filePath)
		{
			AcceptChanges();
			SortDataSet();
			BiConfigurationModification.SaveConfigurationXml(this, filePath);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Table names")]
		void SortDataSet()
		{
			Dictionary<string, string> useCasesTableDict = new Dictionary<string, string>()
			{
				{ "TestableObjectType", "TestableObjectTypeID" },
				{ "ResultSet", "TestableObjectType, Type, SubType, Name" },
				{ "TargetType", "Name" },
				{ "UseCase", "TestDataContext, Name" }
			};

			foreach (DataTable table in Tables)
			{
				foreach (DataColumn column in table.Columns)
				{
					column.ColumnMapping = MappingType.Attribute;
				}
			}

			this.EnforceConstraints = false;
			foreach (var table in useCasesTableDict)
			{
				DataView dataView = Tables[table.Key].Copy().AsDataView();
				dataView.Sort = table.Value;

				Tables[table.Key].Rows.Clear();
				foreach (DataRow dataRow in dataView.ToTable().Rows)
				{
					Tables[table.Key].Rows.Add(dataRow.ItemArray);
				}
			}
			EnforceConstraints = true;
		}
	}
}
