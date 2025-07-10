using System;

using Enterprise.DocumentScanning.Business;
namespace Enterprise.DocumentScanning.GUI
{
	public class ViewMenuItem : eDocMenuItem
	{
		public ViewMenuItem(EventHandler onClickHandler)
			: base(Constants.ViewMenuText, onClickHandler)
		{
		}

		public ViewMenuItem()
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

		public override bool GetEnabledStatus(Enterprise.DocumentScanning.Business.StorageDocsBase selectedElement, bool multipleSelected)
		{
			return selectedElement != null && !multipleSelected;
		}
	}
}
