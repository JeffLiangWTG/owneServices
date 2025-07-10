using System;
using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CustomsChargeTypeList = Enterprise.Customs.Common.CustomsChargeTypeList;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(JobComInvoiceGroupHeader))]
	sealed class JobComInvoiceGroupHeaderTest : Customs.Business.Testing.BaseJobComInvoiceGroupHeaderTest
	{
		public void TestImportIncoTermAndChargeFactory()
		{
			var declaration = Factory.New<JobDeclaration>();
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertType<ImportIncoTermAndCustomsChargeFactory>(groupHeader.IncoTermAndChargeFactory);
		}

		public void TestExportIncoTermAndChargeFactory()
		{
			var declaration = Factory.New<JobDeclaration>();
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertType<ExportIncoTermAndCustomsChargeFactory>(groupHeader.IncoTermAndChargeFactory);
		}

		public new void TestApportion()
		{
			var testDec = GetNewDeclarationForTest();
			var groupHeader = testDec.JobComInvoiceGroupHeaders[0];

			var groupHeader1 = groupHeader.JobComInvoiceGroupHeaders.AddNew();
			var oNS = groupHeader1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 1000, testDec.LocalCurrencyCode);

			if (DistributeByShouldBeChangedForApportion)
			{
				oNS.J7_DistributeBy = DistributedByForApportionDefaultValue;
			}

			var oNSKey = oNS.ChargeKey;

			var groupHeader2 = groupHeader1.JobComInvoiceGroupHeaders.AddNew();
			var invoice1 = groupHeader1.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_InvoiceAmount = 1000m;
			invoice1.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

			var invoice2 = groupHeader1.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_InvoiceAmount = 1000m;
			invoice2.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

			var invoice3 = groupHeader2.JobComInvoiceHeaders.AddNew();
			invoice3.JZ_InvoiceAmount = 1000m;
			invoice3.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

			testDec.ResumeApportionment();
			AssertEquals("Apportioned ONS", 333.3m, decimal.Round(invoice1.GroupCharges.GetCharge(oNSKey).Amount, 1));
			AssertEquals("Apportioned ONS", 333.3m, decimal.Round(invoice2.GroupCharges.GetCharge(oNSKey).Amount, 1));
			AssertEquals("Apportioned ONS", 333.3m, decimal.Round(invoice3.GroupCharges.GetCharge(oNSKey).Amount, 1));

			invoice3.JZ_InvoiceAmount = 3000m;
			testDec.ResumeApportionment();
			AssertEquals("Apportioned ONS", 200m, invoice1.GroupCharges.GetCharge(oNSKey).Amount);
			AssertEquals("Apportioned ONS", 200m, invoice2.GroupCharges.GetCharge(oNSKey).Amount);
			AssertEquals("Apportioned ONS", 600m, invoice3.GroupCharges.GetCharge(oNSKey).Amount);
		}

		public new void TestApportionWhenSubGroupChargeGetsCleared()
		{
			var testDec = GetNewDeclarationForTest();
			testDec.AutoCreateChargesBasedOnIncoTerm = false;
			var groupHeader = testDec.JobComInvoiceGroupHeaders[0];

			var groupHeader1 = groupHeader.JobComInvoiceGroupHeaders.AddNew();
			var groupHeader2 = groupHeader1.JobComInvoiceGroupHeaders.AddNew();

			var invoice1 = groupHeader1.JobComInvoiceHeaders.AddNew();
			var invoice2 = groupHeader1.JobComInvoiceHeaders.AddNew();
			var invoice3 = groupHeader2.JobComInvoiceHeaders.AddNew();
			var invoice4 = groupHeader2.JobComInvoiceHeaders.AddNew();

			var charge = groupHeader1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 1000, testDec.LocalCurrencyCode);
			var charge2 = groupHeader2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 600, testDec.LocalCurrencyCode);
			var oNSKey = groupHeader1.Charges[0].ChargeKey;

			if (DistributeByShouldBeChangedForApportion)
			{
				charge.J7_DistributeBy = DistributedByForApportionDefaultValue;
				charge2.J7_DistributeBy = DistributedByForApportionDefaultValue;
			}

			invoice1.JZ_InvoiceAmount = 1000m;
			invoice1.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice2.JZ_InvoiceAmount = 1000m;
			invoice2.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice3.JZ_InvoiceAmount = 1000m;
			invoice3.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice4.JZ_InvoiceAmount = 1000m;
			invoice4.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			testDec.ResumeApportionment();

			AssertEquals("PreCondition:Apportioned Insurance for invoice1", 200m, invoice1.GroupCharges.GetCharge(oNSKey).Amount);
			AssertEquals("PreCondition:Apportioned Insurance for invoice2", 200m, invoice2.GroupCharges.GetCharge(oNSKey).Amount);
			AssertEquals("PreCondition:Apportioned Insurance for invoice3", 300m, invoice3.GroupCharges.GetCharge(oNSKey).Amount);
			AssertEquals("PreCondition:Apportioned Insurance for invoice4", 300m, invoice4.GroupCharges.GetCharge(oNSKey).Amount);

			groupHeader2.Charges[0].Delete();
			testDec.ResumeApportionment();
			AssertEquals("Apportioned Insurance for invoice1", 250m, invoice1.GroupCharges.GetCharge(oNSKey).Amount);
			AssertEquals("Apportioned Insurance for invoice2", 250m, invoice2.GroupCharges.GetCharge(oNSKey).Amount);
			AssertEquals("Apportioned Insurance for invoice3", 250m, invoice3.GroupCharges.GetCharge(oNSKey).Amount);
			AssertEquals("Apportioned Insurance for invoice4", 250m, invoice4.GroupCharges.GetCharge(oNSKey).Amount);
		}

		public new void TestApportionWhenSubGroupComesToHaveCharge()
		{
			var testDec = GetNewDeclarationForTest();
			var groupHeader = testDec.JobComInvoiceGroupHeaders[0];

			var groupHeader1 = groupHeader.JobComInvoiceGroupHeaders.AddNew();
			var groupHeader2 = groupHeader1.JobComInvoiceGroupHeaders.AddNew();

			var invoice1 = groupHeader1.JobComInvoiceHeaders.AddNew();
			var invoice2 = groupHeader1.JobComInvoiceHeaders.AddNew();
			var invoice3 = groupHeader2.JobComInvoiceHeaders.AddNew();
			var invoice4 = groupHeader2.JobComInvoiceHeaders.AddNew();

			var charge = groupHeader1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 1000, testDec.LocalCurrencyCode);
			var oNSKey = charge.ChargeKey;

			if (DistributeByShouldBeChangedForApportion)
			{
				charge.J7_DistributeBy = DistributedByForApportionDefaultValue;
			}

			invoice1.JZ_InvoiceAmount = 1000m;
			invoice1.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice2.JZ_InvoiceAmount = 1000m;
			invoice2.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice3.JZ_InvoiceAmount = 1000m;
			invoice3.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice4.JZ_InvoiceAmount = 1000m;
			invoice4.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

			var charge2 = groupHeader2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 600, testDec.LocalCurrencyCode);

			if (DistributeByShouldBeChangedForApportion)
			{
				charge2.J7_DistributeBy = DistributedByForApportionDefaultValue;
			}
			testDec.ResumeApportionment();
			AssertEquals("Apportioned Insurance for invoice1", 200m, invoice1.GroupCharges.GetCharge(oNSKey).Amount);
			AssertEquals("Apportioned Insurance for invoice2", 200m, invoice2.GroupCharges.GetCharge(oNSKey).Amount);
			AssertEquals("Apportioned Insurance for invoice3", 300m, invoice3.GroupCharges.GetCharge(oNSKey).Amount);
			AssertEquals("Apportioned Insurance for invoice4", 300m, invoice4.GroupCharges.GetCharge(oNSKey).Amount);
		}

		public new void TestApportionWhenSubGroupDoesntHaveCharge()
		{
			var testDec = GetNewDeclarationForTest();
			var groupHeader = testDec.JobComInvoiceGroupHeaders[0];

			var groupHeader1 = groupHeader.JobComInvoiceGroupHeaders.AddNew();
			var groupHeader2 = groupHeader1.JobComInvoiceGroupHeaders.AddNew();

			var invoice1 = groupHeader1.JobComInvoiceHeaders.AddNew();
			var invoice2 = groupHeader1.JobComInvoiceHeaders.AddNew();
			var invoice3 = groupHeader2.JobComInvoiceHeaders.AddNew();
			var invoice4 = groupHeader2.JobComInvoiceHeaders.AddNew();

			var charge = groupHeader1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 1000, testDec.LocalCurrencyCode);
			var oNSKey = charge.ChargeKey;

			if (DistributeByShouldBeChangedForApportion)
			{
				charge.J7_DistributeBy = DistributedByForApportionDefaultValue;
			}

			invoice1.JZ_InvoiceAmount = 1000m;
			invoice1.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice2.JZ_InvoiceAmount = 1000m;
			invoice2.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice3.JZ_InvoiceAmount = 1000m;
			invoice3.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice4.JZ_InvoiceAmount = 1000m;
			invoice4.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			testDec.ResumeApportionment();
			AssertEquals("Apportioned Insurance for invoice1", 250m, invoice1.GroupCharges.GetCharge(oNSKey).Amount);
			AssertEquals("Apportioned Insurance for invoice2", 250m, invoice2.GroupCharges.GetCharge(oNSKey).Amount);
			AssertEquals("Apportioned Insurance for invoice3", 250m, invoice3.GroupCharges.GetCharge(oNSKey).Amount);
			AssertEquals("Apportioned Insurance for invoice4", 250m, invoice4.GroupCharges.GetCharge(oNSKey).Amount);
		}

		public new void TestApportionWhenSubGroupHasItsOwnCharge()
		{
			var testDec = GetNewDeclarationForTest();
			var groupHeader = testDec.JobComInvoiceGroupHeaders[0];

			var groupHeader1 = groupHeader.JobComInvoiceGroupHeaders.AddNew();
			var groupHeader2 = groupHeader1.JobComInvoiceGroupHeaders.AddNew();

			var invoice1 = groupHeader1.JobComInvoiceHeaders.AddNew();
			var invoice2 = groupHeader1.JobComInvoiceHeaders.AddNew();
			var invoice3 = groupHeader2.JobComInvoiceHeaders.AddNew();
			var invoice4 = groupHeader2.JobComInvoiceHeaders.AddNew();

			var charge = groupHeader1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 1000, testDec.LocalCurrencyCode);
			var charge2 = groupHeader2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 600, testDec.LocalCurrencyCode);

			if (DistributeByShouldBeChangedForApportion)
			{
				charge.J7_DistributeBy = DistributedByForApportionDefaultValue;
				charge2.J7_DistributeBy = DistributedByForApportionDefaultValue;
			}

			var oNSKey = charge.ChargeKey;

			invoice1.JZ_InvoiceAmount = 1000m;
			invoice1.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice2.JZ_InvoiceAmount = 1000m;
			invoice2.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice3.JZ_InvoiceAmount = 1000m;
			invoice3.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice4.JZ_InvoiceAmount = 1000m;
			invoice4.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			testDec.ResumeApportionment();
			AssertEquals("Apportioned Insurance for invoice1", 200m, invoice1.GroupCharges.GetCharge(oNSKey).Amount);
			AssertEquals("Apportioned Insurance for invoice2", 200m, invoice2.GroupCharges.GetCharge(oNSKey).Amount);
			AssertEquals("Apportioned Insurance for invoice3", 300m, invoice3.GroupCharges.GetCharge(oNSKey).Amount);
			AssertEquals("Apportioned Insurance for invoice4", 300m, invoice4.GroupCharges.GetCharge(oNSKey).Amount);
		}

		public new void TestApportionWithInvoicesWithoutSubGroupCharges()
		{
			var testDec = GetNewDeclarationForTest();
			var groupHeader1 = testDec.JobComInvoiceGroupHeaders[0];
			var groupHeader2 = groupHeader1.JobComInvoiceGroupHeaders.AddNew();
			groupHeader2.JZ_InvoiceNumber = "GroupInvoice2";
			var groupHeader3 = groupHeader2.JobComInvoiceGroupHeaders.AddNew();
			groupHeader3.JZ_InvoiceNumber = "GroupInvoice3";
			var invoice1 = groupHeader1.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_InvoiceNumber = "INV1";
			var invoice2 = groupHeader1.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_InvoiceNumber = "INV2";
			var invoice3 = groupHeader2.JobComInvoiceHeaders.AddNew();
			invoice3.JZ_InvoiceNumber = "INV3";
			var invoice4 = groupHeader2.JobComInvoiceHeaders.AddNew();
			invoice4.JZ_InvoiceNumber = "INV4";
			var invoice5 = groupHeader3.JobComInvoiceHeaders.AddNew();
			invoice5.JZ_InvoiceNumber = "INV5";
			var invoice6 = groupHeader3.JobComInvoiceHeaders.AddNew();
			invoice6.JZ_InvoiceNumber = "INV6";

			invoice1.JZ_InvoiceAmount = 1000m;
			invoice1.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice2.JZ_InvoiceAmount = 1000m;
			invoice2.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice3.JZ_InvoiceAmount = 1000m;
			invoice3.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice4.JZ_InvoiceAmount = 1000m;
			invoice4.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice5.JZ_InvoiceAmount = 1000m;
			invoice5.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice6.JZ_InvoiceAmount = 1000m;
			invoice6.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

			var charge1 = groupHeader1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 2000m, testDec.LocalCurrencyCode);
			var charge2 = groupHeader3.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 1000m, testDec.LocalCurrencyCode);
			var oNSKey = charge1.ChargeKey;

			if (DistributeByShouldBeChangedForApportion)
			{
				charge1.J7_DistributeBy = DistributedByForApportionDefaultValue;
				charge2.J7_DistributeBy = DistributedByForApportionDefaultValue;
			}

			testDec.ResumeApportionment();
			AssertEquals("Apportioned Charges", 250m, invoice1.GroupCharges.GetCharge(oNSKey).Amount);
			AssertEquals("Apportioned Charges", 250m, invoice2.GroupCharges.GetCharge(oNSKey).Amount);
			AssertEquals("Apportioned Charges", 250m, invoice3.GroupCharges.GetCharge(oNSKey).Amount);
			AssertEquals("Apportioned Charges", 250m, invoice4.GroupCharges.GetCharge(oNSKey).Amount);
			AssertEquals("Apportioned Charges", 500m, invoice5.GroupCharges.GetCharge(oNSKey).Amount);
			AssertEquals("Apportioned Charges", 500m, invoice6.GroupCharges.GetCharge(oNSKey).Amount);
		}

		public override void TestChargesToImportForLandedCosting()
		{
			var testDec = GetNewDeclarationForTest();
			var groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			var fOBInvoice = groupHeader.JobComInvoiceHeaders.AddNew();
			fOBInvoice.JZ_IncoTerm = "FOB";
			fOBInvoice.JZ_InvoiceAmount = 10000m;
			fOBInvoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

			var groupCharges = groupHeader.Charges;
			groupCharges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 1000m, testDec.LocalCurrencyCode);
			groupCharges.AddNew(CustomsChargeTypeList.Codes.DeductionCharge, 100m, testDec.LocalCurrencyCode);
			var groupCommission = groupCharges.AddNew(CustomsChargeTypeList.Codes.Commission, 100m, testDec.LocalCurrencyCode);
			testDec.ResumeApportionment();
			AssertEquals("FOB Invoice has three charges apportioned", 3, fOBInvoice.GroupCharges.Count);
			AssertEquals("First row is ONS", CustomsChargeTypeList.Codes.OverseasInsurance, fOBInvoice.GroupCharges[0].J7_ChargeType);
			AssertEquals("Apportioned ONS not included in lines", false, fOBInvoice.GroupCharges[0].J7_IsIncludedInITOT);

			AssertEquals("Second row is DED", CustomsChargeTypeList.Codes.DeductionCharge, fOBInvoice.GroupCharges[1].J7_ChargeType);
			AssertEquals("Apportioned DED included in lines", false, fOBInvoice.GroupCharges[1].J7_IsIncludedInITOT);

			groupCommission.J7_IsIncludedInITOT = true;
			testDec.ResumeApportionment();

			var result = new List<IDefaultLandedCostInput>(((ILandedCostChargeHolder)groupHeader).ChargesToImportForLandedCosting).ToArray();
			AssertEquals("two charges in Result", 2, result.Length);
			AssertEquals("ONS", true, result[0].ChargeDescription.Contains(CustomsChargeTypeList.Descriptions.OverseasInsurance.ToString().ToUpper()));
			AssertEquals("DED should be brought", true, result[1].ChargeDescription.Contains("Deduction (or Discount) from Entry"));
			AssertEquals("DED should be brought as a negative amount", -100m, result[1].AmountToDistribute.Amount);
			AssertEquals("OTH charge should not be brought as this is included in lines", false, result[1].ChargeDescription.Contains(CustomsChargeTypeList.Descriptions.Commission.ToString().ToUpper()));
		}

		public new void TestZeroValueApportion()
		{
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				var testDec = GetNewDeclarationForTest();
				var topGroup = testDec.JobComInvoiceGroupHeaders[0];

				var charge = topGroup.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 300, testDec.LocalCurrencyCode);
				var charge2 = topGroup.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 300, testDec.LocalCurrencyCode);
				if (DistributeByShouldBeChangedForApportion)
				{
					charge.J7_DistributeBy = DistributedByForApportionDefaultValue;
					charge2.J7_DistributeBy = DistributedByForApportionDefaultValue;
				}

				var oTHKey = topGroup.Charges[0].ChargeKey;
				var oNSKey = topGroup.Charges[1].ChargeKey;

				var invoice = topGroup.JobComInvoiceHeaders.AddNew();
				invoice.JZ_InvoiceAmount = 2000m;
				invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				testDec.ResumeApportionment();
				AssertEquals("PreCondition: OTH apportioned", 300m, invoice.GroupCharges.GetCharge(oTHKey).Amount);
				AssertEquals("PreCondition: OSN apportioned", 300m, invoice.GroupCharges.GetCharge(oNSKey).Amount);

				topGroup.Charges[0].J7_Amount = 0;
				testDec.ResumeApportionment();
				AssertEquals("OTH apportioned", 0m, invoice.GroupCharges.GetCharge(oTHKey).Amount);
				AssertEquals("OSN apportioned", 300m, invoice.GroupCharges.GetCharge(oNSKey).Amount);
			}
		}

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<GroupInvoiceCharge>);

		protected override void SetUp()
		{
			base.SetUp();
			distributeByForExport = CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value);
		}

		protected override void TearDown()
		{
			base.TearDown();
			distributeByForExport?.Dispose();
		}

		IDisposable distributeByForExport;
	}
}
