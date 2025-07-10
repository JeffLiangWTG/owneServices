using System.Collections.Generic;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.DocumentEngineCore.DocumentSupport.Testing
{
	class DocumentSupporterWithConcreteAdditionalBODataSourceTest : DocumentSupporterBaseTest
	{
		public override void TestIsDataContextSupportedForBusinessObjectDataContext()
		{
			base.TestIsDataContextSupportedForBusinessObjectDataContext();

			AssertEquals("DocSupporter.IsDataContextSupported(new DataContextValue('.DummyTradeObject'))", true, DocSupporter.IsDataContextSupported(new DataContextValue(".DummyTradeObject")));
			AssertEquals("DocSupporter.IsDataContextSupported(new DataContextValue('.Testing.DummyTradeObject'))", true, DocSupporter.IsDataContextSupported(new DataContextValue(".Testing.DummyTradeObject")));
		}

		protected override string ExpectedFilterForSupportedDataContexts
		{
			get { return "(SO_DataContext in ('.DummyEnterpriseBusinessObject', '.DummyTradeObject', '.Testing.DummyTradeObject', 'UnitTest'))"; }
		}

		public override void TestGetBODocDataProvidersForBusinessObjectDataContext()
		{
			base.TestGetBODocDataProvidersForBusinessObjectDataContext();

			IBODocDataProvider[] results = DocSupporter.GetBODocDataProviders(new DataContextValue(".DummyTradeObject"), null);
			AssertNotNull("GetBODocDataProviders for .DummyTradeObject should not be null.", results);
			AssertEquals("GetBODocDataProviders Count for .DummyTradeObject", 1, results.Length);
			AssertEquals("GetBODocDataProviders[0] for .DummyTradeObject", typeof(DummyTradeObject), BODocDataProvider.GetBusinessObject(results[0]).GetType());
		}

		public override void TestGetBODocDataProvidersForDocumentWrapperDataContext()
		{
			var results = DocSupporter.GetBODocDataProviders(new DataContextValueForTesting(DataContext.Dummy), null);
			AssertNotNull("GetBODocDataProviders for DataContext.Dummy", results);
			AssertEquals("GetBODocDataProviders Count for DataContext.Dummy", 1, results.Length);
			AssertEquals("GetBODocDataProviders for DataContext.Dummy", typeof(DocWrappers.Testing.DocumentWrapperForTesting), results[0].GetType());
		}

		public override void TestCommaSeparatedListOfSupportedDataContexts()
		{
			AssertEquals("UnitTest, .DummyEnterpriseBusinessObject, .DummyTradeObject, .Testing.DummyTradeObject", DocSupporter.CommaSeparatedListOfSupportedDataContexts);
		}

		public override void TestListOfSupportedDataContexts()
		{
			CodeDescriptionPairList list = DocSupporter.ListOfSupportedDataContexts;
			AssertEquals("list.Count", 4, list.Count);
			AssertEquals("list[0].Code", "UnitTest", list[0].Code);
			AssertEquals("list[1].Code", ".DummyEnterpriseBusinessObject", list[1].Code);
			AssertEquals("list[2].Code", ".DummyTradeObject", list[2].Code);
			AssertEquals("list[3].Code", ".Testing.DummyTradeObject", list[3].Code);
		}

		#region Implementation

		protected override DocumentSupporterForTesting GetNewDocSupporter(DummyEnterpriseBusinessObject parentBusinessObject)
		{
			return new DocumentSupporterWithConcreteAdditionalBODataSource(parentBusinessObject);
		}

		[WTG.StaticAnalysis.Annotation.CodeAlive(@"This is used in DocumentSupporter/DocumentSupporterWithConcreteAdditionalBODataSourceTest.")]
		protected class DocumentSupporterWithConcreteAdditionalBODataSource : DocumentSupporterForTesting
		{
			public DocumentSupporterWithConcreteAdditionalBODataSource(DummyEnterpriseBusinessObject parentBusinessObject)
				: base(parentBusinessObject)
			{
			}

			protected override List<DocumentSupporter> AdditionalBODataSourceDocumentSupporters
			{
				get
				{
					List<DocumentSupporter> result = base.AdditionalBODataSourceDocumentSupporters;
					result.Add(AdditionalDocumentSupporterParent.DocumentSupporter);
					return result;
				}
			}

			protected virtual DummyTradeObject AdditionalDocumentSupporterParent
			{
				get { return Factory.New<DummyTradeObject>(); }
			}
		}

		#endregion
	}
}
