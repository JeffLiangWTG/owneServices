using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocOuterPack : DocumentWrapper
	{
		DocOuterPack(OuterPack outerPack, BusinessObjectFactory factoryToWrap)
			: base(outerPack, factoryToWrap)
		{
		}

		public static DocOuterPack New(OuterPack outerPack, BusinessObjectFactory factoryToWrap)
		{
			if (outerPack != null)
			{
				return new DocOuterPack(outerPack, factoryToWrap);
			}
			else
			{
				return null;
			}
		}

		public override string ToString()
		{
			return Number.ToString();
		}

		protected OuterPack OuterPack
		{
			get { return (OuterPack)WrappedObject; }
		}

		public ZInt Number
		{
			get { return OuterPack.Number; }
		}

		public ZString MarksAndNumbers
		{
			get { return OuterPack.MarksAndNumbers; }
		}

		public DocShipment Shipment
		{
			get { return OuterPack.DocShipment; }
		}

		public DocPackLines PackLine
		{
			get { return OuterPack.DocPackLines; }
		}

		public ZString ItemNumber
		{
			get { return OuterPack.ItemNumber; }
		}

		protected ZString fPackType;
		public ZString PackType
		{
			get { return fPackType; }
			set { fPackType = value; }
		}

		public ZString PrimaryBarcode
		{
			get { return OuterPack.PrimaryBarcode; }
		}

		public ZString PrimaryBarcodeDisplayText
		{
			get { return OuterPack.PrimaryBarcodeDisplayText; }
		}

		public ZString SecondaryBarcode
		{
			get { return OuterPack.SecondaryBarcode; }
		}

		public ZString SecondaryBarcodeDisplayText
		{
			get { return OuterPack.SecondaryBarcodeDisplayText; }
		}

		public ZString HazardousDescription
		{
			get { return (OuterPack.DocPackLines != null) ? OuterPack.DocPackLines.HazardousDescription : (ZString)""; }
		}

		#region Optional Information

		public ZString OptionalInformation1
		{
			get { return OuterPack.OptionalInformation1; }
		}

		public ZString OptionalInformation2
		{
			get { return OuterPack.OptionalInformation2; }
		}

		public ZString OptionalInformation3
		{
			get { return OuterPack.OptionalInformation3; }
		}

		public ZString OptionalInformation4
		{
			get { return OuterPack.OptionalInformation4; }
		}

		public ZString OptionalInformation5
		{
			get { return OuterPack.OptionalInformation5; }
		}

		public ZString OptionalInformation6
		{
			get { return OuterPack.OptionalInformation6; }
		}

		public ZString OptionalDescription1
		{
			get { return OuterPack.OptionalDescription1; }
		}

		public ZString OptionalDescription2
		{
			get { return OuterPack.OptionalDescription2; }
		}

		public ZString OptionalDescription3
		{
			get { return OuterPack.OptionalDescription3; }
		}

		public ZString OptionalDescription4
		{
			get { return OuterPack.OptionalDescription4; }
		}

		public ZString OptionalDescription5
		{
			get { return OuterPack.OptionalDescription5; }
		}

		public ZString OptionalDescription6
		{
			get { return OuterPack.OptionalDescription6; }
		}

		#endregion
	}

	public class OuterPack : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ZInt Number;
		public ZInt ShipmentTotalPieceCount;
		public ZInt NumberInConsol;
		public ZInt ConsolTotalPieceCount;
		public ZString PrimaryBarcode;
		public ZString PrimaryBarcodeDisplayText;
		public ZString SecondaryBarcode;
		public ZString SecondaryBarcodeDisplayText;
		public ZString PackType;
		public DocShipment DocShipment;
		public DocPackLines DocPackLines;
		public ZString MarksAndNumbers;
		public ZString ItemNumber;
		public static new ZString TableName
		{
			get
			{
				return "OuterPack";
			}
		}

		DocAWB DocAWBWrapper { get; set; }

		#region Optional Information Properties

		public ZString OptionalInformation1 { get; set; }
		public ZString OptionalInformation2 { get; set; }
		public ZString OptionalInformation3 { get; set; }
		public ZString OptionalInformation4 { get; set; }
		public ZString OptionalInformation5 { get; set; }
		public ZString OptionalInformation6 { get; set; }

		public ZString OptionalDescription1 { get; set; }
		public ZString OptionalDescription2 { get; set; }
		public ZString OptionalDescription3 { get; set; }
		public ZString OptionalDescription4 { get; set; }
		public ZString OptionalDescription5 { get; set; }
		public ZString OptionalDescription6 { get; set; }

		#endregion

		#region Additional Properties for Optional Information and Barcode

		public virtual ZString ConsolOrigin
		{
			get { return DocAWBWrapper.AWBOriginCode; }
		}

		public virtual ZString ConsolDestination
		{
			get { return DocAWBWrapper.AirportOfDestinationCode; }
		}

		public virtual ZString ConsolPieceNumber
		{
			get { return NumberInConsol.ToString(); }
		}

		public virtual ZString ConsolPieceCount
		{
			get { return ConsolTotalPieceCount.ToString(); }
		}

		public virtual ZString ConsolPieceNumberOfCount
		{
			get { return Res.GetString("508172bb-2ee2-4c3b-a906-4957ead39d44", "{0} of {1}", ConsolPieceNumber, ConsolTotalPieceCount == 0 ? ZString.Empty : ConsolPieceCount); }
		}

		public virtual ZString ConsolWeight
		{
			get
			{
				return DocAWBWrapper.DocConsol != null ? DocAWBWrapper.DocConsol.Weight : ZString.Empty;
			}
		}

		public virtual ZString HousebillNumber
		{
			get
			{
				return DocShipment != null ? DocShipment.HouseBill : ZString.Empty;
			}
		}

		public virtual ZString ShipmentPieceNumber
		{
			get
			{
				return DocShipment != null ? DocShipment.SequenceNumber.ToString() : "";
			}
		}

		public virtual ZString ShipmentPieceCount
		{
			get
			{
				return ShipmentTotalPieceCount == 0 && DocShipment != null ? DocShipment.OuterPacks.ToString() : Convert.ToString(ShipmentTotalPieceCount);
			}
		}

		public virtual ZString ShipmentPieceNumberAndCount
		{
			get
			{
				return Res.GetString("508172bb-2ee2-4c3b-a906-4957ead39d44", "{0} of {1}", ShipmentPieceNumber, ShipmentPieceCount);
			}
		}

		public virtual ZString ShipmentWeight
		{
			get
			{
				return DocShipment != null ? DocShipment.Weight : ZString.Empty;
			}
		}

		public virtual ZString ShipmentHandlingInformation
		{
			get
			{
				return DocShipment != null ? DocShipment.HandlingInstructions : ZString.Empty;
			}
		}

		public virtual ZString IssuingCompanyName
		{
			get
			{
				if (GlbBranch.CurrentBranch != null && !GlbBranch.CurrentBranch.IsNull && !GlbBranch.CurrentBranch.GB_OH_OrgProxy.IsEmpty)
				{
					return GlbBranch.CurrentBranch.OrgProxy.OH_FullName;
				}
				else
				{
					return GlbCompany.CurrentCompany.OrgProxy.OH_FullName;
				}
			}
		}

		#endregion

		public void SetupOptionalInformation(DocAWB docAWB)
		{
			DocAWBWrapper = docAWB;
			AWBLabelCustomisation awbLabelCutomisation = FreightDataRegistry.Instance.AWBBarcodeLabelCustomisation.Value;

			ZString optionalInformation = new(), optionalDescription = new();

			ProcessOptionalInformation(awbLabelCutomisation.OptionalInformation1, awbLabelCutomisation.OptionalDescription1, ref optionalInformation, ref optionalDescription);
			OptionalInformation1 = optionalInformation;
			OptionalDescription1 = optionalDescription;

			ProcessOptionalInformation(awbLabelCutomisation.OptionalInformation2, awbLabelCutomisation.OptionalDescription2, ref optionalInformation, ref optionalDescription);
			OptionalInformation2 = optionalInformation;
			OptionalDescription2 = optionalDescription;

			ProcessOptionalInformation(awbLabelCutomisation.OptionalInformation3, awbLabelCutomisation.OptionalDescription3, ref optionalInformation, ref optionalDescription);
			OptionalInformation3 = optionalInformation;
			OptionalDescription3 = optionalDescription;

			ProcessOptionalInformation(awbLabelCutomisation.OptionalInformation4, awbLabelCutomisation.OptionalDescription4, ref optionalInformation, ref optionalDescription);
			OptionalInformation4 = optionalInformation;
			OptionalDescription4 = optionalDescription;

			ProcessOptionalInformation(awbLabelCutomisation.OptionalInformation5, awbLabelCutomisation.OptionalDescription5, ref optionalInformation, ref optionalDescription);
			OptionalInformation5 = optionalInformation;
			OptionalDescription5 = optionalDescription;

			ProcessOptionalInformation(awbLabelCutomisation.OptionalInformation6, awbLabelCutomisation.OptionalDescription6, ref optionalInformation, ref optionalDescription);
			OptionalInformation6 = optionalInformation;
			OptionalDescription6 = optionalDescription;

			SetupSecondaryBarCode();
		}

		void ProcessOptionalInformation(ZString optionalInformationField, ZString optionalDescriptionField, ref ZString optionalInfo, ref ZString optionalDescription)
		{
			optionalInfo = "";
			optionalDescription = "";

			if (optionalInformationField == AWBLabelOptionalInformationList.Codes.ConsolOrigin)
			{
				optionalInfo = ConsolOrigin;
				optionalDescription = optionalDescriptionField;
			}

			else if (optionalInformationField == AWBLabelOptionalInformationList.Codes.ConsolDestination)
			{
				optionalInfo = ConsolDestination;
				optionalDescription = optionalDescriptionField;
			}

			else if (optionalInformationField == AWBLabelOptionalInformationList.Codes.ConsolPieceNumber)
			{
				optionalInfo = ConsolPieceNumber;
				optionalDescription = optionalDescriptionField;
			}

			else if (optionalInformationField == AWBLabelOptionalInformationList.Codes.ConsolPieceCount)
			{
				optionalInfo = ConsolPieceCount;
				optionalDescription = optionalDescriptionField;
			}

			else if (optionalInformationField == AWBLabelOptionalInformationList.Codes.ConsolPieceNumberOfCount)
			{
				optionalInfo = ConsolPieceNumberOfCount;
				optionalDescription = optionalDescriptionField;
			}

			else if (optionalInformationField == AWBLabelOptionalInformationList.Codes.ConsolWeight)
			{
				optionalInfo = ConsolWeight;
				optionalDescription = optionalDescriptionField;
			}

			else if (optionalInformationField == AWBLabelOptionalInformationList.Codes.HousebillNumber)
			{
				optionalInfo = HousebillNumber;
				optionalDescription = optionalDescriptionField;
			}

			else if (optionalInformationField == AWBLabelOptionalInformationList.Codes.ShipmentPieceNumber)
			{
				optionalInfo = ShipmentPieceNumber;
				optionalDescription = optionalDescriptionField;
			}

			else if (optionalInformationField == AWBLabelOptionalInformationList.Codes.ShipmentPieceCount)
			{
				optionalInfo = ShipmentPieceCount;
				optionalDescription = optionalDescriptionField;
			}

			else if (optionalInformationField == AWBLabelOptionalInformationList.Codes.ShipmentPieceNumberOfCount)
			{
				optionalInfo = ShipmentPieceNumberAndCount;
				optionalDescription = optionalDescriptionField;
			}

			else if (optionalInformationField == AWBLabelOptionalInformationList.Codes.ShipmentWeight)
			{
				optionalInfo = ShipmentWeight;
				optionalDescription = optionalDescriptionField;
			}

			else if (optionalInformationField == AWBLabelOptionalInformationList.Codes.ShipmentHandlingInformation)
			{
				optionalInfo = ShipmentHandlingInformation;
				optionalDescription = optionalDescriptionField;
			}

			else if (optionalInformationField == AWBLabelOptionalInformationList.Codes.IssuingCompanyName)
			{
				optionalInfo = IssuingCompanyName;
				optionalDescription = optionalDescriptionField;
			}
		}

		void SetupSecondaryBarCode()
		{
			ZString barcodeTextToEncode = "";
			ZString appendix = "";

			AWBLabelCustomisation awbLabelCutomisation = FreightDataRegistry.Instance.AWBBarcodeLabelCustomisation.Value;
			if (awbLabelCutomisation.BarcodeHousebillNumber)
			{
				appendix = "H" + HousebillNumber;
				barcodeTextToEncode = AppendTextToEncode(barcodeTextToEncode, appendix);
			}

			if (awbLabelCutomisation.BarcodeShipmentPieceCount)
			{
				appendix = "S" + ShipmentPieceCount.PadLeft(4, '0');
				barcodeTextToEncode = AppendTextToEncode(barcodeTextToEncode, appendix);
			}

			if (awbLabelCutomisation.BarcodeShipmentPieceNumber)
			{
				appendix = "Y" + ShipmentPieceNumber.PadLeft(4, '0');
				barcodeTextToEncode = AppendTextToEncode(barcodeTextToEncode, appendix);
			}

			if (awbLabelCutomisation.BarcodeShipmentWeight)
			{
				appendix = "A" + ShipmentWeight.PadLeft(7, '0');
				if (DocShipment != null)
				{
					if (DocShipment.WeightUnit == Core.Constants.Weight.Kilograms)
					{
						appendix += "K";
					}
					if (DocShipment.WeightUnit == Core.Constants.Weight.Pounds)
					{
						appendix += "L";
					}
				}
				barcodeTextToEncode = AppendTextToEncode(barcodeTextToEncode, appendix);
			}

			if (awbLabelCutomisation.BarcodeConsolOrigin)
			{
				appendix = "O" + ConsolOrigin.PadLeft(3);
				barcodeTextToEncode = AppendTextToEncode(barcodeTextToEncode, appendix);
			}

			if (awbLabelCutomisation.BarcodeConsolDestination)
			{
				appendix = "D" + ConsolDestination.PadLeft(3);
				barcodeTextToEncode = AppendTextToEncode(barcodeTextToEncode, appendix);
			}

			if (awbLabelCutomisation.BarcodeConsolPieceCount)
			{
				appendix = "P" + ConsolPieceCount.PadLeft(4, '0');
				barcodeTextToEncode = AppendTextToEncode(barcodeTextToEncode, appendix);
			}

			if (awbLabelCutomisation.BarcodeConsolWeight)
			{
				appendix = "T" + ConsolWeight.PadLeft(7, '0');
				if (DocAWBWrapper.DocConsol != null)
				{
					if (DocAWBWrapper.DocConsol.WeightUnit == Core.Constants.Weight.Kilograms)
					{
						appendix += "K";
					}
					if (DocAWBWrapper.DocConsol.WeightUnit == Core.Constants.Weight.Pounds)
					{
						appendix += "L";
					}
				}
				barcodeTextToEncode = AppendTextToEncode(barcodeTextToEncode, appendix);
			}

			if (awbLabelCutomisation.BarcodeShipmentHandlingInformation)
			{
				appendix = "B" + ShipmentHandlingInformation.Substring(0, Math.Min(38, ShipmentHandlingInformation.Length));
				barcodeTextToEncode = AppendTextToEncode(barcodeTextToEncode, appendix);
			}

			EncodeSecondaryBarcode(barcodeTextToEncode);
		}

		#region Barcode Encoding

		public void EncodePrimaryBarcode(ZString textToEncode)
		{
			TextBarcode barcode = new TextBarcode(textToEncode);
			PrimaryBarcode = barcode.TextAs128sFontString;
			PrimaryBarcodeDisplayText = textToEncode;
		}

		public void EncodeSecondaryBarcode(ZString textToEncode)
		{
			TextBarcode barcode = new TextBarcode(textToEncode);
			SecondaryBarcode = barcode.TextAs128sFontString;
			SecondaryBarcodeDisplayText = textToEncode;
		}

		const string Delimiter = "+";

		public ZString AppendTextToEncode(ZString textToEncode, ZString appendix)
		{
			if (textToEncode.Length > 0 && appendix.Length > 0)
			{
				textToEncode += Delimiter;
			}
			textToEncode += appendix;

			return textToEncode;
		}

		#endregion
	}
}
