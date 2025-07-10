using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using Moq;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ExternalRequestFeatureHelperTest : TestCaseWithFactory
	{
		public void TestIsEnabled_IsTrue_WhenEnabledInFeatureControl()
		{
			var featureDataMock = new Mock<IFeatureData>();
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.ExternaRequestFeature, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

			ObjectFactory.Substitute(featureControlMock.Object);

			AssertEquals("Should be true when feature code is enabled.", true, ExternalRequestFeatureHelper.IsExternalRequestEnabled());
		}

		public void TestIsEnabled_IsFalse_WhenDisabledInFeatureControl()
		{
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.ExternaRequestFeature, CancellationToken.None)).Returns(Task.FromResult((IFeatureData)null));

			ObjectFactory.Substitute(featureControlMock.Object);

			AssertEquals("Should be false when feature code is disabled.", false, ExternalRequestFeatureHelper.IsExternalRequestEnabled());
		}

#if DEBUG

		public void TestIsEnabled_WhenOverriddenByEnvironmentVariableToTrue_ToMakeLifeEasierForDevelopers()
		{
			System.Environment.SetEnvironmentVariable("EXTREQ_Enabled", "tRuE");

			var featureDataMock = new Mock<IFeatureData>();
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.ExternaRequestFeature, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

			ObjectFactory.Substitute(featureControlMock.Object);

			AssertEquals("Should be true when environment variable messes with things.", true, ExternalRequestFeatureHelper.IsExternalRequestEnabled());
		}

		public void TestIsEnabled_WhenOverriddenByEnvironmentVariableToFalse_ToMakeLifeEasierForDevelopers()
		{
			System.Environment.SetEnvironmentVariable("EXTREQ_Enabled", "fAlSE");

			var featureDataMock = new Mock<IFeatureData>();
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.ExternaRequestFeature, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

			ObjectFactory.Substitute(featureControlMock.Object);

			AssertEquals("Should be false when environment variable messes with things.", false, ExternalRequestFeatureHelper.IsExternalRequestEnabled());
		}

		protected override void TearDown()
		{
			System.Environment.SetEnvironmentVariable("EXTREQ_Enabled", string.Empty);
		}
#endif
	}
}
