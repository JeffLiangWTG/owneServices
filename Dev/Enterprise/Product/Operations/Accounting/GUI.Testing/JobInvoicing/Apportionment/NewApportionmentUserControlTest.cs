using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting;
using Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.JobInvoicing.Apportionment.Testing
{
	public class NewApportionmentUserControlTest : TestCaseWithFactory
	{
		public void TestTaxBranchControls()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol();
			Factory.Save();

			var listing = new ApportionmentListing(Factory, consol);

			AssertTaxBranchControls(true);
			AssertTaxBranchControls(false);

			void AssertTaxBranchControls(bool enableTaxBranchReporting)
			{
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableTaxBranchReporting))
				{
					using (var form = new ZForm())
					using (var control = new NewApportionmentUserControl(listing))
					{
						form.Controls.Add(control);
						form.Show();

						var costTaxBranchGuidFindBox = control.Controls.Find("CostTaxBranchGuidFindBox", true)[0];
						AssertEquals(enableTaxBranchReporting, costTaxBranchGuidFindBox.Visible);

						AssertEquals(!enableTaxBranchReporting, control.CostSummaryGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_GB_CostTaxBranch).IsUnavailable);
						AssertEquals(!enableTaxBranchReporting, control.GetApportionedChargesGrid().GetColumnStyle(JobChargeSchema.JR_GB_CostTaxBranch.Name).IsUnavailable);
						AssertEquals(!enableTaxBranchReporting, control.GetApportionedChargesGrid().GetColumnStyle(JobChargeSchema.JR_GB_SellTaxBranch.Name).IsUnavailable);
					}
				}
			}
		}

		public void TestSupplyTypeControls()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol();
			Factory.Save();

			var listing = new ApportionmentListing(Factory, consol);

			AssertSupplyTypeControls(true);
			AssertSupplyTypeControls(false);

			void AssertSupplyTypeControls(bool enableSupplyTypeClassificationCodes)
			{
				using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableSupplyTypeClassificationCodes))
				{
					using (var form = new ZForm())
					using (var control = new NewApportionmentUserControl(listing))
					{
						form.Controls.Add(control);
						form.Show();

						var costSupplyTypeDropEdit = control.Controls.Find("CostSupplyTypeDropEdit", true)[0];
						AssertEquals(enableSupplyTypeClassificationCodes, costSupplyTypeDropEdit.Visible);

						AssertEquals(!enableSupplyTypeClassificationCodes, control.CostSummaryGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_SupplyType).IsUnavailable);
						AssertEquals(!enableSupplyTypeClassificationCodes, control.GetApportionedChargesGrid().GetColumnStyle(Charge.Schema.JR_CostSupplyType).IsUnavailable);
						AssertEquals(!enableSupplyTypeClassificationCodes, control.GetApportionedChargesGrid().GetColumnStyle(Charge.Schema.JR_SellSupplyType).IsUnavailable);
					}
				}
			}
		}

		public void TestRelatedJobColumn()
		{
			var creator = new TestObjectCreator(Factory);

			var consol = creator.CreateConsol();
			var listing = new ApportionmentListing(Factory, consol);
			using (var form = new ZForm())
			using (var control = new NewApportionmentUserControl(listing))
			{
				form.Controls.Add(control);
				form.Show();

				Assert(control.GetApportionedChargesGrid().GetColumnStyle(nameof(ApportionSplitCharge.JR_Calc_RelatedJobNumber)).IsUnavailable);
			}

			consol = creator.CreateGatewayConsol(sendingGatewayCompany: GlbCompany.CurrentCompany);
			listing = new ApportionmentListing(Factory, consol, true);

			using (var form = new ZForm())
			using (var control = new NewApportionmentUserControl(listing))
			{
				form.Controls.Add(control);
				form.Show();

				Assert(!control.GetApportionedChargesGrid().GetColumnStyle(nameof(ApportionSplitCharge.JR_Calc_RelatedJobNumber)).IsUnavailable);
			}
		}

		public void TestTargetConsolJobAlreadyMutexed()
		{
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var creator = new TestObjectCreator(Factory);

			//			gC0001		gC0002		C0003		C0004
			//	AUBNE	-	AUSYD	-	SGSIN	-	HKHKG	-	USLAX
			//												 \
			//													C0005
			//														\
			//															USNYC
			//				|-	-	-	-	-	S0001	-	-	-|
			//	|-	-	-	-	-	-	-	S0002	-	-	-	-	-	-|
			//	|-	-	-	S0003	-	-|
			//
			var setup = creator.CreateGatewayConsolsAndShipments();
			Factory.Save();

			using (var gatewayJob = creator.CreateJob(setup.gC0002))
			{
				var gtwCharge = gatewayJob.Charges.AddNew();
				gtwCharge.JR_AC = creator.FRT.PK;
				gtwCharge.JR_OH_SellAccount = setup.senAg.PK;
				gtwCharge.JR_OSSellAmt = 3333m;
				gtwCharge.JR_RX_NKSellCurrency = "AUD";
				gtwCharge.JR_JH_InternalJob = gatewayJob.PK;
				gtwCharge.JR_GB_InternalBranch = gtwCharge.JR_GB;
				gtwCharge.JR_GE_InternalDept = gtwCharge.JR_GE;

				var listing = setup.gC0002.GetApportionments(true);
				using (var control = new NewApportionmentUserControl(listing))
				{
					listing.PrepareForConsolCosting();
					var consolCost = Factory.Load<JobConsolCost>(gtwCharge.JR_E6_GatewaySellHeader);
					var apportionmentCharges = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().ToList();

					AssertEquals(3, apportionmentCharges.Count);

					var s1ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0001");
					var s2ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0002");
					var s3ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0003");

					AssertNull(new Job.Loader(setup.gC0001).Load());

					var c0001 = new BusinessObjectFactory().Load<ForwardingConsol>(setup.gC0001.PK);
					using (var c0001job = new Job.Loader(c0001).TryLoadOrCreateWithMutex())
					{
						AssertNotNull(c0001job);

						AssertNoExceptionThrown(() => s2ASCharge.JR_JobNumber = "C0001");
						AssertEquals("S0002", s2ASCharge.JR_JobNumber);
						AssertEquals(@"You have created the job C0001 on another form, but haven't saved it yet.
Please close or save other forms that use job C0001 to continue.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		#region Raw Data

		public void TestRawDataButton_MessageWhenHasNotAutoRate()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var listing = new ApportionmentListing(Factory, consol);
			using (DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm())
			using (var newApportionmentUserControl = new NewApportionmentUserControl(listing))
			{
				form.Controls.Add(newApportionmentUserControl);
				form.Show();

				newApportionmentUserControl.CostRateAuditTabPage_ForTestOnly.Show();

				var wiseRatesRawDataUserControl = ControlTestHelper.FindControls<WiseRatesRawDataUserControl>(newApportionmentUserControl).Single();
				AssertEquals("WiseRatesData button visible", true, wiseRatesRawDataUserControl.Visible);

				wiseRatesRawDataUserControl.SetDataBinding(listing, "");

				var wiseRatesRawDataButton = ControlTestHelper.FindControls<ZButton>(wiseRatesRawDataUserControl).Single();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				wiseRatesRawDataButton.PerformClick();
				AssertEquals(
					"GIVEN not autorate yet, WHEN clicking WiseRatesRawDataButton THEN should show info message",
					"Please perform Autorating in this session before you can see Raw Data from Rates Service. This information is available until the form is closed.",
					UnitTestUserNotification.Instance.LastMessage.Text);

				// simulate autorate
				var cache = Factory.GetCachedValue("AutoRatingStarerCore.RateCharges.RawResponse", () => new Dictionary<ZGuid, string>());
				cache.GetOrAdd(consol.PK, () => "test");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				wiseRatesRawDataButton.PerformClick();
				AssertNullOrEmpty(
					"GIVEN was autorated yet, WHEN clicking WiseRatesRawDataButton THEN should not show info message",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPlaceOfSupplyColumnsAndFieldAreAvailable()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			{
				AssertPlaceOfSupplyColumnsAndFieldAreAvailable(Core.Constants.CountryCodes.India, true);
			}
			AssertPlaceOfSupplyColumnsAndFieldAreAvailable(Core.Constants.CountryCodes.Australia, false);

			void AssertPlaceOfSupplyColumnsAndFieldAreAvailable(string countryCode, bool isAvailable)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var listing = new ApportionmentListing(Factory, consol);
					using (var form = new ZForm())
					using (var control = new NewApportionmentUserControl(listing))
					{
						form.Controls.Add(control);
						form.Show();

						AssertEquals(isAvailable, !control.CostSummaryGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_PlaceOfSupply).IsUnavailable);
						AssertEquals(isAvailable, control.PlaceOfSupplyDropEdit_ForTestOnly.Visible);
					}
				}
			}
		}

		public void TestWiseRatesRawDataButton_WhenDiagnosticSettingsIncludeRawDataIsTrue_ThenShowWiseRatesDataButton()
		{
			AssertWiseRatesRawDataButton(diagnosticSettingsIncludeRawData: true, expectWiseRatesDataButton: true);
		}

		public void TestWiseRatesRawDataButton_WhenDiagnosticSettingsIncludeRawDataIsFalse_ThenHideWiseRatesDataButton()
		{
			AssertWiseRatesRawDataButton(diagnosticSettingsIncludeRawData: false, expectWiseRatesDataButton: false);
		}

		void AssertWiseRatesRawDataButton(bool diagnosticSettingsIncludeRawData, bool expectWiseRatesDataButton)
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var listing = new ApportionmentListing(Factory, consol);
			using (DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, diagnosticSettingsIncludeRawData))
			using (var form = new ZForm())
			using (var newApportionmentUserControl = new NewApportionmentUserControl(listing))
			{
				form.Controls.Add(newApportionmentUserControl);
				form.Show();

				newApportionmentUserControl.CostRateAuditTabPage_ForTestOnly.Show();

				var wiseRatesRawDataUserControl = ControlTestHelper.FindControls<WiseRatesRawDataUserControl>(newApportionmentUserControl).Single();
				AssertEquals("WiseRatesData button visible", expectWiseRatesDataButton, wiseRatesRawDataUserControl.Visible);
			}
		}

		#endregion

		public void TestAuditLogNoteControlIsCorrectlyBound()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var listing = new ApportionmentListing(Factory, consol);
			using (var form = new ZForm())
			using (var control = new NewApportionmentUserControl(listing))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("Did you edit the control in the designer? Please check for unintended consequences in Component Designer generated code.", "CostsFilteredCollection", control.AuditLogNoteUserControl_ForTestOnly.GetBindingMember());
			}
		}

		public void TestHideIsApprovedWhenInvoiceApprovalEnabled()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var listing = new ApportionmentListing(Factory, consol);
			using (var form = new ZForm())
			using (var control = new NewApportionmentUserControl(listing))
			{
				form.Controls.Add(control);

				form.Show();

				Assert("IsApproved.IsUnavailable", !control.CostSummaryGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.IsApproved).IsUnavailable);
			}

			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var form = new ZForm())
			using (var control = new NewApportionmentUserControl(listing))
			{
				form.Controls.Add(control);

				form.Show();

				Assert("IsApproved.IsUnavailable", control.CostSummaryGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.IsApproved).IsUnavailable);
			}
		}

		public void TestHideNonApplicableControlsForGovtChargeCode()
		{
			var consol = Factory.New<ForwardingConsol>();
			foreach (var enableGovtChargeCode in new[] { true, false })
			{
				using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, enableGovtChargeCode))
				{
					ApportionmentListing listing = new ApportionmentListing(Factory, consol);
					using (ZForm form = new ZForm())
					using (NewApportionmentUserControl control = new NewApportionmentUserControl(listing))
					{
						form.Controls.Add(control);
						form.Show();
						AssertEquals(!enableGovtChargeCode, control.CostSummaryGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_CostGovtChargeCode).IsUnavailable);
						AssertEquals(!enableGovtChargeCode, control.CostSummaryGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_SellGovtChargeCode).IsUnavailable);
						AssertEquals(!enableGovtChargeCode, control.ApportionedChargesGrid_ForTestOnly.GetColumnStyle(AutoJobCharge.Schema.JR_CostGovtChargeCode).IsUnavailable);
						AssertEquals(!enableGovtChargeCode, control.ApportionedChargesGrid_ForTestOnly.GetColumnStyle(AutoJobCharge.Schema.JR_SellGovtChargeCode).IsUnavailable);
					}
				}
			}
		}

		public void TestHideNonApplicableControlsForGatewaySellApportionemnt()
		{
			var gatewayConsol = Factory.New<ForwardingConsol>();
			gatewayConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			gatewayConsol.JK_AgentType = Core.Constants.AgentType.Agent;
			gatewayConsol.JK_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			gatewayConsol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			gatewayConsol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var port = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			port.O5_PortOrCountry = gatewayConsol.JK_RL_NKLoadPort;
			port.O5_OA_AgentOfficeAddress = gatewayConsol.JK_OA_SendingForwarderAddress;
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			gatewayConsol.SendingForwarder.AppointedGatewayAgentPorts.Add(port);
			Assert("Is Gateway", gatewayConsol.IsGateway());

			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			ApportionmentListing listing = new ApportionmentListing(Factory, gatewayConsol, true);
			using (ZForm form = new ZForm())
			using (NewApportionmentUserControl control = new NewApportionmentUserControl(listing))
			{
				form.Controls.Add(control);
				form.Show();
				Assert(control.CostSummaryGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_CostGovtChargeCode).IsUnavailable);
				Assert(control.CostSummaryGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_SellGovtChargeCode).IsUnavailable);
				Assert(control.CostSummaryGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_IsForCollectInvoice).IsUnavailable);
				Assert(control.ApportionedChargesGrid_ForTestOnly.GetColumnStyle(AutoJobCharge.Schema.JR_CostGovtChargeCode).IsUnavailable);
				Assert(control.ApportionedChargesGrid_ForTestOnly.GetColumnStyle(AutoJobCharge.Schema.JR_SellGovtChargeCode).IsUnavailable);
				Assert(!control.IncludeOnCollectCheckBox_ForTestOnly.Visible);
			}
		}

		public void TestHideNonApplicableControlsForCanada()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ApportionmentListing listing = new ApportionmentListing(Factory, consol);
			using (ZForm form = new ZForm())
			using (NewApportionmentUserControl control = new NewApportionmentUserControl(listing))
			{
				form.Controls.Add(control);

				form.Show();
				AssertEquals("ExtraTaxPanel_ForTestOnly Should not be visible", false, control.ExtraTaxPanel_ForTestOnly.Visible);

				AssertEquals(true, control.CostSummaryGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_OSExtraTaxAmount).IsUnavailable);
				AssertEquals(true, control.CostSummaryGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_OSGSTRealAmount).IsUnavailable);
			}

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Canada);
			Factory.Save();
			AssertEquals("Company should  be in Canada ", true, GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada);

			using (ZForm form = new ZForm())
			using (NewApportionmentUserControl control = new NewApportionmentUserControl(listing))
			{
				form.Controls.Add(control);

				form.Show();
				AssertEquals("ExtraTaxPanel_ForTestOnly Should be visible", true, control.ExtraTaxPanel_ForTestOnly.Visible);
				AssertEquals("ExtraTaxAmountCalcFindBox_ForTestOnly binding should be changed", "CostsFilteredCollection.E6_OSExtraTaxAmount", control.ExtraTaxAmountCalcFindBox_ForTestOnly.BindToAmount);
				AssertEquals("ExtraTaxAmountCalcFindBox_ForTestOnly caption should be changed", "QST Amount", control.ExtraTaxAmountCalcFindBox_ForTestOnly.GetExtension<ILabelCaptionRenderer>().Caption);

				AssertColumnStyle(control.CostSummaryGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_OSExtraTaxAmount), false, null, "QST Amount", "QST Amt");
				AssertColumnStyle(control.CostSummaryGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_OSGSTRealAmount), false, null, "GST Amount", "GST Amt");
			}
		}

		public void TestHideNonApplicableControlsForIndia()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ApportionmentListing listing = new ApportionmentListing(Factory, consol);
			using (ZForm form = new ZForm())
			using (NewApportionmentUserControl control = new NewApportionmentUserControl(listing))
			{
				form.Controls.Add(control);

				form.Show();
				AssertEquals("ExtraTaxPanel_ForTestOnly Should not be visible", false, control.ExtraTaxPanel_ForTestOnly.Visible);

				AssertEquals(true, control.CostSummaryGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_OSExtraTaxAmount).IsUnavailable);
				AssertEquals(true, control.CostSummaryGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_OSGSTRealAmount).IsUnavailable);
			}

			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.India);
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.CountryCodes.India);

			using (ZForm form = new ZForm())
			using (NewApportionmentUserControl control = new NewApportionmentUserControl(listing))
			{
				form.Controls.Add(control);

				form.Show();
				try
				{
					AssertEquals("ExtraTaxPanel_ForTestOnly Should be visible", true, control.ExtraTaxPanel_ForTestOnly.Visible);
					AssertEquals("ExtraTaxAmountCalcFindBox_ForTestOnly binding should be changed", "CostsFilteredCollection.E6_OSExtraTaxAmount", control.ExtraTaxAmountCalcFindBox_ForTestOnly.BindToAmount);
					AssertEquals("ExtraTaxAmountCalcFindBox_ForTestOnly caption should be changed", "SGST Amount", control.ExtraTaxAmountCalcFindBox_ForTestOnly.GetExtension<ILabelCaptionRenderer>().Caption);

					AssertColumnStyle(control.CostSummaryGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_OSExtraTaxAmount), false, null, "SGST Amount", "SGST Amt");
					AssertColumnStyle(control.CostSummaryGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_OSGSTRealAmount), false, null, "CGST/IGST Amount", "GST Amount");
				}
				finally
				{
					GlbCompany.CurrentCompany.SetCountry(oldCountry);
				}
			}
		}

		public void TestHideNonApplicableControlsForMexico()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ApportionmentListing listing = new ApportionmentListing(Factory, consol);
			using (ZForm form = new ZForm())
			using (NewApportionmentUserControl control = new NewApportionmentUserControl(listing))
			{
				form.Controls.Add(control);

				form.Show();
				AssertEquals("ExtraTaxPanel_ForTestOnly Should not be visible", false, control.ExtraTaxPanel_ForTestOnly.Visible);

				AssertEquals(true, control.CostSummaryGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_OSExtraTaxAmount).IsUnavailable);
				AssertEquals(true, control.CostSummaryGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_OSGSTRealAmount).IsUnavailable);
			}

			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Mexico);
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.CountryCodes.Mexico);

			using (ZForm form = new ZForm())
			using (NewApportionmentUserControl control = new NewApportionmentUserControl(listing))
			{
				form.Controls.Add(control);

				form.Show();

				try
				{
					AssertEquals("ExtraTaxPanel_ForTestOnly Should be visible", true, control.ExtraTaxPanel_ForTestOnly.Visible);
					AssertEquals("ExtraTaxAmountCalcFindBox_ForTestOnly binding should be changed", "CostsFilteredCollection.E6_OSExtraTaxAmount", control.ExtraTaxAmountCalcFindBox_ForTestOnly.BindToAmount);
					AssertEquals("ExtraTaxAmountCalcFindBox_ForTestOnly caption should be changed", "RET Amount", control.ExtraTaxAmountCalcFindBox_ForTestOnly.GetExtension<ILabelCaptionRenderer>().Caption);

					AssertColumnStyle(control.CostSummaryGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_OSExtraTaxAmount), false, null, "RET Amount", "RET Amt");
					AssertColumnStyle(control.CostSummaryGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_OSGSTRealAmount), false, null, "IVA Amount", "IVA Amt");
				}
				finally
				{
					GlbCompany.CurrentCompany.SetCountry(oldCountry);
				}
			}
		}

		public void TestQuickCalculateMenuExists()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ApportionmentListing listing = new ApportionmentListing(Factory, consol);
			using (ZForm form = new ZForm())
			using (NewApportionmentUserControl control = new NewApportionmentUserControl(listing))
			{
				form.Controls.Add(control);

				control.OnLoad_ForTestOnly(EventArgs.Empty);
				MenuItem quickCalculateMenuItem = control.CostSummaryGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Quick Calculate");
				AssertNotNull(quickCalculateMenuItem);
			}
		}

		public void TestDeleteGwSellApportionmentMenuExists()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertEquals(false, consol.IsGateway());
			var listing = new ApportionmentListing(Factory, consol);
			using (var form = new ZForm())
			using (var control = new NewApportionmentUserControl(listing))
			{
				form.Controls.Add(control);
				control.OnLoad_ForTestOnly(EventArgs.Empty);
				var menuItem = control.CostSummaryGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Reverse/Delete Gateway Sell Apportionment");
				AssertNull(menuItem);
			}

			var gatewayConsol = Factory.New<ForwardingConsol>();
			gatewayConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			gatewayConsol.JK_AgentType = Core.Constants.AgentType.Agent;
			gatewayConsol.JK_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			gatewayConsol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			gatewayConsol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var port = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			port.O5_PortOrCountry = gatewayConsol.JK_RL_NKLoadPort;
			port.O5_OA_AgentOfficeAddress = gatewayConsol.JK_OA_SendingForwarderAddress;
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			gatewayConsol.SendingForwarder.AppointedGatewayAgentPorts.Add(port);
			Assert("Is Gateway", gatewayConsol.IsGateway());

			listing = new ApportionmentListing(Factory, gatewayConsol, true);

			using (var form = new ZForm())
			using (var control = new NewApportionmentUserControl(listing))
			{
				form.Controls.Add(control);

				control.OnLoad_ForTestOnly(EventArgs.Empty);
				var menuItem = control.CostSummaryGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Reverse/Delete Gateway Sell Apportionment");
				AssertNotNull(menuItem);
			}
		}

		public void TestAutoRatingNotePopupButtonShouldShowLongTextCorrectly()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var listing = new ApportionmentListing(Factory, consol);
			using (var form = new ZForm())
			using (var control = new NewApportionmentUserControl(listing))
			{
				form.Controls.Add(control);
				form.Show();

				Assert(control.AutoRatingNotePopupButton_ForTestOnly.AutoSize);
				Assert(control.AutoRatingNotePopupButton_ForTestOnly.AutoSizeMode == AutoSizeMode.GrowOnly);

				var originalWidth = control.AutoRatingNotePopupButton_ForTestOnly.Width;
				control.AutoRatingNotePopupButton_ForTestOnly.Text = "LONG TEXT | LONG TEXT | LONG TEXT | LONG TEXT | LONG TEXT | ";
				Assert(control.AutoRatingNotePopupButton_ForTestOnly.Width > originalWidth);
			}
		}

		public void TestAutoRatingNotePopupButton_IsOnProperTabAndHasCorrectMessage()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var listing = new ApportionmentListing(Factory, consol);

			using (var form = new ZForm())
			using (var control = new NewApportionmentUserControlForTest(listing))
			{
				form.Controls.Add(control);
				form.Show();
				control.CostRateAuditTabPage_ForTestOnly.Show();

				Assert("Control should be placed on the Rate Audit Tab", control.AutoRatingNotePopupButton_ForTestOnly.Parent == control.CostRateAuditTabPage_ForTestOnly);
				Assert("AutoRating Log button error message should be not default", control.AutoRatingNotePopupButton_ForTestOnly.NoNoteExistsError.StartsWith("You must perform Autorating in this session before you can see Full Autorating log information. This information is available until the form is closed."));
			}
		}

		public void TestChargeGroupRelatedGridColumns()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var listing = new ApportionmentListing(Factory, consol);
			using var form = new ZForm();
			using var control = new NewApportionmentUserControlForTest(listing);
			form.Controls.Add(control);
			form.Show();

			var chargeGroupColumnStyle = control.CostSummaryGrid_Exposed.GetColumnStyle(BaseCharge.Schema.ChargeGroup);
			var chargeCodeSubGroupColumnStyle = control.CostSummaryGrid_Exposed.GetColumnStyle(BaseCharge.Schema.ChargeCodeSubGroup);

			AssertNotNull(chargeGroupColumnStyle);
			AssertNotNull(chargeCodeSubGroupColumnStyle);

			AssertEquals("Charge Group", chargeGroupColumnStyle.CaptionResourceString.Caption);
			AssertEquals("Charge Code Sub Group", chargeCodeSubGroupColumnStyle.CaptionResourceString.Caption);

			AssertEquals("Charge Group column in JobChargeBoundGrid should be available.", false, chargeGroupColumnStyle.IsUnavailable);
			AssertEquals("Charge Code Sub Group column in JobChargeBoundGrid should be available.", false, chargeCodeSubGroupColumnStyle.IsUnavailable);

			AssertEquals("Charge Group column in JobChargeBoundGrid should be invisible for default.", false, chargeGroupColumnStyle.IsVisible);
			AssertEquals("Charge Code Sub Group column in JobChargeBoundGrid should be invisible for default.", false, chargeCodeSubGroupColumnStyle.IsVisible);

			AssertEquals("Charge Group column in JobChargeBoundGrid should be read-only.", true, chargeGroupColumnStyle.IsReadOnly);
			AssertEquals("Charge Code Sub Group column in JobChargeBoundGrid should be read-only.", true, chargeCodeSubGroupColumnStyle.IsReadOnly);
		}

		public void TestUserControlIsNotEditableWhenConsolJobInvoicingEditSecurityCheckPointIsSetNotAllowed()
		{
			ForwardingConsol gatewayConsol = CreateGatewayConsol(Factory);
			Assert("Pre-condition", gatewayConsol.IsGateway());
			Env.Security.GatewayConsolJobInvoicingEnterOrModify.IsAllowed = false;

			AssertEditStatus(Factory, gatewayConsol, false);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Assert("Pre-condition", !consol.IsGateway());
			Env.Security.MaintainConsolJobInvoicingEnterOrModify.IsAllowed = false;

			AssertEditStatus(Factory, consol, false);
		}

		public void TestUserControlIsEditableWhenConsolJobInvoicingEditSecurityCheckPointIsSetAllowed()
		{
			ForwardingConsol gatewayConsol = CreateGatewayConsol(Factory);
			Assert("Pre-condition", gatewayConsol.IsGateway());
			Env.Security.GatewayConsolJobInvoicingEnterOrModify.IsAllowed = true;

			AssertEditStatus(Factory, gatewayConsol, true);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Assert("Pre-condition", !consol.IsGateway());
			Env.Security.MaintainConsolJobInvoicingEnterOrModify.IsAllowed = true;

			AssertEditStatus(Factory, consol, true);
		}

		public void TestTaxDateEditControl()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var listing = new ApportionmentListing(Factory, consol);
			using (var form = new ZForm())
			using (var control = new NewApportionmentUserControl(listing))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(true, control.TaxDateEdit_ForTestOnly.Visible);
				var taxDateColumn = control.CostSummaryGrid_ForTestOnly.GetColumnStyle(JobConsolCost.Schema.E6_TaxDate);
				AssertNotNull(taxDateColumn);
				AssertEquals(false, taxDateColumn.IsVisible);
			}
		}

		public void TestCostRateAuditTextBoxUsesMonospaceFont()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var listing = new ApportionmentListing(Factory, consol);

			using var form = new ZForm();
			using var control = new NewApportionmentUserControl(listing);

			form.Controls.Add(control);
			form.Show();

			AssertEquals("Lucida Console", control.CostRateAuditTextBox_ForTestOnly.Font.Name);
		}

		void AssertEditStatus(BusinessObjectFactory factory, ForwardingConsol consol, bool expectedStatus)
		{
			var listing = new ApportionmentListing(Factory, consol, consol.IsGateway());
			using (var form = new ZForm())
			using (var control = new NewApportionmentUserControl(listing))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("Status of DetailTabPage_ForTestOnly is not correct.", expectedStatus, control.DetailTabPage_ForTestOnly.Enabled);
				AssertEquals("Status of CostRateAuditTabPage_ForTestOnly is not correct.", expectedStatus, control.CostRateAuditTabPage_ForTestOnly.Enabled);
			}
		}

		void AssertColumnStyle(ZGridColumnInfo zGridColumnInfo, bool isUnavailable, string caption, string captionResourceStringCaption, string captionResourceStringShortCaption)
		{
			AssertEquals(isUnavailable, zGridColumnInfo.IsUnavailable);
			AssertEquals(caption, zGridColumnInfo.Caption);
			AssertEquals(captionResourceStringCaption, zGridColumnInfo.CaptionResourceString.Caption);
			AssertEquals(captionResourceStringShortCaption, zGridColumnInfo.CaptionResourceString.ShortCaption);
		}

		static ForwardingConsol CreateGatewayConsol(BusinessObjectFactory factory)
		{
			var gatewayConsol = factory.NewWithValidTestData<ForwardingConsol>();
			gatewayConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			gatewayConsol.JK_UniqueConsignRef = "JB007";
			gatewayConsol.JK_AgentType = Core.Constants.AgentType.Agent;
			gatewayConsol.JK_RL_NKLoadPort = "AUSYD";
			gatewayConsol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			gatewayConsol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var port = factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			port.O5_PortOrCountry = "AUSYD";
			port.O5_OA_AgentOfficeAddress = gatewayConsol.JK_OA_SendingForwarderAddress;
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			gatewayConsol.SendingForwarder.AppointedGatewayAgentPorts.Add(port);

			var consolJob = new JobHeader.Loader(gatewayConsol).TryLoadOrCreateWithoutMutexForTestOnly();
			consolJob.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-8);
			factory.Save();
			return gatewayConsol;
		}

		TestObjectCreator TestObjectCreator
		{
			get { return fCreator ?? (fCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fCreator;
	}
}
