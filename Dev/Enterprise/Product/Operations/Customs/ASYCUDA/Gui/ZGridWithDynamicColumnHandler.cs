using System;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public class ZGridWithDynamicColumnHandler : ZGrid
	{
		public ZGridWithDynamicColumnHandler() : this(() => { })
		{
		}

		public ZGridWithDynamicColumnHandler(Action doColumnStuffHandler)
			: base()
		{
			doColumnStuff = doColumnStuffHandler;
		}
		readonly Action doColumnStuff;

		protected override void RefreshTableStylesCore()
		{
			base.RefreshTableStylesCore();
			if (doColumnStuff != null)
			{
				doColumnStuff();
			}
		}
	}
}
