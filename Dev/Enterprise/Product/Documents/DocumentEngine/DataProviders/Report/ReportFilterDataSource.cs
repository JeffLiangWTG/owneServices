using System;
using System.Data;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.DataProviders
{
	class ReportFilterDataSource : ZExpressionEvaluator
	{
		public ReportFilterDataSource(DataRow dataSource, string functionString)
			: base(functionString)
		{
			DataSource = dataSource;
		}
		readonly DataRow DataSource;

		protected override object GetValueFor(string propertyName)
		{
			int index = DataSource.Table.Columns.IndexOf(propertyName);
			if (index != -1)
			{
				return DataSource.ItemArray.GetValue(index);
			}

			try
			{
				FieldNotFoundException.ReportFieldNotFound(propertyName + (NoResString)" from [" + FunctionString + (NoResString)"]", new DataProviderList(BODocDataProvider.Get(BODocDataProvider.GetBusinessObject(DataSource))));
			}
			catch (InvalidCastException)
			{
				FieldNotFoundException.ReportFieldNotFound(propertyName + (NoResString)" from [" + FunctionString + (NoResString)"]", DataSource);
			}

			return "<" + propertyName + ">";
		}
	}
}
