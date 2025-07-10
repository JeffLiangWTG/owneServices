using CargoWise.Common;
using CargoWise.Customs.MX.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.MX.Manifest.Business
{
	internal class M11Wrapper : IM11ManifestBillLadingDetails
	{
		readonly AsycudaBill bill;
		readonly AsycudaManifestHeader header;
		readonly string weightUQ;
		readonly string volumeUQ;

		public M11Wrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
			header = this.bill.Header;
			weightUQ = SEA309Helper.WeightUnitCodeCalculator(this.bill.ABL_GrossWeightUQ);
			volumeUQ = SEA309Helper.VolumeUnitCodeCalculator(this.bill.ABL_VolumeUQ);
		}

		string IM11ManifestBillLadingDetails.HBLNumber => bill.ABL_BillNumber;

		string IM11ManifestBillLadingDetails.LocationIdentifier => header.AMA_CustomsLoadPort;

		string IM11ManifestBillLadingDetails.Quantity => bill.ABL_ManifestQty.ToString();

		string IM11ManifestBillLadingDetails.ManifestUnitCode
		{
			get
			{
				var query = new ZQuery(RefPacksSchema.RP_CommercialPack, bill.ABL_ManifestUQ);
				query.AddToFilter(RefPacksSchema.RP_CustomsCountry, Core.Constants.CountryCodes.Mexico);
				var pack = bill.Factory.LoadTop1<CusRefPacks>(query);

				return pack?.RP_CustomsPack ?? bill.ABL_ManifestUQ;
			}
		}

		string IM11ManifestBillLadingDetails.Weight
		{
			get
			{
				return weightUQ == WeightConstants.Kilograms
					? ((ZDecimal)(Core.Constants.Weight.ConvertSafe(bill.ABL_GrossWeight, bill.ABL_GrossWeightUQ, Core.Constants.Weight.Kilograms))).ToStringTrimZeros(0)
					: bill.ABL_GrossWeight.ToStringTrimZeros(0);
			}
		}

		string IM11ManifestBillLadingDetails.WeightUnitCode => weightUQ;

		string IM11ManifestBillLadingDetails.Volume
		{
			get
			{
				return volumeUQ == VolumeConstants.CubicCentimeters
					? (ZString)Core.Constants.Volume.ConvertSafe(bill.ABL_Volume, bill.ABL_VolumeUQ, Core.Constants.Volume.CubicCentimeters).ToString()
					: bill.ABL_Volume.ToStringTrimZeros();
			}
		}

		string IM11ManifestBillLadingDetails.VolumeUnitQualifier => volumeUQ;

		string IM11ManifestBillLadingDetails.HBLCode => SEA309Constants.HBLCode;

		string IM11ManifestBillLadingDetails.PlaceOfReceiptbyPreCarrier => ZString.Empty;

		string IM11ManifestBillLadingDetails.WayBillNumber => ZString.Empty;

		string IM11ManifestBillLadingDetails.StandardCarrierAlphaCode => SEA309Helper.CarrierCode(header);

		string IM11ManifestBillLadingDetails.StandardCarrierAlphaCode2 => ZString.Empty;

		string IM11ManifestBillLadingDetails.StandardCarrierAlphaCode3 => SEA309Helper.CarrierCode(header);

		string IM11ManifestBillLadingDetails.StandardCarrierAlphaCode4 => SEA309Helper.CarrierCode(header);

		string IM11ManifestBillLadingDetails.ShipperExportDeclaration => ZString.Empty;

		string IM11ManifestBillLadingDetails.ExportExceptionCode => ZString.Empty;

		string IM11ManifestBillLadingDetails.StandardCarrierAlphaCode5 => ZString.Empty;

		string IM11ManifestBillLadingDetails.StandardCarrierAlphaCode6 => ZString.Empty;

		string IM11ManifestBillLadingDetails.LocationIdentifier2 => header.LastForeignPort;

		string IM11ManifestBillLadingDetails.LocationIdentifier3 => header.AMA_Nature == ShipmentTypeList.Codes.Import23 ? header.AMA_CustomsLoadPort : ZString.Empty;

		string IM11ManifestBillLadingDetails.TransportationMethodTypeCode => ZString.Empty;

		string IM11ManifestBillLadingDetails.PaymentMethod => ZString.Empty;

		string IM11ManifestBillLadingDetails.IndustryCode => ZString.Empty;

		string IM11ManifestBillLadingDetails.LocationIdentifier4 => ZString.Empty;

		string IM11ManifestBillLadingDetails.ServiceLevelCode => ZString.Empty;

		string IM11ManifestBillLadingDetails.Date => ZString.Empty;

		string IM11ManifestBillLadingDetails.ConditionResponseCode => ZString.Empty;
	}
}
