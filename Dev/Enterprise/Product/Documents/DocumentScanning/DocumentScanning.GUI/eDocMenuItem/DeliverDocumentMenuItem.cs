using System;

using Enterprise.DocumentScanning.Business;
namespace Enterprise.DocumentScanning.GUI
{
	public class DeliverDocumentMenuItem : eDocMenuItem
	{
		public DeliverDocumentMenuItem(EventHandler onClickHandler)
			: base(Constants.DeliverDocumentMenuText, onClickHandler)
		{
		}

		public DeliverDocumentMenuItem()
			: this(null)
		{
		}

		protected override bool IsDependentOnGridEditable
		{
			get { return false; }
		}

		public override bool GetEnabledStatus(StorageDocsBase selectedElement, bool multipleSelected)
		{
			if (multipleSelected)
			{
				return true;
			}
			else
			{
				return selectedElement != null && !selectedElement.SC_IsDeleted;
			}
		}
	}
}
