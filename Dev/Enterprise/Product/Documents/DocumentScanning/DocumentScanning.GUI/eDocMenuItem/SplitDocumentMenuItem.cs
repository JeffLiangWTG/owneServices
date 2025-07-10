using System;
using Enterprise.DocumentScanning.Business;

namespace Enterprise.DocumentScanning.GUI
{
	public class SplitDocumentMenuItem : eDocMenuItem
	{
		public SplitDocumentMenuItem(EventHandler onClickHandler)
			: base(Constants.SplitDocumentMenuText, onClickHandler)
		{
		}

		public SplitDocumentMenuItem()
			: this(null)
		{
		}

		protected override bool IsDependentOnViewable
		{
			get { return true; }
		}

		public override bool GetEnabledStatus(StorageDocsBase selectedElement, bool multipleSelected)
		{
			if (selectedElement != null && (selectedElement.EDocFormat == Core.Constants.FileFormats.TIF || selectedElement.EDocFormat == Core.Constants.FileFormats.PDF))
			{
				return !multipleSelected && !selectedElement.SC_IsDeleted && selectedElement.IsInDatabase;
			}

			return false;
		}
	}
}
