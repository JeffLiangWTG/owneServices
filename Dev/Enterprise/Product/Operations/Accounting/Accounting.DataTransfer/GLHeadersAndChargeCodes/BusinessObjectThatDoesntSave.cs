using CargoWise.EntityFramework;

namespace Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes
{
	public class BusinessObjectThatDoesntSave : NonPersistentBusinessObject, IObsoleteValidation
	{
		public BusinessObjectThatDoesntSave(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
