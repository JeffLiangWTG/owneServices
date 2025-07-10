using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.JAS.Business;
using Enterprise.Client.JAS.Business.JXC.Export;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Client.JAS.GUI
{
	public partial class PreShipmentExporterForm : ZChildForm, IJXCExportForm
	{
		public PreShipmentExporterForm(JASForwardingShipment shipment)
			: base(new PreShipmentWrapper(shipment))
		{
			InitializeComponent();
		}

		public override string FormHeading
		{
			get { return "Pre-Shipment Exporter Form"; }
		}

		void OKButton_Click(object sender, System.EventArgs e)
		{
			HandleOKButtonClick();
		}

		void HandleOKButtonClick()
		{
			AirOceanMessageExporter exporter = AirOceanMessageExporter.New(PreShipmentWrapper);
			JXCMessageGUIExportDirector exportDirector = GetNewJXCMessageGUIExportDirector(exporter, this);
			if (exportDirector.EnsureMessageCanBeExported())
			{
				exportDirector.Export();
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		protected virtual JXCMessageGUIExportDirector GetNewJXCMessageGUIExportDirector(AirOceanMessageExporter exporter, IJXCExportForm form)
		{
			return new JXCMessageGUIExportDirector(exporter, form);
		}

		PreShipmentWrapper PreShipmentWrapper
		{
			get { return (PreShipmentWrapper)base.BusinessEntity; }
		}

		#region IJXCExportForm Members

		void IJXCExportForm.ValidateAll()
		{
			ValidateAll(ValidationType.Light);
		}

		BusinessObject IJXCExportForm.BusinessEntity
		{
			get { return PreShipmentWrapper; }
		}

		#endregion
	}
}
