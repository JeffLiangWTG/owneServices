using CargoWise.Common;
using CargoWise.Customs.MX.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.MX.Manifest.Business
{
	internal class M13Wrapper : IM13ManifestAmendmentDetails
	{
		readonly AsycudaBill bill;
		readonly string action;
		readonly string cAAT;
		readonly AsycudaManifestHeader header;
		readonly string reason;

		public M13Wrapper(AsycudaBill bill, string action, string reason)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
			header = this.bill.Header;
			this.action = action;
			cAAT = SEA309Helper.CarrierCode(header);
			this.reason = reason;
		}

		string IM13ManifestAmendmentDetails.StandardCarrierAlphaCode => cAAT;

		string IM13ManifestAmendmentDetails.LocationIdentifier => header.AMA_Nature == ShipmentTypeList.Codes.Import23 ? header.AMA_CustomsDischargePort : header.AMA_CustomsLoadPort;

		string IM13ManifestAmendmentDetails.AmendmentTypeCode => action;

		string IM13ManifestAmendmentDetails.HBLNumber => bill.ABL_BillNumber;

		string IM13ManifestAmendmentDetails.Quantity => action == SEA309Constants.AmendmentManifest ? (ZString)bill.ABL_ManifestQty.ToString() : ZString.Empty;

		string IM13ManifestAmendmentDetails.AmendmentCode => reason;

		string IM13ManifestAmendmentDetails.ActionCode => ZString.Empty;

		string IM13ManifestAmendmentDetails.WayBillNumber => ZString.Empty;

		string IM13ManifestAmendmentDetails.StandardCarrierAlphaCode2 => cAAT;

		string IM13ManifestAmendmentDetails.StandardCarrierAlphaCode3 => ZString.Empty;

		string IM13ManifestAmendmentDetails.IdentificationCode => ZString.Empty;

		string IM13ManifestAmendmentDetails.IdentificationCode2 => ZString.Empty;
	}
}
