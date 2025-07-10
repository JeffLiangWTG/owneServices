using System;

using Enterprise.DocumentScanning.Business;
namespace Enterprise.DocumentScanning.GUI
{
	public class SaveFileAsMenuItem : eDocMenuItem
	{
		public SaveFileAsMenuItem(EventHandler onClickHandler)
			: base(Constants.SaveFileAsMenuText, onClickHandler)
		{
		}

		public SaveFileAsMenuItem()
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
			return multipleSelected || selectedElement != null && !selectedElement.SC_IsDeleted;
		}
	}
}
