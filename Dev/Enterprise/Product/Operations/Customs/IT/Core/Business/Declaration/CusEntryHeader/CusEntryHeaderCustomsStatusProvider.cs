using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public class CusEntryHeaderCustomsStatusProvider : ISadCustomsStatusProvider
{
	public CusEntryHeaderCustomsStatusProvider(CusEntryHeader entryHeader)
	{
		isImport = Argument.NotNull(entryHeader, nameof(entryHeader)).IsImport;
	}

	readonly ZBool isImport;

	ZString ISadCustomsStatusProvider.AwaitingMessageStatus => ITMessageStatusList.Codes.AwaitingOriginal;

	ZString ISadCustomsStatusProvider.AcknowledgedMessageStatus => ITMessageStatusList.Codes.AcknowledgedOriginal;

	ZString ISadCustomsStatusProvider.ClearedMessageStatus => ITMessageStatusList.Codes.ClearOriginal;

	ZString ISadCustomsStatusProvider.ErrorMessageStatus => ITMessageStatusList.Codes.ErrorOriginal;

	ZString ISadCustomsStatusProvider.RegisteredCustomsStatus => ITEntryStatusList.Codes.Registered;

	ZString ISadCustomsStatusProvider.UnderControlCustomsStatus => ITEntryStatusList.Codes.UnderControl;

	ZString ISadCustomsStatusProvider.ClearedCustomsStatus => GetClearedCustomsStatus();

	ZString ISadCustomsStatusProvider.NbRejectedCustomsStatus => ITEntryStatusList.Codes.NbRejected;

	ZString ISadCustomsStatusProvider.ArrivalCustomsStatus => ITEntryStatusList.Codes.Arrival;

	ImmutableArray<CustomsStatusOrder> ISadCustomsStatusProvider.StatusWithInformationOrderCollection => GetStatusWithInformationOrderCollection();

	#region Implementation

	ZString GetClearedCustomsStatus()
	{
		return isImport
			? ITEntryStatusList.Codes.ImportCleared
			: ITEntryStatusList.Codes.ExportCleared;
	}

	ImmutableArray<CustomsStatusOrder> GetStatusWithInformationOrderCollection()
	{
		return isImport
			? SadCustomsStatusInformationProvider.ImportCustomsStatusWithInformationOrderCollection
			: SadCustomsStatusInformationProvider.ExportCustomsStatusWithInformationOrderCollection;
	}

	#endregion
}
