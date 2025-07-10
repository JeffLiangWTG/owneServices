using System;

using Enterprise.DocumentScanning.Business;
namespace Enterprise.DocumentScanning.GUI
{
	public class RestoreMenuItem : eDocMenuItem
	{
		public RestoreMenuItem(EventHandler onClickHandler)
			: base(Constants.RestoreMenuText, onClickHandler)
		{
		}

		public RestoreMenuItem()
			: this(null)
		{
		}

		public override bool GetEnabledStatus(StorageDocsBase selectedElement, bool multipleSelected)
		{
			return multipleSelected || selectedElement != null && selectedElement.SC_IsDeleted;
		}
	}
}
