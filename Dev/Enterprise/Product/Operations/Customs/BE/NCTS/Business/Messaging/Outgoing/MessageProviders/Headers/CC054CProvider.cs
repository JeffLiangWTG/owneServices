using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC054CProvider : NctsDepartureHeaderProvider, ICC054C
	{
		public CC054CProvider(NctsHeader nctsHeader, bool releaseRequested) : base(nctsHeader)
		{
			ReleaseRequested = releaseRequested;
		}

		public bool ReleaseRequested { get; }

		public DateTime ReleaseRequestDateAndTime => DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(ZDateTime.UtcNow.ToDateTime(), removeMillisecond: true);

		public override string MessageType => Constants.MessageTypes.CC054C;
	}
}
