using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.EndpointManagement.Module.Testing
{
	[TestedType(typeof(EdiTrustedMessagingConfigFilterBusinessObject))]
	public class EdiTrustedMessagingConfigFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new EdiTrustedMessagingConfigFilterBusinessObject();
		}

		public void TestFilters()
		{
			EDIDataRegistry.Instance.MyAccountTrustedServices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionBoolCollection() { "CCC" });

			var config1 = Factory.New<EdiTrustedMessagingConfig>();
			config1.ETM_Product = "CW1";
			config1.ETM_CertificateType = CertificateTypeList.Codes.CentralSystemCertificate;

			var config2 = Factory.New<EdiTrustedMessagingConfig>();
			config2.ETM_Product = "CW1";
			config2.ETM_CertificateType = CertificateTypeList.Codes.PreDeploymentCertificate;

			var config3 = Factory.New<EdiTrustedMessagingConfig>();
			config3.ETM_Product = "EN1";
			config3.ETM_CertificateType = CertificateTypeList.Codes.TrustedSystemCertificate;

			var config4 = Factory.New<EdiTrustedMessagingConfig>();
			config4.ETM_Product = "ABU";
			config4.ETM_CertificateType = CertificateTypeList.Codes.CentralSystemCertificate;

			var config5 = Factory.New<EdiTrustedMessagingConfig>();
			config5.ETM_Product = "CCC";
			config5.ETM_CertificateType = CertificateTypeList.Codes.TrustedSystemCertificate;

			var config6 = Factory.New<EdiTrustedMessagingConfig>();
			config6.ETM_Product = "CCC";
			config6.ETM_CertificateType = CertificateTypeList.Codes.CentralSystemCertificate;

			Factory.Save();

			var collection = new EdiTrustedMessagingConfigGlobalCollection(Factory);
			AssertEquals(5, collection.Count);
			AssertCollectionContains(config1, collection);
			AssertCollectionContains(config2, collection);
			AssertCollectionContains(config4, collection);
			AssertCollectionContains(config5, collection);
			AssertCollectionContains(config6, collection);

			var filter = new EdiTrustedMessagingConfigFilterBusinessObject();
			var productFilter = (ModuleTextFilter)filter["Product"];
			productFilter.Property = "ABU";
			productFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			productFilter.IsActive = true;
			collection = new EdiTrustedMessagingConfigGlobalCollection(Factory, filter.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(config4, collection);

			productFilter.Property = "CW1";
			var certificateTypeFilter = (ModuleTextFilter)filter["Certificate Type"];
			certificateTypeFilter.Property = CertificateTypeList.Codes.CentralSystemCertificate;
			certificateTypeFilter.IsActive = true;
			collection = new EdiTrustedMessagingConfigGlobalCollection(Factory, filter.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(config1, collection);

			productFilter.Property = "CCC";
			certificateTypeFilter.IsActive = false;
			collection = new EdiTrustedMessagingConfigGlobalCollection(Factory, filter.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(config5, collection);
			AssertCollectionContains(config6, collection);
		}
	}
}
