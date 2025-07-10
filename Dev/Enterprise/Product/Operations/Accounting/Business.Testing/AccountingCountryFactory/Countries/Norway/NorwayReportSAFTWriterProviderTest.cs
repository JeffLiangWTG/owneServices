using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class NorwayReportSAFTWriterProviderTest : TestCaseWithFactory
	{
		public void TestReportSAFTWriterIs1_10_AsDefault()
		{
			var result = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Core.Constants.CountryCodes.Norway) as IInstanceProvider<IReportSAFTWriter>).Get();

			AssertNotNull(result);
			AssertEquals("SAFT version is 1.1 by default since 'ACCSAFT13' feature flag is disabled by default", SAFTVersion.SAFT1_10, result.GetSAFTVersion);
			AssertEquals(SharedConstants.Languages.Norwegian, result.GetLocalLanguage);
		}

		public void TestReportSAFTWriterIs1_30_WhenFeatureFlagIsEnabled()
		{
			var mockFeatureData = new Mock<IFeatureData>();
			mockFeatureManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingSAFT13Report, CancellationToken.None)).Returns(Task.FromResult(mockFeatureData.Object));
			var result = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Core.Constants.CountryCodes.Norway) as IInstanceProvider<IReportSAFTWriter>).Get();

			AssertNotNull(result);
			AssertEquals("SAFT version is 1.3 when 'ACCSAFT13' feature flag is enabled", SAFTVersion.SAFT1_30, result.GetSAFTVersion);
			AssertEquals(SharedConstants.Languages.Norwegian, result.GetLocalLanguage);
		}

		protected override void SetUp()
		{
			base.SetUp();

			mockFeatureManager = new Mock<IFeatureControlManager>();
			ObjectFactory.Substitute(mockFeatureManager.Object);
		}

		protected override void TearDown()
		{
			base.TearDown();
			ObjectFactory.DisposeSubstitutions();
		}

		Mock<IFeatureControlManager> mockFeatureManager;
	}
}
