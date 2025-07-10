using System;
using CargoWise.IO;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class ExportIncoTermAndCustomsChargeFactoryTest : IncoTermAndCustomsChargeFactoryTest
	{
		public override void TestConfigurationFile()
		{
			var incotermFactory = new ExportIncoTermAndCustomsChargeFactoryForTest();
			AssertEquals("Export Incoterm Factory should use export configuration xml", "Enterprise.Customs.FR.Business.Declaration.Valuation.ExportIncoTermsConfiguration.xml", incotermFactory.ConfigurationFile);
		}

		public override void TestIncotermsConfigurationXML()
		{
			var config = XMLExtractor.GetIncoTermsConfiguration("Enterprise.Customs.FR.Business.Declaration.Valuation.ExportIncoTermsConfiguration.xml");
			AssertNotNull(config);
			AssertEquals("Mapping count", 21, config.Mapping.Length);
			AssertEquals("EXW", "EXW", config.Mapping[0].Filter.IncoTerm);
			AssertEquals("Agreed place", "1", config.Mapping[0].Filter.AgreedPlace);
			AssertEquals("2 charges expected", 2, config.Mapping[0].Charges.Charge.Length);
			AssertEquals("ChargeType", "FRE", config.Mapping[0].Charges.Charge[0].ChargeType);
			AssertEquals("IsIncludedInInvoice", false, config.Mapping[0].Charges.Charge[0].IsIncludedInInvoice);
			AssertEquals("IsMandatory", true, config.Mapping[0].Charges.Charge[0].IsMandatory);
			AssertEquals("IsDutiable", true, config.Mapping[0].Charges.Charge[0].IsDutiable);
			AssertEquals("IsStatisticalValueApplicable", true, config.Mapping[0].Charges.Charge[0].IsStatisticalValueApplicable);
			AssertEquals("IsGSTApplicable", false, config.Mapping[0].Charges.Charge[0].IsGSTApplicable);
			AssertEquals("FNE", "FNE", config.Mapping[0].Charges.Charge[1].ChargeType);
			AssertEquals("IsIncludedInInvoice", false, config.Mapping[0].Charges.Charge[1].IsIncludedInInvoice);
			AssertEquals("IsMandatory", false, config.Mapping[0].Charges.Charge[1].IsMandatory);
			AssertEquals("IsDutiable", true, config.Mapping[0].Charges.Charge[1].IsDutiable);
			AssertEquals("IsStatisticalValueApplicable", true, config.Mapping[1].Charges.Charge[1].IsStatisticalValueApplicable);
			AssertEquals("IsGSTApplicable", false, config.Mapping[0].Charges.Charge[1].IsGSTApplicable);
			AssertEquals("FRE", "FRE", config.Mapping[1].Charges.Charge[0].ChargeType);
		}

		protected override string IncoTermAndCustomsChargeConfigurationFilename => "Enterprise.Customs.FR.Business.Testing.Declaration.Valuation.ExportIncoTermAndCustomsChargeConfiguration.csv";

		public override void TestIncoTermAndCustomsChargeConfiguration()
		{
			var resultData = new ZStringBuilder(DataHeading);
			var incoTermFactory = new ExportIncoTermAndCustomsChargeFactory();
			var customsChargeCodeDictionary = incoTermFactory.CustomsChargeCodeDictionary;

			foreach (var incoTermChargeConfiguration in customsChargeCodeDictionary)
			{
				var key = incoTermChargeConfiguration.Key;
				foreach (var charge in incoTermChargeConfiguration.Value)
				{
					resultData.Append(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
									key.IncoTerm,
									key.EffectiveAgreedPlaceCode,
									key.ModeOfTransport,
									key.AirRouteType,
									charge.Code,
									charge.IsIncludedInITOT,
									charge.IsDutiable,
									charge.IsStatisticalValueApplicable,
									charge.IsVATible));
				}
			}
			AssertMultilineASCIIEquals("Text from file '" + IncoTermAndCustomsChargeConfigurationFilename + "'", resourceRetriever.Value.GetString(IncoTermAndCustomsChargeConfigurationFilename), resultData.ToStringWithNewLineBetweenAppends());
		}
		const string DataHeading = "IncoTerm, AgreedPlace, TransportMode, AirRouteType, ChargeCode, IsIncludedInInvoice, IsDutiable, IsStatisticalValueApplicable, IsGSTApplicable";

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
	public class ExportIncoTermAndCustomsChargeFactoryForTest : ExportIncoTermAndCustomsChargeFactory
	{
		public ExportIncoTermAndCustomsChargeFactoryForTest() : base()
		{
		}

		public ZString ConfigurationFile => GetConfigurationFile();
	}
}
