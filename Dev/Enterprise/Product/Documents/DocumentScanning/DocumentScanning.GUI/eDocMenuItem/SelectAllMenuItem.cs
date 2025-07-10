using System;

using Enterprise.DocumentScanning.Business;
namespace Enterprise.DocumentScanning.GUI
{
	public class SelectAllMenuItem : eDocMenuItem
	{
		public SelectAllMenuItem(EventHandler onClickHandler)
			: base(Constants.SelectAllMenuText, onClickHandler)
		{
		}

		public SelectAllMenuItem()
			: this(null)
		{
		}

		protected override bool IsDependentOnGridEditable
		{
			get { return false; }
		}

		public override bool GetEnabledStatus(StorageDocsBase selectedElement, bool multipleSelected)
		{
			return true;
		}
	}
}
