using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentScanning.Business;
namespace Enterprise.DocumentScanning.GUI
{
	public class ReviewParsedResultsMenuItem : eDocMenuItem
	{
		public ReviewParsedResultsMenuItem(EventHandler onClickHandler)
			: base(Constants.ReviewParsedResultsMenuText, onClickHandler)
		{
		}

		public ReviewParsedResultsMenuItem()
			: this(null)
		{
		}

		public override bool GetEnabledStatus(StorageDocsBase selectedElement, bool multipleSelected)
		{
			if (multipleSelected)
			{
				return false;
			}
			else
			{
				return selectedElement != null && !selectedElement.SC_IsDeleted && selectedElement.CanReviewParsedResult;
			}
		}

		public override bool GetEnabledStatusWhenMultipleElementsSelected(BusinessObject[] selectedElements)
		{
			return false;
		}
	}
}
