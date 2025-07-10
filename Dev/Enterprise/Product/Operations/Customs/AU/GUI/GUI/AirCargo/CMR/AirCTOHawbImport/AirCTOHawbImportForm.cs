using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class AirCTOHawbImportForm : ZTemplateForm
	{
		public AirCTOHawbImportForm()
		{
		}

		public AirCTOHawbImportForm(CTOCusHAWB businessEntity)
			: base(businessEntity)
		{
			PlugIns.AddJobInvoicing(businessEntity.InvoicingSupporter);
		}

		public override string FormCaption
		{
			get { return "Master Bill"; }
		}

		protected override bool SupportsEDocs
		{
			get { return false; }
		}

		protected override bool ShowNotesTab
		{
			get { return false; }
		}

		public override bool IsResizableByTabPageAllowed => true;
	}
}
