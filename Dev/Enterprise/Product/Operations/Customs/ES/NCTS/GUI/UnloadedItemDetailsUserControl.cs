using System;
using System.Windows.Forms;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public partial class UnloadedItemDetailsUserControl : EU.NCTS.GUI.UnloadedItemDetailsUserControl, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public UnloadedItemDetailsUserControl()
		{
			InitializeComponent();
			SetUpColumnsToSupportingDocumentsGrid();
		}

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return (control.Name == "OriginalStatisticalValueCalcEdit" && previousControl.Name == "UnloadedStatisticalValueCalcEdit")
				|| (control.Name == "UnloadedStatisticalValueCalcEdit" && previousControl.Name == "OriginalStatisticalValueCalcEdit");
		}

		NctsHeader currentHeader;

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			UnhookEvents();
			base.OnCurrentDataItemChanged(e);
			HookEvents();
		}

		#region Hook / Unhook Events

		void HookEvents()
		{
			currentHeader = (NctsHeader)CurrentDataItem;
			if (currentHeader != null)
			{
				currentHeader.ESNctsHeader.CEN_PreviousSummaryDeclarationInfo.ValueChanged += CEN_PreviousSummaryDeclarationInfo_ValueChanged;
				currentHeader.BH_HeaderTypeInfo.ValueChanged += BH_HeaderTypeInfo_ValueChanged;

				BH_HeaderTypeInfo_ValueChanged(currentHeader, null);
				CEN_PreviousSummaryDeclarationInfo_ValueChanged(currentHeader.ESNctsHeader, null);
			}
		}

		void UnhookEvents()
		{
			if (currentHeader != null)
			{
				currentHeader.ESNctsHeader.CEN_PreviousSummaryDeclarationInfo.ValueChanged -= CEN_PreviousSummaryDeclarationInfo_ValueChanged;
				currentHeader.BH_HeaderTypeInfo.ValueChanged -= BH_HeaderTypeInfo_ValueChanged;
			}
		}

		#endregion

		void SetGoodsItemDifferencesGridsReadOnly(NctsHeader header)
		{
			if (header?.IsArrivalMovement ?? false)
			{
				var (matchingResult, _) = header.FindRelevantDepartureRecordForCombinedDepartureAndArrival();

				var departureFound = matchingResult == NctsHeader.DepartureRecordFindResult.FoundByMatchingMrn ||
									 matchingResult == NctsHeader.DepartureRecordFindResult.AlreadyCombinedDepartureAndArrival;

				OriginalContainersGrid.ReadOnly = departureFound;
				OriginalPackagesGrid.ReadOnly = departureFound;
				OriginalSupportingDocumentsGrid.ReadOnly = departureFound;
			}
		}

		void BH_HeaderTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetGoodsItemDifferencesGridsReadOnly((NctsHeader)sender);
		}

		void CEN_PreviousSummaryDeclarationInfo_ValueChanged(object sender, EventArgs e)
		{
			var esNctsHeader = (CusESNctsHeader)sender;
			BillOfLadingVisibility(esNctsHeader.CEN_PreviousSummaryDeclaration.IsEmpty);
		}

		void BillOfLadingVisibility(bool previousSummaryEmpty)
		{
			BillOfLadingTextBox.Visible = !previousSummaryEmpty;
		}

		void SetUpColumnsToSupportingDocumentsGrid()
		{
			using (OriginalSupportingDocumentsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				OriginalSupportingDocumentsGrid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo()
				{
					ColumnName = NctsSupportingDocument.Schema.CSI_LineNo,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40),
					CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("4E563EB6-F4CC-4539-BED7-CB40B14E54E1", "Seq. N.", "Seq. Number", "Sequence Number"),
				});
			}

			using (UnloadedSupportingDocumentsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				var unloadedDescriptionColumnStyle = UnloadedSupportingDocumentsGrid.GetColumnStyle(NctsSupportingDocument.Schema.CSI_Description);
				unloadedDescriptionColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			}
		}
	}
}
