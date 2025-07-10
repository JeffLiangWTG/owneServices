namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ISeaOutturnReportHeaderInformationProvider
	{
		ISeaOutturnReportHeaderInformation GetHeader(CusUnderbond underbond);
	}
}
