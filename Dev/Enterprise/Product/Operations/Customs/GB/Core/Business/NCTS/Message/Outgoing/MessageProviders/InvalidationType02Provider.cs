using System;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class InvalidationType02Provider : IInvalidationType02
	{
		readonly NctsHeader header;
		protected readonly NctsDepartureMovementHeader depHeader;

		public InvalidationType02Provider(NctsHeader nctsHeader, ZString justification)
		{
			header = Argument.NotNull(nctsHeader, nameof(nctsHeader));
			depHeader = Argument.NotNull(header.MovementHeader, nameof(depHeader));
			this.justification = justification;
		}

		public DateTime? RequestDateAndTime => DataProviderHelper.GetProviderDateTime(ZDateTime.Now);

		public DateTime? DecisionDateAndTime => null;

		public int? Decision => null;

		public bool InitiatedByCustoms => depHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.CancellationRequestedByCustoms;

		public string Justification => justification;

		readonly ZString justification;
	}
}
