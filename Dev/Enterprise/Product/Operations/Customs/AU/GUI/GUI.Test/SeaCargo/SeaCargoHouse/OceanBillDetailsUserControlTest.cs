using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class OceanBillDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestCreateControl()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OBL001";
			oceanBill.CB_MasterHouseBill = "MHB";
			oceanBill.CB_ApplicationCode = "CMR";
			oceanBill.CB_DateOfFirstArrival = ZDateTime.BrettsBirthday;
			oceanBill.CB_DateOfArrival = ZDateTime.BrettsBirthday.AddDays(1);
			oceanBill.CB_DateOfDeparture = ZDateTime.BrettsBirthday.AddDays(2);
			oceanBill.CB_ResponsiblePartyID = "RP";
			oceanBill.CB_PrincipalID = "P1";
			oceanBill.CB_VesselName = "ADMIRALENGRACHT";
			oceanBill.CB_LloydsIMO = "8610033";
			oceanBill.CB_Voyage = "AM1";
			oceanBill.CB_RL_NKPortOfFirstArrival = "AUSYD";
			oceanBill.CB_RL_NKPortOfDischarge = "AUBNE";
			oceanBill.CB_RL_NKPortOfLoading = "SGSIN";
			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_Code = "SL1";
			oceanBill.CB_OH_ShippingLine = shippingLine.PK;
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "TS1";
			branch.GB_BranchName = "TESTBRANCH1";
			oceanBill.CB_GB = branch.PK;
			using (var testForm = new ZForm(oceanBill))
			using (var oceanBillDetailsUserControl = new OceanBillDetailsUserControl())
			{
				testForm.Controls.Add(oceanBillDetailsUserControl);
				oceanBillDetailsUserControl.SetDataBinding(oceanBill, "");
				testForm.Show();
				AssertEquals("OBL001", testForm.FindSingle<ZTextBox>("CB_OceanBillBoundTextBox").Text);
				AssertEquals("MHB", testForm.FindSingle<ZTextBox>("CusSCAOceanBillParentBillTextBox").Text);
				AssertEquals("CMR", testForm.FindSingle<ZDropEdit>("CB_ApplicationCodeBoundDropEdit").Text);
				AssertEquals("18-SEP-71", testForm.FindSingle<ZDateEdit>("FirstArrivalDateEdit").Text);
				AssertEquals("19-SEP-71", testForm.FindSingle<ZDateEdit>("ArrivalDateEdit").Text);
				AssertEquals("20-SEP-71", testForm.FindSingle<ZDateEdit>("DepartureDateEdit").Text);
				AssertEquals("RP", testForm.FindSingle<ZTextBox>("ResponsiblePartyIDTextBox").Text); // zTextBox3
				AssertEquals("P1", testForm.FindSingle<ZTextBox>("PrincipalIDTextBox").Text); // zTextBox2
				AssertEquals("8610033", testForm.FindSingle<ZDropEdit>("LloydsDropEdit").Text);
				AssertEquals("ADMIRALENGRACHT", testForm.FindSingle<ZCodeFindBox>("CB_RV_NKVessel").Text);
				AssertEquals("AM1", testForm.FindSingle<ZTextBox>("CB_VoyageBoundTextBox").Text);
				AssertEquals("AUSYD", testForm.FindSingle<ZCodeFindBox>("CB_RL_NKPortOfFirstArrivalCodeFindBox").Text);
				AssertEquals("AUBNE", testForm.FindSingle<ZCodeFindBox>("CB_RL_NKPortOfDischargeBoundCodeFindBox").Text);
				AssertEquals("SGSIN", testForm.FindSingle<ZCodeFindBox>("CB_RL_NKPortOfLoadingBoundCodeFindBox").Text);
				AssertEquals("SL1", testForm.FindSingle<ZGuidFindBox>("ShippingLineGuidFindBox").Text);
				AssertEquals("TS1", testForm.FindSingle<ZGuidFindBox>("BranchGuidFindBox").Text);
				AssertEquals(false, testForm.FindSingle<ZCheckBox>("overrideFreightDefaultsCheckBox").Visible);
			}
		}

		public void TestReadOnly()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OBL001";
			oceanBill.CB_MasterHouseBill = "MHB";
			oceanBill.CB_ApplicationCode = "CMR";
			oceanBill.CB_DateOfFirstArrival = ZDateTime.BrettsBirthday;
			oceanBill.CB_DateOfArrival = ZDateTime.BrettsBirthday.AddDays(1);
			oceanBill.CB_DateOfDeparture = ZDateTime.BrettsBirthday.AddDays(2);
			oceanBill.CB_ResponsiblePartyID = "RP";
			oceanBill.CB_PrincipalID = "P1";
			oceanBill.CB_VesselName = "ADMIRALENGRACHT";
			oceanBill.CB_LloydsIMO = "8610033";
			oceanBill.CB_Voyage = "AM1";
			oceanBill.CB_RL_NKPortOfFirstArrival = "AUSYD";
			oceanBill.CB_RL_NKPortOfDischarge = "AUBNE";
			oceanBill.CB_RL_NKPortOfLoading = "SGSIN";
			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_Code = "SL1";
			oceanBill.CB_OH_ShippingLine = shippingLine.PK;
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "TS1";
			branch.GB_BranchName = "TESTBRANCH1";
			oceanBill.CB_GB = branch.PK;

			using (var testForm = new ZForm(oceanBill))
			using (var oceanBillDetailsUserControl = new OceanBillDetailsUserControl())
			{
				testForm.Controls.Add(oceanBillDetailsUserControl);
				oceanBillDetailsUserControl.SetDataBinding(oceanBill, "");
				testForm.Show();
				Assert("GroupBoxOceanBill is never readonly", !testForm.FindSingle<ZGroupBox>("GroupBoxOceanBill").GetReadOnly());
				Assert(!testForm.FindSingle<ZTextBox>("CB_OceanBillBoundTextBox").ReadOnly);
				Assert(!testForm.FindSingle<ZTextBox>("CusSCAOceanBillParentBillTextBox").ReadOnly);
				Assert(!testForm.FindSingle<ZDateEdit>("FirstArrivalDateEdit").ReadOnly);
				Assert(!testForm.FindSingle<ZDateEdit>("ArrivalDateEdit").ReadOnly);
				Assert(!testForm.FindSingle<ZDateEdit>("DepartureDateEdit").ReadOnly);
				Assert(!testForm.FindSingle<ZTextBox>("ResponsiblePartyIDTextBox").ReadOnly);
				Assert(!testForm.FindSingle<ZTextBox>("PrincipalIDTextBox").ReadOnly);
				Assert(!testForm.FindSingle<ZDropEdit>("LloydsDropEdit").ReadOnly);
				Assert(!testForm.FindSingle<ZCodeFindBox>("CB_RV_NKVessel").ReadOnly);
				Assert(!testForm.FindSingle<ZTextBox>("CB_VoyageBoundTextBox").ReadOnly);
				Assert(!testForm.FindSingle<ZCodeFindBox>("CB_RL_NKPortOfFirstArrivalCodeFindBox").ReadOnly);
				Assert(!testForm.FindSingle<ZCodeFindBox>("CB_RL_NKPortOfDischargeBoundCodeFindBox").ReadOnly);
				Assert(!testForm.FindSingle<ZCodeFindBox>("CB_RL_NKPortOfLoadingBoundCodeFindBox").ReadOnly);
				Assert(!testForm.FindSingle<ZGuidFindBox>("ShippingLineGuidFindBox").ReadOnly);
				Assert(!testForm.FindSingle<ZGuidFindBox>("BranchGuidFindBox").ReadOnly);
				Assert("ApplicationCode is always readonly", testForm.FindSingle<ZDropEdit>("CB_ApplicationCodeBoundDropEdit").ReadOnly);
				oceanBillDetailsUserControl.ReadOnly = true;
				Assert("GroupBoxOceanBill is never readonly", !testForm.FindSingle<ZGroupBox>("GroupBoxOceanBill").GetReadOnly());
				Assert(testForm.FindSingle<ZTextBox>("CB_OceanBillBoundTextBox").ReadOnly);
				Assert(testForm.FindSingle<ZTextBox>("CusSCAOceanBillParentBillTextBox").ReadOnly);
				Assert(testForm.FindSingle<ZDateEdit>("FirstArrivalDateEdit").ReadOnly);
				Assert(testForm.FindSingle<ZDateEdit>("ArrivalDateEdit").ReadOnly);
				Assert(testForm.FindSingle<ZDateEdit>("DepartureDateEdit").ReadOnly);
				Assert(testForm.FindSingle<ZTextBox>("ResponsiblePartyIDTextBox").ReadOnly);
				Assert(testForm.FindSingle<ZTextBox>("PrincipalIDTextBox").ReadOnly);
				Assert(testForm.FindSingle<ZDropEdit>("LloydsDropEdit").ReadOnly);
				Assert(testForm.FindSingle<ZCodeFindBox>("CB_RV_NKVessel").ReadOnly);
				Assert(testForm.FindSingle<ZTextBox>("CB_VoyageBoundTextBox").ReadOnly);
				Assert(testForm.FindSingle<ZCodeFindBox>("CB_RL_NKPortOfFirstArrivalCodeFindBox").ReadOnly);
				Assert(testForm.FindSingle<ZCodeFindBox>("CB_RL_NKPortOfDischargeBoundCodeFindBox").ReadOnly);
				Assert(testForm.FindSingle<ZCodeFindBox>("CB_RL_NKPortOfLoadingBoundCodeFindBox").ReadOnly);
				Assert(testForm.FindSingle<ZGuidFindBox>("ShippingLineGuidFindBox").ReadOnly);
				Assert(testForm.FindSingle<ZGuidFindBox>("BranchGuidFindBox").ReadOnly);
				Assert("ApplicationCode is always readonly", testForm.FindSingle<ZDropEdit>("CB_ApplicationCodeBoundDropEdit").ReadOnly);
				oceanBillDetailsUserControl.ReadOnly = false;
				Assert("GroupBoxOceanBill is never readonly", !testForm.FindSingle<ZGroupBox>("GroupBoxOceanBill").GetReadOnly());
				Assert(testForm.FindSingle<ZTextBox>("CB_OceanBillBoundTextBox").ReadOnly);
				Assert(testForm.FindSingle<ZTextBox>("CusSCAOceanBillParentBillTextBox").ReadOnly);
				Assert(testForm.FindSingle<ZDateEdit>("FirstArrivalDateEdit").ReadOnly);
				Assert(testForm.FindSingle<ZDateEdit>("ArrivalDateEdit").ReadOnly);
				Assert(testForm.FindSingle<ZDateEdit>("DepartureDateEdit").ReadOnly);
				Assert(testForm.FindSingle<ZTextBox>("ResponsiblePartyIDTextBox").ReadOnly);
				Assert(testForm.FindSingle<ZTextBox>("PrincipalIDTextBox").ReadOnly);
				Assert(testForm.FindSingle<ZDropEdit>("LloydsDropEdit").ReadOnly);
				Assert(testForm.FindSingle<ZCodeFindBox>("CB_RV_NKVessel").ReadOnly);
				Assert(testForm.FindSingle<ZTextBox>("CB_VoyageBoundTextBox").ReadOnly);
				Assert(testForm.FindSingle<ZCodeFindBox>("CB_RL_NKPortOfFirstArrivalCodeFindBox").ReadOnly);
				Assert(testForm.FindSingle<ZCodeFindBox>("CB_RL_NKPortOfDischargeBoundCodeFindBox").ReadOnly);
				Assert(testForm.FindSingle<ZCodeFindBox>("CB_RL_NKPortOfLoadingBoundCodeFindBox").ReadOnly);
				Assert(testForm.FindSingle<ZGuidFindBox>("ShippingLineGuidFindBox").ReadOnly);
				Assert(testForm.FindSingle<ZGuidFindBox>("BranchGuidFindBox").ReadOnly);
				Assert("ApplicationCode is always readonly", testForm.FindSingle<ZDropEdit>("CB_ApplicationCodeBoundDropEdit").ReadOnly);
			}
		}

		public void TestUpdateForOceanBillUnpack()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_MultiOBLUnpack = false;
			using (var form = new ZForm(oceanBill))
			using (var oceanBillDetailsUserControl = new OceanBillDetailsUserControl())
			{
				form.Controls.Add(oceanBillDetailsUserControl);
				oceanBillDetailsUserControl.SetDataBinding(oceanBill, "");
				form.Show();
				AssertEquals("OceanBillLabel.Visible", true, form.FindSingle<ZLabel>("OceanBillLabel").Visible);
				AssertEquals("CusSCAOceanBillParentBillLabel.Visible", true, form.FindSingle<ZLabel>("CusSCAOceanBillParentBillLabel").Visible);
				AssertEquals("CB_OceanBillBoundTextBox.Visible", true, form.FindSingle<ZTextBox>("CB_OceanBillBoundTextBox").Visible);
				AssertEquals("CusSCAOceanBillParentBillTextBox.Visible", true, form.FindSingle<ZTextBox>("CusSCAOceanBillParentBillTextBox").Visible);
				oceanBill.CB_MultiOBLUnpack = true;
				oceanBillDetailsUserControl.UpdateForOceanBillUnpack();
				AssertEquals("OceanBillLabel.Visible", false, form.FindSingle<ZLabel>("OceanBillLabel").Visible);
				AssertEquals("CusSCAOceanBillParentBillLabel.Visible", false, form.FindSingle<ZLabel>("CusSCAOceanBillParentBillLabel").Visible);
				AssertEquals("CB_OceanBillBoundTextBox.Visible", false, form.FindSingle<ZTextBox>("CB_OceanBillBoundTextBox").Visible);
				AssertEquals("CusSCAOceanBillParentBillTextBox.Visible", false, form.FindSingle<ZTextBox>("CusSCAOceanBillParentBillTextBox").Visible);
			}
		}

		public void TestMessagingModeVisible()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			using (var testForm = new ZForm(oceanBill))
			using (var oceanBillDetailsUserControl = new OceanBillDetailsUserControl())
			{
				testForm.Controls.Add(oceanBillDetailsUserControl);
				oceanBillDetailsUserControl.SetDataBinding(oceanBill, "");
				testForm.Show();
				AssertEquals("MessagingModeLabel.Visible", true, testForm.FindSingle<Control>("MessagingModeLabel").Visible);
				AssertEquals("CB_ApplicationCodeBoundDropEdit.Visible", true, testForm.FindSingle<Control>("CB_ApplicationCodeBoundDropEdit").Visible);
				oceanBillDetailsUserControl.SetMessagingModeVisiblity(false);
				AssertEquals("MessagingModeLabel.Visible", false, testForm.FindSingle<Control>("MessagingModeLabel").Visible);
				AssertEquals("CB_ApplicationCodeBoundDropEdit.Visible", false, testForm.FindSingle<Control>("CB_ApplicationCodeBoundDropEdit").Visible);
			}
		}

		public void TestOnOverrideFreightDefaultsChangingRequiresConfirmation()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var consol = Factory.New<ForwardingConsol>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			using (var form = new ZForm(oceanBill))
			using (var oceanBillDetailsUserControl = new OceanBillDetailsUserControl())
			{
				form.Controls.Add(oceanBillDetailsUserControl);
				form.Show();

				AssertEquals(false, oceanBill.OverrideFreightDefaults);

				var overrideCheckbox = form.FindSingle<ZCheckBox>("overrideFreightDefaultsCheckBox");
				AssertEquals("Checkbox is visible when attached to a Consol", true, overrideCheckbox.Visible);
				overrideCheckbox.Checked = true;
				AssertEquals(true, overrideCheckbox.Checked);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				overrideCheckbox.Checked = false;
				AssertStartsWith("", "Removing the override will reset your Sea Cargo data.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, overrideCheckbox.Checked);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				overrideCheckbox.Checked = false;
				AssertStartsWith("", "Removing the override will reset your Sea Cargo data.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, overrideCheckbox.Checked);
			}
		}
	}
}
