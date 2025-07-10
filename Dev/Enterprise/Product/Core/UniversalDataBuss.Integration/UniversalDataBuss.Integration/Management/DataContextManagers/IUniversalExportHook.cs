using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IUniversalExportHook
	{
		void OnUniversalXmlExport(BusinessObject businessObject, IEDICommunicationsMode mode);
		void OnUniversalXmlExportValidationFailure(BusinessObject businessObject, IEDICommunicationsMode mode);
	}
}
