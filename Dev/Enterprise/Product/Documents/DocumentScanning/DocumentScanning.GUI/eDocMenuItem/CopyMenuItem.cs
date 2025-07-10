using System;
using Enterprise.DocumentScanning.Business;

namespace Enterprise.DocumentScanning.GUI
{
	public class CopyMenuItem : eDocMenuItem
	{
		public CopyMenuItem(EventHandler onClickHandler)
			: base(Constants.CopyMenuText, onClickHandler)
		{
		}

		public CopyMenuItem()
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
