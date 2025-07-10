using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using NUnit.Framework;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	[TestedType(typeof(OrgCusCodeXmlMappings))]
	sealed class OrgCusCodeXmlMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get
			{
				return new[]
				{
					/// Please keep this list in alphabetical order. Pay attention to country/region name, not the containing class.
					typeof(OrgCusCode.AlbaniaCodeTypes),
					typeof(AlgeriaOrgCusCodeInfo.OrgCusCodes),
					typeof(OrgCusCode.AngolaCodeTypes),
					typeof(ArgentinaOrgCusCodeInfo.OrgCusCodes),
					typeof(OrgCusCode.ArmeniaCodeTypes),
					typeof(OrgCusCode.AUQuarantineCodeTypes),
					typeof(OrgCusCode.AustraliaCodeTypes),
					typeof(OrgCusCode.AustriaCodeTypes),
					typeof(OrgCusCode.AzerbaijanCodeTypes),
					typeof(OrgCusCode.BangladeshCodeTypes),
					typeof(OrgCusCode.BarbadosCodeTypes),
					typeof(OrgCusCode.BelarusCodeTypes),
					typeof(BelizeOrgCusCodeInfo.OrgCusCodes),
					typeof(OrgCusCode.BeninCodeTypes),
					typeof(OrgCusCode.BoliviaCodeTypes),
					typeof(OrgCusCode.BosniaAndHerzegovinaCodeTypes),
					typeof(BrazilOrgCusCodeInfo.OrgCusCodes),
					typeof(BurkinaFasoOrgCusCodeInfo.OrgCusCodes),
					typeof(OrgCusCode.CACodeTypes),
					typeof(OrgCusCode.CameroonCodeTypes),
					typeof(OrgCusCode.ChadCodeTypes),
					typeof(ChileOrgCusCodeInfo.OrgCusCodes),
					typeof(OrgCusCode.ChinaCodeTypes),
					typeof(OrgCusCode.CodeTypes),
					typeof(ColombiaOrgCusCodeInfo.OrgCusCodes),
					typeof(CostaRicaOrgCusCodeInfo.OrgCusCodes),
					typeof(CostaRicaOrgCusCodeInfo.OrgCusCodes),
					typeof(OrgCusCode.CoteDivoireCodeTypes),
					typeof(OrgCusCode.CroatiaCodeTypes),
					typeof(OrgCusCode.CubaCodeTypes),
					typeof(OrgCusCode.CuracaoCodeTypes),
					typeof(CzechRepublicOrgCusCodeInfo.OrgCusCodes),
					typeof(OrgCusCode.DenmarkCodeTypes),
					typeof(DominicanRepublicOrgCusCodeInfo.OrgCusCodes),
					typeof(OrgCusCode.EcuadorCodeTypes),
					typeof(OrgCusCode.EgyptCodeTypes),
					typeof(ElSalvadorOrgCusCodeInfo.OrgCusCodes),
					typeof(OrgCusCode.EquatorialGuineaCodeTypes),
					typeof(OrgCusCode.EthiopiaCodeTypes),
					typeof(OrgCusCode.EuropeanUnionSharedCodeTypes),
					typeof(OrgCusCode.FranceCodeTypes),
					typeof(OrgCusCode.FrenchPolynesiaCodeTypes),
					typeof(GermanyOrgCusCodeInfo.OrgCusCodes),
					typeof(OrgCusCode.GhanaCodeTypes),
					typeof(OrgCusCode.GreeceCodeTypes),
					typeof(OrgCusCode.GuamCodeTypes),
					typeof(OrgCusCode.GuatemalaCodeTypes),
					typeof(OrgCusCode.HKCodeTypes),
					typeof(OrgCusCode.HondurasCodeTypes),
					typeof(OrgCusCode.HungaryCodeTypes),
					typeof(OrgCusCode.IcelandCodeTypes),
					typeof(IndiaOrgCusCodeInfo.OrgCusCodes),
					typeof(OrgCusCode.IndonesiaCodeTypes),
					typeof(OrgCusCode.IranCodeTypes),
					typeof(OrgCusCode.IrelandCodeTypes),
					typeof(OrgCusCode.IsraelCodeTypes),
					typeof(ItalyOrgCusCodeInfo.OrgCusCodes),
					typeof(OrgCusCode.JamaicaCodeTypes),
					typeof(OrgCusCode.JapanCodeTypes),
					typeof(OrgCusCode.KenyaCodeTypes),
					typeof(OrgCusCode.KiribatiCodeTypes),
					typeof(KoreaSouthComplianceInfo.CodeTypes),
					typeof(OrgCusCode.KosovoCodeTypes),
					typeof(OrgCusCode.LatviaCodeTypes),
					typeof(OrgCusCode.LebanonCodeTypes),
					typeof(OrgCusCode.LithuaniaCodeTypes),
					typeof(OrgCusCode.LuxembourgCodeTypes),
					typeof(MacauOrgCusCodeInfo.OrgCusCodes),
					typeof(OrgCusCode.MadagascarCodeTypes),
					typeof(OrgCusCode.MalawiCodeTypes),
					typeof(MalaysiaOrgCusCodeInfo.OrgCusCodes),
					typeof(OrgCusCode.MaldivesCodeTypes),
					typeof(OrgCusCode.MaliCodeTypes),
					typeof(MexicoOrgCusCodeInfo.OrgCusCodes),
					typeof(OrgCusCode.MoroccoCodeTypes),
					typeof(OrgCusCode.MozambiqueCodeTypes),
					typeof(OrgCusCode.MyanmarCodeTypes),
					typeof(OrgCusCode.NetherlandsCodeTypes),
					typeof(OrgCusCode.NewCaledoniaCodeTypes),
					typeof(OrgCusCode.NicaraguaCodeTypes),
					typeof(OrgCusCode.NigerCodeTypes),
					typeof(OrgCusCode.NigeriaCodeTypes),
					typeof(OrgCusCode.NorthernMarianaIslandsCodeTypes),
					typeof(OrgCusCode.NorwayCodeTypes),
					typeof(OrgCusCode.NZCodeTypes),
					typeof(OrgCusCode.PalauCodeTypes),
					typeof(PanamaOrgCusCodeInfo.OrgCusCodes),
					typeof(OrgCusCode.ParaguayCodeTypes),
					typeof(OrgCusCode.PeruCodeTypes),
					typeof(OrgCusCode.PolandCodeTypes),
					typeof(OrgCusCode.PuertoRicoCodeTypes),
					typeof(OrgCusCode.RomaniaCodeTypes),
					typeof(OrgCusCode.RussiaCodeTypes),
					typeof(OrgCusCode.RwandaCodeTypes),
					typeof(OrgCusCode.SamoaCodeTypes),
					typeof(SaudiArabiaOrgCusCodeInfo.OrgCusCodes),
					typeof(OrgCusCode.SenegalCodeTypes),
					typeof(SerbiaOrgCusCodeInfo.OrgCusCodes),
					typeof(OrgCusCode.SierraLeoneCodeTypes),
					typeof(OrgCusCode.SingaporeCodeTypes),
					typeof(OrgCusCode.SlovakiaCodeTypes),
					typeof(OrgCusCode.SloveniaCodeTypes),
					typeof(OrgCusCode.SouthAfricaCodeTypes),
					typeof(OrgCusCode.SpainCodeTypes),
					typeof(OrgCusCode.SriLankaCodeTypes),
					typeof(OrgCusCode.SwissCodeTypes),
					typeof(OrgCusCode.TaiwanCodeTypes),
					typeof(OrgCusCode.TanzaniaCodeTypes),
					typeof(OrgCusCode.ThailandCodeTypes),
					typeof(OrgCusCode.TogoCodeTypes),
					typeof(OrgCusCode.TongaCodeTypes),
					typeof(OrgCusCode.TrinidadAndTobagoCodeTypes),
					typeof(TurkeyOrgCusCodeInfo.OrgCusCodes),
					typeof(UgandaOrgCusCodeInfo.OrgCusCodes),
					typeof(OrgCusCode.UnitedArabEmiratesCodeTypes),
					typeof(OrgCusCode.UnitedKingdomCodeTypes),
					typeof(UruguayOrgCusCodeInfo.OrgCusCodes),
					typeof(OrgCusCode.USACodeTypes),
					typeof(OrgCusCode.VenezuelaCodeTypes),
					typeof(OrgCusCode.ZambiaCodeTypes),
					typeof(ZimbabweOrgCusCodeInfo.OrgCusCodes),
					// Please search for your country/region name before adding it to the end of this list.
				};
			}
		}

		protected override bool EnterpriseAndExternalCodeShouldBeSame
		{
			get { return true; }
		}

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst()
		{
			var list = new List<string> { OrgCusCode.CodeTypes.WarehouseControlledPremisesID, OrgCusCode.CodeTypes.DepotControlledPremisesID };
			list.AddRange(new MasterFiles.Business.Customs.CN.EnterpriseQualificationList().GetAllCodes());
			return list.ToArray();
		}

		public void TestAllCusCodeCodeTypeClassesAreListedForXMLMappingChecking()
		{
			StringBuilder missingClasses = new StringBuilder();

			Type[] nestedCodeTypes = typeof(OrgCusCode).GetNestedTypes(BindingFlags.Public);
			Assert("NestedCodeTypes has list of classes", nestedCodeTypes.Length > 0);

			foreach (Type nestedClass in nestedCodeTypes)
			{
				if (!nestedClass.IsAssignableFrom(typeof(TestCase)) && nestedClass.Name.Contains("CodeType") && !ClassIsRegisteredForXMLMappingCheck(nestedClass))
				{
					missingClasses.AppendLine(nestedClass.Name);
				}
			}

			if (missingClasses.Length > 0)
			{
				Fail("There are some CodeType classes declared in OrgCusCode in MasterFiles which are NOT registered for XML mapping checking in Datatransfer. Please add the class types below to the EnterpriseCodeDefinitionsClassesToCheckAgainst property in DataTransfer\\OrgCusCodeXmlMappingsTest:\r\n\r\n" + missingClasses);
			}
		}

		public void TestAllCusCodeCodeTypeClassesAreListedForXMLMappingChecking_DeepCheck()
		{
			OrgCusCode orgCusCode = new BusinessObjectFactory().New<OrgCusCode>();
			List<string> missedCusCodes = new List<string>();
			List<string> excludedCusCodes = new List<string>(GetEnterpriseCodesToExcludeCheckingAgainst());

			foreach (RefCountry country in orgCusCode.Lookups.CodeCountries)
			{
				int oldCount = missedCusCodes.Count;

				orgCusCode.OK_RN_NKCodeCountry = country.Code;
				missedCusCodes.AddRange(
					from ICodeDescription codeType in orgCusCode.Lookups.OK_CodeType_List
					where !OrgCusCodeXmlMappings.Instance.ContainsEnterpriseCode(codeType.Code) && !excludedCusCodes.Contains(codeType.Code)
					select codeType.Code);

				if (missedCusCodes.Count > oldCount)
				{
					missedCusCodes[oldCount] = "\r\nCountry: " + country.Code.ToString() + ", Codes: " + missedCusCodes[oldCount];
				}
			}

			Assert("Following Cus Codes are not mapped for XML transfer:" + string.Join(", ", missedCusCodes.ToArray()), missedCusCodes.Count == 0);
		}

		bool ClassIsRegisteredForXMLMappingCheck(Type nestedClass)
		{
			return Array.Exists(EnterpriseCodeDefinitionsClassesToCheckAgainst, registeredType => nestedClass == registeredType);
		}
	}
}
