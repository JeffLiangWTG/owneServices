using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class CusEntryHeaderValidationTest : Customs.Business.Testing.CusEntryHeaderValidationTest
	{
		public void TestFreightAndInsurance()
		{
			var invoice = declaration.Invoices[0];
			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(0m, entry.Insurance);
			AssertEquals(0m, entry.Freight);
			CombineAssertions("When Freight is zero, No error incoterms : EXW, FAS, FCA, FOB, CIN / When Insurance is zero, No error incoterms : EXW, FAS, FCA, FOB, CFR, CPT", () =>
			{
				AssertFreightAndInsuranceNoMessageError(entry, invoice, IncotermList.Codes.ExWorks);
				AssertFreightAndInsuranceNoMessageError(entry, invoice, IncotermList.Codes.FreeAlongsideShip);
				AssertFreightAndInsuranceNoMessageError(entry, invoice, IncotermList.Codes.FreeCarrier);
				AssertFreightAndInsuranceNoMessageError(entry, invoice, IncotermList.Codes.FreeOnBoard);
				invoice.JZ_IncoTerm = IncotermList.Codes.CostAndInsurance;
				entry.Validation.ValidateAll();
				AssertNoMessageErrors(entry.FreightInfo);

				invoice.JZ_IncoTerm = IncotermList.Codes.CostAndFreight;
				entry.Validation.ValidateAll();
				AssertNoMessageErrors(entry.InsuranceInfo);

				invoice.JZ_IncoTerm = IncotermList.Codes.CarriagePaidTo;
				entry.Validation.ValidateAll();
				AssertNoMessageErrors(entry.InsuranceInfo);
			});

			CombineAssertions("When Freight is zero, Has error incoterms : CIF, DES, DEQ, DDU, DDP, CIP, DAT, DPU, CFR, CPT / When Insurance is zero, Has error incoterms : CIF, DES, DEQ, DDU, DDP, CIP, DAT, DPU, CIN", () =>
			{
				AssertFreightAndInsuranceHasMessageError(entry, invoice, IncotermList.Codes.CostInsuranceAndFreight);
				AssertFreightAndInsuranceHasMessageError(entry, invoice, IncotermList.Codes.DeliveredExShip);
				AssertFreightAndInsuranceHasMessageError(entry, invoice, IncotermList.Codes.DeliveredExQuay);
				AssertFreightAndInsuranceHasMessageError(entry, invoice, IncotermList.Codes.DeliveredDutyUnpaid);
				AssertFreightAndInsuranceHasMessageError(entry, invoice, IncotermList.Codes.DeliveredDutyPaid);
				AssertFreightAndInsuranceHasMessageError(entry, invoice, IncotermList.Codes.CarriageAndInsurancePaidTo);
				AssertFreightAndInsuranceHasMessageError(entry, invoice, IncotermList.Codes.DeliveredAtTerminal);
				AssertFreightAndInsuranceHasMessageError(entry, invoice, IncotermList.Codes.DeliveredAtPlaceUnloaded);
				invoice.JZ_IncoTerm = IncotermList.Codes.CostAndFreight;
				entry.Validation.ValidateAll();
				AssertHasMessageErrorContaining(entry.FreightInfo, "Freight (KRW) cannot be zero.");

				invoice.JZ_IncoTerm = IncotermList.Codes.CarriagePaidTo;
				entry.Validation.ValidateAll();
				AssertHasMessageErrorContaining(entry.FreightInfo, "Freight (KRW) cannot be zero.");

				invoice.JZ_IncoTerm = IncotermList.Codes.CostAndInsurance;
				entry.Validation.ValidateAll();
				AssertHasMessageErrorContaining(entry.InsuranceInfo, "Insurance (KRW) cannot be zero.");
			});

			invoice.InvoiceLines[0].Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 1m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			invoice.InvoiceLines[0].Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 1m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			CombineAssertions("When Freight is over zero, No error incoterms : EXW, FAS, FCA, CIF, DES, DEQ, DDU, CIP, DAT, DPU, CFR, CPT / When Insurance is over zero, No error incoterms : EXW, FAS, FCA, CIF, DES, DEQ, DDU, CIP, DAT, DPU, CIN", () =>
			{
				AssertFreightAndInsuranceNoMessageError(entry, invoice, IncotermList.Codes.ExWorks);
				AssertFreightAndInsuranceNoMessageError(entry, invoice, IncotermList.Codes.FreeAlongsideShip);
				AssertFreightAndInsuranceNoMessageError(entry, invoice, IncotermList.Codes.FreeCarrier);
				AssertFreightAndInsuranceNoMessageError(entry, invoice, IncotermList.Codes.CostInsuranceAndFreight);
				AssertFreightAndInsuranceNoMessageError(entry, invoice, IncotermList.Codes.DeliveredExShip);
				AssertFreightAndInsuranceNoMessageError(entry, invoice, IncotermList.Codes.DeliveredExQuay);
				AssertFreightAndInsuranceNoMessageError(entry, invoice, IncotermList.Codes.DeliveredDutyUnpaid);
				AssertFreightAndInsuranceNoMessageError(entry, invoice, IncotermList.Codes.DeliveredDutyPaid);
				AssertFreightAndInsuranceNoMessageError(entry, invoice, IncotermList.Codes.CarriageAndInsurancePaidTo);
				AssertFreightAndInsuranceNoMessageError(entry, invoice, IncotermList.Codes.DeliveredAtTerminal);
				AssertFreightAndInsuranceNoMessageError(entry, invoice, IncotermList.Codes.DeliveredAtPlaceUnloaded);
				invoice.JZ_IncoTerm = IncotermList.Codes.CostAndFreight;
				entry.Validation.ValidateAll();
				AssertNoMessageErrors(entry.FreightInfo);

				invoice.JZ_IncoTerm = IncotermList.Codes.CarriagePaidTo;
				entry.Validation.ValidateAll();
				AssertNoMessageErrors(entry.FreightInfo);

				invoice.JZ_IncoTerm = IncotermList.Codes.CostAndInsurance;
				entry.Validation.ValidateAll();
				AssertNoMessageErrors(entry.InsuranceInfo);
			});

			CombineAssertions("When Freight is over zero, Has error incoterms : FOB / When Insurance is over zero, Has error incoterms : FOB", () =>
			{
				invoice.JZ_IncoTerm = IncotermList.Codes.FreeOnBoard;
				entry.Validation.ValidateAll();
				AssertHasMessageErrorContaining(entry.FreightInfo, "If Incoterm is ‘FOB’ then, Freight must be zero.");
				AssertHasMessageErrorContaining(entry.InsuranceInfo, "If Incoterm is ‘FOB’ then, Insurance must be zero.");
			});

			invoice.InvoiceLines[0].Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, -2m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			invoice.InvoiceLines[0].Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, -2m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			AssertEquals(-1m, entry.Insurance);
			AssertEquals(-1m, entry.Freight);
			entry.Validation.ValidateAll();
			AssertHasMessageErrorContaining("In any case, less than 0 is an error.", entry.FreightInfo, MandatoryValidation.ValueCannotBeNegative);
			AssertHasMessageErrorContaining("In any case, less than 0 is an error.", entry.InsuranceInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		void AssertFreightAndInsuranceNoMessageError(CusEntryHeader entry, JobComInvoiceHeader invoice, string incoterm)
		{
			var validation = entry.Validation;
			invoice.JZ_IncoTerm = incoterm;
			entry.Validation.ValidateAll();
			AssertNoMessageErrors(entry.FreightInfo);
			AssertNoMessageErrors(entry.InsuranceInfo);
		}
		void AssertFreightAndInsuranceHasMessageError(CusEntryHeader entry, JobComInvoiceHeader invoice, string incoterm)
		{
			var validation = entry.Validation;
			invoice.JZ_IncoTerm = incoterm;
			entry.Validation.ValidateAll();
			AssertHasMessageErrorContaining(entry.FreightInfo, "Freight (KRW) cannot be zero.");
			AssertHasMessageErrorContaining(entry.InsuranceInfo, "Insurance (KRW) cannot be zero.");
		}

		public void TestTotalPackages()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TotalNoOfPacksPackType = "BA";

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_NoOfPacks = 10m;
			declaration.InvoiceLines.AddNew();

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CustomsEntryHeaders[0];
			entry.Validation.ValidateAll();
			AssertNoMessageErrors(entry.TotalPackagesInfo);

			declaration.Invoices[0].JZ_NoOfPacks = 0m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			entry.Validation.ValidateAll();
			AssertHasMessageErrorContaining(entry.TotalPackagesInfo, "If 'Pack Type' is not 'bulk', then 'Total Packages' needs to be bigger than 0.");

			declaration.JE_TotalNoOfPacksPackType = "VG";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			entry.Validation.ValidateAll();
			AssertNoMessageErrors(entry.TotalPackagesInfo);
		}

		public void TestCustomsValue()
		{
			var invoice = declaration.Invoices[0];
			var entry = declaration.CustomsEntryHeaders[0];
			CusEntryLine entryLine = entry.AllEntryLines[0];
			entryLine.CL_CustomsValue = -1;
			entry.Validation.ValidateAll();
			AssertHasMessageErrorContaining(entry.CustomsValueInfo, MandatoryValidation.ValueCannotBeNegative);

			entryLine.CL_CustomsValue = 0;
			entry.ResetIsCustomsValueCalculated();
			entry.Validation.ValidateAll();
			AssertNoMessageErrors(entry.CustomsValueInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}
		JobDeclaration declaration;
	}
}
