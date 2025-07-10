using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class NorwayReportSAFTWriterProvider : IReportSAFTWriter
	{
		SAFTVersion IReportSAFTWriter.GetSAFTVersion => IsSAFTv130FeatureEnabled ? SAFTVersion.SAFT1_30 : SAFTVersion.SAFT1_10;

		ZString IReportSAFTWriter.GetLocalLanguage => SharedConstants.Languages.Norwegian;

		bool IsSAFTv130FeatureEnabled => ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(LicenceFeatureCodeList.Codes.AccountingSAFT13Report) != null;
	}
}
