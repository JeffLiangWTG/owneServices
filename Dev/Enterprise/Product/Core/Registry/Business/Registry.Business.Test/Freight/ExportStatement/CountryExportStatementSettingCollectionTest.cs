using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CountryExportStatementSettingCollection))]
	sealed class CountryExportStatementSettingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CountryExportStatementSettingCollection>
	{
		public void TestIndexer_UsingCountryCode()
		{
			CountryExportStatementSettingCollection settings = new CountryExportStatementSettingCollection();
			CountryExportStatementSetting setting1 = settings.AddNew();
			setting1.CountryCode = "AU";

			CountryExportStatementSetting setting2 = settings.AddNew();
			setting2.CountryCode = "US";

			AssertEquals(setting1, settings["AU"]);
			AssertEquals(setting2, settings["US"]);
		}

		public void TestGetExportStatementDescriptionPairListForCountry()
		{
			var countrySettings = FreightDataRegistry.Instance.ExportStatementSettings.Value;

			var pairList = countrySettings.GetExportStatementDescriptionPairListForCountry("");
			AssertEquals(0, pairList.Count);

			pairList = countrySettings.GetExportStatementDescriptionPairListForCountry(Core.Constants.CountryCodes.Australia);
			AssertEquals(0, pairList.Count);

			pairList = countrySettings.GetExportStatementDescriptionPairListForCountry(Core.Constants.CountryCodes.UnitedStates);
			AssertEquals(44, pairList.Count);

			pairList = countrySettings.GetExportStatementDescriptionPairListForCountry(Core.Constants.CountryCodes.PuertoRico);
			AssertEquals(44, pairList.Count);

			pairList = countrySettings.GetExportStatementDescriptionPairListForCountry(Core.Constants.CountryCodes.Guam);
			AssertEquals(44, pairList.Count);

			pairList = countrySettings.GetExportStatementDescriptionPairListForCountry(Core.Constants.CountryCodes.AmericanSamoa);
			AssertEquals(44, pairList.Count);

			pairList = countrySettings.GetExportStatementDescriptionPairListForCountry(Core.Constants.CountryCodes.VirginIslands);
			AssertEquals(44, pairList.Count);

			pairList = countrySettings.GetExportStatementDescriptionPairListForCountry(Core.Constants.CountryCodes.NorthernMarianaIslands);
			AssertEquals(44, pairList.Count);

			var testData = GetUsReferenceExportStatementSettings().ToList();
			for (var order = 0; order < pairList.Count; order++)
			{
				AssertExportStatement(pairList[order], testData[order].Code, testData[order].StatementDescription);
			}
		}

		public void TestGetMandatoryStatements()
		{
			CountryExportStatementSettingCollection countrySettings = new CountryExportStatementSettingCollection();
			CountryExportStatementSetting sEDSetting = countrySettings.AddNew();
			sEDSetting.CountryCode = Core.Constants.CountryCodes.Australia;
			sEDSetting.Statements.Add(new ExportStatementSetting(sEDSetting, "GBH", "A user defined statement", "", "", "", "UDF", true, true, true, true, true, true));
			sEDSetting.Statements.Add(new ExportStatementSetting(sEDSetting, "MAT", "A mandatory statement", "", "", "", "MAN", true, true, true, true, true, true));
			FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, countrySettings);
			CodeDescriptionPairList pairList = countrySettings.GetExportStatementDescriptionPairListForCountry(Core.Constants.CountryCodes.Australia);
			AssertEquals(1, pairList.Count);
			AssertExportStatement(pairList[0], "GBH", "A user defined statement");
		}

		void AssertExportStatement(ICodeDescription pair, string code, string description)
		{
			AssertEquals("Code", code, pair.Code);
			AssertEquals("Description", description, pair.Description);
		}

		public void TestAddDefaultValues()
		{
			var countrySettings = new CountryExportStatementSettingCollection();

			countrySettings.AddDefaultValues("BL");
			AssertEquals("Count", 0, countrySettings.Count);

			countrySettings.AddDefaultValues(Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("Count", 1, countrySettings.Count);
			var sedSetting = countrySettings[0];
			AssertEquals("CountryCode", Core.Constants.CountryCodes.UnitedStates, sedSetting.CountryCode);
			AssertEquals("SEDSetting.Statements.Count", 44, sedSetting.Statements.Count);

			countrySettings.AddDefaultValues(Core.Constants.CountryCodes.PuertoRico);
			AssertEquals("Count", 2, countrySettings.Count);
			sedSetting = countrySettings[1];
			AssertEquals("CountryCode", Core.Constants.CountryCodes.PuertoRico, sedSetting.CountryCode);
			AssertEquals("SEDSetting.Statements.Count", 44, sedSetting.Statements.Count);

			countrySettings.AddDefaultValues(Core.Constants.CountryCodes.Guam);
			AssertEquals("Count", 3, countrySettings.Count);
			sedSetting = countrySettings[2];
			AssertEquals("CountryCode", Core.Constants.CountryCodes.Guam, sedSetting.CountryCode);
			AssertEquals("SEDSetting.Statements.Count", 44, sedSetting.Statements.Count);

			var testData = GetUsReferenceExportStatementSettings().ToList();
			for (var order = 0; order < sedSetting.Statements.Count; order++)
			{
				AssertExportStatementSetting(sedSetting.Statements[order], testData[order].Code, testData[order].Statement, testData[order].StatementDescription);
			}
		}

		void AssertExportStatementSetting(ExportStatementSetting setting, string name, string statement, string statementDescription)
		{
			AssertEquals("Code", name, setting.Code);
			AssertEquals("Statement", statement, setting.Statement);
			AssertEquals("StatementDescription", statementDescription, setting.StatementDescription);
			AssertEquals("Field1", "", setting.Field1);
			AssertEquals("Field2", "", setting.Field2);
			AssertEquals("Visibility", "UDF", setting.Visibility);
			AssertEquals("UseOnHawb", true, setting.UseOnHawb);
			AssertEquals("UseOnDirectIATAMawb", true, setting.UseOnDirectIATAMawb);
			AssertEquals("UseOnConsolidationMawb", true, setting.UseOnConsolidationMawb);
			AssertEquals("UseOnHouseBillOfLading", true, setting.UseOnHouseBillOfLading);
			AssertEquals("UseOnDirectMasterBillOfLading", true, setting.UseOnDirectMasterBillOfLading);
			AssertEquals("UseOnConsolidationMasterBillOfLading", true, setting.UseOnConsolidationMasterBillOfLading);
		}

		public void TestIsDuplicateSetting()
		{
			CountryExportStatementSettingCollection countrySettings = new CountryExportStatementSettingCollection();
			CountryExportStatementSetting countrySetting1 = countrySettings.AddNew();
			countrySetting1.CountryCode = Core.Constants.CountryCodes.Australia;
			AssertEquals("Should not be duplicate", false, countrySettings.IsDuplicateSetting(countrySetting1));

			CountryExportStatementSetting countrySetting2 = countrySettings.AddNew();
			countrySetting2.CountryCode = countrySetting1.CountryCode;
			AssertEquals("Should be duplicate", true, countrySettings.IsDuplicateSetting(countrySetting1));

			countrySetting2.CountryCode = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals("Should not be duplicate", false, countrySettings.IsDuplicateSetting(countrySetting1));
		}

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CountryExportStatementSetting();
		}

		protected override CountryExportStatementSettingCollection GetCollectionToTest()
		{
			return new CountryExportStatementSettingCollection();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion

		static IEnumerable<(string Code, string Statement, string StatementDescription)> GetUsReferenceExportStatementSettings()
		{
			yield return ("PRF", "AES", "AES Proof of Filing Citation");
			yield return ("ASH", "AES", "AES Split Shipments");
			yield return ("PDU", "AESPOST", "Postdeparture Citation-USPPI");
			yield return ("PDA", "AESPOST", "Postdeparture Citation-Agent");
			yield return ("DWN", "AESDOWN", "AES Downtime Citation");
			yield return ("LOW", "NOEEI §30.37(a)", "NOEEI §30.37(a) - Low Value (<$2501)");
			yield return ("TOT", "NOEEI §30.37(b)", "NOEEI §30.37(b) - Tools of trade");
			yield return ("TMP", "NOEEI §30.37(r)", "NOEEI §30.37(r) - Return of Temporary Import Bond");
			yield return ("CAS", "NOEEI §30.36", "NOEEI §30.36");
			yield return ("ARM", "NOEEI §30.39", "NOEEI §30.39 - Shipments to US armed services");
			yield return ("BND", "NOEEI §30.2(d)(1)", "NOEEI §30.2(d)(1) - Goods under CBP bond, not in consumption");
			yield return ("TER", "NOEEI §30.2(d)(2)", "NOEEI §30.2(d)(2) - Goods between US and US territories (not PR, VI)");
			yield return ("EET", "NOEEI §30.2(d)(3)", "NOEEI §30.2(d)(3) - Exclusion for electronic transmissions and intangible transfers");
			yield return ("GFT", "NOEEI §30.37(h)", "NOEEI §30.37(h) - 15 CFR 740.12(a)&(b) Gifts and donations");
			yield return ("DIP", "NOEEI §30.37(i)", "NOEEI §30.37(i) - Diplomatic pouches");
			yield return ("REM", "NOEEI §30.37(j)", "NOEEI §30.37(j) - Human remains");
			yield return ("AVS", "NOEEI §30.37(o)", "NOEEI §30.37(o) - 15 CFR 740.15(c) Parts for US airlines");
			yield return ("TME", "NOEEI §30.37(q)", "NOEEI §30.37(q) - Temporary Exports (1 YR)");
			yield return ("GBN", "NOEEI §30.2(d)(4)", "NOEEI §30.2(d)(4) - Goods to Guantanamo Bay Naval Base");
			yield return ("IWA", "NOEEI §30.2(d)(5)", "NOEEI §30.2(d)(5) - Ultimate Dest. US or Int'l Waters for US person");
			yield return ("UMC", "NOEEI §30.37(c)", "NOEEI §30.37(c) - Shipments from US to US transiting through MX or CA");
			yield return ("MCU", "NOEEI §30.37(d)", "NOEEI §30.37(d) - Shipments from MX to CA or CA to MX transiting through US");
			yield return ("TSW", "NOEEI §30.37(f)", "NOEEI §30.37(f) - 15 CFR 772 Technology and software");
			yield return ("BKS", "NOEEI §30.37(g)", "NOEEI §30.37(g) - Literature to libraries, governments");
			yield return ("BUS", "NOEEI §30.37(k)", "NOEEI §30.37(k) - Company records");
			yield return ("PET", "NOEEI §30.37(l)", "NOEEI §30.37(l) - Pets as baggage");
			yield return ("CAR", "NOEEI §30.37(m)", "NOEEI §30.37(m) - Carriers' stores");
			yield return ("DUN", "NOEEI §30.37(n)", "NOEEI §30.37(n) - Dunnage");
			yield return ("BGG", "NOEEI §30.37(p)", "NOEEI §30.37(p) - Passenger, Crew Baggage");
			yield return ("BNK", "NOEEI §30.37(s)", "NOEEI §30.37(s) - Issued bank notes, securities, coins");
			yield return ("TDC", "NOEEI §30.37(t)", "NOEEI §30.37(t) - International transaction documents");
			yield return ("DAT", "NOEEI §30.37(u)", "NOEEI §30.37(u) - 22 CFR 123.22(b)(3)(iii) Technical data, Defense services");
			yield return ("VSL", "NOEEI §30.37(v)", "NOEEI §30.37(v) - Shipping containers");
			yield return ("APO", "NOEEI §30.37(w)", "NOEEI §30.37(w) - Shipments to Army Post Office, Diplomatic Post Office, Fleet Post Office");
			yield return ("BAG", "NOEEI §30.37(x)", "NOEEI §30.37(x) - 15 CFR 740.14 Baggage");
			yield return ("OFE", "NOEEI §30.40(a)", "NOEEI §30.40(a) - Office equipment for US gov offices");
			yield return ("HHG", "NOEEI §30.40(b)", "NOEEI §30.40(b) - Household goods for US gov employees");
			yield return ("FME", "NOEEI §30.40(c)", "NOEEI §30.40(c) - Food, medicines, supplies for US gov");
			yield return ("EY1", "NOEEI §30.37(y)(1)", "NOEEI §30.37(y)(1) – Published literature/media destined to country group E:1 and E:2");
			yield return ("EY2", "NOEEI §30.37(y)(2)", "NOEEI §30.37(y)(2) – Shipments to U.S. government destined to Country Group E:1 and E:2 under License Exception GOV");
			yield return ("EY3", "NOEEI §30.37(y)(3)", "NOEEI §30.37(y)(3) - Personal effects exported as exemption BAG to Country Group E:1 and E:2");
			yield return ("EY4", "NOEEI §30.37(y)(4)", "NOEEI §30.37(y)(4) - Gifts/donations exported to Country Group E:1 and E:2 under License Exception GFT");
			yield return ("EY5", "NOEEI §30.37(y)(5)", "NOEEI §30.37(y)(5) – Vessels/Aircraft temporary export to Country Group E:1 or E:2 under License Exception AVS");
			yield return ("EY6", "NOEEI §30.37(y)(6)", "NOEEI §30.37(y)(6) - Tools of trade temporary export (1yr) for use in Country Group E:1 or E:2 under License Exception BAG or TMP");
		}
	}
}
