using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.VisualBoards.Business.Test
{
	class SharedBoardFactoryProviderTest : VisualBoardsTestCase
	{
		public void TestReadonlyFactoriesCreatedThroughBoardProvider_ShouldNotHaveValidationEnabled_OrDataRefresh()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory);
			var board = VisualBoardsTestHelper.CreateBoard(system);

			var viewModel = VisualBoardsTestHelper.CreateSlideshowViewModel(board);
			var provider = viewModel.FactoryProvider;

			var factory1 = provider.GetBoardGUIThreadFactory();
			var factory2 = provider.GetNewBackgroundThreadLoaderFactory("Nope");
			var factory3 = provider.GetNewEditFactory("Yarr");

			AssertType<ReadOnlyBusinessObjectFactory>(factory1);
			AssertType<ReadOnlyBusinessObjectFactory>(factory2);
			AssertType<BusinessObjectFactory>(factory3);

			AssertEquals("Readonly factories don't need validation", true, factory1.IsValidationSuspended);
			AssertEquals("Readonly factories don't need validation", true, factory2.IsValidationSuspended);
			AssertEquals("Editable factories really need validation", false, factory3.IsValidationSuspended);

			AssertEquals("Main thread factory could be used non-transiently, so should be able to receive data updates from other factories", true, factory1.RefreshEnabled);
			AssertEquals("Background thread factory will be used transiently, so doesn't need any updates", false, factory2.RefreshEnabled);
			AssertEquals("Edit factories need to be able to publish their committed changes via data refresh bus", true, factory3.RefreshEnabled);
		}

		public void TestGetAnyFactory_ShouldBeInitialisedWithService()
		{
			var service = new DummyBoardFactoryService();
			var descriptor = new DummySectionDescriptor(service);

			using (ObjectFactory.Substitute("VisualBoardSectionDescriptors", new ArrayList { descriptor }))
			{
				var system = VisualBoardsTestHelper.CreateSystem(Factory);
				var board = VisualBoardsTestHelper.CreateBoard(system);
				var section = board.Sections.AddNew();
				section.MS_SectionType = "FRG";

				var viewModel = VisualBoardsTestHelper.CreateSlideshowViewModel(board);
				var provider = viewModel.FactoryProvider;

				var factoryMethods = provider.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy).Where(m => typeof(BusinessObjectFactory).IsAssignableFrom(m.ReturnType)).ToArray();

				AssertEquals(4, factoryMethods.Length);

				foreach (var method in factoryMethods)
				{
					var args = method.GetParameters().Length == 1 ? new[] { string.Empty } : null;
					var factory = (BusinessObjectFactory)method.Invoke(provider, args);

					CombineAssertions(method.Name, () =>
					{
						AssertNotNull(factory);
						AssertEquals(service, factory.ServiceContainer.GetService<DummyBoardFactoryService>());
					});

					break;
				}
			}
		}

		public void TestGetSecondaryServerConnectionProvider_ShouldNotCacheConnectionProvider()
		{
			var factoryProvider = new SharedBoardFactoryProvider();
			var connectionProvider1 = factoryProvider.GetSecondaryServerConnectionProvider();
			var connectionProvider2 = factoryProvider.GetSecondaryServerConnectionProvider();

			AssertNotEquals("We want to return a new Secondary Connection Provider each time so that changes to secondary server settings can take effect without doing a system upgrade. SAD!", connectionProvider1, connectionProvider2);
		}

		public void TestGetNewBackgroundThreadLoaderFactory_ShouldUseSecondaryConnectionProvider()
		{
			BMSRegistry.Instance.BoardOnSecondaryServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var factoryProvider = new SharedBoardFactoryProvider();
			var providerMock = new Mock<ISecondaryServerConnectionProvider>();
			var cnForReportMock = new Mock<IDbConnectionForReportingWrapper>();

			using (SecondaryServerConnectionProviderProvider.OverrideProvider(providerMock.Object))
			{
				cnForReportMock.Setup(c => c.Connection).Returns(Db.Connection);
				providerMock.Setup(p => p.GetNewConnectionWrapper(It.IsAny<string>(), It.IsAny<string>()))
					.Returns(cnForReportMock.Object);

				var factory = factoryProvider.GetNewBackgroundThreadLoaderFactory("Test");

				AssertNotNull(factory);
				Assert(factory.NameForDebugging == "Test (Secondary)");
				providerMock.Verify(p => p.GetNewConnectionWrapper(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
				cnForReportMock.Verify(c => c.Connection, Times.Once);
			}
		}
	}
}
