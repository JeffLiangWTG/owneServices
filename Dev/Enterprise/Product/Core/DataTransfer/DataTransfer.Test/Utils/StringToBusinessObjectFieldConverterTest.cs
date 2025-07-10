using System;
using System.Reflection;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class StringToBusinessObjectFieldConverterTest : TestCaseWithFactory
	{
		#region SetPropertyInfoValue

		public void TestSetPropertyInfoValue()
		{
			OrgHeader bO = Factory.New<OrgHeader>();
			NotificationBuffer notify = new NotificationBuffer(null);
			StringToBusinessObjectFieldConverter.InstanceForCurrentCompany.SetPropertyInfoValue(bO.ZPropertyInfoHash[OrgHeader.Schema.OH_RL_NKClosestPort], "AUSYD", ForeignKeyType.PortNK, notify);

			AssertEquals("Should have no errors", false, notify.HasErrors);
			AssertEquals("Should set the port ok", "AUSYD", bO.OH_RL_NKClosestPort);

			StringToBusinessObjectFieldConverter.InstanceForCurrentCompany.SetPropertyInfoValue(bO.ZPropertyInfoHash[OrgHeader.Schema.OH_RL_NKClosestPort], "ZZTESTING", ForeignKeyType.PortNK, notify);

			AssertEquals("Should have no errors", false, notify.HasErrors);
			AssertEquals("Should set the port to first 5 characters", "ZZTES", bO.OH_RL_NKClosestPort);
			INotification[] notifications = notify.GetEventsByType(WarningType.Warning);
			AssertEquals(true, notifications.Length > 0);
			bool found = false;
			foreach (NotificationSubscriberNotification notification in notifications)
			{
				if (notification.AdditionalInfo == "Could not find Port (ZZTESTING)")
				{
					found = true;
					break;
				}
			}

			AssertEquals("Notify Contains: " + System.Environment.NewLine + notify.AsString, true, found);
		}

		public void TestSetPropertyInfoValue_WhenStringExceeds()
		{
			OrgHeader bO = Factory.New<OrgHeader>();
			NotificationBuffer notify = new NotificationBuffer(null);
			StringToBusinessObjectFieldConverter.InstanceForCurrentCompany.SetPropertyInfoValue(bO.ZPropertyInfoHash[OrgHeader.Schema.OH_RL_NKClosestPort], "123456789", notify);

			AssertEquals("Should set the the string ok although a bit truncated", "12345", bO.OH_RL_NKClosestPort);
			AssertEquals("Should have max length error", true, notify.ContainsNotificationType(WarningType.MaxLengthExceeded));
		}

		public void TestSetDecimalPropertyInfoValue()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			NotificationBuffer notification = new NotificationBuffer(null);

			dummy.Z0_AnotherDecimal = 10;
			StringToBusinessObjectFieldConverter.InstanceForCurrentCompany.SetPropertyInfoValue(dummy.Z0_AnotherDecimalInfo, 999999.999m, 9, 3, notification, "Blah");
			AssertEquals("Should have no errors", false, notification.HasErrors);
			AssertEquals("Should have set the value", 999999.999m, dummy.Z0_AnotherDecimal);

			dummy.Z0_AnotherDecimal = 10;
			StringToBusinessObjectFieldConverter.InstanceForCurrentCompany.SetPropertyInfoValue(dummy.Z0_AnotherDecimalInfo, 1000000.000m, 9, 3, notification, "Blah");
			AssertEquals("Should have added an error", true, notification.HasErrors);
			AssertEquals("Error text", "Error: Value overflow error (Blah; value = 1000000.000, max = 999999.999)\r\n", notification.AsString);
			AssertEquals("Should have not set the value", 10m, dummy.Z0_AnotherDecimal);
		}

		public void TestSetZDatePropertyInfoValue()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			NotificationBuffer notification = new NotificationBuffer(null);
			var today = DateTime.Today;

			dummy.Z0_DateOnly = new ZDate(2000, 2, 2);
			StringToBusinessObjectFieldConverter.InstanceForCurrentCompany.SetPropertyInfoValue(dummy.Z0_DateOnlyInfo, today);
			AssertEquals("Should have set the value", new ZDate(today), dummy.Z0_DateOnly);

			StringToBusinessObjectFieldConverter.InstanceForCurrentCompany.SetPropertyInfoValue(dummy.Z0_DateOnlyInfo, DateTime.MaxValue);
			AssertEquals("Max value", new ZDate(DateTime.MaxValue), dummy.Z0_DateOnly);
			StringToBusinessObjectFieldConverter.InstanceForCurrentCompany.SetPropertyInfoValue(dummy.Z0_DateOnlyInfo, DateTime.MinValue);
			AssertEquals("Min value", ZDateTime.Empty, dummy.Z0_DateOnly);
		}

		#endregion

		#region Matching/Fuzzy Matching on Org

		public void TestNoMatchOnOrgFullNameEmpty()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			OrgHeader[] allOrgsWithEmptyFullName = (OrgHeader[])factory.Load(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.Equal, ZString.Empty));
			foreach (OrgHeader org in allOrgsWithEmptyFullName)
			{
				org.Delete();
			}

			OrgHeader testOrg = factory.New<OrgHeader>();
			testOrg.OH_FullName = "";
			ZGuid unmatchOrgPK = factory.New(typeof(OrgHeader)).PK;

			NotificationBuffer buffer = new NotificationBuffer(null);
			TestStringToBusinessObjectFieldConverterForCurrentCompany converter = new TestStringToBusinessObjectFieldConverterForCurrentCompany();
			converter.fUnmatchOrgPK = unmatchOrgPK;
			ZGuid actualOrgPK = converter.GetPKFromNKGivenFKType(factory, "", ForeignKeyType.OrganisationMatchWithFullName, buffer);
			AssertEquals("Unmatch org should be returned, not the org with the empty full name", unmatchOrgPK, actualOrgPK);
			AssertEquals("Should be an error specifying no match", true, buffer.ContainsNotificationType(ErrorType.MissingPKFromNK));
			AssertEquals("That should be the only error", 1, buffer.Events.Length);
		}

		class TestStringToBusinessObjectFieldConverterForCurrentCompany : StringToBusinessObjectFieldConverter
		{
			public ZGuid fUnmatchOrgPK;

			public TestStringToBusinessObjectFieldConverterForCurrentCompany()
				: base(GlbCompany.CurrentCompany.GC_OH_OrgProxy)
			{
			}

			protected override ZGuid UnmatchOrgPK
			{
				get { return fUnmatchOrgPK; }
			}
		}

		public void TestCantFindOrgSoUseUnmatchAccountOrg()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgHeader unmatchOrg = factory.New<OrgHeader>();
			unmatchOrg.OH_Code = "unmatch";

			TestStringToBusinessObjectFieldConverterForCurrentCompany converter = new TestStringToBusinessObjectFieldConverterForCurrentCompany();
			converter.fUnmatchOrgPK = unmatchOrg.PK;
			ZGuid resultantOrgPK = converter.GetPKFromNKGivenFKType(
				factory, "test", ForeignKeyType.OrganisationMatchWithFullName, new NotificationBuffer(null));
			AssertEquals(resultantOrgPK, unmatchOrg.PK);
		}

		public void TestFuzzyOrgMatch()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgHeader fuzzyOrg = factory.New<OrgHeader>();
			fuzzyOrg.OH_FullName = "fuzzy ltd";
			fuzzyOrg.MainAddress.OA_Address1 = "Address1";
			fuzzyOrg.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(fuzzyOrg);

			OrgHeader exactOrg = factory.New<OrgHeader>();
			exactOrg.OH_FullName = "exact";
			exactOrg.MainAddress.OA_Address1 = "Address1";
			OrgHeader decoyExactOrg = factory.New<OrgHeader>();
			decoyExactOrg.OH_FullName = "exact ltd";
			decoyExactOrg.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(decoyExactOrg);

			OrgHeader mappedOrg = factory.New<OrgHeader>();
			mappedOrg.OH_FullName = "splat";
			mappedOrg.MainAddress.OA_Address1 = "Address1";
			mappedOrg.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(mappedOrg);
			OrgPatternMatchOverride orgOverride = factory.New<OrgPatternMatchOverride>();
			orgOverride.OO_Relationship = Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			orgOverride.OO_ForeignCode = "mappedorg";
			orgOverride.OO_LocalGuid = mappedOrg.PK;
			orgOverride.OO_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			OrgHeader decoyMappedOrg = factory.New<OrgHeader>();
			decoyMappedOrg.OH_FullName = "mappedorg";
			decoyMappedOrg.MainAddress.OA_Address1 = "Address1";
			decoyMappedOrg.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(decoyMappedOrg);

			NotificationBuffer buffer = new NotificationBuffer(null);
			ZGuid fuzzyMatch = StringToBusinessObjectFieldConverter.InstanceForCurrentCompany.GetPKFromNKGivenFKType(factory, "fuzzy", ForeignKeyType.OrganisationMatchWithFullName, buffer);
			AssertEquals("Match by a fuzzy match", fuzzyMatch, fuzzyOrg.PK);
			AssertEquals("Should notify user of fuzzy match", true, buffer.ContainsNotificationType(ErrorType.OrgMatchedFuzzy));

			ZGuid exactMatch = StringToBusinessObjectFieldConverter.InstanceForCurrentCompany.GetPKFromNKGivenFKType(factory, "exact", ForeignKeyType.OrganisationMatchWithFullName, buffer);
			AssertEquals("Matched by an exact match", exactMatch, exactOrg.PK);

			ZGuid mappedOrgMatch = StringToBusinessObjectFieldConverter.InstanceForCurrentCompany.GetPKFromNKGivenFKType(factory, "mappedorg", ForeignKeyType.OrganisationMatchWithFullName, buffer);
			AssertEquals("Matched by explicit org mapping", mappedOrgMatch, mappedOrg.PK);
		}

		public void TestOrgProxyPKChange()
		{
			var instance1 = StringToBusinessObjectFieldConverter.InstanceForCurrentCompany;
			var orgProxyPK = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			AssertEquals(orgProxyPK, instance1.MappingOrgPK);
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_OH_OrgProxy)).PK;
			Factory.Save();
			var instance2 = StringToBusinessObjectFieldConverter.InstanceForCurrentCompany;
			AssertEquals(company.GC_OH_OrgProxy, instance2.MappingOrgPK);
			AssertNotEquals("New Instance should be created when proxy is changed", instance1, instance2);
		}

		public void TestMoreThan1FuzzyOrgMatchFailureMode()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgHeader moreThan1MatchOrg1 = factory.New<OrgHeader>();
			moreThan1MatchOrg1.OH_FullName = "something";
			moreThan1MatchOrg1.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(moreThan1MatchOrg1);
			OrgHeader moreThan1MatchOrg2 = factory.New<OrgHeader>();
			moreThan1MatchOrg2.OH_FullName = "something";
			moreThan1MatchOrg2.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(moreThan1MatchOrg2);

			ZGuid unmatchOrgPK = factory.New(typeof(OrgHeader)).PK;

			NotificationBuffer buffer = new NotificationBuffer(null);
			TestStringToBusinessObjectFieldConverterForCurrentCompany converter = new TestStringToBusinessObjectFieldConverterForCurrentCompany();
			converter.fUnmatchOrgPK = unmatchOrgPK;
			ZGuid moreThan1Match = converter.GetPKFromNKGivenFKType(factory, "something", ForeignKeyType.OrganisationMatchWithFullName, buffer);
			Assert("Testing more than 1 match", buffer.ContainsNotificationType(ErrorType.MoreThan1NKMatch));
			AssertEquals("Should return unmatch account org", unmatchOrgPK, moreThan1Match);
		}

		public void TestUnmatchedOrgMatchFailureMode()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgHeader crapOrg = factory.New<OrgHeader>();
			crapOrg.OH_FullName = "something";
			crapOrg.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(crapOrg);

			ZGuid unmatchOrgPK = factory.New(typeof(OrgHeader)).PK;

			NotificationBuffer buffer = new NotificationBuffer(null);
			TestStringToBusinessObjectFieldConverterForCurrentCompany converter = new TestStringToBusinessObjectFieldConverterForCurrentCompany();
			converter.fUnmatchOrgPK = unmatchOrgPK;
			ZGuid actualMatch = converter.GetPKFromNKGivenFKType(factory, "somethingelse", ForeignKeyType.OrganisationMatchWithFullName, buffer);
			Assert("Missing Fuzzy Org failure mode", buffer.ContainsNotificationType(ErrorType.MissingPKFromNK));
			AssertEquals("Should return unmatch account org", unmatchOrgPK, actualMatch);
		}

		public void TestAdditionalFuzzyOrgEnumsAddedRecently()
		{
			AssertEquals(
				"Expecting only 6 OrgMatchType types, better add stuff to the switch statement",
				6, Enum.GetNames(typeof(OrgMatchType)).Length);
		}

		#endregion

		#region Matching Ports on Pattern Match Override Table

		public void TestMatchingPortsOnPatternMatchOverrideTable()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			RefUNLOCO unloco = factory.New<RefUNLOCO>();
			unloco.RL_PortName = "something";
			OrgPatternMatchOverride orgOverride = factory.New<OrgPatternMatchOverride>();
			orgOverride.OO_Relationship = Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			orgOverride.OO_ForeignCode = "mapped_portname";
			orgOverride.OO_LocalGuid = unloco.PK;
			orgOverride.OO_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			NotificationBuffer buffer = new NotificationBuffer(null);
			TestStringToBusinessObjectFieldConverterForCurrentCompany converter = new TestStringToBusinessObjectFieldConverterForCurrentCompany();
			ZGuid unlocoMatch = converter.GetPKFromNKGivenFKType(factory, "mapped_portname", ForeignKeyType.PortNK, buffer);
			AssertEquals("Pattern match override table for RefUNLOCO", unloco.PK, unlocoMatch);
		}

		#endregion

		#region Matching Charge Codes on Pattern Match Override Table

		public void TestMatchingChargeCodesOnPatternMatchOverrideTable()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(AccChargeCodeSchema.AC_Code, "FRT");
			filter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

			AccChargeCode fRT = Factory.LoadTop1<AccChargeCode>(filter);

			OrgPatternMatchOverride orgOverride = Factory.New<OrgPatternMatchOverride>();
			orgOverride.OO_Relationship = Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
			orgOverride.OO_ForeignCode = "mapped_chargecode";
			orgOverride.OO_LocalCode = fRT.AC_Code;
			orgOverride.OO_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			NotificationBuffer buffer = new NotificationBuffer(null);
			TestStringToBusinessObjectFieldConverterForCurrentCompany converter = new TestStringToBusinessObjectFieldConverterForCurrentCompany();

			IZType chargeCodeMatch = converter.ConvertRawStringToZTypeValue(typeof(ZGuid), ForeignKeyType.ChargeCodeNK, Factory, "mapped_chargecode", buffer);
			AssertEquals("Pattern match override table for charge codes", fRT.PK, chargeCodeMatch);
		}

		#endregion

		public void TestMatchingCountry()
		{
			OrgPatternMatchOverride orgOverride = Factory.New<OrgPatternMatchOverride>();
			orgOverride.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Country;
			orgOverride.OO_ForeignCode = "india";
			orgOverride.OO_LocalGuid = Core.Constants.CountryGuids.SouthAfrica;
			orgOverride.OO_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			Factory.Save();

			NotificationBuffer buffer = new NotificationBuffer(null);
			TestStringToBusinessObjectFieldConverterForCurrentCompany converter = new TestStringToBusinessObjectFieldConverterForCurrentCompany();
			ZGuid countryGUIDMatch = converter.GetPKFromNKGivenFKType(Factory, "india", ForeignKeyType.CountryNK, buffer);
			AssertEquals(Core.Constants.CountryGuids.SouthAfrica, countryGUIDMatch);

			IZType countryMatch = converter.ConvertRawStringToZTypeValue(typeof(ZString), ForeignKeyType.CountryNK, Factory, "india", buffer);
			AssertEquals(Core.Constants.CountryCodes.SouthAfrica, (ZString)countryMatch);

			countryMatch = converter.ConvertRawStringToZTypeValue(typeof(ZGuid), ForeignKeyType.CountryNK, Factory, "india", buffer);
			AssertEquals(Core.Constants.CountryGuids.SouthAfrica, (ZGuid)countryMatch);
		}

		#region Matching Pack Type Code

		public void TestMatchingPackTypeCode()
		{
			NotificationBuffer buffer = new NotificationBuffer(null);
			TestStringToBusinessObjectFieldConverterForCurrentCompany converter = new TestStringToBusinessObjectFieldConverterForCurrentCompany();

			IZType packCodeMatch = converter.ConvertRawStringToZTypeValue(typeof(ZGuid), ForeignKeyType.PackTypeCodeNK, Factory, "PL", buffer);
			AssertEquals("Pattern match override table for charge codes", "PL", packCodeMatch);

			OrgPatternMatchOverride orgOverride = Factory.New<OrgPatternMatchOverride>();
			orgOverride.OO_Relationship = Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.PackageType;
			orgOverride.OO_ForeignCode = "PL";
			orgOverride.OO_LocalCode = "PLT";
			orgOverride.OO_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			packCodeMatch = converter.ConvertRawStringToZTypeValue(typeof(ZGuid), ForeignKeyType.PackTypeCodeNK, Factory, "PL", buffer);
			AssertEquals("Pattern match override table for charge codes", "PLT", packCodeMatch);
		}

		#endregion

		#region Matching INCOTerms Code

		public void TestMatchingINCOTermsCode()
		{
			NotificationBuffer buffer = new NotificationBuffer(null);
			TestStringToBusinessObjectFieldConverterForCurrentCompany converter = new TestStringToBusinessObjectFieldConverterForCurrentCompany();

			IZType packCodeMatch = converter.ConvertRawStringToZTypeValue(typeof(ZGuid), ForeignKeyType.IncoTermNK, Factory, "PP", buffer);
			AssertEquals("Pattern match override table for charge codes", "PP", packCodeMatch);

			OrgPatternMatchOverride orgOverride = Factory.New<OrgPatternMatchOverride>();
			orgOverride.OO_Relationship = Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.IncoTerm;
			orgOverride.OO_ForeignCode = "PP";
			orgOverride.OO_LocalCode = "PPD";
			orgOverride.OO_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			packCodeMatch = converter.ConvertRawStringToZTypeValue(typeof(ZGuid), ForeignKeyType.IncoTermNK, Factory, "PP", buffer);
			AssertEquals("Pattern match override table for charge codes", "PPD", packCodeMatch);
		}

		#endregion

		#region Matching Currency Code

		public void TestMatchingCurrencyCode()
		{
			NotificationBuffer buffer = new NotificationBuffer(null);
			TestStringToBusinessObjectFieldConverterForCurrentCompany converter = new TestStringToBusinessObjectFieldConverterForCurrentCompany();
			ZGuid southAfricanCurrencyPK = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.SouthAfrica).PK;

			IZType currencyCodeMatch = converter.ConvertRawStringToZTypeValue(typeof(ZString), ForeignKeyType.CurrencyNK, Factory, "ZA", buffer);
			AssertEquals("Pattern match override table for charge codes", "ZA", currencyCodeMatch);

			currencyCodeMatch = converter.ConvertRawStringToZTypeValue(typeof(ZGuid), ForeignKeyType.CurrencyNK, Factory, "ZA", buffer);
			AssertEquals("Pattern match override table for charge codes", ZGuid.Empty, currencyCodeMatch);

			OrgPatternMatchOverride orgOverride = Factory.New<OrgPatternMatchOverride>();
			orgOverride.OO_Relationship = Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.Currency;
			orgOverride.OO_ForeignCode = "ZA";
			orgOverride.OO_LocalGuid = southAfricanCurrencyPK;
			orgOverride.OO_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			currencyCodeMatch = converter.ConvertRawStringToZTypeValue(typeof(ZString), ForeignKeyType.CurrencyNK, Factory, "ZA", buffer);
			AssertEquals("Pattern match override table for charge codes", "ZAR", currencyCodeMatch);

			currencyCodeMatch = converter.ConvertRawStringToZTypeValue(typeof(ZGuid), ForeignKeyType.CurrencyNK, Factory, "ZA", buffer);
			AssertEquals("Pattern match override table for charge codes", southAfricanCurrencyPK, currencyCodeMatch);
		}

		#endregion

		#region Matching Events Code

		public void TestMatchingEventsCode()
		{
			NotificationBuffer buffer = new NotificationBuffer(null);
			TestStringToBusinessObjectFieldConverterForCurrentCompany converter = new TestStringToBusinessObjectFieldConverterForCurrentCompany();

			IZType eventCodeMatch = converter.ConvertRawStringToZTypeValue(typeof(ZGuid), ForeignKeyType.EventCodeNK, Factory, "PP", buffer);
			AssertEquals("Pattern match override table for event codes", "PP", eventCodeMatch);

			OrgPatternMatchOverride orgOverride = Factory.New<OrgPatternMatchOverride>();
			orgOverride.OO_Relationship = Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.EventCode;
			orgOverride.OO_ForeignCode = "PP";
			orgOverride.OO_LocalCode = "PPD";
			orgOverride.OO_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			eventCodeMatch = converter.ConvertRawStringToZTypeValue(typeof(ZGuid), ForeignKeyType.EventCodeNK, Factory, "PP", buffer);
			AssertEquals("Pattern match override table for event types", "PPD", eventCodeMatch);
		}

		#endregion

		#region Matching Warehouse

		public void TestMatchingWarehouse()
		{
			var filter = new ZQuery();
			filter.AddToFilter(WhsWarehouseSchema.WW_WarehouseName, "AA");

			BusinessObject warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Warehouse.Integration.IWhsWarehouse)));
			PropertyInfo nameInfo = ObjectFactory.GetType<Enterprise.Warehouse.Integration.IWhsWarehouse>().GetProperty("WW_WarehouseName", BindingFlags.Public | BindingFlags.Instance);
			nameInfo.SetValue(warehouse, new ZString("Warehouse"), null);

			OrgPatternMatchOverride matchOverride = Factory.New<OrgPatternMatchOverride>();
			matchOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Warehouse;
			matchOverride.OO_ForeignCode = "AA";
			matchOverride.OO_LocalGuid = warehouse.PK;
			matchOverride.OO_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			var buffer = new NotificationBuffer(null);
			TestStringToBusinessObjectFieldConverterForCurrentCompany converter = new TestStringToBusinessObjectFieldConverterForCurrentCompany();

			IZType chargeCodeMatch = converter.ConvertRawStringToZTypeValue(typeof(ZGuid), ForeignKeyType.WarehouseNK, Factory, "AA", buffer);
			AssertEquals("Pattern match override table for Warehouses", warehouse.PK, chargeCodeMatch);
		}

		#endregion

		#region Matching Service Level Code

		public void TestMatchingServiceLevelCode()
		{
			RefServiceLevel serviceLevel = Factory.New<RefServiceLevel>();
			serviceLevel.RS_Code = "SSL";
			Factory.Save();

			OrgPatternMatchOverride orgOverride = Factory.New<OrgPatternMatchOverride>();
			orgOverride.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.ServiceLevel;
			orgOverride.OO_ForeignCode = "SL";
			orgOverride.OO_LocalGuid = serviceLevel.PK;
			orgOverride.OO_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			Factory.Save();

			NotificationBuffer buffer = new NotificationBuffer(null);
			TestStringToBusinessObjectFieldConverterForCurrentCompany converter = new TestStringToBusinessObjectFieldConverterForCurrentCompany();
			ZGuid serviceLevelGUIDMatch = converter.GetPKFromNKGivenFKType(Factory, "SL", ForeignKeyType.RefServiceLevelNK, buffer);
			AssertEquals(serviceLevel.PK, serviceLevelGUIDMatch);

			IZType serviceLevelMatch = converter.ConvertRawStringToZTypeValue(typeof(ZString), ForeignKeyType.RefServiceLevelNK, Factory, "SL", buffer);
			AssertEquals(serviceLevel.RS_Code, (ZString)serviceLevelMatch);

			serviceLevelMatch = converter.ConvertRawStringToZTypeValue(typeof(ZGuid), ForeignKeyType.RefServiceLevelNK, Factory, "SL", buffer);
			AssertEquals(serviceLevel.PK, (ZGuid)serviceLevelMatch);
		}

		#endregion

		#region Matching Zone Code

		public void TestMatchingZoneCode()
		{
			var zone = Factory.NewWithValidTestData<RefZoneHeader>();
			zone.FZ_Code = "BBBB";
			Factory.Save();

			var orgOverride = Factory.New<OrgPatternMatchOverride>();
			orgOverride.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.IntZone;
			orgOverride.OO_ForeignCode = "AAAA";
			orgOverride.OO_LocalGuid = zone.PK;
			orgOverride.OO_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			Factory.Save();

			var buffer = new NotificationBuffer(null);
			var converter = new TestStringToBusinessObjectFieldConverterForCurrentCompany();
			ZGuid zoneGuidMatch = converter.GetPKFromNKGivenFKType(Factory, "AAAA", ForeignKeyType.IntZoneNK, buffer);
			AssertEquals(zone.PK, zoneGuidMatch);

			IZType zoneMatch = converter.ConvertRawStringToZTypeValue(typeof(ZString), ForeignKeyType.IntZoneNK, Factory, "AAAA", buffer);
			AssertEquals(zone.FZ_Code, (ZString)zoneMatch);

			zoneMatch = converter.ConvertRawStringToZTypeValue(typeof(ZGuid), ForeignKeyType.IntZoneNK, Factory, "AAAA", buffer);
			AssertEquals(zone.PK, (ZGuid)zoneMatch);
		}

		#endregion

		public void TestGetPKFromContainerType()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			RefContainer refContainer1 = factory.New<RefContainer>();
			RefContainer refContainer2 = factory.New<RefContainer>();

			refContainer1.RC_ISOType = "123";
			refContainer2.RC_ISOType = "123";

			OrgPatternMatchOverride containerOverride = factory.New<OrgPatternMatchOverride>();
			containerOverride.OO_Relationship = Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.ContainerType;
			containerOverride.OO_ForeignCode = "mapped_containerType";
			containerOverride.OO_LocalGuid = refContainer1.PK;
			containerOverride.OO_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			NotificationBuffer notification = new NotificationBuffer();

			StringToBusinessObjectFieldConverter converter = new TestStringToBusinessObjectFieldConverterForCurrentCompany();
			ZGuid refContainerMatch = converter.GetPKFromNKGivenFKType(factory, "mapped_containerType", ForeignKeyType.ContainerCodeNK, notification);
			Assert("Notification has no erros", !notification.HasErrors);
			AssertEquals("RefContainer Matches", refContainer1.PK, refContainerMatch);

			notification.Clear();
			AssertNull("Precondition: Refcontainer of code '20AA' doesn't exist", factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20AA"));
			refContainerMatch = converter.GetPKFromNKGivenFKType(factory, "20AA", ForeignKeyType.ContainerCodeNK, notification);
			AssertEquals(ZGuid.Empty, refContainerMatch);
			AssertEquals("Notification has warnings", true, notification.HasWarnings);
			var expectedWarningMessage = "Invalid container type specified - 20AA";
			AssertContains(String.Format("Notification should have warnings - {0}", expectedWarningMessage), expectedWarningMessage, notification.AsString);
		}

		public void TestGetPKFromCurrencyCode()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			RefCurrency refCurr1 = factory.New<RefCurrency>();
			RefCurrency refCurr2 = factory.New<RefCurrency>();

			refCurr1.RX_Code = "123";
			refCurr2.RX_Code = "456";

			OrgPatternMatchOverride containerOverride = factory.New<OrgPatternMatchOverride>();
			containerOverride.OO_Relationship = Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.Currency;
			containerOverride.OO_ForeignCode = "mapped_currencyCode";
			containerOverride.OO_LocalGuid = refCurr1.PK;
			containerOverride.OO_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			NotificationBuffer notification = new NotificationBuffer();

			StringToBusinessObjectFieldConverter converter = new TestStringToBusinessObjectFieldConverterForCurrentCompany();
			ZGuid refContainerMatch = converter.GetPKFromNKGivenFKType(factory, "mapped_currencyCode", ForeignKeyType.CurrencyNK, notification);
			Assert("Notification has no erros", !notification.HasErrors);
			AssertEquals("RefContainer Matches", refCurr1.PK, refContainerMatch);
		}

		public void TestIsFKGuidColumn()
		{
			var converter = StringToBusinessObjectFieldConverter.InstanceForCurrentCompany;

			AssertEquals("IsFKGuidColumn [AB]?", false, converter.IsFKGuidColumn("AB"));
			AssertEquals("IsFKGuidColumn [A_B]?", false, converter.IsFKGuidColumn("A_B"));
			AssertEquals("IsFKGuidColumn [AB_]?", false, converter.IsFKGuidColumn("AB_"));
			AssertEquals("IsFKGuidColumn [AB_C]?", false, converter.IsFKGuidColumn("AB_C"));
			AssertEquals("IsFKGuidColumn [AB_CD_]?", false, converter.IsFKGuidColumn("AB_CD_"));
			AssertEquals("IsFKGuidColumn [AB_CD_NKSomething]?", false, converter.IsFKGuidColumn("AB_CD_NKSomething"));
			AssertEquals("IsFKGuidColumn [AB_CD]?", true, converter.IsFKGuidColumn("AB_CD"));
			AssertEquals("IsFKGuidColumn [AB_CD_Something]?", true, converter.IsFKGuidColumn("AB_CD_Something"));

			AssertEquals("IsFKGuidColumn [A_BCD]?", false, converter.IsFKGuidColumn("A_BCD"));
			AssertEquals("IsFKGuidColumn [AB_CDE_]?", false, converter.IsFKGuidColumn("AB_CDE_"));
			AssertEquals("IsFKGuidColumn [AB_CDE_NKSomething]?", false, converter.IsFKGuidColumn("AB_CDE_NKSomething"));
			AssertEquals("IsFKGuidColumn [AB_CDE]?", true, converter.IsFKGuidColumn("AB_CDE"));
			AssertEquals("IsFKGuidColumn [AB_CDE_Something]?", true, converter.IsFKGuidColumn("AB_CDE_Something"));

			AssertEquals("IsFKGuidColumn [ABC_D]?", false, converter.IsFKGuidColumn("ABC_D"));
			AssertEquals("IsFKGuidColumn [ABC_DE_]?", false, converter.IsFKGuidColumn("ABC_DE_"));
			AssertEquals("IsFKGuidColumn [ABC_DE_NKSomething]?", false, converter.IsFKGuidColumn("ABC_DE_NKSomething"));
			AssertEquals("IsFKGuidColumn [ABC_DE]?", true, converter.IsFKGuidColumn("ABC_DE"));
			AssertEquals("IsFKGuidColumn [ABC_DE_Something]?", true, converter.IsFKGuidColumn("ABC_DE_Something"));

			AssertEquals("IsFKGuidColumn [ABC_DEFG]?", false, converter.IsFKGuidColumn("ABC_DEFG"));
			AssertEquals("IsFKGuidColumn [ABC_DEF_]?", false, converter.IsFKGuidColumn("ABC_DEF_"));
			AssertEquals("IsFKGuidColumn [ABC_DEF_NKSomething]?", false, converter.IsFKGuidColumn("ABC_DEF_NKSomething"));
			AssertEquals("IsFKGuidColumn [ABC_DEF]?", true, converter.IsFKGuidColumn("ABC_DEF"));
			AssertEquals("IsFKGuidColumn [ABC_DEF_Something]?", true, converter.IsFKGuidColumn("ABC_DEF_Something"));
		}
	}
}
