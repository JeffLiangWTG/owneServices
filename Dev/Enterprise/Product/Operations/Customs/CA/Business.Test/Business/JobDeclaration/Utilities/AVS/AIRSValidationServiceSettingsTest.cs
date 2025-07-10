using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Registry;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class AIRSValidationServiceSettingsTest : TestCaseWithFactory
	{
		public void TestAIRSValidationServiceSettings()
		{
			using (CACustomsDataRegistry.Instance.FrenchLanguageIndicator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (CACustomsDataRegistry.Instance.AIRSValidationKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TEST"))
			using (CACustomsDataRegistry.Instance.AIRSValidationRequestUserName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "UserName"))
			using (CACustomsDataRegistry.Instance.AIRSValidationRequestPassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Password"))
			using (CACustomsDataRegistry.Instance.WebProxyAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://www.testuri.com"))
			{
				var setting = new AIRSValidationServiceSettings(Factory.New<JobDeclaration>());
				Assert("FrenchPreferred", setting.FrenchPreferred);
				AssertEquals("Key", "TEST", setting.Key);
				AssertEquals("UserName", "UserName", setting.UserName);
				AssertEquals("Password", "Password", setting.Password);
				AssertEquals("WebProxyAddress", "http://www.testuri.com", setting.WebProxyUri);
			}
		}

		public void TestUri()
		{
			var setting = new AIRSValidationServiceSettings(Factory.New<JobDeclaration>());
			AssertEquals(CACustomsDataRegistry.Instance.AIRSValidationRequestURL.Value, setting.Uri);
			using (CACustomsDataRegistry.Instance.AIRSValidationRequestURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://avs-svs.inspection.gc.ca/avs/bvs.svc"))
			{
				AssertEquals(CACustomsDataRegistry.Instance.AIRSValidationRequestURL.Value, setting.Uri);
			}
			using (CACustomsDataRegistry.Instance.AIRSValidationRequestURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				AssertEquals("https://avs-svs.inspection.gc.ca/avs/bvs.svc/secure", setting.Uri);
			}
		}

		public void TestWebProxyUri()
		{
			var setting = new AIRSValidationServiceSettings(Factory.New<JobDeclaration>());
			AssertEquals(CACustomsDataRegistry.Instance.WebProxyAddress.Value, string.Empty);
			using (CACustomsDataRegistry.Instance.WebProxyAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://avs-svs.inspection.gc.ca/avs/bvs.svc"))
			{
				AssertEquals(CACustomsDataRegistry.Instance.WebProxyAddress.Value, setting.WebProxyUri);
			}
		}

		public void TestCheckValidationServiceSetting()
		{
			AssertEquals(CACustomsDataRegistry.Instance.AIRSValidationKey.Value, string.Empty);
			var setting = new AIRSValidationServiceSettings(Factory.New<JobDeclaration>());
			AssertEquals(setting.CheckValidationServiceSetting(), @"AIRS Validation Key hasn't been setup. The key is allocated by CFIA. Please set it up in Registry -> Customs -> Country or Region Specific -> Canada -> Import -> Declaration -> CFIA -> AIRS Validation Key");

			CACustomsDataRegistry.Instance.AIRSValidationKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test");
			CACustomsDataRegistry.Instance.AIRSValidationRequestURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Invalid URL");
			setting = new AIRSValidationServiceSettings(Factory.New<JobDeclaration>());
			AssertEquals(setting.CheckValidationServiceSetting(), @"An invalid URL entered in Registry -> Customs -> Country or Region Specific -> Canada -> Import -> Declaration -> CFIA -> AIRS Validation Request URL");

			CACustomsDataRegistry.Instance.AIRSValidationRequestURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://avs-svs.inspection.gc.ca/avs/bvs.svc");
			CACustomsDataRegistry.Instance.WebProxyAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Invalid URL");
			setting = new AIRSValidationServiceSettings(Factory.New<JobDeclaration>());
			AssertEquals(setting.CheckValidationServiceSetting(), @"An invalid URL entered in Registry -> Customs -> Country or Region Specific -> Canada -> Import -> Declaration -> CFIA -> Web Proxy Address");
		}
	}
}
