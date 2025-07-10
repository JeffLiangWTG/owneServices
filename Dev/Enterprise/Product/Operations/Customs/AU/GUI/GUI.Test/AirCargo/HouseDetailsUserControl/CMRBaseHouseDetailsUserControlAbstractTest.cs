using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	abstract class CMRBaseHouseDetailsUserControlAbstractTest : TestCaseWithFactory
	{
		public void TestVisibility()
		{
			AUCustomsDataRegistry.Instance.ActivateAlternatePartShipmentModel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			using (ZForm form = new ZForm(mawb))
			using (CMRBaseHouseDetailsUserControl userControl = GetHouseDetailsControl())
			{
				form.Controls.Add(userControl);
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(userControl, "");
				userControl.SetDataBinding(hawb, "");
				form.Show();
				Assert(!userControl.ConRefLabel.Visible);
				Assert(!userControl.conRefTextBox.Visible);
			}

			AUCustomsDataRegistry.Instance.ActivateAlternatePartShipmentModel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			using (ZForm form = new ZForm(mawb))
			using (CMRBaseHouseDetailsUserControl userControl = GetHouseDetailsControl())
			{
				form.Controls.Add(userControl);
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(userControl, "");
				userControl.SetDataBinding(hawb, "");
				form.Show();
				Assert(userControl.ConRefLabel.Visible);
				Assert(userControl.conRefTextBox.Visible);
			}
		}

		public void TestUserFriendlyStatuses()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			CusHAWB hAWB2 = mAWB.ChildBills.AddNew();
			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+2HF2 5CBC D7BF:1+8'
DTM+9:20051103122504817010:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'
FTX+AHN+++LCL UNDERBOND SATISFIED:NO'
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'
FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'
FTX+AHN+++IAR AQIS CLEARED:YES'
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'
FTX+AHN+++ACS EVALUATION COMPLETE:YES'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++IMPORT DECLARATION PAID:N/A'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+QA123++11++++9044748::11'
LOC+12+AUSYD::6'
LOC+4+9122P::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:H00001052/SYD1::1'
RFF+MB:1234564'
RFF+AAQ:LCLU99999999'
DOC+1'
PAC+++LCL:67:95'
UNT+34+000001'".Replace("\r\n", "");
			message.EM_LinkedObject = hAWB;
			message.EM_LinkUniqueID = hAWB.PK;
			message.EM_LinkTable = "CusHAWB";
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_Status = "RCV";
			message.EM_MessageType = "CRS";
			message.EM_MessageSubType = "CRS";
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			hAWB.Messages.Add(message);
			using (ZForm testForm = new ZForm(mAWB))
			{
				CMRBaseHouseDetailsUserControl control = new CMRBaseHouseDetailsUserControl();
				((ICompositeControlBindingSourceProvider)testForm).BindingSource.SetBindingMember(control, "");
				testForm.Controls.Add(control);
				control.HAWB = hAWB;
				testForm.Show();
				string statusResult = @"Information CONSOLIDATED STATUS : HELD
COMPLETE UNDERBOND SERIES APPROVED : N/A
LCL UNDERBOND SATISFIED : NO
CARGO REPORT ACS EVALUATED : NO
IMPORT DECLARATIONS MATCHED : N/A
IMPORT DECLARATION ACS EVALUATED : N/A
IMPORT DECLARATION AQIS EVALUATED : N/A
ACS IMPORT DECLARATION EVALUATION COMPLETE : N/A
AQIS IMPORT DECLARATION EVALUATION COMPLETE : N/A
IMPORT DECLARATION PAID : N/A
CARGO REPORT SAC : NO

========================================
Warning: Cargo is not a consolidation.
========================================";
				control.DetailsButton_Click(null, new EventArgs());
				AssertMultilineASCIIEquals("Should display status", statusResult, UnitTestUserNotification.Instance.LastMessage.ToString());
				control.HAWB = hAWB2;
				control.DetailsButton_Click(null, new EventArgs());
				AssertEquals(UserFriendlyStatusMessages.StatusNotAvailable, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPopulateABNFromConsignee()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "Cuckoo Sqkr";
			consignee.PrimaryRegistrationNumber.Number = "123456789";
			hAWB.CS_OA_ConsigneeAddress = consignee.MainAddress.PK;
			CusHAWB hAWB2 = mAWB.ChildBills.AddNew();
			OrgHeader consignee2 = Factory.New<OrgHeader>();
			consignee2.OH_Code = "Gibbiceps";
			consignee2.PrimaryRegistrationNumber.Number = "987654321";
			hAWB2.CS_OA_ConsigneeAddress = consignee2.MainAddress.PK;
			using (ZForm testForm = new ZForm(mAWB))
			{
				CMRBaseHouseDetailsUserControl control = new CMRBaseHouseDetailsUserControl();
				testForm.Controls.Add(control);
				((ICompositeControlBindingSourceProvider)testForm).BindingSource.SetBindingMember(control, "");
				control.HAWB = hAWB;
				testForm.Show();
				control.PopulateABNButton_Click(null, new EventArgs());
				AssertEquals("123456789", hAWB.CS_ResponsiblePartyID);
				control.HAWB = hAWB2;
				control.PopulateABNButton_Click(null, new EventArgs());
				AssertEquals("987654321", hAWB2.CS_ResponsiblePartyID);
			}
		}

		public void TestHAWBIsSetOnBind()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			using (ZForm form = new ZForm(hAWB))
			{
				using (CMRBaseHouseDetailsUserControl control = GetHouseDetailsControl())
				{
					form.Show();
					control.SetDataBinding(hAWB, "");
					AssertEquals(hAWB, control.HAWB);
				}
			}
		}

		public void TestSACCheckBox()
		{
			CTOCusMAWB mAWB = Factory.New<CTOCusMAWB>();
			CTOCusHAWB hAWB = (CTOCusHAWB)mAWB.AllChildBills.AddNew();
			using (ZForm form = new ZForm(mAWB))
			using (CMRBaseHouseDetailsUserControl userControl = GetHouseDetailsControl())
			{
				userControl.ShowSACForm = true;
				form.Controls.Add(userControl);
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(userControl, "");
				userControl.SetDataBinding(hAWB, "");
				form.Show();
				userControl.SACCheckBox.Focus();
				userControl.SACCheckBox.Checked = false;
				AssertEquals("No form shown", false, ZFormModaliser.LastFormShownDialogForTest is CMRSACDialogBox);
				userControl.SACCheckBox.Checked = true;
				AssertEquals("Form shown", true, ZFormModaliser.LastFormShownDialogForTest is CMRSACDialogBox);
				ZFormModaliser.LastFormShownDialogForTest.Dispose();
			}
		}

		protected abstract CMRBaseHouseDetailsUserControl GetHouseDetailsControl();
	}
}
