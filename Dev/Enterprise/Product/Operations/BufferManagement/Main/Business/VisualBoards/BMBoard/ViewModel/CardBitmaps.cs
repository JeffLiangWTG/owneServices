using System;
using System.Drawing;
using CargoWise.Common;

namespace Enterprise.BufferManagement.Business
{
	public sealed class CardBitmaps : IDisposable
	{
		public CardBitmaps(Image normalBitmap, Image darkenedBitmap)
		{
			Argument.NotNull(normalBitmap, nameof(normalBitmap));
			Argument.NotNull(darkenedBitmap, nameof(darkenedBitmap));

			NormalBitmap = normalBitmap;
			DarkenedBitmap = darkenedBitmap;
		}

		public Image NormalBitmap { get; }

		public Image DarkenedBitmap { get; }

		public bool IsDisposed { get; private set; }

		public void Dispose()
		{
			if (!IsDisposed)
			{
				IsDisposed = true;

				if (!NormalBitmap.IsDisposed())
				{
					NormalBitmap.Dispose();
				}

				if (!DarkenedBitmap.IsDisposed())
				{
					DarkenedBitmap.Dispose();
				}
			}
		}
	}
}
