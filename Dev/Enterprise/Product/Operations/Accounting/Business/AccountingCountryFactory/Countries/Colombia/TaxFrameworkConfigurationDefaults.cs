using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Countries.Colombia
{
	public class TaxFrameworkConfigurationDefaults : ITaxFrameworkConfigurationDefaults
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Tax authority names")]
		List<TaxAuthoritiesConfiguration> ITaxFrameworkConfigurationDefaults.GetTaxAuthorities()
		{
			var result = new List<TaxAuthoritiesConfiguration>();

			result.Add(new TaxAuthoritiesConfiguration() { Country = "CO", Code = "05001", TaxAuthorityType = "MUN", Name = "ALCALDIA DE MEDELLIN - ANTIOQUIA" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "CO", Code = "05615", TaxAuthorityType = "MUN", Name = "ALCALDIA DE RIONEGRO - ANTIOQUIA" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "CO", Code = "08001", TaxAuthorityType = "MUN", Name = "ALCALDIA DISTRITAL DE BARRANQUILLA - ATLANTICO" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "CO", Code = "11001", TaxAuthorityType = "MUN", Name = "DIRECCION DISTRATAL DE IMPUESTOS DE BOGOTA - BOGOTA D.C." });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "CO", Code = "13001", TaxAuthorityType = "MUN", Name = "ALCALDIA DISTRITAL DE CARTAGENA DE INDIAS - BOLIVAR" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "CO", Code = "25214", TaxAuthorityType = "MUN", Name = "ALCALDIA MUNICIPAL DE COTA - CUNDINAMARCA" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "CO", Code = "25286", TaxAuthorityType = "MUN", Name = "ALCALDIA MUNICIPAL DE FUNZA CUNDINAMARCA - CUNDINAMARCA" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "CO", Code = "47001", TaxAuthorityType = "MUN", Name = "ALCALDIA DISTRITAL DE SANTA MARTA - MAGDALENA" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "CO", Code = "66001", TaxAuthorityType = "MUN", Name = "ALCALDIA DE PEREIRA - RISARALDA" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "CO", Code = "76001", TaxAuthorityType = "MUN", Name = "ALCALDIA DE SANTIAGO DE CALI - VALLE DEL CAUCA" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "CO", Code = "76109", TaxAuthorityType = "MUN", Name = "ALCALDIA DE BUENAVENTURA - VALLE DEL CAUCA" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "CO", Code = "76520", TaxAuthorityType = "MUN", Name = "ALCALDIA DE PALMIRA - VALLE DEL CAUCA" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "CO", Code = "DIAN", TaxAuthorityType = "NAT", Name = "DIRECCION DE IMPUESTOS Y ADUANAS NACIONALES" });

			return result;
		}

		List<TaxSystemsConfiguration> ITaxFrameworkConfigurationDefaults.GetTaxSystems()
		{
			return new List<TaxSystemsConfiguration>
			{
				new()
				{
					Country = "CO",
					Code = "RETEFUENTE",
					TaxSuperType = "RII",
					TaxAuthorityType = "NAT",
					RegistrationLevel = "CMP",
					IncludeInInvoceTotal = true,
					AdjustmentSign = "NEG",
					TaxBaseCalculationMethod = "ILA",
					TaxAmountCalculationMethod = "BTR",
					ThresholdRule = "NTR",
					TaxRateSource = "TGR",
					Name = (NoResString)"RETENCION EN LA FUENTE SOBRE LA RENTA"
				},
				new()
				{
					Country = "CO",
					Code = "AUTOFUENTE",
					TaxSuperType = "TRX",
					TaxAuthorityType = "NAT",
					RegistrationLevel = "CMP",
					IncludeInInvoceTotal = false,
					AdjustmentSign = "NEG",
					TaxBaseCalculationMethod = "ILA",
					TaxAmountCalculationMethod = "BTR",
					ThresholdRule = "NTR",
					TaxRateSource = "TGR",
					Name = (NoResString)"AUTORRETENCION EN LA FUENTE SOBRE LA RENTA"
				},
				new ()
				{
					Country = "CO",
					Code = "RETEICA",
					TaxSuperType = "RII",
					TaxAuthorityType = "MUN",
					RegistrationLevel = "CMP",
					IncludeInInvoceTotal = true,
					AdjustmentSign = "NEG",
					TaxBaseCalculationMethod = "ILA",
					TaxAmountCalculationMethod = "BTR",
					ThresholdRule = "NTR",
					TaxRateSource = "TGR",
					Name = (NoResString)"RETENCION DE ICA"
				},
				new ()
				{
					Country = "CO",
					Code = "AUTOICA",
					TaxSuperType = "TRX",
					TaxAuthorityType = "MUN",
					RegistrationLevel = "CMP",
					IncludeInInvoceTotal = false,
					AdjustmentSign = "NEG",
					TaxBaseCalculationMethod = "ILA",
					TaxAmountCalculationMethod = "BTR",
					ThresholdRule = "NTR",
					TaxRateSource = "TGR",
					Name = (NoResString)"AUTORRETENCION ICA"
				}
			};
		}
	}
}
