namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IAirOutturnReportHeaderInformationProvider
	{
		IAirOutturnReportHeaderInformation GetHeader(CusUnderbond underbond);
	}
}
