using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.GB.ICS.Business;
using EUManifest = Enterprise.Customs.EU.Manifest.Business;

namespace Enterprise.Customs.GB.ICS.Messaging.IE315
{
	internal class HeaderWrapper : IHeader
	{
		public HeaderWrapper(AsycudaManifestHeaderBase asycudaManifestHeader, ZDateTime utcDateTime)
		{
			manifest = asycudaManifestHeader;
			this.utcDateTime = utcDateTime;
		}
		readonly AsycudaManifestHeaderBase manifest;
		readonly ZDateTime utcDateTime;

		ZBool IsAIR => manifest.AMA_TransportMode == Core.Constants.TransportModes.Air;
		ZBool IsSEA => manifest.AMA_TransportMode == Core.Constants.TransportModes.Sea;
		ZBool IsROA => manifest.AMA_TransportMode == Core.Constants.TransportModes.Road;

		readonly Dictionary<string, string> transportModePairs = new Dictionary<string, string>
		{
			{ Core.Constants.TransportModes.Air, "4" },
			{ Core.Constants.TransportModes.Sea, "1" },
			{ Core.Constants.TransportModes.Road, "3" },
		};

		ZString IHeader.ReferenceNumber => manifest.AMA_JobReference;

		ZString IHeader.TransportModeAtBorder
		{
			get
			{
				_ = transportModePairs.TryGetValue(manifest.AMA_TransportMode, out var result);
				return result;
			}
		}

		ZString IHeader.IdentityOfMeansOfTransportCrossingBorder => IsSEA ? manifest.AMA_VesselName : IsROA ? manifest.AMA_VehicleRegistration : ZString.Empty;

		ZString IHeader.IdentityOfMeansOfTransportCrossingBorderLNG => ZString.Empty; // Do not send.

		ZString IHeader.NationalityOfMeansOfTransportCrossingBorder => IsROA ? manifest.AMA_RN_NKConveyanceNationality : ZString.Empty;

		ZInt IHeader.TotalNumberOfItems => manifest.Bills.Cast<EUManifest.AsycudaBill>().Sum(x => x.Packs.Count);

		ZInt IHeader.TotalNumberOfPackages => manifest.Bills.Cast<EUManifest.AsycudaBill>().Sum(x => x.Packs.Cast<EUManifest.AsycudaPack>().Sum(y => y.APA_PackQty));

		ZDecimal IHeader.TotalGrossMass => manifest.Bills.Cast<EUManifest.AsycudaBill>().Sum(x => x.ABL_GrossWeight);

		ZBool IHeader.IsTotalGrossMassSpecified => true;

		ZString IHeader.DeclarationPlace => manifest.Branch.Address1.SubstringSafe(0, 35);

		ZString IHeader.DeclarationPlaceLNG => ZString.Empty; // Do not send.

		ZString IHeader.SpecificCircumstanceIndicator => manifest.SpecificCircumstanceIndicator;

		ZString IHeader.TransportChargesMethodOfPayment => manifest.MethodOfPayment;

		ZString IHeader.CommercialReferenceNumber => manifest.AMA_MasterBill;

		ZString IHeader.ConveyanceReferenceNumber => IsSEA || IsAIR ? manifest.AMA_Voyage : IsROA ? manifest.AMA_VehicleRegistration : ZString.Empty;

		ZString IHeader.PlaceOfLoading => manifest.AMA_RL_NKPortOfLoading;

		ZString IHeader.PlaceOfLoadingLNG => ZString.Empty; // Do not send.

		ZString IHeader.PlaceOfUnloading => manifest.AMA_RL_NKPortOfDischarge;

		ZString IHeader.PlaceOfUnloadingLNG => ZString.Empty; // Do not send.

		ZDateTime IHeader.DeclarationDateAndTime => new DateTime(utcDateTime.Year, utcDateTime.Month, utcDateTime.Day, utcDateTime.Hour, utcDateTime.Minute, 0);
	}
}
