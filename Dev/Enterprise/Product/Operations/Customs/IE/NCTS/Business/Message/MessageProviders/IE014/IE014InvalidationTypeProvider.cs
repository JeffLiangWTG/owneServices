using System;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.IE.NCTS.Business
{
	class IE014InvalidationTypeProvider : NctsDepartureHeaderMessageProvider, IIE014InvalidationType
	{
		public IE014InvalidationTypeProvider(NctsHeader nctsHeader, ZString justification) : base(nctsHeader)
		{
			this.justification = justification;
		}

		public DateTime RequestDateTime => PreparationDateAndTime;

		public bool InitiatedByCustoms
		{
			get
			{
				var customsStatus = NctsHeader.MovementHeader?.BM_CustomsStatus;
				return customsStatus.HasValue && customsStatus.Value.EqualsIgnoringCase(NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested);
			}
		}

		public string Justification => justification;

		readonly ZString justification;
	}
}
