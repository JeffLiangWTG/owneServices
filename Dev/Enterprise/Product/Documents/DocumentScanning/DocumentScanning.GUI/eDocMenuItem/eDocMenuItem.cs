using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI
{
	public abstract class eDocMenuItem : ZMenuItem
	{
		public eDocMenuItem(MultilingualString name, EventHandler onClickHandler)
			: base(name, onClickHandler)
		{
		}

		public void UpdateVisibility(StorageDocsBase elementAtMouse, BusinessObject[] selectedElements, bool isGridEditable, bool isViewable)
		{
			bool multipleSelected = selectedElements.Length > 1;
			bool newValue = GetEnabledStatus(elementAtMouse, multipleSelected);

			if (multipleSelected)
			{
				newValue = newValue && GetEnabledStatusWhenMultipleElementsSelected(selectedElements);
			}

			if (IsDependentOnGridEditable)
			{
				newValue = newValue && isGridEditable;
			}

			if (IsDependentOnViewable)
			{
				newValue = newValue && isViewable;
			}

			if (Enabled != newValue)
			{
				Enabled = newValue;
			}
		}

		protected virtual bool IsDependentOnGridEditable
		{
			get { return true; }
		}

		protected virtual bool IsDependentOnViewable
		{
			get { return false; }
		}

		public abstract bool GetEnabledStatus(StorageDocsBase selectedElement, bool multipleSelected);

		public virtual bool GetEnabledStatusWhenMultipleElementsSelected(BusinessObject[] selectedElements)
		{
			return true;
		}
	}
}
