using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Module;

namespace Enterprise.Customs.CA.Module
{
	public partial class CusClassificationFilterControl : Customs.Module.CusClassificationFilterControl
	{
		public CusClassificationFilterControl()
		{
			InitializeComponent();
		}

		public CusClassificationFilterControl(IBusinessObjectCollection gridCollection, CusClassificationFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			ChangeGridDetails();
		}

		void ChangeGridDetails()
		{
			using (FilteredGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				FilteredGrid.SetColumnWidth(CusClassification.Schema.CC_LookupCode, 90);
				FilteredGrid.SetColumnWidth(CusClassification.Schema.CC_Description, 200);
				FilteredGrid.SetColumnWidth(CusClassification.Schema.CC_IsActive, 70);
				FilteredGrid.SetColumnWidth(CusClassification.Schema.CC_LastAuditedUser, 90);
				FilteredGrid.SetColumnWidth(CusClassification.Schema.CC_LastAuditedDate, 95);
				FilteredGrid.RemoveFromAvailableColumns(CusClassification.Schema.CC_TariffNum);
			}
		}
	}
}
