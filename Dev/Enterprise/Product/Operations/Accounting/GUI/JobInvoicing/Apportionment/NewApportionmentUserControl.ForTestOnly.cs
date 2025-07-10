#if DEBUG

using System;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting
{
	public partial class NewApportionmentUserControl
	{
		public ZArchitecture.ZGrid CostSummaryGrid_ForTestOnly
		{
			get { return CostSummaryGrid; }
			set { CostSummaryGrid = value; }
		}

		public ZPanel ExtraTaxPanel_ForTestOnly
		{
			get { return ExtraTaxPanel; }
			set { ExtraTaxPanel = value; }
		}

		public void OnLoad_ForTestOnly(EventArgs e)
		{
			OnLoad(e);
		}

		public ZTabPage CostRateAuditTabPage_ForTestOnly
		{
			get { return CostRateAuditTabPage; }
			set { CostRateAuditTabPage = value; }
		}

		public ZDropEdit PlaceOfSupplyDropEdit_ForTestOnly
		{
			get { return PlaceOfSupplyDropEdit; }
			set { PlaceOfSupplyDropEdit = value; }
		}

		public CalculationXMLUserControl AuditLogNoteUserControl_ForTestOnly
		{
			get { return auditLogNoteUserControl; }
			set { auditLogNoteUserControl = value; }
		}

		public ZArchitecture.ZGrid ApportionedChargesGrid_ForTestOnly
		{
			get { return ApportionedChargesGrid; }
			set { ApportionedChargesGrid = value; }
		}

		public ZCheckBox IncludeOnCollectCheckBox_ForTestOnly
		{
			get { return IncludeOnCollectCheckBox; }
			set { IncludeOnCollectCheckBox = value; }
		}

		public ZCalcFindBox ExtraTaxAmountCalcFindBox_ForTestOnly
		{
			get { return ExtraTaxAmountCalcFindBox; }
			set { ExtraTaxAmountCalcFindBox = value; }
		}

		public ZTextBox CostRateAuditTextBox_ForTestOnly
		{
			get { return CostRateAuditTextBox; }
			set { CostRateAuditTextBox = value; }
		}

		public ZStmNotePopupButton AutoRatingNotePopupButton_ForTestOnly
		{
			get { return AutoRatingNotePopupButton; }
			set { AutoRatingNotePopupButton = value; }
		}

		public ZDateEdit TaxDateEdit_ForTestOnly
		{
			get { return TaxDateEdit; }
			set { TaxDateEdit = value; }
		}

		public ZTabPage DetailTabPage_ForTestOnly
		{
			get { return DetailTabPage; }
			set { DetailTabPage = value; }
		}
	}
}

#endif
