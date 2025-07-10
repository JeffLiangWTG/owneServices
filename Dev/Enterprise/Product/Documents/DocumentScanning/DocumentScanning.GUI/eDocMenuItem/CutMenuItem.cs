using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentScanning.Business;

namespace Enterprise.DocumentScanning.GUI
{
	public class CutMenuItem : eDocMenuItem
	{
		public CutMenuItem(EventHandler onClickHandler)
			: base(Constants.CutMenuText, onClickHandler)
		{
		}

		public CutMenuItem()
			: this(null)
		{
		}

		protected override bool IsDependentOnViewable
		{
			get { return true; }
		}

		public override bool GetEnabledStatus(StorageDocsBase selectedElement, bool multipleSelected)
		{
			if (multipleSelected)
			{
				return true;
			}
			else if (selectedElement != null)
			{
				return !selectedElement.SC_IsDeleted && !selectedElement.SC_IsSystemGenerated;
			}

			return false;
		}

		public override bool GetEnabledStatusWhenMultipleElementsSelected(BusinessObject[] selectedElements)
		{
			foreach (BusinessObject obj in selectedElements)
			{
				StorageDocsBase document = obj as StorageDocsBase;
				if (document != null && (document.IsDeleted || document.SC_IsSystemGenerated))
				{
					return false;
				}
			}
			return true;
		}
	}
}
