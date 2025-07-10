using System;
using System.Windows.Forms;

namespace Enterprise.DocumentEngine.GUI.DocumentDelivery
{
	interface IDocumentDeliveryView
	{
		void HideLanguageSelectionDropDown();
		void HidePageRangesPanel();
		void HideBackgroundDeliveryCheckBox();

		event EventHandler SaveAsButtonClicked;
		event FormClosedEventHandler ViewClosed;
		SaveAsFileInfo GetSaveAsFileName();
	}
}
