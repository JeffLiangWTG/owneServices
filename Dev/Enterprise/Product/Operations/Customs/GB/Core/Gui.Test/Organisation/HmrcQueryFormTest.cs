using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Organisation;
using Enterprise.Customs.GB.CDS.Organisation;
using Enterprise.Customs.GB.GUI.Organisation;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Testing
{
	[TestedType(typeof(HmrcQueryForm))]
	sealed class HmrcQueryFormTest : ZFormBasherTest
	{
		public void TestSendEORIQuery()
		{
			using var form = new HmrcQueryForm(checker);
			form.Show();
			form.VerifyEORICheckBox.Checked = true;
			form.VerifyVATCheckBox.Checked = false;
			form.QueryButton.PerformClick();

			AssertEquals(DialogResult.OK, form.DialogResult);
			AssertEquals(true, checker.VerifyEORI);
			AssertEquals(false, checker.VerifyVAT);
			AssertEquals(1, organisationWrapper.Messages.Count);
			AssertContains("GBEORI1234", organisationWrapper.Messages[0].EM_MessageText);
		}

		public void TestSendVATQuery()
		{
			using var form = new HmrcQueryForm(checker);
			form.Show();
			form.VerifyEORICheckBox.Checked = false;
			form.VerifyVATCheckBox.Checked = true;
			form.QueryButton.PerformClick();

			AssertEquals(DialogResult.OK, form.DialogResult);
			AssertEquals(false, checker.VerifyEORI);
			AssertEquals(true, checker.VerifyVAT);
			AssertEquals(1, organisationWrapper.Messages.Count);
			AssertContains("VAT1234", organisationWrapper.Messages[0].EM_MessageText);
		}

		public void TestSendBothQueries()
		{
			using var form = new HmrcQueryForm(checker);
			form.Show();
			form.VerifyEORICheckBox.Checked = true;
			form.VerifyVATCheckBox.Checked = true;
			form.QueryButton.PerformClick();

			AssertEquals(DialogResult.OK, form.DialogResult);
			AssertEquals(true, checker.VerifyEORI);
			AssertEquals(true, checker.VerifyVAT);
			AssertEquals(2, organisationWrapper.Messages.Count);
			AssertContains("VAT1234", organisationWrapper.Messages[0].EM_MessageText);
			AssertContains("GBEORI1234", organisationWrapper.Messages[1].EM_MessageText);
		}

		public void TestSendNone()
		{
			using var form = new HmrcQueryForm(checker);
			form.Show();
			form.VerifyEORICheckBox.Checked = false;
			form.VerifyVATCheckBox.Checked = false;
			form.QueryButton.PerformClick();

			AssertEquals(DialogResult.None, form.DialogResult);
			AssertEquals(false, checker.VerifyEORI);
			AssertEquals(false, checker.VerifyVAT);
			AssertEquals(0, organisationWrapper.Messages.Count);
			AssertEquals("Please select at least one number for verification!", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		protected override Form GetFormToBashCore() => new HmrcQueryForm(checker);

		OrgHeaderWrapper organisationWrapper;
		NonPersistentOrgHeaderChecker checker;

		protected override void SetUp()
		{
			base.SetUp();
			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			organisation.OH_Code = ZGuid.NewZGuid().ToString().ToUpper().Substring(0, 8);
			organisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "VAT1234", Core.Constants.CountryCodes.UnitedKingdom);
			organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORI1234", Core.Constants.CountryCodes.UnitedKingdom);
			organisationWrapper = new OrgHeaderWrapper(organisation);
			checker = new NonPersistentOrgHeaderChecker(Factory, organisation);
			Factory.Save();
		}
	}
}
