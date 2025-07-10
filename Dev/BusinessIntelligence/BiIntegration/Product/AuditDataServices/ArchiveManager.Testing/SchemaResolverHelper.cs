using System;
using CargoWise.Application;
using CargoWise.Schema;
using Moq;

namespace Enterprise.AuditDataServices.ArchiveManager.Testing
{
	public static class SchemaResolverHelper
	{
		public static IDisposable SetupApplicationSchemaResolver()
		{
			var originalResolver = ObjectFactory.Get<IApplicationSchemaResolver>();
			var resolver = new Mock<IApplicationSchemaResolver>(MockBehavior.Strict);

			Func<string, ITableSchema> getTableSchema = (string tableName) =>
			{
				return tableName == "InvalidMatchTest" ? InvalidMatchTestSchema.Instance : originalResolver.GetTableSchema(tableName);
			};

			Func<string, string, SchemaColumn> getSchemaColumn = (string columnName, string tableName) =>
			{
				var tableSchema = getTableSchema(tableName);
				return tableSchema?.GetSchemaColumn(columnName);
			};

			resolver.Setup(x => x.GetSchemaColumn(It.IsAny<string>(), It.IsAny<string>())).Returns(getSchemaColumn);
			resolver.Setup(x => x.GetTableSchema(It.IsAny<string>())).Returns(getTableSchema);

			return ObjectFactory.Substitute(resolver.Object);
		}
	}
}
