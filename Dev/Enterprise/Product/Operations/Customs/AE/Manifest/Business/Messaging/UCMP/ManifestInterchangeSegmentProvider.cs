using CargoWise.EntityFramework;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.AE.Registry;
using Enterprise.Edifact.Generic.V4;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.AE.Manifest.Business;

sealed class ManifestInterchangeSegmentProvider : IInterchangeSegmentProvider
{
	public bool TryGetHeaderSegment(BusinessObject messageParent, out UNBSegment headerSegment)
	{
		headerSegment = default;
		if (messageParent is AsycudaBill bill)
		{
			var interchangeHeader = GetInterchangeHeader(bill);
			headerSegment = CommonMessageBuilder.GetUNBSegment(interchangeHeader);
			return true;
		}
		return false;
	}

	CUSCARInterchangeHeaderProvider GetInterchangeHeader(AsycudaBill bill)
	{
		var senderId = AECustomsRegistry.Instance.NAICServiceProviderCode.Value;
		var senderInternalId = GetOrgMPCI(ManifestMessageExtensions.AeOrgProxyForManifestMessage(bill.Factory));
		var senderInternalSubId = GetOrgMPCI(bill.Factory.Load<OrgHeader>(bill.ShipperOrgPK));

		return new CUSCARInterchangeHeaderProvider(senderId, senderInternalId, senderInternalSubId);

		string GetOrgMPCI(OrgHeader org)
		{
			return org?.CustomsCodes.GetCustomsRegNo(OrgCusCode.UnitedArabEmiratesCodeTypes.MPCINumber, CountryCodes.UnitedArabEmirates);
		}
	}
}
