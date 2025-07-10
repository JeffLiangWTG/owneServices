using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.H7.GUI
{
	public class H7PreviousDocumentsUserControl : RelatedDocumentsUserControlWithGrid
	{
		public H7PreviousDocumentsUserControl() : base(ResStringPreviousDocuments, PreviousDocumentsTabSequence)
		{
		}

		protected override void AdditionalInfosGridColumnsVisible()
		{
			using (Grid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				var codeColumnStyle = Grid.GetColumnStyle(AutoCusSupportingInfo.Schema.CSI_Code);

				if (codeColumnStyle != null && codeColumnStyle is not ZDropEditColumnStyleInfo)
				{
					Grid.ColumnStyles.Remove(codeColumnStyle);
					Grid.ColumnStyles.Add(new ZDropEditColumnStyleInfo(AutoCusSupportingInfo.Schema.CSI_Code, 80));
				}

				base.AdditionalInfosGridColumnsVisible();
			}
		}

		protected override void SetAdditionalInfosLayout()
		{
			DetailsLayoutControl.SetLayout(new H7PreviousDocumentDetailsLayout());
		}

		protected override IReadOnlyList<string> OrderedColumns => new[]
		{
			AutoCusSupportingInfo.Schema.CSI_Code,
			DocumentDescription,
			AutoCusSupportingInfo.Schema.CSI_ReferenceNumber,
		};
	}
}
