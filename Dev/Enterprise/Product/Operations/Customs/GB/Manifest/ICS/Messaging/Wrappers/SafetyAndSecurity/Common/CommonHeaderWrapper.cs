using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Manifest.Business;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using AsycudaBill = Enterprise.Customs.EU.Manifest.Business.AsycudaBill;

namespace Enterprise.Customs.GB.SafetyAndSecurity.Messaging
{
	abstract class CommonHeaderWrapper
	{
		public CommonHeaderWrapper(AsycudaManifestHeaderBase asycudaManifestHeader, ZDateTime utcDateTime)
		{
			this.manifest = Argument.NotNull(asycudaManifestHeader, nameof(asycudaManifestHeader));
			this.utcDateTime = Argument.NotNull(utcDateTime, nameof(utcDateTime));
		}
		readonly AsycudaManifestHeaderBase manifest;
		readonly ZDateTime utcDateTime;

		protected AsycudaManifestHeaderBase Manifest => manifest;

		protected ZDateTime UtcDateTime => utcDateTime;

		bool IsSeaTypeTransportMode => manifest.AMA_TransportMode == GBSSTransportTypeList.Codes.SeaFreight || manifest.AMA_TransportMode == GBSSTransportTypeList.Codes.InlandWaterTransport;

		bool IsRoaTransportMode => manifest.AMA_TransportMode == GBSSTransportTypeList.Codes.RoadFreight;

		bool IsRorTransportMode => manifest.AMA_TransportMode == GBSSTransportTypeList.Codes.RoroAccompanied;

		bool IsRouTransportMode => manifest.AMA_TransportMode == GBSSTransportTypeList.Codes.RoroUnaccompanied;

		bool IsRoadTypeTransportMode => IsRoaTransportMode || IsRorTransportMode || IsRouTransportMode;

		readonly Dictionary<string, string> transportModePairs = new Dictionary<string, string>
		{
			{ GBSSTransportTypeList.Codes.SeaFreight, "1" },
			{ GBSSTransportTypeList.Codes.RailFreight, "2" },
			{ GBSSTransportTypeList.Codes.RoadFreight, "3" },
			{ GBSSTransportTypeList.Codes.AirFreight, "4" },
			{ GBSSTransportTypeList.Codes.InlandWaterTransport, "8" },
			{ GBSSTransportTypeList.Codes.RoroAccompanied, "10" },
			{ GBSSTransportTypeList.Codes.RoroUnaccompanied, "11" },
		};

		public string IdentityOfMeansOfTransportCrossingBorder =>
			IsRoadTypeTransportMode ? manifest.AMA_VehicleRegistration :
			IsSeaTypeTransportMode ? manifest.AMA_LloydsNumber : string.Empty;

		public string IdentityOfMeansOfTransportCrossingBorderLNG => string.Empty; // Do not send.

		public string NationalityOfMeansOfTransportCrossingBorder => IsRoadTypeTransportMode ? manifest.AMA_RN_NKConveyanceNationality : string.Empty;

		public string TransportModeAtBorder => transportModePairs.TryGetValue(manifest.AMA_TransportMode, out var result) ? result : string.Empty;

		public string TotalNumberOfItems => manifest.Bills.Cast<AsycudaBill>().Count().ToString();

		public string TotalNumberOfPackages => manifest.Bills.Cast<AsycudaBill>().Sum(x => x.Packs.Cast<AsycudaPack>().Select(x => new PackageWrapper(x)).Sum(y => y.NumberOfPackagesInt + y.NumberOfPiecesInt + (y.IsBulk ? 1 : 0))).ToString();

		public decimal TotalGrossMass => manifest.Bills.Cast<AsycudaBill>().Sum(x => x.ABL_GrossWeight);

		public string SpecificCircumstanceIndicator => manifest.SpecificCircumstanceIndicator;

		public string TransportChargesMethodOfPayment => manifest.MethodOfPayment;

		public string CommercialReferenceNumber => manifest.AMA_JobReference;

		public string ConveyanceReferenceNumber => manifest.AMA_MasterBill;

		public string PlaceOfLoading => string.Empty;

		public string PlaceOfLoadingLNG => string.Empty; // Do not send.

		public string PlaceOfUnloading => string.Empty;

		public string PlaceOfUnloadingLNG => string.Empty; // Do not send.
	}
}
