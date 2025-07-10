using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public sealed class DangerousGoodsDataProvider : IDangerousGoods
{
	public static IEnumerable<DangerousGoodsDataProvider> NewCollection(CusEntryLine entryLine) => (FuncsHelper.IsCHNE015V3Active ? NewCollectionV3(entryLine) : NewCollectionV4(entryLine)).EmptyIfNull();

	static IEnumerable<DangerousGoodsDataProvider> NewCollectionV3(CusEntryLine entryLine) => NewCollectionV4(entryLine)?.Take(1);

	static IEnumerable<DangerousGoodsDataProvider> NewCollectionV4(CusEntryLine entryLine)
		=> entryLine?.InvoiceLines.Cast<JobComInvoiceLine>()
			.SelectMany(x => x.UNDGs)
			.Where(x => x.UNDGSubstance != null)
			.Select(x => x.UNDGSubstance.DG_UNNO)
			.Distinct()
			.Select((x, i) => new DangerousGoodsDataProvider(x, i + 1));

	DangerousGoodsDataProvider(ZString unNumber, int sequenceNumber)
	{
		UNNumber = unNumber;
		SequenceNumber = sequenceNumber;
	}

	public int SequenceNumber { get; }

	public string UNNumber { get; }
}
