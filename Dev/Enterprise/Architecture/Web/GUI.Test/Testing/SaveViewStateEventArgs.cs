using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class SaveViewStateEventArgs : EventArgs
	{
		public SaveViewStateEventArgs(object savedViewState)
			: base()
		{
			fSavedViewState = savedViewState;
		}

		public object SavedViewState
		{
			get { return fSavedViewState; }
		}
		readonly object fSavedViewState;
	}
}
