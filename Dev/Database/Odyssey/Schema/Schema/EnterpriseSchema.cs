using System;
using System.Globalization;
using System.Reflection;
using CargoWise.Common.Testing;
using CargoWise.Schema;

namespace Enterprise.ZArchitecture.Schema
{
	public sealed class EnterpriseSchema : AutoEnterpriseSchema
	{
		public static ITableSchema GetTableSchema(string tableName)
		{
			if (string.IsNullOrEmpty(tableName))
			{
				return null;
			}

			var nameStart = tableName.LastIndexOf('.') + 1;
			if (nameStart > 0)
			{
				tableName = tableName.Substring(nameStart);
			}

			return GetStandardTableSchema(tableName) ?? EnterpriseSchema.ClientSpecificTableSchemaSource.GetTableSchema(tableName);
		}

		/// <summary>
		/// Get the ITableSchema object for a table with the given ColumnNamePrefix.
		/// WARNING: Only tables in the primary database are considered.
		/// </summary>
		public static ITableSchema GetTableSchemaFromColumnNamePrefix(string columnNamePrefix)
		{
			if (columnNamePrefix == null)
			{
				return null;
			}

			var result = GetStandardTableSchemaFromColumnNamePrefix(columnNamePrefix);

			if (result != null)
			{
				return result;
			}

			foreach (var clientTableSchema in EnterpriseSchema.ClientSpecificTableSchemaSource.TableSchemas)
			{
				if (clientTableSchema.PK.Name.StartsWith(columnNamePrefix))
				{
					return clientTableSchema;
				}
			}

			return null;
		}

		internal static ITableSchemaSource ClientSpecificTableSchemaSource
		{
			get
			{
				var tableSchemaSource = fClientSpecificTableSchemaSource.Value;
				return tableSchemaSource;
			}
		}

		[SuppressThreadStaticFieldMessage]
		static readonly Lazy<ITableSchemaSource> fClientSpecificTableSchemaSource = new Lazy<ITableSchemaSource>(InitializeTableSchemaSource);

		static ITableSchemaSource InitializeTableSchemaSource()
		{
			Assembly zModulesAssembly = Assembly.Load("Enterprise.ZArchitecture.Modules");
			Type clientHookLoaderType = zModulesAssembly.GetType("Enterprise.ZArchitecture.Modules.ClientHookLoader");
			var tableSchemaSource = (ITableSchemaSource)clientHookLoaderType.InvokeMember("Instance", BindingFlags.GetProperty, null, null, null, CultureInfo.InvariantCulture);
			return tableSchemaSource;
		}
	}
}
