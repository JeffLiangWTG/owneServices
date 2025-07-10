using System;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business.Testing
{
	class JobComInvoiceHeaderApportionTest : Customs.Business.Testing.BaseJobComInvoiceHeaderApportionTest
	{
		public new void TestApportionChargeWhenChargeIsRelevantForIncoTerm()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				var testDeclaration = GetNewDeclaration();
				testDeclaration.AutoCreateChargesBasedOnIncoTerm = false;
				var allInvoicesGroup = testDeclaration.JobComInvoiceGroupHeaders[0];
				var header1 = allInvoicesGroup.JobComInvoiceHeaders.AddNew();
				header1.JZ_JE = testDeclaration.PK;
				var groupLanding = allInvoicesGroup.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.AdditionCharge);
				groupLanding.J7_Amount = 100m;
				groupLanding.J7_RX_NKCurrency = allInvoicesGroup.JobDeclaration.LocalCurrencyCode;
				var header2 = allInvoicesGroup.JobComInvoiceHeaders.AddNew();
				header1.JZ_IncoTerm = "FOB";
				header1.JZ_InvoiceAmount = 1000m;
				header1.JZ_RX_NKInvoice_Currency = allInvoicesGroup.JobDeclaration.LocalCurrencyCode;
				header2.JZ_JE = testDeclaration.PK;
				header2.JZ_IncoTerm = "CIF";
				header2.JZ_InvoiceAmount = 2000m;
				header2.JZ_RX_NKInvoice_Currency = allInvoicesGroup.JobDeclaration.LocalCurrencyCode;
				testDeclaration.ResumeApportionment();
				AssertEquals("Landing Charge is apportioned to FOB invoice", 33.33M, header1.GroupCharges[0].J7_Amount);
				AssertEquals("Landing Charge is apportioned to CIF invoice", 66.67M, header2.GroupCharges[0].J7_Amount);
			}
		}

		public new void TestApportionLandingCharges()
		{
			var testDeclaration = GetNewDeclaration();
			testDeclaration.AutoCreateChargesBasedOnIncoTerm = false;
			var allInvoicesGroup = testDeclaration.JobComInvoiceGroupHeaders[0];
			var header1 = allInvoicesGroup.JobComInvoiceHeaders.AddNew();
			header1.JZ_JE = testDeclaration.PK;
			var chargeKey = new ChargeCodeChargeKey(Customs.Business.CustomsChargeTypeList.Codes.LandingCharges, false, false);
			header1.JZ_RX_NKInvoice_Currency = header1.JobDeclaration.LocalCurrencyCode;
			header1.JZ_InvoiceAmount = 1000m;
			header1.JZ_IncoTerm = "CIF";
			allInvoicesGroup.Charges.AddNew(chargeKey.ChargeCode, 150m, header1.JobDeclaration.LocalCurrencyCode);
			testDeclaration.ResumeApportionment();
			AssertEquals(0, header1.GroupCharges.Count);
		}

		public new void TestChangeIncoTermUpdateApportion()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				var testDeclaration = GetNewDeclaration();
				testDeclaration.AutoCreateChargesBasedOnIncoTerm = false;
				var allInvoicesGroup = testDeclaration.JobComInvoiceGroupHeaders[0];
				var header1 = allInvoicesGroup.JobComInvoiceHeaders.AddNew();
				header1.JZ_JE = testDeclaration.PK;
				var groupLanding = allInvoicesGroup.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.AdditionCharge);
				groupLanding.J7_Amount = 100m;
				groupLanding.J7_RX_NKCurrency = allInvoicesGroup.JobDeclaration.LocalCurrencyCode;
				var header2 = allInvoicesGroup.JobComInvoiceHeaders.AddNew();
				header2.JZ_JE = testDeclaration.PK;
				header1.JZ_IncoTerm = "FOB";
				header1.JZ_InvoiceAmount = 2000m;
				header1.JZ_RX_NKInvoice_Currency = allInvoicesGroup.JobDeclaration.LocalCurrencyCode;
				header2.JZ_IncoTerm = "CIF";
				header2.JZ_InvoiceAmount = 2000m;
				header2.JZ_RX_NKInvoice_Currency = allInvoicesGroup.JobDeclaration.LocalCurrencyCode;
				testDeclaration.ResumeApportionment();
				AssertEquals("Landing Charge is apportioned to Header1", 50m, header1.GroupCharges[0].J7_Amount);
				AssertEquals("Landing Charge is apportioned to Header2", 50m, header2.GroupCharges[0].J7_Amount);
			}
		}

		protected override Customs.Business.BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();
	}
}
