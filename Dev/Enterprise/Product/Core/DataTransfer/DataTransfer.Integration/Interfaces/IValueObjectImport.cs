using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.DataTransfer.Integration
{
	public interface IValueObjectImport
	{
		bool CanImport(IValueObject valueObject);
		void Import(BusinessObject businessObject, IValueObject valueObject, IValueObjectImportContext importContext);
	}
}
