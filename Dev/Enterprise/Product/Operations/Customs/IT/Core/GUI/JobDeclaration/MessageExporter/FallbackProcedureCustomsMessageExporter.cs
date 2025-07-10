using CargoWise.Types;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.GUI;

public class FallbackProcedureCustomsMessageExporter : CustomsMessageExporter
{
	protected override ZString GetFileName(ITEDIMessage message) => fallbackProcedureDefaultExportFilename;

	protected override ZString GetFileContent(ITEDIMessage message) => message.EM_MessageText;

	const string fallbackProcedureDefaultExportFilename = "pratiche.001";
}
