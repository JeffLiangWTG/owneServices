using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class CommercialInvoiceDetailsUserControl : ZUserControl
	{
		public CommercialInvoiceDetailsUserControl()
		{
			InitializeComponent();
			SetPropertiesLostByDesign();
		}

		void SetPropertiesLostByDesign()
		{
			BindingSource.SetBindingMember(NoteTextLongTextControl, nameof(JobComInvoiceHeader.JZ_Remarks));
		}
	}
}
