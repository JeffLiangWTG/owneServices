using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class DeclarationSubmissionProviderTest : TestCaseWithFactory
{
	public void TestSubmissionTypeListForUAECustoms()
	{
		var uAEManifestFeatureControlData = new UAECustomsModuleFeatureControlData() { EnableUAESeaExportManifest = true };
		var featureDataMock = new Mock<IFeatureData>();
		featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out uAEManifestFeatureControlData)).Returns(true);

		var featureControlMock = new Mock<IFeatureControlManager>();
		featureControlMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.UAEDubaiCustomsModule, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

		var company = Factory.New<GlbCompany>();
		var countryCode = Core.Constants.CountryCodes.UnitedArabEmirates;
		company.GC_Code = $"U{countryCode}";
		company.GC_RN_NKCountryCode = countryCode;
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertSubmissionTypeList("No Feature Control", "ITF");
			using (ObjectFactory.Substitute(featureControlMock.Object))
			{
				AssertSubmissionTypeList("UAEDubaiCustomsModule Feature Control", "BTH, BIT, ITF, BLT");
			}
		});

		void AssertSubmissionTypeList(string description, string expectedCodes)
		{
			var fallbackLevel = new FallbackLevel(company, null, null);
			var customsInterface = new LocalCountryCustomsInterface()
			{
				CurrentFallbackLevel = fallbackLevel
			};
			var actualCodes = customsInterface.SubmissionTypeList.CodesAsString;
			AssertEquals(description, expectedCodes, actualCodes);
		}
	}
}
