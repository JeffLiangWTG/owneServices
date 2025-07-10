using System;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public static class DataContextFactory
	{
		public static IDataContextDataObject New()
		{
			return New(SchemaVersionManager.Current.Namespace);
		}

		public static IDataContextDataObject New(string currentNamespace)
		{
			switch (currentNamespace)
			{
				case UniversalXmlInfo.Namespace_2011_11:
					return new Universal._2011_11.DataContext();
				case UniversalXmlInfo.Namespace_2012_11:
					return new Universal._2012_11.DataContext();
				default:
					throw new InvalidOperationException("All namespaces must have a handler to return an appropriate DataContext object.");
			}
		}

		#region Data Target

		public static IDataTargetDataObject NewDataTarget()
		{
			return NewDataTarget(SchemaVersionManager.Current.Namespace);
		}

		public static IDataTargetDataObject NewDataTarget(string currentNamespace)
		{
			switch (currentNamespace)
			{
				case UniversalXmlInfo.Namespace_2011_11:
					return new Universal._2011_11.DataTarget();
				case UniversalXmlInfo.Namespace_2012_11:
					return new Universal._2012_11.DataTarget();
				default:
					throw new InvalidOperationException("All namespaces must have a handler to return an appropriate DataTarget object.");
			}
		}

		#endregion

		#region Data Source

		public static IDataSourceDataObject NewDataSource()
		{
			return NewDataSource(SchemaVersionManager.Current.Namespace);
		}

		public static IDataSourceDataObject NewDataSource(string nameSpace)
		{
			switch (nameSpace)
			{
				case UniversalXmlInfo.Namespace_2011_11:
					return new Universal._2011_11.DataSource();
				case UniversalXmlInfo.Namespace_2012_11:
					return new Universal._2012_11.DataSource();
				default:
					throw new InvalidOperationException("All namespaces must have a handler to return an appropriate DataTarget object.");
			}
		}

		#endregion

		public static IDataContextDataObject New(IDataContextManager manager, string currentNamespace)
		{
			var result = New(currentNamespace);
			result.AddDataSource(manager.DataContextType, manager.DataContextKey);
			return result;
		}
	}
}
