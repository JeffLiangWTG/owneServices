using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class DocumentFactoryProviderTest : TestCase
	{
		public void TestReturnsDocumentFactoryWithParentFactory()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DocumentFactoryProvider provider = new DocumentFactoryProvider();
			IDocumentFactory idf = provider.GetFactory(factory);
			AssertNotNull(idf);
			DocumentFactory df = idf as DocumentFactory;
			AssertNotNull(df);
			AssertEquals(factory, df.FactoryForEverythingExceptEDocs);
		}

		public void TestChildFactories()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DocumentFactoryProvider provider = new DocumentFactoryProvider();
			DocumentFactory df = provider.GetFactory(factory);
			AssertEquals(1, df.ChildFactories.Count);
			AssertEquals(factory, df.ChildFactories[0]);
		}
	}
	#region Implementation
	public class DocumentFactoryProviderForTest : IDocumentFactoryProviderForTest
	{
		public DbBackendDocumentFactoryForTest GetFactory(BusinessObjectFactory factoryForEverythingExceptEDocs)
		{
			return new DbBackendDocumentFactoryForTest(factoryForEverythingExceptEDocs);
		}

		IDocumentFactoryForTest IDocumentFactoryProviderForTest.GetFactory(BusinessObjectFactory factoryForEverythingExceptEDocs)
		{
			return this.GetFactory(factoryForEverythingExceptEDocs);
		}
	}

	public sealed class DbBackendDocumentFactoryForTest : DbBackendDocumentFactory, IDocumentFactoryForTest, IDisposable
	{
		public DbBackendDocumentFactoryForTest(BusinessObjectFactory factoryForEverythingExceptEDocs) : base(factoryForEverythingExceptEDocs)
		{
		}

		readonly List<DbConnection> connections = new List<DbConnection>();

		bool IDocumentFactoryForTest.Import(byte[] contents, ZString filenameOnly, ZString userSuppliedRefType, ZGuid userSuppliedRefPK, ZString userSuppliedDocType, ZString userSuppliedDocSource, ZGuid visibleCompanyPK, ZGuid visibleBranchPK, ZGuid visibleDepartmentPK, string fileFullPath)
		{
			return Import(contents, filenameOnly, userSuppliedRefType, userSuppliedRefPK, userSuppliedDocType, userSuppliedDocSource, visibleCompanyPK, visibleBranchPK, visibleDepartmentPK, fileFullPath);
		}

		public bool UseExtraConnectionForEDocsFactories { get; set; }

		protected override NumberedBusinessObjectFactory GetNumberedBusinessObjectFactory(int number)
		{
			var connection = Db.NewExtraConnectionToMainDb();
			connections.Add(connection);
			return UseExtraConnectionForEDocsFactories ? new NumberedBusinessObjectFactory(connection, this) : base.GetNumberedBusinessObjectFactory(number);
		}

		#region IDisposable Support

		bool disposedValue;

		public void Dispose()
		{
			if (!disposedValue)
			{
				connections.ForEach(f => f.Dispose());
				disposedValue = true;
			}
		}

		#endregion
	}
	#endregion
}
