using System;

using Enterprise.DocumentScanning.Business;

namespace Enterprise.DocumentScanning.GUI
{
	public class UnallocateMenuItem : eDocMenuItem
	{
		public UnallocateMenuItem(EventHandler onClickHandler)
			: base(Constants.UnallocateMenuText, onClickHandler)
		{
		}

		public UnallocateMenuItem()
			: this(null)
		{
		}

		public override bool GetEnabledStatus(StorageDocsBase selectedElement, bool multipleSelected)
		{
			if (multipleSelected)
			{
				return true;
			}
			else if (selectedElement != null && selectedElement.IsImageFile)
			{
				return !(selectedElement is StorageDocsUnallocated) && selectedElement.IsAllocated && !selectedElement.SC_IsDeleted && !selectedElement.SC_IsSystemGenerated;
			}

			return false;
		}
	}
}
