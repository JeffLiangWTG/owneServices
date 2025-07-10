using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CusFiscalReferenceProviderTest : TestCaseWithFactory
{
	public void TestGetByDataGroupingCode()
	{
		var provider = EU.Business.Declaration.CusFiscalReferenceProvider.GetByDataGroupingCode(Core.Constants.CountryCodes.Italy);
		CombineAssertions(() =>
		{
			AssertType<CusFiscalReferenceProvider>("CusFiscalReferenceProvider Type", provider);
			AssertEquals("CusFiscalReferenceProvider DataGroupingCode", Core.Constants.CountryCodes.Italy, provider.DataGroupingCode);
		});
	}

	public void TestGetNewLookups()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var fiscalReference = entryInstruction.FiscalReferences.AddNew();
		AssertType<CusFiscalReferenceLookups>("Lookups Type", fiscalReference.Provider.GetNewLookups(fiscalReference));
	}

	public void TestGetReferenceMaxLength()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var fiscalReference = entryInstruction.FiscalReferences.AddNew();
		AssertEquals("CFR_Reference MaxLength", 17, fiscalReference.CFR_ReferenceInfo.MaxLength);
	}

	public void TestRecalculateReferenceIfNeeded_InvoiceLine()
	{
		var organisation = Factory.New<OrgHeader>();
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var fiscalReference = invoiceLine.FiscalReferences.AddNew();

		fiscalReference.CFR_OA_Owner = ZGuid.Empty;
		fiscalReference.Provider.RecalculateReferenceIfNeeded(fiscalReference);
		AssertEquals("When Owner is not set, CFR_Reference", "", fiscalReference.CFR_Reference);

		fiscalReference.CFR_OA_Owner = organisation.MainAddress.PK;
		fiscalReference.Provider.RecalculateReferenceIfNeeded(fiscalReference);
		AssertEquals("When Owner is set but has no VAT OrgCusCode, CFR_Reference", "", fiscalReference.CFR_Reference);

		organisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.IVA, "IVACODE", Core.Constants.CountryCodes.Italy);
		fiscalReference.Provider.RecalculateReferenceIfNeeded(fiscalReference);
		AssertEquals("When Owner is set and has VAT OrgCusCode, CFR_Reference", "IVACODE", fiscalReference.CFR_Reference);

		organisation.CustomsCodes.RemoveAndDeleteAll();
		fiscalReference.Provider.RecalculateReferenceIfNeeded(fiscalReference);
		AssertEquals("When Owner is set and has no OrgCusCode and CFR_Reference was already populated, CFR_Reference", "", fiscalReference.CFR_Reference);

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		organisation.CustomsCodes.RemoveAll();
		organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345678", Core.Constants.CountryCodes.Italy);
		fiscalReference.Provider.RecalculateReferenceIfNeeded(fiscalReference);
		AssertEquals("Import Declaration, when owner is set and has EOR OrgCusCode, CFR_Reference", "IT12345678", fiscalReference.CFR_Reference);
	}

	public void TestRecalculateReferenceIfNeeded_EntryInstruction()
	{
		var organisation = Factory.New<OrgHeader>();
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var fiscalReference = entryInstruction.FiscalReferences.AddNew();

		fiscalReference.CFR_OA_Owner = ZGuid.Empty;
		fiscalReference.Provider.RecalculateReferenceIfNeeded(fiscalReference);
		AssertEquals("When Owner is not set, CFR_Reference", "", fiscalReference.CFR_Reference);

		fiscalReference.CFR_OA_Owner = organisation.MainAddress.PK;
		fiscalReference.Provider.RecalculateReferenceIfNeeded(fiscalReference);
		AssertEquals("When Owner is set but has no EOR OrgCusCode, CFR_Reference", "", fiscalReference.CFR_Reference);

		organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORCODE", Core.Constants.CountryCodes.Italy);
		fiscalReference.Provider.RecalculateReferenceIfNeeded(fiscalReference);
		AssertEquals("When Owner is set and has EOR OrgCusCode, CFR_Reference", "EORCODE", fiscalReference.CFR_Reference);

		organisation.CustomsCodes.RemoveAndDeleteAll();
		fiscalReference.Provider.RecalculateReferenceIfNeeded(fiscalReference);
		AssertEquals("When Owner is set and has no OrgCusCode and CFR_Reference was already populated, CFR_Reference", "", fiscalReference.CFR_Reference);

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		organisation.CustomsCodes.RemoveAll();
		organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345678", Core.Constants.CountryCodes.Italy);
		fiscalReference.Provider.RecalculateReferenceIfNeeded(fiscalReference);
		AssertEquals("Import Declaration, when owner is set and has EOR OrgCusCode, CFR_Reference", "IT12345678", fiscalReference.CFR_Reference);
	}

	public void TestGetNewValidation()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var fiscalReference = entryInstruction.FiscalReferences.AddNew();
		AssertType<CusFiscalReferenceValidation>("IT Validation", fiscalReference.Provider.GetNewValidation(fiscalReference));
	}
}
