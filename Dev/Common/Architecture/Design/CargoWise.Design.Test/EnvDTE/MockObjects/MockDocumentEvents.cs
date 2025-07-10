using System;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockDocumentEvents : EnvDTE.DocumentEvents
	{
		public event EnvDTE._dispDocumentEvents_DocumentOpenedEventHandler DocumentOpened;

		public void FireDocumentOpened(EnvDTE.Document document)
		{
			DocumentOpened?.Invoke(document);
		}

		#region _dispDocumentEvents_Event Members

		event EnvDTE._dispDocumentEvents_DocumentClosingEventHandler EnvDTE._dispDocumentEvents_Event.DocumentClosing
		{
			add { throw new Exception("The method or operation is not implemented."); }
			remove { throw new Exception("The method or operation is not implemented."); }
		}

		event EnvDTE._dispDocumentEvents_DocumentOpeningEventHandler EnvDTE._dispDocumentEvents_Event.DocumentOpening
		{
			add { throw new Exception("The method or operation is not implemented."); }
			remove { throw new Exception("The method or operation is not implemented."); }
		}

		event EnvDTE._dispDocumentEvents_DocumentSavedEventHandler EnvDTE._dispDocumentEvents_Event.DocumentSaved
		{
			add { throw new Exception("The method or operation is not implemented."); }
			remove { throw new Exception("The method or operation is not implemented."); }
		}

		#endregion
	}
}
