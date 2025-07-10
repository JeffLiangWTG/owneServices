using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class AirCTOHawbExportForm : ZTemplateForm
	{
		public AirCTOHawbExportForm()
		{
		}

		public AirCTOHawbExportForm(ExportCustomsManifestLines line)
			: base(line)
		{
			PlugIns.Add(ControllerIDs.DocAddresses);
			PlugIns.AddJobInvoicing(line.InvoicingSupporter);
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
