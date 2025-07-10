using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Accounting.GUI.InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD;

namespace Enterprise.Accounting.GUI
{
	public partial class LoginFormWithTwoCredentialSupportBranchDepartmentLevel : LoginForm
	{
		public AlternativeCredentials Credentials2
		{
			get { return fCredentials2; }
		}
		AlternativeCredentials fCredentials2;

#if DEBUG

		public void DoLogin2ForTest(string loginName, string password)
		{
			fCredentials2 = new ARCreditNoteApprovalAlternativeCredentials(loginName, password,
												FirstLevelSecurityRequiredBranchDepartment, SecondLevelSecurityRequiredBranchDepartment,
												ThirdLevelSecurityRequiredBranchDepartment, FourthLevelSecurityRequiredBranchDepartment,
												FifthLevelSecurityRequiredBranchDepartment, SixthLevelSecurityRequiredBranchDepartment);
		}

		public void DoLogin1ForTest(string loginName, string password)
		{
			fCredentials = new ARCreditNoteApprovalAlternativeCredentials(loginName, password,
												FirstLevelSecurityRequiredBranchDepartment, SecondLevelSecurityRequiredBranchDepartment,
												ThirdLevelSecurityRequiredBranchDepartment, FourthLevelSecurityRequiredBranchDepartment,
												FifthLevelSecurityRequiredBranchDepartment, SixthLevelSecurityRequiredBranchDepartment);
		}

#endif

		public LoginFormWithTwoCredentialSupportBranchDepartmentLevel(List<BranchDepartmentPair> firstLevelSecurityRequiredBranchDepartment,
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

		public void PopulateFirstCredentialWithCurrentStaff()
		{
			LoginTextBox.Text = GlbStaff.CurrentUser.GS_LoginName;
			LoginTextBox.ReadOnly = true;
			PasswordTextBox.Text = "*********";
			PasswordTextBox.ReadOnly = true;

#if DEBUG
			if (Globals.IsTest)
			{
				prePopulatedFirstLoginUser = LoginTextBox.Text;
			}
#endif
		}

#if DEBUG
		internal string prePopulatedFirstLoginUser;
#endif
		public List<BranchDepartmentPair> FirstLevelSecurityRequiredBranchDepartment
		{
			get
			{
				if (firstLevelSecurityRequiredBranchDepartment == null)
				{
					firstLevelSecurityRequiredBranchDepartment = new List<BranchDepartmentPair>();
				}

				return firstLevelSecurityRequiredBranchDepartment;
			}
			set
			{
				firstLevelSecurityRequiredBranchDepartment = value;
			}
		}
		List<BranchDepartmentPair> firstLevelSecurityRequiredBranchDepartment;
		public List<BranchDepartmentPair> SecondLevelSecurityRequiredBranchDepartment
		{
			get
			{
				if (secondLevelSecurityRequiredBranchDepartment == null)
				{
					secondLevelSecurityRequiredBranchDepartment = new List<BranchDepartmentPair>();
				}

				return secondLevelSecurityRequiredBranchDepartment;
			}
			set
			{
				secondLevelSecurityRequiredBranchDepartment = value;
			}
		}
		List<BranchDepartmentPair> secondLevelSecurityRequiredBranchDepartment;

		public List<BranchDepartmentPair> ThirdLevelSecurityRequiredBranchDepartment
		{
			get
			{
				if (thirdLevelSecurityRequiredBranchDepartment == null)
				{
					thirdLevelSecurityRequiredBranchDepartment = new List<BranchDepartmentPair>();
				}

				return thirdLevelSecurityRequiredBranchDepartment;
			}
			set
			{
				thirdLevelSecurityRequiredBranchDepartment = value;
			}
		}
		List<BranchDepartmentPair> thirdLevelSecurityRequiredBranchDepartment;

		public List<BranchDepartmentPair> FourthLevelSecurityRequiredBranchDepartment
		{
			get
			{
				if (fourthLevelSecurityRequiredBranchDepartment == null)
				{
					fourthLevelSecurityRequiredBranchDepartment = new List<BranchDepartmentPair>();
				}

				return fourthLevelSecurityRequiredBranchDepartment;
			}
			set
			{
				fourthLevelSecurityRequiredBranchDepartment = value;
			}
		}
		List<BranchDepartmentPair> fourthLevelSecurityRequiredBranchDepartment;

		public List<BranchDepartmentPair> FifthLevelSecurityRequiredBranchDepartment
		{
			get
			{
				if (fifthLevelSecurityRequiredBranchDepartment == null)
				{
					fifthLevelSecurityRequiredBranchDepartment = new List<BranchDepartmentPair>();
				}

				return fifthLevelSecurityRequiredBranchDepartment;
			}
			set
			{
				fifthLevelSecurityRequiredBranchDepartment = value;
			}
		}
		List<BranchDepartmentPair> fifthLevelSecurityRequiredBranchDepartment;

		public List<BranchDepartmentPair> SixthLevelSecurityRequiredBranchDepartment
		{
			get
			{
				if (sixthLevelSecurityRequiredBranchDepartment == null)
				{
					sixthLevelSecurityRequiredBranchDepartment = new List<BranchDepartmentPair>();
				}

				return sixthLevelSecurityRequiredBranchDepartment;
			}
			set
			{
				sixthLevelSecurityRequiredBranchDepartment = value;
			}
		}
		List<BranchDepartmentPair> sixthLevelSecurityRequiredBranchDepartment;

		protected override void OKButton_Click(object sender, System.EventArgs e)
		{
			bool hasError = false;

			if (!string.IsNullOrEmpty(LoginTextBox.Text))
			{
				//fCredentials = new AlternativeCredentials(LoginTextBox.Text, PasswordTextBox.Text);
				fCredentials = new ARCreditNoteApprovalAlternativeCredentials(LoginTextBox.Text, PasswordTextBox.Text,
																FirstLevelSecurityRequiredBranchDepartment, SecondLevelSecurityRequiredBranchDepartment,
																ThirdLevelSecurityRequiredBranchDepartment, FourthLevelSecurityRequiredBranchDepartment,
																FifthLevelSecurityRequiredBranchDepartment, SixthLevelSecurityRequiredBranchDepartment);
			}
			else
			{
				fCredentials = null;
				hasError = true;
			}
			if (!string.IsNullOrEmpty(LoginTextBox2.Text))
			{
				//fCredentials2 = new AlternativeCredentials(LoginTextBox2.Text, PasswordTextBox2.Text);
				fCredentials2 = new ARCreditNoteApprovalAlternativeCredentials(LoginTextBox2.Text, PasswordTextBox2.Text,
																FirstLevelSecurityRequiredBranchDepartment, SecondLevelSecurityRequiredBranchDepartment,
																ThirdLevelSecurityRequiredBranchDepartment, FourthLevelSecurityRequiredBranchDepartment,
																FifthLevelSecurityRequiredBranchDepartment, SixthLevelSecurityRequiredBranchDepartment);
			}
			else
			{
				fCredentials2 = null;
				hasError = true;
			}
			if (LoginTextBox2.Text == LoginTextBox.Text)
			{
				fCredentials = null;
				fCredentials2 = null;
				hasError = true;
				Globals.Message.ShowError(Res.GetString("CA86ABF9-55A4-4480-AFF1-D3C77E3D2E71", "User 1 and user 2 cannot be the same user."));
			}
			if (!hasError)
			{
				Close();
			}
		}
	}
}
