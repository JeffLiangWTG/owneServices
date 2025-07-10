using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
{
	public partial class PreviousDocumentsDetailsLayoutControl : ZUserControl, ISupportMultipleResourceStringDataSupporter, ISupportMultipleResourceStringData
	{
		public PreviousDocumentsDetailsLayoutControl()
		{
			InitializeComponent();
		}

		public void SetLayout(IPanelLayoutProvider layout)
		{
			DetailsPanel.UpdateLayout(layout);
		}

		public ISupportMultipleResourceStringData SupportMultipleResourceStringData => this;

		public IReadOnlyList<string> MultipleKeysToUse => IsBoundToEntryInstructions ? new[] { Constants.CusEntryInstruction.PreviousDocumentCaption } : System.Array.Empty<string>();

		protected bool IsBoundToEntryInstructions => this.DataMember == CusEntryInstructionInfoBindingMemberName;
		const string CusEntryInstructionInfoBindingMemberName = nameof(JobDeclaration.CustomsEntryInstructions) + "." + nameof(JobComInvoiceLine.PreviousDocuments);
	}
}
