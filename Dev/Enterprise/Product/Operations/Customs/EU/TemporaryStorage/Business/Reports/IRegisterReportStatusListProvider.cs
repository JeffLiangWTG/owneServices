using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.TemporaryStorage.Business
{
	public interface IRegisterReportStatusListProvider
	{
		ReadOnlyCodeDescriptionPairList ReportStatusList { get; }
	}
}
