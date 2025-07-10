using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.DataTransfer.Integration
{
	public interface IValueObjectExport
	{
		bool CanExport(IValueObject valueObject);
		void Export(BusinessObject businessObject, IValueObject valueObject, IValueObjectExportContext context);
	}
}
