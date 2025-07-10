using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MailManager.Integration
{
	public interface IMailItemTemplateCollection : IBusinessObjectCollection
	{
		ZString GetTemplateBody(ZGuid pk);
		ZString CategoryCodeToFilter { get; set; }
		void FilterByCurrentLoginDetails();
		ZGuid GetPKOfFirstValidTemplateForCurrentCompany();
	}
}
