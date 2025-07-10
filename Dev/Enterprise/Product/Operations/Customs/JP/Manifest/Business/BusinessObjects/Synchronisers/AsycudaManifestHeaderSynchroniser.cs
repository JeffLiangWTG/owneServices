using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class AsycudaManifestHeaderSynchroniser(AsycudaManifestHeader destination, ForwardingConsol sourceConsol) : ASYCUDA.Business.AsycudaManifestHeaderSynchroniser(destination, sourceConsol)
	{
		protected new AsycudaManifestHeader Destination => (AsycudaManifestHeader)base.Destination;

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			var firstBookingNumber = Source.Numbers.Cast<CusEntryNumber>().FirstOrDefault(number => number.CE_EntryType == CusEntryNumberTypes.JP.BookingNumber);
			if (firstBookingNumber != null)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.AMA_BookingNumberInfo, firstBookingNumber.CE_EntryNumInfo));
			}

			Synchronisers.Add(new FieldSynchroniser(Destination.AMA_RL_NKPortOfFinalDepartureInfo, () => GetMoveInDestination(Source), () => GetSourceValuesAffectingMoveInDestination(Source)));
		}

		IZType GetMoveInDestination(ForwardingConsol consol)
		{
			var result = ZString.Empty;
			if (consol.ContainerYardEmptyPickupAddress is OrgAddress containerYardEmptyPickupAddress)
			{
				var ccpNumberFromAddress = containerYardEmptyPickupAddress.CustomsCodes?.GetCustomsRegNo(CodeTypes.ControlledPremisesID, CountryCodes.Japan) ?? ZString.Empty;
				result = ccpNumberFromAddress.IsEmpty ? containerYardEmptyPickupAddress.Header?.CustomsCodes?.GetCustomsRegNo(CodeTypes.ControlledPremisesID, CountryCodes.Japan) ?? ZString.Empty : ccpNumberFromAddress;
			}

			return result.Left(Destination.AMA_RL_NKPortOfFinalDepartureInfo.MaxLength);
		}

		IEnumerable<ZPropertyInfo> GetSourceValuesAffectingMoveInDestination(ForwardingConsol consol)
		{
			yield return consol.JK_OA_ContainerYardEmptyPickupAddressInfo;
		}

		protected override IZType GetCarrier() => Source.JK_OA_ShippingLineAddress;

		protected override void SynchronisersAMA_AgentType()
		{
		}

		protected override IEnumerable<ZPropertyInfo> GetSourceInfosAffectingCarrier() => new[] { Source.JK_OA_ShippingLineAddressInfo };

		protected override BusinessObjectCollectionSynchroniser GetNewBillCollectionSynchroniser()
		{
			return new AsycudaBillCollectionSynchroniser(Destination);
		}

		protected override BusinessObjectCollectionSynchroniser GetNewConsolContainerCollectionSynchroniser() => new AsycudaConsolContainerCollectionSynchroniser(Source, Destination);
	}
}
