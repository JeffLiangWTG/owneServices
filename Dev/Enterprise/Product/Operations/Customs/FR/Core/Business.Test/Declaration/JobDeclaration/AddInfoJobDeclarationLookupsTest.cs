using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class AddInfoJobDeclarationLookupsTest : EU.Business.Declaration.Testing.AddInfoJobDeclarationLookupsTest
	{
		public void TestImportInlandTransportDefaultCode()
		{
			using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
				AssertEquals("When JE_TransportMode is IWT, the default value of JE_TransportMeans is 80", "80", declaration.JE_TransportMeans);
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				AssertEquals("When JE_TransportMode is SEA, the default value of JE_TransportMeans is 10", "10", declaration.JE_TransportMeans);
			}
		}

		public void TestValuationBypassCodeList()
		{
			CombineAssertions(() =>
			{
				var valuationBypassCodeList = lookups.ValuationBypassCodeList;
				AssertEquals("Values", "A, B, C, D, E, F, G, H, I, J, K, L, M", valuationBypassCodeList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<ValuationBypassCodeList>(), valuationBypassCodeList);
			});
		}

		public void TestDeferTypeList()
		{
			CombineAssertions(() =>
			{
				var vatProcedureList = lookups.DeferTypeList;
				AssertEquals("Values", "2, L, S", vatProcedureList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<VATProcedureList>(), vatProcedureList);
			});
		}

		public void TestVATCanaList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "VAT National Additional Codes");
			helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1035", "Article 1695 du CGI - Autoliquidation de la TVA à l''importation", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, GuaranteeTypeList.Codes.ALT);
			helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1001", "Je m''engage à respecter les conditions de l''article 275 du CGI - TVA seule", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, GuaranteeTypeList.Codes.AI2);
			helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1002", "Je m''engage à respecter les conditions de l''article 275 du CGI - taxes fiscales seules", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, GuaranteeTypeList.Codes.AI2);
			helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1003", "Je m''engage à respecter les conditions de l''article 275 du CGI - TVA et taxes fiscales", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, GuaranteeTypeList.Codes.AI2);
			helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1011", "Article 275 du CGI sans dispense de visa - TVA seule", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, GuaranteeTypeList.Codes.AI2);
			helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1012", "Article 275 du CGI sans dispense de visa - taxes fiscales seules", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, GuaranteeTypeList.Codes.AI2);
			helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1013", "Article 275 du CGI sans dispense de visa - TVA et taxes fiscales", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, GuaranteeTypeList.Codes.AI2);
			Factory.Save();

			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			AssertContainsExactElementsInAnyOrder(new VatCanaForAI2List(Factory).GetAllCodes(), lookups.VatCanaList.GetAllCodes());

			declaration.ZG_VATDeferType = VATProcedureList.Codes.L;
			AssertContainsExactElementsInAnyOrder(new ZString[] { "1035" }, lookups.VatCanaList.GetAllCodes());

			declaration.ZG_VATDeferType = VATProcedureList.Codes.S;
			AssertEquals(string.Empty, declaration.Lookups.VatCanaList.CodesAsString);
		}

		public void TestAgreedPlaceCodeList_Country()
		{
			using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.ZG_AgreedPlaceCode = "FR";
				AssertType<RefCountryCollection>(declaration.AddInfoLookups.AgreedPlaceCodeList);
			}
		}

		public void TestAgreedPlaceCodeList_UNLOCODE()
		{
			using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				AssertType<RefUNLOCOCollection>(declaration.AddInfoLookups.AgreedPlaceCodeList);
			}
		}

		public void TestAgreedPlaceCodeList_RefCusCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: euDataGrouping);

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "IncoTerm Key");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "1", "111", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "2", "222", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();

			var list = declaration.AddInfoLookups.AgreedPlaceCodeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "1, 2", ((CodeDescriptionPairList)list).CodesAsString);
				AssertSame("Should be cached", list, declaration.AddInfoLookups.AgreedPlaceCodeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			lookups = new JobDeclarationLookups(declaration);
		}
		JobDeclaration declaration;
		JobDeclarationLookups lookups;
	}
}
