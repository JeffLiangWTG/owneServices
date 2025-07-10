using System.Collections.Immutable;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public static class SadCustomsStatusInformationProvider
{
	public static ImmutableArray<CustomsStatusOrder> ImportCustomsStatusWithInformationOrderCollection => ImmutableArray.Create(
			new CustomsStatusOrder(ZString.Empty, 0),
			new CustomsStatusOrder(ITEntryStatusList.Codes.Registered, 1),
			new CustomsStatusOrder(ITEntryStatusList.Codes.NbRejected, 2),
			new CustomsStatusOrder(ITEntryStatusList.Codes.UnderControl, 3),
			new CustomsStatusOrder(ITEntryStatusList.Codes.ImportCleared, 5));

	public static ImmutableArray<CustomsStatusOrder> ExportCustomsStatusWithInformationOrderCollection => ImmutableArray.Create(
			new CustomsStatusOrder(ZString.Empty, 0),
			new CustomsStatusOrder(ITEntryStatusList.Codes.Registered, 1),
			new CustomsStatusOrder(ITEntryStatusList.Codes.NbRejected, 2),
			new CustomsStatusOrder(ITEntryStatusList.Codes.UnderControl, 3),
			new CustomsStatusOrder(ITEntryStatusList.Codes.ExportCleared, 4),
			new CustomsStatusOrder(ITEntryStatusList.Codes.Exit, 5),
			new CustomsStatusOrder(ITEntryStatusList.Codes.Arrival, 6));

	public static int CompareImportCustomsStatusOrder(ZString statusA, ZString statusB) => CompareCustomsStatusOrder(ImportCustomsStatusWithInformationOrderCollection, statusA, statusB);
	public static int CompareExportCustomsStatusOrder(ZString statusA, ZString statusB) => CompareCustomsStatusOrder(ExportCustomsStatusWithInformationOrderCollection, statusA, statusB);

	public static int CompareCustomsStatusOrder(ImmutableArray<CustomsStatusOrder> customsStatusWithInformationOrderCollection, ZString statusA, ZString statusB)
	{
		var statusAInformationOrder = customsStatusWithInformationOrderCollection.FirstOrDefault(x => x.EntryStatusCode == statusA)?.Order ?? int.MinValue;
		var statusBInformationOrder = customsStatusWithInformationOrderCollection.FirstOrDefault(x => x.EntryStatusCode == statusB)?.Order ?? int.MinValue;
		if (statusAInformationOrder == statusBInformationOrder)
		{
			return 0;
		}
		else if (statusBInformationOrder > statusAInformationOrder)
		{
			return 1;
		}
		else
		{
			return -1;
		}
	}
}
