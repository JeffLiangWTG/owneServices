using System.Threading;
using System.Threading.Tasks;
using CargoWise.FeatureControl.Abstractions;
using Moq;

namespace Enterprise.Accounting.ElectronicMessaging.Germany.Testing
{
	class EInvoicingTestHelperForGermany
	{
		internal IFeatureControlManager GetFeatureControlManagerMock(bool isB2bEnabled, bool isB2GinXTEnabled)
		{
			var featureDataParameter = @"
			{
				""DE"": {
					""Features"": [PLACEHOLDER PLACEHOLDER_B2G]
				}
			}"
			.Replace("PLACEHOLDER_B2G", isB2GinXTEnabled ? ((isB2bEnabled ? "," : "") + "\"B2GinXT\"") : "")
			.Replace("PLACEHOLDER", isB2bEnabled ? "\"B2B\"" : "");

			var featureDataMock = new Mock<IFeatureData>();
			featureDataMock.SetupGet(x => x.Parameter).Returns(featureDataParameter);

			var featureControlManagerMock = new Mock<IFeatureControlManager>();
			featureControlManagerMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingEInvoicingConfiguration, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));
			return featureControlManagerMock.Object;
		}
	}
}
