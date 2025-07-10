using System.Windows.Forms;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	[TestedType(typeof(HRJobApplicantAccreditationInfoForm))]
	public class HRJobApplicantAccreditationInfoFormTest : ZFormBasherTest
	{
		public void TestFormHeading()
		{
			HRJobApplicant applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_FullName = "Sergey Gordok";
			using (HRJobApplicantAccreditationInfoForm form = new HRJobApplicantAccreditationInfoForm(applicant))
			{
				AssertEquals("Form caption", "Accreditation for Sergey Gordok", form.FormHeading);
			}
		}

		protected override Form GetFormToBashCore()
		{
			HRJobApplicant applicant = Factory.New<HRJobApplicant>();
			applicant.HA_FullName = "name";
			applicant.HA_EmailAddress = "email@addre.ss";
			Factory.Save();
			return new HRJobApplicantAccreditationInfoForm(applicant);
		}
	}
}
