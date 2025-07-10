using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class AddInfoJobDeclarationLookupsTest : TestCaseWithFactory
	{
		public void TestAgreedPlaceCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: grouping);

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "IncoTerm Key");
			var commonCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "1", "111", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			var includeCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "0", "000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var lookups = declaration.AddInfoLookups;
			CombineAssertions(() =>
			{
				AssertEquals("Latvia should contain CusCodeList items of type INKEY from LV and not EU (EUN)", "0", ((CodeDescriptionPairList)lookups.AgreedPlaceCodeList).CodesAsString);
				AssertSame("Factory Cache is setup", Factory.GetCachedValue("AgreedPlaceCodeList_LV", () => new CodeDescriptionPairList()), lookups.AgreedPlaceCodeList);
			});
		}

		public void TestAgreedPlaceCodeList_Country()
		{
			ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true);
			declaration.ZG_AgreedPlaceCode = "BE";
			AssertType<RefCountryCollection>(declaration.AddInfoLookups.AgreedPlaceCodeList);
		}

		public void TestAgreedPlaceCodeList_UNLOCODE()
		{
			ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true);
			declaration.ZG_AgreedPlaceCode = "BEA";
			AssertType<RefUNLOCOCollection>(declaration.AddInfoLookups.AgreedPlaceCodeList);
		}

		public void TestMethodOfPaymentListWithoutFallback()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: grouping);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Method Of Payment");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "A", "DESC", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "B", "DESC", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			Factory.Save();

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var lookups = declaration.AddInfoLookups;
			CombineAssertions(() =>
			{
				var list = lookups.MethodOfPaymentList;
				AssertEquals("Only Country specific list", "B", list.CodesAsString);
				AssertSame("Factory Cache is setup", Factory.GetCachedValue("MethodOfPaymentList_LV", () => new CodeDescriptionPairList()), lookups.MethodOfPaymentList);
			});
		}

		public void TestMethodOfPaymentListWithFallback()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: grouping);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Method Of Payment");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "A", "DESC", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			Factory.Save();

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("A", declaration.AddInfoLookups.MethodOfPaymentList.CodesAsString);
		}

		public void TestBorderTransportMeansList_Import()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			var lookups = declaration.AddInfoLookups;
			CombineAssertions(() =>
			{
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(ZString.Empty, ZString.Empty);
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.Sea, "02, 06");
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.Rail, "03, 06");
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.Road, "01, 05, 06");
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.Air, "04, 06");
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.Mail, "06, 07");
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.FixedTransportInstallations, "06, 07");
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.InlandWaterwayTransport, "02, 06");
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.OwnPropulsion, "06, 07");
				AssertSame("Testing cache", lookups.BorderTransportMeansList, lookups.BorderTransportMeansList);
			});
		}

		public void TestBorderTransportMeansList_Export()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			var lookups = declaration.AddInfoLookups;
			CombineAssertions(() =>
			{
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(ZString.Empty, ZString.Empty);
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.Sea, "10, 11");
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.Rail, "21");
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.Road, "30");
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.Air, "40, 41");
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.Mail, "10, 11, 21, 30, 40, 41, 80, 81");
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.FixedTransportInstallations, "10, 11, 21, 30, 40, 41, 80, 81");
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.InlandWaterwayTransport, "80, 81");
				AssertBorderMeansOfTransportListIsCorrectForDecTransport(TransportTypeList.Codes.OwnPropulsion, "10, 11, 21, 30, 40, 41, 80, 81");
				AssertSame("Testing cache", lookups.BorderTransportMeansList, lookups.BorderTransportMeansList);
			});
		}

		public void TestBorderTransportMeansListDefaults_Export()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			var lookups = declaration.AddInfoLookups;
			CombineAssertions(() =>
			{
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				AssertEquals("Mode = Sea", "10", lookups.BorderTransportMeansList.DefaultCode);
				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				AssertEquals("Mode = Air", "40", lookups.BorderTransportMeansList.DefaultCode);
				declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
				AssertEquals("Mode = Rail", "21", lookups.BorderTransportMeansList.DefaultCode);
				declaration.JE_TransportMode = TransportTypeList.Codes.Road;
				AssertEquals("Mode = Road", "30", lookups.BorderTransportMeansList.DefaultCode);
				declaration.JE_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
				AssertEquals("Mode = IWT", "80", lookups.BorderTransportMeansList.DefaultCode);
				declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
				AssertNull("Mode = Fixed", lookups.BorderTransportMeansList.DefaultCode);
			});
		}

		public void TestInlandTransportCodeDescriptionPairListBuilder_Import()
		{
			declaration.JE_MessageType = "IMP";
			var lookupsForTest = new AddInfoJobDeclarationLookupsForTest(declaration);

			CombineAssertions("When is UCC6", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					AssertType<InlandTransportCodeDescriptionPairListBuilder>("Builder Type", lookupsForTest.GetInlandTransportCodeDescriptionPairListBuilderExposed());
					AssertEquals("Full List CodesAsString", "10, 11, 20, 30, 40, 41, 80, 81", declaration.AddInfoLookups.InlandTransportCodeList.CodesAsString);
				}
			});

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				AssertNull("When is not UCC6, Builder", lookupsForTest.GetInlandTransportCodeDescriptionPairListBuilderExposed());
			}
		}

		public void TestInlandTransportCodeDescriptionPairListBuilder_Export()
		{
			declaration.JE_MessageType = "EXP";
			var lookupsForTest = new AddInfoJobDeclarationLookupsForTest(declaration);

			CombineAssertions("When is UCC6", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					AssertType<InlandTransportCodeDescriptionPairListBuilder>("Builder Type", lookupsForTest.GetInlandTransportCodeDescriptionPairListBuilderExposed());
					AssertEquals("Full List CodesAsString", "10, 11, 20, 21, 30, 31, 40, 41, 80, 81", declaration.AddInfoLookups.InlandTransportCodeList.CodesAsString);
				}
			});

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				AssertNull("When is not UCC6, Builder", lookupsForTest.GetInlandTransportCodeDescriptionPairListBuilderExposed());
			}
		}

		public void TestInlandTransportCodeList()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				AssertType<MeansOfTransportList>("When is not UCC6, InlandTransportCodeList", declaration.AddInfoLookups.InlandTransportCodeList);
			}
		}

		void AssertBorderMeansOfTransportListIsCorrectForDecTransport(ZString decTransport, ZString expectedBorderMeansOfTransportCodes)
		{
			declaration.JE_TransportMode = decTransport;
			AssertEquals(decTransport, expectedBorderMeansOfTransportCodes, declaration.AddInfoLookups.BorderTransportMeansList.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclarationForTest>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
		}

		JobDeclarationForTest declaration;

		class AddInfoJobDeclarationLookupsForTest : Business.Declaration.JobDeclarationLookups
		{
			public AddInfoJobDeclarationLookupsForTest(JobDeclaration parent) : base(parent)
			{
			}

			public InlandTransportCodeDescriptionPairListBuilder GetInlandTransportCodeDescriptionPairListBuilderExposed()
				=> GetInlandTransportCodeDescriptionPairListBuilder();
		}

		sealed class JobDeclarationForTest : JobDeclaration
		{
			public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public EUCommonConstants.TransportModeSource TransportMeansDependencyForTest;

			protected override EUCommonConstants.TransportModeSource TransportMeansDependencyCore => TransportMeansDependencyForTest;
		}
	}
}
