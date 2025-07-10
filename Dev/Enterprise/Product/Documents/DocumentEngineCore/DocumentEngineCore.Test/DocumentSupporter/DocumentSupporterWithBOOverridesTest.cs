using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngineCore.DocumentSupport.Testing
{
	class DocumentSupporterWithBOOverridesTest : DocumentSupporterBaseTest
	{
		public override void TestIsDataContextSupportedForBusinessObjectDataContext()
		{
			base.TestIsDataContextSupportedForBusinessObjectDataContext();

			AssertEquals("DocSupporter.IsDataContextSupported(new DataContextValue('.DummyChildBusinessObject'))", true, DocSupporter.IsDataContextSupported(new DataContextValue(".DummyChildBusinessObject")));
			AssertEquals("DocSupporter.IsDataContextSupported(new DataContextValue('.Testing.DummyChildBusinessObject'))", true, DocSupporter.IsDataContextSupported(new DataContextValue(".Testing.DummyChildBusinessObject")));
		}

		protected override string ExpectedFilterForSupportedDataContexts
		{
			get { return "(SO_DataContext in ('.DummyChildBusinessObject', '.DummyEnterpriseBusinessObject', '.Testing.DummyChildBusinessObject', 'UnitTest'))"; }
		}

		public override void TestGetBODocDataProvidersForBusinessObjectDataContext()
		{
			BizObject.Collection.AddNew();
			BizObject.Collection.AddNew();

			base.TestGetBODocDataProvidersForBusinessObjectDataContext();

			IBODocDataProvider[] results = DocSupporter.GetBODocDataProviders(new DataContextValue(".DummyChildBusinessObject"), null);
			AssertNotNull("GetBODocDataProviders for .DummyChildBusinessObject should not be null.", results);
			AssertEquals("GetBODocDataProviders Count for .DummyChildBusinessObject", 2, results.Length);
			AssertEquals("GetBODocDataProviders[0] for .DummyChildBusinessObject2", typeof(DummyChildEnterpriseBusinessObject), BODocDataProvider.GetBusinessObject(results[0]).GetType());
			AssertEquals("GetBODocDataProviders[1] for .DummyChildBusinessObject2", typeof(DummyChildEnterpriseBusinessObject), BODocDataProvider.GetBusinessObject(results[1]).GetType());
		}

		public override void TestCommaSeparatedListOfSupportedDataContexts()
		{
			AssertEquals("UnitTest, .DummyEnterpriseBusinessObject, .DummyChildBusinessObject, .Testing.DummyChildBusinessObject", DocSupporter.CommaSeparatedListOfSupportedDataContexts);
		}

		public override void TestListOfSupportedDataContexts()
		{
			CodeDescriptionPairList list = DocSupporter.ListOfSupportedDataContexts;
			AssertEquals("list.Count", 4, list.Count);
			AssertEquals("list[0].Code", "UnitTest", list[0].Code);
			AssertEquals("list[1].Code", ".DummyEnterpriseBusinessObject", list[1].Code);
			AssertEquals("list[2].Code", ".DummyChildBusinessObject", list[2].Code);
			AssertEquals("list[3].Code", ".Testing.DummyChildBusinessObject", list[3].Code);
		}

		#region Implementation
		protected override DocumentSupporterForTesting GetNewDocSupporter(DummyEnterpriseBusinessObject parentBusinessObject)
		{
			return new DocumentSupporterForBOTesting(parentBusinessObject);
		}

		internal class DocumentSupporterForBOTesting : DocumentSupporterForTesting
		{
			public DocumentSupporterForBOTesting(DummyEnterpriseBusinessObject parentBusinessObject)
				: base(parentBusinessObject)
			{
			}

			protected override List<DataContextValue> GetSupportedBODataSources()
			{
				List<DataContextValue> result = base.GetSupportedBODataSources();
				result.AddRange(GetSupportedBODataSourcesFor(typeof(DummyChildBusinessObject)));
				return result;
			}

			protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
			{
				if (dataContextValue.WantsBusinessObjectOfType(typeof(DummyChildBusinessObject)))
				{
					return BODocDataProvider.GetArray(DummyBO.Collection.ToArray());
				}
				return base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
			}
		}
		#endregion
	}
}
