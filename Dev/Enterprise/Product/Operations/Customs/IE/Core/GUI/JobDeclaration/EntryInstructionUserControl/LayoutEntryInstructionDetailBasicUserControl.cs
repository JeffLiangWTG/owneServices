using System;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public class LayoutEntryInstructionDetailBasicUserControl : Customs.GUI.LayoutEntryInstructionDetailBasicUserControl
	{
		protected override void UpdateControlsAfterLayoutUpdated(BaseJobDeclaration declaration)
		{
			base.UpdateControlsAfterLayoutUpdated(declaration);

			var requestedDocumentsGroupBox = dynamicDetailsPanel.Find(x => string.Equals(x.Name, "RequestedDocumentsGroupBox", StringComparison.Ordinal)).FirstOrDefault();

			var columnSeparator0 = dynamicDetailsPanel.Find(x => string.Equals(x.Name, "ColumnSeparator0", StringComparison.Ordinal)).FirstOrDefault();
			if (columnSeparator0 != null)
			{
				requestedDocumentsGroupBox?.AllowOverlap(columnSeparator0);
			}

			var columnSeparator1 = dynamicDetailsPanel.Find(x => string.Equals(x.Name, "ColumnSeparator1", StringComparison.Ordinal)).FirstOrDefault();
			if (columnSeparator1 != null)
			{
				requestedDocumentsGroupBox?.AllowOverlap(columnSeparator1);
			}
		}
	}
}
