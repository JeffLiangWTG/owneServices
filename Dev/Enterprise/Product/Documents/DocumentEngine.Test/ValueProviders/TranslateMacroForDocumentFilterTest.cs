using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.ValueReplacers.Testing
{
	sealed class TranslateMacroForDocumentFilterTest : TestCaseWithFactory
	{
		public void TestDocumentFilterShouldEscapeSpecialCharacters()
		{
			var dummy = Factory.New<DummyBusinessObject>();

			var filterMacroTranslator = new TranslateMacroForDocumentFilter(new DataProviderList(BODocDataProvider.Get(dummy)));

			dummy.Z0_NVarChar = @"<A>";
			var filter1 = @"""<Z0_NVarChar>""==""\<A\>""";
			var result1 = filterMacroTranslator.ReplaceMacros(filter1);
			AssertEquals(@"""\<A\>""==""\<A\>""", result1);

			dummy.Z0_NVarChar = @"AA\";
			var filter2 = @"""<Z0_NVarChar>""==""AA\\""";
			var result2 = filterMacroTranslator.ReplaceMacros(filter2);
			AssertEquals(@"""AA\\""==""AA\\""", result2);

			dummy.Z0_NVarChar = @"""A""";
			var filter3 = @"""<Z0_NVarChar>""==""\""A\""""";
			var result3 = filterMacroTranslator.ReplaceMacros(filter3);
			AssertEquals(@"""\""A\""""==""\""A\""""", result3);

			dummy.Z0_Code = "ABC";
			dummy.Z0_NVarChar = @"A<Z0_Code>""='|\&""";
			var filter4 = @"""<Z0_NVarChar>""==""ABCDEFG""";
			var result4 = filterMacroTranslator.ReplaceMacros(filter4);
			AssertEquals(@"""A\<Z0_Code\>\""='|\\&\""""==""ABCDEFG""", result4);

			dummy.Z0_NVarChar = @"<A>";
			dummy.Z0_Description = @"\B\";
			dummy.Z0_Decimal = 10m;
			var filter5 = @"""<If(<Z0_Decimal>&gt;2, ""<Z0_Description>"", ""<Z0_NVarChar>"")>""==""DDDD""";
			var result5 = filterMacroTranslator.ReplaceMacros(filter5);
			AssertEquals(@"""\\B\\""==""DDDD""", result5);
		}

		public void TestGetCurrentCompanyWhenHasNoOrgProxy()
		{
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;
			var filter = new TranslateMacroForDocumentFilter(new DataProviderList(new TestClass()));
			var result = filter.ReplaceMacros("<CurrentCompany.OrgProxy.CustomsCodes[CCS:US].OK_CustomsRegNo>");

			AssertEquals(string.Empty, result);
		}

		public void TestCompanyCountryCodeGetsHardMacroInPlaceOfFieldOnBODocDataProvider()
		{
			AssertEquals("Precondition: GlbCompany.CurrentCompany.CountryCode.Code", "AU", GlbCompany.CurrentCompany.Country.Code);

			var dataSource = new Enterprise.DocumentEngine.Testing.CompanyCountryCodeDocDataProvider(Enterprise.Core.Constants.CountryCodes.Netherlands);
			var filter = new TranslateMacroForDocumentFilter(new DataProviderList(dataSource));

			AssertEquals("filter.ReplaceMacros(\"<CompanyCountryCode>\")", "AU", filter.ReplaceMacros("<CompanyCountryCode>"));
		}

		public void TestGenericMacrosWork()
		{
			string countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			Assert("!string.IsNullOrEmpty(countryCode)", !string.IsNullOrEmpty(countryCode));

			TranslateMacroForDocumentFilter translator = new TranslateMacroForDocumentFilter(new DataProviderList(new TestClass()));
			AssertEquals("translator.ReplaceMacros(\"<CompanyCountryCode>\")", countryCode, translator.ReplaceMacros("<CompanyCountryCode>"));

			AssertEquals("translator.ReplaceMacros(\"<FicticiousMacroNotImplementedAnywhere>\")", "", translator.ReplaceMacros("<FicticiousMacroNotImplementedAnywhere>"));
			AssertEquals("translator.ReplaceMacros(\"<FicticiousTable.NotImplementedAnywhere>\")", "", translator.ReplaceMacros("<FicticiousTable.NotImplementedAnywhere>"));
		}

		public void TestTranslateFromBizoWithFallback()
		{
			DummyBusinessObject dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_VarCharMax = "Laughing";
			dummyBO.Z0_Date = new ZDateTime(2016, 6, 14);
			TranslateMacroForDocumentFilter testTranslator = new TranslateMacroForDocumentFilter(new DataProviderList(new TestClass(), BODocDataProvider.Get(dummyBO)));
			AssertEquals("Hi", testTranslator.ReplaceMacros("<Test>"));
			AssertEquals("Hi, Ali", testTranslator.ReplaceMacros("<Test>, Ali"));
			AssertEquals("Laughing, Ali", testTranslator.ReplaceMacros("<Z0_VarCharMax>, Ali"));
			AssertEquals("Hi, Laughing Ali", testTranslator.ReplaceMacros("<Test>, <Z0_VarCharMax> Ali"));
			AssertEquals("Tuesday", testTranslator.ReplaceMacros("<DateTimeAsString('<Z0_Date>','dddd')>"));
		}

		public void TestTranslateFromBizo()
		{
			TranslateMacroForDocumentFilter testTranslator = new TranslateMacroForDocumentFilter(new DataProviderList(new TestClass()));
			AssertEquals("Hi", testTranslator.ReplaceMacros("<Test>"));
			AssertEquals("Hi, Ali", testTranslator.ReplaceMacros("<Test>, Ali"));
		}

		public void TestTranslateBackSlash()
		{
			TranslateMacroForDocumentFilter testTranslator = new TranslateMacroForDocumentFilter(new DataProviderList(new TestClass()));
			AssertEquals(@"\\", testTranslator.ReplaceMacros("<BackSlash>"));
		}

		public void TestTranslateDoubleQuote()
		{
			TranslateMacroForDocumentFilter testTranslator = new TranslateMacroForDocumentFilter(new DataProviderList(new TestClass()));
			AssertEquals(@"2\""Pipe", testTranslator.ReplaceMacros("<DoubleQuote>"));
		}

		public void TestTranslateFromSystemBizo()
		{
			TranslateMacroForDocumentFilter testTranslator = new TranslateMacroForDocumentFilter(new DataProviderList(new TestClass()));
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, testTranslator.ReplaceMacros("<CurrentCompany.Country.LocalCurrency.RX_Code>"));
		}

		public void TestUseNewDocBuilderFreightDocuments()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderForwardingDocuments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TranslateMacroForDocumentFilter translator = new TranslateMacroForDocumentFilter(new DataProviderList(new TestClass()));
			AssertEquals("Get Value for: <UseDocBuilderFreightDocs>", "Y", translator.ReplaceMacros("<UseDocBuilderFreightDocs>"));

			DocumentsDataRegistry.Instance.UseNewDocBuilderForwardingDocuments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Get Value for: <UseDocBuilderFreightDocs>", "N", translator.ReplaceMacros("<UseDocBuilderFreightDocs>"));
		}

		public void TestUseNewDocBuilderWarehouseDocumentsOnly()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderWarehouseDocumentsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TranslateMacroForDocumentFilter translator = new TranslateMacroForDocumentFilter(new DataProviderList(new TestClass()));
			AssertEquals("Get Value for: <UseDocBuilderWarehouseDocs>", "Y", translator.ReplaceMacros("<UseDocBuilderWarehouseDocsOnly>"));

			DocumentsDataRegistry.Instance.UseNewDocBuilderWarehouseDocumentsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Get Value for: <UseDocBuilderWarehouseDocs>", "N", translator.ReplaceMacros("<UseDocBuilderWarehouseDocsOnly>"));
		}

		public void TestUseNewDocBuilderOrganizationDocumentsOnly()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderOrganizationDocumentsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var translator = new TranslateMacroForDocumentFilter(new DataProviderList(new TestClass()));
			AssertEquals("Get Value for: <UseDocBuilderOrganizationDocs>", "Y", translator.ReplaceMacros("<UseDocBuilderOrganizationDocs>"));

			DocumentsDataRegistry.Instance.UseNewDocBuilderOrganizationDocumentsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Get Value for: <UseDocBuilderOrganizationDocs>", "N", translator.ReplaceMacros("<UseDocBuilderOrganizationDocs>"));
		}

		public void TestUseNewDocBuilderLinerAndAgencyDocumentsOnly()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderLinerAndAgencyDocumentsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var translator = new TranslateMacroForDocumentFilter(new DataProviderList(new TestClass()));
			AssertEquals("Get Value for: <UseDocBuilderLinerAndAgencyDocs>", "Y", translator.ReplaceMacros("<UseDocBuilderLinerAndAgencyDocs>"));

			DocumentsDataRegistry.Instance.UseNewDocBuilderLinerAndAgencyDocumentsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Get Value for: <UseDocBuilderLinerAndAgencyDocs>", "N", translator.ReplaceMacros("<UseDocBuilderLinerAndAgencyDocs>"));
		}

		public void TestUseNewDocBuilderRatingAndQuotationDocuments()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TranslateMacroForDocumentFilter translator = new TranslateMacroForDocumentFilter(new DataProviderList(new TestClass()));
			AssertEquals("Get Value for: <UseDocBuilderRatingAndQuotationDocs>", "Y", translator.ReplaceMacros("<UseDocBuilderRatingAndQuotationDocs>"));

			DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Get Value for: <UseDocBuilderRatingAndQuotationDocs>", "N", translator.ReplaceMacros("<UseDocBuilderRatingAndQuotationDocs>"));
		}

		public void TestUseNewDocBuilderOrderManagerDocuments()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderOrderManagerDocuments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TranslateMacroForDocumentFilter translator = new TranslateMacroForDocumentFilter(new DataProviderList(new TestClass()));
			AssertEquals("Get Value for: <UseDocBuilderOrderDocs>", "Y", translator.ReplaceMacros("<UseDocBuilderOrderDocs>"));

			DocumentsDataRegistry.Instance.UseNewDocBuilderOrderManagerDocuments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Get Value for: <UseDocBuilderOrderDocs>", "N", translator.ReplaceMacros("<UseDocBuilderOrderDocs>"));
		}

		class TestClass : IBODocDataProvider
		{
			public ZString Test
			{
				get { return "Hi"; }
			}

			public ZString BackSlash
			{
				get { return @"\"; }
			}

			public ZString DoubleQuote
			{
				get { return @"2""Pipe"; }
			}

			#region IBODocDataProvider Members

			DocWrapperCopyInfo IBODocDataProvider.AdditionalCopyInfo
			{
				get { return null; }
			}

			CargoWise.EntityFramework.BusinessObject IBODocDataProvider.BusinessObjectToLogAgainst
			{
				get { return null; }
			}

			ZString IBODocDataProvider.GetDocDataValue(ZString docDataIdentifier, ZString formatStringForFallbackValue)
			{
				return ZString.Empty;
			}

			string[] IBODocDataProvider.ImageNamesToRemove
			{
				get { return null; }
			}

			CargoWise.EntityFramework.BusinessObject IBODocDataProvider.ParentBusinessObject
			{
				get { return null; }
			}

			void IBODocDataProvider.SetDocWrapperContext(Dictionary<string, object> constants)
			{
			}

			IZType IBODocDataProvider.GetCustomField(string fieldName, string typeName)
			{
				return ZString.Empty;
			}

			string IBODocDataProvider.GetCustomFieldCodeDescription(string fieldName, string typeName)
			{
				return string.Empty;
			}

			ZDateTime IBODocDataProvider.GetEventLastDateTime(string eventCode)
			{
				return ZDateTime.Empty;
			}

			#endregion
		}
	}
}
