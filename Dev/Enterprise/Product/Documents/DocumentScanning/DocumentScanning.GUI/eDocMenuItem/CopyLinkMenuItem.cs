using System;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentScanning.GUI
{
	public class CopyLinkMenuItem : eDocMenuItem
	{
		public CopyLinkMenuItem(EventHandler onClickHandler)
			: base(Constants.CopyLinkMenuText, onClickHandler)
		{
		}

		public CopyLinkMenuItem()
			: this(null)
		{
		}

		protected override bool IsDependentOnGridEditable
		{
			get { return false; }
		}

		protected override bool IsDependentOnViewable
		{
			get { return true; }
		}

		public override bool GetEnabledStatus(StorageDocsBase selectedElement, bool multipleSelected)
		{
			Func<bool> elementNotDeleted = () => selectedElement != null && !selectedElement.SC_IsDeleted;
			Func<bool> isSavedInDatabase = () => Globals.IsTest || selectedElement.IsInDatabase;

			return !multipleSelected && elementNotDeleted() && isSavedInDatabase();
		}
	}
}
