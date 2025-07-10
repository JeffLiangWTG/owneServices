using System;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NCTSTransitOfficeProvider : INCTSTransitOffice
	{
		public static NCTSTransitOfficeProvider NewOrNull(NctsEuOfficeCode officeCode) => officeCode != null ? new NCTSTransitOfficeProvider(officeCode) : null;

		NCTSTransitOfficeProvider(NctsEuOfficeCode officeCode)
		{
			this.officeCode = Argument.NotNull(officeCode, nameof(officeCode));
		}

		public string ReferenceNumber => officeCode.CY_Data;

		public DateTime? ArrivalDateTime => officeCode.CY_Date.ToUniversalBranchTime().ToNullableDateTime();

		readonly NctsEuOfficeCode officeCode;
	}
}
