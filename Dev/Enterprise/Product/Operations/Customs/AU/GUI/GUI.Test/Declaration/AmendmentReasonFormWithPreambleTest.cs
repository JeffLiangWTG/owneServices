using System;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(AmendmentReasonForm))]
	sealed class AmendmentReasonFormWithPreambleTest : Customs.GUI.Testing.AmendmentReasonFormTest
	{
		public void TestFormPreamble()
		{
			using (var testForm = (AmendmentReasonForm)GetFormToBash())
			{
				testForm.Show();
				var preambleLabel = testForm.FindSingle<ZLabel>("PreambleLabel");
				AssertEquals("Preamble is visible", true, preambleLabel.Visible);
				AssertEquals("Preamble text is displayed in the form", PreambleForTesting, preambleLabel.CaptionResourceString);
				var reasonLabel = testForm.FindSingle<ZLabel>("ReasonLabel");
				AssertEquals("ReasonLabel is visible", true, reasonLabel.Visible);
				AssertEquals("ReasonLabel has updated text", "Please enter a reason for the amendment:", reasonLabel.Text);
			}
		}

		protected override AmendmentWithdrawalReason GetNewAmendmentWithdrawalReason() => new CMRAmendmentWithdrawalReason();

		protected override Form GetFormToBashCore() => (Form)Activator.CreateInstance(FormToBashType, new object[] { AmendmentReason, PreambleForTesting });

		ResourceStringData PreambleForTesting => NoResourceStringData.GetData("You are requesting a Department Officer to manually amend this REX.  If you continue, this REX will be locked until the officer manually approves the requested changes.  You need to send a reason for requesting this amendment.");
	}
}
