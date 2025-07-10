using System;

using Enterprise.DocumentScanning.Business;
namespace Enterprise.DocumentScanning.GUI
{
	public class DeleteMenuItem : eDocMenuItem
	{
		public DeleteMenuItem(EventHandler onClickHandler)
			: base(Constants.DeleteMenuText, onClickHandler)
		{
		}

		public DeleteMenuItem()
			: this(null)
		{
		}

		public override bool GetEnabledStatus(StorageDocsBase selectedElement, bool multipleSelected)
		{
			return multipleSelected || selectedElement != null && !selectedElement.SC_IsDeleted;
		}
	}
}
