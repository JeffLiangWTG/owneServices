using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using Moq;
using static Enterprise.Registry.Business.RatingFeatureHelper.CarrierConnect;
using static Enterprise.Registry.Business.RatingFeatureHelper.Urs;

namespace Enterprise.Registry.Business.Testing;

public class RatingFeatureHelperTest : TestCaseWithFactory
{
	#region Carrier Connect

	public void TestC3FeatureEnabled_IsTrue_WhenEnabledInFeatureControl()
	{
		var cWCarrierConnectFeatureRule = new CWCarrierConnectFeatureRule { Enabled = true };
		var featureDataMock = new Mock<IFeatureData>();
		var featureControlMock = new Mock<IFeatureControlManager>();
		featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out cWCarrierConnectFeatureRule)).Returns(true);
		featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.CargoWiseCarrierConnect, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

		ObjectFactory.Substitute(featureControlMock.Object);

		AssertEquals("Should be true when feature code is enabled.", true, RatingFeatureHelper.CarrierConnect.IsFeatureEnabled());
	}

	public void TestC3FeatureEnabled_IsFalse_WhenDisabledInFeatureControl()
	{
		var cWCarrierConnectFeatureRule = new CWCarrierConnectFeatureRule { Enabled = false };
		var featureDataMock = new Mock<IFeatureData>();
		var featureControlMock = new Mock<IFeatureControlManager>();
		featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out cWCarrierConnectFeatureRule)).Returns(true);
		featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.CargoWiseCarrierConnect, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

		ObjectFactory.Substitute(featureControlMock.Object);

		AssertEquals("Should be false when feature code is disabled.", false, RatingFeatureHelper.CarrierConnect.IsFeatureEnabled());
	}

	public void TestC3FeatureAndRegistryEnabled_IsFalse_WhenDisabledInFeatureControlAndEnabledInRegistry()
	{
		var cWCarrierConnectFeatureRule = new CWCarrierConnectFeatureRule { Enabled = false };
		var featureDataMock = new Mock<IFeatureData>();
		var featureControlMock = new Mock<IFeatureControlManager>();
		featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out cWCarrierConnectFeatureRule)).Returns(true);
		featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.CargoWiseCarrierConnect, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

		ObjectFactory.Substitute(featureControlMock.Object);

		using (RatingDataRegistry.Instance.CargoWiseCarrierConnectForJobAutorating.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		{
			AssertEquals("Should be false when feature control is disabled.", false, RatingFeatureHelper.CarrierConnect.IsEnabledForRateSelection());
		}
	}

	public void TestC3FeatureAndRegistryEnabled_IsFalse_WhenDisabledInFeatureControlAndDisabledInRegistry()
	{
		var cWCarrierConnectFeatureRule = new CWCarrierConnectFeatureRule { Enabled = false };
		var featureDataMock = new Mock<IFeatureData>();
		var featureControlMock = new Mock<IFeatureControlManager>();
		featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out cWCarrierConnectFeatureRule)).Returns(true);
		featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.CargoWiseCarrierConnect, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

		ObjectFactory.Substitute(featureControlMock.Object);

		using (RatingDataRegistry.Instance.CargoWiseCarrierConnectForJobAutorating.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
		{
			AssertEquals("Should be false when disabled in feature control and registry.", false, RatingFeatureHelper.CarrierConnect.IsEnabledForRateSelection());
		}
	}

	public void TestC3FeatureAndRegistryEnabled_IsFalse_WhenEnabledInFeatureControlAndDisabledInRegistry()
	{
		var cWCarrierConnectFeatureRule = new CWCarrierConnectFeatureRule { Enabled = true };
		var featureDataMock = new Mock<IFeatureData>();
		var featureControlMock = new Mock<IFeatureControlManager>();
		featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out cWCarrierConnectFeatureRule)).Returns(true);
		featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.CargoWiseCarrierConnect, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

		ObjectFactory.Substitute(featureControlMock.Object);

		using (RatingDataRegistry.Instance.CargoWiseCarrierConnectForJobAutorating.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
		{
			AssertEquals("Should be false when disabled in registry.", false, RatingFeatureHelper.CarrierConnect.IsEnabledForRateSelection());
		}
	}

	public void TestC3FeatureAndRegistryEnabled_IsTrue_WhenEnabledInFeatureControlAndEnabledInRegistry()
	{
		var cWCarrierConnectFeatureRule = new CWCarrierConnectFeatureRule { Enabled = true };
		var featureDataMock = new Mock<IFeatureData>();
		var featureControlMock = new Mock<IFeatureControlManager>();
		featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out cWCarrierConnectFeatureRule)).Returns(true);
		featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.CargoWiseCarrierConnect, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

		ObjectFactory.Substitute(featureControlMock.Object);

		using (RatingDataRegistry.Instance.CargoWiseCarrierConnectForJobAutorating.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		{
			AssertEquals("Should be true when enabled in feature control and registry.", true, RatingFeatureHelper.CarrierConnect.IsEnabledForRateSelection());
		}
	}

	#endregion

	#region URS

	public void TestUrsFeatureHelperForC3_DisabledByDefault()
	{
		AssertEquals("URS should be not enabled by default.", false, RatingFeatureHelper.Urs.IsEnabled);
	}

	public void TestUrsFeatureHelperForC3_WhenEnabledInFCM_IsEnabled()
	{
		var ursRule = new UrsFeatureRule { Enabled = true, Url = "https://fcm.cargowise.com" };
		var featureDataMock = new Mock<IFeatureData>();
		var featureControlMock = new Mock<IFeatureControlManager>();
		featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out ursRule)).Returns(true);
		featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.UniversalRatesService, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

		ObjectFactory.Substitute(featureControlMock.Object);

		AssertEquals("URS should be enabled.", true, RatingFeatureHelper.Urs.IsEnabled);
		AssertEquals("URS URL should be from FCM", "https://fcm.cargowise.com", RatingFeatureHelper.Urs.Url);
	}

	public void TestUrsFeatureHelperForC3_WhenEnabledInRegistry_IsEnabled()
	{
		using (RatingDataRegistry.Instance.UniversalRatesServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://registry.cargowise.com"))
		using (RatingDataRegistry.Instance.EnableUrsIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		{
			AssertEquals("URS should be enabled.", true, RatingFeatureHelper.Urs.IsEnabled);
			AssertEquals("URS URL should be from Registry", "https://registry.cargowise.com", RatingFeatureHelper.Urs.Url);
		}
	}

	public void TestUrsFeatureHelperForC3_WhenEnabledInBoth_FcmIsPreferred()
	{
		using (RatingDataRegistry.Instance.UniversalRatesServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://registry.cargowise.com"))
		using (RatingDataRegistry.Instance.EnableUrsIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		{
			var ursRule = new UrsFeatureRule { Enabled = true, Url = "https://fcm.cargowise.com" };
			var featureDataMock = new Mock<IFeatureData>();
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out ursRule)).Returns(true);
			featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.UniversalRatesService, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

			ObjectFactory.Substitute(featureControlMock.Object);

			AssertEquals("URS should be enabled.", true, RatingFeatureHelper.Urs.IsEnabled);
			AssertEquals("URS URL should be from FCM", "https://fcm.cargowise.com", RatingFeatureHelper.Urs.Url);
		}
	}

	public void TestUrsFeatureHelperForLegacy_WhenFeatureEnabled_LegacyEnabled_IsEnabled()
	{
		using (RatingDataRegistry.Instance.UniversalRatesServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://registry.cargowise.com"))
		using (RatingDataRegistry.Instance.EnableUrsIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		using (RatingDataRegistry.Instance.UseUrsForLegacyRateSelectorAndMMS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		{
			var ursRule = new UrsFeatureRule { Enabled = true, Url = "https://fcm.cargowise.com" };
			var featureDataMock = new Mock<IFeatureData>();
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out ursRule)).Returns(true);
			featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.UniversalRatesService, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

			ObjectFactory.Substitute(featureControlMock.Object);

			AssertEquals("URS should be enabled.", true, RatingFeatureHelper.Urs.IsEnabledForLegacy);
			AssertEquals("URS URL should be from FCM", "https://fcm.cargowise.com", RatingFeatureHelper.Urs.Url);
		}
	}

	public void TestUrsFeatureHelperForLegacy_WhenFeatureEnabled_LegacyDisabled_IsDisabled()
	{
		using (RatingDataRegistry.Instance.UniversalRatesServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://registry.cargowise.com"))
		using (RatingDataRegistry.Instance.EnableUrsIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		using (RatingDataRegistry.Instance.UseUrsForLegacyRateSelectorAndMMS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
		{
			var ursRule = new UrsFeatureRule { Enabled = true, Url = "https://fcm.cargowise.com" };
			var featureDataMock = new Mock<IFeatureData>();
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out ursRule)).Returns(true);
			featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.UniversalRatesService, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

			ObjectFactory.Substitute(featureControlMock.Object);

			AssertEquals("URS should be enabled.", false, RatingFeatureHelper.Urs.IsEnabledForLegacy);
			AssertEquals("URS URL should be from FCM", "https://fcm.cargowise.com", RatingFeatureHelper.Urs.Url);
		}
	}

	public void TestUrsFeatureHelperForLegacy_WhenFeatureDisabled_LegacyDisabled_IsDisabled()
	{
		using (RatingDataRegistry.Instance.UniversalRatesServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://registry.cargowise.com"))
		using (RatingDataRegistry.Instance.EnableUrsIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
		using (RatingDataRegistry.Instance.UseUrsForLegacyRateSelectorAndMMS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
		{
			var ursRule = new UrsFeatureRule { Enabled = true, Url = "https://fcm.cargowise.com" };
			var featureDataMock = new Mock<IFeatureData>();
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out ursRule)).Returns(true);
			featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.UniversalRatesService, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

			ObjectFactory.Substitute(featureControlMock.Object);

			AssertEquals("URS should be enabled.", false, RatingFeatureHelper.Urs.IsEnabledForLegacy);
		}
	}

	public void TestUrsFeatureHelperForLegacy_WhenFeatureDisabled_LegacyEnabled_IsDisabled()
	{
		using (RatingDataRegistry.Instance.UniversalRatesServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://registry.cargowise.com"))
		using (RatingDataRegistry.Instance.EnableUrsIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
		using (RatingDataRegistry.Instance.UseUrsForLegacyRateSelectorAndMMS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		{
			var ursRule = new UrsFeatureRule { Enabled = false, Url = "https://fcm.cargowise.com" };
			var featureDataMock = new Mock<IFeatureData>();
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out ursRule)).Returns(true);
			featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.UniversalRatesService, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

			ObjectFactory.Substitute(featureControlMock.Object);

			AssertEquals("URS should be enabled.", false, RatingFeatureHelper.Urs.IsEnabledForLegacy);
		}
	}

	#endregion
}
