using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class GUACODHeaderProvider : IGUACODHeader
	{
		public GUACODHeaderProvider(SendAccessCodeViewModel sendAccessCode)
		{
			this.sendAccessCode = sendAccessCode;
		}

		readonly SendAccessCodeViewModel sendAccessCode;

		public string HolderOfTransitProcedure => sendAccessCode.GuaranteeHeader.PermitHolder.GetEoriDetails();

		public string CustomsOfficeOfGuarantee => sendAccessCode.OfficeOfGuarantee.ValueOrNullIfEmpty();

		public DateTime? EffectiveDate => null;

		public string GRN => sendAccessCode.GuaranteeHeader.CPH_Number.ValueOrNullIfEmpty();

		public string AccessCodeCurrent => sendAccessCode.GuaranteeHeader.MainAccessCode.ValueOrNullIfEmpty();

		public string AccessCodeNew => sendAccessCode.NewMainAccessCode.ValueOrNullIfEmpty();

		public IReadOnlyCollection<string> AccessCodes => accessCodes ?? (accessCodes = sendAccessCode.GuaranteeHeader.AdditionalAccessCodes.Select(c => c.CPR_ValueFrom.ValueOrNullIfEmpty()).ToArray());
		IReadOnlyCollection<string> accessCodes;
	}
}
