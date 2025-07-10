using System;

using Enterprise.DocumentScanning.Business;

namespace Enterprise.DocumentScanning.GUI
{
	public class AllocateMenuItem : eDocMenuItem
	{
		public AllocateMenuItem(EventHandler onClickHandler)
			: base(Constants.AllocateMenuText, onClickHandler)
		{
		}

		public AllocateMenuItem()
			: this(null)
		{
		}

		public override bool GetEnabledStatus(StorageDocsBase selectedElement, bool multipleSelected)
		{
			if (multipleSelected)
			{
				return true;
			}
			else if (selectedElement != null)
			{
				return selectedElement is StorageDocsUnallocated && !selectedElement.SC_IsDeleted;
			}
			return false;
		}
	}
}
