using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Testing;

namespace Enterprise.UniversalDataBuss.Core.Testing
{
	sealed class GenericDataObjectReaderTest : TestCaseWithFactory
	{
		public void TestCreateNewColumnIndexer_Defaulting()
		{
			var (reader, _) = CreateReader();

			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var columnIndexer = reader.CreateNewColumnIndexerExposed(StmNoteSchema.PK, typeof(StmNote));

				CombineAssertions(() =>
				{
					AssertNotNull("columnIndexer not created", columnIndexer);
					AssertType<StmNote>("BO should be returned as IColumnIndexer", columnIndexer);
				});
			}
		}

		public void TestCreateNewColumnIndexer_NoDefaulting()
		{
			var (reader, _) = CreateReader();

			using(eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var columnIndexer = reader.CreateNewColumnIndexerExposed(StmNoteSchema.PK, typeof(StmNote));

				CombineAssertions(() =>
				{
					AssertNotNull("columnIndexer not created", columnIndexer);
					Assert("BO should be converted to DataRow", columnIndexer is DataRow);
				});
			}
		}

		public void TestCreateNewColumnIndexer_Abstract()
		{
			var (reader, logger) = CreateReader();

			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var columnIndexer = reader.CreateNewColumnIndexerExposed(DummyBusinessObjectSchema.PK, typeof(DummyAbstractBusinessObjectWithTypeDecider));

				CombineAssertions(() =>
				{
					AssertNotNull("columnIndexer not created", columnIndexer);
					Assert("DataRow created via RowFactory", columnIndexer is DataRow);
				});
			}
		}

		public void TestCreateNewBusinessObject_Null()
		{
			var (reader, _) = CreateReader();
			AssertNull("No BO created with null type", reader.CreateNewBusinessObjectExposed(null));
		}

		public void TestCreateNewBusinessObject_Abstract()
		{
			var (reader, logger) = CreateReader();
			CombineAssertions(() =>
			{
				AssertNull("No BO created for abstract type", reader.CreateNewBusinessObjectExposed(typeof(DummyAbstractBusinessObjectWithTypeDecider)));
				AssertContains("Logs", "Cannot create an abstract type", logger.Logs);
			});
		}

		public void TestCreateNewBusinessObject_Actual()
		{
			var (reader, _) = CreateReader();
			CombineAssertions(() =>
			{
				var note = reader.CreateNewBusinessObjectExposed(typeof(StmNote));
				AssertNotNull("Failed to create StmNote", note);
				AssertType<StmNote>("StemNote expected", note);
			});
		}

		(GenericDataObjectReaderForTest<DummyDataObject> Reader, TestErrorLogger Logger) CreateReader()
		{
			var logger = new TestErrorLogger();
			var reader = new GenericDataObjectReaderForTest<DummyDataObject>(new DummyDataObject(), logger, new UniversalObjectFactory());

			return (reader, logger);
		}

		class DummyDataObject : IDataObject
		{ }

		class GenericDataObjectReaderForTest<T> : DataObjectReader<T> where T : IDataObject
		{
			public GenericDataObjectReaderForTest(T dataObject, IXmlImportLogger logger, UniversalObjectFactory factory) : base(dataObject, logger, factory)
			{
			}

			public IColumnIndexer CreateNewColumnIndexerExposed(SchemaGuidColumn columnPK, Type type) => CreateNewColumnIndexer(columnPK, type);
			public BusinessObject CreateNewBusinessObjectExposed(Type type) => CreateNewBusinessObject(type);
		}
	}
}
