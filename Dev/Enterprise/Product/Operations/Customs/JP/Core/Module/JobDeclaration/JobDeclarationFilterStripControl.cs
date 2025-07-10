using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.JP.Module
{
	public partial class JobDeclarationFilterStripControl : Customs.Module.JobDeclarationFilterStripControl
	{
		public JobDeclarationFilterStripControl(JobDeclarationModule module, IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(module, gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			InitializeGridColumns();
		}

		void InitializeGridColumns()
		{
			var messageStatusColumnStyle = FilteredGrid.GetColumnStyle("JE_MessageStatus");
			messageStatusColumnStyle.GroupName = Res.GetData("444e6e07-992c-43ea-9cca-431bddc86123", "Message Status");
			messageStatusColumnStyle.CaptionResourceString = null;

			var messageStatusDescriptionColumnStyle = FilteredGrid.GetColumnStyle("JE_MessageStatusDescription");
			messageStatusDescriptionColumnStyle.GroupName = Res.GetData("444e6e07-992c-43ea-9cca-431bddc86123", "Message Status");
			messageStatusDescriptionColumnStyle.CaptionResourceString = null;

			var importerStatusColumnStyle = FilteredGrid.GetColumnStyle("JE_OH_Importer");
			importerStatusColumnStyle.CaptionResourceString = Res.GetData("ae02bfbb-249f-4fbf-a20e-122770ac6676", "Importer/Consignee");
			importerStatusColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);

			var supplierStatusColumnStyle = FilteredGrid.GetColumnStyle("JE_OH_Supplier");
			supplierStatusColumnStyle.CaptionResourceString = Res.GetData("d2b22ffb-31f9-4505-9a98-c09e20cec6ec", "Shipper/Exporter");
			supplierStatusColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
		}
	}
}
