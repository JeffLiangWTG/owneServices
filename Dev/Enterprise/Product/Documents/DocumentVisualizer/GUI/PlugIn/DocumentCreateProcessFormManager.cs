using System;
using System.Threading;
using CargoWise.Data;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentVisualizer.GUI
{
	public class DocumentVisualizerProgressManager : ZProcessStatusFormManager<DocumentVisualizerProgressForm>, IProgressManager
	{
		public DocumentVisualizerProgressManager()
		{
			otherFormShownDetector = ZFormModaliser.SetTemporaryDelegateToCallBeforeShowingFormsOrDialogs(frm =>
			{
				if (popupForm != null
					&& !popupForm.IsDisposed)
				{
					HideForm();
					Thread.Sleep(1); // give a chance to the popup thread to execute and hide the popup form
					popupForm = null;

					otherFormShownDetector.Dispose();
					otherFormShownDetector = null;
				}
			});
		}

		IDisposable otherFormShownDetector;
		DocumentVisualizerProgressForm popupForm;

		protected override DocumentVisualizerProgressForm CreateFormCore()
		{
			using (Db.DisposableActionForDbConnection())
			{
				popupForm = base.CreateFormCore();
				return popupForm;
			}
		}

		#region IDisposable members

		void IDisposable.Dispose()
		{
			otherFormShownDetector?.Dispose();
			otherFormShownDetector = null;

			popupForm = null;

			base.Dispose();
		}

		#endregion

		#region IProgressManager memebers

		void IProgressManager.UpdateStatus(string message) => UpdateStatus(message, 0);

		#endregion
	}
}
