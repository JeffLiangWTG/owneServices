using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class DummyFileNameProvider : IDataExportCSVFileNameProvider
	{
		ZString IDataExportCSVFileNameProvider.FileNameSuffix
		{
			get { return "Dummy :<>\\/|?*Suffix"; }
		}
	}
}
