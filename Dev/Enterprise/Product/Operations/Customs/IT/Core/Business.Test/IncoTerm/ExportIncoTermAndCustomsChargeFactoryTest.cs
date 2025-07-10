using System;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ExportIncoTermAndCustomsChargeFactoryTest : IncoTermAndCustomsChargeFactoryTest
{
	protected override Type ExpectedIncoTermAndChargeFactoryType => typeof(ExportIncoTermAndCustomsChargeFactory);

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
	protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + TestConstants.ProjectRelativePath + @"IncoTerm\Testing\ExportIncoTermAndCustomsChargeConfiguration.csv";

	protected override string GetCountryContext() => Core.Constants.CountryCodes.Italy;
}
