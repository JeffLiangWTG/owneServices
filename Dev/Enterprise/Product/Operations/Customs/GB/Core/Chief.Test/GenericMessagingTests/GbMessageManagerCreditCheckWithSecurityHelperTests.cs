using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Customs.GB.Business.GBUniversalReferenceConstants;

namespace Enterprise.Customs.GB.Chief.Testing
{
	class GbMessageManagerCreditCheckWithSecurityHelperTests : TestCaseWithFactory
	{
		public void TestShouldCheckNotLodgedArrived()
		{
			GBCustomsDataRegistry.Instance.ChiefValidationCreditCheck_AlwaysCheck.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			GBCustomsDataRegistry.Instance.ChiefValidationCreditCheck_NotLodgedArrived.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_EntrySubStyle = "D";  // not arrived
			var shutUp = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			var helper = new GbMessageManagerCreditCheckWithSecurityHelperForTest(declaration, shutUp);
			AssertEquals("Not an arrived declaration, no need to check", false, helper.ShouldCheckExposed);
			declaration.JE_EntrySubStyle = "A";  // arrived
			AssertEquals("Arrived declaration, check", true, helper.ShouldCheckExposed);
			declaration.JE_EntrySubStyle = "D";  // not arrived
			GBCustomsDataRegistry.Instance.ChiefValidationCreditCheck_AlwaysCheck.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("'Always check' trumps others", true, helper.ShouldCheckExposed);
		}

		public void TestShouldCheckPreLodgedArriving()
		{
			GBCustomsDataRegistry.Instance.ChiefValidationCreditCheck_AlwaysCheck.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			GBCustomsDataRegistry.Instance.ChiefValidationCreditCheck_PreLodgedArriving.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_EntrySubStyle = "D";  // not arrived
			declaration.SingleEntry.EntryNumber = "12345";
			declaration.SingleEntry.CH_RouteOfEntry = "H";
			Assert("PreReq", declaration.IsPrelodgedAndNotCancelled);
			var shutUp = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			var helper = new GbMessageManagerCreditCheckWithSecurityHelperForTest(declaration, shutUp);
			AssertEquals("Prelodged but not marking as arrived", false, helper.ShouldCheckExposed);
			declaration.JE_EntrySubStyle = "A";  // arrived
			AssertEquals("Prelodged and marking as arrived", true, helper.ShouldCheckExposed);
			declaration.JE_EntrySubStyle = "D";  // not arrived
			GBCustomsDataRegistry.Instance.ChiefValidationCreditCheck_AlwaysCheck.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("'Always check' trumps others", true, helper.ShouldCheckExposed);
		}

		public void TestShouldCheckEstimateDuty()
		{
			GBCustomsDataRegistry.Instance.ChiefValidationCreditCheck_AlwaysCheck.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			GBCustomsDataRegistry.Instance.ChiefValidationCreditCheck_EstimateDuty.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 10m);
			var declaration = Factory.New<JobDeclaration>();
			var shutUp = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			var helper = new GbMessageManagerCreditCheckWithSecurityHelperForTest(declaration, shutUp);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "GBP";
			var invoiceLine = (EU.Business.Declaration.JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000;
			invoiceLine.JI_CustomsSecondQuantity = 1;
			var box47TaxLineForB00Vat = invoiceLine.Taxes.AddNew();
			box47TaxLineForB00Vat.Data.G4_Type = "B00";
			box47TaxLineForB00Vat.Data.G4_RateDuty = "S"; // S - standard VAT - 20%
			CreateTaxOrFee(TaxOrFeeTypeCode.StandardRate, "GB", .2);
			CreateTaxOrFee(TaxOrFeeTypeCode.ZeroRated, "GB", 0);
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			declaration.DoMerge();
			var entry = declaration.CustomsEntryHeaders[0];
			entry.EntryNumber = "123456";
			AssertEquals("Lodged dec, zero recorded liability, some new liability, should check", true, helper.ShouldCheckExposed);
			entry.MergedLines[0].Fees.AddOrUpdate(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, 199m);
			AssertEquals("Lodged dec, 199 recorded liability, 200 new liability, should NOT check as we have not increased by enough", false, helper.ShouldCheckExposed);
			invoiceLine.JI_LinePrice = 2000;
			AssertEquals("Lodged dec, 199 recorded liability, 400 new liability, should check as we have increased by over 10%", true, helper.ShouldCheckExposed);
			box47TaxLineForB00Vat.Data.G4_RateDuty = "Z";  // zero rate
			AssertEquals("Lodged dec, 199 recorded liability, 0 new liability, should NOT check", false, helper.ShouldCheckExposed);
			GBCustomsDataRegistry.Instance.ChiefValidationCreditCheck_AlwaysCheck.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("'Always check' trumps others", true, helper.ShouldCheckExposed);

			void CreateTaxOrFee(ZString code, ZString country, ZDecimal rate)
			{
				TestHelper.CreateTaxOrFee(code, rate, country, ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			}
		}

		[GuiTest]
		public void TestShouldAlwaysCheckDeniedParty()
		{
			GBCustomsDataRegistry.Instance.ChiefValidationCreditCheck_AlwaysCheck.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			GBCustomsDataRegistry.Instance.ChiefValidationCreditCheck_NotLodgedArrived.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var declaration = Factory.New<JobDeclarationForTest>();
			var sender = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			var helper = new GbMessageManagerCreditCheckWithSecurityHelperForTest(declaration, sender);
			Factory.Save();

			declaration.JE_EntrySubStyle = "D";  // not arrived
			AssertEquals("Not an arrived declaration, no need to check", false, helper.ShouldCheckExposed);

			declaration.DPSFreightMovementRestricted = false;
			AssertEquals("Credit Check is ignored, Denied Party OK", true, helper.WarnAboutCreditChecks());

			declaration.DPSFreightMovementRestricted = true;
			AssertEquals("Fails Denied Party when Credit Check is ignored", false, helper.WarnAboutCreditChecks());
			AssertEquals("Warning text", "Unable to submit message due to Denied Party Screening cancellation.", sender.InvalidOperationText);

			declaration.JE_EntrySubStyle = "A";  // arrived
			AssertEquals("Arrived declaration, check", true, helper.ShouldCheckExposed);
			declaration.JE_EntrySubStyle = "D";  // not arrived
			GBCustomsDataRegistry.Instance.ChiefValidationCreditCheck_AlwaysCheck.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("'Always check' trumps others", true, helper.ShouldCheckExposed);

			declaration.DPSFreightMovementRestricted = false;
			AssertEquals("Credit Check is OK", true, helper.WarnAboutCreditChecks());

			declaration.DPSFreightMovementRestricted = true;
			AssertEquals("Fails Denied Party even though Credit Check OK", false, helper.WarnAboutCreditChecks());
			AssertEquals("Warning text", "Unable to submit message due to Denied Party Screening cancellation.", sender.InvalidOperationText);
		}

		public void TestAnyKindOfGbCheckFlagResultsInBaseCreditEnquiryWithPopup()
		{
			// Asserts that if GB tells base that a check is needed, one is performed and the popup is shown
			CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			GBCustomsDataRegistry.Instance.ChiefValidationCreditCheck_AlwaysCheck.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			GBCustomsDataRegistry.Instance.CcsukLocalHostMnemonic.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "FOO");
			MawbTestHelper.MakeBadge("XYZ", GatewayList.Codes.CCSUKviaNTMsgGW, false);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsProfile = "XYZ";
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			Factory.Save();

			var newFunction = new Customs.Business.CusdecMessageFunction.New();
			var shutUp = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			var messageSender = (Customs.Business.IDeclarationMessageSender)Activator.CreateInstance(ObjectFactory.GetType<Integration.Customs.GB.IDeclarationMessageSenderChooser>());
			messageSender.Send(declaration, shutUp, newFunction);
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var importerOnCreditHold = Factory.NewWithValidTestData<OrgHeader>();
			importerOnCreditHold.OH_IsDebtor = true;
			importerOnCreditHold.CompanyData.OB_AROnCreditHold = true;
			importerOnCreditHold.OH_Code = "DJC123";
			declaration.JE_OH_Importer = importerOnCreditHold.PK;
			Factory.Save();

			CombineAssertions(() =>
			{
				importerOnCreditHold.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
				AssertEquals("IsCreditOnHold should be true for importer", true, importerOnCreditHold.CreditChecker.IsCreditOnHold());

				shutUp = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
				messageSender.Send(declaration, shutUp, newFunction);
				AssertContains("Show restriction reasons", "on Credit Hold", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestWarnAboutCreditChecks_RemoveCreditCheckErrorInExternalSystem()
		{
			MawbTestHelper.MakeBadge("XYZ", GatewayList.Codes.CCSUKviaNTMsgGW, false);
			CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			using (GBCustomsDataRegistry.Instance.ChiefValidationCreditCheck_AlwaysCheck.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_IsDebtor = true;
				org.CompanyData.OB_AROnCreditHold = true;
				org.OH_Code = "DJC123";

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_CustomsProfile = "XYZ";
				declaration.JE_OH_Supplier = org.PK;
				declaration.JE_OH_Importer = org.PK;

				Factory.Save();

				org.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();

				CombineAssertions(() =>
				{
					using (AccountingMasterFilesRegistry.Instance.CreditControlApprovalMode.SetTemporaryValue(
						GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
						AccountingMasterFilesConstants.CreditControlApprovalModes.ApproveLocallyInCW1.Code))
					{
						var shutUp = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);

						AssertEquals("Should not be allowed (CW1)", false, new GbMessageManagerCreditCheckWithSecurityHelper(declaration, shutUp).WarnAboutCreditChecks());
						AssertEquals("Submit message with credit restriction canceled.", shutUp.InvalidOperationText);
						AssertContains("Credit failure expected", "on Credit Hold", UnitTestUserNotification.Instance.LastMessage.Text);
					}

					using (AccountingMasterFilesRegistry.Instance.CreditControlApprovalMode.SetTemporaryValue(
						GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
						AccountingMasterFilesConstants.CreditControlApprovalModes.ApproveInExternalSystem.Code))
					{
						var shutUp = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);

						AssertEquals("Should not be allowed (EXT)", false, new GbMessageManagerCreditCheckWithSecurityHelper(declaration, shutUp).WarnAboutCreditChecks());
						AssertNull("No error expected", shutUp.InvalidOperationText);
						AssertNotContains("No credit failure expected", "on Credit Hold", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				});
			}
		}

		UniversalReferenceTestDataHelper TestHelper => testHelper ?? (testHelper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper testHelper;
	}

	class GbMessageManagerCreditCheckWithSecurityHelperForTest : GbMessageManagerCreditCheckWithSecurityHelper
	{
		public GbMessageManagerCreditCheckWithSecurityHelperForTest(JobDeclaration declaration, Customs.Business.ISendsMessagesToCustoms sendMessagesToCustoms)
		: base(declaration, sendMessagesToCustoms)
		{
		}

		public bool ShouldCheckExposed
		{
			get { return ShouldCheckCreditForThisDeclarationAndMessage(); }
		}
	}

	class JobDeclarationForTest : JobDeclaration
	{
		public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			DPSPartiesForTesting = Array.Empty<ScreeningParty>();
		}

		protected override bool IsDPSFreightMovementRestrictedCore()
		{
			return DPSFreightMovementRestricted;
		}
		public bool DPSFreightMovementRestricted { get; set; }

		protected override ScreeningParty[] GetScreeningPartiesCore()
		{
			return DPSPartiesForTesting;
		}
		public ScreeningParty[] DPSPartiesForTesting { get; set; }
	}
}
