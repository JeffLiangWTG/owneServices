using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class LocalExportDetailsUserControl : ZUserControl
	{
		public LocalExportDetailsUserControl()
		{
			InitializeComponent();
			InitializeLongTextControlBindings();
			this.SetTariffFindBoxDelegates(TariffFindBox);
		}

		void InitializeLongTextControlBindings()
		{
			BindingSource.SetBindingMember(DescriptionLongTextControl, JobComInvoiceLine.Schema.JI_Description);
		}
	}
}
