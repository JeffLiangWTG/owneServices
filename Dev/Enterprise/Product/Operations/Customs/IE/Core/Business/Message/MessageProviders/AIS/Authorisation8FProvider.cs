using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.AIS.UCC5;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class Authorisation8FProvider : IAuth8F
	{
		public Authorisation8FProvider(CusEntryHeader entryHeader)
		{
			instruction = Argument.NotNull(entryHeader?.EntryInstruction, nameof(entryHeader));
		}

		public IReadOnlyCollection<IAddressWithName> Parties
			=> partiesCached ??= GetParties().ToArray();
		IReadOnlyCollection<IAddressWithName> partiesCached;

		public IDateTimesPeriodsAndPlaces DateTimesPeriodsAndPlaces
			=> CachedValueHelper.GetValue(ref dateTimesPeriodsAndPlacesCached, () => new DateTimesPeriodsAndPlacesProvider(instruction));
		CachedValue<IDateTimesPeriodsAndPlaces> dateTimesPeriodsAndPlacesCached;

		public IIdentificationOfGoods IdentificationOfGoods
			=> CachedValueHelper.GetValue(ref identificationOfGoodsCached, () => new IdentificationOfGoodsProvider(instruction));
		CachedValue<IIdentificationOfGoods> identificationOfGoodsCached;

		public IEconomicConditions EconomicConditions
			=> CachedValueHelper.GetValue(ref economicConditionsCached, () => new EconomicConditionsProvider(instruction));
		CachedValue<IEconomicConditions> economicConditionsCached;

		public string DetailsOfPlannedActivities => instruction.DetailsOfPlannedActivities;

		public IOthers Others
			=> CachedValueHelper.GetValue(ref othersCached, () => new OthersProvider(instruction));
		CachedValue<IOthers> othersCached;

		IReadOnlyCollection<IAddressWithName> GetParties()
		{
			var parties = new List<IAddressWithName>();
			if (instruction.Owner?.MainAddress is OrgAddress address)
			{
				parties.Add(AddressWithNameProvider.New(address));
			}
			parties.AddRange(instruction.OwnerOfGoodsCollection.Select(owner => owner.Address).WhereNotNull().Select(x => AddressWithNameProvider.New(x)));
			return parties;
		}

		readonly CusEntryInstruction instruction;
	}
}
