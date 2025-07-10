using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class IE007TransitOperationProvider : IIE007TransitOperation
	{
		readonly NctsHeader nctsHeader;

		public IE007TransitOperationProvider(NctsHeader nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		}
		public DateTime ArrivalNotificationDateAndTime => DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(nctsHeader.ArrivalMovementHeader.BM_ArrivalDate.ToZDateTime(), removeMillisecond: true);

		public bool SimplifiedProcedure => !(nctsHeader.CusAuthorizationUsages?.FirstOrDefault()?.AGC_Code.IsEmpty ?? true);

		public bool IncidentFlag => nctsHeader.BH_ExportFlag.EqualsIgnoringCase(YesNoList.Codes.Yes);

		public string MRN => nctsHeader.ArrivalMrnFromUser;
	}
}
