using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IAnnexAESMessageDataProvider : IAESCommonDataProvider
	{
		IAESCommonExportOperationMRN ExportOperation { get; }
		ZString DispatchRequestCode { get; }
		IReadOnlyCollection<IAnnexDocCommon> Documents { get; }
	}
}
