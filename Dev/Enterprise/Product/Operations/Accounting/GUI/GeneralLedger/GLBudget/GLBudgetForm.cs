using System.ComponentModel;
using Enterprise.Accounting.Business.GeneralLedger.GLBudget;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI
{
	public partial class GLBudgetForm : ZForm
	{
		ZGuidFindBox AU_AGBoundFindBox;
		ZGuidFindBox AU_GBBoundFindBox;
		ZGuidFindBox AU_GEBoundFindBox;
		ZCalcEdit AU_OpeningBoundCalcEdit;
		ZDropEdit AU_AllocationTypeBoundDropDownEdit;
		ZCalcEdit AU_AllocationValueBoundCalcEdit;
		ZCalcEdit AU_AllocationIncrementBoundCalcEdit;
		ZCalcEdit AU_Calc_TotalAmountBoundReadOnlyCalcEdit;
		ZPanel GuidPanel;
		ZPanel AllocationPanel;
		ZGrid AccGLBudgetLinesBoundGrid;
		ZYearEdit AU_YearBoundYearEdit;
		ZTemplateTabControl zTabControl1;
		ZTabPage Budget;
		ZStmNoteTabPage zStmNoteTabPage1;
		ZLogsTabPage zEventTabPage1;
		ZCalcEdit TotalPercentageCalcEdit;
		ZPanel BottomPanel;
		Core.Forms.ZPostingButtonsUserControl PostingButtonsControl;
		IContainer components;

		public GLBudgetForm()
		{
			Initialise();
		}

		public GLBudgetForm(GLBudget businessEntity) : base(businessEntity)
		{
			Initialise();
			PlugIns.Add(ControllerIDs.Audit);
		}

		void Initialise()
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsControl);
			this.AutoAddPreviousNextButtons = true;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}
	}
}

