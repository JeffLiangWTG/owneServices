using CargoWise.Types;

namespace Enterprise.Customs.IE.Business
{
	public class EntryLineConfiguration : EU.Business.EntryLineConfiguration
	{
		protected override ZBool MergeJI_RN_NKCountryOfExportCore(EU.Business.Declaration.JobDeclaration declaration) => declaration.IsExport;
	}
}
