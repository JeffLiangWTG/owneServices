using System.Collections.Generic;
using Enterprise.MasterFiles.GUI;
using static Enterprise.Accounting.GUI.InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD;

namespace Enterprise.Accounting.GUI
{
	public partial class LoginFormWithRequest : LoginForm
	{
		public LoginFormWithRequest(List<BranchDepartmentPair> firstLevelSecurityRequiredBranchDepartment = null,
								List<BranchDepartmentPair> secondLevelSecurityRequiredBranchDepartment = null,
								List<BranchDepartmentPair> thirdLevelSecurityRequiredBranchDepartment = null,
								List<BranchDepartmentPair> fourthLevelSecurityRequiredBranchDepartment = null,
								List<BranchDepartmentPair> fifthLevelSecurityRequiredBranchDepartment = null,
								List<BranchDepartmentPair> sixthLevelSecurityRequiredBranchDepartment = null)
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

#if DEBUG
		public override void DoLoginForTest(string loginName, string password)
		{
			fCredentials = new ARCreditNoteApprovalAlternativeCredentials(loginName, password,
				FirstLevelSecurityRequiredBranchDepartment, SecondLevelSecurityRequiredBranchDepartment,
				ThirdLevelSecurityRequiredBranchDepartment, FourthLevelSecurityRequiredBranchDepartment,
				FifthLevelSecurityRequiredBranchDepartment, SixthLevelSecurityRequiredBranchDepartment);
		}

#endif

		protected override void OKButton_Click(object sender, System.EventArgs e)
		{
			if ((FirstLevelSecurityRequiredBranchDepartment != null && FirstLevelSecurityRequiredBranchDepartment.Count > 0)
				|| (SecondLevelSecurityRequiredBranchDepartment != null && SecondLevelSecurityRequiredBranchDepartment.Count > 0)
				|| (ThirdLevelSecurityRequiredBranchDepartment != null && ThirdLevelSecurityRequiredBranchDepartment.Count > 0)
				|| (FourthLevelSecurityRequiredBranchDepartment != null && FourthLevelSecurityRequiredBranchDepartment.Count > 0)
				|| (FifthLevelSecurityRequiredBranchDepartment != null && FifthLevelSecurityRequiredBranchDepartment.Count > 0)
				|| (SixthLevelSecurityRequiredBranchDepartment != null && SixthLevelSecurityRequiredBranchDepartment.Count > 0))
			{
				fCredentials = new ARCreditNoteApprovalAlternativeCredentials(LoginTextBox.Text, PasswordTextBox.Text,
					FirstLevelSecurityRequiredBranchDepartment, SecondLevelSecurityRequiredBranchDepartment,
					ThirdLevelSecurityRequiredBranchDepartment, FourthLevelSecurityRequiredBranchDepartment,
					FifthLevelSecurityRequiredBranchDepartment, SixthLevelSecurityRequiredBranchDepartment);
				Close();
			}
			else
			{
				base.OKButton_Click(sender, e);
			}
		}
	}
}
