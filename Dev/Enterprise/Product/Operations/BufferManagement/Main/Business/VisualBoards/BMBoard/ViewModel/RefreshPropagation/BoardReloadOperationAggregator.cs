using System.Collections.Generic;
using System.Linq;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public class BoardReloadOperationAggregator : IBoardRefreshOperationAggregator
	{
		public void AggregateRefresh(IEnumerable<IBoardRefreshable> refreshables, IBoardRefreshContext refreshContext)
		{
			var reloadOperation = (BoardReloadOperation)refreshContext;
			var viewModel = refreshables.OfType<BMBoardSectionViewModel>().FirstOrDefault(); // This check is a magic trick, for only subscribing to DataRefresh if there is a Component section.

			if (viewModel != null && viewModel.DataRefreshBusSubscriberRefreshesTickets)
			{
				reloadOperation.AddDisposableSubscription("01476ac3-1416-4519-b0b2-0b21fe4cd210", dispatcher =>
				{
					var subscriber = new VisualBoardDataRefreshBusSubscriber(viewModel.BoardViewModel, viewModel.FactoryProvider, dispatcher);
					viewModel.BoardViewModel.SlideShowViewModel.DataRefreshBusSubscriber = subscriber;
					return subscriber;
				});
			}
		}
	}
}
