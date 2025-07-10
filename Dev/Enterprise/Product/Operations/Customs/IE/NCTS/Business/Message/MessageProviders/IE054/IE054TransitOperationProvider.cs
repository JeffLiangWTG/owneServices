using System;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.IE.NCTS.Business
{
	class IE054TransitOperationProvider : NctsDepartureHeaderMessageProvider, IIE054TransitOperation
	{
		public IE054TransitOperationProvider(NctsHeader nctsHeader, ZString releaseRequest) : base(nctsHeader)
		{
			this.releaseRequest = releaseRequest;
		}

		public DateTime ReleaseRequestDateAndTime => DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(ZDateTime.Now.ToDateTime(), removeMillisecond: true);

		public bool IsReleaseRequested => releaseRequest.EqualsIgnoringCase(ReleaseRequestedFlagList.Codes.Yes);
		readonly ZString releaseRequest;

		public string MRN => NctsHeader.MovementReferenceNumber;
	}
}
