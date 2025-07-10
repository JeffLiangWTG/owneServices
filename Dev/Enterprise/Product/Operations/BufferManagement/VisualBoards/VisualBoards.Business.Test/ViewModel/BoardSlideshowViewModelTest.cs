using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using NUnit.Framework;

namespace Enterprise.VisualBoards.Business.Test
{
	[TestedType(typeof(BoardSlideshowViewModel))]
	public class BMBoardSlideshowViewModelTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBoardViewModels()
		{
			AssertEquals(3, viewModel.BoardViewModels.Length);

			AssertEquals(system1.PK, viewModel.BoardViewModels[0].SystemPK);
			AssertEquals(board1.PK, viewModel.BoardViewModels[0].BoardPK);
			AssertEquals(10, viewModel.BoardViewModels[0].IntervalInSlideShowSeconds);

			AssertEquals(system2.PK, viewModel.BoardViewModels[1].SystemPK);
			AssertEquals(board2.PK, viewModel.BoardViewModels[1].BoardPK);
			AssertEquals(20, viewModel.BoardViewModels[1].IntervalInSlideShowSeconds);

			AssertEquals(system2.PK, viewModel.BoardViewModels[2].SystemPK);
			AssertEquals(board3.PK, viewModel.BoardViewModels[2].BoardPK);
			AssertEquals(17, viewModel.BoardViewModels[2].IntervalInSlideShowSeconds);
		}

		public void TestNavigation()
		{
			AssertEquals(viewModel.BoardViewModels[0], viewModel.CurrentBoardViewModel);

			viewModel.MoveForward();
			AssertEquals(viewModel.BoardViewModels[1], viewModel.CurrentBoardViewModel);

			viewModel.MoveBackwards();
			AssertEquals(viewModel.BoardViewModels[0], viewModel.CurrentBoardViewModel);

			viewModel.MoveBackwards();
			AssertEquals(viewModel.BoardViewModels[2], viewModel.CurrentBoardViewModel);

			viewModel.MoveBackwards();
			AssertEquals(viewModel.BoardViewModels[1], viewModel.CurrentBoardViewModel);

			viewModel.MoveForward();
			AssertEquals(viewModel.BoardViewModels[2], viewModel.CurrentBoardViewModel);

			viewModel.MoveForward();
			AssertEquals(viewModel.BoardViewModels[0], viewModel.CurrentBoardViewModel);
		}

		public void TestRefreshBoardViewModel()
		{
			var system1 = VisualBoardsTestHelper.CreateSystem(Factory);
			system1.FS_Name = "Team Valor System";
			var system2 = VisualBoardsTestHelper.CreateSystem(Factory);
			system2.FS_Name = "Team Instinct System";
			var board = VisualBoardsTestHelper.CreateBoard(system1, "Pokemon Trainer Board");

			Factory.Save();

			var viewModel = VisualBoardsTestHelper.CreateSlideshowViewModel(board);

			AssertEquals(system1.PK, viewModel.CurrentBoardViewModel.SystemPK);

			board.MB_FS_System = system2.PK;
			Factory.Save();

			viewModel.ReloadCurrentBoardViewModel();

			AssertEquals("Should have reloaded the view model and changed the system", system2.PK, viewModel.CurrentBoardViewModel.SystemPK);
		}

		#region Thread Bashing

		public void TestReplaceService_WhenMultipleThreadsAccessingServices_ShouldNotThrowExceptions()
		{
			const int numberOfServices = 100;
			var services = Enumerable.Range(0, numberOfServices).Select(_ => new DummyBoardFactoryService { StalenessPolicy = BoardServiceStalenessPolicy.StaleBeforeBoardRefresh }).ToArray();
			var factoryProvider = viewModel.FactoryProvider;

			viewModel.ReplaceService(services[0]);

			var preConditionFactory = factoryProvider.GetNewBackgroundThreadLoaderFactory("Nyaaaa");
			AssertNotNull("Pre-condition: factory created through the viewModel.FactoryProvider should have a service already", preConditionFactory.ServiceContainer.GetService<DummyBoardFactoryService>());

			AssertNoExceptionThrown(() =>
			{
				var task = Task.Factory.StartNew(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						for (var i = 1; i < numberOfServices; i++)
						{
							foreach (var service in services)
							{
								viewModel.ReplaceService(service);
							}
						}
					}
				});

				for (var i = 0; i < numberOfServices; i++)
				{
					var anotherFactory = factoryProvider.GetNewBackgroundThreadLoaderFactory("Bleep bloop " + i);

					AssertNotNull(anotherFactory.ServiceContainer.GetService<DummyBoardFactoryService>());
				}

				task.Wait();
			});
		}

		public void TestCloneViewModel_WhenMultipleThreadsAccessingServices_ShouldNotThrowExceptions()
		{
			const int numberOfServices = 100;
			var services = Enumerable.Range(0, numberOfServices).Select(_ => new DummyBoardFactoryService { StalenessPolicy = BoardServiceStalenessPolicy.StaleBeforeBoardRefresh }).ToArray();
			var servicesInnards = (HashSet<IBoardFactoryService>)viewModel.GetType().GetField("services", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(viewModel);
			var factoryProvider = viewModel.FactoryProvider;

			foreach (var service in services)
			{
				servicesInnards.Add(service);
			}

			var preConditionFactory = factoryProvider.GetNewBackgroundThreadLoaderFactory("Nyaaaa");
			AssertNotNull("Pre-condition: factory created through the viewModel.FactoryProvider should have a service already", preConditionFactory.ServiceContainer.GetService<DummyBoardFactoryService>());

			AssertNoExceptionThrown(() =>
			{
				var task = Task.Factory.StartNew(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						for (var i = 1; i < numberOfServices; i++)
						{
							viewModel.Clone();
						}
					}
				});

				for (var i = 0; i < numberOfServices; i++)
				{
					Thread.Sleep(10);
					lock (viewModel.ServiceLocker)
					{
						servicesInnards.Remove(services[i]);
					}
				}

				task.Wait();
			});
		}

		public void TestRefreshServices_WhenMultipleThreadsAccessingServices_ShouldNotThrowExceptions()
		{
			const int numberOfServices = 100;
			var services = Enumerable.Range(0, numberOfServices).Select(_ => new DummyBoardFactoryService { StalenessPolicy = BoardServiceStalenessPolicy.StaleBeforeBoardRefresh }).ToArray();
			var factoryProvider = viewModel.FactoryProvider;

			viewModel.ReplaceService(services[0]);

			var preConditionFactory = factoryProvider.GetNewBackgroundThreadLoaderFactory("Nyaaaa");
			AssertNotNull("Pre-condition: factory created through the viewModel.FactoryProvider should have a service already", preConditionFactory.ServiceContainer.GetService<DummyBoardFactoryService>());

			AssertNoExceptionThrown(() =>
			{
				var task = Task.Factory.StartNew(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						for (var i = 1; i < numberOfServices; i++)
						{
							viewModel.RefreshServices(BoardServiceStalenessPolicy.StaleBeforeBoardRefresh);
						}
					}
				});

				for (var i = 0; i < numberOfServices; i++)
				{
					foreach (var service in services)
					{
						viewModel.ReplaceService(service);
					}
				}

				task.Wait();
			});
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.FillWithValidTestData();
			Factory.Save();

			return VisualBoardsTestHelper.CreateSlideshowViewModel(board);
		}

		protected override void SetUp()
		{
			base.SetUp();

			system1 = Factory.NewWithValidTestData<BMSystem>();
			board1 = system1.Boards.AddNew();
			board1.FillWithValidTestData();

			system2 = Factory.NewWithValidTestData<BMSystem>();
			board2 = system2.Boards.AddNew();
			board2.FillWithValidTestData();
			board3 = system2.Boards.AddNew();
			board3.FillWithValidTestData();

			var slideshow = VisualBoardsTestHelper.CreateSlideshow(Factory, Tuple.Create(board1, 10), Tuple.Create(board2, 20), Tuple.Create(board3, 17));

			Factory.Save();

			viewModel = VisualBoardsTestHelper.CreateSlideshowViewModel(slideshow);
		}

		BoardSlideshowViewModel viewModel;
		BMSystem system1;
		BMSystem system2;
		BMBoard board1;
		BMBoard board2;
		BMBoard board3;

		#endregion
	}
}
