using Enterprise.CommissionManagement.Business;
using Enterprise.CommissionManagement.GUI;
using Enterprise.CommissionManagement.Module;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.CommissionManagement.Module
{
	public partial class EDICommissionManagementFilterControl : CommissionManagementFilterControl
	{
		public EDICommissionManagementFilterControl(ViewCommissionLineCollection collection, CommissionManagementFilterBusinessObject strip)
			: base(collection, strip)
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				var companyStringTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
				companyStringTextBoxColumnStyleInfo.ColumnName = "CommissionHeader+AdditionalInfo+EnterpriseDatabaseClientCompanyString";
				companyStringTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
				LinesPreviewPane.LinesGrid.ColumnStyles.Insert(7, companyStringTextBoxColumnStyleInfo);
			}
		}
	}
}
