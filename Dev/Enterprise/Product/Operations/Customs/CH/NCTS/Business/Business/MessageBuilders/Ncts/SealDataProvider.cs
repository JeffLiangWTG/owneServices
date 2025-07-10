using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class SealDataProvider : ISeal
{
	public static IEnumerable<SealDataProvider> NewCollection(NctsDepartureHeaderContainer headerContainer)
	{
		return headerContainer == null || (headerContainer.Seal1.IsEmpty && headerContainer.Seal2.IsEmpty) ? null : GetSeals(headerContainer);
	}

	static IEnumerable<SealDataProvider> GetSeals(NctsDepartureHeaderContainer headerContainer)
	{
		var sequenceNumber = 0;

		if (!headerContainer.Seal1.IsEmpty)
		{
			yield return new SealDataProvider(++sequenceNumber, headerContainer.Seal1);
		}
		if (!headerContainer.Seal2.IsEmpty)
		{
			yield return new SealDataProvider(++sequenceNumber, headerContainer.Seal2);
		}

		foreach (EU.NCTS.Business.CusSeal additionalSeal in headerContainer.AdditionalSeals)
		{
			yield return new SealDataProvider(++sequenceNumber, additionalSeal.BK_SealNumber);
		}
	}

	protected SealDataProvider(int sequenceNumber, ZString identifier)
	{
		SequenceNumber = sequenceNumber;
		this.identifier = identifier;
	}
	readonly ZString identifier;

	public int SequenceNumber { get; }

	public virtual string Identifier => identifier.ReturnNullIfEmpty();

	public virtual string UnloadingRemarkText => null;
}
