using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IDataExportCSVFileNameProvider
	{
		ZString FileNameSuffix { get; }
	}
}
