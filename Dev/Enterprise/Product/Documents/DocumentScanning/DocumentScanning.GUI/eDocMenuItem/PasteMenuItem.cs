using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentScanning.Business;

namespace Enterprise.DocumentScanning.GUI
{
	public class PasteMenuItem : eDocMenuItem
	{
		public PasteMenuItem(EventHandler onClickHandler)
			: base(Constants.PasteMenuText, onClickHandler)
		{
		}

		public PasteMenuItem()
			: this(null)
		{
		}

		public override bool GetEnabledStatus(StorageDocsBase selectedElement, bool multipleSelected)
		{
			if (selectedElement != null)
			{
				return !selectedElement.SC_IsDeleted && !selectedElement.SC_IsSystemGenerated;
			}

			return true;
		}

		public override bool GetEnabledStatusWhenMultipleElementsSelected(BusinessObject[] selectedElements)
		{
			return false;
		}
	}
}
