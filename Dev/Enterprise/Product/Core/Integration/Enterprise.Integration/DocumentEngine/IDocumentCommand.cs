using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration.DocumentEngine
{
	public interface IDocumentCommand
	{
		ZGuid PK { get; }
		ZString SU_FilterList { get; set; }
		ZBool SU_IsPublished { get; set; }
		ZBool SU_IsSystemDefined { get; set; }
		ZString SU_MenuName { get; set; }
		ZString SU_MenuPath { get; set; }
		ZString SU_BusinessContext { get; set; }
		ZString SU_MenuType { get; set; }
		ZString MenuItemUniqueCode { get; }
		bool IsApplicable { get; }
		bool HasChildMenus { get; }
		string GetDeliveryRestrictionErrorMessage(BusinessObject parentBusinessObject, BusinessObject deliveryBusinessObject);
	}
}
