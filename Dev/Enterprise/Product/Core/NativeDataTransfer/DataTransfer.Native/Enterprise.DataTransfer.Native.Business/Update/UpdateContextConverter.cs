using System;
using Enterprise.DataTransfer.Native.Business.Requests;
using Enterprise.DataTransfer.Native.Business.Update.AccChargeCodes;
using Enterprise.DataTransfer.Native.Business.Update.AddInfoOrgMatching;
using Enterprise.DataTransfer.Native.Business.Update.CodeMappings;
using Enterprise.DataTransfer.Native.Business.Update.Container;
using Enterprise.DataTransfer.Native.Business.Update.Declaration;
using Enterprise.DataTransfer.Native.Business.Update.EntityInfos;
using Enterprise.DataTransfer.Native.Business.Update.ExchangeRates;
using Enterprise.DataTransfer.Native.Business.Update.LanguageCodes;
using Enterprise.DataTransfer.Native.Business.Update.NewsAnnouncement;
using Enterprise.DataTransfer.Native.Business.Update.OrgAddressAdditionalInfos;
using Enterprise.DataTransfer.Native.Business.Update.OrgAddressMatching;
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

namespace Enterprise.DataTransfer.Native.Business.Update
{
	public class UpdateContextConverter : IConverter<HeaderData, UpdateContext>
	{
		public UpdateContextConverter(AncillaryImportServices sessionServices, INativeFactoryProvider factoryProvider)
		{
			this.sessionServices = sessionServices ?? throw new ArgumentNullException(nameof(sessionServices));
			this.factoryProvider = factoryProvider ?? throw new ArgumentNullException(nameof(factoryProvider));
		}
		readonly AncillaryImportServices sessionServices;
		readonly INativeFactoryProvider factoryProvider;

		public UpdateContext Convert(HeaderData source)
		{
			var context = new UpdateContext(sessionServices, factoryProvider);

			var orderWorkflowTemplateSetting = new OrderWorkflowTemplateSetting { Enable = true, Context = context };
			orderWorkflowTemplateSetting.Interceptor = new OrderWorkflowTemplateInterceptor(orderWorkflowTemplateSetting, sessionServices);

			var workflowTemplateSetting = new WorkflowTemplateSetting { Enable = true, Context = context };
			workflowTemplateSetting.Interceptor = new WorkflowTemplateInterceptor(workflowTemplateSetting, sessionServices);

			var productMatchingSetting = new ProductMatchingSetting { Enable = true, Context = context };
			productMatchingSetting.Interceptor = new ProductMatchingInterceptor(productMatchingSetting, sessionServices);

			var orgSupplierPartMatchingSetting = new OrgSupplierPartMatchingSetting { Enable = true, Context = context };
			orgSupplierPartMatchingSetting.Interceptor = new OrgSupplierPartMatchingInterceptor(orgSupplierPartMatchingSetting, sessionServices);

			var accChargeCodeSetting = new AccChargeCodeSetting { Enable = true, Context = context };
			accChargeCodeSetting.Interceptor = new AccChargeCodeInterceptor(accChargeCodeSetting, sessionServices);

			var rateSetting = new RateSetting(source) { Enable = true, Context = context };
			rateSetting.Interceptor = new RateInterceptor(rateSetting, sessionServices);

			var orgMatchingSetting = new OrgMatchingSetting { Enable = true, Context = context };
			orgMatchingSetting.Interceptor = new OrgMatchingInterceptor(orgMatchingSetting, sessionServices);

			var codeMappingSetting = (source == null) ? new CodeMappingSetting(EDICodeMapper.GetDefaultOrgCode()) { Enable = true, Context = context }
													  : new CodeMappingSetting(source.OwnerCode) { Enable = source.EnableCodeMapping, Context = context };
			codeMappingSetting.Interceptor = new CodeMappingInterceptor(codeMappingSetting, sessionServices);

			var orgCodeGenerationSetting = (source == null) ? new OrgCodeGenerationSetting(EDICodeMapper.GetDefaultOrgCode()) { Enable = true, EnableCodeMapping = true, Context = context }
															: new OrgCodeGenerationSetting(source.OwnerCode) { Enable = source.EnableCodeMapping, EnableCodeMapping = source.EnableCodeMapping, Context = context };
			orgCodeGenerationSetting.Interceptor = new OrgCodeGenerationInterceptor(orgCodeGenerationSetting, sessionServices);

			var entityInfoSetting = new EntityInfoSetting { Enable = true, Context = context, EntityInfo = context.EntityInfo };
			entityInfoSetting.Interceptor = new EntityInfoInterceptor(entityInfoSetting, sessionServices);

			var staffSetting = new StaffSetting { Enable = true, Context = context };
			staffSetting.Interceptor = new StaffInterceptor(staffSetting, sessionServices);

			var productEntityMatchingSetting = new ProductEntityMatchingSetting { Enable = true, Context = context };
			productEntityMatchingSetting.Interceptor = new ProductEntityMatchingInterceptor(productEntityMatchingSetting, sessionServices);

			var orgCountryDataMatchingSetting = new OrgCountryDataSetting { Enable = true, Context = context };
			orgCountryDataMatchingSetting.Interceptor = new OrgCountryDataInterceptor(orgCountryDataMatchingSetting, sessionServices);

			var orgPhoneNumberSetting = new OrgPhoneNumberSetting { Enable = true, Context = context };
			orgPhoneNumberSetting.Interceptor = new OrgPhoneNumberInterceptor(orgPhoneNumberSetting, sessionServices);

			var orgAddressMatchingSetting = new OrgAddressMatchingSetting { Enable = true, Context = context };
			orgAddressMatchingSetting.Interceptor = new OrgAddressMatchingInterceptor(orgAddressMatchingSetting, sessionServices);

			var languageCodeSetting = new LanguageCodesSetting { Enable = true, Context = context };
			languageCodeSetting.Interceptor = new LanguageCodesInterceptor(languageCodeSetting, sessionServices);

			var orgContactSetting = new OrgContactSetting { Enable = true, Context = context };
			orgContactSetting.Interceptor = new OrgContactInterceptor(orgContactSetting, sessionServices);

			var addInfoOrgMatchingSetting = new AddInfoOrgMatchingSetting { Enable = true, Context = context };
			addInfoOrgMatchingSetting.Interceptor = new AddInfoOrgMatchingInterceptor(addInfoOrgMatchingSetting, sessionServices);

			var undgDataItemSetting = new UNDGDataItemSetting { Enable = true, Context = context };
			undgDataItemSetting.Interceptor = new UNDGDataItemInterceptor(undgDataItemSetting, sessionServices);

			var orgSalesCallSetting = new OrgSalesCallSetting { Enable = true, Context = context };
			orgSalesCallSetting.Interceptor = new OrgSalesCallInterceptor(orgSalesCallSetting, sessionServices);

			var orgOpportunitySetting = new OrgOpportunitySetting { Enable = true, Context = context };
			orgOpportunitySetting.Interceptor = new OrgOpportunityInterceptor(orgOpportunitySetting, sessionServices);

			var newsAnnouncementSetting = new NewsAnnouncementSetting { Enable = true, Context = context };
			newsAnnouncementSetting.Interceptor = new NewsAnnouncementInterceptor(newsAnnouncementSetting, sessionServices);

			var declarationSetting = new DeclarationInterceptorSetting(source) { Enable = true, Context = context };
			declarationSetting.Interceptor = new DeclarationInterceptor(declarationSetting, sessionServices);

			var orgAddressAdditionalInfoSetting = new OrgAddressAdditionalInfoSetting { Enable = true, Context = context };
			orgAddressAdditionalInfoSetting.Interceptor = new OrgAddressAdditionalInfoInterceptor(orgAddressAdditionalInfoSetting, sessionServices);

			var dangerousGoodsCountryReferenceMappingSetting = new DangerousGoodsCountryReferenceMappingSetting { Enable = true, Context = context };
			dangerousGoodsCountryReferenceMappingSetting.Interceptor = new DangerousGoodsCountryReferenceMappingInterceptor(dangerousGoodsCountryReferenceMappingSetting, sessionServices);

			var containerSetting = new ContainerSetting { Enable = true, Context = context };
			containerSetting.Interceptor = new ContainerInterceptor(containerSetting, sessionServices);

			var exchangeRatesSetting = new ExchangeRatesSetting { Enable = true, Context = context };
			exchangeRatesSetting.Interceptor = new ExchangeRatesInterceptor(exchangeRatesSetting, sessionServices);

			// On syntax interceptor should not have dependency
			// However, there is dependency between these interceptor
			// i.e.:
			// Code Mapping and OrgMatching
			// Product Matching and OrgSupplierPart Matching

			context.InterceptorSettings.Add(orderWorkflowTemplateSetting);
			context.InterceptorSettings.Add(orgSupplierPartMatchingSetting);
			context.InterceptorSettings.Add(productMatchingSetting);
			context.InterceptorSettings.Add(accChargeCodeSetting);
			context.InterceptorSettings.Add(rateSetting);
			context.InterceptorSettings.Add(productEntityMatchingSetting);
			context.InterceptorSettings.Add(orgMatchingSetting);
			context.InterceptorSettings.Add(codeMappingSetting);
			context.InterceptorSettings.Add(orgCodeGenerationSetting);
			context.InterceptorSettings.Add(entityInfoSetting);
			context.InterceptorSettings.Add(staffSetting);
			context.InterceptorSettings.Add(orgCountryDataMatchingSetting);
			context.InterceptorSettings.Add(orgPhoneNumberSetting);
			context.InterceptorSettings.Add(orgAddressMatchingSetting);
			context.InterceptorSettings.Add(languageCodeSetting);
			context.InterceptorSettings.Add(orgContactSetting);
			context.InterceptorSettings.Add(addInfoOrgMatchingSetting);
			context.InterceptorSettings.Add(undgDataItemSetting);
			context.InterceptorSettings.Add(orgSalesCallSetting);
			context.InterceptorSettings.Add(orgOpportunitySetting);
			context.InterceptorSettings.Add(newsAnnouncementSetting);
			context.InterceptorSettings.Add(workflowTemplateSetting);
			context.InterceptorSettings.Add(declarationSetting);
			context.InterceptorSettings.Add(orgAddressAdditionalInfoSetting);
			context.InterceptorSettings.Add(dangerousGoodsCountryReferenceMappingSetting);
			context.InterceptorSettings.Add(containerSetting);
			context.InterceptorSettings.Add(exchangeRatesSetting);

			context.IsInterceptorSettingsAdded = true;

			return context;
		}
	}
}
