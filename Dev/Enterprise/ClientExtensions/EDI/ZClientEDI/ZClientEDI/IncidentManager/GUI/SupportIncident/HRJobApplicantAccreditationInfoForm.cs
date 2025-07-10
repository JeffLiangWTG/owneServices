using System;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class HRJobApplicantAccreditationInfoForm : ZChildForm
	{
		public HRJobApplicantAccreditationInfoForm(HRJobApplicant applicant)
			: base(applicant)
		{
		}

		public HRJobApplicantAccreditationInfoForm()
		{
		}

		public override string FormHeading
		{
			get { return "Accreditation for " + ((HRJobApplicant)BusinessEntity).HA_FullName; }
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			InitializeComponent();
		}
	}
}
