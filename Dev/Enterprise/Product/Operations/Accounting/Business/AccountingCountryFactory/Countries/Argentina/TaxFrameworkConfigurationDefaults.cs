using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Countries.Argentina
{
	public class TaxFrameworkConfigurationDefaults : ITaxFrameworkConfigurationDefaults
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Tax authority names")]
		List<TaxAuthoritiesConfiguration> ITaxFrameworkConfigurationDefaults.GetTaxAuthorities()
		{
			var result = new List<TaxAuthoritiesConfiguration>();
			result.Add(new TaxAuthoritiesConfiguration() { Country = "AR", Code = "AFIP", TaxAuthorityType = "NAT", Name = "ADMINISTRACION FEDERAL DE INGRESOS PUBLICOS" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "AR", Code = "S01AR", TaxAuthorityType = "STA", Name = "AGIP - CIUDAD AUTONOMA DE BUENOS AIRES" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "AR", Code = "S02AR", TaxAuthorityType = "STA", Name = "ARBA - PROVINCIA DE BUENOS AIRES" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "AR", Code = "S03AR", TaxAuthorityType = "STA", Name = "DIRECCION GENERAL DE RENTAS CATAMARCA" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "AR", Code = "S04AR", TaxAuthorityType = "STA", Name = "ADMINISTRACION TRIBUTARIA PROVINCIAL CHACO" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "AR", Code = "S05AR", TaxAuthorityType = "STA", Name = "DIRECCION GENERAL DE RENTAS CHUBUT" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "AR", Code = "S06AR", TaxAuthorityType = "STA", Name = "RENTAS CORDOBA" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "AR", Code = "S07AR", TaxAuthorityType = "STA", Name = "DIRECCION GENERAL DE RENTAS CORRIENTES" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "AR", Code = "S08AR", TaxAuthorityType = "STA", Name = "ADMINISTRADORA TRIBUTARIA DE ENTRE RIOS" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "AR", Code = "S09AR", TaxAuthorityType = "STA", Name = "DIRECCION GENERAL DE RENTAS FORMOSA" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "AR", Code = "S10AR", TaxAuthorityType = "STA", Name = "DIRECCION PROVINCIAL DE RENTAS JUJUY" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "AR", Code = "S11AR", TaxAuthorityType = "STA", Name = "DIRECCION GENERAL DE RENTAS LA PAMPA" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "AR", Code = "S12AR", TaxAuthorityType = "STA", Name = "DIRECCION GENERAL DE INGRESOS PROVINCIALES LA RIOJA" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "AR", Code = "S13AR", TaxAuthorityType = "STA", Name = "ADMINISTRACION TRIBUTARIA MENDOZA" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "AR", Code = "S14AR", TaxAuthorityType = "STA", Name = "AGENCIA TRIBUTARIA MISIONES" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "AR", Code = "S15AR", TaxAuthorityType = "STA", Name = "DIRECCION PROVINCIAL DE RENTAS NEUQUEN" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "AR", Code = "S16AR", TaxAuthorityType = "STA", Name = "AGENCIA RECAUDACION TRIBUTARIA PROVINCIA DE RIO NEGRO" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "AR", Code = "S17AR", TaxAuthorityType = "STA", Name = "DIRECCION GENERAL DE RENTAS DE LA PROVINCIA DE SALTA" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "AR", Code = "S18AR", TaxAuthorityType = "STA", Name = "DIRECCION GENERAL DE RENTAS SAN JUAN" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "AR", Code = "S19AR", TaxAuthorityType = "STA", Name = "DIRECCION PROVINCIAL DE INGRESOS PUBLICOS SAN LUIS" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "AR", Code = "S20AR", TaxAuthorityType = "STA", Name = "AGENCIA SANTACRUCEÑA DE INGRESOS PUBLICOS - SANTA CRUZ" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "AR", Code = "S21AR", TaxAuthorityType = "STA", Name = "ADMINISTRACION PROVINCIAL DE IMPUESTOS SANTA FE" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "AR", Code = "S22AR", TaxAuthorityType = "STA", Name = "DIRECCION GENERAL DE RENTAS SANTIAGO DEL ESTERO" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "AR", Code = "S23AR", TaxAuthorityType = "STA", Name = "AGENCIA DE RECAUDACION FUEGUINA - TIERRA DEL FUEGO" });
			result.Add(new TaxAuthoritiesConfiguration() { Country = "AR", Code = "S24AR", TaxAuthorityType = "STA", Name = "DIRECCION GENERAL DE RENTAS TUCUMAN" });
			return result;
		}

		List<TaxSystemsConfiguration> ITaxFrameworkConfigurationDefaults.GetTaxSystems()
		{
			var result = new List<TaxSystemsConfiguration>();
			result.Add(new TaxSystemsConfiguration()
			{
				Country = "AR",
				Code = "PERIB",
				TaxSuperType = "PER",
				TaxAuthorityType = "STA",
				RegistrationLevel = "CMP",
				IncludeInInvoceTotal = true,
				AdjustmentSign = "POS",
				TaxBaseCalculationMethod = "ILA",
				TaxAmountCalculationMethod = "BTR",
				ThresholdRule = "NTR",
				TaxRateSource = "OFT",
				Name = (NoResString)"PERCEPCION DE INGRESOS BRUTOS"
			});
			return result;
		}
	}
}
