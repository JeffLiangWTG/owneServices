using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class PackingUserControl : BasePackingControl
	{
		public PackingUserControl()
		{
			InitializeComponent();
			HouseBillsGroupBox.Text = "Master and House Bills";
			new UNDGDataItemFormManager(PackingDetailsGrid).Initialize();
			PackingDetailsGrid.GridId = "GridLayoutio4uPJ92QiJBDALlyBv5Jw==";
		}

		#region Grid Column Layout

		protected override void ChangeGridColumnsVisibility()
		{
			string houseBillCaption = JobDeclaration.IsPost ? "Parcel Post Number" : "Bill Num";
			HouseBillsGrid.SetColumnCaption(CusDecHouseBillSchema.Constants.CU_BillNum, houseBillCaption);
			HouseBillsGrid.SetAvailability(JobDeclaration.IsPartShipConsignmentReferenceRelevant, Bill.Schema.CU_fPartShipConsignmentReference);

			HouseBillsGrid.SetAvailability(!JobDeclaration.IsPost, [CusDecHouseBillSchema.Constants.CU_BillType, Customs.Business.Bill.Schema.CU_ParentBillUniqueCode]);
			PackingDetailsGrid.SetAvailability(JobDeclaration.IsSea, Customs.Business.BasePackage.Schema.CW_ContainerNoOrEquipmentNo);

			string houseBillMasterBillCaption = JobDeclaration.IsPost ? "Parcel Post Number" : "Linked Bill(Lowest Bill)";
			PackingDetailsGrid.SetColumnCaption(Customs.Business.BasePackage.Schema.CW_HouseBill, houseBillMasterBillCaption);

			base.ChangeGridColumnsVisibility();
		}

		#endregion

		#region Control Visiblity and Settings

		new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
		}

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();
			JobDeclaration aUDeclaration = JobDeclaration;
			bool packingVisible = aUDeclaration.IsImport && !JobDeclaration.IsExWarehouse && !aUDeclaration.IsTransportModeOther && !aUDeclaration.IsSAC;

			HouseBillPanel.Dock = packingVisible && JobDeclaration.IsImportCMR ? DockStyle.Top : DockStyle.Fill;
			HouseBillPanel.Visible = packingVisible;
			PackingDetailsPanel.Visible = packingVisible && JobDeclaration.IsImportCMR;
			ExportLabel.Visible = !packingVisible;
			ExportLabel.Dock = DockStyle.Fill;
			HouseBillsGroupBox.Text = JobDeclaration.IsPost ? "Parcel Post Numbers" : "Master and House Bills";
		}

		#endregion

		#region Packing Details Grid

		void PackingDetailsGrid_Click(object sender, EventArgs e)
		{
			if (PackingDetailsGrid.CurrentRowIndex >= 0)
			{
				int columnNumber = PackingDetailsGrid.CurrentCell.ColumnNumber;
				if (columnNumber >= 0 && columnNumber < PackingDetailsGrid.Columns.Count &&
					PackingDetailsGrid.Columns[columnNumber].ColumnStyle.MappingName == "CW_CargoStatus" &&
					PackingDetailsGrid.GetCurrentCellBounds().Contains(PackingDetailsGrid.PointToClient(MousePosition)))
				{
					int rownumber = PackingDetailsGrid.CurrentCell.RowNumber;
					if (JobDeclaration != null && rownumber >= 0 && rownumber < JobDeclaration.Packages.Count)
					{
						Package pack = JobDeclaration.Packages[rownumber];
						if (pack != null && pack.PackingGroup != null && !pack.PackingGroup.CR_CargoStatus.IsEmpty)
						{
							bool includeAdditionalDSAStatusSection = true;
							Globals.Message.ShowInformation(pack.PackingGroup.LineConsolidatedCargoStatusDescription(ref includeAdditionalDSAStatusSection));
						}
					}
				}
			}
		}

		#endregion
	}
}
