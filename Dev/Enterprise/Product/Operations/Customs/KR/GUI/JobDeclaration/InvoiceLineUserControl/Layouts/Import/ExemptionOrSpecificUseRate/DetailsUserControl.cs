using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class DetailsUserControl : ZUserControl
	{
		public DetailsUserControl()
		{
			InitializeComponent();

			this.BindingSource.DataSourceType = typeof(Business.JobDeclaration);
			Controls.ChangeBindingPaths(BindingSource, BindingPathForDeclaration);
			SetReadOnly();
			SetPropertiesLostByDesign();
		}

		void SetPropertiesLostByDesign()
		{
			BindingSource.SetBindingMember(RemarkLongTextControl, "FilteredInvoiceLines.AdditionalInformationContent");
		}

		void SetReadOnly()
		{
			DutyReductionCodeFindBox.ReadOnly = true;
			InstalmentCodeFindBox.ReadOnly = true;
			SpecificUseCheckBox.ReadOnly = true;
		}

		public void BindToMessageSendingObject()
		{
			BindingSource.DataSourceType = typeof(Business.JobDeclarationMiscMessageSendingObjectParent);
			Controls.ChangeBindingPaths(BindingSource, BindingPathForMessageSending);

			DutyReductionTypeDropEdit.ReadOnly = true;
			SpecificUseCheckBox.ReadOnly = true;
			DutyReductionCodeFindBox.ReadOnly = true;
			InstalmentCodeFindBox.ReadOnly = true;
			RemarkTextBox.ReadOnly = true;
		}

		const string BindingPathForMessageSending = "SendingObjectsCollection.MessageSendingEntryLines.InvoiceLine.";
		const string BindingPathForDeclaration = "FilteredInvoiceLines.";
	}
}
