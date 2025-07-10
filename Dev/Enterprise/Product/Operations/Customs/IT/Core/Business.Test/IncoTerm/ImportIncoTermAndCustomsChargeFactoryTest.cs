using System;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ImportIncoTermAndCustomsChargeFactoryTest : IncoTermAndCustomsChargeFactoryTest
{
	protected override Type ExpectedIncoTermAndChargeFactoryType => typeof(ImportIncoTermAndCustomsChargeFactory);

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
	protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + TestConstants.ProjectRelativePath + @"IncoTerm\Testing\ImportIncoTermAndCustomsChargeConfiguration.csv";

	protected override string GetCountryContext() => Core.Constants.CountryCodes.Italy + Common.EU.EUJobMessageTypeList.Codes.Import;
}
