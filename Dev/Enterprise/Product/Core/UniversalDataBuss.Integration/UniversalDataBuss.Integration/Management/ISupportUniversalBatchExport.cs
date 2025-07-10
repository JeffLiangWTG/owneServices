using CargoWise.EntityFramework;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface ISupportUniversalBatchExport : IUniversalXmlWorkflowProcessor
	{
		void AddAnotherExportedBusinessObject(BusinessObject anotherExportedBO);
		void AddAnotherTopLevelDataObjectWriterAndXmlWriter(ITopLevelDataObjectWriter topLevelDataObjectWriter, IXmlWriter xmlWriter);
	}
}
