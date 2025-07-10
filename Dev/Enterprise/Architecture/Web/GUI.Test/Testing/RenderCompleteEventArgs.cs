using System;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class RenderCompleteEventArgs : EventArgs
	{
		public RenderCompleteEventArgs(ZString renderedOutput)
			: base()
		{
			fRenderedOutput = renderedOutput;
		}

		public ZString RenderedOutput
		{
			get { return fRenderedOutput; }
		}
		readonly ZString fRenderedOutput;
	}
}
