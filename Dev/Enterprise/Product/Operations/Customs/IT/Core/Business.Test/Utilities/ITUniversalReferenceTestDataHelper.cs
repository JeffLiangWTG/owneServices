using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

public class ITUniversalReferenceTestDataHelper : UniversalReferenceTestDataHelper
{
	public ITUniversalReferenceTestDataHelper(BusinessObjectFactory factory) : base(factory)
	{
	}

	public void CreateRefCusProcedure40And71ForCurrentCountry()
	{
		CreateRefCusProcedure71ForCurrentCountry();
		CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, "A", "40", "00", "", "Two", "IMP", group: "IFD", intoWarehouse: false);
	}

	public void CreateRefCusProcedure71ForCurrentCountry()
	{
		CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, "A", "71", "00", "", "One", "IMP", group: "IFD", intoWarehouse: true);
	}

	public void CreateRefCusProcedure53ForCurrentCountry()
	{
		var refCusProcedure = CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, "A", "53", "00", "", "One", "IMP", group: "IFD");
		refCusProcedure.ZZ6_IntoTemporaryImport = "Y";
	}

	public void CreateRefCusProcedure23ForCurrentCountry()
	{
		var refCusProcedure = CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, "A", "23", "00", "", "One", "IMP", group: "IFD");
		refCusProcedure.ZZ6_IntoTemporaryExport = "Y";
	}

	public void CreateRefCusProcedure51ForCurrentCountry()
	{
		var refCusProcedure = CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, "A", "51", "00", "", "One", "IMP", group: "IFD");
		refCusProcedure.ZZ6_IntoInwardProcessing = "Y";
	}

	public void CreateRefCusProcedure21And22ForCurrentCountry()
	{
		var refCusProcedure21 = CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, "A", "21", "00", "", "One", "EXP", group: "IFD");
		refCusProcedure21.ZZ6_IntoOutwardProcessing = "Y";
		var refCusProcedure22 = CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, "A", "22", "00", "", "One", "EXP", group: "IFD");
		refCusProcedure22.ZZ6_IntoOutwardProcessing = "Y";
	}

	public void SetupPortTaxRates()
	{
		CreateNewOrGetExistingCusCodeType("HCOMM", "Commodity Code");
		CreateCusCodeList("IT", "HCOMM", "A2", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
		CreateCusCodeList("IT", "HCOMM", "A1", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
		CreateCusCodeList("IT", "HCOMM", "A3", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
	}

	public void SetupHarbourRates()
	{
		CreateHarbourRate("TAX", "ITVCE", "ALL", "A3", ZDate.Today.AddYears(-1), ZDate.Today.AddYears(1), "0.5 * [TNE]", "IT", portTaxType: "9AA");
		CreateHarbourRate("TAX", "USLAX", "ALL", "A4", ZDate.Today.AddYears(-1), ZDate.Today.AddYears(1), "0.1 * [TNE]", "IT", portTaxType: "9AB");
	}

	public void CreateTaxOrFeeWithRelatedVatApplicability(ZString tariffTypeCode, ZString tarffCode, params (ZString TaxCode, ZDecimal TaxRate, ZString AdditionalCode)[] taxesWithVatApplicability)
	{
		CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy).ZZZ_ZZZ_Grouping = CreateNewOrGetExistingDataGrouping("EUN").PK;
		var tariffTypePK = CreateNewOrGetExistingTariffType("EUN", tariffTypeCode).PK;
		factory.Save();
		var tariff = CreateTariff(Core.Constants.CountryCodes.Italy, tariffTypePK, tarffCode, ZDateTime.BrettsBirthday, ZDateTime.Today, "Alpha Bravo");

		foreach (var (taxCode, taxRate, additionalCode) in taxesWithVatApplicability)
		{
			CreateTaxOrFee(taxCode, taxRate, Core.Constants.CountryCodes.Italy);
			CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.Italy, taxCode, startDate: ZDateTime.Today.AddDays(-2), endDate: ZDateTime.Today.AddDays(2), additionalCode: additionalCode);
		}

		factory.Save();
	}

	public RefCusProcedure CreateNewRefCusProcedure(ZString procedureCode, string shipmentType = null, Action<RefCusProcedure> configurationAction = null)
	{
		var procedure = factory.New<RefCusProcedure>();
		procedure.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.Country.Code;
		procedure.ZZ6_ShipmentType = shipmentType ?? ZString.Empty;
		procedure.ZZ6_ProcedureCode = procedureCode.SubstringSafe(0, 2);
		procedure.ZZ6_PreviousProcedureCode = procedureCode.SubstringSafe(2, 2);
		procedure.ZZ6_Concession = procedureCode.PadRight(7).Right(3);
		configurationAction?.Invoke(procedure);
		return procedure;
	}

	public void SetUpRefDataWithVATRequirement()
	{
		var procedureNotRequiringVatImp = CreateRefCusProcedure("IT", "IM", "42", "00", "", "42 desc", "IMP");
		procedureNotRequiringVatImp.ZZ6_CalculateVAT = false;

		var procedureNotRequiringVatImp2 = CreateRefCusProcedure("IT", "IM", "63", "00", "", "63 desc", "IMP");
		procedureNotRequiringVatImp2.ZZ6_CalculateVAT = false;

		var procedureRequiringVatImp = CreateRefCusProcedure("IT", "IM", "5", "00", "", "5 desc", "IMP");
		procedureRequiringVatImp.ZZ6_CalculateVAT = true;

		var procedureNotRequiringVatExp = CreateRefCusProcedure("IT", "EX", "2", "00", "", "2 desc", "EXP");
		procedureNotRequiringVatExp.ZZ6_CalculateVAT = true;
	}
}
