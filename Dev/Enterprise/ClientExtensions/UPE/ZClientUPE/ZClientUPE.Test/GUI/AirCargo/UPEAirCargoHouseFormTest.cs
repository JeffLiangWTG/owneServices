using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.GUI.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.GUI
{
	class UPEAirCargoHouseFormTest : TestCaseWithFactory
	{
		public void TestTopLevelTabControl()
		{
			AssertEquals("Top level tab control has to be main tab control", Form.MainTabControl, Form.TopLevelTabControl);
		}

		public void TestFormCaption()
		{
			Form.BusinessEntity.CS_HAWB = "HOUSEBILL";
			Assert(Form.FormCaption.EndsWith(" - HOUSEBILL"));
		}

		public void TestPlugInsExist()
		{
			AssertNotNull("Process Queue Plug-in should exist", Form.PlugIns.GetPlugIn(ControllerIDs.ProcessQueue));
			AssertNotNull("eDocs Plug-in should exist", Form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
			AssertNotNull("UDF Plug-in should exist", Form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
		}

		public void TestNewButtonNotShownOnForm()
		{
			Form.Show();
			Application.DoEvents();
			Assert("Form should not have new button", !Form.HasNewButton);
		}

		public void TestRunScreeningValidation_OnLoad()
		{
			CMRReferenceFilesTestHelper.InsertThesaurusData(Factory, "Nuclear");
			var masterBill = Factory.New<UPECusMAWB>();
			masterBill.ChildBills.Add(HouseBill);
			HouseBill.CS_GoodsDescription = "NUCLEAR";
			AssertEquals("Sanity check. Stop word found, should have warning", true, HouseBill.CS_GoodsDescriptionInfo.HasWarnings());
			using (HouseBill.SuspendValidationTesting())
			{
				HouseBill.ClearAllNotifications();
			}

			Form.Show();
			Application.DoEvents();
			AssertEquals("Stop word found, should be validation on form load", true, HouseBill.CS_GoodsDescriptionInfo.HasWarnings());
		}

		public void TestAlerts()
		{
			HouseBill.AlertsList.Add("Test Alert");
			Form.Show();
			Application.DoEvents();
			AssertEquals(true, Form.AlertForm.Visible);
			AssertEquals("Test Alert", HouseBill.AlertsList[0]);
		}

		public void TestValidateAndSave_ConfirmPreAlertToCustoms()
		{
			HouseBill = GetNewCusHAWBValidForACA();
			HouseBill.DutyType = DutyTypeCodeDescriptionPairList.Codes.NonDutiable;
			using (UPEAirCargoHouseFormForTest aCAForm = new UPEAirCargoHouseFormForTest(HouseBill))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				aCAForm.InternalValidateAndSave();
				AssertEquals(0, HouseBill.Messages.Count);
				AssertEquals("Confirmation message", "Do you wish to pre-alert the HAWB to Customs?", UnitTestUserNotification.Instance.LastMessage.Text);
				HouseBill.CS_ConsignorName = "";
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				aCAForm.InternalValidateAndSave();
				AssertEquals(0, HouseBill.Messages.Count);
				string errorMessage = "Could not pre-alert HAWB to customs because of the following errors:\nCS_ConsignorName: Consignor Name is required";
				AssertEquals("Message to user when there are errors sending ACA", errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Message should be an error", UnitTestUserNotification.Instance.LastMessage.WasError);
				HouseBill.CS_ConsignorName = "ConsignorName";
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				aCAForm.InternalValidateAndSave();
				AssertEquals(1, HouseBill.Messages.Count);
				Assert("ConfirmationMessage", ContainsPreviousMessage("Do you wish to pre-alert the HAWB to Customs?"));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				aCAForm.InternalValidateAndSave();
				Assert("User should NOT have been ask to pre-alert hawb if message to customs has already been sent", !ContainsPreviousMessage("Do you wish to pre-alert the HAWB to Customs?"));
			}
		}

		bool ContainsPreviousMessage(string expectedMessage)
		{
			foreach (UnitTestUserNotification.PreviousMessage message in UnitTestUserNotification.Instance.PreviousMessages)
			{
				if (message.Text == expectedMessage)
				{
					return true;
				}
			}

			return false;
		}

		#region Event Handlers
		public void TestCusHAWBGuiHelper_EventHandlersHooked()
		{
			AssertEquals(true, Form.CusHAWBGuiEventHandlers.IsEventHooked);
		}

		public void TestValidateAndSave_RunPreSaveDialogs_ContinueWithSaveYes()
		{
			using (HouseBill.GetValidationSuspender())
			{
				HouseBill.HasChanges = true;
				HouseBill.DutyType = DutyTypeCodeDescriptionPairList.Codes.NonDutiable;
				HouseBill.ShipmentType = ShipmentTypeCodeDescriptionPairList.Codes.Documents;
				Form.CusHAWBGuiEventHandlers.RunPreSaveDialogs_ContinueWithSave = ContinueWithSave.Yes;
				ContinueWithSave @continue = Form.InternalValidateAndSave();
				AssertEquals("ContinueWithSave=Yes", ContinueWithSave.Yes, @continue);
				AssertEquals("Form should be saved", false, HouseBill.HasChanges);
			}
		}

		public void TestValidateAndSave_RunPreSaveDialogs_ContinueWithSaveNo()
		{
			HouseBill.HasChanges = true;
			Form.CusHAWBGuiEventHandlers.RunPreSaveDialogs_ContinueWithSave = ContinueWithSave.No;
			ContinueWithSave @continue = Form.InternalValidateAndSave();
			AssertEquals("ContinueWithSave=No", ContinueWithSave.No, @continue);
			AssertEquals("Form should NOT be saved", true, HouseBill.HasChanges);
		}

		public void TestProcessQueueEventHooked()
		{
			AssertEquals(true, HouseBill.CurrentQueue.SubscribedToEIRRaisedProcessing);
		}

		#endregion
		#region Implementation
		#region GetNewCusHAWBValidForACA
		UPECusHAWB GetNewCusHAWBValidForACA()
		{
			CusMAWB cusMAWB = Factory.New<CusMAWB>();
			cusMAWB.CM_ArrivalDate = new ZDateTime(2005, 10, 10);
			cusMAWB.CM_FlightNo = "QF123";
			cusMAWB.CM_MAWB = "08133333333";
			cusMAWB.CM_RL_NKDischargePort = "AUSYD";
			cusMAWB.CM_RL_NKFirstArrivalPort = "AUBNE";
			cusMAWB.CM_RL_NKLoadPort = "SGSIN";
			UPECusHAWB result = (UPECusHAWB)cusMAWB.ChildBills.AddNew();
			result.CS_HAWB = "UPECusHAWB";
			result.CS_RL_NKOrigin = "SGSIN";
			result.CS_RL_NKDestination = "AUSYD";
			result.CS_MasterHouseBill = "08144444444";
			result.CS_Weight = 10m;
			result.CS_WeightUQ = "KG";
			result.CS_GoodsValue = 5230.12m;
			result.CS_RX_NKGoodsCurrency = "ZAR";
			result.CS_GoodsDescription = "GoodsDescription";
			result.CS_IsSurplus = false;
			result.CS_FreightPrepaidCollect = CMRMethodsOfPayment.Codes.PrepaidOnly;
			result.ShipmentType = ShipmentTypeCodeDescriptionPairList.Codes.Documents;
			result.CS_ShipmentType = "DOC";
			result.CS_PiecesManifested = 10;
			result.CS_RS_NK_ServiceLevel = "STD";
			result.CS_ConsigneeCity = "Sydney";
			result.CS_ConsigneeContactName = "ConsigneeContactName";
			result.CS_ConsigneeName = "ConsigneeName";
			result.CS_ConsigneePhone = "9637660";
			result.CS_ConsigneePostcode = "7551";
			result.CS_ConsigneeState = "NSW";
			result.CS_ConsigneeStreet = "Street1";
			result.CS_ConsigneeStreet2 = "Street2";
			result.CS_RN_NKConsigneeCountry = "AU";
			result.CS_ConsignorCity = "Singapore";
			result.CS_ConsignorContactName = "ConsignorContactName";
			result.CS_ConsignorName = "ConsignorName";
			result.CS_ConsignorPhone = "963766";
			result.CS_ConsignorPostcode = "7550";
			result.CS_ConsignorState = "Jurong";
			result.CS_ConsignorStreet = "Pasir";
			result.CS_ConsignorStreet2 = "Gudang";
			result.CS_RN_NKConsignorCountry = "SG";
			return result;
		}

		#endregion
		ZString currentRegNo;
		OrgHeader currentCompany;
		BusinessObjectFactory currentCompanyFactory;
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			currentCompanyFactory = new BusinessObjectFactory();
			currentRegNo = GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number;
			currentCompany = currentCompanyFactory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			currentCompany.PrimaryRegistrationNumber.Number = "21 003 980 130 123";
			currentCompanyFactory.Save();
			HouseBill = Factory.NewWithValidTestData<UPECusHAWB>();
			Form = new UPEAirCargoHouseFormForTest(HouseBill);
		}

		protected override void TearDown()
		{
			currentCompany.PrimaryRegistrationNumber.Number = currentRegNo;
			currentCompanyFactory.Save();
			Form.Dispose();
			base.TearDown();
		}

		UPEAirCargoHouseFormForTest Form;
		UPECusHAWB HouseBill;
		#endregion
		#region Test Classes
		class UPEAirCargoHouseFormForTest : UPEAirCargoHouseForm
		{
			public UPEAirCargoHouseFormForTest(UPECusHAWB businessEntity) : base(businessEntity)
			{
			}

			public new ZTabControl TopLevelTabControl
			{
				get
				{
					return base.TopLevelTabControl;
				}
			}

			public new ZTemplateTabControl MainTabControl
			{
				get
				{
					return base.MainTabControl;
				}
			}

			public bool HasNewButton
			{
				get
				{
					return fApplyButton != null && fApplyButton.Text.IndexOf("New") > -1;
				}
			}

			public TestCusHAWBGuiEventHandlers CusHAWBGuiEventHandlers
			{
				get
				{
					if (fCusHAWBGuiEventHandlers == null)
					{
						fCusHAWBGuiEventHandlers = new TestCusHAWBGuiEventHandlers(BusinessEntity);
					}

					return fCusHAWBGuiEventHandlers;
				}
			}

			TestCusHAWBGuiEventHandlers fCusHAWBGuiEventHandlers;
			protected override CusHAWBGuiEventHandlers GetCusHAWBGuiEventHandlers()
			{
				return CusHAWBGuiEventHandlers;
			}
		}
		#endregion
	}
}
