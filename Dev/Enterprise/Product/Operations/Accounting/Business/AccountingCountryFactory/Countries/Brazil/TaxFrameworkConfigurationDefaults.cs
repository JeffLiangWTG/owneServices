using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Countries.Brazil
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Tax authority name")]
	public class TaxFrameworkConfigurationDefaults : ITaxFrameworkConfigurationDefaults
	{
		List<TaxAuthoritiesConfiguration> ITaxFrameworkConfigurationDefaults.GetTaxAuthorities()
		{
			var result = new List<TaxAuthoritiesConfiguration>();
			result.Add(new TaxAuthoritiesConfiguration() { Country = "BR", Code = "IMBEL", TaxAuthorityType = "MUN", Name = "PREFEITURA DE BELO HORIZONTE 3106200" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "BR", Code = "IMBRI", TaxAuthorityType = "MUN", Name = "PREFEITURA DE BARUERI 3505708" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "BR", Code = "IMBTI", TaxAuthorityType = "MUN", Name = "PREFEITURA DO BETIM 3106705" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "BR", Code = "IMCBA", TaxAuthorityType = "MUN", Name = "PREFEITURA DE CUIABÁ 5103403" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "BR", Code = "IMCPQ", TaxAuthorityType = "MUN", Name = "PREFEITURA DE CAMPINAS 3509502" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "BR", Code = "IMCWB", TaxAuthorityType = "MUN", Name = "PREFEITURA DE CURITIBA 4106902" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "BR", Code = "IMFLN", TaxAuthorityType = "MUN", Name = "PREFEITURA DE FLORIANÓPOLIS 4205407" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "BR", Code = "IMFOR", TaxAuthorityType = "MUN", Name = "PREFEITURA DE FORTALEZA 2304400" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "BR", Code = "IMGRU", TaxAuthorityType = "MUN", Name = "PREFEITURA DE GUARULHOS 3518800" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "BR", Code = "IMHTL", TaxAuthorityType = "MUN", Name = "PREFEITURA DE HORTOLÂNDIA 3519071" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "BR", Code = "IMITJ", TaxAuthorityType = "MUN", Name = "PREFEITURA DE ITAJAÍ 4208203" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "BR", Code = "IMMAO", TaxAuthorityType = "MUN", Name = "PREFEITURA DE MANAUS 1302603" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "BR", Code = "IMPNG", TaxAuthorityType = "MUN", Name = "PREFEITURA DE PARANAGUÁ 4118204" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "BR", Code = "IMPOA", TaxAuthorityType = "MUN", Name = "PREFEITURA DE PORTO ALEGRE 4314902" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "BR", Code = "IMPQZ", TaxAuthorityType = "MUN", Name = "PREFEITURA DE POÁ 3539806" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "BR", Code = "IMREC", TaxAuthorityType = "MUN", Name = "PREFEITURA DO RECIFE 2611606" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "BR", Code = "IMRCL", TaxAuthorityType = "MUN", Name = "PREFEITURA DE RIO CLARO 3543907" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "BR", Code = "IMRIO", TaxAuthorityType = "MUN", Name = "PREFEITURA DO RIO DE JANEIRO 3304557" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "BR", Code = "IMSAO", TaxAuthorityType = "MUN", Name = "PREFEITURA DE SÃO PAULO 3550308" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "BR", Code = "IMSJE", TaxAuthorityType = "MUN", Name = "PREFEITURA DE SÃO JOSÉ 4216602" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "BR", Code = "IMSJK", TaxAuthorityType = "MUN", Name = "PREFEITURA DE SÃO JOSÉ DOS CAMPOS 3549904" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "BR", Code = "IMSSA", TaxAuthorityType = "MUN", Name = "PREFEITURA DE SALVADOR 2927408" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "BR", Code = "IMSSZ", TaxAuthorityType = "MUN", Name = "PREFEITURA DE SANTOS 3548500" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "BR", Code = "IMVIX", TaxAuthorityType = "MUN", Name = "PREFEITURA DE VITÓRIA 3205309" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "BR", Code = "RFB", TaxAuthorityType = "NAT", Name = "RECEITA FEDERAL DO BRASIL" });
			return result;
		}

		List<TaxSystemsConfiguration> ITaxFrameworkConfigurationDefaults.GetTaxSystems()
		{
			var result = new List<TaxSystemsConfiguration>();
			result.Add(new TaxSystemsConfiguration()
			{
				Country = "BR",
				Code = "ISS",
				TaxSuperType = "TRX",
				TaxAuthorityType = "MUN",
				RegistrationLevel = "BRN",
				IncludeInInvoceTotal = false,
				AdjustmentSign = "NEG",
				TaxBaseCalculationMethod = "ILA",
				TaxAmountCalculationMethod = "BTR",
				ThresholdRule = "NTR",
				TaxRateSource = "TGR",
				Name = "PROVISÃO DO ISS"
			});
			result.Add(new TaxSystemsConfiguration()
			{
				Country = "BR",
				Code = "PISPROV",
				TaxSuperType = "TRX",
				TaxAuthorityType = "NAT",
				RegistrationLevel = "CMP",
				IncludeInInvoceTotal = false,
				AdjustmentSign = "NEG",
				TaxBaseCalculationMethod = "ILA",
				TaxAmountCalculationMethod = "BTR",
				ThresholdRule = "NTR",
				TaxRateSource = "TGR",
				Name = "PROVISÃO DO PIS"
			});
			result.Add(new TaxSystemsConfiguration()
			{
				Country = "BR",
				Code = "COFPROV",
				TaxSuperType = "TRX",
				TaxAuthorityType = "NAT",
				RegistrationLevel = "CMP",
				IncludeInInvoceTotal = false,
				AdjustmentSign = "NEG",
				TaxBaseCalculationMethod = "ILA",
				TaxAmountCalculationMethod = "BTR",
				ThresholdRule = "NTR",
				TaxRateSource = "TGR",
				Name = "PROVISÃO DO COFINS"
			});
			result.Add(new TaxSystemsConfiguration()
			{
				Country = "BR",
				Code = "COFINS",
				TaxSuperType = "RII",
				TaxAuthorityType = "NAT",
				RegistrationLevel = "CMP",
				IncludeInInvoceTotal = true,
				AdjustmentSign = "NEG",
				TaxBaseCalculationMethod = "ILA",
				TaxAmountCalculationMethod = "BTR",
				ThresholdRule = "NTR",
				TaxRateSource = "TGR",
				Name = "RETENÇÃO DO COFINS"
			});
			result.Add(new TaxSystemsConfiguration()
			{
				Country = "BR",
				Code = "CSLL",
				TaxSuperType = "RII",
				TaxAuthorityType = "NAT",
				RegistrationLevel = "CMP",
				IncludeInInvoceTotal = true,
				AdjustmentSign = "NEG",
				TaxBaseCalculationMethod = "ILA",
				TaxAmountCalculationMethod = "BTR",
				ThresholdRule = "NTR",
				TaxRateSource = "TGR",
				Name = "RETENÇÃO DO CSLL"
			});
			result.Add(new TaxSystemsConfiguration()
			{
				Country = "BR",
				Code = "INSS",
				TaxSuperType = "RII",
				TaxAuthorityType = "NAT",
				RegistrationLevel = "CMP",
				IncludeInInvoceTotal = true,
				AdjustmentSign = "NEG",
				TaxBaseCalculationMethod = "ILA",
				TaxAmountCalculationMethod = "BTR",
				ThresholdRule = "NTR",
				TaxRateSource = "TGR",
				Name = "RETENÇÃO DO INSS"
			});
			result.Add(new TaxSystemsConfiguration()
			{
				Country = "BR",
				Code = "IRRF",
				TaxSuperType = "RII",
				TaxAuthorityType = "NAT",
				RegistrationLevel = "CMP",
				IncludeInInvoceTotal = true,
				AdjustmentSign = "NEG",
				TaxBaseCalculationMethod = "ILA",
				TaxAmountCalculationMethod = "BTR",
				ThresholdRule = "NTR",
				TaxRateSource = "TGR",
				Name = "RETENÇÃO DO IRRF"
			});
			result.Add(new TaxSystemsConfiguration()
			{
				Country = "BR",
				Code = "PIS",
				TaxSuperType = "RII",
				TaxAuthorityType = "NAT",
				RegistrationLevel = "CMP",
				IncludeInInvoceTotal = true,
				AdjustmentSign = "NEG",
				TaxBaseCalculationMethod = "ILA",
				TaxAmountCalculationMethod = "BTR",
				ThresholdRule = "NTR",
				TaxRateSource = "TGR",
				Name = "RETENÇÃO DO PIS"
			});
			result.Add(new TaxSystemsConfiguration()
			{
				Country = "BR",
				Code = "ISSRETIDO",
				TaxSuperType = "RII",
				TaxAuthorityType = "MUN",
				RegistrationLevel = "BRN",
				IncludeInInvoceTotal = true,
				AdjustmentSign = "NEG",
				TaxBaseCalculationMethod = "ILA",
				TaxAmountCalculationMethod = "BTR",
				ThresholdRule = "NTR",
				TaxRateSource = "TGR",
				Name = "ISS RETIDO"
			});
			return result;
		}
	}
}
