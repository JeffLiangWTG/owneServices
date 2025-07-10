using System;
using System.Data;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngineCore.DocumentSupport.Testing
{
	sealed class FetchStrategyTest : TestCaseWithFactory
	{
		public void TestInitialiseFetchStrategy_FetchStrategyNotSupported()
		{
			var bizo = Factory.New<DummyDocumentSupportable>();
			bizo.FetchStrategyForTesting = new BusinessObjectFetchStrategy(bizo);
			AssertNull(bizo.FetchStrategy as IDocumentSupporterFetchStrategy);

			var documentSupporter = bizo.DocumentSupporter;

			using (var disposableAction = documentSupporter.InitialiseFetchStrategy())
			{
				AssertNull(disposableAction);
				((IExternalFetchHintSupporter)Factory).AddTableFetchHintCreator(CusContainerSchema.Instance, (IColumnIndexer arg) => null, true);
			}

			AssertContains("SetupCreator was not set up", "AddTableFetchHintCreator was called without setting up the creator on the Factory", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestInitialiseFetchStrategy_FetchStrategySupported()
		{
			bool addFetchHintsMethodCalled = false;
			var bizo = Factory.New<DummyDocumentSupportable>();
			bizo.FetchStrategyForTesting = new DummyFetchStrategy(bizo)
			{
				OnAddDocumentSupporterFetchHints = () =>
				{
					addFetchHintsMethodCalled = true;
					((IExternalFetchHintSupporter)Factory).AddTableFetchHintCreator(CusContainerSchema.Instance, (IColumnIndexer arg) => null, true);
				}
			};
			AssertNotNull(bizo.FetchStrategy as IDocumentSupporterFetchStrategy);

			var documentSupporter = bizo.DocumentSupporter;
			Assert("Document Fetch Strategy not yet set up", !addFetchHintsMethodCalled);

			using (var disposableAction = documentSupporter.InitialiseFetchStrategy())
			{
				AssertType<DisposableAction>(disposableAction);
			}

			Assert("Fetch Strategy method was called", addFetchHintsMethodCalled);
			AssertEquals("SetupCreator was set up correctly", ZString.Empty, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestInitialiseFetchStrategy_FetchStrategySetupFailure()
		{
			var bizo = Factory.New<DummyDocumentSupportable>();
			bizo.FetchStrategyForTesting = new DummyFetchStrategy(bizo)
			{
				OnAddDocumentSupporterFetchHints = () => throw new NotImplementedException()
			};
			AssertNotNull(bizo.FetchStrategy as IDocumentSupporterFetchStrategy);

			var documentSupporter = bizo.DocumentSupporter;

			AssertExceptionThrown<NotImplementedException>(() => documentSupporter.InitialiseFetchStrategy());
			((IExternalFetchHintSupporter)Factory).AddTableFetchHintCreator(CusContainerSchema.Instance, (IColumnIndexer arg) => null, true);

			AssertContains("SetupCreator was cleaned up", "AddTableFetchHintCreator was called without setting up the creator on the Factory", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		class DummyDocumentSupportable : DummyBusinessObject, IDocumentSupportable
		{
			public DummyDocumentSupportable(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public DocumentSupporter DocumentSupporter => documentSupporter ?? (documentSupporter = new DummyDocumentSupporter(this));
			DocumentSupporter documentSupporter;

			public IBusinessObjectFetchStrategy FetchStrategyForTesting;
			protected override IBusinessObjectFetchStrategy GetFetchStrategy() => FetchStrategyForTesting ?? base.GetFetchStrategy();
		}

		class DummyDocumentSupporter : DocumentSupporter
		{
			public DummyDocumentSupporter(BusinessObject parentBusinessObject) : base(parentBusinessObject)
			{
			}

			public override BusinessContext BusinessContext => new BusinessContext();
			public override ISecurityCheckpoint CustomisationSecurityCheckpoint => null;
			protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun) => null;
			protected override Core.Constants.DataContext[] GetSupportedDataContexts() => null;
		}

		class DummyFetchStrategy : BusinessObjectFetchStrategy, IDocumentSupporterFetchStrategy
		{
			public DummyFetchStrategy(BusinessObject parent) : base(parent)
			{
			}

			public void AddDocumentSupporterFetchHints()
			{
				OnAddDocumentSupporterFetchHints?.Invoke();
			}

			public Action OnAddDocumentSupporterFetchHints { get; set; }
		}
	}
}
