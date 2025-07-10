using Enterprise.Customs.DE.Business.CusTempStorage;

namespace Enterprise.Customs.DE.GUI
{
	public partial class CHGTSTDeclarationUserControl : CHGBaseDeclarationUserControl
	{
		public CHGTSTDeclarationUserControl()
		{
			InitializeComponent();
			InitializeGrid();
			RemoveLinesGridColumn(CusTempStorageLine.Schema.GoodsOwnerOrgPK);
			RemoveLinesGridColumn(CusTempStorageLine.Schema.TSL_OA_GoodsOwner);
			RemoveLinesGridColumn(CusTempStorageLine.Schema.TSL_GoodsOwnerIdentifier);
			RemoveLinesGridColumn(CusTempStorageLine.Schema.TSL_GoodsOwnerIdentifierBranchNo);
		}

		protected void InitializeGrid()
		{
			var zDropEditColumnStyleInfo = new ZArchitecture.GUI.ZDropEditColumnStyleInfo()
			{
				ColumnName = CHGTSTCusTempStorageDec.Schema.NewCustodianBranch,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(132)
			};
			DecsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo);
			DecsGrid.ReOrderColumns(DecsGridefaultColumnsInSortOrder);
		}

		protected override void SetControlVisibility(bool isAWBDeclaration)
		{
			AWBDeclarationUserControl.Visible = isAWBDeclaration;
			REGDeclarationUserControl.Visible = !isAWBDeclaration;
		}

		protected override void SetColumnAvailability(bool isAWBDeclaration)
		{
			LinesGrid.SetAllAvailability(true);
			if (isAWBDeclaration)
			{
				LinesGrid.RemoveFromAvailableColumns(CusTempStorageLine.Schema.TSL_LineNo);
			}
			else
			{
				LinesGrid.RemoveFromAvailableColumns(CusTempStorageLine.Schema.TSL_OwnerReferenceType,
					CusTempStorageLine.Schema.TSL_OA_Custodian,
					CusTempStorageLine.Schema.TSL_CustodianIdentifier,
					CusTempStorageLine.Schema.TSL_CustodianIdentifierBranchNo,
					CusTempStorageLine.Schema.CustodianOrgPK);
			}
			LinesGrid.ReOrderColumns(LinesGridDefaultColumnsInSortOrder);
		}

		string[] DecsGridefaultColumnsInSortOrder
		{
			get
			{
				if (decsGridefaultColumnsInSortOrder == null)
				{
					decsGridefaultColumnsInSortOrder = new[]
					{
						CHGTSTCusTempStorageDec.Schema.STH_IdentificationIndicator,
						CHGTSTCusTempStorageDec.Schema.NewCustodianBranch,
						CHGTSTCusTempStorageDec.Schema.STH_SystemCreateTimeUtc,
						CHGTSTCusTempStorageDec.Schema.STH_MessageStatus
					};
				}
				return decsGridefaultColumnsInSortOrder;
			}
		}
		string[] decsGridefaultColumnsInSortOrder;

		string[] LinesGridDefaultColumnsInSortOrder
		{
			get
			{
				if (linesGridDefaultColumnsInSortOrder == null)
				{
					linesGridDefaultColumnsInSortOrder = new string[]
					{
						CusTempStorageLine.Schema.TSL_LineNo,
						CusTempStorageLine.Schema.TSL_OwnerReferenceType,
						CusTempStorageLine.Schema.CustodianOrgPK,
						CusTempStorageLine.Schema.TSL_OA_Custodian,
						CusTempStorageLine.Schema.TSL_CustodianIdentifier,
						CusTempStorageLine.Schema.TSL_CustodianIdentifierBranchNo,
						CusTempStorageLine.Schema.TSL_LocationOfGoods,
						CusTempStorageLine.Schema.TSL_CustomsStatus
					};
				}
				return linesGridDefaultColumnsInSortOrder;
			}
		}
		string[] linesGridDefaultColumnsInSortOrder;

		void RemoveLinesGridColumn(string columnName)
		{
			var info = LinesGrid.GetColumnStyle(columnName);
			if (info != null)
			{
				LinesGrid.ColumnStyles.Remove(info);
			}
		}
	}
}
