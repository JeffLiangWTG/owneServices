#if DEBUG

using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.Statements
{
	public partial class StatementPrintForm
	{
		public ZDateEdit CutoffDateEdit_ForTestOnly
		{
			get { return CutoffDateEdit; }
			set { CutoffDateEdit = value; }
		}

		public ZArchitecture.ZCalcEdit OutstandingAmountCalcEdit_ForTestOnly
		{
			get { return OutstandingAmountCalcEdit; }
			set { OutstandingAmountCalcEdit = value; }
		}

		public ZPeriodEdit CutoffPeriodEdit_ForTestOnly
		{
			get { return CutoffPeriodEdit; }
			set { CutoffPeriodEdit = value; }
		}

		public ZCheckBox DisbursementInvoicesCheckBox_ForTestOnly
		{
			get { return DisbursementInvoicesCheckBox; }
			set { DisbursementInvoicesCheckBox = value; }
		}

		public ZGroupBox OrganisationGroupBox_ForTestOnly
		{
			get { return OrganisationGroupBox; }
			set { OrganisationGroupBox = value; }
		}

		public ZGuidFindBox OrganisationGuidFindBox_ForTestOnly
		{
			get { return OrganisationGuidFindBox; }
			set { OrganisationGuidFindBox = value; }
		}

		public ZGuidFindBox TransactionBranchFindBox_ForTestOnly
		{
			get { return TransactionBranchFindBox; }
			set { TransactionBranchFindBox = value; }
		}

		public ZGuidFindBox DepartmentGuidFindBox_ForTestOnly
		{
			get { return DepartmentGuidFindBox; }
			set { DepartmentGuidFindBox = value; }
		}

		public void FormCancelButton_Click_ForTestOnly(object sender, System.EventArgs e)
		{
			FormCancelButton_Click(sender, e);
		}

		public void PrintButton_Click_ForTestOnly(object sender, System.EventArgs e)
		{
			PrintButton_Click(sender, e);
		}
	}
}

#endif
