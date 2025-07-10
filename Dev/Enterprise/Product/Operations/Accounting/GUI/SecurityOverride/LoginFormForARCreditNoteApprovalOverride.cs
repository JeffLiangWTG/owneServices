using System.Collections.Generic;
using Enterprise.MasterFiles.GUI;
using static Enterprise.Accounting.GUI.InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD;

namespace Enterprise.Accounting.GUI
{
	public partial class LoginFormForARCreditNoteApprovalOverride : LoginForm
	{
		public LoginFormForARCreditNoteApprovalOverride(List<BranchDepartmentPair> firstLevelSecurityRequiredBranchDepartment,
														List<BranchDepartmentPair> secondLevelSecurityRequiredBranchDepartment,
														List<BranchDepartmentPair> thirdLevelSecurityRequiredBranchDepartment,
														List<BranchDepartmentPair> fourthLevelSecurityRequiredBranchDepartment,
														List<BranchDepartmentPair> fifthLevelSecurityRequiredBranchDepartment,
														List<BranchDepartmentPair> sixthLevelSecurityRequiredBranchDepartment)
		{
			FirstLevelSecurityRequiredBranchDepartment = firstLevelSecurityRequiredBranchDepartment;
			SecondLevelSecurityRequiredBranchDepartment = secondLevelSecurityRequiredBranchDepartment;
			ThirdLevelSecurityRequiredBranchDepartment = thirdLevelSecurityRequiredBranchDepartment;
			FourthLevelSecurityRequiredBranchDepartment = fourthLevelSecurityRequiredBranchDepartment;
			FifthLevelSecurityRequiredBranchDepartment = fifthLevelSecurityRequiredBranchDepartment;
			SixthLevelSecurityRequiredBranchDepartment = sixthLevelSecurityRequiredBranchDepartment;
			InitializeComponent();
		}

		public List<BranchDepartmentPair> FirstLevelSecurityRequiredBranchDepartment
		{
			get => firstLevelSecurityRequiredBranchDepartment ?? (firstLevelSecurityRequiredBranchDepartment = new List<BranchDepartmentPair>());
			set => firstLevelSecurityRequiredBranchDepartment = value;
		}
		List<BranchDepartmentPair> firstLevelSecurityRequiredBranchDepartment;

		public List<BranchDepartmentPair> SecondLevelSecurityRequiredBranchDepartment
		{
			get => secondLevelSecurityRequiredBranchDepartment ?? (secondLevelSecurityRequiredBranchDepartment = new List<BranchDepartmentPair>());
			set => secondLevelSecurityRequiredBranchDepartment = value;
		}
		List<BranchDepartmentPair> secondLevelSecurityRequiredBranchDepartment;

		public List<BranchDepartmentPair> ThirdLevelSecurityRequiredBranchDepartment
		{
			get => thirdLevelSecurityRequiredBranchDepartment ?? (thirdLevelSecurityRequiredBranchDepartment = new List<BranchDepartmentPair>());
			set => thirdLevelSecurityRequiredBranchDepartment = value;
		}
		List<BranchDepartmentPair> thirdLevelSecurityRequiredBranchDepartment;

		public List<BranchDepartmentPair> FourthLevelSecurityRequiredBranchDepartment
		{
			get => fourthLevelSecurityRequiredBranchDepartment ?? (fourthLevelSecurityRequiredBranchDepartment = new List<BranchDepartmentPair>());
			set => fourthLevelSecurityRequiredBranchDepartment = value;
		}
		List<BranchDepartmentPair> fourthLevelSecurityRequiredBranchDepartment;

		public List<BranchDepartmentPair> FifthLevelSecurityRequiredBranchDepartment
		{
			get => fifthLevelSecurityRequiredBranchDepartment ?? (fifthLevelSecurityRequiredBranchDepartment = new List<BranchDepartmentPair>());
			set => fifthLevelSecurityRequiredBranchDepartment = value;
		}
		List<BranchDepartmentPair> fifthLevelSecurityRequiredBranchDepartment;

		public List<BranchDepartmentPair> SixthLevelSecurityRequiredBranchDepartment
		{
			get => sixthLevelSecurityRequiredBranchDepartment ?? (sixthLevelSecurityRequiredBranchDepartment = new List<BranchDepartmentPair>());
			set => sixthLevelSecurityRequiredBranchDepartment = value;
		}
		List<BranchDepartmentPair> sixthLevelSecurityRequiredBranchDepartment;

		protected override void OKButton_Click(object sender, System.EventArgs e)
		{
			fCredentials = new ARCreditNoteApprovalAlternativeCredentials(LoginTextBox.Text, PasswordTextBox.Text,
											FirstLevelSecurityRequiredBranchDepartment, SecondLevelSecurityRequiredBranchDepartment,
											ThirdLevelSecurityRequiredBranchDepartment, FourthLevelSecurityRequiredBranchDepartment,
											FifthLevelSecurityRequiredBranchDepartment, SixthLevelSecurityRequiredBranchDepartment);
			Close();
		}

#if DEBUG
		public override void DoLoginForTest(string loginName, string password)
		{
			fCredentials = new ARCreditNoteApprovalAlternativeCredentials(loginName, password,
											FirstLevelSecurityRequiredBranchDepartment, SecondLevelSecurityRequiredBranchDepartment,
											ThirdLevelSecurityRequiredBranchDepartment, FourthLevelSecurityRequiredBranchDepartment,
											FifthLevelSecurityRequiredBranchDepartment, SixthLevelSecurityRequiredBranchDepartment);
		}
#endif
	}
}
