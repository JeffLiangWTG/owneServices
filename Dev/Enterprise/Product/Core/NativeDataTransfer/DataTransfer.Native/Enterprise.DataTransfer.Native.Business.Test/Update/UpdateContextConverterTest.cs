using System.Linq;
using Enterprise.DataTransfer.Native.Business.Requests;
using Enterprise.DataTransfer.Native.Business.Update.AccChargeCodes;
using Enterprise.DataTransfer.Native.Business.Update.CodeMappings;
using Enterprise.DataTransfer.Native.Business.Update.Container;
using Enterprise.DataTransfer.Native.Business.Update.Declaration;
using Enterprise.DataTransfer.Native.Business.Update.EntityInfos;
using Enterprise.DataTransfer.Native.Business.Update.ExchangeRates;
using Enterprise.DataTransfer.Native.Business.Update.NewsAnnouncement;
using Enterprise.DataTransfer.Native.Business.Update.OrgAddressAdditionalInfos;
using Enterprise.DataTransfer.Native.Business.Update.OrgCodeGenerations;
using Enterprise.DataTransfer.Native.Business.Update.OrgCountryData;
using Enterprise.DataTransfer.Native.Business.Update.OrgMatchings;
using Enterprise.DataTransfer.Native.Business.Update.OrgOpportunity;
using Enterprise.DataTransfer.Native.Business.Update.OrgPhoneNumber;
using Enterprise.DataTransfer.Native.Business.Update.OrgSalesCall;
using Enterprise.DataTransfer.Native.Business.Update.OrgSupplierPartMatchings;
using Enterprise.DataTransfer.Native.Business.Update.ProductEntityMatching;
using Enterprise.DataTransfer.Native.Business.Update.ProductMatchings;
using Enterprise.DataTransfer.Native.Business.Update.Rates;
using Enterprise.DataTransfer.Native.Business.Update.Staff;
using Enterprise.DataTransfer.Native.Business.Update.WorkflowTemplates;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.CodeMappings;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update
{
	public class UpdateContextConverterTest : TransactionedTestCase
	{
		public void TestConvert_When_UpdateSetting_Is_Null()
		{
			source = null;
			var result = converter.Convert(source);

			var defaultOrgCode = EDICodeMapper.GetDefaultOrgCode();

			Assert(result.InterceptorSettings.Any(s => s is CodeMappingSetting));
			Assert(result.InterceptorSettings.Any(s => s is OrgCodeGenerationSetting));

			// Code Mapping Setting
			var codeMappingSetting = (CodeMappingSetting)result.InterceptorSettings.Single(s => s is CodeMappingSetting);
			AssertNotNull(codeMappingSetting.Interceptor);
			Assert(codeMappingSetting.Interceptor is CodeMappingInterceptor);

			AssertEquals(true, codeMappingSetting.Enable);
			AssertEquals(defaultOrgCode, codeMappingSetting.OwnerCode);

			// Org Code Generation Setting
			var orgCodeGenerationSetting = (OrgCodeGenerationSetting)result.InterceptorSettings.Single(s => s is OrgCodeGenerationSetting);
			AssertNotNull(orgCodeGenerationSetting.Interceptor);
			Assert(orgCodeGenerationSetting.Interceptor is OrgCodeGenerationInterceptor);

			AssertEquals(true, orgCodeGenerationSetting.Enable);
			AssertEquals(defaultOrgCode, orgCodeGenerationSetting.OwnerCode);
		}

		public void TestConvert_When_UpdateSetting_Not_Null()
		{
			source = new HeaderData_Universal();
			var result = converter.Convert(source);

			Assert(result.InterceptorSettings.Any(s => s is CodeMappingSetting));
			Assert(result.InterceptorSettings.Any(s => s is OrgCodeGenerationSetting));

			var codeMappingSetting = (CodeMappingSetting)result.InterceptorSettings.Single(s => s is CodeMappingSetting);
			AssertEquals(source.EnableCodeMapping, codeMappingSetting.Enable);
			AssertEquals(source.OwnerCode, codeMappingSetting.OwnerCode);
			AssertNotNull(codeMappingSetting.Interceptor);
			Assert(codeMappingSetting.Interceptor is CodeMappingInterceptor);

			var orgCodeGenerationSetting = (OrgCodeGenerationSetting)result.InterceptorSettings.Single(s => s is OrgCodeGenerationSetting);
			AssertNotNull(orgCodeGenerationSetting.Interceptor);
			Assert(orgCodeGenerationSetting.Interceptor is OrgCodeGenerationInterceptor);

			AssertEquals(source.EnableCodeMapping, orgCodeGenerationSetting.Enable);
			AssertEquals(source.OwnerCode, orgCodeGenerationSetting.OwnerCode);
		}

		public void TestConvert_InterceptorSequence()
		{
			var result = converter.Convert(null);
			var settings = result.InterceptorSettings.ToList();
			AssertEquals("All interceptors are checked", 27, settings.Count);
			AssertType<OrderWorkflowTemplateSetting>(settings[0]);
			AssertType<OrgSupplierPartMatchingSetting>(settings[1]);
			AssertType<ProductMatchingSetting>(settings[2]);
			AssertType<AccChargeCodeSetting>(settings[3]);
			AssertType<RateSetting>(settings[4]);
			AssertType<ProductEntityMatchingSetting>(settings[5]);
			AssertType<OrgMatchingSetting>(settings[6]);
			AssertType<CodeMappingSetting>(settings[7]);
			AssertType<OrgCodeGenerationSetting>(settings[8]);
			AssertType<EntityInfoSetting>(settings[9]);
			AssertType<StaffSetting>(settings[10]);
			AssertType<OrgCountryDataSetting>(settings[11]);
			AssertType<OrgPhoneNumberSetting>(settings[12]);
			AssertType<OrgAddressMatching.OrgAddressMatchingSetting>(settings[13]);
			AssertType<LanguageCodes.LanguageCodesSetting>(settings[14]);
			AssertType<OrgContactSetting>(settings[15]);
			AssertType<AddInfoOrgMatching.AddInfoOrgMatchingSetting>(settings[16]);
			AssertType<UNDGDataItemSetting>(settings[17]);
			AssertType<OrgSalesCallSetting>(settings[18]);
			AssertType<OrgOpportunitySetting>(settings[19]);
			AssertType<NewsAnnouncementSetting>(settings[20]);
			AssertType<WorkflowTemplateSetting>(settings[21]);
			AssertType<DeclarationInterceptorSetting>(settings[22]);
			AssertType<OrgAddressAdditionalInfoSetting>(settings[23]);
			AssertType<DangerousGoodsCountryReferenceMappingSetting>(settings[24]);
			AssertType<ContainerSetting>(settings[25]);
			AssertType<ExchangeRatesSetting>(settings[26]);
		}

		protected override void SetUp()
		{
			sessionServices = new AncillaryImportServices();
			converter = new UpdateContextConverter(sessionServices, new FactoryProvider());
		}
		AncillaryImportServices sessionServices;
		HeaderData source;
		UpdateContextConverter converter;
	}
}
