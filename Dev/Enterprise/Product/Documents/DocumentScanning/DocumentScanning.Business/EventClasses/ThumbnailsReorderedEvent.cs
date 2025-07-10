using System;

namespace Enterprise.DocumentScanning.Business
{
	public delegate void ThumbnailsReorderEventHandler(object sender, ThumbnailsReorderEventHandlerArgs e);

	public class ThumbnailsReorderEventHandlerArgs : EventArgs
	{
		public ThumbnailsReorderEventHandlerArgs(int[] aNewOrder, int[] aPagesToMove, int aDestIndex)
		{
			fNewOrder = aNewOrder;
			fPagesToMove = aPagesToMove;
			fDestIndex = aDestIndex;
		}

		public int[] NewOrder
		{
			get { return fNewOrder; }
		}
		readonly int[] fNewOrder;

		public int[] PagesToMove
		{
			get { return fPagesToMove; }
		}
		readonly int[] fPagesToMove;

		public int DestIndex
		{
			get { return fDestIndex; }
		}
		readonly int fDestIndex;
	}
}
