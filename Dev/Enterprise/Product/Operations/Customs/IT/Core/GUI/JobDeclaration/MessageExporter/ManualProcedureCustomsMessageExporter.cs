using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.GUI;

public class ManualProcedureCustomsMessageExporter : CustomsMessageExporter
{
	protected override ZString GetFileName(ITEDIMessage message)
	{
		var interchange = Argument.NotNull(message.Interchange, nameof(message.Interchange));
		var headerTagContent = GetHeaderTagContent(interchange);
		return headerTagContent.SubstringSafe(16, 12);
	}

	protected override ZString GetFileContent(ITEDIMessage message)
	{
		var interchange = Argument.NotNull(message.Interchange, nameof(message.Interchange));
		var headerTagContent = GetHeaderTagContent(interchange);
		return new ZStringBuilder()
			.AppendIfNotEmpty(headerTagContent)
			.AppendIfNotEmpty(message.EM_MessageText)
			.ToStringWithNewLineBetweenAppends();
	}

	ZString GetHeaderTagContent(EDIInterchange interchange) => MessageProcessorHelper.RetrieveValueOfXmlNode(interchange.EI_HeaderText, (NoResString)"Header");
}
