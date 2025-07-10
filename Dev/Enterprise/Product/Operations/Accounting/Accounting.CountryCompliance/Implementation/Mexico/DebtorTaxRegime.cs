using CargoWise.Types;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Accounting.CountryCompliance.Implementation.Mexico.Constants;

namespace Enterprise.Accounting.CountryCompliance.Implementation.Mexico
{
	class DebtorTaxRegime : IDebtorTaxRegime
	{
		ZString IDebtorTaxRegime.GetOrgCusCode() => OrgCusCodes.REG;

		CodeDescriptionPairList IDebtorTaxRegime.GetTaxRegimeIdTypes()
		{
			var mexico_taxRegimeIdTypes = new CodeDescriptionPairList();

			#region SuppressResourceStringsCheckRegion

			mexico_taxRegimeIdTypes.AddPair("601", "General de Ley Personas Morales");
			mexico_taxRegimeIdTypes.AddPair("603", "Personas Morales con Fines no Lucrativos");
			mexico_taxRegimeIdTypes.AddPair("605", "Sueldos y Salarios e Ingresos Asimilados a Salarios");
			mexico_taxRegimeIdTypes.AddPair("606", "Arrendamiento");
			mexico_taxRegimeIdTypes.AddPair("607", "Régimen de Enajenación o Adquisición de Bienes");
			mexico_taxRegimeIdTypes.AddPair("608", "Demás ingresos");
			mexico_taxRegimeIdTypes.AddPair("610", "Residentes en el Extranjero sin Establecimiento Permanente en México");
			mexico_taxRegimeIdTypes.AddPair("611", "Ingresos por Dividendos (socios y accionistas)");
			mexico_taxRegimeIdTypes.AddPair("612", "Personas Físicas con Actividades Empresariales y Profesionales");
			mexico_taxRegimeIdTypes.AddPair("614", "Ingresos por intereses");
			mexico_taxRegimeIdTypes.AddPair("615", "Régimen de los ingresos por obtención de premios");
			mexico_taxRegimeIdTypes.AddPair("616", "Sin obligaciones fiscales");
			mexico_taxRegimeIdTypes.AddPair("620", "Sociedades Cooperativas de Producción que optan por diferir sus ingresos");
			mexico_taxRegimeIdTypes.AddPair("621", "Incorporación Fiscal");
			mexico_taxRegimeIdTypes.AddPair("622", "Actividades Agrícolas, Ganaderas, Silvícolas y Pesqueras");
			mexico_taxRegimeIdTypes.AddPair("623", "Opcional para Grupos de Sociedades");
			mexico_taxRegimeIdTypes.AddPair("624", "Coordinados");
			mexico_taxRegimeIdTypes.AddPair("625", "Régimen de las Actividades Empresariales con ingresos a través de Plataformas Tecnológicas");
			mexico_taxRegimeIdTypes.AddPair("626", "Régimen Simplificado de Confianza");

			#endregion SuppressResourceStringsCheckRegion

			return mexico_taxRegimeIdTypes;
		}
	}
}
