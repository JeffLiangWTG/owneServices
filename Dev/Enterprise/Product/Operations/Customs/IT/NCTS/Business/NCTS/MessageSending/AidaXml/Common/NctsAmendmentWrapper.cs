using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.Customs.Shared.MessageContracts;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class NctsAmendmentWrapper : INctsAmendment
{
	public NctsAmendmentWrapper(NctsHeaderMessageSendingObject sendingObject)
	{
		this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
	}

	IReadOnlyCollection<HouseConsignmentToBeDeletedTypeDataProviderAbstractClass> INctsAmendment.HouseConsignmentToBeDeleted => houseConsignmentToBeDeleted ??= GetHouseConsignmentToBeDeleted();
	IReadOnlyCollection<HouseConsignmentToBeDeletedTypeDataProviderAbstractClass> houseConsignmentToBeDeleted;

	string IAmendment.Mrn => sendingObject.NctsHeader.MovementReferenceEntryNumber?.CE_EntryNum;

	string IAmendment.Reason => sendingObject.Reason;

	string IAmendment.LegislativeReference => sendingObject.LegislativeReference;

	IReadOnlyCollection<HouseConsignmentToBeDeletedTypeDataProviderAbstractClass> GetHouseConsignmentToBeDeleted()
	{
		return sendingObject.NctsHeader
			.GetGoodsItems()
			.Cast<NctsDepartureCargoDesc>()
			.Where(x => x.IsCustomsStatusDeletionRequested || x.Bill.IsCustomsStatusDeletionRequested)
			.GroupBy(x => x.Bill.SequenceNumber)
			.Select(x =>
			{
				var articleNumberToBeDeleted = x.First().Bill.IsCustomsStatusDeletionRequested ? null : x.Select(g => (int)g.BY_LineNo.ToZInt());
				return new HouseConsignmentToBeDeletedTypeDataProvider(x.Key, articleNumberToBeDeleted);
			}).ToCollection();
	}

	readonly NctsHeaderMessageSendingObject sendingObject;
}
