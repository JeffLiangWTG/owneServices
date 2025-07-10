using System;

using Enterprise.DocumentScanning.Business;
namespace Enterprise.DocumentScanning.GUI
{
	public class DeletePermanentlyMenuItem : eDocMenuItem
	{
		public DeletePermanentlyMenuItem(EventHandler onClickHandler)
			: base(Constants.DeletePermanentlyMenuText, onClickHandler)
		{
		}

		public DeletePermanentlyMenuItem()
			: this(null)
		{
		}

		public override bool GetEnabledStatus(StorageDocsBase selectedElement, bool multipleSelected)
		{
			return multipleSelected || selectedElement != null;
		}
	}
}
