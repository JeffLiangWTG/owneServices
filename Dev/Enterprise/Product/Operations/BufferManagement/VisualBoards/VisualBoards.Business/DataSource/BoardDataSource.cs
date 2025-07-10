using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.VisualBoards.Business
{
	public class BoardDataSource
	{
		internal BoardDataSource(BoardViewModel boardViewModel, IBMBoard board)
		{
			this.boardViewModel = boardViewModel;

			SectionsAndViewModels = GetSectionsAndViewModels(board).ToArray();

			boardViewModel.Sections.Clear();

			foreach (var pair in SectionsAndViewModels)
			{
				boardViewModel.Sections.Add(pair.ViewModel);
			}

			foreach (var viewModel in boardViewModel.Sections)
			{
				viewModel.Initialise();
			}
		}

		readonly BoardViewModel boardViewModel;

		BoardFactoryProvider FactoryProvider => boardViewModel.FactoryProvider;

		public IEnumerable<BoardSectionViewModelPair> SectionsAndViewModels { get; }

		IEnumerable<BoardSectionViewModelPair> GetSectionsAndViewModels(IBMBoard board)
		{
			foreach (var section in board.Sections.OrderBy(s => s.Row).ThenBy(s => s.Column))
			{
				var descriptor = SectionDescriptorProvider.Get(section.MS_SectionType);
				var viewModel = descriptor?.GetViewModel(section, boardViewModel) ?? new BoardSectionViewModel(section, boardViewModel);

				yield return new BoardSectionViewModelPair(section, viewModel);
			}
		}

		public void RelinquishThreadOwnershipIfOwned()
		{
			MaybeAdjustOwnershipOnRelevantFactories(ThreadSentryAction.RelinquishOwnership);
		}

		public void TakeThreadOwnershipIfNotOwned()
		{
			MaybeAdjustOwnershipOnRelevantFactories(ThreadSentryAction.TakeOwnership);
		}

		void MaybeAdjustOwnershipOnRelevantFactories(ThreadSentryAction action)
		{
			foreach (var factory in MonitoredFactories)
			{
				if (factory != null)
				{
					switch (action)
					{
						case ThreadSentryAction.RelinquishOwnership:
							if (factory.ThreadSentry.IsOwner)
							{
								factory.ThreadSentry.RelinquishThreadOwnership();
							}
							break;

						case ThreadSentryAction.TakeOwnership:
							if (!factory.ThreadSentry.IsOwner)
							{
								factory.ThreadSentry.TakeThreadOwnership();
							}
							break;
					}
				}
			}
		}

		IEnumerable<BusinessObjectFactory> MonitoredFactories
		{
			get
			{
				yield return SectionsAndViewModels.Select(x => x.Section.Factory).FirstOrDefault();
				yield return FactoryProvider.GetBoardGUIThreadFactory();
				yield return boardViewModel.SlideShowViewModel.Source.Factory;
			}
		}

		enum ThreadSentryAction
		{
			RelinquishOwnership,
			TakeOwnership,
		}
	}

	public class BoardSectionViewModelPair
	{
		public BoardSectionViewModelPair(IBMBoardSection section, BoardSectionViewModel viewModel)
		{
			Section = section;
			ViewModel = viewModel;
		}

		public IBMBoardSection Section { get; }
		public BoardSectionViewModel ViewModel { get; }
	}
}
