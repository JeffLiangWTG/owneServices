using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public class EntryLineConfiguration : EU.Business.EntryLineConfiguration
	{
		protected override ZBool SupportingDocumentsSupportCore(BusinessObject businessObject) => true;
	}
}
