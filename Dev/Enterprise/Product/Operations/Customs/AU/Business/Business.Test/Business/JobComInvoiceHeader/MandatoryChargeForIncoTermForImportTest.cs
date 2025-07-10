using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class MandatoryChargeForIncoTermForImportTest : TestCaseWithFactory
	{
		public void TestFillIncoTermMandatoryCharges()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_MessageType = "IMP";
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.LandedIntoStore;
			AssertMandatoryCharges(3, invoice.Charges,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.LandingCharges).ChargeCodeChargeKey,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight).ChargeCodeChargeKey,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance).ChargeCodeChargeKey);

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			AssertMandatoryCharges(2, invoice.Charges,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight).ChargeCodeChargeKey,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance).ChargeCodeChargeKey);

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostFreightWithAmpersand;
			AssertMandatoryCharges(1, invoice.Charges,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight).ChargeCodeChargeKey);

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndInsurance;
			AssertMandatoryCharges(1, invoice.Charges,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance).ChargeCodeChargeKey);

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			AssertMandatoryCharges(2, invoice.GroupHeader.Charges,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight).ChargeCodeChargeKey,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance).ChargeCodeChargeKey);

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.PackedAtFactory;
			AssertMandatoryCharges(3, invoice.GroupHeader.Charges,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.ForeignInlandFreight).ChargeCodeChargeKey,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight).ChargeCodeChargeKey,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance).ChargeCodeChargeKey);

			invoice.GroupHeader.Charges.RemoveAndDeleteAll();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedCostInsuranceAndFreight;
			AssertMandatoryCharges(2, invoice.Charges,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight).ChargeCodeChargeKey,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance).ChargeCodeChargeKey);
			AssertMandatoryCharges(1, invoice.GroupHeader.Charges,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.PackingCost).ChargeCodeChargeKey);

			invoice.GroupHeader.Charges.RemoveAndDeleteAll();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedCostAndFreight;

			AssertMandatoryCharges(1, invoice.Charges,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight).ChargeCodeChargeKey);

			AssertMandatoryCharges(2, invoice.GroupHeader.Charges,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.PackingCost).ChargeCodeChargeKey,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance).ChargeCodeChargeKey);

			invoice.GroupHeader.Charges.RemoveAndDeleteAll();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedFreeOnBoard;
			AssertMandatoryCharges(3, invoice.GroupHeader.Charges,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.PackingCost).ChargeCodeChargeKey,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight).ChargeCodeChargeKey,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance).ChargeCodeChargeKey);

			invoice.GroupHeader.Charges.RemoveAndDeleteAll();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedAtFactory;
			declaration.ResumeApportionment();
			AssertMandatoryCharges(4, invoice.GroupHeader.Charges,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.PackingCost).ChargeCodeChargeKey,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight).ChargeCodeChargeKey,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance).ChargeCodeChargeKey,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.ForeignInlandFreight).ChargeCodeChargeKey);
		}

		public void TestFillMandatoryWhenGroupChargesExist()
		{
			groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100, JobDeclaration.LocalCurrencyConstantCode);
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			declaration.ResumeApportionment();
			AssertMandatoryCharges(1, invoice.GroupHeader.Charges, invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance).ChargeCodeChargeKey);
		}

		public void TestDontRemoveMandatoryChargeInGroupWhenAnotherInvoicesEnters()
		{
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedAtFactory;
			AssertMandatoryCharges(4, invoice.GroupHeader.Charges,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.ForeignInlandFreight).ChargeCodeChargeKey,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.PackingCost).ChargeCodeChargeKey,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight).ChargeCodeChargeKey,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance).ChargeCodeChargeKey);

			JobComInvoiceHeader anotherInvoice = groupHeader.JobComInvoiceHeaders.AddNew();
			anotherInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			AssertEquals("Empty charges in 'invoice' stay", 4, invoice.GroupHeader.Charges.Count);
			AssertMandatoryCharges(2, anotherInvoice.Charges,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight).ChargeCodeChargeKey,
				invoice.IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance).ChargeCodeChargeKey);
		}

		public void TestJZ_RX_NKInvoice_CurrencyIsAUDForEXW()
		{
			invoice.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			AssertEquals("Curr", "AUD", invoice.JZ_RX_NKInvoice_Currency);
			AssertEquals("InfoReadonly", true, invoice.JZ_RX_NKInvoice_CurrencyInfo.ReadOnly);
		}

		public void TestDateOfValuationComesFromOverride()
		{
			invoice.JZ_ValuationDateOverride = new ZDateTime(2005, 8, 17);
			AssertEquals(new ZDateTime(2005, 8, 17), invoice.DateOfValuation);
		}

		#region TestCloneInvoice
		public void TestCloneQuarantineHeader()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			JobComInvoiceGroupHeader group = testDec.JobComInvoiceGroupHeaders[0];
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "~~~111~~~";
			QuarantineExDocHeader exdocHeader = invoice.QuarantineExDocHeader;
			exdocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			exdocHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.Automatic;
			exdocHeader.QH_AQISRegion = "CBR";
			exdocHeader.QH_StorageEstablishment = "88";
			Factory.Save();

			JobDeclaration clonedDec = (JobDeclaration)testDec.TemplateCopy();
			JobComInvoiceHeader clonedInvoice = clonedDec.Invoices[0];

			Assert("No HasChanges", !clonedInvoice.QuarantineExDocHeader.HasChanges);
			AssertEquals("Produce type", EXDOCCommodityCodes.Codes.Dairy, clonedInvoice.QuarantineExDocHeader.QH_ProduceType);
			AssertEquals("Certificate Print Indicator", EXDOCCertificatePrintCodes.Codes.Automatic, clonedInvoice.QuarantineExDocHeader.QH_CertificatePrintIndicator);
			AssertEquals("AQIS Region", exdocHeader.QH_AQISRegion, clonedInvoice.QuarantineExDocHeader.QH_AQISRegion);
			AssertEquals("Storage Establishment", ZString.Empty, clonedInvoice.QuarantineExDocHeader.QH_StorageEstablishment);
		}
		#endregion

		#region Implementation

		JobDeclaration declaration;
		JobComInvoiceGroupHeader groupHeader;
		JobComInvoiceHeader invoice;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			invoice = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice.JZ_InvoiceAmount = 1000;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
		}

		void AssertMandatoryCharges(int chargeCount, IJobComInvChargeCollection<JobComInvCharge> collection, params ChargeCodeChargeKey[] mandatoryCharges)
		{
			AssertEquals(invoice.JZ_IncoTerm + " should have " + chargeCount + " charges", chargeCount, collection.Count);
			foreach (ChargeCodeChargeKey charge in mandatoryCharges)
			{
				Assert(charge + " exists", collection.Find(charge) != null);
			}
		}

		#endregion
	}
}
