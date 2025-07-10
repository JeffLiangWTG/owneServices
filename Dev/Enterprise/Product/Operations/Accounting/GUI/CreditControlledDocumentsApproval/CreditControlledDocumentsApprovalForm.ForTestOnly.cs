#if DEBUG

using System;
using CargoWise.Windows.UI;
using Enterprise.DocumentScanning.GUI;
using Enterprise.DocumentScanning.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class CreditControlledDocumentsApprovalForm
	{
		public void OnApplyButtonClick_ForTestOnly(object sender, EventArgs e)
		{
			OnApplyButtonClick(sender, e);
		}

		public Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl_ForTestOnly
		{
			get { return PostingButtonsUserControl; }
			set { PostingButtonsUserControl = value; }
		}

		public ZPanel TopGridPanel_ForTestOnly
		{
			get { return TopGridPanel; }
			set { TopGridPanel = value; }
		}

		public ZPanel TopSingleRequestPanel_ForTestOnly
		{
			get { return TopSingleRequestPanel; }
			set { TopSingleRequestPanel = value; }
		}

		public ZArchitecture.ZTextBox ReasonDescriptionTextBox_ForTestOnly
		{
			get { return ReasonDescriptionTextBox; }
			set { ReasonDescriptionTextBox = value; }
		}

		public ZTabPage DetailTabPage_ForTestOnly
		{
			get { return detailTabPage; }
			set { detailTabPage = value; }
		}

		public ZTabPage EDocsTabPage_ForTestOnly
		{
			get { return eDocsTabPage; }
			set { eDocsTabPage = value; }
		}

		public eDocsPlugIn EDocPlugIn_ForTestOnly
		{
			get { return eDocPlugIn; }
			set { eDocPlugIn = value; }
		}

		public eDocsUserControl EdocUserControl_ForTestOnly
		{
			get { return edocUserControl; }
			set { edocUserControl = value; }
		}

		public Business.CreditControlledDocumentsApprovalBulk Approvals_ForTestOnly => Approvals;

		public ZButton OpenJobButton_ForTestOnly
		{
			get { return openJobButton; }
			set { openJobButton = value; }
		}

		public ZForm LastShownJobForm_ForTestOnly
		{
			get { return LastShownJobForm; }
			set { LastShownJobForm = value; }
		}

		public ZArchitecture.ZGrid OrgInBreachGrid_ForTestOnly
		{
			get { return orgInBreachGrid; }
			set { orgInBreachGrid = value; }
		}

		public ZTabControl MainTabControl_ForTestOnly
		{
			get { return mainTabControl; }
			set { mainTabControl = value; }
		}

		public ZGuidDropEdit OrgZGuidDropEdit_ForTestOnly
		{
			get { return orgZGuidDropEdit; }
			set { orgZGuidDropEdit = value; }
		}

		public ZTabPage CreditTabPage_ForTestOnly
		{
			get { return creditTabPage; }
			set { creditTabPage = value; }
		}

		public ZArchitecture.ZTextBox DepartmentTextBox_ForTestOnly
		{
			get { return departmentTextBox; }
			set { departmentTextBox = value; }
		}

		public ZPanel IncoTermPanel_ForTestOnly
		{
			get { return incoTermPanel; }
			set { incoTermPanel = value; }
		}

		public ZArchitecture.ZTextBox ApprovalLevelTextBox_ForTestOnly
		{
			get { return approvalLevelTextBox; }
			set { approvalLevelTextBox = value; }
		}

		public KBindingSource BindingSource_ForTestOnly => BindingSource;
	}
}

#endif
