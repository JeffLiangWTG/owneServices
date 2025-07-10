using Enterprise.Customs.DE.Business.CusTempStorage;

namespace Enterprise.Customs.DE.GUI
{
	public partial class CHGOFFDeclarationUserControl : CHGBaseDeclarationUserControl
	{
		public CHGOFFDeclarationUserControl()
		{
			InitializeComponent();
			var info = LinesGrid.GetColumnStyle(CusTempStorageLine.Schema.TSL_LocationOfGoods);
			if (info != null)
			{
				LinesGrid.ColumnStyles.Remove(info);
			}
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

		string[] LinesGridDefaultColumnsInSortOrder
		{
			get
			{
				if (linesGridDefaultColumnsInSortOrder == null)
				{
					linesGridDefaultColumnsInSortOrder = new[]
					{
						CusTempStorageLine.Schema.TSL_LineNo,
						CusTempStorageLine.Schema.TSL_OwnerReferenceType,
						CusTempStorageLine.Schema.CustodianOrgPK,
						CusTempStorageLine.Schema.TSL_OA_Custodian,
						CusTempStorageLine.Schema.TSL_CustodianIdentifier,
						CusTempStorageLine.Schema.TSL_CustodianIdentifierBranchNo,
						CusTempStorageLine.Schema.GoodsOwnerOrgPK,
						CusTempStorageLine.Schema.TSL_OA_GoodsOwner,
						CusTempStorageLine.Schema.TSL_GoodsOwnerIdentifier,
						CusTempStorageLine.Schema.TSL_GoodsOwnerIdentifierBranchNo,
						CusTempStorageLine.Schema.TSL_CustomsStatus
					};
				}
				return linesGridDefaultColumnsInSortOrder;
			}
		}
		string[] linesGridDefaultColumnsInSortOrder;
	}
}
