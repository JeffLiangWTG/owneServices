using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.GB.Business
{
	public class EntryLineConfiguration : EU.Business.EntryLineConfiguration
	{
		protected override ZBool SupportingDocumentsSupportCore(BusinessObject businessObject) => true;

		protected override ZBool MergeJI_RN_NKCountryOfExportCore(JobDeclaration declaration) => true;
	}
}
