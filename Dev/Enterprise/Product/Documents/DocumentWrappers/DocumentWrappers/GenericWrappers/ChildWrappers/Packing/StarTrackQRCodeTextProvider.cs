using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Packing.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class StarTrackQRCodeTextProvider
	{
		public StarTrackQRCodeTextProvider(PackageWrapper packageWrapper, FreightWrapper parentWrapper, PkgPackage package)
		{
			this.PackageWrapper = packageWrapper;
			this.ParentWrapper = parentWrapper;
			this.Package = package;
			this.PackageJob = package?.PackageJob;
		}

		readonly PackageWrapper PackageWrapper;
		readonly FreightWrapper ParentWrapper;
		readonly PkgPackage Package;
		readonly PkgPackageJob PackageJob;

		//	1. Receiver Suburb:			30		Consignee City
		//	2. Receiver Postcode:		4		Consignee Postcode
		//	3. Consignment Number:		12		Transport Reference
		//	4. Freight Item Number:		20		Package ID
		//	5. Product Code:			3		Carrier Service Code
		//	6. Payer Account:			8		N/A
		//	7. Sender Account:			8		Account Number
		//	8. Consignment Quantity:	4		Count of Package IDs
		//	9. Consignment Weight:		5		Total Consignment weight in KGs
		//	10. Consignment Cube		5		Total Consignment volume * 1000
		//	11. Dispatch Date			8		Label print date time YYYYMMDD
		//	12. Receiver Name 1:		40		Consignee Full Name
		//	13.	Receiver Name 2:		40		Consignee Additional Address Info
		//	14. Unit Type:				3		PackageType
		//	15. Destination Depot:		4		Transport Zone Name
		//	16. Receiver Address L1:	40		Consignee Address Line 1
		//	17. Receiver Address L2:	40		Consignee Address Line 2
		//	18. Receiver Phone Number:	14		Consignee phone
		//	19. Dangerous Goods Indicator:	1	if "DG" then "Y" else "N"
		//	20. Movement Type Indicator:	1	"N"
		//	21. Not Before Date:		12		N/A
		//	22. Not After Date:			12		N/A
		//	23.	ATL Number:				10		N/A
		//	24. RA Number:				10		N/A

		#region StarTrackQRCodeText

		public ZString StarTrackQRCodeText()
		{
			var result = GetTextValueWithPadding(ZString.Empty, 334);
			if (ParentWrapper != null)
			{
				var sb = new ZStringBuilder();

				sb.Append(ReceiverSuburb);
				sb.Append(ReceiverPostCode);
				sb.Append(ConsignmentNumber);
				sb.Append(FreightItemNumber);
				sb.Append(ProductCode);
				sb.Append(PayerAccount);
				sb.Append(SenderAccount);
				sb.Append(ConsignmentQuantity);
				sb.Append(ConsignmentWeight);
				sb.Append(ConsignmentCube);
				sb.Append(DispatchDate);
				sb.Append(ReceiverName1);
				sb.Append(ReceiverName2);
				sb.Append(UnitType);
				sb.Append(DestinationDepot);
				sb.Append(ReceiverAddressLine1);
				sb.Append(ReceiverAddressLine2);
				sb.Append(ReceiverPhoneNumber);
				sb.Append(DangerousGoodsIndicator);
				sb.Append(MovementTypeIndicator);
				sb.Append(NotBeforeDate);
				sb.Append(NotAfterDate);
				sb.Append(ATLNumber);
				sb.Append(RANumber);

				result = sb.ToString();
			}

			return result;
		}

		#endregion

		#region Fields

		public ZString ReceiverSuburb => ParentWrapper != null ? GetTextValueWithPadding(ParentWrapper.Consignee.MainAddress.City, 30) : GetTextValueWithPadding(ZString.Empty, 30);
		public ZString ReceiverPostCode => ParentWrapper != null ? GetTextValueWithPadding(ParentWrapper.Consignee.MainAddress.PostCode, 4) : GetTextValueWithPadding(ZString.Empty, 4);
		public ZString ConsignmentNumber => ParentWrapper != null ? GetTextValueWithPadding(ParentWrapper.TransportReference, 12) : GetTextValueWithPadding(ZString.Empty, 12);
		public ZString FreightItemNumber => GetTextValueWithPadding(PackageWrapper.RefNumber, 20);

		public ZString ProductCode
		{
			get
			{
				if (ParentWrapper != null)
				{
					var productCode = ParentWrapper.CarrierServiceLevel.ProductCode;
					return GetTextValueWithPadding(productCode != ZString.Empty ? productCode : ParentWrapper.CarrierServiceLevel.Code, 3);
				}

				return GetTextValueWithPadding(ZString.Empty, 3);
			}
		}

		public ZString PayerAccount => GetTextValueWithPadding(ZString.Empty, 8);
		public ZString SenderAccount
		{
			get
			{
				return (ParentWrapper != null && ParentWrapper.CarrierAccount != null)
					? GetTextValueWithPadding(ParentWrapper.CarrierAccount.AccountNumber, 8)
					: GetTextValueWithPadding(ZString.Empty, 8);
			}
		}

		public ZString ConsignmentQuantity
		{
			get
			{
				return (PackageJob != null) ? GetTextValueWithPadding(PackageJob.GetAllPackageIDs().Count().ToString(CultureInfo.InvariantCulture), 4) : GetTextValueWithPadding(ZString.Empty, 4);
			}
		}

		public ZString ConsignmentWeight
		{
			get
			{
				var result = GetTextValueWithPadding(ZString.Empty, 5);

				if (Package != null)
				{
					var unit = Package.KP_WeightUQ;
					if (Core.Constants.Weight.ContainsCode(unit))
					{
						decimal packageWeight = Package.KP_Weight;
						ZDecimal weightInKilograms = (unit == Core.Constants.Weight.Kilograms)
												? packageWeight
												: Core.Constants.Weight.Convert(packageWeight, unit, Core.Constants.Weight.Kilograms);
						result = GetTextValueWithPadding(weightInKilograms.Round(0).ToString(), 5);
					}
				}

				return result;
			}
		}

		public ZString ConsignmentCube
		{
			get
			{
				var result = GetTextValueWithPadding(ZString.Empty, 5);

				if (Package != null)
				{
					var unit = Package.KP_VolumeUQ;
					if (Core.Constants.Volume.ContainsCode(unit))
					{
						decimal volume = Package.KP_Volume;
						ZDecimal volumeInLiters = (unit == Core.Constants.Volume.CubicDecimetres)
												? volume
												: Core.Constants.Volume.Convert(volume, unit, Core.Constants.Volume.CubicDecimetres);

						var volumeInLitersString = volumeInLiters.Round(0).ToString();

						result = volumeInLitersString.Length > 5 ? (ZString)VolumeOverflowString : GetTextValueWithPadding(volumeInLitersString, 5);
					}
				}

				return result;
			}
		}
		const string VolumeOverflowString = "*****";    // SuppressReason Volume Overflow constant string

		public ZString DispatchDate => ZDateTime.Now.ToString(DispatchDateFormatString, CultureInfo.InvariantCulture);
		const string DispatchDateFormatString = "yyyyMMdd";     // SuppressReason Dispatch Date format string constant

		public ZString ReceiverName1 => ParentWrapper != null ? GetTextValueWithPadding(ParentWrapper.Consignee.CompanyName, 40) : GetTextValueWithPadding(ZString.Empty, 40);
		public ZString ReceiverName2 => ParentWrapper != null ? GetTextValueWithPadding(ParentWrapper.Consignee.Organisation.MainAddress.UnrestrictedAdditionalAddressInformation, 40) : GetTextValueWithPadding(ZString.Empty, 40);
		public ZString UnitType => (Package != null) ? GetTextValueWithPadding(Package.KP_F3_NKPackType, 3) : GetTextValueWithPadding(ZString.Empty, 3);
		public ZString DestinationDepot => ParentWrapper != null ? GetTextValueWithPadding(ParentWrapper.TransportZone, 4) : GetTextValueWithPadding(ZString.Empty, 4);
		public ZString ReceiverAddressLine1 => ParentWrapper != null ? GetTextValueWithPadding(ParentWrapper.Consignee.MainAddress.AddressLine1, 40) : GetTextValueWithPadding(ZString.Empty, 40);
		public ZString ReceiverAddressLine2 => ParentWrapper != null ? GetTextValueWithPadding(ParentWrapper.Consignee.MainAddress.AddressLine2, 40) : GetTextValueWithPadding(ZString.Empty, 40);
		public ZString ReceiverPhoneNumber => ParentWrapper != null ? GetTextValueWithPadding(ParentWrapper.Consignee.MainPhone, 14) : GetTextValueWithPadding(ZString.Empty, 14);
		public ZString MovementTypeIndicator => "N";
		public ZString DangerousGoodsIndicator => (Package != null && Package.UNDGs.Any()) ? "Y" : "N";
		public ZString NotBeforeDate => GetTextValueWithPadding(ZString.Empty, 12);
		public ZString NotAfterDate => GetTextValueWithPadding(ZString.Empty, 12);
		public ZString ATLNumber => GetTextValueWithPadding(ZString.Empty, 10);
		public ZString RANumber => GetTextValueWithPadding(ZString.Empty, 10);

		#endregion

		#region GetTextValueWithPadding

		ZString GetTextValueWithPadding(ZString textValue, int length) => textValue.Length > length ? textValue.Left(length) : textValue.PadRight(length);

		#endregion
	}
}
