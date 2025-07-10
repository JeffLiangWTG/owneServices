using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	public partial class EDIOrganisationFilterControl : OrganisationFilterControl
	{
		public EDIOrganisationFilterControl()
		{
			InitializeComponent();
		}

		public EDIOrganisationFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: this(gridCollection, filterBusinessObject, OrgModuleType.Standard)
		{
		}

		public EDIOrganisationFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject, OrgModuleType moduleType)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			SetupContextMenu();
			if (!DesignModeFinder.IsDesigning)
			{
				SetCustomizeColumnCaption();
			}
		}

		void SetupContextMenu()
		{
			this.Grid.ContextMenu.MenuItems.Add("-");
			this.Grid.ContextMenu.MenuItems.Add(new ZMenuItem("Bulk Add Discounts", BulkActionHandler));
		}

		void BulkActionHandler(object sender, EventArgs e)
		{
			if (Grid.SelectedElements.Length > 0)
			{
				var bizO = new BulkAddDiscountBizO(Grid.SelectedElements);
				ZFormModaliser.ShowDialogAndDispose(new BulkAddDiscountForm(bizO));
			}
		}

		void SetCustomizeColumnCaption()
		{
			foreach (var columnStyle in grid.ColumnStyles)
			{
				var calcEditColumnStyleInfo = columnStyle as ZArchitecture.ZCalcEditColumnStyleInfo;
				if (calcEditColumnStyleInfo != null)
				{
					if (calcEditColumnStyleInfo.ColumnName == "MiscServ+OM_CMAcheivableClientRevenue")
					{
						calcEditColumnStyleInfo.Caption = EDIDataRegistry.Instance.AchievableBusinessLabel.Value;
					}
					else if (calcEditColumnStyleInfo.ColumnName == "MiscServ+OM_CMNoOfEmployees")
					{
						calcEditColumnStyleInfo.Caption = EDIDataRegistry.Instance.NumberOfEmployeesLabel.Value;
					}
					else if (calcEditColumnStyleInfo.ColumnName == "MiscServ+OM_CMAmountOfBusinessWon")
					{
						calcEditColumnStyleInfo.Caption = EDIDataRegistry.Instance.AmountOfBusinessWonLabel.Value;
					}
					else if (calcEditColumnStyleInfo.ColumnName == "MiscServ+OM_CMTotalClientRevenue")
					{
						calcEditColumnStyleInfo.Caption = EDIDataRegistry.Instance.TotalClientRevenueLabel.Value;
					}
					else if (calcEditColumnStyleInfo.ColumnName == "MiscServ+OM_CMWarehouseRevenue")
					{
						calcEditColumnStyleInfo.Caption = EDIDataRegistry.Instance.WarehouseRevenueLabel.Value;
					}
					else if (calcEditColumnStyleInfo.ColumnName == "MiscServ+OM_CMConsultingRevenue")
					{
						calcEditColumnStyleInfo.Caption = EDIDataRegistry.Instance.ConsultingRevenueLabel.Value;
					}
					else if (calcEditColumnStyleInfo.ColumnName == "MiscServ+OM_CMPaidUpCapital")
					{
						calcEditColumnStyleInfo.Caption = EDIDataRegistry.Instance.PaidUpCapitalLabel.Value;
					}
				}
			}
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new EDIOrganisationFilterStrip();
		}
	}
}
